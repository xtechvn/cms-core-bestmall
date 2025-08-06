using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.GroupProducts;
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
    public class GroupProductDAL : GenericService<GroupProduct>
    {
        private static DbWorker _DbWorker;

        public GroupProductDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);

        }

        /// <summary>
        /// Delete Group Product
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>
        ///  0 : errors
        /// -1 : is used
        /// -2 : has child
        /// >0 : success
        /// </returns>
        public async Task<int> DeleteGroupProduct(int Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var IsHasChild = _DbContext.GroupProducts.Any(s => s.ParentId == Id);

                   
                    if (IsHasChild)
                    {
                        return -2;
                    }

                    var entity = await FindAsync(Id);
                    _DbContext.GroupProducts.Remove(entity);
                    await _DbContext.SaveChangesAsync();
                    return Id;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteGroupProduct - GroupProductDAL: " + ex);
                return 0;
            }
        }

       

        public async Task<List<GroupProduct>> getCategoryDetailByCategoryId(int[] category_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var group_product = _DbContext.GroupProducts.AsNoTracking().Where(s => category_id.Contains(s.Id)).ToListAsync();

                    return await group_product;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getCategoryDetailByCategoryId - GroupProductDAL: " + ex);
                return null;
            }
        }
       
        
       
       
       

        /// <summary>
        /// cuonglv
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<List<GroupProduct>> getAllGroupProduct()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var group_product = _DbContext.GroupProducts.AsNoTracking().Where(s => s.Status == (int)StatusType.BINH_THUONG).ToListAsync();

                    return await group_product;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getAllGroupProduct - GroupProductDAL: " + ex);
                return null;
            }
        }

        /// <summary>
        /// cuonglv
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<GroupProduct> getDetailByPath(string path)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var group_product = await _DbContext.GroupProducts.AsNoTracking().FirstOrDefaultAsync(s => s.Status == (int)StatusType.BINH_THUONG && s.Path == path.Trim());
                    return group_product;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getDetailByPath - GroupProductDAL: " + ex);
                return null;
            }
        }

        public async Task<bool> IsGroupHeader(List<int> groups)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var group_product = await _DbContext.GroupProducts.AsNoTracking().Where(s => s.Status == (int)StatusType.BINH_THUONG && groups.Contains(s.Id) && s.IsShowFooter==true).ToListAsync();
                    if (group_product != null && group_product.Count > 0)
                    {
                        return false;
                    }
                    else return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("IsGroupHeader - GroupProductDAL: " + ex);
                return false;
            }
        }
        public List<GroupProduct> GetByParentId(long parent_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.GroupProducts.Where(s => s.ParentId == parent_id && s.Status == (int)ArticleStatus.PUBLISH).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByParentId - GroupProductDAL: " + ex);

            }
            return null;
        }
        public List<GroupProduct> Search(string keyword,int parent_id=1)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    //return _DbContext.GroupProducts.Where(s => s.Name.Contains(keyword) && s.ParentId == parent_id && s.Status == (int)ArticleStatus.PUBLISH).ToList();
                    var list_tier1= _DbContext.GroupProducts.Where(s => s.Name.Contains(keyword) && s.ParentId==parent_id && s.Status == (int)ArticleStatus.PUBLISH).ToList();
                    if(list_tier1 != null && list_tier1.Count>0)
                    {
                        var list_ids = list_tier1.Select(s => s.Id);
                        list_tier1.AddRange(_DbContext.GroupProducts.Where(s => s.Name.Contains(keyword) && list_ids.Contains(s.Id) && s.Status == (int)ArticleStatus.PUBLISH).ToList());
                    }
                    return list_tier1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - GroupProductDAL: " + ex);

            }
            return new List<GroupProduct>();
        }
        public List<GroupProduct> GetByParentIdOrder(long parent_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data= _DbContext.GroupProducts.Where(s => s.ParentId == parent_id).ToList();
                    if(data!=null && data.Count > 0)
                    {
                        return data.OrderBy(x=>x.OrderNo).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByParentIdOrder - GroupProductDAL: " + ex);

            }
            return null;
        }
        public async Task<List<GroupProduct>> SearchSP(string keyword, int parent_id = 1)
        {
            try
            {

                SqlParameter[] objParam =
                [
                   new SqlParameter("@Keyword", keyword?? (object)DBNull.Value),
                   new SqlParameter("@ParentId",parent_id),
                ];
                var dt = _DbWorker.GetDataTable("SP_GetListGroupProduct", objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<GroupProduct>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SearchSP - GroupProductDAL: " + ex);
            }
            return null;
        }

    }
}
