using System;
using System.Collections.Generic;
using System.Text;

namespace FilleUploadCore.FileReaders
{
    public interface IFileReader
    {
        IEnumerable<Row> Read(byte[] content);
    }
}
