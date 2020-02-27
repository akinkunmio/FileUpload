using FileUploadAndValidation;
using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.Helpers;
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
    public class FirsWhtFileService : IFileService
    {
        public FirsWhtFileService()
        { 

        }

        public async Task<UploadResult> ValidateContent(IEnumerable<Row> contentRows, UploadResult uploadResult)
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

            for(var i = 0; i < ContentTypeColumnContract.FirsWht().Length; i++)
            {
                ColumnContract contract = ContentTypeColumnContract.FirsWht()[i];
                Column column = row.Columns[i];

                try
                {
                    if (contract.Required == true && column.Value == null)
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
                        if (!GenericHelpers.ColumnDataTypes().ContainsKey(contract.DataType))
                            errorMessage = "Specified data type is not supported";
                        try
                        {
                            dynamic typedValue = Convert.ChangeType(column.Value, GenericHelpers.ColumnDataTypes()[contract.DataType]);
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

        public async Task<UploadResult> Upload(UploadOptions uploadOptions, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            //var uploadResult = new UploadResult();
            var headerRow = new Row();
            uploadResult.RowsCount = rows.Count();
            try
            {
                if (!rows.Any())
                    throw new ArgumentException("Empty rows");

                if (uploadOptions.ValidateHeaders)
                {
                    headerRow = rows.First();
                    GenericHelpers.ValidateHeaderRow(headerRow, ContentTypeColumnContract.FirsWht());
                }

                var contentRows = uploadOptions.ValidateHeaders ? rows.Skip(1) : rows;

                uploadResult = await ValidateContent(contentRows, uploadResult);
                uploadResult.BatchId = GenerateUniqueId();
                return await UploadToRemote(headerRow, contentRows, uploadResult);
            }
            catch (Exception exception)
            {
                uploadResult.ErrorMessage = exception.Message;
                return uploadResult;
            }
        }

        private string GenerateUniqueId()
        {
            return Guid.NewGuid().ToString() + "|" + DateTime.Now.ToString();
        }

        private Task<UploadResult> UploadToRemote(Row headerRow, IEnumerable<Row> contentRows, UploadResult uploadResult)
        {
            return Task.FromResult(uploadResult);
        }

        public Task SaveRowsToDB(Guid scheduleId, byte[] contents)
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

        public Task SaveRowsToDB(string scheduleId, IEnumerable<Row> contents)
        {
            throw new NotImplementedException();
        }

        public Task UploadToNas(string scheduleId, IEnumerable<Row> contents, string contentType)
        {
            throw new NotImplementedException();
        }

        public Task<ValidateRowsResult> ValidateContent(IEnumerable<Row> contentRows)
        {
            throw new NotImplementedException();
        }
    }
  
    public enum ValidationTypes
    {
        NUBAN, LUHN, BVN
    }
}

