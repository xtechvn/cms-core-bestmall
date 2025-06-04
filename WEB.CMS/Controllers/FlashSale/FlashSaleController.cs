using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Repositories.IRepositories;
using System.Globalization;
using System.Security.Claims;
using Utilities;
using WEB.Adavigo.CMS.Service;
using WEB.CMS.Controllers.Product.Bussiness;
using WEB.CMS.Customize;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers.FlashSale
{
    [CustomAuthorize]

    public class FlashSaleController : Controller
    {
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly ProductSpecificationMongoAccess _productSpecificationMongoAccess;
        private readonly IConfiguration _configuration;
        private readonly IGroupProductRepository _groupProductRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly RedisConn _redisConn;
        private readonly ProductDetailService productDetailService;
        private StaticAPIService _staticAPIService;
        private readonly int group_product_root = 31;
        private readonly int db_index = 9;
        private readonly ProductESRepository _productESRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IFlashSaleRepository _flashSaleRepository;
        private readonly IFlashSaleProductRepository _flashSaleProductRepository;
        public FlashSaleController(IConfiguration configuration, RedisConn redisConn, IGroupProductRepository groupProductRepository, ILabelRepository labelRepository,
            ISupplierRepository supplierRepository, IAllCodeRepository allCodeRepository, IFlashSaleRepository flashSaleRepository, IFlashSaleProductRepository flashSaleProductRepository)
        {
            _productV2DetailMongoAccess = new ProductDetailMongoAccess(configuration);
            _productSpecificationMongoAccess = new ProductSpecificationMongoAccess(configuration);
            _staticAPIService = new StaticAPIService(configuration);
            _redisConn = redisConn;
            _redisConn.Connect();
            _groupProductRepository = groupProductRepository;
            db_index = Convert.ToInt32(configuration["Redis:Database:db_search_result"]);
            _configuration = configuration;
            productDetailService = new ProductDetailService(configuration);
            _productESRepository = new ProductESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _labelRepository = labelRepository;
            _supplierRepository = supplierRepository;
            _allCodeRepository = allCodeRepository;
            _flashSaleRepository = flashSaleRepository;
            _flashSaleProductRepository = flashSaleProductRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Search(DateTime? fromdate, DateTime? todate, int status,int page_index=1,int page_size=10)
        {
            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";

            if (fromdate >= todate)
            {
                return View();
            }
            if (fromdate == DateTime.MinValue) fromdate = null;
            if (todate == DateTime.MinValue) todate = null;
            page_index = page_index<1? 1: page_index;
            page_size = page_size<1? 10: page_size; 
            status = status < 0? 0: status; 
            // Gọi SP thông qua repository
            var flashSales = await _flashSaleRepository.GetList(fromdate, todate, status, page_index, page_size);
            // Trả về View với dữ liệu đã lấy được
            return View(flashSales);
        }
        public async Task<IActionResult> Detail(int id=-1)
        {
            ViewBag.Detail = new Entities.Models.FlashSale();
            ViewBag.Products = new List<Entities.Models.FlashSaleProduct>();
            ViewBag.ProductsDetail = new List<ProductMongoDbModel>();
            ViewBag.Supplier = new Supplier();
            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
            if (id > 0)
            {
                var detail = await _flashSaleRepository.GetByID(id);
                if(detail!=null && detail.Id > 0)
                {
                    ViewBag.Detail = detail;
                    var product = await _flashSaleProductRepository.GetByFlashSaleID(id);
                    if(product!=null && product.Count > 0)
                    {
                        product = product.OrderBy(x => x.Position).ToList();
                        ViewBag.Products = product;
                        ViewBag.ProductsDetail = await _productV2DetailMongoAccess.ListByProducts(product.Select(x=>x.ProductId).ToList());
                    }
                }
                if (detail != null && detail.SupplierId != null && detail.SupplierId > 0)
                {
                    ViewBag.Supplier = _supplierRepository.GetById((int)detail.SupplierId);
                }
            }
            return View();
        }
        [HttpPost]

        public IActionResult ProductFlashSale(int id)
        {
            ViewBag.FlashSaleId = id;
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> ProductFlashSaleSearch(string keyword = "", int group_id = -1,int supplier_id=-1)
        {
            ViewBag.Main = new List<ProductMongoDbModel>();

            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
            var main_products = await _productV2DetailMongoAccess.ListingProductFlashSale(keyword, group_id, supplier_id);
            ViewBag.Main = main_products;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Summit(Entities.Models.FlashSale flashsale, List<FlashSaleProduct> flashsale_product)
        {
            try
            {
                // 1. Validate FlashSale
                if (flashsale == null || !flashsale.SupplierId.HasValue|| flashsale.FromDate == default(DateTime) || flashsale.ToDate == default(DateTime))
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại",
                    });
                }
                if (flashsale.FromDate >= flashsale.ToDate) 
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Thời gian bắt đầu chiến dịch không được lớn hơn thời gian kết thúc",
                    });
                }
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                flashsale.UserCreateId = _UserLogin;
                flashsale.CreateDate = DateTime.Now;
                flashsale.UserUpdateId = _UserLogin;
                flashsale.UpdateLast = DateTime.Now;
                // 2. Create FlashSale
                if (flashsale.Id <= 0)
                {
                    _flashSaleRepository.CreateFlashSale(flashsale);
                }
                else
                {
                    var exists=await _flashSaleRepository.GetByID(flashsale.Id);
                    if(exists != null && exists.Id>0)
                    {
                        flashsale.UserCreateId = exists.UserCreateId;
                        flashsale.CreateDate = exists.CreateDate;
                        _flashSaleRepository.UpdateFlashSale(flashsale);
                    }
                    else
                    {
                        _flashSaleRepository.CreateFlashSale(flashsale);
                    }
                }
                if (flashsale.Id <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Lỗi trong quá trình xử lý, vui lòng thử lại / liên hệ hỗ trợ kỹ thuật",
                    });
                }
                // 3. Validate and Create FlashSale Products
                if (flashsale_product != null && flashsale_product.Any())
                {
                    foreach (var product in flashsale_product)
                    {
                        product.CampaignId = flashsale.Id;
                        if (product.Id <= 0)
                        {
                            _flashSaleProductRepository.CreateFlashSaleProduct(product);
                        }
                        else
                        {
                            _flashSaleProductRepository.UpdateFlashSaleProduct(product);
                        }
                    }
                }

                return Ok(new
                {
                    is_success = true,
                    msg = "Thêm mới / Cập nhật chiến dịch Flashsale thành công",
                    data = flashsale.Id,
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Summit - FlashSaleController: " + ex.ToString());
                return Ok(new
                {
                    is_success = false,
                    msg = "Thêm mới / Cập nhật sản phẩm thất bại, vui lòng liên hệ bộ phận IT",
                    err = ex.ToString(),
                });
            }
           
        }
    }
}
