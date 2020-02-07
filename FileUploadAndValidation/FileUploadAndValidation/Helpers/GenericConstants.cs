using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Helpers
{
    public static class GenericConstants
    {
        public static string PendingValidation = "PendingValidation";
        public static string AwaitingInitiation = "AwaitingInitiation";
        public static string AwaitingApproval = "AwaitingApproval";
        public static string BillPaymentIdPlusItem = "BillPaymentId+Item";
        public static string BillPaymentId = "BillPaymentId";
        public const string FirsWht = "firswht";
        public const string Autopay = "autopay";
        public const string Sms = "sms";
        public const string BillPayment = "billpayment";
        public const string ValidateBillerTransactions = "/qbtrans/api/v1/payments/bills/validate"; 
    }
}
