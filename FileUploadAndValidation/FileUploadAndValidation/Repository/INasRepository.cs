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

namespace FileUploadAndValidation.Repository
{
    public class NasRepository : INasRepository
    {
        private readonly IAppConfig _appConfig;
        public NasRepository(IAppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public Task<FileProperty> SaveFileToValidate(string batchId, IEnumerable<NasBillPaymentDto> billPayments)
        {
            try
            {
                var fileLocation = _appConfig.NasFolderLocation;
                var fileName = @"/UploadService/" + batchId + "_validate.json";
                string json = JsonConvert.SerializeObject(billPayments);
                File.WriteAllText(fileLocation + fileName, json);
                
                return Task.FromResult(new FileProperty 
                { 
                    BatchId = batchId, 
                    DataStore = 1,
                    Url = fileLocation + fileName
                });
            }
            catch(Exception)
            {
                throw new AppException($"An error occured while saving file with batch id : {batchId} to NAS for validation", 500);
            }
        }

        public Task<string> SaveRawFile(string batchId, Stream stream, string extension)
        {
            var filePath = _appConfig.NasFolderLocation + @"/UploadService";
            var fileName = batchId + "_raw." + extension;
            string path = Path.Combine(filePath, fileName);
            try
            {
                using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
                {
                    stream.CopyTo(outputFileStream);
                }
                return Task.FromResult(path);
            }
            catch (Exception)
            {
                throw new AppException($"An error occured while saving raw file with batch id : {batchId} to NAS", 500);
            }
        }

        public async Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(BillPaymentValidatedQueueMessage queueMessage)
        {
            var result = new List<RowValidationStatus>();
            var fileLocation = Path.Combine(_appConfig.NasFolderLocation, queueMessage.ResultLocation);
            try
            {
                if (File.Exists(fileLocation))
                {
                    result = JsonConvert.DeserializeObject<List<RowValidationStatus>>(System.IO.File.ReadAllText(fileLocation));
                }

            }
            catch (Exception)
            {
                //log error to db 
                //throw new AppException($"An error occured while extracting file with batch id : {batchId} to NAS for validation", 500);
            }

            return await Task.FromResult(result?.AsEnumerable());
        }
    }
    public interface INasRepository
    {
        Task<FileProperty> SaveFileToValidate(string batchId, IEnumerable<NasBillPaymentDto> billPayments);

        Task<string> SaveRawFile(string batchId, Stream stream, string extension);

        Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(BillPaymentValidatedQueueMessage queueMessage);
    }

}
