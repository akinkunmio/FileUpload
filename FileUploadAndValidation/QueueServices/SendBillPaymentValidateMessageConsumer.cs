using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation.Repository;
using FileUploadAndValidation.UploadServices;
using FileUploadApi;
using FilleUploadCore.Exceptions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Qb.BillPaymentTransaction.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileUploadAndValidation.Utils;

namespace FileUploadAndValidation.QueueServices
{
    public class SendBillPaymentValidateMessageConsumer : IConsumer<ValidationResponseData>
    {
        private readonly IDbRepository _dbRepository;
        private readonly INasRepository _nasRepository;
        private readonly ILogger<SendBillPaymentValidateMessageConsumer> _logger;


        public SendBillPaymentValidateMessageConsumer(IDbRepository dbRepository, 
            INasRepository nasRepository,
            ILogger<SendBillPaymentValidateMessageConsumer> logger
            )
        {
            _dbRepository = dbRepository;
            _nasRepository = nasRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ValidationResponseData> context)
         {
            var queueMessage = context.Message;
            var batchId = queueMessage.RequestId;
            try
            {
                var validationStatuses = await _nasRepository.ExtractValidationResult(new PaymentValidateMessage 
                    ( 
                        batchId: batchId,
                        resultLocation: queueMessage.ResultLocation, 
                        createdAt: queueMessage.CreatedAt
                    )
                );

                _logger.LogInformation($"Log information {queueMessage.RequestId} | {queueMessage.ResultLocation} | {queueMessage.CreatedAt}");

                var validRowsCount = validationStatuses
                                                    .Where(v => v.Status.ToLower().Equals("valid"))
                                                    .Count();

                if (validationStatuses.Count() > 0)
                    await _dbRepository.UpdateValidationResponse(new UpdateValidationResponseModel
                    {
                        BatchId = batchId,
                        ModifiedDate = DateTime.Now.ToString(),
                        NasToValidateFile = queueMessage.ResultLocation,
                        NumOfValidRecords = validRowsCount,
                        Status = (validRowsCount > 0) ? GenericConstants.AwaitingInitiation : GenericConstants.NoValidRecord,
                        RowStatuses = validationStatuses.ToList()
                    });

                var fileSummary = await _dbRepository.GetBatchUploadSummary(batchId);
                    
                var validationResult = await _dbRepository.GetPaymentRowStatuses(batchId, new PaginationFilter(fileSummary.NumOfRecords, 1));
                validationResult = validationResult.OrderBy(v => v.RowNum);
                var fileName = await _nasRepository.SaveValidationResultFile(batchId, fileSummary.ItemType, fileSummary.ContentType, validationResult);

                await _dbRepository.UpdateUploadSuccess(batchId, fileName);
            }
            catch (AppException ex)
            {
                _logger.LogError($"Error occured while updating payment items from queue in database with error message {ex.Message} | {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while updating payment items from queue in database with error message {ex.Message} | {ex.StackTrace}");
            }
        }
    }
}
