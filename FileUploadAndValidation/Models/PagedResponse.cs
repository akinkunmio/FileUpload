using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class PagedResponse<T>
    {
        public PagedResponse()
        {
        }

        public PagedResponse(IEnumerable<T> data)
        {
            Data = data;
        }

        public IEnumerable<T> Data { get; set; }

        public int? PageSize { get; set; }

        public int? PageNumber { get; set; }

        public int TotalCount { get; set; }

        public string Error { get; set; }

        public decimal ValidAmountTotal { get; set; }

        public string Status { get; set; }

        public string ContentType { get; set; }

        public string ItemType { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string BatchId { get; set; }

        public string FileName { get; set; }

        public int ValidCount { get; set; }

        public int InvalidCount { get; set; }
    }

    public class SummaryPagedResponse<T>
    {
        public SummaryPagedResponse()
        {
        }

        public SummaryPagedResponse(IEnumerable<T> data)
        {
            Data = data;
        }

        public IEnumerable<T> Data { get; set; }

        public int? PageSize { get; set; }

        public int? PageNumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string Status { get; set; }

        public int TotalCount { get; set; }

        public string Error { get; set; }
    }

    public class PaginationFilter
    {
        public PaginationFilter(int pageSize, int pageNumber, StatusEnum status, string taxType)
        {
            PageSize = (pageSize > 0) ?  pageSize : 10;
            PageNumber = (pageNumber > 0) ? pageNumber : 1;
            Status = status;
            TaxType = taxType;
        }
        public PaginationFilter(int pageSize, int pageNumber)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public PaginationFilter()
        {

        }
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public StatusEnum Status { get; set; } = StatusEnum.All;

        public string TaxType { get; set; }

        public string ContentType { get; set; }

        public string ItemType { get; set; }
    }

    public class SummaryPaginationFilter
    {
        public SummaryPaginationFilter(int pageSize, int pageNumber, string productCode, SummaryStatusEnum status)
        {
            PageSize = (pageSize > 0) ? pageSize : 10;
            PageNumber = (pageNumber > 0) ? pageNumber : 1;
            ProductCode = productCode;
            Status = status;
        }

        public SummaryPaginationFilter(int pageSize, int pageNumber)
        {
            PageSize = (pageSize > 0) ? pageSize : 10;
            PageNumber = (pageNumber > 0) ? pageNumber : 1;
        }

        public SummaryPaginationFilter()
        {

        }

        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public SummaryStatusEnum Status { get; set; } = SummaryStatusEnum.All;

        //public string ContentType { get; set; }

        //public string ItemType { get; set; }
    }

    public enum StatusEnum
    {
        All = 0,
        Valid = 1,
        Invalid = 2
    }

    public enum SummaryStatusEnum
    {
        All = 0,
        Valid = 1,
        Invalid = 2,
        ValidAndInvalid = 3
    }
}
