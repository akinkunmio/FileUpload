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

namespace FileUploadAndValidation.Repository
{
    public class NasRepository : INasRepository
    {
        private readonly ILogger<NasRepository> _logger;
        public NasRepository(ILogger<NasRepository> logger)
        {
            _logger = logger;
        }

        public async Task<FileProperty> SaveFileToValidate(string batchId, IEnumerable<NasBillPaymentDto> billPayments)
        {
            try
            {
                var fileLocation = @"../data/validate/";
                var fileName = batchId + "_validate.json";

                string json = JsonConvert.SerializeObject(billPayments);

                var path = fileLocation + fileName;

                File.WriteAllText(path, json);
                
                return await Task.FromResult(new FileProperty 
                { 
                    BatchId = batchId, 
                    DataStore = 1,
                    Url = $"validate/{fileName}"
                });
            }
            catch(Exception)
            {
                throw new AppException($"An error occured while saving file with batch id : {batchId} to NAS for validation", 500);
            }
        }

        public async Task<FileProperty> SaveFileToConfirmed(string batchId, IEnumerable<NasBillPaymentDto> billPayments)
        {
            try
            {
                var fileLocation = @"../data/confirmed/";
                var fileName = batchId + "_confirmed.json";

                string json = JsonConvert.SerializeObject(billPayments);

                var path = fileLocation + fileName;

                await File.WriteAllTextAsync(path, json);

                return await Task.FromResult(new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = $"confirmed/{fileName}"
                });
            }
            catch (Exception)
            {
                throw new AppException($"An error occured while saving file with batch id : {batchId} to NAS for validation", 500);
            }
        }

        public async Task<string> SaveRawFile(string batchId, Stream stream, string extension)
        {
            var fileLocation = @"../data/raw/";
            var fileName = batchId + "_raw." + extension;

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
                throw new AppException($"An error occured while saving raw file with batch id : {batchId} to NAS "+ ex.Message, 500);
            }

            return $"raw/{fileName}";
        }

        public async Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(BillPaymentValidateMessage queueMessage)
        {
            var result = new List<RowValidationStatus>();
            var location = @"../data/";
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
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Log information {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException($"An error occured while extracting File Validation Result with BatchId : {queueMessage.BatchId} to NAS for validation", (int)HttpStatusCode.InternalServerError);
            }

            return result?.AsEnumerable();
        }

        public async Task<string> GetTemplateFileContentAsync(string fileName, MemoryStream outputStream)
        {
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
                    return path;
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
                throw new AppException($"An error occured while extracting Template File in path : {path} from NAS"+ex.Message, (int)HttpStatusCode.InternalServerError);
            }
        }
    }
    public interface INasRepository
    {
        Task<FileProperty> SaveFileToValidate(string batchId, IEnumerable<NasBillPaymentDto> billPayments);

        Task<FileProperty> SaveFileToConfirmed(string batchId, IEnumerable<NasBillPaymentDto> billPayments);

        Task<string> SaveRawFile(string batchId, Stream stream, string extension);

        Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(BillPaymentValidateMessage queueMessage);

        Task<string> GetTemplateFileContentAsync(string fileName, MemoryStream outputStream);
    }

}
