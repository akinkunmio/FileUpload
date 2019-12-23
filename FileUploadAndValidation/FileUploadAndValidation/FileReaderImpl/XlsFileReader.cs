using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.FileReaders
{
    public class XlsFileReader : IFileReader
    {
        public IEnumerable<Row> Read(byte[] content)
        {
            return new List<Row>();
        }
    }
}
