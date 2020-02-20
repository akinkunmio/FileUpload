
IF (OBJECT_ID('sp_insert_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_bill_payments
END
GO


IF (OBJECT_ID('sp_insert_bill_payment_transaction_summary') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_insert_bill_payment_transaction_summary
END
GO

IF (OBJECT_ID('sp_get_batch_upload_summary_id_by_batch_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summary_id_by_batch_id
END
GO


IF (OBJECT_ID('sp_get_bill_payments_status_by_batch_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_bill_payments_status_by_batch_id
END
GO

IF (OBJECT_ID('sp_get_batch_upload_summary_by_batch_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_batch_upload_summary_by_batch_id
END
GO

IF (OBJECT_ID('sp_get_bill_payments_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_bill_payments_by_transactions_summary_id
END
GO

IF (OBJECT_ID('sp_get_bill_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_get_bill_payments_status_by_transactions_summary_id
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



CREATE PROCEDURE [dbo].[sp_get_batch_upload_summary_by_batch_id]
@batch_id NVARCHAR (100)
AS
	SELECT * FROM tbl_transactions_summary WHERE batch_id = @batch_id;
GO


CREATE PROCEDURE [dbo].[sp_get_confirmed_bill_payments_by_transactions_summary_id]
@transactions_summary_id bigint,
@status NVARCHAR(50)
AS
	SELECT [row],[product_code],[item_code],[customer_id],[amount] 
	FROM tbl_bill_payment_transactions_detail 
	WHERE [transactions_summary_id] = @transactions_summary_id AND [row_status] = @status;
GO

CREATE PROCEDURE [dbo].[sp_get_bill_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint
AS
	SELECT [error],[row_status],[row_num] 
	FROM tbl_bill_payment_transactions_detail 
	WHERE transactions_summary_id = @transactions_summary_id;
GO

CREATE PROCEDURE [dbo].[sp_get_batch_upload_summary_id_by_batch_id]
@batch_id NVARCHAR (100)
AS
	SELECT transactions_summary_id 
	FROM tbl_transactions_summary 
	WHERE batch_id = @batch_id;
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
@error NVARCHAR (256),
@row_num INT,
@row_status NVARCHAR (50)
AS
	UPDATE tbl_bill_payment_transactions_detail SET error=@error, row_num=@row_num, row_status=@row_status
	WHERE transactions_summary_id=@transactions_summary_id;
GO

CREATE PROCEDURE [dbo].[sp_insert_bill_payment_transaction_summary]
@batch_id nvarchar(100),
@status nvarchar(50),
@item_type nvarchar(50),
@num_of_records int,
@upload_date nvarchar(50),
@content_type nvarchar(256)
AS
	INSERT INTO tbl_transactions_summary(batch_id,transaction_status,item_type,num_of_records,upload_date,content_type) 
VALUES(@batch_id,@status,@item_type,@num_of_records,@upload_date,@content_type) SELECT SCOPE_IDENTITY();
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
VALUES(@product_code,@item_code,@customer_id,@amount,@transactions_summary_id,@row_status,@row_num,@created_date) SELECT SCOPE_IDENTITY();
GO
