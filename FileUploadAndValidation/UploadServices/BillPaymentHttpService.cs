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
using System.Web;

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
                    throw new AppException("Made a bad request while trying to perform bill payment validation", (int)HttpStatusCode.BadRequest);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AppException("Unauthorized to perform  bill payment validation", (int)HttpStatusCode.Unauthorized);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new AppException("File to validate was not found", (int)HttpStatusCode.NotFound);
                }
                else
                {
                    throw new AppException("An error occured while performing bill payment validation", (int)response.StatusCode);
                }

                return validateResponse;
            }
            catch(AppException ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new AppException("Error occured while performing bill payment validation"+ex.Message, (int)HttpStatusCode.InternalServerError);
            }

        }

        private HttpRequestMessage CheckGreaterFiftyRecords(bool check, string url, string batchId = null)
        {
            var request = check 
                ? new HttpRequestMessage(HttpMethod.Get, $"/qbtrans/api/v1/payments/bills/validate?dataStore=1" +
                    $"&Url={url}&BatchId={batchId}")
                : new HttpRequestMessage(HttpMethod.Get, $"/qbtrans/api/v1/payments/bills/validate?dataStore=1" +
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

                var request = new HttpRequestMessage(HttpMethod.Post, $"/qbtrans/api/v1/payments/bills/initiate-payment?dataStore=1&Url={fileProperty.Url}")
                {
                    Content = new StringContent(req, Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", initiatePaymentOptions.AuthToken.Replace("Bearer ", ""));


                var response = await _httpClient.SendAsync(request);
                var responseResult = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var approvalResult = JsonConvert.DeserializeObject<SuccessInitiatePaymentResponse>(responseResult);
                    return new ConfirmedBillResponse { PaymentInitiated = true };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var approvalResult = JsonConvert.DeserializeObject<FailedInitiatePaymentResponse>(responseResult);
                    throw new AppException(approvalResult.ResponseDescription, (int)HttpStatusCode.BadRequest, new ConfirmedBillResponse { PaymentInitiated = false });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AppException("Unauthorized to initiate bill transaction payment.", (int)HttpStatusCode.Unauthorized, new ConfirmedBillResponse { PaymentInitiated = false });
                }
                else
                {
                    FailedInitiatePaymentResponse approvalResult;
                    if (responseResult != null)
                    {
                        approvalResult = JsonConvert.DeserializeObject<FailedInitiatePaymentResponse>(responseResult);
                        throw new AppException(approvalResult.ResponseDescription, (int)response.StatusCode, new ConfirmedBillResponse { PaymentInitiated = false });
                    }
                    throw new AppException("Unable to initiate bill transaction payment.", (int)HttpStatusCode.Unauthorized, new ConfirmedBillResponse { PaymentInitiated = false });
                }
            }
            catch (AppException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new AppException("Unknown error occured while initiating Bill Payment Initiation"+ex.Message, (int)HttpStatusCode.InternalServerError);
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
