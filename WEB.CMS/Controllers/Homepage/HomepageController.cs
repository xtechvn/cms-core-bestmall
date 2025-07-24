using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Caching.RedisWorker;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.BestMall.CMS.Service;
using WEB.CMS.Controllers.Product.Bussiness;
using WEB.CMS.Customize;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers.Homepage
{
    [CustomAuthorize]

    public class HomepageController : Controller
    {
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly ProductSpecificationMongoAccess _productSpecificationMongoAccess;
        private readonly IConfiguration _configuration;
        private readonly IGroupProductRepository _groupProductRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly RedisConn _redisConn;
        private readonly ProductDetailService productDetailService;
        private StaticAPIService _staticAPIService;
        private readonly ProductESRepository _productESRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        public HomepageController(IConfiguration configuration, RedisConn redisConn, IGroupProductRepository groupProductRepository, ILabelRepository labelRepository,
            ISupplierRepository supplierRepository, IAllCodeRepository allCodeRepository, ProductDetailMongoAccess productV2DetailMongoAccess, ProductSpecificationMongoAccess productSpecificationMongoAccess,
            ProductESRepository productESRepository)
        {
            _productV2DetailMongoAccess = productV2DetailMongoAccess;
            _productSpecificationMongoAccess = productSpecificationMongoAccess;
            _staticAPIService = new StaticAPIService(configuration);
            _redisConn = redisConn;
            _redisConn.Connect();
            _groupProductRepository = groupProductRepository;
            _configuration = configuration;
            productDetailService = new ProductDetailService(configuration, productV2DetailMongoAccess, productSpecificationMongoAccess);
            _productESRepository = productESRepository;
            _labelRepository = labelRepository;
            _supplierRepository = supplierRepository;
            _allCodeRepository = allCodeRepository;
        }
        public async Task<IActionResult> Index()
        {
            int max_slide = 3;
            int max_sub = 2;
            ViewBag.BannerSlide =  new List<AllCode>();
            ViewBag.BannerSub =  new List<AllCode>();
            ViewBag.MaxSlide = max_slide;
            ViewBag.MaxSub = max_sub;
            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.Static = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
            try
            {
              
                ViewBag.BannerSlide = _allCodeRepository.GetListByType("HOMEPAGE_SLIDE");
                ViewBag.BannerSub = _allCodeRepository.GetListByType("HOMEPAGE_SUBBANNER");

            }
            catch
            {

            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Summit(List<AllCode> banner_main, List<AllCode> banner_sub)
        {

            try
            {
                int _UserId = 0;

                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (banner_main != null && banner_main .Count>0) {
                    foreach (var banner in banner_main) {
                        banner.Type = "HOMEPAGE_SLIDE";
                        banner.UpdateTime=DateTime.Now;
                        banner.CreateDate = DateTime.Now;
                        banner.CreatedBy = _UserId;
                        banner.UpdatedBy = _UserId;
                        if (banner.Id > 0)
                        {
                            await _allCodeRepository.Update(banner);
                        }

                    }
                }
                if (banner_sub != null && banner_sub.Count > 0)
                {
                    foreach (var banner in banner_sub)
                    {
                        banner.Type = "HOMEPAGE_SUBBANNER";
                        banner.UpdateTime = DateTime.Now;
                        banner.CreateDate = DateTime.Now;
                        banner.CreatedBy = _UserId;
                        banner.UpdatedBy = _UserId;
                        if (banner.Id > 0)
                        {
                            await _allCodeRepository.Update(banner);
                        }
                    }
                }
                _redisConn.clear(CacheName.HOMEPAGE_SLIDE, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                return Ok(new
                {
                    is_success = true,
                    message="Cập nhật thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Summit - HomepageController: " + ex);
            }
            return Ok(new
            {
                is_success=false,
                message = "Cập nhật thất bại"

            });
        }
    }
}
