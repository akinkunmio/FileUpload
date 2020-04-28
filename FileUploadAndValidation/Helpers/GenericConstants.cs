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
        public const string WHT = "WHT";
        public const string WVAT = "WVAT";
        public const string FirsPayee = "PAYEE";
        public const string Autopay = "autopay";
        public const string Sms = "sms";
        public const string BillPayment = "billpayment";
        public const string Firs = "firs";
        public const string ValidateBillerTransactions = "/qbtrans/api/v1/payments/bills/validate";
        public const int RECORDS_SMALL_SIZE = 50;
        public static string BillPaymentTxtTemplate = "BillPaymentTxtTemplate.txt";
        public static string BillPaymentCsvTemplate = "BillPaymentCsvTemplate.csv";
        public static string BillPaymentXlsTemplate = "BillPaymentXlsTemplate.xls";
        public static string BillPaymentXlsxTemplate = "BillPaymentXlsxTemplate.xlsx";
        public static string BillPaymentTemplate = "BillPaymentTemplate";
        public static string ValidationResultFile = "ValidationResult.csv";

    }
}
