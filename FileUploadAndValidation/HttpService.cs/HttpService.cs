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

        private string GetValidateUrl(string itemType)
        {

            if (itemType.ToLower().Equals(GenericConstants.BillPaymentId)
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem))
                return GenericConstants.ValidateBillPaymentUrl;

            else if(itemType.ToLower().Equals(GenericConstants.Firs))
                return GenericConstants.ValidateFirsUrl;

            else if (itemType.ToLower().Equals(GenericConstants.MultiTax))
                return GenericConstants.ValidateMultitaxUrl;

            return "";
        }

        private string GetInitiatePaymentUrl(string contentType, string itemType)
        {

            if (contentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()))
                return GenericConstants.InitiateBillPaymentUrl;

            else if (contentType.ToLower().Equals(GenericConstants.Firs.ToLower())
                && (itemType.ToLower().Equals(GenericConstants.Wht)
                || itemType.ToLower().Equals(GenericConstants.Wvat)))
                return GenericConstants.InitiateFirsPaymentUrl;

            else if (contentType.ToLower().Equals(GenericConstants.Firs.ToLower())
                && (itemType.ToLower().Equals(GenericConstants.MultiTax)))
                return GenericConstants.InitiateFirsMultitaxPaymentUrl;
            return "";
        }

        public async Task<ValidationResponse> ValidateRecords(FileProperty fileProperty, string authToken, bool greaterThanFifty)
        {
            var validateResponse =  new ValidationResponse();
            try
            {
                var requestBody = ConstructValidateRequestString(greaterThanFifty, fileProperty.Url, fileProperty.ContentType, fileProperty.ItemType, fileProperty.BusinessTin, fileProperty.BatchId);

                var request = new HttpRequestMessage(HttpMethod.Post, GetValidateUrl(fileProperty.ItemType))
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

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AppException("Contact your administrator to elevate your access rights!.", (int)HttpStatusCode.Unauthorized);
                }

                return validateResponse;
            }
            catch(AppException ex)
            {
                _logger.LogError("Error occured while making http request to validate payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while making http request to validate payment with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                //return new ValidationResponse
                //{
                //    ResponseCode = "90000",
                //    ResponseDescription = "Validation request is being processed",
                //    ResponseData = new ValidationData
                //    {
                //        NumOfRecords = 6,
                //        ResultMode = "Queue"
                //    }
                //};
                throw new AppException("Error occured while performing payment validation", 400);
            }
        }

        private string ConstructValidateRequestString(bool check, string url, string contentType, string itemType = null, string businessTin = null, string batchId = null)
        {
            string result = "";
            if (contentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()))
                result =  (check) 
                ?
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

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && (itemType.ToLower().Equals(GenericConstants.Wht)
                || itemType.ToLower().Equals(GenericConstants.Wvat)))
                result = JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    TaxTypeCode = itemType.ToLower(),
                    BusinessTin = businessTin
                });

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.MultiTax))
                result = JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    BatchId = batchId
                });

            return result;
        }

        private string ConstructInitiatePaymentRequestString(InitiatePaymentOptions initiatePaymentOptions, FileProperty fileProperty)
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
                    initiatePaymentOptions.TaxTypeId,
                    initiatePaymentOptions.TaxTypeName,
                    initiatePaymentOptions.ProductId,
                    initiatePaymentOptions.CurrencyCode,
                    TaxTypeCode = fileProperty.ItemType.ToUpper(),
                    ProductCode  = fileProperty.ContentType.ToUpper(),
                    CustomerNumber = initiatePaymentOptions.BusinessTin,
                    IsScheduleTaxType = true,
                    initiatePaymentOptions.BusinessId,
                    initiatePaymentOptions.UserId,
                    initiatePaymentOptions.ApprovalConfigId,
                    initiatePaymentOptions.UserName,
                    DataStore = 1,
                    DataStoreUrl = fileProperty.Url
                });

            return result;
        }

        public async Task<ConfirmedBillResponse> InitiatePayment(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions)
        {
            try
            {
                var req = ConstructInitiatePaymentRequestString(initiatePaymentOptions, fileProperty);

                var request = new HttpRequestMessage(HttpMethod.Post, GetInitiatePaymentUrl(fileProperty.ContentType, fileProperty.ItemType))
                {
                    Content = new StringContent(req, Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", initiatePaymentOptions.AuthToken.Replace("Bearer ", ""));


                var response = await _httpClient.SendAsync(request);
                var responseResult = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var approvalResult = JsonConvert.DeserializeObject<InitiatePaymentResponse>(responseResult);
                    return new ConfirmedBillResponse 
                    {
                         ApprovalStatusKey = approvalResult.ResponseData.approvalStatusKey,
                         NoOfApprovals = approvalResult.ResponseData.noOfApprovals,
                         Status = approvalResult.ResponseData.status,
                         Verdict = approvalResult.ResponseData.verdict
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var approvalResult = JsonConvert.DeserializeObject<InitiatePaymentResponse>(responseResult);
                    throw new AppException(approvalResult.ResponseDescription, (int)HttpStatusCode.BadRequest);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new AppException("Contact your administrator to elevate your access rights!.", (int)HttpStatusCode.Unauthorized);
                }
                else
                {
                    InitiatePaymentResponse approvalResult;
                    if (responseResult != null)
                    {
                        approvalResult = JsonConvert.DeserializeObject<InitiatePaymentResponse>(responseResult);
                        throw new AppException(approvalResult.ResponseDescription, (int)response.StatusCode);
                    }
                    throw new AppException("Unable to initiate payment. Please, retry!.", (int)HttpStatusCode.BadRequest);
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
                throw new AppException("An error occured. Please, retry!.", 400);
            }

        }
    }

    public interface IHttpService
    {
        Task<ValidationResponse> ValidateRecords(FileProperty fileProperty, string authToken, bool greaterThanFifty);

        Task<ConfirmedBillResponse> InitiatePayment(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions);
    }

    public class ConfirmedBillResponse
    {
        public object ApprovalStatusKey { get; set; }
        public string Status { get; set; }
        public string Verdict { get; set; }
        public string NoOfApprovals { get; set; }
    }
}
