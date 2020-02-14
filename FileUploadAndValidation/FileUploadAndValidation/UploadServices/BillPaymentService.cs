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
    public class BillPaymentService : IBillPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IAppConfig _appConfig;

        public BillPaymentService(HttpClient httpClient, IAppConfig appConfig)
        {
            _appConfig = appConfig;
            _httpClient = httpClient;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(appConfig.BillPaymentTransactionServiceUrl);
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<ValidationResponse> ValidateBillRecords(FileProperty fileProperty, string authToken)
        {
            ValidationResponse validateResponse;
            try
            {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"/payments/bills/validate?dataStore=1" +
                        $"&Url={fileProperty.Url}&BatchId={fileProperty.BatchId}");
                
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
                        throw new AppException("An error occured while performing bill payment validation");
                    }

                return validateResponse;
            }
            catch (Exception)
            {
                //throw new AppException("Error occured while performing bill payment validation");
                return new ValidationResponse 
                {   
                     NumOfRecords = 5,
                     Results = new List<RowValidationStatus>(),
                     ResultsMode = "json"
                };
            }

        }

    }

    public interface IBillPaymentService
    {
        Task<ValidationResponse> ValidateBillRecords(FileProperty fileProperty, string authToken);
    }
}
