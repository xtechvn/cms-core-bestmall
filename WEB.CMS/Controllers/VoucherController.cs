using Azure.Core;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using DocumentFormat.OpenXml.Spreadsheet;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class VoucherController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ManagementUser _ManagementUser;
        private readonly IVoucherRepository voucherRepository;
        private readonly RedisConn _redisConn;
        private ClientESRepository clientESRepository;
        private IClientRepository _clientRepository;

        public VoucherController( ManagementUser managementUser, IConfiguration configuration, IVoucherRepository voucherRepository, RedisConn redisConn, 
            ClientESRepository _clientESRepository, IClientRepository clientRepository)
        {

            _ManagementUser = managementUser;
            _configuration = configuration;
            this.voucherRepository = voucherRepository;
            _redisConn = redisConn;
            _redisConn.Connect();
            clientESRepository = _clientESRepository;
            _clientRepository=clientRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Search(string keyword, int status, int pageIndex, int pageSize)
        {

            try
            {
                var model = await voucherRepository.GetVoucherPagingList(keyword,status,pageIndex,pageSize,null);

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
            ViewBag.Client = new List<Client>();
            ViewBag.ListProduct = new List<ProductMongoDbModel>();

            if (id > 0) {

                var detail = await voucherRepository.GetById(id);
                if (detail != null && detail.Id>0) {
                    ViewBag.Detail = detail;
                }
                if (detail != null && detail.GroupUserPriority != null && detail.GroupUserPriority.Trim() != "")
                {
                    List<long> client_ids = JsonConvert.DeserializeObject<List<long>>(detail.GroupUserPriority);
                    if (client_ids != null && client_ids.Count > 0)
                    {
                        ViewBag.Client = _clientRepository.GetClientByIds(client_ids);
                    }
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Summit(Voucher request)
        {
            try
            {
                string message = "Tạo voucher [" + request.Code + "] thành công";
                if (request.Id <= 0)
                {
                    request.Cdate= DateTime.Now;
                    request.Id = await voucherRepository.InsertVoucher(request);
                    
                }
                else
                {
                    var exists=await voucherRepository.GetById(request.Id);
                    if (exists != null && exists.Id>0)
                    {
                        request.Cdate = exists.Cdate;
                        await voucherRepository.UpdateVoucher(request);
                        message = "Cập nhật voucher [" + request.Code + "] thành công";
                    }
                }
                if (request.Id > 0) {
                    string cache_name = CacheType.VOUCHER;
                    _redisConn.clear(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                    if (request.GroupUserPriority != null && request.GroupUserPriority.Trim() != "")
                    {
                        List<long> client_ids=JsonConvert.DeserializeObject<List<long>>(request.GroupUserPriority);
                        if(client_ids!=null && client_ids.Count > 0)
                        {
                            foreach(var client in client_ids)
                            {
                                 cache_name = CacheType.VOUCHER + client;
                                 _redisConn.clear(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                            }
                        }
                    }
                }
                return Ok(new
                {
                    is_success = true,
                    msg = message,
                });
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
