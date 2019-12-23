using FileUploadAndValidation.FileReaders;
using FilleUploadCore.FileReaders;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FileUploadAndValidation
{
    class Program
    {
        private const string fileRecordsType = "FIRS_WHT";

        static void Main(string[] args)
        {
            #region "Startup.cs code"
            var uploadServiceFactory = new UploadServiceFactory(); //set this up in Startup.cs
            uploadServiceFactory.Register(fileRecordsType, new FIRS_WHTUploadService()); //set this up in Startup.cs

            var fileExtension = "csv"; //extract this from filename
            var fileReaderFactory = new FileHandlerFactory(); //set this up in Startup.cs
            fileReaderFactory.Register("csv", new CSVFileReader());
            fileReaderFactory.Register("txt", new CSVFileReader());
            #endregion

            try
            {
                var fileReader = fileReaderFactory.FindOrDefault(fileExtension);
                if (fileReader == null)
                    throw new ArgumentOutOfRangeException($"{fileExtension} file not supported");

                var bytes = Encoding.UTF8.GetBytes(GetFileContent());
                var rows = fileReader.Read(bytes);

                var uploadService = uploadServiceFactory.FindOrDefault(fileRecordsType); //new FIRS_WHTUploadService();

                if (uploadService == null)
                    throw new ArgumentOutOfRangeException($"{fileRecordsType} file not supported");

                Console.WriteLine("Rows count: " + rows.Count());
                uploadService.Upload(rows);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }

        private static string GetFileContent() =>
                new StringBuilder()
                .AppendLine("Name,Age,Job")
                .AppendLine("Tunmise,30,Developer")
                .AppendLine("James,33,DevOps")
                .ToString();
    }
}
