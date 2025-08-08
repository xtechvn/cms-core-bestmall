using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL
{
    public class VoucherDAL : GenericService<Voucher>
    {

        private static DbWorker _DbWorker;
        public VoucherDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<Voucher> FindByVoucherCode(string voucherCode)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Code.ToUpper() == voucherCode.ToUpper());
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex);
                return null;
            }
        }

        public async Task<Voucher> FindByVoucherCode(string voucherCode, bool is_public = false)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Code.ToUpper() == voucherCode.ToUpper() && s.IsPublic == is_public);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex);
                return null;
            }
        }

        public async Task<List<Voucher>> getVoucherPublic(bool is_public)
        {
            try
            {
                var current_date = DateTime.Now;
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Vouchers.AsNoTracking().Where(s => s.IsPublic == is_public && s.Cdate <= current_date && s.EDate >= current_date).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getVoucherPublic - VoucherDAL: " + ex);
                return null;
            }
        }
        public async Task<Voucher> GetById(int voucherId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return await _DbContext.Vouchers.FirstOrDefaultAsync(s => s.Id == voucherId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - VoucherDAL: " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// Lấy danh sách voucher có phân trang và tìm kiếm.
        /// </summary>
        public async Task<DataTable> GetVoucherPagingList(string keyword, int status, int pageIndex, int pageSize)
        {
            try
            {
                SqlParameter[] objParam =
                [
                    new SqlParameter("@keyword", keyword ?? (object)DBNull.Value),
                    new SqlParameter("@status", status),
                    new SqlParameter("@page_index", pageIndex),
                    new SqlParameter("@page_size", pageSize),
                ];
                return _DbWorker.GetDataTable("SP_GetListVoucher", objParam);
            }
            catch (Exception ex)
            {
                // LogHelper.InsertLogTelegram("GetVoucherPagingList - VoucherDAL: " + ex);
                throw ex;
            }
        }

        /// <summary>
        /// Thêm mới một voucher.
        /// </summary>
        public async Task<long> InsertVoucher(Voucher model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[] {
                new SqlParameter("@code", model.Code),
                new SqlParameter("@cdate", model.Cdate),
                new SqlParameter("@udate", model.Udate),
                new SqlParameter("@eDate", model.EDate),
                new SqlParameter("@limitUse", model.LimitUse),
                new SqlParameter("@price_sales", model.PriceSales),
                new SqlParameter("@unit", model.Unit),
                new SqlParameter("@rule_type", model.RuleType),
                new SqlParameter("@group_user_priority", model.GroupUserPriority ?? (object)DBNull.Value),
                new SqlParameter("@is_public", model.IsPublic),
                new SqlParameter("@description", model.Description ?? (object)DBNull.Value),
                new SqlParameter("@is_limit_voucher", model.IsLimitVoucher),
                new SqlParameter("@limit_total_discount", model.LimitTotalDiscount ?? (object)DBNull.Value),
                new SqlParameter("@store_apply", model.StoreApply ?? (object)DBNull.Value),
                new SqlParameter("@is_max_price_product", model.IsMaxPriceProduct),
                new SqlParameter("@min_total_amount", model.MinTotalAmount ?? (object)DBNull.Value),
                new SqlParameter("@campaign_id", model.CampaignId ?? (object)DBNull.Value)
                };
                return  _DbWorker.ExecuteNonQuery("SP_InsertVoucher", objParam);
            }
            catch (Exception ex)
            {
                // LogHelper.InsertLogTelegram("InsertVoucher - VoucherDAL: " + ex);
                return -2;
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một voucher.
        /// </summary>
        public async Task<long> UpdateVoucher(Voucher model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[] {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@code", model.Code),
                new SqlParameter("@cdate", model.Cdate),
                new SqlParameter("@udate", model.Udate),
                new SqlParameter("@eDate", model.EDate),
                new SqlParameter("@limitUse", model.LimitUse),
                new SqlParameter("@price_sales", model.PriceSales),
                new SqlParameter("@unit", model.Unit),
                new SqlParameter("@rule_type", model.RuleType),
                new SqlParameter("@group_user_priority", model.GroupUserPriority ?? (object)DBNull.Value),
                new SqlParameter("@is_public", model.IsPublic),
                new SqlParameter("@description", model.Description ?? (object)DBNull.Value),
                new SqlParameter("@is_limit_voucher", model.IsLimitVoucher),
                new SqlParameter("@limit_total_discount", model.LimitTotalDiscount ?? (object)DBNull.Value),
                new SqlParameter("@store_apply", model.StoreApply ?? (object)DBNull.Value),
                new SqlParameter("@is_max_price_product", model.IsMaxPriceProduct),
                new SqlParameter("@min_total_amount", model.MinTotalAmount ?? (object)DBNull.Value),
                new SqlParameter("@campaign_id", model.CampaignId ?? (object)DBNull.Value)
            };
                return  _DbWorker.ExecuteNonQuery("SP_UpdateVoucher", objParam);
            }
            catch (Exception ex)
            {
                return -2;
            }
        }
    }
}
