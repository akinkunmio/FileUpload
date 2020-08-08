IF (OBJECT_ID('sp_update_bill_payments_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_bill_payments_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_bill_payments_detail_enterprise_error]
@transactions_summary_id BIGINT,
@surcharge decimal,
@batchFee decimal,
@transactionFee decimal,
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
@surcharge decimal,
@batchFee decimal,
@transactionFee decimal,
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
@surcharge decimal,
@batchFee decimal,
@transactionFee decimal,
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
	@surcharge decimal,
	@batchFee decimal,
	@transactionFee decimal,
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
@surcharge decimal,
@batchFee decimal,
@transactionFee decimal,
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
@surcharge decimal,
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_multi_tax_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName
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
@surcharge decimal,
@batchFee decimal,
@transactionFee decimal,
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
@surcharge decimal,
@batchFee decimal,
@transactionFee decimal,
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