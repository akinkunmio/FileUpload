using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilleUploadCore.UploadManagers
{
    //public abstract class UploadServiceBase
    //{
    //    protected abstract UploadOptions GetUploadOptions();
    //    protected abstract void ValidateHeader(Row headerRow);
    //    protected abstract void ValidateContent(IEnumerable<Row> contentRows);
    //    protected abstract Task UploadToRemote(Row headerRow, IEnumerable<Row> contentRows);

    //    public Task Upload(IEnumerable<Row> rows)
    //    {
    //        var options = GetUploadOptions() ?? new UploadOptions();

    //        if (!rows.Any())
    //            throw new ArgumentException("Empty rows");

    //        Row headerRow = new Row();

    //        if (options.ValidateHeaders)
    //        {
    //            headerRow = rows.First();
    //            ValidateHeader(headerRow);
    //        }

    //        var contentRows = options.ValidateHeaders ? rows.Skip(1) : rows;

    //        ValidateContent(contentRows);

    //        return UploadToRemote(headerRow, contentRows);
    //    }
    //}

    public class UploadOptions
    {
        public string AuthToken { get; set; }

        public string ContentType { get; set; }     

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string ValidationType { get; set; }

        public string FileExtension { get; set; }

        public string  RawFileLocation { get; set; }

        public long? UserId { get; set; }

        public string ProductCode { get; set; }

        public string BatchId { get; set; }
    }

    public class InitiatePaymentOptions
    {
        public string AuthToken { get; set; }

        public long? UserId { get; set; }

        public long? ApprovalConfigId { get; set; }

        public string UserName { get; set; }

        public long? BusinessId { get; set; }
    }
}
