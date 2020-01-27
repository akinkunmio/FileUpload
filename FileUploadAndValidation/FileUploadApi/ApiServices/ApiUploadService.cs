using FileUploadAndValidation.FileReaderImpl;
using FileUploadAndValidation.FileReaderImpl.CsvTxtMappers;
using FileUploadAndValidation.FileReaders;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.UploadServices;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using FilleUploadCore.Helpers;
using FilleUploadCore.UploadManagers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;
using static FileUploadAndValidation.Models.UploadResult;

namespace FileUploadApi.ApiServices
{
    public class ApiUploadService : IApiUploadService
    {
        private readonly ITxtCsvReader<FirsWhtsModel> _txtCsvReader;
        private readonly IFileReader _xlsxFileReader;
        private readonly IFileReader _xlsFileReader;
        private readonly IFileService _firsWhtService;
        private readonly IFileService _autoPayService;
        private readonly IFileService _bulkBillPaymentService;
        private readonly IFileService _bulkSmsService;

        public ApiUploadService(Func<FileReaderTypeEnum, IFileReader> fileReader,
            Func<FileServiceTypeEnum, IFileService> fileService,
            ITxtCsvReader<FirsWhtsModel> txtCsvReader)
        {
            _txtCsvReader = txtCsvReader;
            _xlsxFileReader = fileReader(FileReaderTypeEnum.XLSX);
            _xlsFileReader = fileReader(FileReaderTypeEnum.XLS);
            _firsWhtService = fileService(FileServiceTypeEnum.FirsWht);
            _autoPayService = fileService(FileServiceTypeEnum.AutoPay);
            _bulkSmsService = fileService(FileServiceTypeEnum.BulkSMS);
            _bulkBillPaymentService = fileService(FileServiceTypeEnum.BulkBillPayment);
        }

        public async Task<UploadResult> UploadFileAsync(UploadOptions uploadOptions, FileTypes fileExtension, byte[] content)
        {
            ArgumentGuard.NotNull(fileExtension, nameof(fileExtension));
            IEnumerable<Row> rows = new List<Row>();
            var uploadResult = new UploadResult();
            List<CsvMappingResult<FirsWhtsModel>> csvMappingResult;
            uploadOptions.ContentType = "FIRS_WHT";
            uploadOptions.ValidateHeaders = true;

            switch (fileExtension)
            {
                case FileTypes.TXT:
                    var mappedRows = new List<Row>();
                    csvMappingResult = await _txtCsvReader.Read(content, new FirsWhtMapper());
                    if (csvMappingResult.Exists(r => r.IsValid == false))
                    {
                        csvMappingResult.ForEach(row =>
                        {
                            uploadResult.ErrorMessage += row.Error.ToString() + Environment.NewLine;
                            uploadResult.Failures = new List<Failure>
                            {
                                new Failure
                                {
                                   RowNumber = row.RowIndex,
                                    
                                }
                            };
                        });

                        if (uploadOptions.ValidateAllRows)
                        {
                            return uploadResult;
                        }
                    }
                    else
                    {
                        csvMappingResult.ForEach(r =>
                        {
                            if (r.IsValid)
                            {
                                mappedRows.Add(
                                    new Row
                                    {
                                        Index = r.RowIndex,
                                        Columns = new List<Column>
                                        {
                                                new Column{ Index = 0 , Value = r.Result.ContractorName },
                                                new Column{ Index = 1 , Value = r.Result.ContractorAddress },
                                                new Column{ Index = 2 , Value = r.Result.ContractorTIN },
                                                new Column{ Index = 3 , Value = r.Result.ContractDescription },
                                                new Column{ Index = 4 , Value = r.Result.TransactionNature },
                                                new Column{ Index = 5 , Value = r.Result.TransactionDate },
                                                new Column{ Index = 6 , Value = r.Result.TransactionInvoiceRefNo },
                                                new Column{ Index = 7 , Value = r.Result.CurrencyOfTransaction },
                                                new Column{ Index = 8 , Value = r.Result.InvoicedValue },
                                                new Column{ Index = 9 , Value = r.Result.ExchangeRateToNaira },
                                                new Column{ Index = 10 , Value = r.Result.InvoiceValueofTransaction },
                                                new Column{ Index = 11 , Value = r.Result.WVATRate },
                                                new Column{ Index = 12 , Value = r.Result.WVATValue },
                                                new Column{ Index = 13 , Value = r.Result.TaxAccountNumber },
                                        }
                                    }
                                );
                            }
                        });
                        if (mappedRows.Count < 0 || mappedRows == null)
                            return uploadResult;
                        rows = mappedRows;
                    }
                    break;
                case FileTypes.XLSX:
                    rows = _xlsxFileReader.Read(content);
                    break;
                case FileTypes.XLS:
                    rows = _xlsFileReader.Read(content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("File type not supported");
            }

            switch (uploadOptions.ContentType.ToUpperInvariant())      {
                case "FIRS_WHT":
                    return await _firsWhtService.Upload(uploadOptions, rows, uploadResult);
                case "AUTOPAY":
                    return await _autoPayService.Upload(uploadOptions, rows, uploadResult);
                case "BULKSMS":
                    return await _bulkSmsService.Upload(uploadOptions, rows, uploadResult);
                case "BULKBILLPAYMENT":
                    return await _bulkBillPaymentService.Upload(uploadOptions, rows, uploadResult);
                default:
                    throw new ArgumentOutOfRangeException("Content type not supported!.");
            }
        }
    }

    public enum FileTypes { XLS, XLSX, TXT, CSV }

}
