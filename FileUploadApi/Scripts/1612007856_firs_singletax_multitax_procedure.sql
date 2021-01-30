IF (OBJECT_ID('sp_update_firs_multitax_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_multitax_payments_detail
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_multitax_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName, batch_convenience_fee=@batchFee, 
	transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO


IF OBJECT_ID('tbl_firs_single_tax_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_firs_single_tax_transactions_detail](
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
	[payer_name] [nvarchar](250) NULL,
	[transaction_convenience_fee] [decimal],
	[batch_convenience_fee] [decimal],
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

IF (OBJECT_ID('sp_insert_invalid_firs_singletax') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_firs_singletax
END
GO

CREATE PROCEDURE [dbo].[sp_insert_invalid_firs_singletax]
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
	INSERT INTO tbl_firs_single_tax_transactions_detail
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

IF (OBJECT_ID('sp_update_firs_singletax_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_singletax_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_singletax_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_single_tax_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

IF (OBJECT_ID('sp_update_firs_singletax_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_singletax_payments_detail
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_singletax_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_single_tax_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName, batch_convenience_fee=@batchFee, 
	transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

IF (OBJECT_ID('sp_get_firs_singletax_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_firs_singletax_payments_status_by_transactions_summary_id
END
GO



CREATE PROCEDURE [dbo].[sp_get_firs_singletax_payments_status_by_transactions_summary_id]
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
					  FROM      tbl_firs_single_tax_transactions_detail(NOLOCK)
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
							batch_convenience_fee,
							transaction_convenience_fee,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_single_tax_transactions_detail(NOLOCK)
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
							batch_convenience_fee,
							transaction_convenience_fee,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_single_tax_transactions_detail(NOLOCK)
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
					batch_convenience_fee,
					transaction_convenience_fee,
					error,
					row_status,
					row_num
				FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
					  FROM      tbl_firs_single_tax_transactions_detail(NOLOCK)
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
							batch_convenience_fee,
							transaction_convenience_fee,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_single_tax_transactions_detail(NOLOCK)
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
							batch_convenience_fee,
							transaction_convenience_fee,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_firs_single_tax_transactions_detail(NOLOCK)
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid' and tax_type = @tax_type
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
			end
		end
GO


IF (OBJECT_ID('sp_get_confirmed_firs_singletax_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_confirmed_firs_singletax_by_transactions_summary_id
END
GO



CREATE PROCEDURE [dbo].[sp_get_confirmed_firs_singletax_by_transactions_summary_id]
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
	FROM tbl_firs_single_tax_transactions_detail (NOLOCK)
	WHERE [transactions_summary_id] = @transactions_summary_id and row_status = 'Valid';
GO