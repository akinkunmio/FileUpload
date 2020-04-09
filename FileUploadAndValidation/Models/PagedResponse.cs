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
            PageSize = 20;
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
    }

    public class PaginationFilter
    {
        public PaginationFilter(int pageSize, int pageNumber)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
        public PaginationFilter()
        {

        }
        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
}
