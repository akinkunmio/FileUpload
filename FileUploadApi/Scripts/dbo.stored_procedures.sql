
IF (OBJECT_ID('sp_get_batch_upload_summary_id_by_batch_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summary_id_by_batch_id
END
GO

IF (OBJECT_ID('sp_get_batch_upload_summaries_by_user_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summaries_by_user_id
END
GO

IF (OBJECT_ID('sp_get_batch_upload_summary_by_batch_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summary_by_batch_id
END
GO

IF (OBJECT_ID('sp_get_bill_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_bill_payments_status_by_transactions_summary_id
END
GO


IF (OBJECT_ID('sp_get_bill_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_bill_payments_status_by_transactions_summary_id
END
GO

IF (OBJECT_ID('sp_get_confirmed_bill_payments_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_confirmed_bill_payments_by_transactions_summary_id
END
GO

IF (OBJECT_ID('sp_insert_bill_payment_transaction_summary') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_bill_payment_transaction_summary
END
GO

IF (OBJECT_ID('sp_insert_valid_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_valid_bill_payments
END
GO

IF (OBJECT_ID('sp_insert_invalid_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_invalid_bill_payments
END
GO


IF (OBJECT_ID('sp_update_bill_payment_summary_status') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payment_summary_status
END
GO

IF (OBJECT_ID('sp_update_bill_payment_upload_summary') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payment_upload_summary
END
GO

IF (OBJECT_ID('sp_update_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payments
END
GO

IF (OBJECT_ID('sp_update_successful_upload') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_successful_upload
END
GO

IF OBJECT_ID('tbl_bill_payment_transactions_detail','U') IS NULL 
begin
CREATE TABLE [dbo].[tbl_bill_payment_transactions_detail](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[product_code] [nvarchar](100) NOT NULL,
	[item_code] [nvarchar](100) NOT NULL,
	[customer_id] [nvarchar](100) NOT NULL,
	[amount] [float] NOT NULL,
	[error] [nvarchar](256) NULL,
	[row_status] [nvarchar](50) NULL,
	[created_date] [nvarchar](50) NULL,
	[modified_date] [nvarchar](50) NULL,
	[row_num] [int] NULL,
	[transactions_summary_id] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO

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
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
GO

CREATE PROCEDURE [dbo].[sp_get_batch_upload_summary_by_batch_id]
@batch_id NVARCHAR (100)
AS
	SELECT * FROM tbl_transactions_summary WHERE batch_id = @batch_id;
GO

CREATE PROCEDURE [dbo].[sp_get_batch_upload_summaries_by_user_id]
@userid bigint
AS
	SELECT * FROM tbl_transactions_summary WHERE userid = @user_id;
GO


CREATE PROCEDURE [dbo].[sp_get_batch_upload_summary_id_by_batch_id]
@batch_id NVARCHAR (100)
AS
	SELECT Id 
	FROM tbl_transactions_summary 
	WHERE batch_id = @batch_id;
GO

CREATE PROCEDURE [dbo].[sp_get_bill_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int
AS
	
	SELECT [product_code],[item_code],[customer_id],[amount],[error],[row_status],[row_num]
	FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
          FROM      tbl_bill_payment_transactions_detail
          WHERE     transactions_summary_id = @transactions_summary_id
        ) AS RowConstrainedResult
	WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
    AND RowNum < @page_number * @page_size + 1
	ORDER BY RowNum
GO


CREATE PROCEDURE [dbo].[sp_get_confirmed_bill_payments_by_transactions_summary_id]
@transactions_summary_id bigint
AS
	SELECT [row_num],[product_code],[item_code],[customer_id],[amount] 
	FROM tbl_bill_payment_transactions_detail 
	WHERE [transactions_summary_id] = @transactions_summary_id and row_status = 'Valid';
GO

								
CREATE PROCEDURE [dbo].[sp_insert_bill_payment_transaction_summary]
@batch_id nvarchar(100),
@status nvarchar(50),
@item_type nvarchar(50),
@num_of_records int,
@upload_date nvarchar(50),
@content_type nvarchar(256),
@nas_raw_file nvarchar(max)
AS
	INSERT INTO tbl_transactions_summary(batch_id, transaction_status, item_type, num_of_records, upload_date, content_type, nas_raw_file) 
	VALUES(@batch_id,@status,@item_type,@num_of_records,@upload_date,@content_type,@nas_raw_file) 
	SELECT SCOPE_IDENTITY();
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


CREATE PROCEDURE [dbo].[sp_update_bill_payment_summary_status]
@batch_id NVARCHAR (256),
@status NVARCHAR (256),
@modified_date NVARCHAR (50)
AS
	UPDATE tbl_transactions_summary 
	SET transaction_status=@status, modified_date=@modified_date
	WHERE batch_id=@batch_id;
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

CREATE PROCEDURE [dbo].[sp_update_bill_payment_upload_summary]
@batch_id  NVARCHAR (256),
@num_of_valid_records INT,
@status NVARCHAR (50),
@modified_date NVARCHAR (50),
@nas_tovalidate_file NVARCHAR(MAX)
AS
	UPDATE tbl_transactions_summary 
	SET num_of_valid_records=@num_of_valid_records, transaction_status=@status, modified_date=@modified_date, nas_tovalidate_file=@nas_tovalidate_file 
	OUTPUT INSERTED.Id
	WHERE batch_id=@batch_id;
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

CREATE PROCEDURE [dbo].[sp_update_successful_upload]
@batch_id  NVARCHAR (256),
@user_id bigint
AS
	UPDATE tbl_transactions_summary 
	SET upload_successful='true', userid=@user_id
	WHERE batch_id=@batch_id;
GO



