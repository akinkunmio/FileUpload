namespace FileUploadApi
{
    public class ConfirmedBillPaymentDto
    {
        public int Row { get; set; }

        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }
      
    }
}