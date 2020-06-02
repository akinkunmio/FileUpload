using System.Collections.Generic;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation
{
    public class ValidatedRow {
        public int Index { get; set; }
        public bool IsValid {get; set; }
        public IList<string> ErrorMessages { get; set; }
        public decimal Amount { get; set; }

        protected string GetColumnValue(List<Column> columns, int index, string defaultValue)
        {
            return columns.Count > index ? columns[index].Value : defaultValue;
        }
    }
}