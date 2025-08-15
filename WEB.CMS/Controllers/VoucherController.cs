using DocumentFormat.OpenXml.Spreadsheet;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Threading.Tasks;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class VoucherController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ManagementUser _ManagementUser;
        private readonly IVoucherRepository voucherRepository;

        public VoucherController( ManagementUser managementUser, IConfiguration configuration, IVoucherRepository voucherRepository)
        {

            _ManagementUser = managementUser;
            _configuration = configuration;
            this.voucherRepository = voucherRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Search(string keyword, int status, int pageIndex, int pageSize)
        {

            try
            {
                var model = await voucherRepository.GetVoucherPagingList(keyword,status,pageIndex,pageSize);

                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - VoucherController: " + ex);
            }

            return PartialView();
        }
        public async Task<IActionResult> Detail(int id=0)
        {
            ViewBag.Detail = new Voucher();
            ViewBag.ListProduct = new List<ProductMongoDbModel>();

            if (id > 0) {

                var detail = await voucherRepository.GetById(id);
                if (detail != null && detail.Id>0) {
                    ViewBag.Detail = detail;
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Summit(Voucher request)
        {
            try
            {
                if (request.Id <= 0)
                {
                    request.Cdate= DateTime.Now;
                    long id=await voucherRepository.InsertVoucher(request);
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Tạo voucher ["+request.Code+"] thành công",
                    });
                }
                else
                {
                    var exists=await voucherRepository.GetById(request.Id);
                    if (exists != null && exists.Id>0)
                    {
                        request.Cdate = exists.Cdate;
                        await voucherRepository.UpdateVoucher(request);
                        return Ok(new
                        {
                            is_success = true,
                            msg = "Cập nhật voucher [" + request.Code + "] thành công",
                        });
                    }
                }

               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Summit - VoucherController: " + ex);
            }
            return Ok(new
            {
                is_success = false,
                msg = "Dữ liệu sản phẩm không chính xác, vui lòng chỉnh sửa và thử lại",
            });
        }
    }
}
