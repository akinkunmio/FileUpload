using FilleUploadCore.FileReaders;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace FileUploadAndValidation.FileReaders
{
    public class XlsxFileReader : IFileReader
    {
        public IEnumerable<Row> Read(byte[] content)
        {
            var rowList = new List<Row>();

            using(var stream = new MemoryStream(content))
            using (ExcelPackage excelPackage = new ExcelPackage(stream))
            {
                //loop all worksheets
                foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                {
                    //loop all rows
                    for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                    {
                        var row = new Row();
                        row.Index = i;
                        //loop all columns in a row
                        for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                        {
                            //add the cell data to the List
                            if (worksheet.Cells[i, j].Value != null)
                            {
                                //validate each row of the sheet
                                row.Columns.Add(new Column() { Index = j, Value = worksheet.Cells[i, j].Value.ToString() });
                            }
                        }
                        rowList.Add(row);
                    }
                }
            }

            return rowList;
        }
    }
}
