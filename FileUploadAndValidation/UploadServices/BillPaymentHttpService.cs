using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using FilleUploadCore.Exceptions;
using FilleUploadCore.UploadManagers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.UploadServices
{
    public class BillPaymentHttpService : IBillPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IAppConfig _appConfig;
        private readonly ILogger<BillPaymentHttpService> _logger;
        public BillPaymentHttpService(HttpClient httpClient, IAppConfig appConfig, ILogger<BillPaymentHttpService> logger)
        {
            _appConfig = appConfig;
            _logger = logger;
            _httpClient = httpClient;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(appConfig.BillPaymentTransactionServiceUrl);
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = new TimeSpan(0, 0, 1, 0, 0);
        }

        public async Task<ValidationResponse> ValidateBillRecords(FileProperty fileProperty, string authToken, bool greaterThanFifty)
        {
            ValidationResponse validateResponse;
            var request = new HttpRequestMessage();
            try
            {

                request = 
                    greaterThanFifty
                    ? CheckGreaterFiftyRecords(greaterThanFifty, fileProperty.Url, fileProperty.BatchId)
                    : CheckGreaterFiftyRecords(greaterThanFifty, fileProperty.Url);

                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.Replace("Bearer ", ""));
                _httpClient.DefaultRequestHeaders.Authorization =
                   new AuthenticationHeaderValue("Bearer", authToken.Replace("Bearer ", ""));

                var response = await _httpClient.SendAsync(request);

                var responseResult = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    validateResponse = JsonConvert.DeserializeObject<ValidationResponse>(responseResult);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new AppException("Unable to perform bill payment validation ", (int)HttpStatusCode.BadRequest);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new AppException("File to validate not found", (int)HttpStatusCode.NotFound);
                }
                else
                {
                    throw new AppException("An error occured while performing bill payment validation "+ response.StatusCode, (int)response.StatusCode);
                }

                return validateResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new AppException("Error occured while performing bill payment validation", (int)HttpStatusCode.InternalServerError);
                //return new ValidationResponse
                //{
                //    NumOfRecords = 51,
                //    Results = new List<RowValidationStatus>(),
                //    ResultsMode = "queue"
                //};
            }

        }

        private HttpRequestMessage CheckGreaterFiftyRecords(bool check, string url, string batchId = null)
        {
            HttpRequestMessage request = check 
                ? new HttpRequestMessage(HttpMethod.Get, $"/payments/bills/validate?dataStore=1" +
                    $"&Url={url}&BatchId={batchId}")
                : new HttpRequestMessage(HttpMethod.Get, $"/payments/bills/validate?dataStore=1" +
                    $"&Url={url}");

            return request;
        }

        public async Task<ConfirmedBillResponse> ConfirmedBillRecords(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions)
        {
            try
            {
                var req = JsonConvert.SerializeObject(new
                {
                    BusinessId = initiatePaymentOptions.BusinessId,
                    UserId = initiatePaymentOptions.UserId,
                    ApprovalConfigId = initiatePaymentOptions.ApprovalConfigId,
                    UserName = initiatePaymentOptions.UserName
                });

                var request = new HttpRequestMessage(HttpMethod.Post, "/payments/bills/initiate-payment?dataStore=1" +
                    $"&Url={fileProperty.Url}&BatchId={fileProperty.BatchId}")
                {
                    Content = new StringContent(req, Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", initiatePaymentOptions.AuthToken.Replace("Bearer ", ""));

                try
                {
                    var response = await _httpClient.SendAsync(request);

                    var responseResult = await response.Content.ReadAsStringAsync();

                    var approvalResult = JsonConvert.DeserializeObject<InitiatePaymentResponse>(responseResult);

                    if (response.IsSuccessStatusCode)
                        return new ConfirmedBillResponse { PaymentInitiated = true };
                    else
                        return new ConfirmedBillResponse { PaymentInitiated = false };
;                }
                catch (Exception)
                {
                    throw new AppException("Unknown error occured while initiating Bill Payment Initiation");
                }
            }
            catch (AppException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new AppException("Unknown error occured! Please retry.");
            }
        }

    }

    public interface IBillPaymentService
    {
        Task<ValidationResponse> ValidateBillRecords(FileProperty fileProperty, string authToken, bool greaterThanFifty);

        Task<ConfirmedBillResponse> ConfirmedBillRecords(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions);
    }

    public class ConfirmedBillResponse
    {
        public bool PaymentInitiated { get; set; }
    }
}
