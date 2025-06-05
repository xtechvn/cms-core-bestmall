using Entities.Models;
using Entities.ViewModels.FlashSales;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    // IFlashSaleRepository.cs
    public interface IFlashSaleRepository
    {
        /// <summary>
        /// Tạo một Flash Sale mới.
        /// </summary>
        /// <param name="model">Đối tượng FlashSale chứa thông tin cần tạo.</param>
        /// <returns>ID của Flash Sale mới được tạo, hoặc 0 nếu có lỗi.</returns>
        int CreateFlashSale(FlashSale model);

        /// <summary>
        /// Cập nhật thông tin Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSale chứa thông tin cập nhật.</param>
        /// <returns>ID của Flash Sale đã được cập nhật, hoặc 0 nếu có lỗi.</returns>
        int UpdateFlashSale(FlashSale model);

        /// <summary>
        /// Lấy danh sách Flash Sale với phân trang và bộ lọc thời gian.
        /// </summary>
        /// <param name="fromdate">Ngày bắt đầu lọc.</param>
        /// <param name="todate">Ngày kết thúc lọc.</param>
        /// <param name="page_index">Chỉ số trang.</param>
        /// <param name="page_size">Kích thước trang.</param>
        /// <returns>DataTable chứa danh sách Flash Sale.</returns>
        public Task<GenericViewModel<FlashSaleListingModel>> GetList(DateTime? fromdate, DateTime? todate, int status, int page_index, int page_size);
        Task<FlashSale> GetByID(int id);
        Task<List<FlashSale>> GetAll();
    }

    // IFlashSaleProductRepository.cs
    public interface IFlashSaleProductRepository
    {
        /// <summary>
        /// Thêm một sản phẩm vào Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSaleProduct chứa thông tin sản phẩm.</param>
        /// <returns>ID của FlashSaleProduct mới được tạo, hoặc 0 nếu có lỗi.</returns>
        long CreateFlashSaleProduct(FlashSaleProduct model);

        /// <summary>
        /// Cập nhật thông tin sản phẩm trong Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSaleProduct chứa thông tin cập nhật.</param>
        /// <returns>ID của FlashSaleProduct đã được cập nhật, hoặc 0 nếu có lỗi.</returns>
        long UpdateFlashSaleProduct(FlashSaleProduct model);
        Task<List<FlashSaleProduct>> GetByFlashSaleID(int id);

        Task<List<FlashSaleProduct>> GetAll();
    }
}
