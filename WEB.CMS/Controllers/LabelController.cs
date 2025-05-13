using Caching.RedisWorker;
using Entities.ViewModels.Funding;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;
using WEB.CMS.Customize;
using Entities.ViewModels.Label;
using Entities.Models;
using System.Collections.Generic;
using WEB.Adavigo.CMS.Service;
using System.Security.Claims;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class LabelController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILabelRepository _labelRepository;
        private readonly RedisConn _redisService;
        private readonly StaticAPIService staticAPIService;

        public LabelController(IConfiguration configuration, RedisConn redisService, ILabelRepository labelRepository)
        {
            _configuration = configuration;
            _redisService = new RedisConn(configuration);
            _redisService.Connect();
            _labelRepository = labelRepository;
            staticAPIService = new StaticAPIService(configuration);

        }
        [HttpPost]
        public async Task<IActionResult> SearchLabel(string txt_search)
        {
            try
            {
                var list = await _labelRepository.Listing(0,txt_search,null, 1,20);
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
                var data = await _labelRepository.Listing(searchModel.status, searchModel.name,searchModel.code,  searchModel.currentPage, searchModel.pageSize);
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
            try
            {
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                string static_domain = _configuration["DomainConfig:ImageStatic"];
                //--Upload ảnh nếu ảnh chưa được upload:
                if (updated_model.Icon!=null && updated_model.Icon.Trim()!="" 
                    &&!updated_model.Icon.Contains(static_domain) && updated_model.Icon.Trim().StartsWith("/"))
                {
                    string full_path = Directory.GetCurrentDirectory() + "\\wwwroot\\" + updated_model.Icon.Replace("/", "\\");
                    try
                    {
                        // Đọc toàn bộ nội dung của file ảnh dưới dạng byte array
                        byte[] imageBytes = System.IO.File.ReadAllBytes(full_path);

                        // Chuyển đổi byte array thành chuỗi Base64
                        string base64String = Convert.ToBase64String(imageBytes);

                        var path = updated_model.Icon.Split(".");

                        Utilities.ViewModels.Article.ImageBase64 image = new()
                        {
                            ImageData = base64String,
                            ImageExtension = path[path.Length - 1]
                        };
                        var url = await staticAPIService.UploadImageBase64(image);
                        if (url != null && url.Trim() != "")
                        {
                            updated_model.Icon = static_domain + url;
                        }
                    }
                    catch
                    {

                    }
                    try { System.IO.File.Delete(full_path); } catch { }
                }
                updated_model.UpdatedBy = _UserLogin;
                updated_model.UpdateTime = DateTime.Now;
                if (updated_model.Id > 0)
                {
                    var exists= await _labelRepository.GetById(updated_model.Id);
                    if(exists!=null && exists.Id > 0)
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
                            msg = (updated_model.Id > 0 ? "Cập nhật" : "Thêm mới") + " thương hiệu thất bại, vui lòng xem lại thông tin hoặc liên hệ admin"
                        });
                    }
                }
                else
                {
                   
                    updated_model.CreateTime = DateTime.Now;
                    updated_model.CreatedBy = _UserLogin;
                    _labelRepository.Insert(updated_model);
                }

                
                return Ok(new
                {
                    isSuccess = true,
                    msg= (updated_model.Id > 0 ? "Cập nhật":"Thêm mới")+" thương hiệu thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - LabelController: " + ex);
            }
            return Ok(new
            {
                isSuccess = false,
                msg = (updated_model.Id > 0 ? "Cập nhật" : "Thêm mới") + " thương hiệu thất bại, vui lòng xem lại thông tin hoặc liên hệ admin"
            });
        }
    }
}
