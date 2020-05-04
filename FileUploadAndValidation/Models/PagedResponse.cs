using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Models
{
    public class PagedResponse<T>
    {
        public PagedResponse()
        {
            PageNumber = 1;
            PageSize = 10;
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
    }

    public class PaginationFilter
    {
        public PaginationFilter(int pageSize, int pageNumber)
        {
            PageSize = (pageSize > 0) ?  pageSize : 10;
            PageNumber = (pageNumber > 0) ? pageNumber : 1;
        }
        public PaginationFilter()
        {

        }
        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
}
