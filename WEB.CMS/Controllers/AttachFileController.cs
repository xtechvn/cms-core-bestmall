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
                        try { System.IO.File.Delete(full_path); } catch { }

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
        [HttpPost]
        public async Task<IActionResult> ConfirmAttachFile(List<AttachfileViewModel> files, int data_id,int type)
        {
            try
            {
                List<AttachfileViewModel> urls = new List<AttachfileViewModel>();
                if (files != null && files.Count > 0)
                {
                    var current_attachs = await _AttachFileRepository.GetListByDataID(data_id, type);
                    if (current_attachs != null && current_attachs.Count>0) {
                        var del_attach = current_attachs.Where(x => !files.Select(y => y.id).Contains(x.Id));
                        if (del_attach.Any())
                        {
                            foreach (var att in del_attach)
                            {
                               await _AttachFileRepository.Delete(att.Id, 0);
                            }
                        }
                    }
                    foreach (var file in files) 
                    {
                        if (file.id > 0 && file.path.Contains(static_domain)) { continue; }
                        string full_path = Directory.GetCurrentDirectory() + "\\wwwroot\\" + file.path.Replace("/", "\\");
                        try
                        {
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
                            if (url != null && url.Trim() != "")
                            {
                                file.path = static_domain + url;
                                urls.Add(file);
                            }
                            try { System.IO.File.Delete(full_path); } catch { }
                            var item = new Entities.Models.AttachFile();
                            if (current_attachs!=null && file.id>0 && current_attachs.Any(x => x.Id == file.id))
                            {
                                item = current_attachs.FirstOrDefault(x => x.Id == file.id);


                            }
                            await _AttachFileRepository.AddAttachFile(new Entities.Models.AttachFile()
                            {
                                Id = file.id,
                                Capacity=0,
                                CreateDate=item.CreateDate??DateTime.Now,
                                DataId=data_id,
                                Ext=file.ext,
                                Path=file.path,
                                Type=type,
                                UserId=0
                                
                            });

                        }
                        catch
                        {
                            continue;
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
                LogHelper.InsertLogTelegram("ConfirmAttachFile - AttachFileController" + ex.ToString());
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Failed"
            });
        }
    }
}
