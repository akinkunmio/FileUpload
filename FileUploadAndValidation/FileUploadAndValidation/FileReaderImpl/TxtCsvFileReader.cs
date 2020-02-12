using CsvHelper;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FileUploadAndValidation.FileReaderImpl
{
    public class TxtCsvFileReader : IFileReader
    {
        public IEnumerable<Row> Read(Stream stream)
        {
            var rowList = new List<Row>();

            using (StreamReader sr = new StreamReader(stream))
            using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture))
            {
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                try
                {
                    using (var dr = new CsvDataReader(csv))
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(dr);

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
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return rowList;
        }

    }
}
