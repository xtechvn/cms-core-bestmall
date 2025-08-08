using Entities.Models;
using Entities.ViewModels;
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
        public async Task<IActionResult> Detail(int voucher_id=0)
        {
            ViewBag.Detail = new Voucher();
            if (voucher_id > 0) {

                var detail = await voucherRepository.GetById(voucher_id);
                if (detail != null && detail.Id>0) {
                    ViewBag.Detail = detail;
                }
            }
            return View();
        }
    }
}
