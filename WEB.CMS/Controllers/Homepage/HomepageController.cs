using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
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
        private readonly int group_product_root = 31;
        private readonly int db_index = 9;
        private readonly ProductESRepository _productESRepository;
        private readonly FlashSaleESRepository _flashSaleESRepository;
        private readonly FlashSaleProductESRepository _flashSaleProductESRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IFlashSaleRepository _flashSaleRepository;
        private readonly IFlashSaleProductRepository _flashSaleProductRepository;
        public HomepageController(IConfiguration configuration, RedisConn redisConn, IGroupProductRepository groupProductRepository, ILabelRepository labelRepository,
            ISupplierRepository supplierRepository, IAllCodeRepository allCodeRepository, IFlashSaleRepository flashSaleRepository, IFlashSaleProductRepository flashSaleProductRepository
            , ProductDetailMongoAccess productV2DetailMongoAccess, ProductSpecificationMongoAccess productSpecificationMongoAccess,
            ProductESRepository productESRepository, FlashSaleESRepository flashSaleESRepository, FlashSaleProductESRepository flashSaleProductESRepository)
        {
            _productV2DetailMongoAccess = productV2DetailMongoAccess;
            _productSpecificationMongoAccess = productSpecificationMongoAccess;
            _staticAPIService = new StaticAPIService(configuration);
            _redisConn = redisConn;
            _redisConn.Connect();
            _groupProductRepository = groupProductRepository;
            db_index = Convert.ToInt32(configuration["Redis:Database:db_search_result"]);
            _configuration = configuration;
            productDetailService = new ProductDetailService(configuration, productV2DetailMongoAccess, productSpecificationMongoAccess);
            _productESRepository = productESRepository;
            _flashSaleESRepository = flashSaleESRepository;
            _flashSaleProductESRepository = flashSaleProductESRepository;
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
    }
}
