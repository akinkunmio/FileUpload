using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;

namespace FileUploadAndValidation
{
    public class FIRS_WHTUploadService : UploadServiceBase
    {
        private readonly UploadOptions _uploadOptions;
        public FIRS_WHTUploadService()
        {
            _uploadOptions = new UploadOptions { ValidateHeaders = true };
        }

        private IEnumerable<ColumnContract> GetColumns()
        {
            return new[]
            {
                new ColumnContract{ ColumnName="TIN", DataType="string", Max=10, Required=true },
                new ColumnContract{ ColumnName="FullName", DataType="string", Max=100, Required=true },
            };
        }

        protected override UploadOptions GetUploadOptions() => _uploadOptions;

        protected override void ValidateHeader(Row headerRow)
        {
            Console.WriteLine("Validating headers...");
            if (!_uploadOptions.ValidateHeaders)
                return;

            if (headerRow == null)
                throw new ArgumentException("Header row not found");

            var expectedNumOfColumns = GetColumns().Count();
            if (headerRow.Columns.Count() != expectedNumOfColumns)
                throw new ArgumentException($"Invalid number of columns. Expected: {expectedNumOfColumns}, Found: {headerRow.Columns.Count()}");
        }

        protected override void ValidateContent(IEnumerable<Row> contentRows)
        {
            Console.WriteLine("Validating rows...");
            contentRows.AsParallel().ForAll(row => Console.WriteLine("Validating row..." + row.Index));
        }

        protected override Task UploadToRemote(Row headerRow, IEnumerable<Row> contentRows)
        {

            //upload to FIRS endpoint or SFTP
            return Task.CompletedTask;
        }
    }

    public enum ValidationTypes
    {
        NUBAN, LUHN, BVN
    }
}
