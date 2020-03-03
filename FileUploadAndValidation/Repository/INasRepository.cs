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
                var fileLocation = _appConfig.NasFolderLocation + @"validate\";
                var fileName = batchId + "_validate.json";

                string json = JsonConvert.SerializeObject(billPayments);

                var path = fileLocation + fileName;

                File.WriteAllText(path, json);
                
                return Task.FromResult(new FileProperty 
                { 
                    BatchId = batchId, 
                    DataStore = 1,
                    Url = path
                });
            }
            catch(Exception)
            {
                throw new AppException($"An error occured while saving file with batch id : {batchId} to NAS for validation", 500);
            }
        }

        public Task<FileProperty> SaveFileToConfirmed(string batchId, IEnumerable<NasBillPaymentDto> billPayments)
        {
            try
            {
                var fileLocation = _appConfig.NasFolderLocation + @"\confirmed\";
                var fileName = batchId + "_confirmed.json";

                string json = JsonConvert.SerializeObject(billPayments);

                var path = fileLocation + fileName;

                File.WriteAllText(path, json);

                return Task.FromResult(new FileProperty
                {
                    BatchId = batchId,
                    DataStore = 1,
                    Url = fileLocation + fileName
                });
            }
            catch (Exception)
            {
                throw new AppException($"An error occured while saving file with batch id : {batchId} to NAS for validation", 500);
            }
        }

        public async Task<string> SaveRawFile(string batchId, Stream stream, string extension)
        {
            var fileLocation = _appConfig.NasFolderLocation + @"\raw\";
            var fileName = batchId + "_raw." + extension;

            string path = fileLocation + fileName;

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

            return path;
        }

        public async Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(BillPaymentValidateMessage queueMessage)
        {
            var result = new List<RowValidationStatus>();

            try
            {
                if (File.Exists(queueMessage.ResultLocation))
                {
                    result = JsonConvert.DeserializeObject<List<RowValidationStatus>>(System.IO.File.ReadAllText(queueMessage.ResultLocation));
                }

            }
            catch (Exception)
            {
                //log error to db 
                throw new AppException($"An error occured while extracting File Validation Result with BatchId : {queueMessage.BatchId} to NAS for validation", 500);
            }

            return await Task.FromResult(result?.AsEnumerable());
        }

      
    }
    public interface INasRepository
    {
        Task<FileProperty> SaveFileToValidate(string batchId, IEnumerable<NasBillPaymentDto> billPayments);

        Task<FileProperty> SaveFileToConfirmed(string batchId, IEnumerable<NasBillPaymentDto> billPayments);

        Task<string> SaveRawFile(string batchId, Stream stream, string extension);

        Task<IEnumerable<RowValidationStatus>> ExtractValidationResult(BillPaymentValidateMessage queueMessage);
    }

}
