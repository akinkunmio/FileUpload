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
            Status = StatusEnum.All;
        }

        public PaginationQuery(int pageNumber, int pageSize, StatusEnum status, string contentType, string itemType)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
            Status = status;
            ContentType = contentType;
            ItemType = itemType;
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public StatusEnum Status { get; set; }

        public string ContentType { get; set; }

        public string ItemType { get; set; }
    }
}
