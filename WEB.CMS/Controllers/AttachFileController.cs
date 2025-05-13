using Entities.ViewModels.Attachment;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    public class AttachFileController : Controller
    {
        private readonly IAttachFileRepository _AttachFileRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly Models.AppSettings config;
        private readonly StaticAPIService staticAPIService;
        private readonly IConfiguration _configuration;
        private readonly string static_domain = "";

        public AttachFileController(IAttachFileRepository attachFileRepository, IWebHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _AttachFileRepository = attachFileRepository;
            _WebHostEnvironment = hostEnvironment;
            config = ReadFile.LoadConfig();
            staticAPIService=new StaticAPIService(configuration);
            _configuration = configuration;
            static_domain = configuration["DomainConfig:ImageStatic"];
        }

        public async Task<IActionResult> Upload(IFormFile[] files)
        {
            try
            {
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                List<string> urls = new List<string>();
                if (files != null && files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        string _FileName = file.FileName;
                        string _UploadFolder = @"uploads/images/"+ _UserLogin;
                        string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                        if (!Directory.Exists(_UploadDirectory))
                        {
                            Directory.CreateDirectory(_UploadDirectory);
                        }
                        string filePath = Path.Combine(_UploadDirectory, _FileName);
                        if (!System.IO.File.Exists(filePath))
                        {
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                        }
                        urls.Add("/" + _UploadFolder + "/" + _FileName);
                    }
                }

                if (urls != null && urls.Count > 0)
                {
                    return new JsonResult(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Thành công",
                        data = urls
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tải tệp đính kèm thất bại",
                        data = urls
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Upload - AttachFileController" + ex.ToString());
            }
            return new JsonResult(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Lỗi trong quá trình tải lên tệp đính kèm, vui lòng liên hệ IT.",
            });
        }
        public async Task<IActionResult> Confirm(List<AttachfileViewModel> files)
        {
            try
            {
                List<string> urls = new List<string>();
                if(files != null && files.Count > 0)
                {
                   
                    foreach (var file in files)
                    {
                        string full_path = Directory.GetCurrentDirectory() + "\\wwwroot\\" + file.path.Replace("/", "\\");
                        // Đọc toàn bộ nội dung của file ảnh dưới dạng byte array
                        byte[] imageBytes = System.IO.File.ReadAllBytes(full_path);

                        // Chuyển đổi byte array thành chuỗi Base64
                        string base64String = Convert.ToBase64String(imageBytes);

                        var path = file.path.Split(".");

                        Utilities.ViewModels.Article.ImageBase64 image = new()
                        {
                            ImageData = base64String,
                            ImageExtension = path[path.Length - 1]
                        };
                        var url = await staticAPIService.UploadImageBase64(image);
                        if(url!=null && url.Trim() != "")
                        {
                            urls.Add(url);
                        }
                    }
                  
                }
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Thành công",
                    data = urls
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Confirm - AttachFileController" + ex.ToString());
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Failed"
            });
        }
    }
}
