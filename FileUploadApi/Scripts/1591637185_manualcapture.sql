IF (OBJECT_ID('sp_get_confirmed_fctirs_multitax_by_transactions_summary_id') IS NOT NULL)
BEGIN
    DROP PROCEDURE sp_get_confirmed_fctirs_multitax_by_transactions_summary_id
END
GO

IF (OBJECT_ID('sp_get_fctirs_multitax_payments_status_by_transactions_summary_id') IS NOT NULL)
BEGIN
    DROP PROCEDURE sp_get_fctirs_multitax_payments_status_by_transactions_summary_id
END
GO


CREATE PROCEDURE sp_get_confirmed_fctirs_multitax_by_transactions_summary_id
@transactions_summary_id bigint
AS
	SELECT [row_num],[product_code],[item_code],[customer_id],[amount] 
	FROM tbl_fctirs_multi_tax_transactions_detail 
	WHERE transactions_summary_id = @transactions_summary_id and row_status = 'Valid';
GO


CREATE PROCEDURE [dbo].[sp_get_fctirs_multitax_payments_status_by_transactions_summary_id]
@transactions_summary_id bigint,
@page_size int,
@page_number int,
@status int,
@tax_type nvarchar = null
AS
		if(@status = 0)
			begin
				SELECT product_code,
					item_code,
					customer_id,
					amount,
					tax_type,
					customer_name,
					phone_number,
					email,
					address_info,
					error,
					row_status,
					row_num
				FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
					  FROM      tbl_fctirs_multi_tax_transactions_detail
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
						SELECT product_code,
							item_code,
							customer_id,
							amount,
							tax_type,
							customer_name,
							phone_number,
							email,
							address_info,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_fctirs_multi_tax_transactions_detail
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid'
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
				else
					begin
						SELECT product_code,
							item_code,
							customer_id,
							amount,
							tax_type,
							customer_name,
							phone_number,
							email,
							address_info,
							error,
							row_status,
							row_num,
							error,
							row_status,
							row_num
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_fctirs_multi_tax_transactions_detail
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid'
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
	end
