using Caching.Elasticsearch;
using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
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
        public FlashSaleController(IConfiguration configuration, RedisConn redisConn, IGroupProductRepository groupProductRepository, ILabelRepository labelRepository,
            ISupplierRepository supplierRepository, IAllCodeRepository allCodeRepository)
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
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
