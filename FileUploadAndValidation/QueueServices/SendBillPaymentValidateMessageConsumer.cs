using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.QueueMessages;
using FileUploadAndValidation.Repository;
using FileUploadApi;
using MassTransit;
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

        public SendBillPaymentValidateMessageConsumer(IBillPaymentDbRepository billPaymentDbRepository, INasRepository nasRepository)
        {
            _billPaymentDbRepository = billPaymentDbRepository;
            _nasRepository = nasRepository;
        }

        public async Task Consume(ConsumeContext<ValidationResponseData> context)
        {
            var queueMessage = context.Message;
            var batchId = queueMessage.ResultLocation.Split('\\').Last().Split('.').First();

            var validationStatuses = await _nasRepository.ExtractValidationResult(new BillPaymentValidateMessage 
                ( 
                    batchId: queueMessage.RequestId,
                    resultLocation: queueMessage.ResultLocation, 
                    createdAt: queueMessage.CreatedAt 
                )
            );
            
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
        }
    }
}
