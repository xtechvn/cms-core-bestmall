using Amazon.Runtime.Internal.Transform;
using Caching.RedisWorker;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.News;
using HuloToys_Service.ElasticSearch;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Common;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.RabitMQ;
using WEB.CMS.Service.News;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class NewsController : Controller
    {
        private const int NEWS_CATEGORY_ID = 10;
        private const int VIDEO_NEWS_CATEGORY_ID = 1;
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly IArticleRepository _ArticleRepository;
        private readonly IUserRepository _UserRepository;
        private readonly ICommonRepository _CommonRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient work_queue;
        private readonly QueueService _queueService;
        private readonly RedisConn _redisConn;
        private readonly ArticleCategoryESService articleCategoryESService;
        private readonly string _UrlStaticImage;

        public NewsController(IConfiguration configuration, IArticleRepository articleRepository, IUserRepository userRepository, ICommonRepository commonRepository, IWebHostEnvironment hostEnvironment, QueueService queueService,
            IGroupProductRepository groupProductRepository, ArticleCategoryESService _articleCategoryESService, IOptions<DomainConfig> domainConfig)
        {
            _ArticleRepository = articleRepository;
            _CommonRepository = commonRepository;
            _UserRepository = userRepository;
            _WebHostEnvironment = hostEnvironment;
            _configuration = configuration;
            _GroupProductRepository = groupProductRepository;
            work_queue = new WorkQueueClient(configuration);
            _queueService = queueService;
            _redisConn = new RedisConn(configuration);
            _redisConn.Connect();
            articleCategoryESService = _articleCategoryESService;
            _UrlStaticImage = domainConfig.Value.ImageStatic;
        }

        public async Task<IActionResult> Index()
        {
            var NEWS_CATEGORY_ID = Convert.ToInt32(_configuration["Config:default_news_root_group"]);
            ViewBag.ListArticleStatus = await _CommonRepository.GetAllCodeByType(AllCodeType.ARTICLE_STATUS);
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1);
            ViewBag.ListAuthor = await _UserRepository.GetUserSuggestionList(string.Empty);
            return View();
        }

        /// <summary>
        /// Search News
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Search(ArticleSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<ArticleViewModel>();
            try
            {
                model = _ArticleRepository.GetPagingList(searchModel, currentPage, pageSize);
                ViewBag.ListID = (model != null && model.ListData != null && model.ListData.Select(x => x.Id).ToList() != null && model.ListData.Select(x => x.Id).ToList().Count > 0) ? JsonConvert.SerializeObject(model.ListData.Select(x => x.Id).ToList()) : "";
            }
            catch
            {

            }
            return PartialView(model);
        }

        public async Task<IActionResult> Detail(long Id)
        {
            var model = new ArticleModel();
            var size_img = ReadFile.LoadConfig().SIZE_IMG;
            ViewBag.size_img = size_img;
            if (Id > 0)
            {
                model = await _ArticleRepository.GetArticleDetail(Id);
            }
            else
            {
                model.Status = ArticleStatus.SAVE;
            }
            var NEWS_CATEGORY_ID = Convert.ToInt32(_configuration["Config:default_news_root_group"]);
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1, model.Categories);
            return View(model);
        }

        public async Task<string> GetSuggestionTag(string name)
        {
            try
            {
                var tagList = await _ArticleRepository.GetSuggestionTag(name);
                return JsonConvert.SerializeObject(tagList);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IActionResult> RelationArticle(long Id)
        {
            var NEWS_CATEGORY_ID = Convert.ToInt32(_configuration["Config:default_news_root_group"]);
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1);
            ViewBag.ListAuthor = await _UserRepository.GetUserSuggestionList(string.Empty);
            return PartialView();
        }

        [HttpPost]
        public IActionResult RelationSearch(ArticleSearchModel searchModel, int currentPage = 1, int pageSize = 10)
        {
            var model = new GenericViewModel<ArticleViewModel>();
            try
            {
                model = _ArticleRepository.GetPagingList(searchModel, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }
        public async Task<IActionResult> AddOrUpdate(int id, int parent_id)
        {
            var model = new ArticleViewModel
            {

                Status = 0
            };

            if (id > 0)
            {
                var article = await _ArticleRepository.GetArticleDetail(id);
                model = new ArticleViewModel
                {
                    Id = article.Id,
                    Status = article.Status,
                    CampaignName = article.CampaignName,
                    AiContent = article.AiContent,
                    PlatForm = article.PlatForm,
                    AimodelType = article.AimodelType
                };
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetFanpageImages(long articleId)
        {
            var images = await _ArticleRepository.GetFanpageImagesAsync(articleId);
            return Json(images);
        }


        [HttpPost]
        public async Task<IActionResult> SaveFanpageImages([FromBody] FanpageImageSaveModel model)
        {
            if (model.ArticleId <= 0 || model.Images == null || !model.Images.Any())
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            await _ArticleRepository.SaveFanpageImagesAsync(model.ArticleId, model.Images);

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> ConvertImagesBeforePost([FromBody] List<string> images)
        {
            var processedImages = new List<string>();

            foreach (var image in images)
            {
                var url = await UpLoadHelper.UploadBase64Src(image, _UrlStaticImage);
                // ✅ Nếu URL trả về KHÔNG chứa static domain thì gắn vào
                if (!string.IsNullOrEmpty(url) && !url.Contains(_UrlStaticImage))
                {
                    url = _UrlStaticImage + url;
                }
                if (!string.IsNullOrEmpty(url))
                {
                    processedImages.Add(url);
                }
            }

            return Ok(processedImages);
        }

        [HttpPost]
        public async Task<IActionResult> UpSert([FromBody] object data)
        {
            try
            {
                // Thiết lập cài đặt Json để bỏ qua các giá trị null và các thành phần thiếu
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                // Deserialize dữ liệu từ request body
                var model = JsonConvert.DeserializeObject<ArticleModel>(data.ToString(), settings);
                // Nếu là bài viết tạo từ AI để đăng Fanpage thì chỉ cần nội dung (Body)
                bool isFanpageAi = model.AimodelType == 1 && model.PlatForm == 1;

                // Lấy giá trị mặc định của NEWS_CATEGORY_ID từ cấu hình
                var NEWS_CATEGORY_ID = Convert.ToInt32(_configuration["Config:default_news_root_group"]);

                // Nếu bài viết có danh mục và danh mục là GroupHeader, thêm NEWS_CATEGORY_ID vào danh mục
                if (await _GroupProductRepository.IsGroupHeader(model.Categories))
                {
                    model.Categories.Add(NEWS_CATEGORY_ID);
                }

                // Lấy thông tin AuthorId từ token người dùng đăng nhập
                if (model != null && HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    model.AuthorId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                // Kiểm tra xem nội dung bài viết có trống không
                model.Body = ArticleHelper.HighLightLinkTag(model.Body);
                // Nếu là AI Fanpage, gán mặc định cho Title và Lead nếu bị null
                if (isFanpageAi)
                {
                    model.Categories = null;
                    model.Title = model.CampaignName;
                    model.Lead = model.AiContent;
                }
                // ✅ Kiểm tra điều kiện bắt buộc
                if (string.IsNullOrWhiteSpace(model.Body))
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Phần nội dung bài viết không được để trống"
                    });
                }
                // ✅ Nếu KHÔNG phải Fanpage-AI → kiểm tra thêm tiêu đề và mô tả
                if (!isFanpageAi)
                {
                    if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Lead))
                    {
                        return Json(new
                        {
                            isSuccess = false,
                            message = "Phần Tiêu đề và Mô tả không được để trống"
                        });
                    }

                    if (model.Lead.Length > 400)
                    {
                        return Json(new
                        {
                            isSuccess = false,
                            message = "Phần Mô tả không được vượt quá 400 ký tự"
                        });
                    }
                }

                // Lưu bài viết và lấy ID của bài viết đã được lưu
                var articleId = await _ArticleRepository.SaveArticle(model);

                // Kiểm tra xem quá trình lưu bài viết có thành công không
                if (articleId > 0)
                {
                   await ClearCacheArticle(articleId, model.Categories);

                    // Trả về kết quả thành công với ID của bài viết
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        dataId = articleId
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
                // Ghi log lỗi và trả về thông báo lỗi
                LogHelper.InsertLogTelegram("UpSert - NewsController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }


        public async Task<IActionResult> ChangeArticleStatus(long Id, int articleStatus)
        {
            try
            {
                var _ActionName = string.Empty;

                switch (articleStatus)
                {
                    case ArticleStatus.PUBLISH:
                        _ActionName = "Đăng bài viết";
                        break;

                    case ArticleStatus.REMOVE:
                        _ActionName = "Hạ bài viết";
                        break;
                }

                var rs = await _ArticleRepository.ChangeArticleStatus(Id, articleStatus);

                if (rs > 0)
                {
                    //  clear cache article
                    var Categories = await _ArticleRepository.GetArticleCategoryIdList(Id);
                    ClearCacheArticle(Id, Categories);

                    // Tạo message để push vào queue
                    var j_param = new Dictionary<string, object>
                            {
                                 { "store_name", "SP_GetAllArticle" },
                                { "index_es", "hulotoys_sp_getallarticle" },
                                {"project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                                  {"id" , Id }

                            };
                    var _data_push = JsonConvert.SerializeObject(j_param);
                    // Push message vào queue
                    var response_queue = work_queue.InsertQueueSimple(_data_push, QueueName.queue_app_push);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = _ActionName + " thành công",
                        dataId = Id
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = _ActionName + " thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeArticleStatus - NewsController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> DeleteArticle(long Id)
        {
            try
            {
                var Categories = await _ArticleRepository.GetArticleCategoryIdList(Id);
                var rs = await _ArticleRepository.DeleteArticle(Id);

                if (rs > 0)
                {
                    //  clear cache article
                    ClearCacheArticle(Id, Categories);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa bài viết thành công",
                        dataId = Id
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa bài viết thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteArticle - NewsController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task ClearCacheArticle(long articleId, List<int> category_ids)
        {
            try
            {
                int db_index = Convert.ToInt32(_configuration["Redis:Database:db_common"]);
                _redisConn.clear(CacheName.ARTICLE + articleId, db_index);
                _redisConn.clear(CacheName.ARTICLE_ID_TAGS + articleId, db_index);
                await articleCategoryESService.DeleteByArticleId(articleId);
                // Xóa cache của bài viết sau khi cập nhật
                if (category_ids != null && category_ids.Count > 0)
                {

                    foreach (var category in category_ids)
                    {
                        await  _redisConn.DeleteCacheByKeyword(CacheName.ARTICLE_CATEGORY + category, db_index);
                        var exists = await _ArticleRepository.FindCategoryByArticleIdAndCategoryId(articleId, category);
                        if (exists != null && exists.Id > 0)
                        {
                           await articleCategoryESService.Insert(new HuloToys_Service.Models.Article.ArticleCategoryESModel()
                            {
                                articleid = articleId,
                                categoryid = category,
                                Id = exists.Id
                            });
                        }

                    }
                }
                // Tạo message để push vào queue
                var j_param = new Dictionary<string, object>
                            {
                               { "store_name", "SP_GetAllArticle" },
                                { "index_es", "hulotoys_sp_getallarticle" },
                                {"project_type", Convert.ToInt16(ProjectType.HULOTOYS) },
                                  {"id" , articleId }

                            };
                var _data_push = JsonConvert.SerializeObject(j_param);
                // Push message vào queue
                var response_queue = work_queue.InsertQueueSimple(_data_push, QueueName.queue_app_push);


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClearCacheArticle - articleId:" + articleId + "\ncategory_ids" +((category_ids == null || category_ids.Count <= 0) ?"[]":"["+string.Join(",",category_ids)+"]") + ex.ToString());
            }
        }
        [HttpPost]
        public async Task<List<NewsViewCount>> GetPageViewByList(List<long> article_id)
        {
            try
            {
                NewsMongoService news_services = new NewsMongoService(_configuration);
                return await news_services.GetListViewedArticle(article_id);
            }
            catch
            {

            }
            return null;
        }

    }
}
