using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FilleUploadCore.FileReaders
{
    public interface IFileReader
    {
        IEnumerable<Row> Read(byte[] content);
    }

   
}

