﻿IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_transactions_summary'
AND COLUMN_NAME = 'businessid')
BEGIN
	ALTER TABLE tbl_transactions_summary ADD [businessid] bigint NOT NULL DEFAULT(0)
END
GO


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
@businessid bigint,
@product_code nvarchar(50),
@product_name nvarchar(50),
@file_name nvarchar(50)
AS
	INSERT INTO tbl_transactions_summary(batch_id, transaction_status, item_type, num_of_records, upload_date, content_type, nas_raw_file, userid,businessid, product_code, product_name, name_of_file) 
	VALUES(@batch_id,@status,@item_type,@num_of_records,@upload_date,@content_type,@nas_raw_file,@userid,@businessid, @product_code, @product_name, @file_name) 
	SELECT SCOPE_IDENTITY();
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
@businessid bigint,
@product_code nvarchar(50),
@product_name nvarchar(50),
@file_name nvarchar(50)
AS
	INSERT INTO tbl_transactions_summary(batch_id, transaction_status, item_type, num_of_records, upload_date, content_type, userid,businessid, product_code, product_name, name_of_file) 
	VALUES(@batch_id,@status,@item_type,@num_of_records,@upload_date,@content_type,@userid,@businessid, @product_code, @product_name, @file_name) 
	SELECT SCOPE_IDENTITY();
GO