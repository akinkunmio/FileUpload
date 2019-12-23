using FilleUploadCore.FileReaders;
using System.Collections.Generic;

namespace FileUploadAndValidation.FileReaders
{
    public class XlsxFileReader : IFileReader
    {
        public IEnumerable<Row> Read(byte[] content)
        {
            return new List<Row>();
        }
    }
}
