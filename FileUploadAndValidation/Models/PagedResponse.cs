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

        public double ValidAmountTotal { get; set; }

        public StatusEnum Status { get; set; }

        public string ContentType { get; set; }

        public string ItemType { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string BatchId { get; set; }

        public string FileName { get; set; }
    }

    public class PaginationFilter
    {
        public PaginationFilter(int pageSize, int pageNumber, StatusEnum status, string contentType, string itemType)
        {
            PageSize = (pageSize > 0) ?  pageSize : 10;
            PageNumber = (pageNumber > 0) ? pageNumber : 1;
            Status = status;
            ContentType = contentType;
            ItemType = itemType;
        }
        public PaginationFilter(int pageSize, int pageNumber, string contentType, string itemType)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            ContentType = contentType;
            ItemType = itemType;
        }

        public PaginationFilter()
        {

        }
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public StatusEnum Status { get; set; } = StatusEnum.All;

        public string ContentType { get; set; }

        public string ItemType { get; set; }
    }

    public enum StatusEnum
    {
        All = 0,
        Valid = 1,
        Invalid = 2
    }
}
