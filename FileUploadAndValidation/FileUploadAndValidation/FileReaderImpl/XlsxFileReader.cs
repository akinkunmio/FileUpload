using FilleUploadCore.FileReaders;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace FileUploadAndValidation.FileReaders
{
    public class XlsxFileReader : IFileReader
    {
        public IEnumerable<Row> Read(Stream stream)
        {
            var rowList = new List<Row>();

            using (ExcelPackage excelPackage = new ExcelPackage(stream))
            {
                //loop all worksheets
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

                //loop all rows
                for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                {
                    var row = new Row() {
                        Index = i,
                        Columns = new List<Column>()
                    };
                    //loop all columns in a row
                    for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                    {
                        //add the cell data to the List
                        if (worksheet.Cells[i, j].Value != null)
                        {
                            row.Columns.Add(new Column() { Index = j, Value = worksheet.Cells[i, j].Value.ToString() });
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
