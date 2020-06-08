IF (OBJECT_ID('sp_get_confirmed_fctirs_multitax_by_transactions_summary_id') IS NOT NULL)
BEGIN
    DROP PROCEDURE sp_get_confirmed_fctirs_multitax_by_transactions_summary_id
END
GO


CREATE PROCEDURE sp_get_confirmed_fctirs_multitax_by_transactions_summary_id
@transactions_summary_id bigint
AS
	SELECT [row_num],[product_code],[item_code],[customer_id],[amount] 
	FROM tbl_fctirs_multi_tax_transactions_detail 
	WHERE transactions_summary_id = @transactions_summary_id and row_status = 'Valid';
GO



