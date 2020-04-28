using AutoMapper;
using FileUploadAndValidation.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.AutoMapperProfiles
{
	public class DomainProfile : Profile
	{
		public DomainProfile()
		{
			CreateMap<RowDetail, BillPayment>();
		}
	}
}
