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
        const int INDEX_OF_CUSTOMER_ID = 4; //PAYER ID
        const int INDEX_OF_AMOUNT = 5;
        const int INDEX_OF_PERIOD_FROM = 6;
        const int INDEX_OF_PERIOD_TO = 7;
        const int INDEX_OF_DESCRIPTION = 8;

        public LASGPaymentRow() {
            
        }

        public LASGPaymentRow(Row row)
        {
            this.Index = row.Index;

            SetupFields(row.Columns);
        }

        private void SetupFields(List<Column> columns)
        {
            ProductCode = GetColumnValue(columns, INDEX_OF_PRODUCT_CODE, "");
            CustomerId = GetColumnValue(columns, INDEX_OF_CUSTOMER_ID, "");
            ItemCode = GetColumnValue(columns, INDEX_OF_ITEM_CODE, "");
            RevenueCode = GetColumnValue(columns, INDEX_OF_REVENUE_CODE, "");
            AgencyCode = GetColumnValue(columns, INDEX_OF_AGENCY_CODE, "");

            if (decimal.TryParse(GetColumnValue(columns, INDEX_OF_AMOUNT, ""), out decimal _amount))
            {
                Amount = _amount;
            }

            StartPeriod = GetColumnValue(columns, INDEX_OF_PERIOD_FROM, "");
            EndPeriod = GetColumnValue(columns, INDEX_OF_PERIOD_TO, StartPeriod);
            if(string.IsNullOrWhiteSpace(EndPeriod) && !string.IsNullOrEmpty(StartPeriod))
                EndPeriod = StartPeriod;
                
            Description = GetColumnValue(columns, INDEX_OF_DESCRIPTION, "");

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(ProductCode))
                errors.Add($"{nameof(ProductCode)} not specified");
            if (string.IsNullOrWhiteSpace(ItemCode))
                errors.Add($"{nameof(ItemCode)} not specified");
            if (string.IsNullOrWhiteSpace(RevenueCode))
                errors.Add($"{nameof(RevenueCode)} not specified");
            if (string.IsNullOrWhiteSpace(AgencyCode))
                errors.Add($"{nameof(AgencyCode)} not specified");
            if (string.IsNullOrWhiteSpace(StartPeriod))
                errors.Add($"{nameof(StartPeriod)} not specified");
            if (string.IsNullOrWhiteSpace(EndPeriod))
                errors.Add($"{nameof(EndPeriod)} not specified");
                
            if (!LASGUtil.TryValidateMonth(StartPeriod))
            {
                errors.Add($"{nameof(StartPeriod)} must be 7 characters or less e.g. JUL{DateTime.Now.Year}");
            }
            
            if (!LASGUtil.TryValidateMonth(EndPeriod))
            {
                errors.Add($"{nameof(EndPeriod)} must be 7 characters or less e.g. JUL{DateTime.Now.Year}");
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
        public static bool TryValidateMonth(string periodMonth){
            var months = new []{"JAN","FEB","MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"};

            if(periodMonth?.Length != 7) return false;

            if(!months.Any(m => m == periodMonth.Substring(0,3))) return false;

            int.TryParse(periodMonth.Substring(3,4), out int year);
            if(year < 2000) return false;

            return true;
        }
    }
}