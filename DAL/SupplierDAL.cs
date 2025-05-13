using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.SupplierConfig;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace DAL.Funding
{
    public class SupplierDAL : GenericService<Supplier>
    {
        private static DbWorker _DbWorker;

        public object In { get; set; }

        public SupplierDAL(string connection) : base(connection)
        {
            _connection = connection;
            _DbWorker = new DbWorker(connection);
        }

        #region supplier
        public Supplier GetById(long supplierId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Suppliers.FirstOrDefault(x => x.SupplierId == supplierId);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - SupplierDAL: " + ex);
                return null;
            }
        }

        public DataTable GetSuggestSupplier(string textSearch, int limit)
        {
            try
            {
                try
                {
                    SqlParameter[] objParam = new SqlParameter[]
                    {
                         new SqlParameter("@TextSearch", textSearch),
                         new SqlParameter("@Limit", limit)
                    };

                    return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetSuggestSupplier, objParam);
                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("GetSuggestSupplier - SP_GetSuggestSupplier. " + ex);
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        public DataTable GetSuggestSupplierOfHotel(int hotel_id, string textSearch, int limit)
        {
            try
            {
                try
                {
                    SqlParameter[] objParam = new SqlParameter[]
                    {
                         new SqlParameter("@HotelId", hotel_id),
                         new SqlParameter("@TextSearch", textSearch),
                         new SqlParameter("@Limit", limit)
                    };

                    return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetSuggestSupplierOfHotel, objParam);
                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("GetSuggestSupplier - SP_GetSuggestSupplierOfHotel. " + ex);
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        public DataTable GetDetailById(long supplierId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                     new SqlParameter("@SupplierId", supplierId)
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetSupplierById, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - SupplierDAL: " + ex);
                return null;
            }

        }

        public DataTable GetPagingList(SupplierSearchModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@FullName", searchModel.FullName ?? (object)DBNull.Value),
                    new SqlParameter("@ServiceType", searchModel.ServiceType ?? (object)DBNull.Value),
                    new SqlParameter("@ProvinceId", searchModel.ProvinceId ?? (object)DBNull.Value),
                    new SqlParameter("@RatingStar", searchModel.RatingStar ?? (object)DBNull.Value),
                    new SqlParameter("@ChainBrands", searchModel.ChainBrands ?? (object)DBNull.Value),
                    new SqlParameter("@SalerId", searchModel.SalerId ?? (object)DBNull.Value),
                    new SqlParameter("@PageIndex", searchModel.currentPage),
                    new SqlParameter("@PageSize", searchModel.pageSize)
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListSupplier, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - SupplierDAL: " + ex);
            }
            return null;
        }

        public int CreateSupplier(SupplierConfigUpsertModel model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@SupplierCode", model.SupplierCode ?? (object)DBNull.Value),
                    new SqlParameter("@FullName", model.FullName),
                    new SqlParameter("@Email", model.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Phone", model.Phone ?? (object)DBNull.Value),
                    new SqlParameter("@Address", model.Address ?? (object)DBNull.Value),
                    new SqlParameter("@CreatedBy", model.CreatedBy),
                    new SqlParameter("@CreatedDate", DateTime.Now),
                };
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertSupplier, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateSupplier - SupplierDAL. " + ex);
                return -1;
            }
        }

        public int UpdateSupplier(SupplierConfigUpsertModel model)
        {
            try
            {
                SqlParameter[] objParam_contractPay = new SqlParameter[]
                {
                    new SqlParameter("@SupplierId", model.SupplierId),
                    new SqlParameter("@SupplierCode", model.SupplierCode ?? (object)DBNull.Value),
                    new SqlParameter("@FullName", model.FullName ?? (object)DBNull.Value),
                    new SqlParameter("@Email", model.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Phone", model.Phone ?? (object)DBNull.Value),
                    new SqlParameter("@Address", model.Address ?? (object)DBNull.Value),
                    new SqlParameter("@UpdatedBy ", model.UpdatedBy)
                };
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateSupplier, objParam_contractPay);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateSupplier - SupplierDAL. " + ex);
                return -1;
            }
        }

        public Supplier CheckExistTaxCode(int id, string taxCode)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Suppliers.FirstOrDefault(x => x.SupplierId != id && taxCode == x.TaxCode);
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckExistTaxCode - SupplierDAL: " + ex);
                return null;
            }
        }

        public Supplier CheckExistName(int id, string name)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Suppliers.FirstOrDefault(x => x.SupplierId != id && name.ToLower() == x.FullName.ToLower());
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckExistName - SupplierDAL: " + ex);
                return null;
            }
        }
        #endregion

        #region supplier - contact

        public DataTable GetSupplierContactDataTable(int supplier_id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@SupplierId", supplier_id)
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailSupplierContactBySupplierId, objParam);
            }
            catch
            {
                throw;
            }
        }

        public SupplierContact GetSupplierContactById(long Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.SupplierContacts.Find(Id);
                }
            }
            catch
            {
                throw;
            }
        }

        public long DeleteSupplierContactById(long Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var model = _DbContext.SupplierContacts.Find(Id);
                    if (model != null)
                    {
                        _DbContext.SupplierContacts.Remove(model);
                        _DbContext.SaveChanges();
                    }
                    return Id;
                }
            }
            catch
            {
                throw;
            }
        }

        public int InsertSupplierContact(SupplierContact model)
        {
            try
            {
                SqlParameter[] objParam_contractPay = new SqlParameter[]
                {
                    new SqlParameter("@SupplierId", model.SupplierId),
                    new SqlParameter("@Name", model.Name ?? (object)DBNull.Value),
                    new SqlParameter("@Mobile", model.Mobile ?? (object)DBNull.Value),
                    new SqlParameter("@Email", model.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Position", model.Position ?? (object)DBNull.Value),
                    new SqlParameter("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value),
                    new SqlParameter("@CreateDate ", DateTime.Now)
                };
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertSupplierContact, objParam_contractPay);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateSupplier - SupplierDAL. " + ex);
                return -1;
            }
        }

        public int UpdateSupplierContact(SupplierContact model)
        {
            try
            {
                SqlParameter[] objParam_contractPay = new SqlParameter[]
                {
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@SupplierId", model.SupplierId),
                    new SqlParameter("@Name", model.Name ?? (object)DBNull.Value),
                    new SqlParameter("@Mobile", model.Mobile ?? (object)DBNull.Value),
                    new SqlParameter("@Email", model.Email ?? (object)DBNull.Value),
                    new SqlParameter("@Position", model.Position ?? (object)DBNull.Value),
                    new SqlParameter("@UpdatedBy", model.UpdatedBy ?? (object)DBNull.Value)
                };
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateSupplierContact, objParam_contractPay);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateSupplier - SupplierDAL. " + ex);
                return -1;
            }
        }
        #endregion

     
        #region supplier - programs
        public DataTable GetDetailProgramBySupplierId(long supplier_id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@SupplierId",supplier_id),
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetDetailProgramBySupplierId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateSupplierRoom - SupplierDAL. " + ex);
                throw;
            }
        }
        #endregion

        #region supplier - service
        public DataTable GetAllServiceBySupplierId(long supplier_id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@SupplierId",supplier_id),
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAllServiceBySupplierId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateSupplierRoom - SupplierDAL. " + ex);
                throw;
            }
        }

        public DataTable GetListHotelBySupplierId(long supplier_id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@SupplierId",supplier_id),
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListHotelBySupplierId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateSupplierRoom - SupplierDAL. " + ex);
                throw;
            }
        }


        #endregion

        #region ticket
        public DataTable GetListPaymentVoucherBySupplierId(SupplierTicketSearchModel model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@SupplierID",model.supplier_id),
                    new SqlParameter("@PageIndex",model.page_index),
                    new SqlParameter("@PageSize", model.page_size)
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListPaymentVoucherBySupplierId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateSupplierRoom - SupplierDAL. " + ex);
                throw;
            }
        }
        #endregion

        #region program
        public DataTable GetListPrograms(SupplierProgramSearchModel model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@ProgramCode",model.ProgramCode ?? (object)DBNull.Value),
                    new SqlParameter("@Description",model.Description ?? (object)DBNull.Value),
                    new SqlParameter("@ProgramStatus",model.ProgramStatus ?? (object)DBNull.Value),
                    new SqlParameter("@ServiceType",model.ServiceType ?? (object)DBNull.Value),
                    new SqlParameter("@SupplierID",model.SupplierID),
                    new SqlParameter("@ClientId",model.ClientId ?? (object)DBNull.Value),
                    new SqlParameter("@StartDateFrom",model.StartDateFrom ?? (object)DBNull.Value),
                    new SqlParameter("@StartDateTo",model.StartDateTo ?? (object)DBNull.Value),
                    new SqlParameter("@EndDateFrom",model.EndDateFrom ?? (object)DBNull.Value),
                    new SqlParameter("@EndDateTo",model.EndDateTo ?? (object)DBNull.Value),
                    new SqlParameter("@UserCreate",model.UserCreate ?? (object)DBNull.Value),
                    new SqlParameter("@CreateDateFrom",model.CreateDateFrom ?? (object)DBNull.Value),
                    new SqlParameter("@CreateDateTo",model.CreateDateTo ?? (object)DBNull.Value),
                    new SqlParameter("@UserVerify",model.UserVerify ?? (object)DBNull.Value),
                    new SqlParameter("@VerifyDateFrom",model.VerifyDateFrom ?? (object)DBNull.Value),
                    new SqlParameter("@VerifyDateTo",model.VerifyDateTo ?? (object)DBNull.Value),
                    new SqlParameter("@PageIndex",model.PageIndex),
                    new SqlParameter("@PageSize",model.PageSize)
                };

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListPrograms, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListPrograms - SupplierDAL. " + ex);
                throw;
            }
        }
        #endregion
    }
}
