using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using FilleUploadCore.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.UploadServices
{
    public class BillPaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IAppConfig _appConfig;

        public BillPaymentService(HttpClient httpClient, IAppConfig appConfig)
        {
            _appConfig = appConfig;
            _httpClient = httpClient;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_appConfig.BillPaymentTransactionServiceBaseUrl);
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ValidationResponse> ValidateBillRecords(string fileName, string filePath, string authToken)
        {
            ValidationResponse validateResponse;
            try
            {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/qbtrans/api/v1/payments/bills/validate?FileLocation={filePath}" +
                        $"&&FileName={fileName}");

                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                    var response = await _httpClient.SendAsync(request);
                    var responseResult = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        validateResponse = JsonConvert.DeserializeObject<ValidationResponse>(responseResult);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new AppException("Unable to perform bill payment validation");
                    }
                    else
                    {
                        throw new AppException("Error occured while performing bill payment validation");
                    }

                return validateResponse;
            }
            catch (Exception)
            {
                throw new AppException("Error occured while performing bill payment validation");
            }

        }
    }

    public interface IPaymentService
    {
        Task<ValidationResponse> ValidateBillRecords(string fileName, string filePath, string authToken);
    }
}
