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
                new ColumnContract{ ColumnName="ProductCode", DataType="string", Max=256, Required=true },
                new ColumnContract{ ColumnName="ItemCode", DataType="string", Max=256 },
                new ColumnContract{ ColumnName="CustomerId", DataType="string", Max=15, Required=true },
                new ColumnContract{ ColumnName="Amount", DataType="integer", Max=100, Required=true },
            };
        }

        public static ColumnContract[] FirsWvat()
        {
            return new[]
           {
                new ColumnContract{ ColumnName="ContractorName", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractorAddress", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractorTIN", DataType="string", Max=15, Required=true },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=true },
                new ColumnContract{ ColumnName="NatureOfTransaction", DataType="string", Max=100, Required=true },
                new ColumnContract{ ColumnName="TransactionDate", DataType="datetime", Required=true },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Max=25, Required=false },
                new ColumnContract{ ColumnName="TransactionCurrency", DataType="string", Max=6, Required=true },
                new ColumnContract{ ColumnName="CurrencyInvoicedValue", DataType="decimal", Max=15, Required=true },
                new ColumnContract{ ColumnName="CurrencyExchangeRate", DataType="decimal", Max=4, Required=true },
                new ColumnContract{ ColumnName="TransactionInvoicedValue", DataType="decimal", Max=15, Required=true },
                new ColumnContract{ ColumnName="WVATRate", DataType="decimal", Max=4, Required=true },
                new ColumnContract{ ColumnName="WVATValue", DataType="decimal", Max=12, Required=true },
                new ColumnContract{ ColumnName="TaxAccountNumber", DataType="string", Max=20, Required=true }
            };
        }

        public static ColumnContract[] FirsWht()
        {
            return new[]
            {
                new ColumnContract{ ColumnName="BeneficiaryTin", DataType="string", Required=true, Min = 13 },
                new ColumnContract{ ColumnName="BeneficiaryName", DataType="string", Required=true },
                new ColumnContract{ ColumnName="BeneficiaryAddress", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractDate", DataType="datetime", Required=true },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractAmount", DataType="decimal", Required=true },
                new ColumnContract{ ColumnName="ContractType", DataType="string", Required=true },
                new ColumnContract{ ColumnName="PeriodCovered", DataType="string", Required=true },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Required=false },
                new ColumnContract{ ColumnName="WhtRate", DataType="decimal", Required=true },
                new ColumnContract{ ColumnName="WhtAmount", DataType="decimal", Required=true },

            };
        }

        public static ColumnContract[] FirsMultiTaxWht()
        {
            return new[]
            {
                new ColumnContract{ ColumnName="BeneficiaryTin", DataType="string", Required=true, Min = 13 },
                new ColumnContract{ ColumnName="BeneficiaryName", DataType="string", Required=true },
                new ColumnContract{ ColumnName="BeneficiaryAddress", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractDate", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractAmount", DataType="decimal", Required=true },
                new ColumnContract{ ColumnName="ContractType", DataType="string", Required=true },
                new ColumnContract{ ColumnName="PeriodCovered", DataType="string", Required=true },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Required=false },
                new ColumnContract{ ColumnName="WhtRate", DataType="decimal", Required=true },
                new ColumnContract{ ColumnName="WhtAmount", DataType="decimal", Required=true },
                new ColumnContract{ ColumnName="ContractorName", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractorAddress", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractorTIN", DataType="string", Max=15, Required=false },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=false },
                new ColumnContract{ ColumnName="NatureOfTransaction", DataType="string", Max=100, Required=false },
                new ColumnContract{ ColumnName="TransactionDate", DataType="datetime", Required=false },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Max=25, Required=false },
                new ColumnContract{ ColumnName="TransactionCurrency", DataType="string", Max=6, Required=false },
                new ColumnContract{ ColumnName="CurrencyInvoicedValue", DataType="decimal", Max=15, Required=false },
                new ColumnContract{ ColumnName="CurrencyExchangeRate", DataType="decimal", Max=4, Required=false },
                new ColumnContract{ ColumnName="TransactionInvoicedValue", DataType="decimal", Max=15, Required=false },
                new ColumnContract{ ColumnName="WVATRate", DataType="decimal", Max=4, Required=false },
                new ColumnContract{ ColumnName="WVATValue", DataType="decimal", Max=12, Required=false },
                new ColumnContract{ ColumnName="TaxAccountNumber", DataType="string", Max=20, Required=false },
                new ColumnContract{ ColumnName="Amount ", DataType="decimal", Max=30, Required=false },
                new ColumnContract{ ColumnName="Comment", DataType="string", Max=256, Required=false },
                new ColumnContract{ ColumnName="DocumentNumber", DataType="string", Max=30, Required=false },
                new ColumnContract{ ColumnName="TaxType", DataType="string", Max=100, Required=true },
            };
        }

        public static ColumnContract[] FirsMultiTaxWvat()
        {
            return new[]
            {
                new ColumnContract{ ColumnName="BeneficiaryTin", DataType="string", Required=false, Min = 13 },
                new ColumnContract{ ColumnName="BeneficiaryName", DataType="string", Required=false },
                new ColumnContract{ ColumnName="BeneficiaryAddress", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractDate", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractAmount", DataType="decimal", Required=false },
                new ColumnContract{ ColumnName="ContractType", DataType="string", Required=false },
                new ColumnContract{ ColumnName="PeriodCovered", DataType="string", Required=false },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Required=false },
                new ColumnContract{ ColumnName="WhtRate", DataType="decimal", Required=false },
                new ColumnContract{ ColumnName="WhtAmount", DataType="decimal", Required=false },
                new ColumnContract{ ColumnName="ContractorName", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractorAddress", DataType="string", Required=true },
                new ColumnContract{ ColumnName="ContractorTIN", DataType="string", Max=15, Required=true },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=true },
                new ColumnContract{ ColumnName="NatureOfTransaction", DataType="string", Max=100, Required=true },
                new ColumnContract{ ColumnName="TransactionDate", DataType="datetime", Required=true },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Max=25, Required=true },
                new ColumnContract{ ColumnName="TransactionCurrency", DataType="string", Max=6, Required=true },
                new ColumnContract{ ColumnName="CurrencyInvoicedValue", DataType="decimal", Max=15, Required=true },
                new ColumnContract{ ColumnName="CurrencyExchangeRate", DataType="decimal", Max=4, Required=true },
                new ColumnContract{ ColumnName="TransactionInvoicedValue", DataType="decimal", Max=15, Required=true },
                new ColumnContract{ ColumnName="WVATRate", DataType="decimal", Max=4, Required=true },
                new ColumnContract{ ColumnName="WVATValue", DataType="decimal", Max=12, Required=true },
                new ColumnContract{ ColumnName="TaxAccountNumber", DataType="string", Max=20, Required=true },
                new ColumnContract{ ColumnName="Amount ", DataType="decimal", Max=30, Required=false },
                new ColumnContract{ ColumnName="Comment", DataType="string", Max=256, Required=false },
                new ColumnContract{ ColumnName="DocumentNumber", DataType="string", Max=30, Required=false },
                new ColumnContract{ ColumnName="TaxType", DataType="string", Max=100, Required=true },
            };
        }

        public static ColumnContract[] FirsMultiTaxOther()
        {
            return new[]
            {
                new ColumnContract{ ColumnName="BeneficiaryTin", DataType="string", Required=false, Min = 13 },
                new ColumnContract{ ColumnName="BeneficiaryName", DataType="string", Required=false },
                new ColumnContract{ ColumnName="BeneficiaryAddress", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractDate", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractAmount", DataType="decimal", Required=false },
                new ColumnContract{ ColumnName="ContractType", DataType="string", Required=false },
                new ColumnContract{ ColumnName="PeriodCovered", DataType="string", Required=false },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Required=false },
                new ColumnContract{ ColumnName="WhtRate", DataType="decimal", Required=false },
                new ColumnContract{ ColumnName="WhtAmount", DataType="decimal", Required=false },
                new ColumnContract{ ColumnName="ContractorName", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractorAddress", DataType="string", Required=false },
                new ColumnContract{ ColumnName="ContractorTIN", DataType="string", Max=15, Required=false },
                new ColumnContract{ ColumnName="ContractDescription", DataType="string", Required=false },
                new ColumnContract{ ColumnName="NatureOfTransaction", DataType="string", Max=100, Required=false },
                new ColumnContract{ ColumnName="TransactionDate", DataType="datetime", Required=false },
                new ColumnContract{ ColumnName="InvoiceNumber", DataType="string", Max=25, Required=false },
                new ColumnContract{ ColumnName="TransactionCurrency", DataType="string", Max=6, Required=false },
                new ColumnContract{ ColumnName="CurrencyInvoicedValue", DataType="decimal", Max=15, Required=false },
                new ColumnContract{ ColumnName="CurrencyExchangeRate", DataType="decimal", Max=4, Required=false },
                new ColumnContract{ ColumnName="TransactionInvoicedValue", DataType="decimal", Max=15, Required=false },
                new ColumnContract{ ColumnName="WVATRate", DataType="decimal", Max=4, Required=false },
                new ColumnContract{ ColumnName="WVATValue", DataType="decimal", Max=12, Required=false },
                new ColumnContract{ ColumnName="TaxAccountNumber", DataType="string", Max=20, Required=false },
                new ColumnContract{ ColumnName="Amount ", DataType="decimal", Max=30, Required=true },
                new ColumnContract{ ColumnName="Comment", DataType="string", Max=256, Required=false },
                new ColumnContract{ ColumnName="DocumentNumber", DataType="string", Max=30, Required=false },
                new ColumnContract{ ColumnName="TaxType", DataType="string", Max=100, Required=true },
            };
        }
    }
}
