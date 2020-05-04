using System.Collections.Generic;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation
{
    public class AutoPayRow
    {
        public AutoPayRow(Row row)
        {
            this.Index = row.Index;
            this.Validate(row.Columns);
        }

        private void Validate(List<Column> columns)
        {
            if(string.IsNullOrWhiteSpace(columns[0].Value))
            {
                IsValid = false;
                this.ErrorMessage = "COVID-19 Error";
            }
            else
            {
                IsValid = true;
            }
        }

        public int Index { get; }
        public bool IsValid {get; private set; }
        public string ErrorMessage { get; private set; }
        public string PaymentReference { get; set; }
        public string BeneficiaryCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string CBNCode { get; set; }
        public string IsCashCard { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }

        #region For Consolidated Upload Only
        public string EmailAddress { get; set; }
        #endregion

        #region For Non-Consolidated Upload
        public string PaymentType { get; set; }
        public string PaymentDate { get; set; }
        #endregion

        //Payment Reference, Beneficiary Code, Beneficiary Name,Account Number, Account Type, CBN Code, Is CashCard, Narration, Amount, Email Address, Currency Code
        //-- Not Consolidated
        //Payment Reference, Payment Type, Beneficiary Code, Payment Date, Narration, Beneficiary Name, CBN Code, Account Number, Account Type, Amount, Currency Code

    }
}