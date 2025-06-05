using DAL;
using DAL.Funding;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.FlashSales;
using Entities.ViewModels.Funding;
using Microsoft.Extensions.Options;
using Nest;
using PdfSharp;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    // FlashSaleRepository.cs
    public class FlashSaleRepository : IFlashSaleRepository
    {
        private readonly FlashSaleDAL _flashSaleDAL;

        // Constructor nhận chuỗi kết nối để khởi tạo DAL
        public FlashSaleRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _flashSaleDAL = new FlashSaleDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        /// <summary>
        /// Tạo một Flash Sale mới.
        /// </summary>
        /// <param name="model">Đối tượng FlashSale chứa thông tin cần tạo.</param>
        /// <returns>ID của Flash Sale mới được tạo, hoặc 0 nếu có lỗi.</returns>
        public int CreateFlashSale(FlashSale model)
        {
            
            return _flashSaleDAL.CreateFlashSale(model);
        }

        /// <summary>
        /// Cập nhật thông tin Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSale chứa thông tin cập nhật.</param>
        /// <returns>ID của Flash Sale đã được cập nhật, hoặc 0 nếu có lỗi.</returns>
        public int UpdateFlashSale(FlashSale model)
        {
            
            return _flashSaleDAL.UpdateFlashSale(model);
        }
      

        /// <summary>
        /// Lấy danh sách Flash Sale với phân trang và bộ lọc thời gian.
        /// </summary>
        /// <param name="fromdate">Ngày bắt đầu lọc.</param>
        /// <param name="todate">Ngày kết thúc lọc.</param>
        /// <param name="page_index">Chỉ số trang.</param>
        /// <param name="page_size">Kích thước trang.</param>
        /// <returns>DataTable chứa danh sách Flash Sale.</returns>
        public async Task<GenericViewModel<FlashSaleListingModel>> GetList(DateTime? fromdate, DateTime? todate,int status, int page_index, int page_size)
        {
            var model = new GenericViewModel<FlashSaleListingModel>();
            try
            {
                var dt = await _flashSaleDAL.GetList(fromdate, todate, status, page_index, page_size);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<FlashSaleListingModel>();
                    model.ListData = data;
                    model.CurrentPage = page_index;
                    model.PageSize = page_size;
                    model.TotalRecord = data[0].TotalRow;
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch(Exception ex) {
                LogHelper.InsertLogTelegram("GetListFlashSale - FlashSaleRepository: " + ex);
            }
            return model;
        }
        public async Task<FlashSale> GetByID(int id)
        {

            return await _flashSaleDAL.GetByID(id);
        }
        public async Task<List<FlashSale>> GetAll()
        {

            return await _flashSaleDAL.GetAllAsync();
        }
    }

    // FlashSaleProductRepository.cs
    public class FlashSaleProductRepository : IFlashSaleProductRepository
    {
        private readonly FlashSaleProductDAL _flashSaleProductDAL;

        public FlashSaleProductRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _flashSaleProductDAL = new FlashSaleProductDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        /// <summary>
        /// Thêm một sản phẩm vào Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSaleProduct chứa thông tin sản phẩm.</param>
        /// <returns>ID của FlashSaleProduct mới được tạo, hoặc 0 nếu có lỗi.</returns>
        public long CreateFlashSaleProduct(FlashSaleProduct model)
        {
            
            return _flashSaleProductDAL.CreateFlashSaleProduct(model);
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm trong Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSaleProduct chứa thông tin cập nhật.</param>
        /// <returns>ID của FlashSaleProduct đã được cập nhật, hoặc 0 nếu có lỗi.</returns>
        public long UpdateFlashSaleProduct(FlashSaleProduct model)
        {
            
            return _flashSaleProductDAL.UpdateFlashSaleProduct(model);
        }
        public async Task<List<FlashSaleProduct>> GetByFlashSaleID(int id)
        {
            
            return await _flashSaleProductDAL.GetByFlashSaleID(id);
        }
        public async Task<List<FlashSaleProduct>> GetAll()
        {

            return await _flashSaleProductDAL.GetAllAsync();
        }
    }
}
