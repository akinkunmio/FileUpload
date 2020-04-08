using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadApi;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Qb.BillPaymentTransaction.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.QueueServices
{
    public class SendBillPaymentValidateMessageConsumer : IConsumer<ValidationResponseData>
    {
        private readonly IBillPaymentDbRepository _billPaymentDbRepository;
        private readonly INasRepository _nasRepository;
        private readonly IFileService _bulkBillPaymentService;
        private readonly ILogger<SendBillPaymentValidateMessageConsumer> _logger;


        public SendBillPaymentValidateMessageConsumer(IBillPaymentDbRepository billPaymentDbRepository, 
            INasRepository nasRepository,
            Func<FileServiceTypeEnum, IFileService> fileService,
            ILogger<SendBillPaymentValidateMessageConsumer> logger
            )
        {
            _billPaymentDbRepository = billPaymentDbRepository;
            _nasRepository = nasRepository;
            _logger = logger;
            _bulkBillPaymentService = fileService(FileServiceTypeEnum.BulkBillPayment);
        }

        public async Task Consume(ConsumeContext<ValidationResponseData> context)
        {
            var queueMessage = context.Message;
            var batchId = queueMessage.RequestId;
            try
            {
                var validationStatuses = await _nasRepository.ExtractValidationResult(new BillPaymentValidateMessage 
                    ( 
                        batchId: batchId,
                        resultLocation: queueMessage.ResultLocation, 
                        createdAt: queueMessage.CreatedAt 
                    )
                );

                _logger.LogInformation("Log information {queueMessage.RequestId} | {queueMessage.ResultLocation} | {queueMessage.CreatedAt}", batchId, queueMessage.ResultLocation, queueMessage.CreatedAt);
                
                if (validationStatuses.Count() > 0)
                    await _billPaymentDbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                    {
                        BatchId = batchId,
                        ModifiedDate = DateTime.Now.ToString(),
                        NasToValidateFile = queueMessage.ResultLocation,
                        NumOfValidRecords = validationStatuses.Where(v => v.Status.ToLower().Equals("valid")).Count(),
                        Status = GenericConstants.AwaitingInitiation,
                        RowStatuses = validationStatuses.ToList()
                    });
                    
                var validationResult = await _bulkBillPaymentService.GetBillPaymentResults(batchId, new PaginationFilter(validationStatuses.Count(), 1));

                await _nasRepository.SaveValidationResultFile(batchId, validationResult.RowStatuses as List<BillPaymentRowStatus>);
            }
            catch (AppException ex)
            {
                _logger.LogError("Error occured while inserting payment items in database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while inserting payment items in database with error message {ex.message} | {ex.StackTrace}", ex.Message, ex.StackTrace);
            }
        }
    }
}
