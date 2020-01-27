using System;
using System.Collections.Generic;
using System.Text;
using TinyCsvParser.Mapping;

namespace FileUploadAndValidation.FileReaderImpl.CsvTxtMappers
{
    public class FirsWhtMapper : CsvMapping<FirsWhtsModel>
    {
        public FirsWhtMapper() : base()
        {
            MapProperty(0, x => x.ContractorName);
            MapProperty(1, x => x.ContractorAddress);
            MapProperty(2, x => x.ContractorTIN);
            MapProperty(3, x => x.ContractDescription);
            MapProperty(4, x => x.TransactionNature);
            MapProperty(5, x => x.TransactionDate);
            MapProperty(6, x => x.TransactionInvoiceRefNo);
            MapProperty(7, x => x.CurrencyOfTransaction);
            MapProperty(8, x => x.InvoicedValue);
            MapProperty(9, x => x.ExchangeRateToNaira);
            MapProperty(10, x => x.InvoiceValueofTransaction);
            MapProperty(11, x => x.WVATRate);
            MapProperty(12, x => x.WVATValue);
            MapProperty(13, x => x.TaxAccountNumber);
        }
    }

    public class FirsWhtsModel
    {
        public string ContractorName { get;  set; }
        public string ContractorAddress { get;  set; }
        public string ContractorTIN { get;  set; }
        public string ContractDescription { get;  set; }
        public string TransactionNature { get;  set; }
        public string TransactionDate { get;  set; }
        public string TransactionInvoiceRefNo { get;  set; }
        public string CurrencyOfTransaction { get;  set; }
        public string InvoicedValue { get;  set; }
        public string ExchangeRateToNaira { get;  set; }
        public string InvoiceValueofTransaction { get;  set; }
        public string WVATRate { get;  set; }
        public string WVATValue { get;  set; }
        public string TaxAccountNumber { get;  set; }
    }
}
