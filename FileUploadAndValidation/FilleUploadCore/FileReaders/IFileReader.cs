using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FilleUploadCore.FileReaders
{
    public interface IFileReader
    {
        IEnumerable<Row> Read(Stream stream);
    }

   
}

