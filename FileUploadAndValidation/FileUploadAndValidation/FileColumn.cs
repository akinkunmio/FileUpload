﻿using System.Collections.Generic;

namespace FileUploadAndValidation
{
    internal class ColumnContract
    {
        public bool Required { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public IEnumerable<ValidationTypes> ValidationTypes { get; set; }
    }
}
