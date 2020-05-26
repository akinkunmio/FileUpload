using FileUploadAndValidation.FileServices;
using FileUploadAndValidation.Helpers;
using FileUploadAndValidation.Models;
using FilleUploadCore.FileReaders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FileUploadAndValidation.FileContentValidators
{
    public class LirsMultiTaxContentValidator : IFileContentValidator
    {
        private readonly ILogger<LirsMultiTaxContentValidator> _logger;

        public LirsMultiTaxContentValidator(ILogger<LirsMultiTaxContentValidator> logger)
        {
            _logger = logger;
        }
        public Task<UploadResult> Validate(FileUploadRequest uploadRequest, IEnumerable<Row> rows, UploadResult uploadResult)
        {
            throw new NotImplementedException();
        }

        private async Task<ValidateRowsResult> ValidateContent(string authority, IEnumerable<Row> contentRows)
        {
            var validRows = new List<RowDetail>();

            ValidateRowModel validateRowModel;
            var failures = new List<Failure>();

            foreach (var row in contentRows)
            {
                validateRowModel = await ValidateRow(authority, row);

                if (validateRowModel.isValid)
                    validRows.Add(validateRowModel.Valid);

                if (!validateRowModel.isValid)
                    failures.Add(validateRowModel.Failure);
            }

            return new ValidateRowsResult { Failures = failures, ValidRows = validRows };
        }

        private async Task<ValidateRowModel> ValidateRow(string authority, Row row)
        {
            var rowDetail = new RowDetail();
            var result = new ValidateRowModel();

            string rowTaxType = default;

            if (authority.ToLower().Equals(GenericConstants.Firs))
                rowTaxType = row.Columns[15].Value;

            var columnContracts = GetColumnContractByTaxType(authority, rowTaxType);

            var validationResult = GenericHelpers.ValidateRowCell(row, columnContracts);

            if (rowTaxType.ToLower().Equals(GenericConstants.Wht))
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    BeneficiaryTin = row.Columns[0].Value,
                    BeneficiaryName = row.Columns[1].Value,
                    BeneficiaryAddress = row.Columns[2].Value,
                    ContractDate = row.Columns[3].Value,
                    ContractDescription = row.Columns[4].Value,
                    ContractAmount = row.Columns[5].Value,
                    ContractType = row.Columns[6].Value,
                    PeriodCovered = row.Columns[7].Value,
                    InvoiceNumber = row.Columns[8].Value,
                    WhtRate = row.Columns[9].Value,
                    WhtAmount = row.Columns[10].Value,
                    PayerTin = row.Columns[14].Value,
                    TaxType = row.Columns[15].Value
                };

            if (rowTaxType.ToLower().Equals(GenericConstants.Vat)
                || rowTaxType.ToLower().Equals(GenericConstants.Cit)
                || rowTaxType.ToLower().Equals(GenericConstants.Edt)
                || rowTaxType.ToLower().Equals(GenericConstants.PreOpLevy))
                rowDetail = new RowDetail
                {
                    RowNum = row.Index,
                    Amount = row.Columns[11].Value,
                    Comment = row.Columns[12].Value,
                    DocumentNumber = row.Columns[13].Value,
                    PayerTin = row.Columns[14].Value,
                    TaxType = row.Columns[15].Value
                };

            result.isValid = validationResult.Validity;

            if (validationResult.Validity)
            {
                result.Valid = rowDetail;
            }
            else
            {
                result.Failure = new Failure
                {
                    ColumnValidationErrors = validationResult.ValidationErrors,
                    Row = rowDetail
                };
            }

            return await Task.FromResult(result);
        }

        private ColumnContract[] GetColumnContractByTaxType(string authority, string taxType)
        {
            ColumnContract[] columnContracts = default;

            if (authority.ToLower().Equals(GenericConstants.Firs))
            {
                if (!string.IsNullOrWhiteSpace(taxType)
                   && taxType.ToLower().Equals(GenericConstants.Wht))
                    columnContracts = ContentTypeColumnContract.FirsMultiTaxWht();

                if (!string.IsNullOrWhiteSpace(taxType)
                   && (taxType.ToLower().Equals(GenericConstants.Vat)
                   || taxType.ToLower().Equals(GenericConstants.PreOpLevy)
                   || taxType.ToLower().Equals(GenericConstants.Cit)
                   || taxType.ToLower().Equals(GenericConstants.Edt)))
                    columnContracts = ContentTypeColumnContract.FirsMultiTaxOther();
            }

            return columnContracts;
        }
    }
}
