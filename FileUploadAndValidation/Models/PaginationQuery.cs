using FilleUploadCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class PaginationQuery
    {
        public PaginationQuery()
        {
            PageNumber = 1;
            PageSize = 10;
            Status = (int)StatusEnum.All;
            TaxType = "all";
        }

        public PaginationQuery(int pageNumber, int pageSize, int status)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
            Status = status;
            TaxType = "all";
        }

        public PaginationQuery(int pageNumber, int pageSize, int status, string taxType)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
            Status = status;
            TaxType = taxType;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int Status { get; set; }

        public string TaxType { get; set; }
    }

    public class SummaryPaginationQuery
    {
        public SummaryPaginationQuery()
        {
            PageNumber = 1;
            PageSize = 10;
        }

        public SummaryPaginationQuery(int pageNumber, int pageSize)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
        }

        public SummaryPaginationQuery(int pageNumber, int pageSize, string productCode, string productName)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
            ProductCode = productCode;
            ProductName = productName;
        }

        public SummaryPaginationQuery(int pageNumber, int pageSize, string productCode, string productName, SummaryStatusEnum status)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
            ProductCode = productCode;
            ProductName = productName;
            Status = status;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public SummaryStatusEnum Status { get; set; } = SummaryStatusEnum.All;
    }
}
