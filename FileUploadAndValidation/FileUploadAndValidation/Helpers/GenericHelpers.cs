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
    }
}
