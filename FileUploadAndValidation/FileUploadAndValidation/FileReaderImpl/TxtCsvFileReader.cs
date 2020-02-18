using CsvHelper;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
                        var rowCount = dataTable.Rows.Count;
                        var columnCount = dataTable.Columns.Count;

                        for (int i = 0; i < rowCount; i++)
                        {
                            var row = new Row()
                            {
                                Index = i + 1,
                                Columns = new List<Column>()
                            };
                            DataRow dataRow = dataTable.Rows[i];
                            //loop all columns in a row
                            for (int j = 0; j < columnCount; j++)
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
                    throw new AppException("An error occured while extracting content of file!."+ ex.Message, (int)HttpStatusCode.InternalServerError);
                }
            }
            return rowList;
        }

    }
}
