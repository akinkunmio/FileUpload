using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FilleUploadCore.Exceptions;
using System.Linq;
using FileUploadAndValidation.QueueMessages;
using Microsoft.Extensions.Logging;
using System.Net;
using CsvHelper;
using System.Globalization;
using FileUploadAndValidation.Helpers;
using FilleUploadCore.Helpers;

namespace FileUploadAndValidation.Repository
{
    public class NasRepository : INasRepository
    {
        private readonly ILogger<NasRepository> _logger;
        private readonly IAppConfig _appConfig;
        public NasRepository(ILogger<NasRepository> logger, IAppConfig appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;
        }

        public async Task<FileProperty> SaveFileToValidate(string batchId, string contentType, string itemType, IEnumerable<RowDetail> rowDetails)
        {
            try
            {
                //var fileLocation = _appConfig.NasFolderLocation + @"\validate\";
                var fileLocation = @"../data/validate/";
                var fileName = batchId + "_validate.json";
                var path = fileLocation + fileName;

                if (!Directory.Exists(fileLocation))
                    Directory.CreateDirectory(fileLocation);

                string jsonString = JsonConvert.SerializeObject(GenericHelpers.GetSaveToNasFileContent(contentType, itemType, rowDetails));

                await File.WriteAllTextAsync(path, jsonString);

                return new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = $"validate/{fileName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                //return new FileProperty
                //{
                //    BatchId = batchId,
                //    DataStore = 1,
                //    Url = $"validate/firs_multitax1_ZMWYAA_202005290823495638_validate.json"
                //};
                throw new AppException($"An error occured while saving file for validation", 400);
            }
        }

        public async Task<FileProperty> SaveFileToConfirmed(string batchId, string contentType, string itemType, IEnumerable<RowDetail> rowDetails)
        {
            try
            {
                //var fileLocation = _appConfig.NasFolderLocation + @"\confirmed\";
                 var fileLocation = @"../data/confirmed/";
                var fileName = batchId + "_confirmed.json";

                if (!Directory.Exists(fileLocation))
                    Directory.CreateDirectory(fileLocation);

                string json = JsonConvert.SerializeObject(GenericHelpers.GetSaveToNasFileContent(contentType, itemType, rowDetails));

                var path = fileLocation + fileName;

                if (!File.Exists(path))
                {
                    await File.WriteAllTextAsync(path, json);
                }

                return new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = $"confirmed/{fileName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw new AppException($"An error occured while saving file. Please, retry!.", 400);
            }
        }

        public async Task<string> SaveTemplateFile(string fName, Stream stream, string ext)
        {
            var fileLocation = @"../data/template/";

            var fileName = fName + "." + ext;

            if (!Directory.Exists(fileLocation))
                Directory.CreateDirectory(fileLocation);


            string path = Path.Combine(fileLocation + fileName);

            try
            {
                using (FileStream outputFileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(outputFileStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw new AppException($"An error occured while saving template file {fName} to NAS ");
            }

            return $"Template file saved!.";
        }

        public async Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(PaymentValidateMessage queueMessage)
        {
            IEnumerable<RowValidationStatus> result;
            var location = @"../data/";
            //var location = _appConfig.NasFolderLocation;

            //var path = location + queueMessage.ResultLocation;

            var path = Path.Combine(location, queueMessage.ResultLocation);

            try
            {
                if (File.Exists(path))
                {
                    var extractedContent = await System.IO.File.ReadAllTextAsync(path, Encoding.UTF8);
                    result = JsonConvert.DeserializeObject<IEnumerable<RowValidationStatus>>(extractedContent);
                }
                else
                    throw new AppException($"Validation file not found at {path}", (int)HttpStatusCode.NotFound);
            }
            catch (AppException ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw new AppException($"An error occured while extracting file validation result", 400);
            }

            return result;
        }

        public async Task GetTemplateFileContentAsync(string fileName, MemoryStream outputStream)
        {
            ArgumentGuard.NotNullOrWhiteSpace(fileName, nameof(fileName));

            var location = @"../data/template/";
            var path = Path.Combine(location, fileName);

            try
            {
                if (File.Exists(path))
                {
                    using (FileStream fsSource = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        await fsSource.CopyToAsync(outputStream);
                    }
                }
                else
                    throw new AppException($"Template file not found at {path}", (int)HttpStatusCode.NotFound);
            }
            catch (AppException ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw new AppException($"An error occured while downloading template file. Please, retry!.", 400);
            }
        }

        public async Task GetUserValidationResultAsync(string fileName, MemoryStream outputStream)
        {
            var location = @"../data/uservalidationresult/";
            var path = Path.Combine(location, fileName);


            try
            {
                if (File.Exists(path))
                {
                    using (FileStream fsSource = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        await fsSource.CopyToAsync(outputStream);
                    }
                }
                else
                    throw new AppException($"File not found at {path}", (int)HttpStatusCode.NotFound);
            }
            catch (AppException ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw new AppException($"An error occured while extracting validation result", 400);
            }
        }

        public async Task<string> SaveValidationResultFile(string batchId, string itemType, string contentType, IEnumerable<RowDetail> content)
        {
            //var location = _appConfig.NasFolderLocation + @"\uservalidationresult\";
            var location = @"../data/uservalidationresult/";

            var fileName = batchId + "_validationresult.csv";

            var path = Path.Combine(location, fileName);

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            try
            {
                if (content == null)
                    throw new AppException($"No content for UserValidationResult for batch Id : {batchId}");

                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(MapToValidationResultFilePOCO(itemType, contentType, content));
                }
            }
            catch (AppException ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                // return "uservalidationresult/firs_wvt_X1KTNC_202005091720288960_validate.json";
                throw new AppException($"An error occured while saving the validation result file", 400);
            }

            return fileName;
        }

        private dynamic MapToValidationResultFilePOCO(string itemType, string contentType, IEnumerable<RowDetail> rowDetails)
        {
            dynamic result = default;

            if (itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem)
                || itemType.ToLower().Equals(GenericConstants.BillPaymentId))
            {
                result = rowDetails
                    .Select(s => new
                    {
                        Row = s.RowNum,
                        ErrorDescription = s.Error,
                        Status = s.RowStatus,
                        s.Amount,
                        s.CustomerId,
                        s.ItemCode,
                        s.ProductCode
                    });
            }

            if (itemType.ToLower().Equals(GenericConstants.Wht)
                && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                result = rowDetails
                    .Select(r => new
                    {
                        Row = r.RowNum,
                        ErrorDescription = r.Error,
                        Status = r.RowStatus,
                        r.BeneficiaryTin,
                        r.BeneficiaryName,
                        r.BeneficiaryAddress,
                        r.ContractDescription,
                        r.ContractDate,
                        r.ContractAmount,
                        r.InvoiceNumber,
                        r.ContractType,
                        r.PeriodCovered,
                        r.WhtRate,
                        r.WhtAmount
                    });
            }

            if (itemType.ToLower().Equals(GenericConstants.Wvat)
                && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                result = rowDetails
                    .Select(r => new
                    {
                        Row = r.RowNum,
                        ErrorDescription = r.Error,
                        Status = r.RowStatus,
                        ContractorName = r.ContractorName,
                        ContractorAddress = r.ContractorAddress,
                        ContractorTin = r.ContractorTin,
                        ContractDescription = r.ContractDescription,
                        TransactionDate = r.TransactionDate,
                        NatureOfTransaction = r.NatureOfTransaction,
                        InvoiceNumber = r.InvoiceNumber,
                        TransactionCurrency = r.TransactionCurrency,
                        CurrencyInvoicedValue = r.CurrencyInvoicedValue,
                        TransactionInvoicedValue = r.TransactionInvoicedValue,
                        CurrencyExchangeRate = r.CurrencyExchangeRate,
                        TaxAccountNumber = r.TaxAccountNumber,
                        WvatRate = r.WVATRate,
                        WvatValue = r.WVATValue
                    });
            }

            if (itemType.ToLower().Equals(GenericConstants.MultiTax)
               && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                result = rowDetails.Select(r => new
                {
                    Row = r.RowNum,
                    ErrorDescription = r.Error,
                    Status = r.RowStatus,
                    r.BeneficiaryTin,
                    r.BeneficiaryName,
                    r.BeneficiaryAddress,
                    r.ContractDescription,
                    r.ContractDate,
                    r.ContractAmount,
                    r.InvoiceNumber,
                    r.ContractType,
                    r.PeriodCovered,
                    r.WhtRate,
                    r.WhtAmount,
                    r.Amount,
                    r.Comment,
                    r.DocumentNumber,
                    r.PayerTin,
                    r.TaxType
                });
            }

            if (itemType.ToLower().Equals(GenericConstants.SingleTax)
                && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                result = rowDetails.Select(r => new
                {
                    Row = r.RowNum,
                    ErrorDescription = r.Error,

                    Status = r.RowStatus,
                    r.ContractorAddress,
                    r.ContractorName,
                    r.ContractorTin,
                    r.CurrencyExchangeRate,
                    r.CurrencyInvoicedValue,
                    r.ContractDescription,
                    r.NatureOfTransaction,
                    r.TaxAccountNumber,
                    r.TransactionCurrency,
                    r.TransactionDate,
                    r.TransactionInvoicedValue,
                    r.WVATRate,
                    r.WVATValue,
                    r.BeneficiaryTin,
                    r.BeneficiaryName,
                    r.BeneficiaryAddress,
                    r.ContractDate,
                    r.ContractAmount,
                    r.InvoiceNumber,
                    r.ContractType,
                    r.PeriodCovered,
                    r.WhtRate,
                    r.WhtAmount,
                    r.Amount,
                    r.Comment,
                    r.DocumentNumber,
                    r.CustomerTin,
                    r.CustomerName,
                    r.TaxType
                });
            }

            if (itemType.ToLower().Equals(GenericConstants.ManualCapture)
               && contentType.ToLower().Equals(GenericConstants.ManualCapture))
            {
                result = rowDetails.Select(r => new
                {
                    Row = r.RowNum,
                    r.ItemCode,
                    r.ProductCode,
                    ErrorDescription = r.Error,
                    Status = r.RowStatus,
                    r.Amount,
                    r.CustomerId,
                    r.CustomerName,
                    Desc = r.TaxType,
                    r.Email,
                    Address = r.AddressInfo
                });
            }

            if (itemType.ToLower().Equals(GenericConstants.Lasg)
               && contentType.ToLower().Equals(GenericConstants.Lasg))
            {
                result = rowDetails.Select(r => new
                {
                    Row = r.RowNum,
                    r.ItemCode,
                    r.ProductCode,
                    ErrorDescription = r.Error,
                    Status = r.RowStatus,
                    r.CustomerId,
                    r.Amount
                });
            }

            return result;
        }
        public async Task<FileProperty> SaveFileToValidate<T>(string batchId, IList<T> rowDetails)
        {
            try
            {
                //var fileLocation = _appConfig.NasFolderLocation + @"\validate\";
                var fileLocation = @"../data/validate/";
                var fileName = batchId + "_validate.json";
                var path = fileLocation + fileName;

                if (!Directory.Exists(fileLocation))
                    Directory.CreateDirectory(fileLocation);

                string jsonString = JsonConvert.SerializeObject(rowDetails);

                await File.WriteAllTextAsync(path, jsonString);

                return new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = $"validate/{fileName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                //return new FileProperty
                //{
                //    BatchId = batchId,
                //    DataStore = 1,
                //    Url = $"validate/firs_multitax1_ZMWYAA_202005290823495638_validate.json"
                //};
                throw new AppException($"An error occured while saving file for validation", 400);
            }
        }

        public async Task<FileProperty> SaveFileToValidate(string batchId, string contentType, string itemType, IEnumerable<RowDetail> rowDetails, string additionalData = null)
        {
            try
            {
                //var fileLocation = _appConfig.NasFolderLocation + @"\validate\";
                var fileLocation = @"../data/validate/";
                var fileName = batchId + "_validate.json";
                var path = fileLocation + fileName;

                if (!Directory.Exists(fileLocation))
                    Directory.CreateDirectory(fileLocation);

                string jsonString = JsonConvert.SerializeObject(GenericHelpers.GetSaveToNasFileContent(contentType, itemType, rowDetails, additionalData));

                await File.WriteAllTextAsync(path, jsonString);

                return new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = $"validate/{fileName}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Log information {ex.Message} | {ex.StackTrace}");
                //return new FileProperty
                //{
                //    BatchId = batchId,
                //    DataStore = 1,
                //    Url = $"validate/firs_multitax1_ZMWYAA_202005290823495638_validate.json"
                //};
                throw new AppException($"An error occured while saving file for validation", 400);
            }
        }


    }

    public interface INasRepository
    {
        Task<FileProperty> SaveFileToValidate<T>(string batchId, IList<T> rowDetails);

        Task<FileProperty> SaveFileToValidate(string batchId, string contentType, string itemType, IEnumerable<RowDetail> rowDetails);

        Task<FileProperty> SaveFileToValidate(string batchId, string contentType, string itemType, IEnumerable<RowDetail> rowDetails, string additionalData);

        Task<FileProperty> SaveFileToConfirmed(string batchId, string contentType, string itemType, IEnumerable<RowDetail> rowDetails);

        Task<string> SaveTemplateFile(string fName, Stream stream, string ext);

        Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(PaymentValidateMessage queueMessage);

        Task GetTemplateFileContentAsync(string fileName, MemoryStream outputStream);

        Task<string> SaveValidationResultFile(string batchId, string itemType, string contentType, IEnumerable<RowDetail> content);

        Task GetUserValidationResultAsync(string fileName, MemoryStream outputStream);
    }

}
