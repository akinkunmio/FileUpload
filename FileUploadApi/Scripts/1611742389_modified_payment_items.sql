IF (OBJECT_ID('sp_update_firs_other_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_other_payments_detail
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_other_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_other_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName, batch_convenience_fee=@batchFee, 
	transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

IF (OBJECT_ID('sp_update_firs_wvat_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wvat_payments_detail
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_wvat_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wvat_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName, batch_convenience_fee=@batchFee, 
	transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO

IF (OBJECT_ID('sp_update_firs_wht_payments_detail') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wht_payments_detail
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_wht_payments_detail]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@row_num INT,
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wht_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName, batch_convenience_fee=@batchFee, 
	transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and row_num=@row_num;
GO


IF (OBJECT_ID('sp_update_firs_wht_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wht_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_wht_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wht_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName,batch_convenience_fee=@batchFee
	, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO


IF (OBJECT_ID('sp_update_firs_wvat_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_wvat_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_wvat_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_wvat_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName,batch_convenience_fee=@batchFee
	, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

IF (OBJECT_ID('sp_update_firs_other_detail_enterprise_error') IS NOT NULL)
BEGIN
	DROP PROCEDURE sp_update_firs_other_detail_enterprise_error
END
GO

CREATE PROCEDURE [dbo].[sp_update_firs_other_detail_enterprise_error]
@transactions_summary_id BIGINT,
@error NVARCHAR (Max),
@surcharge decimal(18, 4),
@batchFee decimal(18, 4),
@transactionFee decimal(18, 4),
@customerName varchar(250),
@row_status NVARCHAR (50)
AS
	UPDATE tbl_firs_other_transactions_detail 
	SET error=@error, row_status=@row_status, payer_name = @customerName,batch_convenience_fee=@batchFee
	, transaction_convenience_fee  = @transactionFee
	WHERE transactions_summary_id=@transactions_summary_id and initial_validation_status = 'Valid';
GO

