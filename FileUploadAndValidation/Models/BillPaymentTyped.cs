using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class BillPaymentTyped
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public decimal Amount { get; set; }

        public int Row { get; set; }

    }

    public class BillPaymentUntyped
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public string Amount { get; set; }

        public int RowNumber { get; set; }
    }

    public class FirsWhtTyped
    {
        public int Row { get; set; }

        public string BeneficiaryTin { get; set; }

        public string BeneficiaryName { get; set; }

        public string BeneficiaryAddress { get; set; }

        public string ContractDate { get; set; }

        public decimal ContractAmount { get; set; }

        public string ContractDescription { get; set; }

        public string InvoiceNumber { get; set; }

        public string ContractType { get; set; }

        public string PeriodCovered { get; set; }

        public decimal WhtRate { get; set; }

        public decimal WhtAmount { get; set; }
    }

    public class FirsWhtUntyped
    {
        public int Row { get; set; }

        public string BeneficiaryTin { get; set; }

        public string BeneficiaryName { get; set; }

        public string BeneficiaryAddress { get; set; }

        public string ContractDate { get; set; }

        public string ContractAmount { get; set; }

        public string ContractDescription { get; set; }

        public string InvoiceNumber { get; set; }

        public string ContractType { get; set; }

        public string PeriodCovered { get; set; }

        public string WhtRate { get; set; }

        public string WhtAmount { get; set; }
    }

    public class FirsWVatTyped
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

        public decimal WVATRate { get; set; }

        public decimal WVATValue { get; set; }
    }

    public class FirsWVatUntyped
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

        public string CurrencyInvoicedValue { get; set; }

        public string TransactionInvoicedValue { get; set; }

        public string CurrencyExchangeRate { get; set; }

        public string TaxAccountNumber { get; set; }

        public string WvatRate { get; set; }

        public string WvatValue { get; set; }
    }

}
