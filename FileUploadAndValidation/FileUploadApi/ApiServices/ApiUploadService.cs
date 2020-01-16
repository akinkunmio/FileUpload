using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.Models;
using FileUploadApi.Services;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.ApiServices
{
    public class ApiUploadService : IApiUploadService
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileReader _csvFileReader;
        private readonly IFileReader _xlsxFileReader;
        private readonly IFileReader _xlsFileReader;

        public ApiUploadService(Func<FileReaderEnum, IFileReader> fileReader, IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
            _csvFileReader = fileReader(FileReaderEnum.csv);
            _xlsxFileReader = fileReader(FileReaderEnum.xlsx);
            _xlsFileReader = fileReader(FileReaderEnum.xls);
        }

        public async Task<UploadResult> UploadFileAsync(FileTypes fileExtension, byte[] content)
        {
            ArgumentGuard.NotNull(fileExtension, nameof(fileExtension));
            IEnumerable<Row> _;
            var uploadResult = new UploadResult();

            switch (fileExtension)
            {
                case FileTypes.CSV:
                    _ = _csvFileReader.Read(content);
                    break;
                case FileTypes.TXT:
                    _ = _csvFileReader.Read(content);
                    break;
                case FileTypes.XLSX:
                    _ = _xlsxFileReader.Read(content);
                    break;
                case FileTypes.XLS:
                    _ = _xlsFileReader.Read(content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("File type not supported");
            }

           return  await _fileUploadService.Upload(_);
        }
       
    }

    public enum FileTypes { XLS, XLSX, TXT, CSV }
}
