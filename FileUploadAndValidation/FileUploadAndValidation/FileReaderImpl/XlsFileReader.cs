using ExcelDataReader;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace FileUploadAndValidation.FileReaders
{
    public class XlsFileReader : IFileReader
    {
        public IEnumerable<Row> Read(byte[] content)
        {
            var rowList = new List<Row>();

            using (var stream = new MemoryStream(content))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();

                DataTable dataTable = result.Tables[0];

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    var row = new Row()
                    {
                        Index = i + 1,
                        Columns = new List<Column>()
                    };
                    DataRow dataRow = dataTable.Rows[i];
                    //loop all columns in a row
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        //add the cell data to the List
                        if (dataRow[j].ToString() != null)
                        {
                            row.Columns.Add(new Column() { Index = j, Value = dataRow[j].ToString() });
                        }
                        else
                        {
                            row.Columns.Add(new Column() { Index = j, Value = "" });
                        }
                    }
                    rowList.Add(row);
                }
            }
            return rowList;
        }
    }
}

