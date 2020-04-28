using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileUploadAndValidation.Helpers
{
    public static class GenericHelpers
    {
        public static void ValidateHeaderRow(Row headerRow, ColumnContract[] columnContracts)
        {
            if (headerRow == null)
                throw new ValidationException("Header row not found");

            var expectedNumOfColumns = columnContracts.Count();
            if (headerRow.Columns.Count() != expectedNumOfColumns)
                throw new ValidationException($"Invalid number of header columns. Expected: {expectedNumOfColumns}, Found: {headerRow.Columns.Count()}");

            for (int i = 0; i < expectedNumOfColumns; i++)
            {
                var columnName = columnContracts[i].ColumnName;
                var headerRowColumn = headerRow.Columns[i].Value.ToString().Trim();
                if (!headerRowColumn.ToLower().Contains(columnName.ToLower()))
                    throw new ValidationException($"Invalid header column name. Expected: {columnName}, Found: {headerRowColumn}");
            }
        }

        public static List<ValidationError> ValidateRowCell(Row row, ColumnContract[] columnContracts, bool isValid)
        {
            var validationErrors = new List<ValidationError>();

            for (var i = 0; i < columnContracts.Length; i++)
            {

                var errorMessage = "";
                ColumnContract contract = columnContracts[i];
                Column column = row.Columns[i];

                try
                {
                    if (contract.Required == true && string.IsNullOrWhiteSpace(column.Value))
                    {
                        errorMessage = "Value must be provided";
                    }
                    if (contract.Max != default && column.Value != null && contract.Max < column.Value.Length)
                    {
                        errorMessage = "Specified maximum length exceeded";
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
                    if (!string.IsNullOrWhiteSpace(errorMessage))
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

            return validationErrors;
        }

        public static Dictionary<string, Type> ColumnDataTypes()
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

        public static string GenerateBatchId(string fileName, DateTime date)
        {
            return fileName + "_" + RandomString() + "_" + date.ToString("yyyyMMddHHmmssffff");
        }
        
        public static string GetFileNameFromBatchId(string batchId)
        {
            var array = batchId.Split('_');
            array = array.Take(array.Count() - 3).ToArray();
            return string.Join("", array);
        }

        private static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}
