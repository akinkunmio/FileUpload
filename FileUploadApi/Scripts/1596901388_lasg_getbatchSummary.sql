go

if OBJECT_ID('sp_get_lasg_multitax_payments_status_by_transactions_summary_id') IS NOT NULL
begin
    DROP PROCEDURE sp_get_lasg_multitax_payments_status_by_transactions_summary_id
end
go

if OBJECT_ID('sp_get_confirmed_lasg_multitax_by_transactions_summary_id') IS NOT NULL
begin
    DROP PROCEDURE sp_get_confirmed_lasg_multitax_by_transactions_summary_id
end
go

CREATE PROCEDURE sp_get_lasg_multitax_payments_status_by_transactions_summary_id
    @transactions_summary_id bigint,
    @page_size int,
    @page_number int,
    @status int,
    @tax_type nvarchar = null
AS
		if(@status = 0)
			begin
				SELECT 
					Id,
					product_code,
					item_code,
					customer_id,
					payer_id,
					agency_code,
					revenue_code,
					start_period,
					end_period,
					narration,
					amount,
					tax_type,
					customer_name,
					surcharge,
					batch_convenience_fee,
					transaction_convenience_fee,
					error,
					row_status,
					created_date,
					modified_date,
					row_num,
					transactions_summary_id,
					initial_validation_status
				FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
					  FROM      tbl_lirs_multi_tax_transactions_detail(NOLOCK)
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
							Id,
							product_code,
							item_code,
							customer_id,
							payer_id,
							agency_code,
							revenue_code,
							start_period,
							end_period,
							narration,
							amount,
							tax_type,
							customer_name,
							surcharge,
							error,
							row_status,
							created_date,
							modified_date,
							row_num,
							transactions_summary_id,
							initial_validation_status
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_lirs_multi_tax_transactions_detail(NOLOCK)
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Valid'
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
				else
					begin
						SELECT 
							Id,
							product_code,
							item_code,
							customer_id,
							payer_id,
							agency_code,
							revenue_code,
							start_period,
							end_period,
							narration,
							amount,
							tax_type,
							customer_name,
							surcharge,
							error,
							row_status,
							created_date,
							modified_date,
							row_num,
							transactions_summary_id,
							initial_validation_status
						FROM    ( SELECT   *, ROW_NUMBER() over (order by Id asc) AS RowNum
							  FROM      tbl_lirs_multi_tax_transactions_detail(NOLOCK)
							  WHERE     transactions_summary_id = @transactions_summary_id and row_status = 'Invalid'
							) AS RowConstrainedResult
						WHERE   RowNum >= ((@page_number * @page_size) - (@page_size)) + 1
						AND RowNum < @page_number * @page_size + 1
						ORDER BY RowNum
					end
	end
go


CREATe PROCEDURE sp_get_confirmed_lasg_multitax_by_transactions_summary_id
@transactions_summary_id bigint
AS
	SELECT [row_num],[product_code],[item_code],[customer_id],[amount], [customer_name], [surcharge],
	[batch_convenience_fee],[transaction_convenience_fee]
	FROM tbl_lirs_multi_tax_transactions_detail (NOLOCK)
	WHERE transactions_summary_id = @transactions_summary_id and row_status = 'Valid';
