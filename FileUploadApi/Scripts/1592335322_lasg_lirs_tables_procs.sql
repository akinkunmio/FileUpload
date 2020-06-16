go
IF OBJECT_ID('tbl_lirs_multi_tax_transactions_detail') IS NULL
BEGIN
	CREATE TABLE [tbl_lirs_multi_tax_transactions_detail](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[product_code] [nvarchar](50) NULL,
		[item_code] [nvarchar](50) NULL,
		[customer_id] [nvarchar](256) NULL,
		[payer_id] [nvarchar](256) NULL,
		[agency_code] [nvarchar](50) NULL,
		[revenue_code] [nvarchar](50) NULL,
		[start_period] [nvarchar](12) NULL,
		[end_period] [nvarchar](12) NULL,
		[narration] [nvarchar](50) NULL,
		[amount] [nvarchar](50) NULL,
		[tax_type] [nvarchar](128) NULL,
		[customer_name] [nvarchar](128) NULL,
		[error] [nvarchar](max) NULL,
		[row_status] [nvarchar](50) NULL,
		[created_date] [nvarchar](50) NULL,
		[modified_date] [nvarchar](50) NULL,
		[row_num] [int] NULL,
		[transactions_summary_id] [bigint] NULL,
		[initial_validation_status] [nvarchar](50) NULL
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

GO
go

if OBJECT_ID('sp_update_lasg_multitax_payments_detail') IS NOT NULL
begin
    DROP PROCEDURE sp_update_lasg_multitax_payments_detail
end
go

CREATE PROCEDURE sp_update_lasg_multitax_payments_detail
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@row_status NVARCHAR (50),
@webguid nvarchar(128) = ''
AS
BEGIN

	UPDATE tbl_lirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status, customer_id = @webguid
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;

END

go

if OBJECT_ID('sp_update_lasg_multitax_detail_enterprise_error') IS NOT NULL
begin
    DROP PROCEDURE sp_update_lasg_multitax_detail_enterprise_error
end
GO

CREATE PROCEDURE sp_update_lasg_multitax_detail_enterprise_error
(
	@transactions_summary_id BIGINT,
	@error NVARCHAR (Max),
	@row_status NVARCHAR (50)
)
AS
BEGIN
	UPDATE tbl_lirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
END
GO

if OBJECT_ID('sp_insert_invalid_lirs_multitax') IS NOT NULL
begin
    DROP PROCEDURE sp_insert_invalid_lirs_multitax
end
go

CREATE procedure sp_insert_invalid_lirs_multitax
(
	@product_code nvarchar(50),
	@item_code nvarchar(50) NULL,
	@customer_id nvarchar(256) NULL,
	@payer_id nvarchar(128) NULL,
	@agency_code nvarchar(50),
	@revenue_code nvarchar(50),
	@start_period nvarchar(12),
	@end_period nvarchar(12),
	@narration nvarchar(50),
	@amount nvarchar(50) NULL,
	@tax_type nvarchar(128) NULL,
	@customer_name nvarchar(128) NULL,

	@error nvarchar(max) NULL,
	@row_status nvarchar(50) NULL,
	@created_date nvarchar(50) NULL,
	@modified_date nvarchar(50) NULL,
	@row_num int NULL,
	@transactions_summary_id bigint NULL,
	@initial_validation_status nvarchar(50) NULL
)
AS
BEGIN

	INSERT INTO tbl_lirs_multi_tax_transactions_detail
	(
		product_code,
		item_code,
		customer_id,
		payer_id,
		agency_code,
		revenue_code,
		start_period,
		end_period,
		narration,
		amount,
		tax_type,
		customer_name,
		error,
		row_status,
		created_date,
		modified_date,
		row_num,
		transactions_summary_id,
		initial_validation_status
	)
	VALUES
	(
		@product_code,
		@item_code,
		@customer_id,
		@payer_id,
		@agency_code,
		@revenue_code,
		@start_period,
		@end_period,
		@narration,
		@amount,
		@tax_type,
		@customer_name,
		@error,
		@row_status,
		@created_date,
		@modified_date,
		@row_num,
		@transactions_summary_id,
		@initial_validation_status
	)

	SELECT Scope_Identity()
END