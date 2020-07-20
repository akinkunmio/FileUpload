using FileUploadAndValidation.Utils;

namespace FileUploadAndValidation.BillPayments
{
    public class LASGPaymentContext
    {
        public LASGPaymentContext(IAppConfig appConfig)
        {
            ProductCode = appConfig.LasgProductCode;
            ProductName = "Lagos State Collections";
        }
        public long UserId { get; set; }
        public long BusinessId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}