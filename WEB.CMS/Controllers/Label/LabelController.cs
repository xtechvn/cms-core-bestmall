using Caching.RedisWorker;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Utilities;
using WEB.CMS.Customize;
using Entities.ViewModels.Label;
using Entities.Models;
using WEB.Adavigo.CMS.Service;
using System.Security.Claims;
using Utilities.Contants;
using WEB.CMS.Models.Product;
using WEB.CMS.Controllers.Bussiness;
using Utilities.Common;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class LabelController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILabelRepository _labelRepository;
        private readonly RedisConn _redisService;
        private RedisConn _redisConn;
        private readonly int db_index = 9;
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly LabelService labelService;

        public LabelController(IConfiguration configuration, RedisConn redisService, ILabelRepository labelRepository)
        {
            _configuration = configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            _labelRepository = labelRepository;
            _redisConn = new RedisConn(configuration);
            _redisConn.Connect();
            db_index = Convert.ToInt32(configuration["Redis:Database:db_search_result"]);
            _productV2DetailMongoAccess = new ProductDetailMongoAccess(configuration);
            labelService = new LabelService(configuration);
        }
        [HttpPost]
        public async Task<IActionResult> SearchLabel(string txt_search)
        {
            try
            {
                var list = await _labelRepository.Listing(0, txt_search, null, 1, 20);
                return new JsonResult(new
                {
                    isSuccess = true,
                    data = list
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SearchLabel - LabelController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Search(LabelSearchRequestViewModel searchModel)
        {
            var model = new GenericViewModel<LabelListingModel>();
            try
            {
                var data = await _labelRepository.Listing(searchModel.status, searchModel.name, searchModel.code, searchModel.currentPage, searchModel.pageSize);
                model.CurrentPage = searchModel.currentPage;
                model.ListData = data;
                model.PageSize = searchModel.pageSize;
                model.TotalRecord = data != null && data.Any() ? data.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.pageSize);
                string static_domain = _configuration["DomainConfig:ImageStatic"];
                ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - LabelController: " + ex);
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int label_id)
        {
            ViewBag.Data = new Label();
            try
            {
                ViewBag.StaticDomain = _configuration["DomainConfig:ImageStatic"];

                if (label_id > 0)
                {
                    ViewBag.Data = await _labelRepository.GetById(label_id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Edit - LabelController: " + ex);
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpSert(Label updated_model)
        {
            bool is_add_new = updated_model != null && updated_model.Id > 0;

            try
            {
                if (updated_model == null
                    || updated_model.LabelCode == null || updated_model.LabelCode.Trim() == ""
                    || updated_model.LabelName == null || updated_model.LabelName.Trim() == ""
                    || updated_model.Icon == null || updated_model.Icon.Trim() == ""
                    )
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        msg = "Dữ liệu không chính xác, vui lòng thử lại"
                    });
                }
                if (updated_model.Status == null) updated_model.Status = 0;
                if (updated_model.Id <= 0)
                {
                    var exists_label = await _labelRepository.GetByCode(updated_model.LabelCode.ToUpper().Trim());
                    if (exists_label != null && exists_label.Id > 0)
                    {
                        return new JsonResult(new
                        {
                            isSuccess = false,
                            msg = "Mã thương hiệu trùng lặp, vui lòng sử dụng mã thương hiệu khác"
                        });
                    }
                }
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                string static_domain = _configuration["DomainConfig:ImageStatic"];
                //--Upload ảnh nếu ảnh chưa được upload:
                if (updated_model.Icon != null && updated_model.Icon.Trim() != ""
                    && !updated_model.Icon.Contains(static_domain) && updated_model.Icon.Trim().StartsWith("\\"))
                {
                    updated_model.Icon = await labelService.UploadLabelImage(updated_model.Icon);
                }
                if (updated_model.Banner != null && updated_model.Banner.Trim() != ""
                   && !updated_model.Banner.Contains(static_domain) && updated_model.Banner.Trim().StartsWith("\\"))
                {
                    updated_model.Banner = await labelService.UploadLabelImage(updated_model.Banner);
                    if (updated_model.Banner != null && updated_model.Banner.Trim() != ""
                   && !updated_model.Banner.Contains(static_domain)) updated_model.Banner = static_domain + updated_model.Banner;
                }
                updated_model.UpdatedBy = _UserLogin;
                updated_model.UpdateTime = DateTime.Now;
                if (updated_model.Id > 0)
                {
                    var exists = await _labelRepository.GetById(updated_model.Id);
                    if (exists != null && exists.Id > 0)
                    {
                        updated_model.CreateTime = exists.CreateTime;
                        updated_model.CreatedBy = exists.CreatedBy;
                        _labelRepository.Update(updated_model);

                    }
                    else
                    {
                        return Ok(new
                        {
                            isSuccess = false,
                            msg = (is_add_new ? "Cập nhật" : "Thêm mới") + " thương hiệu thất bại, vui lòng xem lại thông tin hoặc liên hệ admin"
                        });
                    }
                }
                else
                {

                    updated_model.CreateTime = DateTime.Now;
                    updated_model.CreatedBy = _UserLogin;
                    _labelRepository.Insert(updated_model);
                }
                _redisConn.clear(CacheName.LABEL+updated_model.Id, db_index);
                _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_LISTING, db_index);
                _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_DETAIL, db_index);
                return Ok(new
                {
                    isSuccess = true,
                    msg = (is_add_new ? "Cập nhật" : "Thêm mới") + " thương hiệu thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - LabelController: " + ex);
            }
            return Ok(new
            {
                isSuccess = false,
                msg = (is_add_new ? "Cập nhật" : "Thêm mới") + " thương hiệu thất bại, vui lòng xem lại thông tin hoặc liên hệ admin"
            });
        }
    }
}
