using Entities.Models;
using ENTITIES.ViewModels.ElasticSearch;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Controllers.Elastic.Bussiness;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers
{
    public class AllCodeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IAllCodeRepository _allCodeRepository;


        public AllCodeController(IConfiguration configuration, IAllCodeRepository allCodeRepository)
        {
            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
           
        }
        [HttpPost]
        public async Task<IActionResult> AddAllCode(AllCode request)
        {

            try
            {
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                request.CreateDate = DateTime.Now;
                request.CreatedBy= _UserId;

                var id=await _allCodeRepository.Create(request);
                if (id > 0)
                {
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Success - "+id
                    });
                }
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddAllCode - AllCodeController: " + ex.ToString());

            }
            return Ok(new
            {
                is_success = false,
                msg = "Failed"
            });

        }
        [HttpPost]
        public async Task<IActionResult> ExcuteCommand(string request)
        {
            var result = _allCodeRepository.Excute(request);
            return Ok(new
            {
                msg = result,
            });

        }
    }
}
