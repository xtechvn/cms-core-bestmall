using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.Label;
using HuloToys_Service.Models.Label;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class LabelDAL : GenericService<Label>
    {
        private static DbWorker _DbWorker;
        public LabelDAL(string connection) : base(connection) {
            _DbWorker = new DbWorker(connection);
        }
      

        public async Task<List<LabelListingModel>> Listing(int status=-1, string label_name=null,string label_code=null, int page_index = -1, int page_size = 100)
        {
            try
            {

                SqlParameter[] objParam =
                [
                    new SqlParameter("@Status", status<0?DBNull.Value:status),
                    new SqlParameter("@LabelName", label_name==null?DBNull.Value: label_name),
                    new SqlParameter("@PageIndex", page_index<0?-1:page_index),
                    new SqlParameter("@PageSize", page_size<10?10:page_size),
                    new SqlParameter("@LabelCode", label_code == null ? DBNull.Value : label_code),
                ];
                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.GetListLabels, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<LabelListingModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Listing - LabelDAL: " + ex);
            }
            return null;
        }
        public int Insert(Label model)
        {
            try
            {
                SqlParameter[] objParam =
                [
                    new SqlParameter("@LabelName ", model.LabelName),
                    new SqlParameter("@LabelCode", model.LabelCode),
                    new SqlParameter("@SupplierId", model.SupplierId),
                    new SqlParameter("@Icon", model.Icon),
                    new SqlParameter("@ParentId", model.ParentId),
                    new SqlParameter("@Level", model.Level),
                    new SqlParameter("@Description", model.Description),
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@CreateTime", model.CreateTime),
                    new SqlParameter("@UpdateTime", model.UpdateTime),
                    new SqlParameter("@CreatedBy", model.CreatedBy),
                    new SqlParameter("@UserSupplierId", model.UserSupplierId),
                    new SqlParameter("@Banner", model.Banner),
                    new SqlParameter("@Avatar ", model.Avatar),
                ];
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.InsertLabel, objParam);
                model.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - LabelDAL: " + ex);
                return 0;
            }
        }
        public int Update(Label model)
        {
            try
            {
                SqlParameter[] objParam =
                [
                    new SqlParameter("@LabelName ", model.LabelName),
                    new SqlParameter("@LabelCode", model.LabelCode),
                    new SqlParameter("@SupplierId", model.SupplierId),
                    new SqlParameter("@Icon", model.Icon),
                    new SqlParameter("@ParentId", model.ParentId),
                    new SqlParameter("@Level", model.Level),
                    new SqlParameter("@Description", model.Description),
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@CreateTime", model.CreateTime),
                    new SqlParameter("@UpdateTime", model.UpdateTime),
                    new SqlParameter("@UpdatedBy", model.UpdatedBy),
                    new SqlParameter("@Id ", model.Id),
                    new SqlParameter("@UserSupplierId ", model.UserSupplierId),
                    new SqlParameter("@Banner ", model.Banner),
                    new SqlParameter("@Avatar ", model.Avatar),
                ];
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.UpdateLabel, objParam);
                model.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - LabelDAL: " + ex);
                return 0;
            }
        }
        public async Task<Label> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return await _DbContext.Labels.AsNoTracking().FirstOrDefaultAsync(s => s.Id==id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - LabelDAL: " + ex);
                return null;
            }
        }
        public async Task<Label> GetByCode(string code)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return await _DbContext.Labels.AsNoTracking().FirstOrDefaultAsync(s => s.LabelCode == code);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - LabelDAL: " + ex);
                return null;
            }
        }
        public async Task<List<Label>> GetAllLabels()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return await _DbContext.Labels.AsNoTracking().Where(x=>x.Status!=2).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - LabelDAL: " + ex);
                return null;
            }
        }
    }
}
