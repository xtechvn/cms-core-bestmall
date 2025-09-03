using Aspose.Cells;
using Caching.RedisWorker;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Common;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models.Product;
using WEB.CMS.RabitMQ;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class GroupProductController : Controller
    {
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly IPositionRepository _PositionRepository;
        private readonly IAllCodeRepository _AllCodeRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly string _UrlStaticImage;
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient work_queue;
        private readonly RedisConn _redisService;
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;

        public GroupProductController(IGroupProductRepository groupProductRepository, ProductDetailMongoAccess productV2DetailMongoAccess,
               IWebHostEnvironment hostEnvironment, IPositionRepository positionRepository,
               RedisConn redisService, IAllCodeRepository allCodeRepository, IOptions<DomainConfig> domainConfig, IConfiguration configuration)
        {
            _GroupProductRepository = groupProductRepository;
            _WebHostEnvironment = hostEnvironment;
            _PositionRepository = positionRepository;
            work_queue = new WorkQueueClient(configuration);

            _AllCodeRepository = allCodeRepository;
            _UrlStaticImage = domainConfig.Value.ImageStatic;
            _configuration = configuration;
            _redisService = redisService;
            _redisService.Connect();
            _productV2DetailMongoAccess = productV2DetailMongoAccess;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Search(string Name, int Status = -1)
        {
            return await _GroupProductRepository.GetListTreeView(Name, Status);
        }

        /// <summary>
        /// Add Or Update GroupProduct
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">
        /// 0: Add child
        /// 1: Edit itseft
        /// </param>
        /// <returns></returns>
        public async Task<IActionResult> AddOrUpdate(int id, int type)
        {
            var model =  new GroupProduct()
            {
                Id = 0,
                Status = 0,
                OrderNo = 0,
                ParentId = id
            };
            try
            {
                string static_domain = _configuration["DomainConfig:ImageStatic"];
                ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
                if (type != 0)
                {
                    model = await _GroupProductRepository.GetById(id);

                }
            }
            catch
            {

            }

            ViewBag.PositionList = await _PositionRepository.GetAll();
            return View(model);
        }

        /// <summary>
        /// public async Task<IActionResult> UpSert(IFormFile imageFile, string imageSize, GroupProductUpsertModel model)
        /// </summary>
        /// <param name="imageFile"></param>
        /// <param name="imageSize"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpSert(GroupProductUpsertModel model)
        {
            try
            {
                var upsertModel = new GroupProduct()
                {
                    Id = model.Id,
                    Name = model.Name,
                    OrderNo = model.OrderNo,
                    ParentId = model.ParentId,
                    Description = model.Description,
                    PositionId = model.PositionId,
                    Status = model.Status,
                    //ImagePath = await UpLoadHelper.UploadBase64Src(model.ImageBase64, _UrlStaticImage),
                    ImagePath = model.ImagePath,
                    IsShowHeader = model.IsShowHeader,
                    IsShowFooter = model.IsShowFooter,
                    ModifiedOn = DateTime.Now,
                    Code = model.Code,
                    ProductCount=(model.Id>0? await _productV2DetailMongoAccess.CountByGroupId(model.Id):0),
                    IsFlashSale=model.IsFlashSale,
                    CreatedOn=DateTime.Now,


                };
                if (model.ImagePath == null|| model.ImagePath.Trim()=="")
                {
                    model.ImageBase64= ImageResizerLegacy.AutoReduceImageQualityBase64(model.ImageBase64);
                    upsertModel.ImagePath = await UpLoadHelper.UploadBase64Src(model.ImageBase64, _UrlStaticImage);
                }
                else if(model.ImagePath.Contains("base64") || model.ImagePath.Contains("data:video"))
                {
                    model.ImagePath = ImageResizerLegacy.AutoReduceImageQualityBase64(model.ImagePath);
                    upsertModel.ImagePath = await UpLoadHelper.UploadBase64Src(model.ImagePath, _UrlStaticImage);

                }
                else
                {
                    var static_url = _configuration["API:StaticURL"];

                    string url_fixed = upsertModel.ImagePath;
                    if (!url_fixed.Contains(static_url)
                    && !url_fixed.Contains("base64")
                    && !url_fixed.Contains("data:video"))
                    {
                        upsertModel.ImagePath = static_url + upsertModel.ImagePath;
                    }
                    upsertModel.ImagePath = await ImageResizerLegacy.DownloadAndOptimizeImageAsync(model.ImagePath, _UrlStaticImage);

                }
                var rs = await _GroupProductRepository.UpSert(upsertModel);
                if (rs > 0)
                {
                    _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU + rs, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU + upsertModel.ParentId, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU + 188, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU_FOOTER + 188, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    _redisService.clear("GROUP_PRODUCT_FLASHSALE_" + model.ParentId, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    await _redisService.DeleteCacheByKeyword(CacheName.ARTICLE_CATEGORY_MENU, Convert.ToInt32(_configuration["Redis:Database:db_common"]));


                    try
                    {

                        var j_param = new Dictionary<string, object>
                                {
                                     { "store_name", "SP_GetGroupProduct" },
                                    { "index_es", "hulotoys_sp_getgroupproduct" },
                                    {"project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                                      {"id" , model.Id }

                                };
                        var _data_push = JsonConvert.SerializeObject(j_param);
                        // Push message vào queue
                        var response_queue = work_queue.InsertQueueSimpleSyncES(_data_push);

                    }
                    catch { }
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        modelId = rs,
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tồn tại nhóm hàng cùng cấp"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }



        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rootParentId = await _GroupProductRepository.GetRootParentId(id);
                var rs = await _GroupProductRepository.Delete(id);

                if (rs > 0)
                {
                    _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU+id, Convert.ToInt32(_configuration["Redis:Database:db_common"]));


                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thành công."
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Nhóm hàng đang được sử dụng. Bạn không thể xóa."
                    });
                }
                else if (rs == -2)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Nhóm hàng đang có cấp con. Bạn không thể xóa."
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thất bại."
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Obsolete]
        public IActionResult UploadExcel(IFormFile fileCrawl)
        {
            try
            {
                var listLink = new List<string>();
                if (fileCrawl == null)
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = new List<String>(),
                        Message = "Vui lòng chọn file."
                    });
                }
                if (!fileCrawl.FileName.Contains("xlsx") && !fileCrawl.FileName.Contains("xsl"))
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = new List<String>(),
                        Message = "File không đúng định dạng. Vui lòng nhập định dạng là file excel."
                    });
                }
                if (fileCrawl.Length > 10000000)
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = new List<String>(),
                        Message = "File bạn tải lên quá 10MB. Vui lòng nhập file có kích thước nhỏ hơn 10MB."
                    });
                }
                Workbook workbook = new Workbook(fileCrawl.OpenReadStream());
                var worksheet = workbook.Worksheets[0];
                var listLinkWrong = new List<string>();//list link khong hop le
                if (worksheet.Cells.Count > 0)
                {
                    //truong hop link trong file khong dung dinh dang
                    var list = worksheet.Cells;
                    for (int i = 1; i < list.Count; i++)
                    {
                        if (list[i].Value == null || string.IsNullOrEmpty(list[i].Value.ToString()))
                        {
                            continue;
                        }
                        listLinkWrong.Add(list[i].Value.ToString());

                    }
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = 2,
                        Data = listLink,
                        DataLinkWrong = listLinkWrong,
                        Message = "Bạn chưa nhập link vào file excel"
                    });
                }
                return new JsonResult(new
                {
                    Code = 1,
                    Data = listLink,
                    DataLinkWrong = listLinkWrong,
                    Message = "Thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UploadExcelAsync: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    DataLinkWrong = new List<String>(),
                    Message = "Lỗi khi gửi file lên server."
                });
            }
        }

        public IActionResult AddCampaign()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GetAllGroup()
        {
            try
            {
                var listGroup = await _GroupProductRepository.GetAll();
                return new JsonResult(new
                {
                    Data = listGroup
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllGroup: " + ex);
                return new JsonResult(new
                {
                    Data = new List<GroupProduct>()
                }); ;
            }
        }

        [HttpPost]
        public IActionResult GetAllPosition()
        {
            try
            {
                var listPosition = _PositionRepository.GetAll();
                return new JsonResult(new
                {
                    Code = 1,
                    Data = listPosition,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllPosition - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<Position>(),
                });
            }
        }

        public IActionResult AddOrUpdatePositionAsync(int id)
        {
            ViewBag.listPosition = _PositionRepository.GetListAll();
            return View();
        }

        [HttpPost]
        public IActionResult AddPositionJson(Position position)
        {
            try
            {
                var postionExists = _PositionRepository.GetByPositionName(position.PositionName);
                if (postionExists != null && postionExists.Result != null)
                {
                    return new JsonResult(new
                    {
                        Code = -2,
                        Message = "Tên kích thước đã tồn tại. Vui lòng nhập tên khác."
                    });
                }
                var rs = _PositionRepository.Create(position);
                if (rs.Result > -1)
                {
                    return new JsonResult(new
                    {
                        Code = 1,
                        Message = "Thêm mới kích thước thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        Code = -1,
                        Message = "Thêm mới kích thước thất bại"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddPositionJson - GroupProductController: " + ex);
                return new JsonResult(new
                {
                    Code = 0,
                    Data = new List<String>(),
                    Message = "Lỗi thêm mới kích thước."
                });
            }
        }

        public IActionResult UpdatePosition(int id)
        {
            try
            {
                var model = _PositionRepository.GetById(id);
                return View(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdatePosition - GrouProduct: " + ex);
                return View();
            }
        }


        public async Task<IActionResult> Clearcache(int id,string name)
        {
            try
            {

                _redisService.clear(CacheName.ARTICLE_B2C_CATEGORY_MENU + id, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                return new JsonResult(new
                {
                    isSuccess = true,
                    message = "Clear cache thành công cho chuyên mục "+ name + " có id là "+id+"."
                });

            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetByParendId(int parent_id)
        {
            try
            {
                var listGroup = await _GroupProductRepository.getCategoryByParentId(parent_id);
                return new JsonResult(new
                {
                   is_success=true,
                   data=listGroup
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByParendId: " + ex);
                return new JsonResult(new
                {
                    is_success = false,
                    data = new List<GroupProduct>()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CorrectImages()
        {
            try
            {
                var listGroup = await _GroupProductRepository.GetAll();
                if(listGroup!=null && listGroup.Count > 0)
                {
                    foreach(var group in listGroup)
                    {
                        group.ImagePath = await ImageResizerLegacy.DownloadAndOptimizeImageAsync(group.ImagePath, _UrlStaticImage);
                        await _GroupProductRepository.UpSert(group);
                    }
                    var j_param = new Dictionary<string, object>
                                {
                                     { "store_name", "SP_GetGroupProduct" },
                                    { "index_es", "hulotoys_sp_getgroupproduct" },
                                    {"project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                                      {"id" , -1 }

                                };
                    var _data_push = JsonConvert.SerializeObject(j_param);
                    // Push message vào queue
                    var response_queue = work_queue.InsertQueueSimpleSyncES(_data_push);
                }
                return new JsonResult(new
                {
                    is_success = true,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CorrectImages: " + ex);
               
            }
            return new JsonResult(new
            {
                is_success = false,
            });
        }
    }
}