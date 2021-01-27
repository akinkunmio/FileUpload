using CsvHelper;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using Microsoft.Extensions.Logging;
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
    public class CsvFileReader : IFileReader
    {
        private readonly ILogger<CsvFileReader> _logger;

        public CsvFileReader(ILogger<CsvFileReader> logger)
        {
            _logger = logger;
        }

        public bool CanRead(string fileExtension)
        {
            return fileExtension.ToLower() == "csv";
        }

        public IEnumerable<Row> Read(Stream stream)
        {
            var rowList = new List<Row>();
            var dataTable = new DataTable();
            bool createColumns = true;

            try
            {
                using (var reader = new StreamReader(stream, true))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    while (csv.Read())
                    {
                        if (createColumns)
                        {
                            for (int i = 0; i < csv.Context.Record.Length; i++)
                                dataTable.Columns.Add(i.ToString());
                            createColumns = false;
                        }

                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < csv.Context.Record.Length; i++)
                            row[i] = csv.Context.Record[i];
                        dataTable.Rows.Add(row);
                    }

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
            catch (Exception ex)
            {
                _logger.LogError($"An Error occured while extracting file, {ex.Message} | {ex.StackTrace}");

                throw new AppException("An error occured while extracting content of file. Please, retry!.", 400);
            }

            return rowList;
        }

    }
}
