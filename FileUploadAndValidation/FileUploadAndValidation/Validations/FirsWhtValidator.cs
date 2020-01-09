using FileUploadAndValidation.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileUploadAndValidation.Validations
{
    public class FirsWhtValidator : AbstractValidator<FirsWhtTransferModel>
    {
        public FirsWhtValidator()
        {
            RuleFor(x => x.ContractDescription)
                .NotEmpty()
                .WithMessage("ContractDescription cannot be empty");

            RuleFor(x => x.ExchangeRateToNaira)
                .NotEmpty()
                .WithMessage("ExchangeRate cannot be empty");

            RuleFor(x => x.ContractorTIN)
                .NotEmpty()
                .WithMessage("ContractorTIN cannot be empty");

            RuleFor(x => x.ContractorName)
                .NotEmpty()
                .WithMessage("ContractorName cannot be empty");

            RuleFor(x => x.ContractorAddress)
                .NotEmpty()
                .WithMessage("ContractorAddress cannot be empty");

            RuleFor(x => x.InvoicedValue)
                .NotEmpty()
                .WithMessage("InvoicedValue cannot have 0 value or be empty");

            RuleFor(x => x.TransactionDate)
                .Cascade(cascadeMode: CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .GreaterThan(DateTime.MinValue)
                .LessThanOrEqualTo(DateTime.Now);

            RuleFor(x => x.TransactionNature)
                .NotEmpty()
                .WithMessage("TransactionNature cannot be empty");

            RuleFor(x => x.TransactionInvoiceRefNo)
                .NotEmpty()
                .WithMessage("TransactionRefNo cannot be empty");

            RuleFor(x => x.WVATRate)
                .NotEmpty()
                .WithMessage("WVATRate cannot be empty");

            RuleFor(x => x.WVATValue)
                .NotEmpty()
                .WithMessage("WVATValue cannot be empty");

            RuleFor(x => x.InvoiceValueofTransaction)
               .NotEmpty()
               .WithMessage("InvoiceValueofTransaction cannot be empty");
        }
    }
}
