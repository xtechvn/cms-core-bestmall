
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.RabitMQ;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ElasticController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient work_queue;
        public ElasticController(IConfiguration configuration)
        {
            work_queue = new WorkQueueClient(configuration);
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult PushToQueue([FromBody] QueueRequest request)
        {
            try
            {
                var j_param = new Dictionary<string, object>
                {
                    { "store_name", request.StoreName },
                    { "index_es", "hulotoys_" + request.StoreName.ToLower() },
                    { "project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                    { "id", -1 }
                };

                var _data_push = JsonConvert.SerializeObject(j_param);
                var response_queue = work_queue.InsertQueueSimpleSyncES(_data_push);

                if (response_queue)
                {
                    return new JsonResult(new { isSuccess = true, message = "Push queue thành công" });
                }
                else
                {
                    return new JsonResult(new { isSuccess = false, message = "Push queue thất bại" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { isSuccess = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult PushAll()
        {
            var storeMapping = new Dictionary<string, string>
            {
                { "Authentication", "hulotoys_sp_getaccountaccessapi" },
                { "AccountClient", "hulotoys_sp_getaccountclient" },
                { "AddressClient", "hulotoys_sp_getaddressclient" },
                { "Article", "hulotoys_sp_getallarticle" },
                { "ArticleCategory", "hulotoys_sp_getarticlecategory" },
                { "Tag", "hulotoys_sp_gettag" },
                { "Order", "hulotoys_sp_getorder" },
                { "OrderDetail", "hulotoys_sp_getorderdetail" },
                { "Client", "hulotoys_sp_getclient" },
                { "Raiting", "hulotoys_sp_getrating" },
                { "LocationProduct", "hulotoys_sp_getlocationproduct" },
                { "ArticleRelated", "hulotoys_sp_getarticlerelated" },
                { "ArticleTag", "hulotoys_sp_getarticletagdata" },
                { "GroupProduct", "hulotoys_sp_getgroupproduct" },
                { "Provinces", "hulotoys_sp_getprovince" },
                { "Districts", "hulotoys_sp_getdistrict" },
                { "Wards", "hulotoys_sp_getward" },
                { "Product", "hulotoys_mongodb_product" },
                { "AttachFile", "hulotoys_sp_getattachfile" },
                { "FlashSale", "hulotoys_sp_getflashsale" },
                { "FlashSaleProduct", "hulotoys_sp_getflashsaleproduct" },
                { "Supplier", "hulotoys_sp_getsupplier" },
                { "OrderMerge", "hulotoys_sp_getordermerge" }
            };

            foreach (var value in storeMapping.Values)
            {
                // Lấy phần sau "hulotoys_"
                var storeName = value.Replace("hulotoys_", "");

                var j_param = new Dictionary<string, object>
                {
                    { "store_name", storeName },
                    { "index_es", "hulotoys_" + storeName.ToLower() },
                    { "project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                    { "id", -1 }
                };

                var _data_push = JsonConvert.SerializeObject(j_param);
                var response_queue = work_queue.InsertQueueSimpleSyncES(_data_push);
            }
            return new JsonResult(new { isSuccess = true, message = "Push queue thành công" });

        }
    }
}
