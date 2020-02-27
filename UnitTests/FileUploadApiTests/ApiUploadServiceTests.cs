using Autofac.Extras.Moq;
using FileUploadAndValidation.Models;
using FileUploadApi.ApiServices;
using FileUploadApi.Services;
using FilleUploadCore.Exceptions;
using FilleUploadCore.UploadManagers;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.FileUploadAndValidationTests
{
    public class ApiUploadServiceTests
    {
        [Fact]
        public async Task GetBillPaymentsStatus_should()
        {
                using (var mock = AutoMock.GetLoose())
                {
                    var service = mock.Create<ApiUploadService>();

                    Func<Task> act = async () => await service.UploadFileAsync(new UploadOptions(), new MemoryStream());
                    Func<Task> act1 = async () => await service.UploadFileAsync(new UploadOptions() { AuthToken = "AuthToken" }, new MemoryStream());
                    Func<Task> act2 = async () => await service.UploadFileAsync(new UploadOptions() { AuthToken = "AuthToken", ContentType = "billpaymentid" }, new MemoryStream());
                    Func<Task> act3 = async () => await service.UploadFileAsync(new UploadOptions() { AuthToken = "AuthToken", ContentType = "billpaymentid", FileExtension = "csv" }, new MemoryStream());
                    Func<Task> act4 = async () => await service.UploadFileAsync(new UploadOptions() { AuthToken = "AuthToken", ContentType = "billpaymentid", FileExtension = "csv", FileName = "dstvpayments" }, new MemoryStream());
                    Func<Task> act5 = async () => await service.UploadFileAsync(new UploadOptions() { AuthToken = "AuthToken", ContentType = "billpaymentid", FileExtension = "csv", FileName = "dstvpayments", RawFileLocation = "//nas//uploadservice" }, new MemoryStream());
                    Func<Task> act6 = async () => await service.UploadFileAsync(new UploadOptions() { AuthToken = "AuthToken", ContentType = "billpayment", FileExtension = "csv", FileName = "dstvpayments", RawFileLocation = "//nas//uploadservice", ItemType = "billpaymentid" }, new MemoryStream());

                    act.Should().Throw<AppException>().WithMessage($"'AuthToken' cannot be null or empty");
                    act1.Should().Throw<AppException>().WithMessage($"'ContentType' cannot be null or empty");
                    act2.Should().Throw<AppException>().WithMessage($"'FileExtension' cannot be null or empty");
                    act3.Should().Throw<AppException>().WithMessage($"'FileName' cannot be null or empty");
                    act4.Should().Throw<AppException>().WithMessage($"'RawFileLocation' cannot be null or empty");
                    act5.Should().Throw<AppException>().WithMessage($"'ItemType' cannot be null or empty");
                    act6.Should().Throw<AppException>().WithMessage($"'Length' cannot have zero value or default");
                }
        }

        [Fact]
        public async Task GetBillPaymentsStatus_throw_an_exception_when_parameters_are_not_set()
        {

            string batchId = "";
            var paginationFilter = new PaginationFilter();

            using (var mock = AutoMock.GetLoose())
            {
                var service = mock.Create<ApiUploadService>();
               
                Func<Task> act = async () => await service.GetBillPaymentsStatus(batchId, paginationFilter);
                Func<Task> act1 = async () => await service.GetBillPaymentsStatus("batchId", paginationFilter);
                Func<Task> act2 = async () => await service.GetBillPaymentsStatus("batchId", new PaginationFilter() { PageNumber = 1 });

                act.Should().Throw<AppException>().WithMessage($"'{nameof(batchId)}' cannot be null or empty");
                act1.Should().Throw<AppException>().WithMessage($"'{nameof(paginationFilter.PageNumber)}' cannot have zero value or default");
                act2.Should().Throw<AppException>().WithMessage($"'{nameof(paginationFilter.PageSize)}' cannot have zero value or default");
            }
        }

        [Fact]
        public async Task GetFileSummary_throw_an_exception_when_batchId_is_null()
        {

            string batchId = "";
            var paginationFilter = new PaginationFilter();

            using (var mock = AutoMock.GetLoose())
            {
                var service = mock.Create<ApiUploadService>();

                Func<Task> act = async () => await service.GetFileSummary(batchId);

                act.Should().Throw<AppException>().WithMessage($"'{nameof(batchId)}' cannot be null or empty");
            }
        }


        [Fact]
        public async Task PaymentInitiationConfirmed_throw_an_exception_when_parameters_are_not_set()
        {
            var batchId = "newbatchid";

            using (var mock = AutoMock.GetLoose())
            {
                var service = mock.Create<ApiUploadService>();

                Func<Task> act = async () => await service.PaymentInitiationConfirmed("", new InitiatePaymentOptions());
                Func<Task> act1 = async () => await service.PaymentInitiationConfirmed(batchId, new InitiatePaymentOptions());
                Func<Task> act2 = async () => await service.PaymentInitiationConfirmed(batchId, new InitiatePaymentOptions() { AuthToken = "Authoken", UserId = 12345, ApprovalConfigId = 1234, UserName = "Adewale" });
                Func<Task> act3 = async () => await service.PaymentInitiationConfirmed(batchId, new InitiatePaymentOptions() { AuthToken = "Authoken", BusinessId = 12345, UserId = 1234, UserName = "Adewale" });
                Func<Task> act4 = async () => await service.PaymentInitiationConfirmed(batchId, new InitiatePaymentOptions() { AuthToken = "Authoken", BusinessId = 12345, ApprovalConfigId = 1234, UserId = 1234 });
                Func<Task> act5 = async () => await service.PaymentInitiationConfirmed(batchId, new InitiatePaymentOptions() { AuthToken = "Authoken", BusinessId = 12345, ApprovalConfigId = 1234, UserName = "Adewale" });


                act.Should().Throw<AppException>().WithMessage($"'batchId' cannot be null or empty");
                act1.Should().Throw<AppException>().WithMessage($"'AuthToken' cannot be null or empty");
                act2.Should().Throw<AppException>().WithMessage($"'BusinessId' cannot be null");
                act3.Should().Throw<AppException>().WithMessage($"'ApprovalConfigId' cannot be null");
                act4.Should().Throw<AppException>().WithMessage($"'UserName' cannot be null or empty");
                act5.Should().Throw<AppException>().WithMessage($"'UserId' cannot be null");
            }
        }
    }
}
