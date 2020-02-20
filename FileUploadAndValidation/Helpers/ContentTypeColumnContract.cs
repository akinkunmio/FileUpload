using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Helpers
{
    public static class ContentTypeColumnContract
    {
        public static ColumnContract[] BillerPaymentIdWithItem()
        {
            return new[]
           {
                new ColumnContract{ ColumnName="product_code", DataType="string", Max=256, Required = true },
                new ColumnContract{ ColumnName="item_code", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="customer_id", DataType="string", Max=15, Required=true },
                new ColumnContract{ ColumnName="amount", DataType="integer", Max=100, Required=true },
            };
        }

        public static ColumnContract[] BillerPaymentId()
        {
            return new[]
           {
                new ColumnContract{ ColumnName="product_code", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="item_code", DataType="string", Max=256 },
                new ColumnContract{ ColumnName="customer_id", DataType="string", Max=15, Required=true },
                new ColumnContract{ ColumnName="amount", DataType="integer", Max=100, Required=true },
            };
        }

        public static ColumnContract[] FirsWht()
        {
            return new[]
           {
                new ColumnContract{ ColumnName="ContractorName", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="ContractorAddress", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="ContractorTIN", DataType="string", Max=15, Required=true },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Max=100, Required=true },
                new ColumnContract{ ColumnName="TransactionNature", DataType="string", Max=100, Required=true },
                new ColumnContract{ ColumnName="TransactionDate", DataType="datetime", Required=true },
                new ColumnContract{ ColumnName="TransactionInvoiceRefNo", DataType="string", Max=25, Required=true },
                new ColumnContract{ ColumnName="CurrencyOfTransaction", DataType="string", Max=6, Required=true },
                new ColumnContract{ ColumnName="InvoicedValue", DataType="decimal", Max=15, Required=true },
                new ColumnContract{ ColumnName="ExchangeRateToNaira", DataType="decimal", Max=4, Required=true },
                new ColumnContract{ ColumnName="InvoiceValueofTransaction", DataType="decimal", Max=15, Required=true },
                new ColumnContract{ ColumnName="WVATRate", DataType="decimal", Max=4, Required=true },
                new ColumnContract{ ColumnName="WVATValue", DataType="decimal", Max=12, Required=true },
                new ColumnContract{ ColumnName="TaxAccountNumber", DataType="decimal", Max=20, Required=true }
            };
        }
    }
}
