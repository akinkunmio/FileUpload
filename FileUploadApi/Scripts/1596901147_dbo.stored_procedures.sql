
IF OBJECT_ID('tbl_bill_payment_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_bill_payment_transactions_detail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[product_code] [nvarchar](100) NULL,
	[item_code] [nvarchar](100) NULL,
	[customer_id] [nvarchar](100) NULL,
	[amount] [nvarchar](100) NULL,
	[error] [nvarchar](max) NULL,
	[row_status] [nvarchar](50) NULL,
	[created_date] [nvarchar](50) NULL,
	[modified_date] [nvarchar](50) NULL,
	[row_num] [int] NULL,
	[transactions_summary_id] [bigint] NULL,
	[initial_validation_status] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO

/****** Object:  Table [dbo].[tbl_fctirs_multi_tax_transactions_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF OBJECT_ID('tbl_fctirs_multi_tax_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_fctirs_multi_tax_transactions_detail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[product_code] [nvarchar](50) NULL,
	[item_code] [nvarchar](50) NULL,
	[customer_id] [nvarchar](256) NULL,
	[amount] [nvarchar](50) NULL,
	[tax_type] [nvarchar](max) NULL,
	[customer_name] [nvarchar](max) NULL,
	[phone_number] [nvarchar](50) NULL,
	[email] [nvarchar](256) NULL,
	[address_info] [nvarchar](max) NULL,
	[error] [nvarchar](max) NULL,
	[row_status] [nvarchar](50) NULL,
	[created_date] [nvarchar](50) NULL,
	[modified_date] [nvarchar](50) NULL,
	[row_num] [int] NULL,
	[transactions_summary_id] [bigint] NULL,
	[initial_validation_status] [nvarchar](50) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO

/****** Object:  Table [dbo].[tbl_firs_multi_tax_transactions_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF OBJECT_ID('tbl_firs_multi_tax_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_firs_multi_tax_transactions_detail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[beneficiary_tin] [nvarchar](256) NULL,
	[beneficiary_name] [nvarchar](256) NULL,
	[beneficiary_address] [nvarchar](256) NULL,
	[contract_date] [nvarchar](256) NULL,
	[contract_amount] [nvarchar](256) NULL,
	[contract_description] [nvarchar](256) NULL,
	[invoice_number] [nvarchar](256) NULL,
	[contract_type] [nvarchar](256) NULL,
	[period_covered] [nvarchar](256) NULL,
	[wht_rate] [nvarchar](256) NULL,
	[wht_amount] [nvarchar](256) NULL,
	[contractor_name] [nvarchar](256) NULL,
	[contractor_address] [nvarchar](256) NULL,
	[contractor_tin] [nvarchar](256) NULL,
	[transaction_date] [nvarchar](256) NULL,
	[nature_of_transaction] [nvarchar](256) NULL,
	[transaction_currency] [nvarchar](50) NULL,
	[currency_invoiced_value] [nvarchar](50) NULL,
	[transaction_invoiced_value] [nvarchar](50) NULL,
	[currency_exchange_rate] [nvarchar](10) NULL,
	[tax_account_number] [nvarchar](50) NULL,
	[wvat_rate] [nvarchar](50) NULL,
	[wvat_value] [nvarchar](50) NULL,
	[amount] [nvarchar](50) NULL,
	[comment] [nvarchar](max) NULL,
	[document_number] [nvarchar](50) NULL,
	[tax_type] [nvarchar](50) NULL,
	[payer_tin] [nvarchar](256) NULL,
	[error] [nvarchar](max) NULL,
	[row_status] [nvarchar](50) NULL,
	[created_date] [nvarchar](50) NULL,
	[modified_date] [nvarchar](50) NULL,
	[row_num] [int] NULL,
	[transactions_summary_id] [bigint] NULL,
	[initial_validation_status] [nvarchar](50) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO

/****** Object:  Table [dbo].[tbl_firs_wht_transactions_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF OBJECT_ID('tbl_firs_wht_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_firs_wht_transactions_detail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[beneficiary_tin] [nvarchar](256) NULL,
	[beneficiary_name] [nvarchar](256) NULL,
	[beneficiary_address] [nvarchar](256) NULL,
	[contract_date] [nvarchar](256) NULL,
	[contract_amount] [nvarchar](256) NULL,
	[invoice_number] [nvarchar](256) NULL,
	[contract_type] [nvarchar](256) NULL,
	[period_covered] [nvarchar](256) NULL,
	[wht_rate] [nvarchar](256) NULL,
	[wht_amount] [nvarchar](256) NULL,
	[error] [nvarchar](max) NULL,
	[row_status] [nvarchar](50) NULL,
	[created_date] [nvarchar](50) NULL,
	[modified_date] [nvarchar](50) NULL,
	[row_num] [int] NULL,
	[transactions_summary_id] [bigint] NULL,
	[initial_validation_status] [nvarchar](50) NULL,
	[contract_description] [nvarchar](256) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO

/****** Object:  Table [dbo].[tbl_firs_wvat_transactions_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF OBJECT_ID('tbl_firs_wvat_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_firs_wvat_transactions_detail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[contractor_name] [nvarchar](256) NULL,
	[contractor_address] [nvarchar](256) NULL,
	[contractor_tin] [nvarchar](256) NULL,
	[contract_description] [nvarchar](256) NULL,
	[transaction_date] [nvarchar](256) NULL,
	[nature_of_transaction] [nvarchar](256) NULL,
	[invoice_number] [nvarchar](256) NULL,
	[transaction_currency] [nvarchar](50) NULL,
	[currency_invoiced_value] [nvarchar](50) NULL,
	[transaction_invoiced_value] [nvarchar](50) NULL,
	[currency_exchange_rate] [nvarchar](10) NULL,
	[tax_account_number] [nvarchar](50) NULL,
	[wvat_rate] [nvarchar](10) NULL,
	[wvat_value] [nvarchar](10) NULL,
	[error] [nvarchar](max) NULL,
	[row_status] [nvarchar](50) NULL,
	[created_date] [nvarchar](50) NULL,
	[modified_date] [nvarchar](50) NULL,
	[row_num] [int] NULL,
	[transactions_summary_id] [bigint] NULL,
	[initial_validation_status] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO


/****** Object:  Table [dbo].[tbl_transactions_summary]    Script Date: 06/06/2020 12:57:10 PM ******/
IF OBJECT_ID('tbl_transactions_summary','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_transactions_summary](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[batch_id] [nvarchar](256) NOT NULL,
	[transaction_status] [nvarchar](50) NULL,
	[item_type] [nvarchar](50) NULL,
	[num_of_records] [int] NULL,
	[upload_date] [nvarchar](50) NULL,
	[modified_date] [nvarchar](50) NULL,
	[uploaded_by] [nvarchar](256) NULL,
	[num_of_valid_records] [int] NULL,
	[nas_tovalidate_file] [nvarchar](max) NULL,
	[nas_raw_file] [nvarchar](max) NULL,
	[nas_confirmed_file] [nvarchar](max) NULL,
	[content_type] [nvarchar](100) NULL,
	[upload_successful] [bit] NULL,
	[userid] [bigint] NULL,
	[nas_uservalidationfile] [nvarchar](max) NULL,
	[valid_amount_sum] [float] NULL,
	[product_code] [nvarchar](50) NULL,
	[name_of_file] [nvarchar](50) NULL,
	[product_name] [nvarchar](50) NULL,
	[error] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_firs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'payer_name')
BEGIN
	ALTER TABLE tbl_firs_multi_tax_transactions_detail ADD [payer_name] varchar(250) Null
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_firs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'transaction_convenience_fee')
BEGIN
	ALTER TABLE tbl_firs_multi_tax_transactions_detail ADD [transaction_convenience_fee] decimal default(0)
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_firs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'batch_convenience_fee')
BEGIN
	ALTER TABLE tbl_firs_multi_tax_transactions_detail ADD [batch_convenience_fee] decimal default(0)
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_fctirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'surcharge')
BEGIN
	ALTER TABLE tbl_fctirs_multi_tax_transactions_detail ADD [surcharge] decimal default(0)
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_fctirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'transaction_convenience_fee')
BEGIN
	ALTER TABLE tbl_fctirs_multi_tax_transactions_detail ADD [transaction_convenience_fee] decimal default(0)
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_fctirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'batch_convenience_fee')
BEGIN
	ALTER TABLE tbl_fctirs_multi_tax_transactions_detail ADD [batch_convenience_fee] decimal default(0)
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_bill_payment_transactions_detail'
AND COLUMN_NAME = 'surcharge')
BEGIN
	ALTER TABLE tbl_bill_payment_transactions_detail ADD [surcharge] decimal default(0)
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_bill_payment_transactions_detail'
AND COLUMN_NAME = 'customer_name')
BEGIN
	ALTER TABLE tbl_bill_payment_transactions_detail ADD [customer_name] varchar(250) Null
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_bill_payment_transactions_detail'
AND COLUMN_NAME = 'transaction_convenience_fee')
BEGIN
	ALTER TABLE tbl_bill_payment_transactions_detail ADD [transaction_convenience_fee] decimal default(0)
END
GO

IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_bill_payment_transactions_detail'
AND COLUMN_NAME = 'batch_convenience_fee')
BEGIN
	ALTER TABLE tbl_bill_payment_transactions_detail ADD [batch_convenience_fee] decimal default(0)
END
GO



/****** Object:  StoredProcedure [dbo].[sp_get_batch_upload_summaries_by_user_id]    Script Date: 06/06/2020 12:57:10 PM ******/

IF (OBJECT_ID('sp_get_batch_upload_summaries_by_user_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summaries_by_user_id
END
GO


CREATE PROCEDURE [dbo].[sp_get_batch_upload_summaries_by_user_id]
@user_id bigint
AS
	SELECT * FROM tbl_transactions_summary(NOLOCK) 
	WHERE userid = @user_id and (upload_successful = 'true' or transaction_status = 'Pending Validation')  
	ORDER BY Id desc;
GO


/****** Object:  StoredProcedure [dbo].[sp_get_batch_upload_summary_by_batch_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_batch_upload_summary_by_batch_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summary_by_batch_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_batch_upload_summary_by_batch_id]
@batch_id NVARCHAR (100)
AS
	SELECT * FROM tbl_transactions_summary(NOLOCK) WHERE batch_id = @batch_id;
GO

/****** Object:  StoredProcedure [dbo].[sp_get_batch_upload_summary_id_by_batch_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_batch_upload_summary_id_by_batch_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summary_id_by_batch_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_batch_upload_summary_id_by_batch_id]
@batch_id NVARCHAR (100)
AS
	SELECT Id 
	FROM tbl_transactions_summary (NOLOCK)
	WHERE batch_id = @batch_id;
GO

/****** Object:  StoredProcedure [dbo].[sp_get_bill_payments_status_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_bill_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_bill_payments_status_by_transactions_summary_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_bill_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int
AS
			SELECT [product_code],[item_code],[customer_id],[amount],[error],[row_status],[row_num]
			FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
				  FROM      tbl_bill_payment_transactions_detail (NOLOCK)
				  WHERE     transactions_summary_id = @transactions_summary_id
				) AS RowConstrainedResult
			WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
			AND RowNum < @page_number * @page_size + 1
			ORDER BY RowNum
		
GO

/****** Object:  StoredProcedure [dbo].[sp_get_confirmed_bill_payments_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_confirmed_bill_payments_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_confirmed_bill_payments_by_transactions_summary_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_confirmed_bill_payments_by_transactions_summary_id]
@transactions_summary_id bigint
AS
	SELECT [row_num],[product_code],[item_code],[customer_id],[amount], [customer_name],[surcharge], 
	[batch_convenience_fee],[transaction_convenience_fee]
	FROM tbl_bill_payment_transactions_detail (NOLOCK)
	WHERE [transactions_summary_id] = @transactions_summary_id and row_status = 'Valid';
GO

/****** Object:  StoredProcedure [dbo].[sp_get_confirmed_firs_multitax_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_confirmed_firs_multitax_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_confirmed_firs_multitax_by_transactions_summary_id
END
GO



CREATE PROCEDURE [dbo].[sp_get_confirmed_firs_multitax_by_transactions_summary_id]
@transactions_summary_id bigint
AS
	SELECT 
		row_num,
		beneficiary_tin,
	beneficiary_name,
	beneficiary_address,
	contract_amount,
	contract_description,
	contract_date,
	contract_type,
	period_covered,
	wht_rate,
	wht_amount,
	invoice_number,
	amount,
	comment,
	document_number,
	tax_type,
	payer_tin,
	payer_name,
	batch_convenience_fee,
	transaction_convenience_fee
	FROM tbl_firs_multi_tax_transactions_detail (NOLOCK)
	WHERE [transactions_summary_id] = @transactions_summary_id and row_status = 'Valid';
GO

/****** Object:  StoredProcedure [dbo].[sp_get_confirmed_firs_wvat_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_confirmed_firs_wvat_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_confirmed_firs_wvat_by_transactions_summary_id
END
GO


CREATE PROCEDURE [dbo].[sp_get_confirmed_firs_wvat_by_transactions_summary_id]
@transactions_summary_id bigint
AS
	SELECT 
		row_num,
		contractor_name,
		contractor_address,
		contractor_tin,
		contract_description,
		transaction_currency,
		transaction_date,
		transaction_invoiced_value,
		nature_of_transaction,
		invoice_number,
		currency_exchange_rate,
		currency_invoiced_value,
		tax_account_number,
		wvat_rate,
		wvat_value
	FROM tbl_firs_wvat_transactions_detail (NOLOCK)
	WHERE [transactions_summary_id] = @transactions_summary_id and row_status = 'Valid';
GO


/****** Object:  StoredProcedure [dbo].[sp_get_firs_multitax_payments_status_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_firs_multitax_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_firs_multitax_payments_status_by_transactions_summary_id
END
GO



CREATE PROCEDURE [dbo].[sp_get_firs_multitax_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int,
@status int,
@tax_type nvarchar(50)
AS
	if(@tax_type = 'all')
		begin
		if(@status = 0)
			begin
				SELECT beneficiary_tin,
					beneficiary_name,
					beneficiary_address,
					contract_amount,
					contract_description,
					contract_date,
					contract_type,
					period_covered,
					wht_rate,
					wht_amount,
					invoice_number,
					amount,
					comment,
					document_number,
					tax_type,
					payer_tin,
					payer_name,
					batch_convenience_fee,
					transaction_convenience_fee,
					error,
					row_status,
					row_num
				FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
					  FROM      tbl_firs_multi_tax_transactions_detail(NOLOCK)
					  WHERE     transactions_summary_id = @transactions_summary_id
					) AS RowConstrainedResult
				WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
				AND RowNum < @page_number * @page_size + 1
				ORDER BY RowNum
			end
		else
			begin
				if(@status = 1)
					begin
						SELECT beneficiary_tin,
							beneficiary_name,
							beneficiary_address,
							contract_amount,
							contract_description,
							contract_date,
							contract_type,
							period_covered,
							wht_rate,
							wht_amount,
							invoice_number,
							amount,
							comment,
							document_number,
							tax_type,
							payer_tin,
							payer_name,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_multi_tax_transactions_detail(NOLOCK)
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid'
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
				else
					begin
						SELECT beneficiary_tin,
							beneficiary_name,
							beneficiary_address,
							contract_amount,
							contract_description,
							contract_date,
							contract_type,
							period_covered,
							wht_rate,
							wht_amount,
							invoice_number,
							amount,
							comment,
							document_number,
							tax_type,
							payer_tin,
							payer_name,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_multi_tax_transactions_detail(NOLOCK)
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid'
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
			end
		end
	else
		begin
			if(@status = 0)
			begin
				SELECT beneficiary_tin,
					beneficiary_name,
					beneficiary_address,
					contract_amount,
					contract_description,
					contract_date,
					contract_type,
					period_covered,
					wht_rate,
					wht_amount,
					invoice_number,
					amount,
					comment,
					document_number,
					tax_type,
					payer_tin,
					payer_name,
					error,
					row_status,
					row_num
				FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
					  FROM      tbl_firs_multi_tax_transactions_detail(NOLOCK)
					  WHERE     transactions_summary_id = @transactions_summary_id and tax_type = @tax_type
					) AS RowConstrainedResult
				WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
				AND RowNum < @page_number * @page_size + 1
				ORDER BY RowNum
			end
		else
			begin
				if(@status = 1)
					begin
						SELECT beneficiary_tin,
							beneficiary_name,
							beneficiary_address,
							contract_amount,
							contract_description,
							contract_date,
							contract_type,
							period_covered,
							wht_rate,
							wht_amount,
							invoice_number,
							amount,
							comment,
							document_number,
							tax_type,
							payer_tin,
							payer_name,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_multi_tax_transactions_detail(NOLOCK)
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid' and tax_type = @tax_type
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
				else
					begin
						SELECT beneficiary_tin,
							beneficiary_name,
							beneficiary_address,
							contract_amount,
							contract_description,
							contract_date,
							contract_type,
							period_covered,
							wht_rate,
							wht_amount,
							invoice_number,
							amount,
							comment,
							document_number,
							tax_type,
							payer_tin,
							payer_name,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_multi_tax_transactions_detail(NOLOCK)
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid' and tax_type = @tax_type
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
			end
		end
GO

/****** Object:  StoredProcedure [dbo].[sp_get_firs_wht_payments_status_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_firs_wht_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_firs_wht_payments_status_by_transactions_summary_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_firs_wht_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int,
@status int
AS
	if(@status = 0)
		begin
			SELECT [beneficiary_tin],[beneficiary_name],beneficiary_address,contract_amount,contract_description,contract_date,contract_type,period_covered,wht_rate,wht_amount,invoice_number,[error],[row_status],[row_num]
			FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
				  FROM      tbl_firs_wht_transactions_detail(NOLOCK)
				  WHERE     transactions_summary_id = @transactions_summary_id
				) AS RowConstrainedResult
			WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
			AND RowNum < @page_number * @page_size + 1
			ORDER BY RowNum
		end
	else
		begin
			if(@status = 1)
				begin
					SELECT [beneficiary_tin],[beneficiary_name],beneficiary_address,contract_amount,contract_description,contract_date,contract_type,period_covered,wht_rate,wht_amount,invoice_number,[error],[row_status],[row_num]
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_firs_wht_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
			else
				begin
					SELECT [beneficiary_tin],[beneficiary_name],beneficiary_address,contract_amount,contract_description,contract_date,contract_type,period_covered,wht_rate,wht_amount,invoice_number,[error],[row_status],[row_num]
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_firs_wht_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
		end
GO

/****** Object:  StoredProcedure [dbo].[sp_get_firs_wvat_payments_status_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_firs_wvat_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_firs_wvat_payments_status_by_transactions_summary_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_firs_wvat_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int,
@status int
AS
	if(@status = 0)
		begin
			SELECT 
					contractor_name,
					contractor_address,
					contractor_tin,
					contract_description,
					transaction_currency,
					transaction_date,
					transaction_invoiced_value,
					nature_of_transaction,
					invoice_number,
					currency_exchange_rate,
					currency_invoiced_value,
					tax_account_number,
					wvat_rate,
					wvat_value,
					error,
					row_status,
					row_num
			FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
				  FROM      tbl_firs_wvat_transactions_detail(NOLOCK)
				  WHERE     transactions_summary_id = @transactions_summary_id
				) AS RowConstrainedResult
			WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
			AND RowNum < @page_number * @page_size + 1
			ORDER BY RowNum
		end
	else
		begin
			if(@status = 1)
				begin
					SELECT 
						contractor_name,
						contractor_address,
						contractor_tin,
						contract_description,
						transaction_currency,
						transaction_date,
						transaction_invoiced_value,
						nature_of_transaction,
						invoice_number,
						currency_exchange_rate,
						currency_invoiced_value,
						tax_account_number,
						wvat_rate,
						wvat_value,
						error,
						row_status,
						row_num
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_firs_wvat_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
			else
				begin
					SELECT 
						contractor_name,
						contractor_address,
						contractor_tin,
						contract_description,
						transaction_currency,
						transaction_date,
						transaction_invoiced_value,
						nature_of_transaction,
						invoice_number,
						currency_exchange_rate,
						currency_invoiced_value,
						tax_account_number,
						wvat_rate,
						wvat_value,
						error,
						row_status,
						row_num
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_firs_wvat_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
		end
GO

/****** Object:  StoredProcedure [dbo].[sp_get_payments_status_by_transactions_summary_id]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_get_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_payments_status_by_transactions_summary_id
END
GO



CREATE PROCEDURE [dbo].[sp_get_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int,
@status int
AS
	if(@status = 0)
		begin
			SELECT [product_code],[item_code],[customer_id],[amount],[error],[row_status],[row_num],[surcharge],[customer_name],[batch_convenience_fee],[transaction_convenience_fee]
			FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
				  FROM      tbl_bill_payment_transactions_detail(NOLOCK)
				  WHERE     transactions_summary_id = @transactions_summary_id
				) AS RowConstrainedResult
			WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
			AND RowNum < @page_number * @page_size + 1
			ORDER BY RowNum
		end
	else
		begin
			if(@status = 1)
				begin
					SELECT [product_code],[item_code],[customer_id],[amount],[error],[row_status],[row_num],[surcharge],[customer_name]
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_bill_payment_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
			else
				begin
					SELECT [product_code],[item_code],[customer_id],[amount],[error],[row_status],[row_num], [surcharge],[customer_name]
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_bill_payment_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
		end
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_bill_payment_transaction_summary]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_bill_payment_transaction_summary') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_bill_payment_transaction_summary
END
GO

CREATE PROCEDURE [dbo].[sp_insert_bill_payment_transaction_summary]
@batch_id nvarchar(100),
@status nvarchar(50),
@item_type nvarchar(50),
@num_of_records int,
@upload_date nvarchar(50),
@content_type nvarchar(256),
@nas_raw_file nvarchar(max),
@userid bigint,
@product_code nvarchar(50),
@product_name nvarchar(50),
@file_name nvarchar(50)
AS
	INSERT INTO tbl_transactions_summary(batch_id, transaction_status, item_type, num_of_records, upload_date, content_type, nas_raw_file, userid, product_code, product_name, name_of_file) 
	VALUES(@batch_id,@status,@item_type,@num_of_records,@upload_date,@content_type,@nas_raw_file,@userid, @product_code, @product_name, @file_name) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_bill_payments]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_bill_payments
END
GO

CREATE PROCEDURE [dbo].[sp_insert_bill_payments]
@product_code nvarchar(100),
@item_code nvarchar(100),
@customer_id nvarchar(100),
@amount float,
@transactions_summary_id bigint,
@row_num int,
@row_status nvarchar(50),
@created_date nvarchar(50)
AS
	INSERT INTO tbl_bill_payment_transactions_detail
	(product_code,
	item_code,
	customer_id,
	amount,
	transactions_summary_id,
	row_status,
	row_num,
	created_date) 
	VALUES(@product_code,@item_code,@customer_id,@amount,@transactions_summary_id,@row_status,@row_num,@created_date) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_invalid_bill_payments]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_invalid_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_bill_payments
END
GO

CREATE PROCEDURE [dbo].[sp_insert_invalid_bill_payments]
@product_code nvarchar(100),
@item_code nvarchar(100),
@customer_id nvarchar(100),
@amount nvarchar(100),
@transactions_summary_id bigint,
@row_num int,
@row_status nvarchar(50),
@created_date nvarchar(50),
@initial_validation_status nvarchar(50),
@error nvarchar(max)
AS
	INSERT INTO tbl_bill_payment_transactions_detail
	(product_code,
	item_code,
	customer_id,
	amount,
	transactions_summary_id,
	row_status,
	row_num,
	created_date,
	initial_validation_status,
	error) 
	VALUES(@product_code,
	@item_code,
	@customer_id,
	@amount,
	@transactions_summary_id,
	@row_status,
	@row_num,
	@created_date,
	@initial_validation_status,
	@error) 
	SELECT SCOPE_IDENTITY();
GO
/****** Object:  StoredProcedure [dbo].[sp_insert_invalid_fctirs_multitax]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_invalid_fctirs_multitax') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_fctirs_multitax
END
GO



CREATE PROCEDURE [dbo].[sp_insert_invalid_fctirs_multitax]
@product_code [nvarchar](50),
@item_code [nvarchar](50),
@customer_id [nvarchar](256),
@amount [nvarchar](50),
@desc [nvarchar](max),
@customer_name [nvarchar](max),
@phone_number [nvarchar](50),
@email [nvarchar](256),
@address [nvarchar](max),
@transactions_summary_id bigint,
@row_num int,
@row_status nvarchar(50),
@created_date nvarchar(50),
@initial_validation_status nvarchar(50),
@error nvarchar(max)
AS
	INSERT INTO tbl_fctirs_multi_tax_transactions_detail
	(product_code,
item_code,
customer_id,
amount,
tax_type,
customer_name,
phone_number,
email,
address_info,
	transactions_summary_id,
	row_status,
	row_num,
	created_date,
	initial_validation_status,
	error) 
	VALUES(@product_code,
@item_code,
@customer_id,
@amount,
@desc,
@customer_name,
@phone_number,
@email,
@address,
	@transactions_summary_id,
	@row_status,
	@row_num,
	@created_date,
	@initial_validation_status,
	@error) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_invalid_firs_multitax]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_invalid_firs_multitax') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_firs_multitax
END
GO

CREATE PROCEDURE [dbo].[sp_insert_invalid_firs_multitax]
@beneficiary_tin [nvarchar](256),
@beneficiary_name [nvarchar](256),
@beneficiary_address [nvarchar](256),
@contract_date [nvarchar](256),
@contract_amount [nvarchar](256),
@contract_description [nvarchar](256),
@invoice_number [nvarchar](256),
@contract_type [nvarchar](256),
@period_covered [nvarchar](256),
@wht_rate [nvarchar](256),
@wht_amount [nvarchar](256),
@amount [nvarchar](50),
@comment [nvarchar](max),
@document_number [nvarchar](50),
@tax_type [nvarchar](50),
@payer_tin [nvarchar](256), 
@transactions_summary_id bigint,
@row_num int,
@row_status nvarchar(50),
@created_date nvarchar(50),
@initial_validation_status nvarchar(50),
@error nvarchar(max)
AS
	INSERT INTO tbl_firs_multi_tax_transactions_detail
	(beneficiary_tin,
	beneficiary_name,
	beneficiary_address,
	contract_amount,
	contract_description,
	contract_date,
	contract_type,
	period_covered,
	wht_rate,
	wht_amount,
	invoice_number,
	amount,
	comment,
	document_number,
	tax_type,
	payer_tin,
	transactions_summary_id,
	row_status,
	row_num,
	created_date,
	initial_validation_status,
	error) 
	VALUES(@beneficiary_tin,
	@beneficiary_name,
	@beneficiary_address,
	@contract_amount,
	@contract_description,
	@contract_date,
	@contract_type,
	@period_covered,
	@wht_rate,
	@wht_amount,
	@invoice_number,
	@amount,
	@comment,
	@document_number,
	@tax_type,
	@payer_tin,
	@transactions_summary_id,
	@row_status,
	@row_num,
	@created_date,
	@initial_validation_status,
	@error) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_invalid_firs_wht]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_invalid_firs_wht') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_firs_wht
END
GO

CREATE PROCEDURE [dbo].[sp_insert_invalid_firs_wht]
@beneficiary_tin nvarchar(256),
@beneficiary_name nvarchar(256),
@beneficiary_address nvarchar(256),
@contract_amount nvarchar(256),
@contract_description  nvarchar(256),
@contract_date nvarchar(256),
@contract_type nvarchar(256),
@period_covered nvarchar(256),
@wht_rate nvarchar(256),
@wht_amount nvarchar(256),
@invoice_number nvarchar(256),
@transactions_summary_id bigint,
@row_num int,
@row_status nvarchar(50),
@created_date nvarchar(50),
@initial_validation_status nvarchar(50),
@error nvarchar(max)
AS
	INSERT INTO tbl_firs_wht_transactions_detail
	(beneficiary_tin,
	beneficiary_name,
	beneficiary_address,
	contract_amount,
	contract_description,
	contract_date,
	contract_type,
	period_covered,
	wht_rate,
	wht_amount,
	invoice_number,
	transactions_summary_id,
	row_status,
	row_num,
	created_date,
	initial_validation_status,
	error) 
	VALUES(@beneficiary_tin,
	@beneficiary_name,
	@beneficiary_address,
	@contract_amount,
	@contract_description,
	@contract_date,
	@contract_type,
	@period_covered,
	@wht_rate,
	@wht_amount,
	@invoice_number,
	@transactions_summary_id,
	@row_status,
	@row_num,
	@created_date,
	@initial_validation_status,
	@error) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_invalid_firs_wvat]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_invalid_firs_wvat') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_firs_wvat
END
GO

CREATE PROCEDURE [dbo].[sp_insert_invalid_firs_wvat]
@contractor_name nvarchar(256),
@contractor_address nvarchar(256),
@contractor_tin nvarchar(256),
@contract_description nvarchar(256),
@transaction_currency nvarchar(50),
@transaction_date nvarchar(256),
@transaction_invoiced_value nvarchar(50),
@nature_of_transaction nvarchar(256),
@invoice_number nvarchar(256),
@currency_exchange_rate nvarchar(10),
@currency_invoiced_value nvarchar(50),
@tax_account_number nvarchar(50),
@wvat_rate nvarchar(10),
@wvat_value nvarchar(10),
@transactions_summary_id bigint,
@row_num int,
@row_status nvarchar(50),
@created_date nvarchar(50),
@initial_validation_status nvarchar(50),
@error nvarchar(max)
AS
	INSERT INTO tbl_firs_wvat_transactions_detail
	(contractor_name,
	contractor_address,
	contractor_tin,
	contract_description,
	transaction_currency,
	transaction_date,
	transaction_invoiced_value,
	nature_of_transaction,
	invoice_number,
	currency_exchange_rate,
	currency_invoiced_value,
	tax_account_number,
	wvat_rate,
	wvat_value,
	transactions_summary_id,
	row_status,
	row_num,
	created_date,
	initial_validation_status,
	error) 
	VALUES(@contractor_name,
	@contractor_address,
	@contractor_tin,
	@contract_description,
	@transaction_currency,
	@transaction_date,
	@transaction_invoiced_value,
	@nature_of_transaction,
	@invoice_number,
	@currency_exchange_rate,
	@currency_invoiced_value,
	@tax_account_number,
	@wvat_rate,
	@wvat_value,
	@transactions_summary_id,
	@row_status,
	@row_num,
	@created_date,
	@initial_validation_status,
	@error) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_valid_bill_payments]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_valid_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_valid_bill_payments
END
GO
CREATE PROCEDURE [dbo].[sp_insert_valid_bill_payments]
@product_code nvarchar(100),
@item_code nvarchar(100),
@customer_id nvarchar(100),
@amount nvarchar(100),
@transactions_summary_id bigint,
@row_num int,
@created_date nvarchar(50),
@initial_validation_status nvarchar(50)
AS
	INSERT INTO tbl_bill_payment_transactions_detail
	(product_code,
	item_code,
	customer_id,
	amount,
	transactions_summary_id,
	row_num,
	created_date,
	initial_validation_status) 
	VALUES(@product_code,@item_code,@customer_id,@amount,@transactions_summary_id,@row_num,@created_date,@initial_validation_status) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_valid_fctirs_multitax]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_valid_fctirs_multitax') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_valid_fctirs_multitax
END
GO


                         

CREATE PROCEDURE [dbo].[sp_insert_valid_fctirs_multitax]
@product_code [nvarchar](50),
@item_code [nvarchar](50),
@customer_id [nvarchar](256),
@amount [nvarchar](50),
@desc [nvarchar](max),
@customer_name [nvarchar](max),
@phone_number [nvarchar](50),
@email [nvarchar](256),
@address [nvarchar](max),
@transactions_summary_id bigint,
@row_num int,
@created_date nvarchar(50),
@initial_validation_status nvarchar(50)
AS
	INSERT INTO tbl_fctirs_multi_tax_transactions_detail
	(product_code,
	item_code,
	customer_id,
	amount,
	tax_type,
	customer_name,
	phone_number,
	email,
	address_info,
	transactions_summary_id,
	row_num,
	created_date,
	initial_validation_status) 
	VALUES(@product_code,
	@item_code,
	@customer_id,
	@amount,
	@desc,
	@customer_name,
	@phone_number,
	@email,
	@address,
	@transactions_summary_id,
	@row_num,
	@created_date,
	@initial_validation_status) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_valid_firs_multitax]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_valid_firs_multitax') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_valid_firs_multitax
END
GO

                         

CREATE PROCEDURE [dbo].[sp_insert_valid_firs_multitax]
@beneficiary_tin [nvarchar](256),
@beneficiary_name [nvarchar](256),
@beneficiary_address [nvarchar](256),
@contract_date [nvarchar](256),
@contract_amount [nvarchar](256),
@contract_description [nvarchar](256),
@invoice_number [nvarchar](256),
@contract_type [nvarchar](256),
@period_covered [nvarchar](256),
@wht_rate [nvarchar](256),
@wht_amount [nvarchar](256),
@amount [nvarchar](50),
@comment [nvarchar](max),
@document_number [nvarchar](50),
@tax_type [nvarchar](50),
@payer_tin [nvarchar](256), 
@transactions_summary_id bigint,
@row_num int,
@created_date nvarchar(50),
@initial_validation_status nvarchar(50)
AS
	INSERT INTO tbl_firs_multi_tax_transactions_detail
	(beneficiary_tin,
	beneficiary_name,
	beneficiary_address,
	contract_amount,
	contract_description,
	contract_date,
	contract_type,
	period_covered,
	wht_rate,
	wht_amount,
	invoice_number,
	amount,
	comment,
	document_number,
	tax_type,
	payer_tin,
	transactions_summary_id,
	row_num,
	created_date,
	initial_validation_status) 
	VALUES(@beneficiary_tin,
	@beneficiary_name,
	@beneficiary_address,
	@contract_amount,
	@contract_description,
	@contract_date,
	@contract_type,
	@period_covered,
	@wht_rate,
	@wht_amount,
	@invoice_number,
	@amount,
	@comment,
	@document_number,
	@tax_type,
	@payer_tin,
	@transactions_summary_id,
	@row_num,
	@created_date,
	@initial_validation_status) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_valid_firs_wht]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_valid_firs_wht') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_valid_firs_wht
END
GO
                                         

CREATE PROCEDURE [dbo].[sp_insert_valid_firs_wht]
@beneficiary_tin nvarchar(256),
@beneficiary_name nvarchar(256),
@beneficiary_address nvarchar(256),
@contract_amount nvarchar(256),
@contract_description  nvarchar(256),
@contract_date nvarchar(256),
@contract_type nvarchar(256),
@period_covered nvarchar(256),
@wht_rate nvarchar(256),
@wht_amount nvarchar(256),
@invoice_number nvarchar(256),
@transactions_summary_id bigint,
@row_num int,
@created_date nvarchar(50),
@initial_validation_status nvarchar(50)
AS
	INSERT INTO tbl_firs_wht_transactions_detail
	(beneficiary_tin,
	beneficiary_name,
	beneficiary_address,
	contract_amount,
	contract_description,
	contract_date,
	contract_type,
	period_covered,
	wht_rate,
	wht_amount,
	invoice_number,
	transactions_summary_id,
	row_num,
	created_date,
	initial_validation_status) 
	VALUES(@beneficiary_tin,
	@beneficiary_name,
	@beneficiary_address,
	@contract_amount,
	@contract_description,
	@contract_date,
	@contract_type,
	@period_covered,
	@wht_rate,
	@wht_amount,
	@invoice_number,@transactions_summary_id,@row_num,@created_date,@initial_validation_status) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_insert_valid_firs_wvat]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_insert_valid_firs_wvat') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_valid_firs_wvat
END
GO


CREATE PROCEDURE [dbo].[sp_insert_valid_firs_wvat]
@contractor_name nvarchar(256),
@contractor_address nvarchar(256),
@contractor_tin nvarchar(256),
@contract_description nvarchar(256),
@transaction_currency nvarchar(50),
@transaction_date nvarchar(256),
@transaction_invoiced_value nvarchar(50),
@nature_of_transaction nvarchar(256),
@invoice_number nvarchar(256),
@currency_exchange_rate nvarchar(10),
@currency_invoiced_value nvarchar(50),
@tax_account_number nvarchar(50),
@wvat_rate nvarchar(10),
@wvat_value nvarchar(10),
@transactions_summary_id bigint,
@row_num int,
@created_date nvarchar(50),
@initial_validation_status nvarchar(50)
AS
	INSERT INTO tbl_firs_wvat_transactions_detail
	(contractor_name,
	contractor_address,
	contractor_tin,
	contract_description,
	transaction_currency,
	transaction_date,
	transaction_invoiced_value,
	nature_of_transaction,
	invoice_number,
	currency_exchange_rate,
	currency_invoiced_value,
	tax_account_number,
	wvat_rate,
	wvat_value,
	transactions_summary_id,
	row_num,
	created_date,
	initial_validation_status) 
	VALUES(@contractor_name,
	@contractor_address,
	@contractor_tin,
	@contract_description,
	@transaction_currency,
	@transaction_date,
	@transaction_invoiced_value,
	@nature_of_transaction,
	@invoice_number,
	@currency_exchange_rate,
	@currency_invoiced_value,
	@tax_account_number,
	@wvat_rate,
	@wvat_value,
	@transactions_summary_id,
	@row_num,
	@created_date,
	@initial_validation_status) 
	SELECT SCOPE_IDENTITY();
GO

/****** Object:  StoredProcedure [dbo].[sp_update_bill_payment_summary_status]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_bill_payment_summary_status') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payment_summary_status
END
GO

CREATE PROCEDURE [dbo].[sp_update_bill_payment_summary_status]
@batch_id NVARCHAR (256),
@status NVARCHAR (256),
@modified_date NVARCHAR (50)
AS
	UPDATE tbl_transactions_summary 
	SET transaction_status=@status, modified_date=@modified_date
	WHERE batch_id=@batch_id;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_bill_payment_upload_summary]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_bill_payment_upload_summary') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payment_upload_summary
END
GO


CREATE PROCEDURE [dbo].[sp_update_bill_payment_upload_summary]
@batch_id  NVARCHAR (256),
@num_of_valid_records INT,
@status NVARCHAR (50),
@modified_date NVARCHAR (50),
@nas_tovalidate_file NVARCHAR(MAX),
@valid_amount_sum  NVARCHAR (50)
AS
	UPDATE tbl_transactions_summary 
	SET num_of_valid_records=@num_of_valid_records, transaction_status=@status, modified_date=@modified_date, nas_tovalidate_file=@nas_tovalidate_file, valid_amount_sum=@valid_amount_sum
	OUTPUT INSERTED.Id
	WHERE batch_id=@batch_id;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_bill_payments]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payments
END
GO


CREATE PROCEDURE [dbo].[sp_update_bill_payments]
@transactions_summary_id BIGINT,
@error NVARCHAR (max),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_bill_payment_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and  row_num=@row_num;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_bill_payments_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_bill_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payments_detail
END
GO


CREATE PROCEDURE [dbo].[sp_update_bill_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_bill_payment_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_bill_payments_detail_enterprise_error]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_bill_payments_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payments_detail_enterprise_error
END
GO




CREATE PROCEDURE [dbo].[sp_update_bill_payments_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_bill_payment_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

/****** Object:  StoredProcedure [dbo].[sp_update_fctirs_multitax_detail_enterprise_error]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_fctirs_multitax_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_fctirs_multitax_detail_enterprise_error
END
GO


CREATE PROCEDURE [dbo].[sp_update_fctirs_multitax_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_fctirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

/****** Object:  StoredProcedure [dbo].[sp_update_fctirs_multitax_payments_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_fctirs_multitax_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_fctirs_multitax_payments_detail
END
GO




CREATE PROCEDURE [dbo].[sp_update_fctirs_multitax_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_fctirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_firs_multitax_detail_enterprise_error]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_firs_multitax_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_multitax_detail_enterprise_error
END
GO




CREATE PROCEDURE [dbo].[sp_update_firs_multitax_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

/****** Object:  StoredProcedure [dbo].[sp_update_firs_multitax_payments_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_firs_multitax_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_multitax_payments_detail
END
GO




CREATE PROCEDURE [dbo].[sp_update_firs_multitax_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_firs_wht_detail_enterprise_error]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_firs_wht_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wht_detail_enterprise_error
END
GO





CREATE PROCEDURE [dbo].[sp_update_firs_wht_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wht_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

/****** Object:  StoredProcedure [dbo].[sp_update_firs_wht_payments_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_firs_wht_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wht_payments_detail
END
GO




CREATE PROCEDURE [dbo].[sp_update_firs_wht_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wht_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_firs_wvat_detail_enterprise_error]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_firs_wvat_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wvat_detail_enterprise_error
END
GO




CREATE PROCEDURE [dbo].[sp_update_firs_wvat_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wvat_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

/****** Object:  StoredProcedure [dbo].[sp_update_firs_wvat_payments_detail]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_firs_wvat_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wvat_payments_detail
END
GO


CREATE PROCEDURE [dbo].[sp_update_firs_wvat_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wvat_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

/****** Object:  StoredProcedure [dbo].[sp_update_successful_upload]    Script Date: 06/06/2020 12:57:10 PM ******/
IF (OBJECT_ID('sp_update_successful_upload') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_successful_upload
END
GO

CREATE PROCEDURE [dbo].[sp_update_successful_upload]
@batch_id  NVARCHAR (256),
@nas_uservalidationfile nvarchar(max)
AS
	UPDATE tbl_transactions_summary 
	SET upload_successful='true', nas_uservalidationfile = @nas_uservalidationfile
	WHERE batch_id=@batch_id;
GO

IF (OBJECT_ID('sp_insert_payment_transaction_summary') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_payment_transaction_summary
END
GO

								
CREATE PROCEDURE [dbo].[sp_insert_payment_transaction_summary]
@batch_id nvarchar(100),
@status nvarchar(50),
@item_type nvarchar(50),
@num_of_records int,
@upload_date nvarchar(50),
@content_type nvarchar(256),
@userid bigint,
@product_code nvarchar(50),
@product_name nvarchar(50),
@file_name nvarchar(50)
AS
	INSERT INTO tbl_transactions_summary(batch_id, transaction_status, item_type, num_of_records, upload_date, content_type, userid, product_code, product_name, name_of_file) 
	VALUES(@batch_id,@status,@item_type,@num_of_records,@upload_date,@content_type,@userid, @product_code, @product_name, @file_name) 
	SELECT SCOPE_IDENTITY();
GO


