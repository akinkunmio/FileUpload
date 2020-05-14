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
      
        public async Task<FileProperty> SaveFileToValidate(string batchId, string itemType, IEnumerable<RowDetail> rowDetails)
        {
            try
            {
                //var fileLocation = _appConfig.NasFolderLocation + @"\validate\";
                var fileLocation = @"../data/validate/";
                var fileName = batchId + "_validate.json";

                dynamic rows = MapToNasToValidateFilePOCO(itemType, rowDetails); 
                string json = JsonConvert.SerializeObject(rows);

                var path = fileLocation + fileName;
               
                File.WriteAllText(path, json);

                return await Task.FromResult(new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = $"validate/{fileName}"
                });
            }
            catch(Exception ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                //return await Task.FromResult(new FileProperty
                //{
                //    BatchId = batchId,
                //    DataStore = 1,
                //    Url = $"validate/firs_wvat_X1KTNC_202005091720288960_validate.json"
                //});
                throw new AppException($"An error occured while saving file to NAS for validation");
            }
        }

        private dynamic MapToNasToValidateFilePOCO(string itemType, IEnumerable<RowDetail> rowDetails)
        {
            dynamic result = default;

            if(itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()) 
                || itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
            {
                result = rowDetails
                    .Select(r => new NasBillPaymentDto
                    {
                        Amount = decimal.Parse(r.Amount),
                        CustomerId = r.CustomerId,
                        ItemCode = r.ItemCode,
                        ProductCode = r.ProductCode,
                        Row = r.RowNum
                    });
            }

            if (itemType.ToLower().Equals(GenericConstants.Wht.ToLower()))
            {
                result = rowDetails
                    .Select(r => new FirsWhtTyped
                    {
                        Row = r.RowNum,
                        BeneficiaryTin = r.BeneficiaryTin,
                        BeneficiaryName = r.BeneficiaryName,
                        BeneficiaryAddress = r.BeneficiaryAddress,
                        ContractDate = r.ContractDate,
                        ContractAmount = decimal.Parse(r.ContractAmount),
                        ContractDescription = r.ContractDescription,
                        InvoiceNumber = r.InvoiceNumber,
                        ContractType = r.ContractType,
                        PeriodCovered = r.PeriodCovered,
                        WhtRate =decimal.Parse(r.WhtRate),
                        WhtAmount = decimal.Parse(r.WhtAmount),
                    });
            }

            if (itemType.ToLower().Equals(GenericConstants.Wvat.ToLower()))
            {
                result = rowDetails
                    .Select(r => new FirsWVatTyped
                    {
                        Row = r.RowNum,
                        ContractorName = r.ContractorName,
                        ContractorAddress = r.ContractorAddress,
                        ContractorTin = r.ContractorTin,
                        ContractDescription = r.ContractDescription,
                        TransactionDate = r.TransactionDate,
                        NatureOfTransaction = r.NatureOfTransaction,
                        InvoiceNumber = r.InvoiceNumber,
                        TransactionCurrency = r.TransactionCurrency,
                        CurrencyInvoicedValue = decimal.Parse(r.CurrencyInvoicedValue),
                        TransactionInvoicedValue = decimal.Parse(r.TransactionInvoicedValue),
                        CurrencyExchangeRate = decimal.Parse(r.CurrencyExchangeRate),
                        TaxAccountNumber = r.TaxAccountNumber,
                        WVATRate = decimal.Parse(r.WvatRate),
                        WVATValue = decimal.Parse(r.WvatValue)
                    });
            }

            return result;
        }

        public async Task<FileProperty> SaveFileToConfirmed(string batchId, string itemType, IEnumerable<RowDetail> rowDetails)
        {
            try
            {
                //var fileLocation = _appConfig.NasFolderLocation + @"\confirmed\";
                var fileLocation = @"../data/raw/";
                var fileName = batchId + "_confirmed.json";

                string json = JsonConvert.SerializeObject(MapToNasToValidateFilePOCO(itemType, rowDetails));

                var path = fileLocation + fileName;

                await File.WriteAllTextAsync(path, json);

                return await Task.FromResult(new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = $"confirmed/{fileName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while saving file with batch id : {batchId} to NAS for validation");
            }
        }

        public async Task<string> SaveRawFile(string batchId, Stream stream, string extension)
        {
            var fileLocation = @"../data/raw/";
            var fileName = batchId + "_raw." + extension;

            //var path = fileLocation + fileName;

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
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while saving raw file with batch id : {batchId} to NAS ");
            }

            return $"raw/{fileName}";
        }

        public async Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(PaymentValidateMessage queueMessage)
        {
            var result = new List<RowValidationStatus>();
            var location = @"../data/";
            //var location = _appConfig.NasFolderLocation;

            //var path = location + queueMessage.ResultLocation;

            var path = Path.Combine(location, queueMessage.ResultLocation);
         
            try
            {
                if (File.Exists(path))
                {
                    var extractedContent = await System.IO.File.ReadAllTextAsync(path, Encoding.Unicode);
                    result = JsonConvert.DeserializeObject<List<RowValidationStatus>>(extractedContent);
                }
                else
                    throw new AppException($"Validation file not found at {path}", (int)HttpStatusCode.NotFound);
            }
            catch(AppException ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while extracting File Validation Result with BatchId : {queueMessage.BatchId} to NAS for validation", (int)HttpStatusCode.InternalServerError);
            }

            return result?.AsEnumerable();
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
            catch(AppException ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while extracting Template File in path : {path} from NAS");
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
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while extracting Validation Result File in path : {path} from NAS");
            }
        }

        public async Task<string> SaveValidationResultFile(string batchId, string itemType, IEnumerable<RowDetail> content)
        {
            var location = _appConfig.NasFolderLocation + @"\uservalidationresult\";
            var fileName = batchId + "_validationresult.csv";

            var path = Path.Combine(location, fileName);

            try
            {
                if (content == null)
                    throw new AppException($"No content for UserValidationResult for batch Id : {batchId}");

                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(MapToValidationResultFilePOCO(itemType, content));
                }
            }
            catch(AppException ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch(Exception ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
               // return "uservalidationresult/firs_wvt_X1KTNC_202005091720288960_validate.json";
                throw new AppException($"An error occured while saving the user validation result file to NAS ");
            }

            return fileName;
        }

        private dynamic MapToValidationResultFilePOCO(string itemType, IEnumerable<RowDetail> rowDetails)
        {
            dynamic result = default;

            if (itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem.ToLower()) || itemType.ToLower().Equals(GenericConstants.BillPaymentId.ToLower()))
            {
                result = rowDetails
                    .Select(s => new BillPaymentRowStatusTyped
                    {
                        Row = s.RowNum,
                        Error = s.Error,
                        Status = s.RowStatus,
                        Amount = double.Parse(s.Amount),
                        CustomerId = s.CustomerId,
                        ItemCode = s.ItemCode,
                        ProductCode = s.ProductCode
                    });
            }

            if (itemType.ToLower().Equals(GenericConstants.Wht.ToLower()))
            {
                result = rowDetails
                    .Select(r => new FirsWhtRowStatusTyped
                    {
                        Row = r.RowNum,
                        Error = r.Error,
                        Status = r.RowStatus,
                        BeneficiaryTin = r.BeneficiaryTin,
                        BeneficiaryName = r.BeneficiaryName,
                        BeneficiaryAddress = r.BeneficiaryAddress,
                        ContractDate = r.ContractDate,
                        ContractAmount = decimal.TryParse(r.ContractAmount, out decimal amount) ? decimal.Parse(r.ContractAmount) : default,
                        InvoiceNumber = r.InvoiceNumber,
                        ContractType = r.ContractType,
                        PeriodCovered = r.PeriodCovered,
                        WhtRate = decimal.TryParse(r.WhtRate, out decimal rate) ? decimal.Parse(r.WhtRate) : default,
                        WhtAmount = decimal.TryParse(r.WhtAmount, out decimal wamount) ? decimal.Parse(r.WhtAmount) : default
                    });
            }

            if (itemType.ToLower().Equals(GenericConstants.Wvat.ToLower()))
            {
                result = rowDetails
                    .Select(r => new FirsWVatRowStatusTyped
                    {
                        Row = r.RowNum,
                        ContractorName = r.ContractorName,
                        ContractorAddress = r.ContractorAddress,
                        ContractorTin = r.ContractorTin,
                        ContractDescription = r.ContractDescription,
                        TransactionDate = r.TransactionDate,
                        NatureOfTransaction = r.NatureOfTransaction,
                        InvoiceNumber = r.InvoiceNumber,
                        TransactionCurrency = r.TransactionCurrency,
                        CurrencyInvoicedValue = decimal.TryParse(r.CurrencyInvoicedValue, out decimal civ) ? decimal.Parse(r.CurrencyInvoicedValue) : default,
                        TransactionInvoicedValue = decimal.TryParse(r.TransactionInvoicedValue, out decimal tiv) ? decimal.Parse(r.TransactionInvoicedValue) : default,
                        CurrencyExchangeRate = decimal.TryParse(r.CurrencyExchangeRate, out decimal cer) ? decimal.Parse(r.CurrencyExchangeRate) : default,
                        TaxAccountNumber = r.TaxAccountNumber,
                        WvatRate = decimal.TryParse(r.WvatRate, out decimal wr) ? decimal.Parse(r.WvatRate) : default,
                        WvatValue = decimal.TryParse(r.WvatValue, out decimal wv) ? decimal.Parse(r.WvatValue) : default
                    });
            }

            return result;
        }

    }
    public interface INasRepository
    {
        Task<FileProperty> SaveFileToValidate(string batchId, string itemType, IEnumerable<RowDetail> rowDetails);

        Task<FileProperty> SaveFileToConfirmed(string batchId, string itemType, IEnumerable<RowDetail> rowDetails);

        Task<string> SaveRawFile(string batchId, Stream stream, string extension);

        Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(PaymentValidateMessage queueMessage);

        Task GetTemplateFileContentAsync(string fileName, MemoryStream outputStream);

        Task<string> SaveValidationResultFile(string batchId, string itemType, IEnumerable<RowDetail> content);

        Task GetUserValidationResultAsync(string fileName, MemoryStream outputStream);
    }

}
