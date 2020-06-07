using FileUploadAndValidation.Helpers;
using FilleUploadCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static ResponseResult CreateResponseResult(UploadResult uploadResult, string contentType, string itemType)
        {
            return new ResponseResult
            {
                BatchId = uploadResult.BatchId,
                ValidRows = uploadResult.ValidRows
                                        .Select(row => GenericHelpers.RowMarshaller(row, contentType, itemType))
                                        .ToList(),
                Failures = uploadResult.Failures.Select(a => new FailedValidation
                {
                    ColumnValidationErrors = a.ColumnValidationErrors,
                    Row = GenericHelpers.RowMarshaller(a.Row, contentType, itemType)
                }).ToList(),
                ErrorMessage = uploadResult.ErrorMessage,
                FileName = uploadResult.FileName,
                RowsCount = uploadResult.RowsCount
            };
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

public class FirsWhtRowDetail : RowDetail 
{
    
}
public class FirsRowDetail : RowDetail 
{

}
    public class RowDetail
    {
        public string PayerTin { get; set; }

        public int RowNum { get; set; }

        public string RowStatus { get; set; }

        public string CreatedDate { get; set; }

        public string ErrorDescription { get; set; }

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

        public string Comment { get; set; }

        public string Desc { get; set; }

        public string CustomerName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string AddressInfo { get; set; }

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
