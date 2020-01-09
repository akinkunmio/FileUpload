using ExcelMapper;
using FileUploadAndValidation.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.ObjectMappers
{

    public class FirsWhtXlsMapper : ExcelClassMap<FirsWhtTransferModel>
    {
        public FirsWhtXlsMapper()
        {
            Map(firsModel => firsModel.ContractDescription)
                .MakeOptional();

            Map(firsModel => firsModel.ContractorAddress)
                .MakeOptional();

            Map(firsModel => firsModel.ContractorName)
                .MakeOptional();

            Map(firsModel => firsModel.ContractorTIN)
                .MakeOptional();

            Map(firsModel => firsModel.ExchangeRateToNaira)
                .MakeOptional();

            Map(firsModel => firsModel.InvoicedValue)
                .MakeOptional();

            Map(firsModel => firsModel.InvoiceValueofTransaction)
                .MakeOptional();

            Map(firsModel => firsModel.TransactionDate)
                .MakeOptional();

            Map(firsModel => firsModel.TransactionNature)
               .MakeOptional();

            Map(firsModel => firsModel.TransactionInvoiceRefNo)
                .MakeOptional();

            Map(firsModel => firsModel.WVATRate)
                .MakeOptional();

            Map(firsModel => firsModel.WVATValue)
                .MakeOptional();
        }
    }

}
