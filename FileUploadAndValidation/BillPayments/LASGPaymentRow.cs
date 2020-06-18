using System;
using System.Collections.Generic;
using System.Linq;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation.BillPayments
{
    public class LASGPaymentRow : ValidatedRow
    {
        const int INDEX_OF_PRODUCT_CODE = 0;
        const int INDEX_OF_ITEM_CODE = 1;
        const int INDEX_OF_REVENUE_CODE = 2;
        const int INDEX_OF_AGENCY_CODE = 3;
        const int INDEX_OF_PAYER_ID = 4; //PAYER ID
        const int INDEX_OF_AMOUNT = 5;
        const int INDEX_OF_PERIOD_FROM = 6;
        const int INDEX_OF_PERIOD_TO = 7;
        const int INDEX_OF_DESCRIPTION = 8;

        public LASGPaymentRow() {}

        public LASGPaymentRow(Row row)
        {
            this.Row = row.Index;

            SetupFields(row.Columns);
        }

        private void SetupFields(List<Column> columns)
        {
            ProductCode = GetColumnValue(columns, INDEX_OF_PRODUCT_CODE, "");
            PayerId = GetColumnValue(columns, INDEX_OF_PAYER_ID, "");
            ItemCode = GetColumnValue(columns, INDEX_OF_ITEM_CODE, "");
            RevenueCode = GetColumnValue(columns, INDEX_OF_REVENUE_CODE, "");
            AgencyCode = GetColumnValue(columns, INDEX_OF_AGENCY_CODE, "");

            var amountProvided = GetColumnValue(columns, INDEX_OF_AMOUNT, "");
            if (decimal.TryParse(amountProvided, out decimal _amount))
            {
                Amount = _amount;
            }

            StartPeriod = GetColumnValue(columns, INDEX_OF_PERIOD_FROM, "");
            EndPeriod = GetColumnValue(columns, INDEX_OF_PERIOD_TO, StartPeriod);
                
            Description = GetColumnValue(columns, INDEX_OF_DESCRIPTION, "");

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(ProductCode))
                errors.Add($"{nameof(ProductCode)} not specified");
            if (string.IsNullOrWhiteSpace(ItemCode))
                errors.Add($"{nameof(ItemCode)} not specified");
            if (string.IsNullOrWhiteSpace(PayerId))
                errors.Add($"{nameof(PayerId)} not specified. E.g. Tax Id");
            if (string.IsNullOrWhiteSpace(RevenueCode))
                errors.Add($"{nameof(RevenueCode)} not specified");
            if (string.IsNullOrWhiteSpace(AgencyCode))
                errors.Add($"{nameof(AgencyCode)} not specified");
            if (string.IsNullOrWhiteSpace(StartPeriod))
                errors.Add($"{nameof(StartPeriod)} not specified");
            if (string.IsNullOrWhiteSpace(EndPeriod))
                errors.Add($"{nameof(EndPeriod)} not specified");
            if (Amount <= 0)
                errors.Add($"{nameof(Amount)} must be greater than 0. Provided amount: {amountProvided} is invalid");

            var startAndEndDateIsValid = true;
            if (!LASGUtil.TryValidateMonth(StartPeriod))
            {
                startAndEndDateIsValid = false;
                errors.Add($"{nameof(StartPeriod)} must be 7 characters e.g. JUL{DateTime.Now.Year}");
            }
            
            if (!LASGUtil.TryValidateMonth(EndPeriod))
            {
                startAndEndDateIsValid = false;
                errors.Add($"{nameof(EndPeriod)} must be 7 characters e.g. JUL{DateTime.Now.Year}");
            }

            if(startAndEndDateIsValid && !LASGUtil.IsValidDateRange(StartPeriod, EndPeriod))
            {
                errors.Add($"{nameof(EndPeriod)} must be greater than {nameof(StartPeriod)}");
            }

            if (string.IsNullOrEmpty(Description))
                errors.Add($"{nameof(Description)} not specified");
            if (Description?.Length > 7)
                errors.Add($"{nameof(Description)} must be 7 characters or less");
            
            IsValid = errors.Count == 0;
            if(!IsValid) ErrorMessages = errors;
        }

        public string AgencyCode {get;set;}
        public string RevenueCode {get;set;}


        ///
        ///<remarks>This is the PayerId or Tax ID</remarks>
        ///
        public string PayerId {get;set;}
        public string CustomerId {get;set;}

        ///
        ///<remarks>WebGuid, LUC, LWC, CBS, etc</remarks>
        ///
        public string ItemCode {get;set;}
        public string ProductCode {get;set;}

        ///
        ///<remarks>JAN2020, DEC2019-JAN2020</remarks>
        ///
        public string StartPeriod {get;set;}
        public string EndPeriod {get;set;}
    }


    static class LASGUtil {
       static string[] months = new []{"JAN","FEB","MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"};
       public static bool TryValidateMonth(string periodMonth){

            if(periodMonth?.Length != 7) return false;

            if(!months.Any(m => m == (periodMonth.Substring(0,3).ToUpper()))) return false;

            int.TryParse(periodMonth.Substring(3,4), out int year);
            if(year < 2000) return false;

            return true;
        }

        internal static bool IsValidDateRange(string startPeriod, string endPeriod, bool checkParamsValidity = true)
        {
            var fromYear = int.Parse(startPeriod.Substring(3,4));
            var toYear = int.Parse(endPeriod.Substring(3,4));

            if (fromYear > toYear) return false;

            var fromMonthGreaterThanTo = Array.IndexOf(months, startPeriod.Substring(0, 3)) > Array.IndexOf(months, endPeriod.Substring(0, 3));
            if (fromYear == toYear && fromMonthGreaterThanTo)
            {
                return false;
            }

            return true;
        }
    }
}