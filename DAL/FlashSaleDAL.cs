using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class FlashSaleDAL : GenericService<FlashSale>
    {
        private static DbWorker _DbWorker;

        public FlashSaleDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        /// <summary>
        /// Tạo một Flash Sale mới.
        /// </summary>
        /// <param name="model">Đối tượng FlashSale chứa thông tin cần tạo.</param>
        /// <returns>ID của Flash Sale mới được tạo, hoặc 0 nếu có lỗi.</returns>
        public int CreateFlashSale(FlashSale model)
        {
            try
            {
                SqlParameter[] objParam =
                [
                    new SqlParameter("@FromDate", model.FromDate),
                    new SqlParameter("@ToDate", model.ToDate),
                    new SqlParameter("@ClientTypeId", (object)model.ClientTypeId ?? DBNull.Value), // Xử lý nullable
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@UserCreateId", model.UserCreateId),
                    new SqlParameter("@SupplierId", (object)model.SupplierId ?? DBNull.Value), // Xử lý nullable
                    new SqlParameter("@UserUpdateId", model.UserCreateId), // Đảm bảo SP có thể nhận hoặc tự xử lý
                ]; 

                int newId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertFlashSale, objParam);
                return newId; // DbWorker sẽ tự động trả về giá trị của @Identity
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateFlashSale - FlashSaleDAL: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Cập nhật thông tin Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSale chứa thông tin cập nhật.</param>
        /// <returns>ID của Flash Sale đã được cập nhật, hoặc 0 nếu có lỗi.</returns>
        public int UpdateFlashSale(FlashSale model)
        {
            try
            {
                SqlParameter[] objParam =
                [
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@FromDate", model.FromDate),
                    new SqlParameter("@ToDate", model.ToDate),
                    new SqlParameter("@ClientTypeId", (object)model.ClientTypeId ?? DBNull.Value),
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@UserUpdateId", model.UserUpdateId),
                    new SqlParameter("@SupplierId", (object)model.SupplierId ?? DBNull.Value),
                    
                ]; 

                // Gọi ExecuteNonQuery và nhận giá trị trả về (ID đã cập nhật)
                int updatedId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateFlashSale, objParam);
                return updatedId; // DbWorker sẽ tự động trả về giá trị của @Identity
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateFlashSale - FlashSaleDAL: " + ex.Message);
                return 0;
            }
        }
    }

    public class FlashSaleProductDAL : GenericService<FlashSaleProduct>
    {
        private static DbWorker _DbWorker;

        public FlashSaleProductDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        /// <summary>
        /// Thêm một sản phẩm vào Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSaleProduct chứa thông tin sản phẩm.</param>
        /// <returns>ID của FlashSaleProduct mới được tạo, hoặc 0 nếu có lỗi.</returns>
        public long CreateFlashSaleProduct(FlashSaleProduct model)
        {
            try
            {
                SqlParameter[] objParam =
                [
                    new SqlParameter("@CampaignId", (object)model.CampaignId ?? DBNull.Value),
                    new SqlParameter("@ProductId", (object)model.ProductId ?? DBNull.Value),
                    new SqlParameter("@DiscountValue", (object)model.DiscountValue ?? DBNull.Value),
                    new SqlParameter("@ValueType", (object)model.ValueType ?? DBNull.Value),
                    new SqlParameter("@Status", (object)model.Status ?? DBNull.Value),
                    new SqlParameter("@Position", (object)model.Position ?? DBNull.Value),
                ]; 


                long newId = Convert.ToInt64(_DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertFlashSaleProduct, objParam));
                return newId; // DbWorker sẽ tự động trả về giá trị của @Identity
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateFlashSaleProduct - FlashSaleProductDAL: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm trong Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSaleProduct chứa thông tin cập nhật.</param>
        /// <returns>ID của FlashSaleProduct đã được cập nhật, hoặc 0 nếu có lỗi.</returns>
        public long UpdateFlashSaleProduct(FlashSaleProduct model)
        {
            try
            {
                SqlParameter[] objParam =
                [
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@CampaignId", (object)model.CampaignId ?? DBNull.Value),
                    new SqlParameter("@ProductId", (object)model.ProductId ?? DBNull.Value),
                    new SqlParameter("@DiscountValue", (object)model.DiscountValue ?? DBNull.Value),
                    new SqlParameter("@ValueType", (object)model.ValueType ?? DBNull.Value),
                    new SqlParameter("@Status", (object)model.Status ?? DBNull.Value),
                    new SqlParameter("@Position", (object)model.Position ?? DBNull.Value),
                ];

                long updatedId = Convert.ToInt64(_DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateFlashSaleProduct, objParam));
                return updatedId; // DbWorker sẽ tự động trả về giá trị của @Identity
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateFlashSaleProduct - FlashSaleProductDAL: " + ex.Message);
                return 0;
            }
        }
    }
}
