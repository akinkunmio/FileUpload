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
        private readonly HttpClient _approvalHttpClient;
        private readonly ILogger<HttpService> _logger;

        public HttpService(HttpClient httpClient, IAppConfig appConfig, ILogger<HttpService> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            _approvalHttpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(appConfig.BillPaymentTransactionServiceUrl);
            _approvalHttpClient.BaseAddress = new Uri(appConfig.ApprovalServiceUrl);
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

            else if(itemType.ToLower().Equals(GenericConstants.SingleTax))
                return GenericConstants.ValidateFirsUrl;

            else if (itemType.ToLower().Equals(GenericConstants.MultiTax))
                return GenericConstants.ValidateMultitaxUrl;
            else if (itemType.ToLower().Equals(GenericConstants.ManualCapture))
                return GenericConstants.ValidateManualCaptureUrl;
            else if (itemType.ToLower().Equals(GenericConstants.Lasg))
                return GenericConstants.ValidateLasgDataUrl;

            return "";
        }

        private string GetInitiatePaymentUrl(string contentType, string itemType)
        {

            if (contentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()) 
                || contentType.ToLower().Equals(GenericConstants.ManualCapture.ToLower())
                || contentType.ToLower().Equals(GenericConstants.Lasg.ToLower()))
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

        public async Task<ApprovalConfigResponseList> GetApprovalConfiguration(long? businessId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/approval/configs/{businessId}");
                request.Headers.Authorization = new AuthenticationHeaderValue(GenericConstants.ApprovalEngineAuthCode);

                var response = await _approvalHttpClient.SendAsync(request);
                var responseResult = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    int statusCode = 400;
                    var error = JsonConvert.DeserializeObject<ApprovalConfigError>(responseResult);
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        statusCode = 401;
                    else
                        statusCode = 400;

                    throw new AppException(error.responseDescription, statusCode);
                }
                var config = JsonConvert.DeserializeObject<ApprovalConfigResponseList>(responseResult);
                return config;
            }
            catch (AppException ex)
            {
                _logger.LogError($"Error while fetching approval configuration {ex.Message} | {ex.StackTrace}");
                throw new AppException(ex.Message, ex.StatusCode); ;
            }
            catch (Exception ex)
            {
                _logger.LogError($"unable to fetch approval configuration {ex.Message} | {ex.StackTrace}");
                throw new AppException("An unexpected error occured.Please try again later.", 400);
            }
            
        }

        public async Task<ValidationResponse> ValidateRecords(FileProperty fileProperty, string authToken)
        {
            var validateResponse =  new ValidationResponse();
            try
            {
                var requestBody = ConstructValidateRequestString(fileProperty.BusinessId, fileProperty.Url, fileProperty.ContentType, fileProperty.ItemType, fileProperty.BusinessTin, fileProperty.BatchId, fileProperty.AdditionalData);

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
                    throw new AppException("Invalid Access Token", (int)HttpStatusCode.Unauthorized);
                }

                return validateResponse;
            }
            catch(AppException ex)
            {
                _logger.LogError($"Error occured while making http request to validate payment with error message {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while trying to make http request to validate payment with error message {ex.Message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
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
                throw new AppException("An error occured. Please, retry!.", 400);
            }
        }

        private string ConstructValidateRequestString(long businessId, string url, string contentType, string itemType = null, string businessTin = null, string batchId = null, string additionalData = null)
        {
            string result = "";
            if (contentType.ToLower().Equals(GenericConstants.BillPayment.ToLower()))
                result =
                JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    BatchId = batchId,
                    BusinessId = businessId
                });

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.SingleTax))
                result = JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    BatchId = batchId,
                    BusinessId = businessId,
                    TaxTypeCode = additionalData.ToLower(),
                    BusinessTin = businessTin,
                });

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.MultiTax))
                result = JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    BatchId = batchId,
                    BusinessId = businessId
                });

            if (contentType.ToLower().Equals(GenericConstants.ManualCapture)
                && itemType.ToLower().Equals(GenericConstants.ManualCapture))
                result = JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    BatchId = batchId,
                    BusinessId = businessId
                });

            if (contentType.ToLower().Equals(GenericConstants.Lasg)
                && itemType.ToLower().Equals(GenericConstants.Lasg))
                result = JsonConvert.SerializeObject(new
                {
                    DataStore = 1,
                    DataStoreUrl = url,
                    BatchId = batchId,
                    BusinessId = businessId
                });

            return result;
        }

        private string ConstructInitiatePaymentRequestString(InitiatePaymentOptions initiatePaymentOptions, FileProperty fileProperty)
        {
            string result = "";

            if (fileProperty.ContentType.ToLower().Equals(GenericConstants.BillPayment.ToLower())
                || fileProperty.ContentType.ToLower().Equals(GenericConstants.Lasg.ToLower())
                || fileProperty.ContentType.ToLower().Equals(GenericConstants.ManualCapture.ToLower())
                || fileProperty.ContentType.ToLower().Equals(GenericConstants.Lasg.ToLower())
                )
                result = JsonConvert.SerializeObject(new
                {
                    initiatePaymentOptions.BusinessId,
                    initiatePaymentOptions.UserId,
                    initiatePaymentOptions.ApprovalConfigId,
                    initiatePaymentOptions.UserName,
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
                    throw new AppException("Invalid Access Token", (int)HttpStatusCode.Unauthorized);
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
                _logger.LogError($"Error occured while making http request to initiate payment with error message {ex.Message} | {ex.StackTrace}");
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while making http request to initiate payment with error message {ex.Message} | {ex.StackTrace}");
                throw new AppException("An error occured. Please, retry!.", 400);
            }
        }
    }

    public interface IHttpService
    {
        Task<ValidationResponse> ValidateRecords(FileProperty fileProperty, string authToken);

        Task<ConfirmedBillResponse> InitiatePayment(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions);

        Task<ApprovalConfigResponseList> GetApprovalConfiguration(long? businessId);
    }

    public class ConfirmedBillResponse
    {
        public object ApprovalStatusKey { get; set; }
        public string Status { get; set; }
        public string Verdict { get; set; }
        public string NoOfApprovals { get; set; }
    }
}
