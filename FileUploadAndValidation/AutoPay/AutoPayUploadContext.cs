using FileUploadAndValidation.BillPayments;

namespace FileUploadAndValidation
{
    public class AutoPayUploadContext
    {

    }

    public class ManualCustomerCaptureContext
    {
        public ManualCustomerCaptureContext()
        {
            Configuration = new ManualCaptureRowConfig();
        }
        public ManualCaptureRowConfig Configuration { get; set; }
        public long UserId {get;set;}
        public long BusinessId {get;set;}
    }
}