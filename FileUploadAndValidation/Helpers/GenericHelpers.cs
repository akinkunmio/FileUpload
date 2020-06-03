using FileUploadAndValidation.Models;
using FilleUploadCore.Exceptions;
using FilleUploadCore.FileReaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileUploadAndValidation.Helpers
{
    public static class GenericHelpers
    {
        private static bool isValid;

        public static void ValidateHeaderRow(Row headerRow, ColumnContract[] columnContracts)
        {
            if (headerRow == null)
                throw new ValidationException("Header row not found");

            var expectedNumOfColumns = columnContracts.Count();
            if (headerRow.Columns.Count() != expectedNumOfColumns)
                throw new ValidationException($"Invalid file uploaded!.");

            for (int i = 0; i < expectedNumOfColumns; i++)
            {
                var columnName = columnContracts[i].ColumnName;
                var headerRowColumn = headerRow.Columns[i].Value.ToString().Trim();
                if (!headerRowColumn.ToLower().Contains(columnName.ToLower()))
                    throw new ValidationException($"Invalid header column name. Expected: {columnName}, Found: {headerRowColumn}");
            }
        }

        public static dynamic RowMarshaller(RowDetail r, string contentType, string itemType)
        {
            dynamic result = default;

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.Wht))
                result = new FirsWhtUntyped
                {
                    Row = r.RowNum,
                    BeneficiaryAddress = r.BeneficiaryAddress,
                    BeneficiaryName = r.BeneficiaryName,
                    BeneficiaryTin = r.BeneficiaryTin,
                    ContractAmount = r.ContractAmount,
                    ContractDate = r.ContractDate,
                    ContractDescription = r.ContractDescription,
                    ContractType = r.ContractType,
                    InvoiceNumber = r.InvoiceNumber,
                    PeriodCovered = r.PeriodCovered,
                    WhtAmount = r.WhtAmount,
                    WhtRate = r.WhtRate
                };

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.Wvat))
                result = new FirsWVatUntyped
                {
                    Row = r.RowNum,
                    ContractorAddress = r.ContractorAddress,
                    ContractorName = r.ContractorName,
                    ContractorTin = r.ContractorTin,
                    CurrencyExchangeRate = r.CurrencyExchangeRate,
                    CurrencyInvoicedValue = r.CurrencyInvoicedValue,
                    ContractDescription = r.ContractDescription,
                    NatureOfTransaction = r.NatureOfTransaction,
                    InvoiceNumber = r.InvoiceNumber,
                    TaxAccountNumber = r.TaxAccountNumber,
                    TransactionCurrency = r.TransactionCurrency,
                    TransactionDate = r.TransactionDate,
                    TransactionInvoicedValue = r.TransactionInvoicedValue,
                    WvatRate = r.WvatRate,
                    WvatValue = r.WvatValue
                };

            if (contentType.ToLower().Equals(GenericConstants.BillPayment)
                && (itemType.ToLower().Equals(GenericConstants.BillPaymentId)
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem)))
                result = new BillPaymentUntyped
                {
                    RowNumber = r.RowNum,
                    Amount = r.Amount,
                    CustomerId = r.CustomerId,
                    ItemCode = r.ItemCode,
                    ProductCode = r.ProductCode
                };

            if (contentType.ToLower().Equals(GenericConstants.Firs)
                && itemType.ToLower().Equals(GenericConstants.MultiTax))
                result = new
                {
                    RowNumber = r.RowNum,
                    r.BeneficiaryTin,
                    r.BeneficiaryName,
                    r.BeneficiaryAddress,
                    r.ContractDate,
                    r.ContractDescription,
                    r.ContractAmount,
                    r.ContractType,
                    r.PeriodCovered,
                    r.InvoiceNumber,
                    r.WhtRate,
                    r.WhtAmount,
                    r.Amount,
                    r.Comment,
                    r.DocumentNumber,
                    r.PayerTin,
                    r.TaxType
                };

                return result;
        }

        public static string ConstructValidationError(Failure failure)
        {
            var result = new StringBuilder();
            for (int i = 0; i < failure.ColumnValidationErrors.Count(); i++)
            {
                result.Append($"{failure.ColumnValidationErrors[i].PropertyName}: {failure.ColumnValidationErrors[i].ErrorMessage}");

                if (failure.ColumnValidationErrors[i+1] != null)
                    result.Append(", ");
            }

            return result.ToString();
        }

        private static dynamic MapToNasValidateObject(string contentType, string itemType, RowDetail r)
        {
            dynamic result = default;

            if (contentType.ToLower().Equals(GenericConstants.BillPayment)
                && (itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem)
                || itemType.ToLower().Equals(GenericConstants.BillPaymentId)))
            {
                result = new 
                {
                    Amount = decimal.Parse(r.Amount),
                    r.CustomerId,
                    r.ItemCode,
                    r.ProductCode,
                    Row = r.RowNum
                };
            }

            if (itemType.ToLower().Equals(GenericConstants.Wht.ToLower())
                && contentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
            {
                result = new
                {
                    Row = r.RowNum,
                    r.BeneficiaryTin,
                    r.BeneficiaryName,
                    r.BeneficiaryAddress,
                    r.ContractDate,
                    ContractAmount = decimal.Parse(r.ContractAmount),
                    r.ContractDescription,
                    r.InvoiceNumber,
                    r.ContractType,
                    r.PeriodCovered,
                    WhtRate = decimal.Parse(r.WhtRate),
                    WhtAmount = decimal.Parse(r.WhtAmount),
                    BusinessTin = r.PayerTin
                };
            }

            if (itemType.ToLower().Equals(GenericConstants.Wvat.ToLower())
                && contentType.ToLower().Equals(GenericConstants.Firs.ToLower()))
            {
                result = new 
                {
                    Row = r.RowNum,
                    r.ContractorName,
                    r.ContractorAddress,
                    r.ContractorTin,
                    r.ContractDescription,
                    r.TransactionDate,
                    r.NatureOfTransaction,
                    r.InvoiceNumber,
                    r.TransactionCurrency,
                    CurrencyInvoicedValue = decimal.Parse(r.CurrencyInvoicedValue),
                    TransactionInvoicedValue = decimal.Parse(r.TransactionInvoicedValue),
                    CurrencyExchangeRate = decimal.Parse(r.CurrencyExchangeRate),
                    r.TaxAccountNumber,
                    WVATRate = decimal.Parse(r.WvatRate),
                    WVATValue = decimal.Parse(r.WvatValue),
                    BusinessTin = r.PayerTin
                };
            }

            if ((itemType.ToLower().Equals(GenericConstants.Cit)
                || itemType.ToLower().Equals(GenericConstants.Edt)
                || itemType.ToLower().Equals(GenericConstants.PreOpLevy)
                || itemType.ToLower().Equals(GenericConstants.Vat))
               && contentType.ToLower().Equals(GenericConstants.Firs))
            {
                result = new 
                {
                    Row = r.RowNum,
                    CustomerTin = r.PayerTin,
                    r.Amount,
                    r.Comment,
                    r.DocumentNumber,
                };
            }

            return result;
        }

        public static dynamic GetSaveToNasFileContent(string contentType, string itemType, IEnumerable<RowDetail> rowDetails)
        {

            if (itemType.ToLower().Equals(GenericConstants.MultiTax))
            {
                var result = new List<ValidateFileNasModel>();

                var whtRows = rowDetails
                    .Where(r => GenericConstants.Wht
                    .Equals(r.TaxType.ToLower()))
                    .Select(s => MapToNasValidateObject(contentType, GenericConstants.Wht, s));

                result.Add(new ValidateFileNasModel
                {
                    Authority = contentType,
                    TaxType = GenericConstants.Wht,
                    Taxes = whtRows
                });

                var citRows = rowDetails
                    .Where(r => GenericConstants.Cit
                    .Equals(r.TaxType.ToLower()))
                    .Select(s => MapToNasValidateObject(contentType, GenericConstants.Cit, s));

                result.Add(new ValidateFileNasModel
                {
                    Authority = contentType,
                    TaxType = GenericConstants.Cit,
                    Taxes = citRows
                });

                var edtRows = rowDetails
                   .Where(r => GenericConstants.Edt
                   .Equals(r.TaxType.ToLower()))
                   .Select(s => MapToNasValidateObject(contentType, GenericConstants.Edt, s));

                result.Add(new ValidateFileNasModel
                {
                    Authority = contentType,
                    TaxType = GenericConstants.Edt,
                    Taxes = edtRows
                });

                var preOpLevyRows = rowDetails
                   .Where(r => GenericConstants.PreOpLevy
                   .Equals(r.TaxType.ToLower()))
                   .Select(s => MapToNasValidateObject(contentType, GenericConstants.PreOpLevy, s));

                result.Add(new ValidateFileNasModel
                {
                    Authority = contentType,
                    TaxType = GenericConstants.PreOpLevy,
                    Taxes = preOpLevyRows
                });

                var vatRows = rowDetails
                  .Where(r => GenericConstants.Vat
                  .Equals(r.TaxType.ToLower()))
                  .Select(s => MapToNasValidateObject(contentType, GenericConstants.Vat, s));

                result.Add(new ValidateFileNasModel
                {
                    Authority = contentType,
                    TaxType = GenericConstants.Vat,
                    Taxes = vatRows
                });

                return result;
            }

            else if (itemType.ToLower().Equals(GenericConstants.Wht))
            {
                return rowDetails
                   .Select(s => MapToNasValidateObject(contentType, GenericConstants.Wht, s));
            }

            else if (itemType.ToLower().Equals(GenericConstants.Wvat))
            {
                return rowDetails
                   .Select(s => MapToNasValidateObject(contentType, GenericConstants.Wvat, s));
            }

            else if (itemType.ToLower().Equals(GenericConstants.BillPaymentId)
                || itemType.ToLower().Equals(GenericConstants.BillPaymentIdPlusItem))
            {
               return rowDetails
                   .Select(s => MapToNasValidateObject(contentType, GenericConstants.BillPaymentId, s));
            }

            return "";
        }

        public static ValidateRowResult ValidateRowCell(Row row, ColumnContract[] columnContracts)
        {
            var validationErrors = new List<ValidationError>();
            bool isValid = true;

            for (var i = 0; i < columnContracts.Length; i++)
            {

                var errorMessage = "";
                ColumnContract contract = columnContracts[i];
                Column column = row.Columns[i];

                try
                {
                    if(contract.ValidateCell != true)
                    {
                        continue;
                    }
                    if (contract.Required == true && string.IsNullOrWhiteSpace(column.Value))
                    {
                        errorMessage = "Value must be provided";
                    }
                    if (contract.Max != default && column.Value != null && contract.Max < column.Value.Length)
                    {
                        errorMessage = "Specified maximum length exceeded";
                    }
                    if (contract.Min != default && column.Value != null && column.Value.Length < contract.Min)
                    {
                        errorMessage = "Specified minimum length not met";
                    }
                    if (contract.DataType != default && column.Value != null)
                    {
                        if (!GenericHelpers.ColumnDataTypes().ContainsKey(contract.DataType))
                            errorMessage = "Specified data type is not supported";
                        try
                        {
                            dynamic typedValue = Convert.ChangeType(column.Value, GenericHelpers.ColumnDataTypes()[contract.DataType]);
                        }
                        catch (Exception)
                        {
                            errorMessage = "Invalid value for data type specified";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                        throw new ValidationException(
                            new ValidationError
                            {
                                ErrorMessage = errorMessage,
                                PropertyName = contract.ColumnName
                            },
                            errorMessage);
                }
                catch (ValidationException exception)
                {
                    isValid = false;
                    validationErrors.Add(exception.ValidationError);
                }
            }

            return new ValidateRowResult { ValidationErrors = validationErrors, Validity = isValid };
        }

        public static Dictionary<string, Type> ColumnDataTypes()
        {
            return new Dictionary<string, Type>() {
                { "string", typeof(string) },
                { "integer", typeof(int) },
                { "decimal", typeof(decimal) },
                { "boolean", typeof(bool) },
                { "datetime", typeof(DateTime) },
                { "character", typeof(char) },
                { "double", typeof(double) }
            };
        }

        public static string GenerateBatchId(string fileName, DateTime date)
        {
            return fileName + "_" + RandomString() + "_" + date.ToString("yyyyMMddHHmmssffff");
            //return "firs_multitax1_ZMWYAA_202005290823495638";
        }
        
        public static string GetFileNameFromBatchId(string batchId)
        {
            var array = batchId.Split('_');
            array = array.Take(array.Count() - 3).ToArray();
            return string.Join("", array);
        }

        private static string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        public static bool ToBool(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool ToNonNullBool(this bool? value)
        {
            if (value == true)
            {
                return true;
            }

            return false;
        }
    }

    public class ValidateRowResult
    {
        public List<ValidationError> ValidationErrors { get; set; }
        public bool Validity { get; set; }
    }
}
