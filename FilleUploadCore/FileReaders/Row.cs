using System.Collections.Generic;

namespace FilleUploadCore.FileReaders
{
    public class Row
    {
        public List<Column> Columns { get; set; }
        public int Index { get; set; }

        public bool IsValid { get; set; }
        public bool Messages { get; set; }
    }
}
