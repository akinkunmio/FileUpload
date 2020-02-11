
IF (OBJECT_ID('sp_insert_bill_payments') IS NOT NULL)
BEGIN
	DROP PROCEDURE usp_fetch_saved_payment_collections_by_business
END
GO


IF (OBJECT_ID('sp_insert_bill_payment_transaction_summary') IS NOT NULL)
BEGIN
	DROP PROCEDURE usp_fetch_saved_payments_by_collection
END
GO

CREATE PROCEDURE [dbo].[sp_insert_bill_payment_transaction_summary]
@batch_id nvarchar(100),
@status nvarchar(50),
@item_type nvarchar(50),
@num_of_records int,
@upload_date nvarchar(50),
@uploaded_by nvarchar(256),
@content_type nvarchar(256)
AS
	INSERT INTO tbl_transactions_summary(batch_id,transaction_status,item_type,num_of_records,upload_date,uploaded_by,content_type) 
VALUES(@batch_id,@status,@item_type,@num_of_records,@upload_date,@uploaded_by,@content_type) SELECT SCOPE_IDENTITY();
GO

CREATE PROCEDURE [dbo].[sp_insert_bill_payments]
@product_code nvarchar(100),
@item_code nvarchar(100),
@customer_id nvarchar(100),
@amount bigint,
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
