using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.DTOs
{
    public class FirsWhtTransferModel
    {

        public string ContractorName { get; set; }

        public string ContractorAddress { get; set; }

        public string ContractorTIN { get; set; }

        public string ContractDescription { get; set; }

        public string TransactionNature { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string TransactionInvoiceRefNo { get; set; }

        public string CurrencyOfTransaction { get; set; }

        public decimal? InvoicedValue { get; set; }

        public decimal? ExchangeRateToNaira { get; set; }

        public decimal? InvoiceValueofTransaction { get; set; }

        public decimal? WVATRate { get; set; }

        public decimal? WVATValue { get; set; }

        public ulong? TaxAccountNumber { get; set; }
    }
}
