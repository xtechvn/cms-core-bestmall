using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class LabelController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILabelRepository _labelRepository;
        private readonly RedisConn _redisService;

        public LabelController(IConfiguration configuration, RedisConn redisService, ILabelRepository labelRepository)
        {
            _configuration = configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            _labelRepository = labelRepository;
        }
        [HttpPost]
        public async Task<IActionResult> Search(string txt_search)
        {
            try
            {
                var list = await _labelRepository.Listing(0,txt_search, 1,20);
                return new JsonResult(new
                {
                    isSuccess = true,
                    data = list
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - LabelController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
    }
}
