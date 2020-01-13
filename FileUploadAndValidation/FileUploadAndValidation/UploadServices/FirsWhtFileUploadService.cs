using FileUploadAndValidation.DTOs;
using FileUploadAndValidation.FileDataExtractor;
using FileUploadAndValidation.Models;
using FileUploadAndValidation.ObjectMappers;
using FileUploadAndValidation.Validations;
using FilleUploadCore.Exceptions;
using FluentValidation.Results;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadApi.Services
{
    public class FirsWhtFileUploadService : IFileUploadService<FirsWhtUploadResult>
    {
        private readonly IDataExtractor<FirsWhtTransferModel> _firsWHTDataExtractor;
        public FirsWhtFileUploadService(IDataExtractor<FirsWhtTransferModel> firsWHTDataExtractor)
        {
            _firsWHTDataExtractor = firsWHTDataExtractor;
        }

        public async Task<FirsWhtUploadResult> ProcessTxtCsvFile(byte[] fileInBytes)
        {
            var mappingResult = await _firsWHTDataExtractor.ExtractDataFromTxtCsvFile(fileInBytes, new FirsWhtTxtCsvMapper());
            
            var uploadResult = new FirsWhtUploadResult();

            var records = mappingResult.Select(s => s.Result).ToList();
            if(mappingResult.Any(m => m.IsValid == false))
            {
                foreach (var unmappedResult in mappingResult.Where(r => r.IsValid == false))
                    uploadResult.Failures.Add(new FileUploadResult.Failure
                    {
                         MappingError = new MappingError 
                         {
                             ColumnIndex = unmappedResult.Error.ColumnIndex,
                             ErrorMessage = unmappedResult.Error.Value,
                             UnmappedRow = unmappedResult.Error.UnmappedRow
                         },
                         RowNumber = unmappedResult.RowIndex
                    });
            }
            
            uploadResult.RecordsCount = mappingResult.Count();

            bool isValid = await RunValidation(records, uploadResult);

            if (!isValid)
            {
                return uploadResult;
            }

            //await UploadTransactions(uploadResult);

            return uploadResult;

        }

        public async Task<FirsWhtUploadResult> ProcessXlsFile(byte[] fileInBytes)
        {
            var records = await _firsWHTDataExtractor.ExtractDataFromXlsFile(fileInBytes, new FirsWhtXlsMapper());

            var result = await ProcessFIRS_WHTTransferList(records);

            return result;
        }

        public async Task<FirsWhtUploadResult> ProcessXlsxFile(byte[] fileInBytes)
        {
            var records = await _firsWHTDataExtractor.ExtractDataFromXlsxFile(fileInBytes, new FirsWhtXlsxMapper());
            var transaferList = await ToFirsWhtTransferList(records);
            var result = await ProcessFIRS_WHTTransferList(transaferList);

            return result;
        }

        private async Task<IList<FirsWhtTransferModel>> ToFirsWhtTransferList(IList<FirsWhtXlsxMapper> excelRecordList)
        {
            var firsDTO = new List<FirsWhtTransferModel>();
            foreach(var record in excelRecordList)
            {
                firsDTO.Add(new FirsWhtTransferModel
                {
                    ContractDescription = record.ContractDescription,
                    ContractorAddress = record.ContractorAddress,
                    ContractorName = record.ContractorName,
                    ContractorTIN = record.ContractorTIN,
                    ExchangeRateToNaira = record.ExchangeRate,
                    InvoicedValue = record.InvoicedValue,
                    InvoiceValueofTransaction = record.InvoiceValueofTransaction,
                    TransactionDate = record.TransactionDate,
                    TransactionNature = record.TransactionNature,
                    TransactionInvoiceRefNo = record.TransactionInvoiceRefNo,
                    WVATRate = record.WVATRate,
                    WVATValue = record.WVATValue,
                    CurrencyOfTransaction = record.CurrencyOfTransaction,
                    TaxAccountNumber = record.TaxAccountNumber
                });
            }
           
            return firsDTO;
        }

        private async Task<FirsWhtUploadResult> ProcessFIRS_WHTTransferList(IList<FirsWhtTransferModel> records)
        {
            var uploadResult = new FirsWhtUploadResult();

            uploadResult.RecordsCount = records.Count();

            bool isValid = await RunValidation(records, uploadResult);

            if (!isValid)
            {
                return uploadResult;
            }

            //await UploadTransactions(uploadResult);

            return uploadResult;
        }

        private Task UploadTransactions(FileUploadResult uploadResult)
        {
            throw new NotImplementedException();
        }

        private ModelValidationResult ToValidationResultModel(IList<ValidationFailure> validationFailures, string message)
        {
            var validationResult = new ModelValidationResult
            {
                Message = message,
                Errors = new List<ValidationError>()
            };

            foreach (var failure in validationFailures)
            {
                validationResult.Errors.Add(new ValidationError
                {
                    ErrorMessage = failure.ErrorMessage,
                    PropertyName = failure.PropertyName
                });
            }

            return validationResult;
        }


        private async Task<bool> RunValidation(IList<FirsWhtTransferModel> records, FirsWhtUploadResult result)
        {
            if (records == null || !records.Any())
            {
                result.ErrorMessage = "No transactions was added for upload";
                return false;
            }

            var isValid = true;

            var validator = new FirsWhtValidator();

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];

                if (record == null)
                    continue;

                try
                {
                    var validationResult = await validator.ValidateAsync(record);

                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(ToValidationResultModel(validationResult.Errors, "validation failed for FIRS_WHT transaction records"));
                    }

                    result.ValidatedRecordsRowNumber.Add(i+1);
                }
                catch (ValidationException ex)
                {
                    var failure = new FileUploadResult.Failure();

                    failure.ValidationErrors = ex.ValidationResult.Errors;
                    failure.RowNumber = i + 1;
                    result.Failures.Add(failure);

                    isValid = false;
                }
            }

            return isValid;
        }
    }
}
