using FileUploadAndValidation.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using TinyCsvParser.Mapping;

namespace FileUploadAndValidation.ObjectMappers
{

    public class FirsWhtTxtCsvMapper : CsvMapping<FirsWhtTransferModel>
    {
        public FirsWhtTxtCsvMapper() : base()
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

}
