namespace FileUploadAndValidation.BillPayments
{
    public class ManualCaptureRowConfig
    {
        public bool AutogenerateCustomerId { get; set; }
        public bool IsEmailRequired { get; set; }
        public bool IsPhoneNumberRequired { get; set; }
        public bool IsAddressRequired { get; set; }
    }
}