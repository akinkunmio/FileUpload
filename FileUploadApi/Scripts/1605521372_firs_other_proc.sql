IF OBJECT_ID('tbl_firs_other_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_firs_other_transactions_detail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[amount] [nvarchar](10) NULL,
	[comment] [nvarchar](256) NULL,
	[document_number] [nvarchar](256) NULL,
	[customer_name] [nvarchar](256) NULL,
	[customer_tin] [nvarchar](256) NULL,
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

IF (OBJECT_ID('sp_insert_invalid_firs_other') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_firs_other
END
GO

CREATE PROCEDURE [dbo].[sp_insert_invalid_firs_other]
@amount nvarchar(10),
@comment nvarchar(256),
@document_number nvarchar(256),
@customer_name nvarchar(256),
@customer_tin nvarchar(50),
@transactions_summary_id bigint,
@row_num int,
@row_status nvarchar(50),
@created_date nvarchar(50),
@initial_validation_status nvarchar(50),
@error nvarchar(max)
AS
	INSERT INTO tbl_firs_other_transactions_detail
	(amount,
	comment,
	document_number,
	customer_name,
	customer_tin,
	transactions_summary_id,
	row_status,
	row_num,
	created_date,
	initial_validation_status,
	error) 
	VALUES(@amount,
	@comment,
	@document_number,
	@customer_name,
	@customer_tin,
	@transactions_summary_id,
	@row_status,
	@row_num,
	@created_date,
	@initial_validation_status,
	@error) 
	SELECT SCOPE_IDENTITY();
GO

IF (OBJECT_ID('sp_insert_valid_firs_other') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_valid_firs_other
END
GO


CREATE PROCEDURE [dbo].[sp_insert_valid_firs_other]
@amount nvarchar(10),
@comment nvarchar(256),
@document_number nvarchar(256),
@customer_name nvarchar(256),
@customer_tin nvarchar(50),
@transactions_summary_id bigint,
@row_num int,
@created_date nvarchar(50),
@initial_validation_status nvarchar(50)
AS
	INSERT INTO tbl_firs_other_transactions_detail
	(amount,
	comment,
	document_number,
	customer_name,
	customer_tin,
	transactions_summary_id,
	row_num,
	created_date,
	initial_validation_status) 
	VALUES(@amount,
	@comment,
	@document_number,
	@customer_name,
	@customer_tin,
	@transactions_summary_id,
	@row_num,
	@created_date,
	@initial_validation_status) 
	SELECT SCOPE_IDENTITY();
GO

IF (OBJECT_ID('sp_get_confirmed_firs_other_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_confirmed_firs_other_by_transactions_summary_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_confirmed_firs_other_by_transactions_summary_id]
@transactions_summary_id bigint
AS
	SELECT 
		row_num,
		amount,
		comment,
		document_number,
		customer_name,
		customer_tin
	FROM tbl_firs_other_transactions_detail (NOLOCK)
	WHERE [transactions_summary_id] = @transactions_summary_id and row_status = 'Valid';
GO

IF (OBJECT_ID('sp_get_firs_other_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_firs_other_payments_status_by_transactions_summary_id
END
GO

CREATE PROCEDURE [dbo].[sp_get_firs_other_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int,
@status int
AS
	if(@status = 0)
		begin
			SELECT 
					amount,
					comment,
					document_number,
					customer_name,
					customer_tin,
					error,
					row_status,
					row_num
			FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
				  FROM      tbl_firs_other_transactions_detail(NOLOCK)
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
						amount,
						comment,
						document_number,
						customer_name,
						customer_tin,
						error,
						row_status,
						row_num
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_firs_other_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
			else
				begin
					SELECT 
						amount,
						comment,
						document_number,
						customer_name,
						customer_tin,
						error,
						row_status,
						row_num
					FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
						  FROM      tbl_firs_other_transactions_detail(NOLOCK)
						  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid'
						) AS RowConstrainedResult
					WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
					AND RowNum < @page_number * @page_size + 1
					ORDER BY RowNum
				end
		end
GO

IF (OBJECT_ID('sp_update_firs_other_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_other_payments_detail
END
GO


CREATE PROCEDURE [dbo].[sp_update_firs_other_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_other_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

IF (OBJECT_ID('sp_update_firs_other_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_other_detail_enterprise_error
END
GO


CREATE PROCEDURE [dbo].[sp_update_firs_other_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_other_transactions_detail 
	SET error=@error, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO