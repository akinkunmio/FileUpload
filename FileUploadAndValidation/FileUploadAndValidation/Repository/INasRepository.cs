using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.Repository
{
    public class NasRepository : INasRepository
    {
        private readonly IAppConfig _appConfig;
        public NasRepository(IAppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public Task<FileProperties> SaveAsJsonFile(string batchId, IEnumerable<BillPayment> billPayments)
        {
            var fileLocation = $"{_appConfig.NasFolderLocation}/Validated";
            var fileName = $"validate_{batchId}.json";
            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText($"{fileLocation}{fileName}"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, billPayments);
            }
            return Task.FromResult(new FileProperties { FileLocation = fileLocation, FileName = fileName });
        }
    }
    public interface INasRepository
    {
        Task<FileProperties> SaveAsJsonFile(string batchId, IEnumerable<BillPayment> billPayments);
    }

}
