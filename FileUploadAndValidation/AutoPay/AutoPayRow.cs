using System;
using System.Collections.Generic;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation
{
    public class AutoPayRow : ValidatedRow
    {
        public AutoPayRow(Row row)
        {
            this.Row = row.Index;
            this.SetupFields(row.Columns);
        }

        private void SetupFields(List<Column> columns)
        {
            if(string.IsNullOrWhiteSpace(columns[0].Value))
            {
                IsValid = false;
                this.ErrorMessages.Add("COVID-19 Error");
            }
            else
            {
                IsValid = true;
            }
        }

        
        public string PaymentReference { get; set; }
        public string BeneficiaryCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string CBNCode { get; set; }
        public string IsCashCard { get; set; }
        public string Narration { get; set; }
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