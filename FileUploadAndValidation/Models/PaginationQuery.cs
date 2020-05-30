﻿using FilleUploadCore.Exceptions;
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
        }

        public PaginationQuery(int pageNumber, int pageSize, int status)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
            Status = (Enum.IsDefined(typeof(StatusEnum), status)) 
                ? (StatusEnum)status 
                : throw new AppException("The field 'Status' must be between 0 and 2.");
        }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public StatusEnum Status { get; set; }
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

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
