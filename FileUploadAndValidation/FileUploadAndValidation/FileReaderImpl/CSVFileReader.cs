using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileUploadAndValidation.FileReaders
{
    public class CSVFileReader : IFileReader
    {
        public IEnumerable<Row> Read(byte[] content)
        {
            var csv = Encoding.UTF8.GetString(content);
            var lines = csv.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            return lines.Select((line, i) => GetRow(line, i));
        }

        private Row GetRow(string row, int index)
        {
            return new Row
            {
                Index = index,
                Columns = row.Split(";")
                          .Select((value, i) => new Column { Index = i, Value = value })
                          .ToList()
            };
        }
    }
}
