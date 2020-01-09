using EPPlus.Core.Extensions;
using ExcelMapper;
using FileUploadAndValidation.DTOs;
using FileUploadAndValidation.ObjectMappers;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace FileUploadAndValidation.FileDataExtractor
{
    public class FirsWhtDataExtractor : IDataExtractor<FirsWhtTransferModel>
    {
       
        public async Task<IList<FirsWhtTransferModel>> ExtractDataFromTxtCsvFile(byte[] fileBytes)
        {
            var records = new List<FirsWhtTransferModel>();
            var mappingResultList = new List<CsvMappingResult<FirsWhtTransferModel>>();

            using (var memoryStream = new MemoryStream(fileBytes))
            {
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                CsvReaderOptions csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
                
                var csvParser = new CsvParser<FirsWhtTransferModel>(csvParserOptions, new FirsWhtTxtCsvMapper());

                var stringifiedStream = Encoding.UTF8.GetString(memoryStream.ToArray());

                mappingResultList = await Task.FromResult(csvParser.ReadFromString(csvReaderOptions, stringifiedStream).ToList());

                mappingResultList.ForEach(e => records.Add(e.Result));
            }

            return records;
        }

        public async Task<IList<FirsWhtTransferModel>> ExtractDataFromXlsFile(byte[] fileBytes)
        {
            var records = default(List<FirsWhtTransferModel>);
            //var bytesArray = Convert.FromBase64String(encodedTo64String);

            using (MemoryStream memStream = new MemoryStream(fileBytes))
            {
                using (var importer = new ExcelImporter(memStream))
                {
                    importer.Configuration.RegisterClassMap<FirsWhtXlsMapper>();
                    //get number of sheets
                    //enumerate over sheets if more than one
                    var sheet = importer.ReadSheet();
                    records = sheet.ReadRows<FirsWhtTransferModel>().ToList();
                }

            }

            return records;
        }

        public async Task<IList<FirsWhtTransferModel>> ExtractDataFromXlxsFile(byte[] fileBytes)
        {
            var transactions = default(List<FirsWhtXlsxMapper>);

            // var fileBytes = Convert.FromBase64String(encodedTo64String);

            using (var stream = new MemoryStream(fileBytes))
            {
                using (var excelPackage = new ExcelPackage(stream))
                {
                    transactions = excelPackage.ToList<FirsWhtXlsxMapper>();
                }
            }

            return await ToFIRS_WHTTransferModelList(transactions);
        }

        private async Task<List<FirsWhtTransferModel>> ToFIRS_WHTTransferModelList(List<FirsWhtXlsxMapper> firsExcelModelList)
        {
            var firsDTO = new List<FirsWhtTransferModel>();
            
            firsExcelModelList.ForEach(model =>
            {
                firsDTO.Add(new FirsWhtTransferModel
                {
                    ContractDescription = model.ContractDescription,
                    ContractorAddress = model.ContractorAddress,
                    ContractorName = model.ContractorName,
                    ContractorTIN = model.ContractorTIN,
                    ExchangeRateToNaira = model.ExchangeRate,
                    InvoicedValue = model.InvoicedValue,
                    InvoiceValueofTransaction = model.InvoiceValueofTransaction,
                    TransactionDate = model.TransactionDate,
                    TransactionNature = model.TransactionNature,
                    TransactionInvoiceRefNo = model.TransactionInvoiceRefNo,
                    WVATRate = model.WVATRate,
                    WVATValue = model.WVATValue,
                    CurrencyOfTransaction = model.CurrencyOfTransaction,
                    TaxAccountNumber = model.TaxAccountNumber
                });
            });

            return firsDTO;
        }

    }
}
