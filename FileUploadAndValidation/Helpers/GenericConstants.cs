using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Helpers
{
    public static class GenericConstants
    {
        public const string PendingValidation = "Pending Validation";
        public const string AwaitingInitiation = "Awaiting Initiation";
        public const string AwaitingApproval = "Awaiting Approval";
        public const string NoValidRecord = "No Valid Record";

        public const string BillPaymentIdPlusItem = "billpaymentid+item";
        public const string BillPaymentId = "billpaymentid";
        public const string FirsWht = "firswht";
        public const string FirsWvat = "firswvat";
        public const string FirsPayee = "firspayee";
        public const string Payee = "payee";
        public const string Wht = "wht";
        public const string Wvat = "wvat";
        public const string Cit = "cit";
        public const string Edt = "edt";
        public const string PreOpLevy = "pre-op levy";
        public const string Vat = "vat";


        public const string Autopay = "autopay";
        public const string Sms = "sms";
        public const string BillPayment = "billpayment";
        public const string MultiTax = "multitax";
       
        public const string Firs = "firs";
        public const string Lirs = "lirs";
        public const string FctIrs = "fctirs";



        public const string ValidateBillPaymentUrl = "/qbtrans/api/v1/payments/bills/validate";
        public const string ValidateFirsUrl = "/qbtrans/api/v1/payments/firs/validate";
        public const string ValidateMultitaxUrl = "/qbtrans/api/v1/payments/hub/firs/validate";
        public const string InitiateBillPaymentUrl = "/qbtrans/api/v1/payments/bills/initiate-payment";
        public const string InitiateFirsPaymentUrl = "/qbtrans/api/v1/payments/firs/initiate-payment";
        public const string InitiateFirsMultitaxPaymentUrl = "/qbtrans/api/v1/payments/hub/firs/initiate-payment";

        public const int RECORDS_SMALL_SIZE = 50;

        public static string BillPaymentTxtTemplate = "BillPaymentTxtTemplate.txt";
        public static string BillPaymentCsvTemplate = "BillPaymentCsvTemplate.csv";
        public static string BillPaymentXlsTemplate = "BillPaymentXlsTemplate.xls";
        public static string BillPaymentXlsxTemplate = "BillPaymentXlsxTemplate.xlsx";
        public static string BillPaymentTemplate = "BillPaymentTemplate";
        public static string ValidationResultFile = "ValidationResult.csv";
        public static string FirsWhtCsvTemplate = "FirsWhtCsvTemplate.csv";
        public static string FirsWvatCsvTemplate = "FirsWVATCsvTemplate.csv";
    }
}
