﻿using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.Utils;
using FilleUploadCore.Exceptions;
using FilleUploadCore.UploadManagers;
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
                
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer ", authToken);

                    var response = await _httpClient.SendAsync(request);

                    var responseResult = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        validateResponse = JsonConvert.DeserializeObject<ValidationResponse>(responseResult);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new AppException("Unable to perform bill payment validation", (int)HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        throw new AppException("An error occured while performing bill payment validation", (int)response.StatusCode);
                    }

                return validateResponse;
            }
            catch (Exception)
            {
                throw new AppException("Error occured while performing bill payment validation", (int)HttpStatusCode.InternalServerError);
                //return new ValidationResponse 
                //{   
                //     NumOfRecords = 5,
                //     Results = new List<RowValidationStatus>() { 
                //        new RowValidationStatus
                //        {
                //              Row = 1,
                //              Status = "valid"  
                //        }
                //     },
                //     ResultsMode = "json"
                //};
            }

        }


        public async Task ConfirmedBillRecords(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions)
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

                request.Headers.Authorization = new AuthenticationHeaderValue(initiatePaymentOptions.AuthToken);

                try
                {
                    var response = await _httpClient.SendAsync(request);

                    var responseResult = await response.Content.ReadAsStringAsync();

                    var approvalResult = JsonConvert.DeserializeObject<InitiatePaymentResponse>(responseResult);

                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                    else
                    {
                        throw new AppException("Error occured while initiating Bill Payment Initiation");
                    }
                }
                catch (Exception)
                {
                    throw new AppException("Unknown error occured while initiating Bill Payment Initiation");
                }
            }
            catch(AppException appEx)
            {
                throw appEx;
            }
            catch(Exception ex)
            {
                throw new AppException("Unknown error occured! Please retry. " + ex.Message);
            }
        }

    }

    public interface IBillPaymentService
    {
        Task<ValidationResponse> ValidateBillRecords(FileProperty fileProperty, string authToken);

        Task ConfirmedBillRecords(FileProperty fileProperty, InitiatePaymentOptions initiatePaymentOptions);
    }
}
