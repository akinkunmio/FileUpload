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
    public class FirsWhtFileUploadService : IFileService
    {
        public FirsWhtFileUploadService()
        { }

        private static ColumnContract[] GetColumns()
        {
            return new[]
            {
                new ColumnContract{ ColumnName="ContractorName", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="ContractorAddress", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="ContractorTIN", DataType="string", Max=15, Required=true},
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Max=100, Required=true},
                new ColumnContract{ ColumnName="TransactionNature", DataType="string", Max=100, Required=true},
                new ColumnContract{ ColumnName="TransactionDate", DataType="datetime", Required=true},
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

        private void ValidateHeader(Row headerRow)
        {
            if (headerRow == null)
                throw new ArgumentException("Header row not found");

            var expectedNumOfColumns = GetColumns().Count();
            if (headerRow.Columns.Count() != expectedNumOfColumns)
                throw new ArgumentException($"Invalid number of columns. Expected: {expectedNumOfColumns}, Found: {headerRow.Columns.Count()}");
            
            for(int i = 0; i < expectedNumOfColumns; i++)
            {
                var columnName = GetColumns()[i].ColumnName;
                var headerRowColumn = headerRow.Columns[i].Value.ToString().Trim();
                if (!headerRowColumn.ToLower().Contains(columnName.ToLower()))
                    throw new ArgumentException($"Invalid header name. Expected: {columnName}, Found: {headerRowColumn}");
            }
        }

        private UploadResult ValidateContent(IEnumerable<Row> contentRows, UploadResult uploadResult)
        {
            Console.WriteLine("Validating rows...");

            contentRows.AsParallel().ForAll(row =>
            {
                var isValidRow = ValidateRow(row, uploadResult);
                if (isValidRow)
                    uploadResult.ValidRows.Add(row.Index);
            });

            return uploadResult;
        }

        private bool ValidateRow(Row row, UploadResult uploadResult)
        {
            var validationErrors = new List<ValidationError>();

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
                    if (contract.Max != default && column.Value != null && contract.Max < column.Value.Length )
                    {
                        errorMessage = "Specified maximum lenght exceeded";
                    }
                    if (contract.Min != default && column.Value != null && column.Value.Length < contract.Min)
                    {
                        errorMessage = "Specified minimum length not met";
                    }
                    if (contract.DataType != default && column.Value != null)
                    {
                        if (!DataTypes().ContainsKey(contract.DataType))
                            errorMessage = "Specified data type is not supported";
                        try
                        {
                            dynamic typedValue = Convert.ChangeType(column.Value, DataTypes()[contract.DataType]);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Invalid value for data type specified";
                        }
                    }
                    if(!string.IsNullOrWhiteSpace(errorMessage))
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
                    validationErrors.Add(exception.ValidationError);
                }
            }

           if(validationErrors.Count() > 0)
            {
                uploadResult.Failures.Add(
                    new Failure { 
                        ColumnValidationErrors = validationErrors, 
                        RowNumber = row.Index 
                    });
            }

            return isValid;
        }

        private static Dictionary<string, Type> DataTypes()
        {
            return new Dictionary<string, Type>() {
                { "string", typeof(string) },
                { "integer", typeof(int) },
                { "decimal", typeof(decimal) },
                { "boolean", typeof(bool) },
                { "datetime", typeof(DateTime) },
                { "character", typeof(char) },
                { "double", typeof(double) }
            };
        }

        public async Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows)
        {
            var uploadResult = new UploadResult();
            var headerRow = new Row();
            uploadResult.RowsCount = rows.Count();
            try
            {
                if (!rows.Any())
                    throw new ArgumentException("Empty rows");

                if (uploadOptions.ValidateHeaders)
                {
                    headerRow = rows.First();
                    ValidateHeader(headerRow);
                }

                var contentRows = uploadOptions.ValidateHeaders ? rows.Skip(1) : rows;

                uploadResult = ValidateContent(contentRows, uploadResult);
                uploadResult.ScheduleId = GenerateUniqueId();
                return await UploadToRemote(headerRow, contentRows, uploadResult);
            }
            catch (Exception exception)
            {
                uploadResult.ErrorMessage = exception.Message;
                return uploadResult;
            }
        }

        private Guid GenerateUniqueId()
        {
            return Guid.NewGuid();
        }

        private Task<UploadResult> UploadToRemote(Row headerRow, IEnumerable<Row> contentRows, UploadResult uploadResult)
        {
            return Task.FromResult(uploadResult);
        }

        public Task SaveToDBForReporting(Guid scheduleId, byte[] contents)
        {
            throw new NotImplementedException();
        }

        public Task SendToEventQueue(Guid scheduleId, byte[] contents)
        {
            throw new NotImplementedException();
        }

        public Task UploadToNas(Guid scheduleId, byte[] contents, string contentType)
        {
            throw new NotImplementedException();
        }
    }
  
    public enum ValidationTypes
    {
        NUBAN, LUHN, BVN
    }
}

