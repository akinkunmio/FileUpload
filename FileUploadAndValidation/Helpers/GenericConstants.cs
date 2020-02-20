using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Helpers
{
    public static class GenericConstants
    {
        public const string PendingValidation = "PendingValidation";
        public const string AwaitingInitiation = "AwaitingInitiation";
        public const string AwaitingApproval = "AwaitingApproval";
        public const string BillPaymentIdPlusItem = "BillPaymentId+Item";
        public const string BillPaymentId = "BillPaymentId";
        public const string FirsWht = "firswht";
        public const string Autopay = "autopay";
        public const string Sms = "sms";
        public const string BillPayment = "billpayment";
        public const string ValidateBillerTransactions = "/qbtrans/api/v1/payments/bills/validate";
        public const int RECORDS_SMALL_SIZE = 50;
    }
}
