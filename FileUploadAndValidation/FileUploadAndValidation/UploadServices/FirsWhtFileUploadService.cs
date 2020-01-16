using FileUploadAndValidation;
using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FileUploadAndValidation.Models.UploadResult;

namespace FileUploadApi.Services
{
    public class FirsWhtFileUploadService : IFileUploadService
    {
        private readonly IFileReader _csvFileReader;
        private readonly IFileReader _xlsxFileReader;
        private readonly IFileReader _xlsFileReader;

        public FirsWhtFileUploadService(Func<FileReaderEnum, IFileReader> fileReader)
        {
            _csvFileReader = fileReader(FileReaderEnum.csv);
            _xlsxFileReader = fileReader(FileReaderEnum.xlsx);
            _xlsFileReader = fileReader(FileReaderEnum.xls);
        }

        private ColumnContract[] GetColumns()
        {
            return new[]
            {
                new ColumnContract{ ColumnName="ContractorName", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="ContractorAddress", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="ContractorTIN", DataType="string", Max=15, Required=true},
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Max=100, Required=true},
                new ColumnContract{ ColumnName="TransactionNature", DataType="string", Max=100, Required=true},
                new ColumnContract{ ColumnName="TransactionDate", DataType="datetime", Max=12, Required=true},
                new ColumnContract{ ColumnName="TransactionInvoiceRefNo", DataType="string", Max=25, Required=true},
                new ColumnContract{ ColumnName="CurrencyOfTransaction", DataType="string", Max=6, Required=true},
                new ColumnContract{ ColumnName="InvoicedValue", DataType="decimal", Max=15, Required=true},
                new ColumnContract{ ColumnName="ExchangeRateToNaira", DataType="decimal", Max=4, Required=true},
                new ColumnContract{ ColumnName="InvoiceValueofTransaction", DataType="decimal", Max=15, Required=true},
                new ColumnContract{ ColumnName="WVATRate", DataType="decimal", Max=4, Required=true},
                new ColumnContract{ ColumnName="WVATValue", DataType="decimal", Max=12, Required=true},
                new ColumnContract{ ColumnName="TaxAccountNumber", DataType="decimal", Max=20, Required=true}
            };

        }

        public UploadOptions GetUploadOptions(bool validateHeaders = true)
        {
            return new UploadOptions { ValidateHeaders = validateHeaders };
        }

        public void ValidateHeader(Row headerRow)
        {
            if (headerRow == null)
                throw new ArgumentException("Header row not found");

            var expectedNumOfColumns = GetColumns().Count();
            if (headerRow.Columns.Count() != expectedNumOfColumns)
                throw new ArgumentException($"Invalid number of columns. Expected: {expectedNumOfColumns}, Found: {headerRow.Columns.Count()}");
        }

        public UploadResult ValidateContent(IEnumerable<Row> contentRows)
        {
            Console.WriteLine("Validating rows...");
            var uploadResult = new UploadResult();
            contentRows.AsParallel().ForAll(row =>
            {
                var isValidRow = ValidateRow(row, uploadResult);
                if (isValidRow)
                {
                    uploadResult.ValidRows.Add(row.Index);
                    uploadResult.RowsCount++;
                }
            });

            return uploadResult;
        }

        private bool ValidateRow(Row row, UploadResult uploadResult)
        {
            var validationFailure = new Failure
            {
                ColumnValidationErrors = new List<ValidationError>(),
                RowNumber = row.Index
            };
            var errorMessage = "";
            var isValid = true;

            for(var i = 0; i < GetColumns().Length; i++)
            {
                ColumnContract contract = GetColumns()[i];
                Column column = row.Columns[i];

                try
                {
                    if (contract.Required && column.Value == null)
                    {
                        errorMessage = "Value must be provided";
                    }
                    if (contract.Max != default && column.Value.Length > contract.Max)
                    {
                        errorMessage = "Specified maximum lenght exceeded";
                    }
                    if (contract.Min != default && column.Value.Length < contract.Min)
                    {
                        errorMessage = "Specified minimum length not met";
                    }
                    if (contract.DataType != null)
                    {
                        var dataTypes = new Dictionary<string, Type>();
                        dataTypes.Add("string", typeof(string));
                        dataTypes.Add("integer", typeof(int));
                        dataTypes.Add("decimal", typeof(decimal));
                        dataTypes.Add("boolean", typeof(bool));
                        dataTypes.Add("datetime", typeof(DateTime));
                        dataTypes.Add("character", typeof(char));
                        dataTypes.Add("double", typeof(double));

                        if (!dataTypes.ContainsKey(column.Value))
                            errorMessage = "Specified data type is not supported";
                        try
                        {
                            Convert.ChangeType(column.Value, dataTypes[column.Value]);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Invalid value for data type specified";
                        }
                    }
                    throw new ValidationException(
                        new ValidationError
                        {
                            ErrorMessage = errorMessage,
                            PropertyName = contract.ColumnName
                        },
                        errorMessage);
                }
                catch (ValidationException exception)
                {
                    isValid = false;
                    validationFailure.ColumnValidationErrors.Add(exception.ValidationError);
                }
            }

            uploadResult.Failures.Add(validationFailure);

            return isValid;
        }

        public async Task<UploadResult> Upload(IEnumerable<Row> rows, bool validateHeaders = true)
        {
            UploadResult uploadResult;
            var headerRow = new Row();
            var options = GetUploadOptions(validateHeaders);

            if (!rows.Any())
                throw new ArgumentException("Empty rows");

            if (options.ValidateHeaders)
            {
                headerRow = rows.First();
                ValidateHeader(headerRow);
            }

            var contentRows = options.ValidateHeaders ? rows.Skip(1) : rows;

            uploadResult = ValidateContent(contentRows);
            uploadResult.RowsCount = rows.Count();


            return await UploadToRemote(headerRow, contentRows, uploadResult);
        }

        private Task<UploadResult> UploadToRemote(Row headerRow, IEnumerable<Row> contentRows, UploadResult uploadResult)
        {
            return Task.FromResult(uploadResult);
        }
    }
  
    public enum ValidationTypes
    {
        NUBAN, LUHN, BVN
    }
}

