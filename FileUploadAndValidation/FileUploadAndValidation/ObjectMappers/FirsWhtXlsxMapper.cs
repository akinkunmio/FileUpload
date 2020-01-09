using EPPlus.Core.Extensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.ObjectMappers
{
    [ExcelWorksheet]
    public class FirsWhtXlsxMapper
    {
        [ExcelTableColumn(1)]
        public string ContractorName { get; set; }

        [ExcelTableColumn(2)]
        public string ContractorAddress { get; set; }

        [ExcelTableColumn(3)]
        public string ContractorTIN { get; set; }

        [ExcelTableColumn(4)]
        public string ContractDescription { get; set; }

        [ExcelTableColumn(5)]
        public string TransactionNature { get; set; }

        [ExcelTableColumn(6)]
        public DateTime? TransactionDate { get; set; }

        [ExcelTableColumn(7)]
        public string TransactionInvoiceRefNo { get; set; }
        [ExcelTableColumn(8)]
        public string CurrencyOfTransaction { get; set; }

        [ExcelTableColumn(9)]
        public decimal? InvoicedValue { get; set; }

        [ExcelTableColumn(10)]
        public decimal? ExchangeRate { get; set; }

        [ExcelTableColumn(11)]
        public decimal? InvoiceValueofTransaction { get; set; }

        [ExcelTableColumn(12)]
        public decimal? WVATRate { get; set; }

        [ExcelTableColumn(13)]
        public decimal? WVATValue { get; set; }

        [ExcelTableColumn(14)]
        public ulong? TaxAccountNumber { get; set; }
    }
}
