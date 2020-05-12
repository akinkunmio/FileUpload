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
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpService> _logger;

        public HttpService(HttpClient httpClient, IAppConfig appConfig, ILogger<HttpService> logger)
        {
            _logger = logger;
            _httpClient = httpClient;

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(appConfig.BillPaymentTransactionServiceUrl);
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = new TimeSpan(0, 0, 1, 0, 0);
        }

        private string GetValidateUrl(string contentType)
        {
            string result = "";

            if (contentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()))
                result = GenericConstants.ValidateBillPaymentUrl;

            else if(contentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
                result = GenericConstants.ValidateFirsUrl;

            return result;
        }

        private string GetInitiatePaymentUrl(string contentType)
        {
            string result = "";

            if (contentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()))
                result = GenericConstants.InitiateBillPaymentUrl;

            else if (contentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
                result = GenericConstants.InitiateFirPaymentUrl;

            return result;

        }

        public async Task<ValidationResponse> ValidateBillRecords(FileProperty fileProperty, string authToken, bool greaterThanFifty)
        {
            ValidationResponse validateResponse;
            try
            {
                var requestBody = ConstructValidateRequestString(greaterThanFifty, fileProperty.Url, fileProperty.ContentType, fileProperty.ItemType, fileProperty.BusinessTin);

                var request = new HttpRequestMessage(HttpMethod.Post, GetValidateUrl(fileProperty.ContentType))
                {
                    Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
                };

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
                    throw new AppException("Bad Request", (int)HttpStatusCode.BadRequest);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AppException("Unauthorized", (int)HttpStatusCode.Unauthorized);
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
                _logger.LogError("Error occured while making http request to initiate payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while making http request to initiate payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException("Error occured while performing bill payment validation"+ex.Message);
            }
        }

        private string ConstructValidateRequestString(bool check, string url, string contentType, string itemType = null, string businessTin = null, string batchId = null)
        {
            string result = "";
            if (contentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()))
                result =  check ?
                JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    BatchId = batchId
                })
                :
                JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url
                });

            if (contentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
                result = JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    TaxTypeCode = itemType.ToLower(),
                    BusinessTin = businessTin
                });

            return result;
        }

        private string ConstructConfirmedRequestString(InitiatePaymentOptions initiatePaymentOptions, FileProperty fileProperty)
        {
            string result = "";

            if (fileProperty.ContentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()))
                result = JsonConvert.SerializeObject(new
                {
                    BusinessId = initiatePaymentOptions.BusinessId,
                    UserId = initiatePaymentOptions.UserId,
                    ApprovalConfigId = initiatePaymentOptions.ApprovalConfigId,
                    UserName = initiatePaymentOptions.UserName,
                    DataStore = 1,
                    DataStoreUrl = fileProperty.Url
                });

            if (fileProperty.ContentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
                result = JsonConvert.SerializeObject(new
                {
                    TaxTypeId = initiatePaymentOptions.TaxTypeId,
                    TaxTypeName = initiatePaymentOptions.TaxTypeName,
                    ProductId = initiatePaymentOptions.ProductId,
                    CurrencyCode = initiatePaymentOptions.CurrencyCode,
                    TaxTypeCode = fileProperty.ItemType.ToUpper(),
                    ProductCode  = fileProperty.ContentType.ToUpper(),
                    CustomerNumber = initiatePaymentOptions.BusinessTin,
                    IsScheduleTaxType = true,
                    BusinessId = initiatePaymentOptions.BusinessId,
                    UserId = initiatePaymentOptions.UserId,
                    ApprovalConfigId = initiatePaymentOptions.ApprovalConfigId,
                    UserName = initiatePaymentOptions.UserName,
                    DataStore = 1,
                    DataStoreUrl = fileProperty.Url
                });

            return result;
        }

        public async Task<ConfirmedBillResponse> ConfirmedBillRecords(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions)
        {
            try
            {
                var req = ConstructConfirmedRequestString(initiatePaymentOptions, fileProperty);

                var request = new HttpRequestMessage(HttpMethod.Post, GetInitiatePaymentUrl(fileProperty.ContentType))
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
                _logger.LogError("Error occured while making http request to initiate payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while making http request to initiate payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw new AppException("Unknown error occured while initiating Bill Payment Initiation"+ex.Message);
            }

        }
    }

    public interface IHttpService
    {
        Task<ValidationResponse> ValidateBillRecords(FileProperty fileProperty, string authToken, bool greaterThanFifty);

        Task<ConfirmedBillResponse> ConfirmedBillRecords(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions);
    }

    public class ConfirmedBillResponse
    {
        public bool PaymentInitiated { get; set; }
    }
}
