IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_transactions_summary'
AND COLUMN_NAME = 'businessid')
BEGIN
	ALTER TABLE tbl_transactions_summary ADD [businessid] bigint NOT NULL DEFAULT(0)
END
GO


IF NOT EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_transactions_summary'
AND COLUMN_NAME = 'businessid')
BEGIN
	ALTER TABLE tbl_transactions_summary ADD [businessid] bigint NOT NULL DEFAULT(0)
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_lirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'surcharge')
BEGIN
	ALTER TABLE tbl_lirs_multi_tax_transactions_detail ALTER COLUMN [surcharge] decimal(18, 4)
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_lirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'transaction_convenience_fee')
BEGIN
	ALTER TABLE tbl_lirs_multi_tax_transactions_detail ALTER COLUMN [transaction_convenience_fee] decimal(18, 4)
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_lirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'batch_convenience_fee')
BEGIN
	ALTER TABLE tbl_lirs_multi_tax_transactions_detail ALTER COLUMN [batch_convenience_fee] decimal(18, 4)
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_transactions_summary'
AND COLUMN_NAME = 'convenience_fee')
BEGIN
	ALTER TABLE tbl_transactions_summary ALTER COLUMN [convenience_fee] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_firs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'transaction_convenience_fee')
BEGIN
	ALTER TABLE tbl_firs_multi_tax_transactions_detail ALTER COLUMN [transaction_convenience_fee] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_firs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'batch_convenience_fee')
BEGIN
	ALTER TABLE tbl_firs_multi_tax_transactions_detail ALTER COLUMN [batch_convenience_fee] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_fctirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'surcharge')
BEGIN
	ALTER TABLE tbl_fctirs_multi_tax_transactions_detail ALTER COLUMN [surcharge] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_fctirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'transaction_convenience_fee')
BEGIN
	ALTER TABLE tbl_fctirs_multi_tax_transactions_detail ALTER COLUMN [transaction_convenience_fee] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_fctirs_multi_tax_transactions_detail'
AND COLUMN_NAME = 'batch_convenience_fee')
BEGIN
	ALTER TABLE tbl_fctirs_multi_tax_transactions_detail ALTER COLUMN [batch_convenience_fee] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_bill_payment_transactions_detail'
AND COLUMN_NAME = 'surcharge')
BEGIN
	ALTER TABLE tbl_bill_payment_transactions_detail ALTER COLUMN [surcharge] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_bill_payment_transactions_detail'
AND COLUMN_NAME = 'transaction_convenience_fee')
BEGIN
	ALTER TABLE tbl_bill_payment_transactions_detail ALTER COLUMN [transaction_convenience_fee] decimal(18, 4) 
END
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbl_bill_payment_transactions_detail'
AND COLUMN_NAME = 'batch_convenience_fee')
BEGIN
	ALTER TABLE tbl_bill_payment_transactions_detail ALTER COLUMN [batch_convenience_fee] decimal(18, 4) 
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





IF (OBJECT_ID('sp_update_bill_payments_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payments_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_bill_payments_detail_enterprise_error]
@transactions_summary_id BIGINT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@error NVARCHAR (Max),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_bill_payment_transactions_detail 
	SET error=@error, row_status=@row_status, customer_name = @customerName, surcharge = @surcharge, batch_convenience_fee=@batchFee
	, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

IF (OBJECT_ID('sp_update_firs_multitax_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_multitax_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_multitax_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName,batch_convenience_fee=@batchFee
	, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

IF (OBJECT_ID('sp_update_fctirs_multitax_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_fctirs_multitax_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_fctirs_multitax_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_fctirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status, surcharge = @surcharge,batch_convenience_fee=@batchFee
	, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

if OBJECT_ID('sp_update_lasg_multitax_detail_enterprise_error') IS NOT NULL
begin
    DROP PROCEDURE sp_update_lasg_multitax_detail_enterprise_error
end
GO

CREATE PROCEDURE sp_update_lasg_multitax_detail_enterprise_error
(
	@transactions_summary_id BIGINT,
	@error NVARCHAR (Max),
	@row_status NVARCHAR (50),
	@surcharge decimal(18, 4),
	@batchFee decimal(18, 4),
	@transactionFee decimal(18, 4),
@customerName varchar(250),
@webguid nvarchar(128) = ''
)
AS
BEGIN
	UPDATE tbl_lirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status,customer_id = @webguid, surcharge = @surcharge, customer_name = @customerName,
	batch_convenience_fee=@batchFee, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
END
GO

IF (OBJECT_ID('sp_update_bill_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payments_detail
END
GO

CREATE PROCEDURE [dbo].[sp_update_bill_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_bill_payment_transactions_detail 
	SET error=@error, row_status=@row_status,customer_name = @customerName, surcharge = @surcharge,
	batch_convenience_fee=@batchFee, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

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

IF (OBJECT_ID('sp_update_fctirs_multitax_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_fctirs_multitax_payments_detail
END
GO

CREATE PROCEDURE [dbo].[sp_update_fctirs_multitax_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_fctirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status, surcharge = @surcharge,batch_convenience_fee=@batchFee, 
	transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

if OBJECT_ID('sp_update_lasg_multitax_payments_detail') IS NOT NULL
begin
    DROP PROCEDURE sp_update_lasg_multitax_payments_detail
end
go

CREATE PROCEDURE sp_update_lasg_multitax_payments_detail
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50),
@webguid nvarchar(128) = ''
AS
BEGIN

	UPDATE tbl_lirs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status, customer_id = @webguid, surcharge = @surcharge, customer_name = @customerName,
	batch_convenience_fee=@batchFee, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;

END

go