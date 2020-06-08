namespace FileUploadAndValidation.BillPayments
{
    public class LASGPaymentContext
    {
        public LASGPaymentContext()
        {
            ProductCode = "LASG";
            ProductName = "Lagos State Collections";
        }
        public long UserId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}