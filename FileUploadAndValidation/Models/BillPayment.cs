using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class BillPayment
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }

        public string BatchId { get; set; }

        public int RowNumber { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }
    }

    public class FailedBillPayment
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public string Amount { get; set; }

        public string BatchId { get; set; }

        public int RowNumber { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }
    }

    public class Firs
    {
        public string BatchId { get; set; }

        public int RowNumber { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }
    }

    public class FirsWht
    {
        public int Row { get; set; }

        public string BeneficiaryTin { get; set; }

        public string BeneficiaryName { get; set; }

        public string BeneficiaryAddress { get; set; }

        public string ContractDate { get; set; }

        public decimal ContractAmount { get; set; }

        public string InvoiceNumber { get; set; }

        public string ContractType { get; set; }

        public string PeriodCovered { get; set; }

        public decimal WhtRate { get; set; }

        public decimal WhtAmount { get; set; }
    }

    public class FirsWVat
    {
        public int Row { get; set; }

        public string ContractorName { get; set; }

        public string ContractorAddress { get; set; }

        public string ContractorTin { get; set; }

        public string ContractDescription { get; set; }

        public string TransactionDate { get; set; }

        public string NatureOfTransaction { get; set; }

        public string InvoiceNumber { get; set; }

        public string TransactionCurrency { get; set; }

        public decimal CurrencyInvoicedValue { get; set; }

        public decimal TransactionInvoicedValue { get; set; }

        public decimal CurrencyExchangeRate { get; set; }

        public string TaxAccountNumber { get; set; }

        public decimal WvatRate { get; set; }

        public decimal WvatValue { get; set; }
    }

    public class FailedFirs
    {
        public string BatchId { get; set; }

        public int RowNumber { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }
    }

    public class FailedFirsWht : FailedFirs
    {
        public string BeneficiaryTin { get; set; }

        public string BeneficiaryName { get; set; }

        public string BeneficiaryAddress { get; set; }

        public string ContractDate { get; set; }

        public string ContractAmount { get; set; }

        public string InvoiceNumber { get; set; }

        public string ContractType { get; set; }

        public string PeriodCovered { get; set; }

        public string WhtRate { get; set; }

        public string WhtAmount { get; set; }
    }

    public class FailedFirsWVat : FailedFirs
    {
        public string ContractorName { get; set; }

        public string ContractorAddress { get; set; }

        public string ContractorTin { get; set; }

        public string ContractDescription { get; set; }

        public string TransactionDate { get; set; }

        public string NatureOfTransaction { get; set; }

        public string InvoiceNumber { get; set; }

        public string TransactionCurrency { get; set; }

        public string CurrencyInvoicedValue { get; set; }

        public string TransactionInvoicedValue { get; set; }

        public string CurrencyExchangeRate { get; set; }

        public string TaxAccountNumber { get; set; }

        public string WvatRate { get; set; }

        public string WvatValue { get; set; }
    }
}
