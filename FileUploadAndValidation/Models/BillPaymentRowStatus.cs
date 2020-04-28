using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class RowStatus
    {
        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }
    }

    public class BillPaymentRowStatus : RowStatus
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }
    }

    public class FirsWhtRowStatus : RowStatus
    {
        public string BeneficiaryTin { get; set; }

        public string BeneficiaryName { get; set; }

        public string BeneficiaryAddress { get; set; }

        public DateTime ContractDate { get; set; }

        public decimal ContractAmount { get; set; }

        public string InvoiceNumber { get; set; }

        public string ContractType { get; set; }

        public string PeriodCovered { get; set; }

        public decimal WhtRate { get; set; }

        public decimal WhtAmount { get; set; }
    }

    public class FirsWVatRowStatus : RowStatus
    {
        public string ContractorName { get; set; }

        public string ContractorAddress { get; set; }

        public string ContractorTin { get; set; }

        public string ContractDescription { get; set; }

        public DateTime TransactionDate { get; set; }

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

    public class BillPaymentRowStatusDto
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }

        public string Error { get; set; }

        public string RowStatus { get; set; }

        public int RowNum { get; set; }
    }


    public class RowStatusDtoObject<T>
    {
        public IEnumerable<T> RowStatusDtos { get; set; }

        public int TotalRowsCount { get; set; }

        public double ValidAmountSum { get; set; }
    }

    public class PagedData<T>
    {
        public IEnumerable<T> Data { get; set; }

        public int TotalRowsCount { get; set; }

        public double TotalAmountSum { get; set; }
    }
}
