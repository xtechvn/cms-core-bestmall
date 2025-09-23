using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;
namespace DAL
{
    public class AllotmentUseDAL : GenericService<AllotmentUse>
    {
        private static DbWorker _DbWorker;

        public AllotmentUseDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public int Insert(AllotmentUse model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@DataId", model.DataId),
                    new SqlParameter("@CreateDate", model.CreateDate==null||model.CreateDate<=DateTime.MinValue ? DBNull.Value:model.CreateDate),
                    new SqlParameter("@AmountUse", model.AmountUse),
                    new SqlParameter("@AllomentFundId", model.AllotmentFundId),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@ServiceType", model.ServiceType),
                    new SqlParameter("@ClientId", model.ClientId),
                    new SqlParameter("@PaymentStatus", model.PaymentStatus),
                     new SqlParameter("@Description", (object?)model.Description ?? DBNull.Value),
                    new SqlParameter("@TotalAmoutCalculate", (object?)model.TotalAmoutCalculate ?? DBNull.Value),
                    new SqlParameter("@PaymentFromDate", (object?)model.PaymentFromDate ?? DBNull.Value),
                    new SqlParameter("@PaymentToDate", (object?)model.PaymentToDate ?? DBNull.Value),
                    new SqlParameter("@BankId", model.BankId ?? (object?)DBNull.Value),
                    new SqlParameter("@AccountNumber", model.AccountNumber?? (object?)DBNull.Value),
                    new SqlParameter("@AccountName", (object?)model.AccountName ?? DBNull.Value),
                    new SqlParameter("@Branch", (object?)model.Branch ?? DBNull.Value),
                };

                return Convert.ToInt32(_DbWorker.ExecuteScalar("SP_InsertAllotmentUse", objParam));
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - AllotmentUseDAL: " + ex);
                return -1;
            }
        }

        public int Update(AllotmentUse model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@DataId", model.DataId),
                    new SqlParameter("@AmountUse", model.AmountUse),
                    new SqlParameter("@AllomentFundId", model.AllotmentFundId),
                    new SqlParameter("@AccountClientId", model.AccountClientId),
                    new SqlParameter("@ServiceType", model.ServiceType),
                    new SqlParameter("@ClientId", model.ClientId),
                    new SqlParameter("@CreateDate", model.CreateDate==null||model.CreateDate<=DateTime.MinValue ? DBNull.Value:model.CreateDate),
                    new SqlParameter("@PaymentStatus", model.PaymentStatus),
                     new SqlParameter("@Description", (object?)model.Description ?? DBNull.Value),
                    new SqlParameter("@TotalAmoutCalculate", (object?)model.TotalAmoutCalculate ?? DBNull.Value),
                    new SqlParameter("@PaymentFromDate", (object?)model.PaymentFromDate ?? DBNull.Value),
                    new SqlParameter("@PaymentToDate", (object?)model.PaymentToDate ?? DBNull.Value),
                    new SqlParameter("@BankId", model.BankId ?? (object?)DBNull.Value),
                    new SqlParameter("@AccountNumber", model.AccountNumber?? (object?)DBNull.Value),
                    new SqlParameter("@AccountName", (object?)model.AccountName ?? DBNull.Value),
                    new SqlParameter("@Branch", (object?)model.Branch ?? DBNull.Value),
                };

                return _DbWorker.ExecuteNonQuery("SP_UpdateAllotmentUse", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - AllotmentUseDAL: " + ex);
                return -1;
            }
        }

        public List<AllotmentUse> GetByAccountClientId(long accountClientId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@AccountClientId", accountClientId)
                };

                var dt= _DbWorker.GetDataTable("SP_GetAllotmentUseByAccountClientId", objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<AllotmentUse>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByAccountClientId - AllotmentUseDAL: " + ex);
            }
            return null;

        }
        public async Task<AllotmentUse> GetByOrderMergeId(long orderMergeId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllotmentUses.AsNoTracking().FirstOrDefaultAsync(x => x.DataId == orderMergeId && x.ServiceType==0);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderMergeId - AllotmentUseDAL: " + ex.ToString());
                return null;
            }
        }
        public async Task<AllotmentFund> GetByClientId(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.AllotmentFunds.FirstOrDefaultAsync(s => s.AccountClientId == clientId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }
        public async Task<AllotmentUse> GetByDataId(long dataId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.AllotmentUses.FirstOrDefaultAsync(s => s.DataId == dataId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByDataId - UserDAL: " + ex);
                return null;
            }
        }
       
    }
}
