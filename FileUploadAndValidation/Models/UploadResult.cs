using FilleUploadCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class UploadResult
    {
        public UploadResult()
        {
            ValidRows = new List<RowDetail>();

            Failures = new List<Failure>();
        }

        public string ErrorMessage { get; set; }

        public string BatchId { get; set; }

        public IList<RowDetail> ValidRows { get; set; }

        public IList<Failure> Failures { get; set; }

        public int RowsCount { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string FileName { get; set; }
    }

    public class ResponseResult
    {
        public string ErrorMessage { get; set; }

        public string BatchId { get; set; }

        public List<dynamic> ValidRows { get; set; }

        public List<FailedValidation> Failures { get; set; }

        public int RowsCount { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string FileName { get; set; }

        public class FailedValidation
        {
            public dynamic Row { get; set; }

            public IList<ValidationError> ColumnValidationErrors { get; set; }
        }
    }

    public class MultiTaxResponseResult
    {
        public string ErrorMessage { get; set; }

        public string BatchId { get; set; }

        public List<dynamic> ValidRows { get; set; }

        public List<FailedValidation> Failures { get; set; }

        public int RowsCount { get; set; }

        public string FileName { get; set; }

        public class FailedValidation
        {
            public dynamic Row { get; set; }

            public IList<ValidationError> ColumnValidationErrors { get; set; }
        }
    }

    public class Failure
    {
        public RowDetail Row { get; set; }

        public IList<ValidationError> ColumnValidationErrors { get; set; }
    }

    public class RowDetail
    {
        public int RowNum { get; set; }

        public string RowStatus { get; set; }

        public string CreatedDate { get; set; }

        public string Error { get; set; }

        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public string Amount { get; set; }

        public string BeneficiaryTin { get; set; }

        public string BeneficiaryName { get; set; }

        public string BeneficiaryAddress { get; set; }

        public string ContractDate { get; set; }
       
        public string ContractDescription { get; set; }

        public string ContractAmount { get; set; }

        public string InvoiceNumber { get; set; }

        public string ContractType { get; set; }

        public string PeriodCovered { get; set; }

        public string WhtRate { get; set; }

        public string WhtAmount { get; set; }

        public string ContractorName { get; set; }

        public string ContractorAddress { get; set; }

        public string ContractorTin { get; set; }

        public string TransactionDate { get; set; }

        public string NatureOfTransaction { get; set; }

        public string TransactionCurrency { get; set; }

        public string CurrencyInvoicedValue { get; set; }

        public string TransactionInvoicedValue { get; set; }

        public string CurrencyExchangeRate { get; set; }

        public string TaxAccountNumber { get; set; }

        public string WvatRate { get; set; }

        public string WvatValue { get; set; }

        public string TaxType { get; set; }

        public string DocumentNumber { get; set; }

        public string Comment { get; internal set; }
    }

    //public class RowdDetailUtyped
    //{
    //    public int RowNumber { get; set; }

    //    public string RowStatus { get; set; }

    //    public string CreatedDate { get; set; }

    //    public string Error { get; set; }

    //    public string ProductCode { get; set; }

    //    public string ItemCode { get; set; }

    //    public string CustomerId { get; set; }

    //    public string Amount { get; set; }

    //    public string BeneficiaryTin { get; set; }

    //    public string BeneficiaryName { get; set; }

    //    public string BeneficiaryAddress { get; set; }

    //    public string ContractDate { get; set; }

    //    public string ContractAmount { get; set; }

    //    public string InvoiceNumber { get; set; }

    //    public string ContractType { get; set; }

    //    public string PeriodCovered { get; set; }

    //    public string WhtRate { get; set; }

    //    public string WhtAmount { get; set; }

    //    public string ContractorName { get; set; }

    //    public string ContractorAddress { get; set; }

    //    public string ContractorTin { get; set; }

    //    public string ContractDescription { get; set; }

    //    public string TransactionDate { get; set; }

    //    public string NatureOfTransaction { get; set; }

    //    public string TransactionCurrency { get; set; }

    //    public string CurrencyInvoicedValue { get; set; }

    //    public string TransactionInvoicedValue { get; set; }

    //    public string CurrencyExchangeRate { get; set; }

    //    public string TaxAccountNumber { get; set; }

    //    public string WvatRate { get; set; }

    //    public string WvatValue { get; set; }

    //}
}
