using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class TypedRowDetail
    {
        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }

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

        public string ContractorName { get; set; }

        public string ContractorAddress { get; set; }

        public string ContractorTin { get; set; }

        public string ContractDescription { get; set; }

        public DateTime TransactionDate { get; set; }

        public string NatureOfTransaction { get; set; }

        public string TransactionCurrency { get; set; }

        public decimal CurrencyInvoicedValue { get; set; }

        public decimal TransactionInvoicedValue { get; set; }

        public decimal CurrencyExchangeRate { get; set; }

        public string TaxAccountNumber { get; set; }

        public decimal WvatRate { get; set; }

        public decimal WvatValue { get; set; }
    }

    public class BillPaymentRowStatusUntyped
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public string Amount { get; set; }

        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }
    }

    public class BillPaymentRowStatusTyped 
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }

        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }
    }

    public class FirsWhtRowStatusUntyped 
    {
        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

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

    public class FirsWhtRowStatusTyped
    {
        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

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

    public class FirsWVatRowStatusTyped
    {
        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

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

    public class FirsWVatRowStatusUntyped
    {
        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }

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

    //public class BillPaymentRowStatusDto
    //{
    //    public string ProductCode { get; set; }

    //    public string ItemCode { get; set; }

    //    public string CustomerId { get; set; }

    //    public double Amount { get; set; }

    //    public string Error { get; set; }

    //    public string RowStatus { get; set; }

    //    public int RowNum { get; set; }
    //}


    public class RowStatusDtoObject
    {
        public IEnumerable<RowDetail> RowStatusDto { get; set; }

        public int TotalRowsCount { get; set; }

        public double ValidAmountSum { get; set; }

        public int ValidRowCount { get; set; }

        public int InvalidRowCount { get; set; }
    }

    public class PagedData<T>
    {
        public IEnumerable<T> Data { get; set; }

        public int TotalRowsCount { get; set; }

        public double TotalAmountSum { get; set; }

        public bool Valid { get; set; }

        public bool Invalid { get; set; }

        public string ContentType { get; set; }

        public string ItemType { get; set; }
    }
}
