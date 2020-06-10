using System;
using System.Collections.Generic;
using FilleUploadCore.FileReaders;

namespace FileUploadAndValidation.BillPayments
{
    public class ManualCaptureRow : ValidatedRow
    {
        private readonly ManualCaptureRowConfig _config;
        const int INDEX_OF_PRODUCT_CODE = 0;
        const int INDEX_OF_ITEM_CODE = 1;
        const int INDEX_OF_CUSTOMER_ID = 2;
        const int INDEX_OF_AMOUNT = 3;
        const int INDEX_OF_DESCRIPTION = 4;
        const int INDEX_OF_CUSTOMER_NAME = 5;
        const int INDEX_OF_PHONE = 6;
        const int INDEX_OF_EMAIL = 7;
        const int INDEX_OF_CUST_ADDRESS = 8;

        public ManualCaptureRow(Row row, ManualCaptureRowConfig config)
        {
            _config = config;

            this.Row = row.Index;

            SetupFields(row.Columns);
        }

        private void SetupFields(List<Column> columns)
        {
            ProductCode = GetColumnValue(columns, INDEX_OF_PRODUCT_CODE, "");
            CustomerId = GetColumnValue(columns, INDEX_OF_CUSTOMER_ID, "");
            ItemCode = GetColumnValue(columns, INDEX_OF_ITEM_CODE, "");
            Description = GetColumnValue(columns, INDEX_OF_DESCRIPTION, "");

            var amountProvided = GetColumnValue(columns, INDEX_OF_AMOUNT, "");
            if (decimal.TryParse(amountProvided, out decimal _amount))
            {
                Amount = _amount;
            }

            CustomerName = GetColumnValue(columns, INDEX_OF_CUSTOMER_NAME, "");
            Email = GetColumnValue(columns, INDEX_OF_EMAIL, "");
            PhoneNumber = GetColumnValue(columns, INDEX_OF_PHONE, "");
            Address = GetColumnValue(columns, INDEX_OF_CUST_ADDRESS, "");

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(ProductCode))
                errors.Add($"{nameof(ProductCode)} not specified");
            if (!_config.AutogenerateCustomerId && string.IsNullOrWhiteSpace(CustomerId))
                errors.Add($"{nameof(CustomerId)} not specified");
            if (string.IsNullOrWhiteSpace(ItemCode))
                errors.Add($"{nameof(ItemCode)} not specified");
            if (Amount <= 0)
                errors.Add($"{nameof(Amount)} must be greater than 0. Provided amount: {amountProvided} is invalid");
            if (string.IsNullOrWhiteSpace(CustomerName))
                errors.Add($"{nameof(CustomerName)} not specified");
            if (_config.IsEmailRequired && string.IsNullOrWhiteSpace(Email))
                errors.Add($"{nameof(Email)} not specified");
            if (_config.IsPhoneNumberRequired && string.IsNullOrWhiteSpace(PhoneNumber))
                errors.Add($"{nameof(PhoneNumber)} not specified");
            if (_config.IsAddressRequired && string.IsNullOrWhiteSpace(Address))
                errors.Add($"{nameof(Address)} not specified");

            IsValid = errors.Count == 0;
            if(!IsValid) ErrorMessages = errors;
        }

        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ItemCode { get; set; }
        public string ProductCode { get; private set; }
    }
}