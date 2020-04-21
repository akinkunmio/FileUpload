using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class BillPaymentRowStatus
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }

        public int Row { get; set; }

        public string Error { get; set; }

        public string Status { get; set; }
    }

    public class BillPaymentRowStatusDto
    {
        public string ProductCode { get; set; }

        public string ItemCode { get; set; }

        public string CustomerId { get; set; }

        public double Amount { get; set; }

        public string Error { get; set; }

        public string RowStatus { get; set; }

        public int RowNum { get; set; }
    }

    public class BillPaymentRowStatusDtoObject
    {
        public IEnumerable<BillPaymentRowStatusDto> RowStatusDtos { get; set; }

        public int TotalRowsCount { get; set; }

        public double ValidAmountSum { get; set; }
    }

    public class PagedData<T>
    {
        public IEnumerable<T> Data { get; set; }

        public int TotalRowsCount { get; set; }

        public double TotalAmountSum { get; set; }
    }
}
