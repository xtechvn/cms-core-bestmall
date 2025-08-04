using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Caching.RedisWorker;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.BankingAccount;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Funding;
using Entities.ViewModels.SupplierConfig;
using ENTITIES.ViewModels.ElasticSearch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Controllers.Bussiness;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class SupplierController : Controller
    {

        private readonly IConfiguration configuration;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly ISupplierRepository _supplierRepository;
        private readonly Models.AppSettings config;
        private readonly IAttachFileRepository _AttachFileRepository;
        private readonly IUserRepository _userRepository;
        private RedisConn _redisConn;
        private SupplierService _supplierService;
        private ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly SupplierESRepository _supplierESRepository;
        private readonly LocationESService _locationESService;
        private readonly int db_index = 9;

        public SupplierController(IAllCodeRepository allCodeRepository, ISupplierRepository supplierRepository, IUserRepository userRepository,
            ICommonRepository commonRepository, IConfiguration _configuration, IWebHostEnvironment webHostEnvironment, IAttachFileRepository attachFileRepository
            , ProductDetailMongoAccess productV2DetailMongoAccess, SupplierESRepository supplierESRepository, LocationESService locationESService, ProductESRepository _productESRepository)
        {
            _allCodeRepository = allCodeRepository;
            _supplierRepository = supplierRepository;
            _commonRepository = commonRepository;
            config = ReadFile.LoadConfig();
            configuration = _configuration;
            _WebHostEnvironment = webHostEnvironment;
            _AttachFileRepository = attachFileRepository;
            _redisConn = new RedisConn(configuration);
            _redisConn.Connect();
            _userRepository = userRepository;
            _supplierService = new SupplierService(configuration, productV2DetailMongoAccess, _productESRepository);
            db_index = Convert.ToInt32(configuration["Redis:Database:db_search_result"]);
            _productV2DetailMongoAccess = productV2DetailMongoAccess;
            _supplierESRepository = supplierESRepository;
            _locationESService = locationESService;
        }

        #region supplier
        public IActionResult Index()
        {
            ViewBag.ServiceTypes = _allCodeRepository.GetListByType("SERVICE_TYPE");
            //ViewBag.Provinces = await _commonRepository.GetProvinceList();
            //ViewBag.Brands = await _brandRepository.GetAll();
            return View();
        }

        [HttpPost]
        public IActionResult Search(SupplierSearchModel searchModel)
        {
            var model = new GenericViewModel<SupplierViewModel>();
            try
            {
                var listSuppliers = _supplierRepository.GetSuppliers(searchModel);
                model.CurrentPage = searchModel.currentPage;
                model.ListData = listSuppliers;
                model.PageSize = searchModel.pageSize;
                model.TotalRecord = listSuppliers != null && listSuppliers.Any() ? listSuppliers.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - SupplierController: " + ex);
            }
            return PartialView(model);
        }

        public async Task<IActionResult> AddOrUpdate(int id)
        {
            var model = new SupplierViewModel();
            ViewBag.Attachment_Root = new List<AttachFile>();
            ViewBag.Attachment_Product = new List<AttachFile>();
            ViewBag.Attachment_Supply = new List<AttachFile>();
            ViewBag.Attachment_Confirm = new List<AttachFile>();
            ViewBag.HaveValidatePermission = false;
            ViewBag.Province = new Province();
            ViewBag.District = new District();
            ViewBag.Ward = new Ward();
            ViewBag.BannerMain = new List<string>();
            ViewBag.BannerSub = new List<string>();
            if (id > 0)
            {
                model = _supplierRepository.GetById(id);
                if(model!=null && model.ProvinceId!=null && model.ProvinceId > 0)
                {
                    ViewBag.Province = _locationESService.GetProvincesById((int)model.ProvinceId);
                }
                if (model != null && model.DistrictId != null && model.DistrictId > 0)
                {
                    ViewBag.District = _locationESService.GetDistrictById((int)model.DistrictId);
                }
                if (model != null && model.WardId != null && model.WardId > 0)
                {
                    ViewBag.Ward = _locationESService.GetWardById((int)model.WardId);
                }

                ViewBag.Attachment_Root = await _AttachFileRepository.GetListByDataID(id, (int)AttachmentType.Supplier_Cert_RootProduct);
                ViewBag.Attachment_Product = await _AttachFileRepository.GetListByDataID(id, (int)AttachmentType.Supplier_Cert_Product);
                ViewBag.Attachment_Supply = await _AttachFileRepository.GetListByDataID(id, (int)AttachmentType.Supplier_Cert_Supply);
                ViewBag.Attachment_Confirm = await _AttachFileRepository.GetListByDataID(id, (int)AttachmentType.Supplier_Cert_Confirm);
                if (model != null && model.BannerMain != null && model.BannerMain.Trim() != "" && model.BannerMain.Trim() != "")
                {
                    ViewBag.BannerMain = JsonConvert.DeserializeObject<List<string>>(model.BannerMain);

                }
                if (model != null && model.BannerSub != null && model.BannerSub.Trim() != "" && model.BannerSub.Trim() != "")
                {
                    ViewBag.BannerSub = JsonConvert.DeserializeObject<List<string>>(model.BannerSub);

                }
                string static_domain = configuration["DomainConfig:ImageStatic"];
                ViewBag.StaticDomain = static_domain;
            }
            try
            {
                int _UserId = 0;

                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                string cache_name = CacheName.USER_ROLE + _UserId + "_" + configuration["CompanyType"];
                var j_data = await _redisConn.GetAsync(cache_name, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                if (j_data != null && j_data.Trim() != "")
                {
                    string decode = CommonHelper.Decode(j_data, configuration["DataBaseConfig:key_api:api_manual"]);
                    UserRoleCacheModel user_role_cache = JsonConvert.DeserializeObject<UserRoleCacheModel>(decode);
                    if (user_role_cache != null && user_role_cache.Permission != null && user_role_cache.Permission.Count() > 0 && user_role_cache.Permission.Any(x => x.MenuId == 22 && x.PermissionId >= (int)Utilities.Contants.SortOrder.SUA))
                    {
                        ViewBag.HaveValidatePermission = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdate - SupplierController: " + ex);
            }
            //ViewBag.HaveValidatePermission = true;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SupplierConfigUpsertModel model)
        {
            try
            {
                var suplier = _supplierRepository.GetByIDOrName(model.SupplierId, model.FullName.Trim());
                if (suplier > 0)
                {
                    {
                        return new JsonResult(new
                        {
                            isSuccess = false,
                            message = "Nhà cung cấp đã tồn tại",
                            data = suplier
                        });
                    }
                }
                var result = _supplierRepository.Add(model);
                await  _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_LISTING, db_index);
                await _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_DETAIL, db_index);

                if (result > 0)
                {
                    try
                    {
                        var exists = _supplierRepository.GetSuplierById(result);
                        string json = JsonConvert.SerializeObject(exists);
                        SupplierESModel sp_es = JsonConvert.DeserializeObject<SupplierESModel>(json.ToLower());
                        await _supplierESRepository.DeleteById(sp_es.supplierid);
                        await _supplierESRepository.InsertAsync(sp_es);
                    }
                    catch { }
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Thêm nhà cung cấp thành công",
                        data = result
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Thêm nhà cung cấp thất bại"
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
        public async Task<IActionResult> Update(SupplierConfigUpsertModel model)
        {
            try
            {
                var result = _supplierRepository.Update(model);
                await _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_LISTING, db_index);
                await _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_DETAIL, db_index);
                if (result > 0)
                {
                    var exists = _supplierRepository.GetById(model.SupplierId);
                    await _supplierService.UpdateSuplierAllProductStatus(exists.SupplierId, (int)exists.Status);
                    try
                    {
                        var exists_model = _supplierRepository.GetSuplierById(result);
                        string json = JsonConvert.SerializeObject(exists_model);
                        SupplierESModel sp_es = JsonConvert.DeserializeObject<SupplierESModel>(json.ToLower());
                        sp_es.id = sp_es.supplierid;
                        await _supplierESRepository.DeleteById(sp_es.supplierid);
                        await _supplierESRepository.InsertAsync(sp_es);
                        await _supplierService.SyncES(model.SupplierId);
                    }
                    catch { }
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật nhà cung cấp thành công",
                        data = result

                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật nhà cung cấp thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        public IActionResult Suggest(string text, int size = 10)
        {
            var data = _supplierRepository.GetSuggestSupplier(text, size);
            return new JsonResult(data.Select(s => new
            {
                id = s.SupplierId,
                name = s.FullName
            }));
        }

        public IActionResult SuggestForHotel(int hotel_id, string text, int size = 10)
        {
            var data = _supplierRepository.GetSuggestSupplierForHotel(hotel_id, text, size);
            return new JsonResult(data.Select(s => new
            {
                id = s.SupplierId,
                name = s.FullName
            }));
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(SupplierSearchModel searchModel)
        {

            int _UserId = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            string _FileName = StringHelpers.GenFileName("Danh sách nhà cung cấp", _UserId, "xlsx");
            string _UploadFolder = @"Template\Export";
            string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

            if (!Directory.Exists(_UploadDirectory))
            {
                Directory.CreateDirectory(_UploadDirectory);
            }
            //delete all file in folder before export
            try
            {
                DirectoryInfo di = new DirectoryInfo(_UploadDirectory);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
            catch
            {
            }
            string FilePath = Path.Combine(_UploadDirectory, _FileName);

            var rsPath = await _supplierRepository.ExportSuppliers(searchModel, FilePath);

            if (!string.IsNullOrEmpty(rsPath))
            {
                return new JsonResult(new
                {
                    isSuccess = true,
                    message = "Xuất dữ liệu thành công",
                    path = "/" + _UploadFolder + "/" + _FileName
                });
            }
            else
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Xuất dữ liệu thất bại"
                });
            }
        }
        #endregion

        public async Task<IActionResult> Detail(int id)
        {
            var model = _supplierRepository.GetDetailById(id);
            ViewBag.Users = new List<User>();
           
            if (id > 0)
            {
                ViewBag.Users = await _userRepository.GetBySuplierId(id);
               
            }
            return View(model);
        }

        #region payment

        [HttpPost]
        public IActionResult PaymentListing(int supplier_id)
        {
            var model = new GenericViewModel<SupplierPaymentViewModel>();
            try
            {
                var datas = _supplierRepository.GetSupplierPaymentList(supplier_id);
                model.CurrentPage = 1;
                model.ListData = datas.ToList();
                model.PageSize = 20;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / 20);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - PaymentListing: " + ex);
            }
            return PartialView(model);
        }

        public IActionResult PaymentUpsert(int id, int supplier_id)
        {
            var model = new BankingAccount()
            {
                SupplierId = supplier_id
            };

            try
            {
                if (id > 0) model = _supplierRepository.GetSupplierPaymentById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - PaymentUpsert: " + ex);
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> PaymentUpsert(BankingAccount model)
        {
            try
            {
                var result = _supplierRepository.UpsertSupplierPayment(model);

                var id = config.SUPPLIERID_ADAVIGO;
                if (result > 0)
                {
                    if (model.SupplierId == id)
                    {
                        string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] + ":" + configuration["DataBaseConfig:MongoServer:pwd"] + "@" + configuration["DataBaseConfig:MongoServer:Host"] + ":" + configuration["DataBaseConfig:MongoServer:Port"] + "/" + configuration["DataBaseConfig:MongoServer:catalog_log"];
                        var client = new MongoClient(url);

                        IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog_log"]);
                        BankingAccountViewModel log = new BankingAccountViewModel()
                        {
                            Id = model.Id,
                            BankId = model.BankId,
                            Branch = model.Branch,
                            AccountName = model.AccountName,
                            AccountNumber = model.AccountNumber,
                            SupplierId = model.SupplierId,
                            Amount = 500000000,
                        };
                        IMongoCollection<BankingAccountViewModel> affCollection = db.GetCollection<BankingAccountViewModel>(configuration["DataBaseConfig:MongoServer:BankingAccount_collection"]);

                        var filter = Builders<BankingAccountViewModel>.Filter.Where(x => x.Id == log.Id);
                        var result_document = affCollection.Find(filter).ToList();
                        if (result_document != null && result_document.Count > 0)
                        {
                            await affCollection.ReplaceOneAsync(filter, log);
                        }
                        else
                        {
                            await affCollection.InsertOneAsync(log);
                        }

                    }
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Lưu thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lưu thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PaymentUpsert - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PaymentDelete(int id)
        {
            try
            {
                var result = _supplierRepository.DeleteSupplierPayment(id);

                if (result > 0)
                {
                    string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] + ":" + configuration["DataBaseConfig:MongoServer:pwd"] + "@" + configuration["DataBaseConfig:MongoServer:Host"] + ":" + configuration["DataBaseConfig:MongoServer:Port"] + "/" + configuration["DataBaseConfig:MongoServer:catalog_log"];
                    var client = new MongoClient(url);

                    IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog_log"]);
                    IMongoCollection<BankingAccountViewModel> affCollection = db.GetCollection<BankingAccountViewModel>(configuration["DataBaseConfig:MongoServer:BankingAccount_collection"]);

                    var filter = Builders<BankingAccountViewModel>.Filter.Where(x => x.Id == id);
                    var result_document = affCollection.Find(filter).ToList();
                    if (result_document != null && result_document.Count > 0)
                    {
                        await affCollection.DeleteOneAsync(filter);
                    }

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PaymentDelete - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region contact
        [HttpPost]
        public IActionResult ContactListing(int supplier_id)
        {
            var model = new GenericViewModel<SupplierContactViewModel>();
            try
            {
                var datas = _supplierRepository.GetSupplierContactList(supplier_id);
                model.CurrentPage = 1;
                model.ListData = datas.ToList();
                model.PageSize = 20;
                model.TotalRecord = datas != null && datas.Any() ? datas.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / 20);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - ContactListing: " + ex);
            }
            return PartialView(model);
        }

        public IActionResult ContactUpsert(int id, int supplier_id)
        {
            var model = new SupplierContact()
            {
                SupplierId = supplier_id
            };

            try
            {
                if (id > 0) model = _supplierRepository.GetSupplierContactById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SupplierController - ContactUpsert: " + ex);
            }
            return PartialView(model);
        }

        [HttpPost]
        public IActionResult ContactUpsert(SupplierContact model)
        {
            try
            {
                var result = _supplierRepository.UpsertSupplierContact(model);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Lưu thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Lưu thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ContactUpsert - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public IActionResult ContactDelete(long id)
        {
            try
            {
                var result = _supplierRepository.DeleteSupplierContact(id);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa thông tin thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa thông tin thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ContactDelete - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region order-history
        [HttpPost]
        public IActionResult OrderListing(SupplierOrderSearchModel model)
        {
            var data = _supplierRepository.GetSupplierOrderList(model);
            return PartialView(data);
        }
        #endregion

        #region tickets
        [HttpPost]
        public IActionResult TicketListing(SupplierTicketSearchModel model)
        {
            var data = _supplierRepository.GetSupplierTicketList(model);
            return PartialView(data);
        }
        #endregion

        [HttpPost]
        public IActionResult SearchSupplier(string txt_search)
        {
            try
            {
                var list = _supplierRepository.GetSuggestSupplier(txt_search, 20);
                return new JsonResult(new
                {
                    isSuccess = true,
                    data = list
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ChangetStausSupplier(int data_id, int status)
        {
            try
            {
                if (data_id <= 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Dữ liệu không chính xác, vui lòng thử lại"
                    });
                }
                var id = _supplierRepository.UpdateSupplierStatus(status, data_id);
                string msg = "Dữ liệu không chính xác, vui lòng thử lại";
                switch (status)
                {
                    case (int)SUPPLIER_STATUS.DELETED:
                        {
                            msg = "Xóa nhà cung cấp thành công";
                        }
                        break;
                    case (int)SUPPLIER_STATUS.ON_WAITING_CONFIRMATION:
                        {
                            msg = "Bỏ duyệt nhà cung cấp thành công";

                        }
                        break;
                    case (int)SUPPLIER_STATUS.CONFIRMED:
                        {
                            msg = "Duyệt nhà cung cấp thành công";

                        }
                        break;
                }
                await _supplierService.UpdateSuplierAllProductStatus(data_id,status);
                await _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_LISTING, db_index);
                await _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_DETAIL, db_index);
                await _supplierService.SyncES(data_id);

                return new JsonResult(new
                {
                    isSuccess = id > 0,
                    message = msg,
                    data = id
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateSuplierUser(User request)
        {
            try
            {
                if (request == null
                    || request.UserName == null || request.UserName.Trim() == ""
                    //|| request.Password == null || request.Password.Trim() == ""
                    || request.FullName == null || request.FullName.Trim() == ""
                    || request.SupplierId == null || request.SupplierId <= 0

                    )
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Dữ liệu không chính xác, vui lòng thử lại"
                    });
                }
                if (request.Id <= 0)
                {
                    var exists_user = await _userRepository.GetByUserName("ncc" + request.SupplierId + "." + request.UserName);
                    if (exists_user != null && exists_user.Id > 0)
                    {
                        return new JsonResult(new
                        {
                            isSuccess = false,
                            message = "Tên đăng nhập đã tồn tại, vui lòng sử dụng tên đăng nhập khác"
                        });
                    }
                }
                var suplier = _supplierRepository.GetById((int)request.SupplierId);
                if (suplier == null || suplier.SupplierId <= 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Dữ liệu không chính xác, vui lòng thử lại"
                    });
                }
                int _UserId = 0;

                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                
                var user = new User()
                {
                    Id = request.Id,
                    FullName = request.FullName,
                    Password = EncodeHelpers.MD5Hash(request.Password),
                    UserName = "ncc" + request.SupplierId + "." + request.UserName,
                    Address = suplier.Address,
                    Manager = 0,
                    SupplierId = request.SupplierId,
                    BirthDay = DateTime.Now,
                    CompanyType = null,
                    CreatedOn = DateTime.Now,
                    Avata = "",
                    CreatedBy = _UserId,
                    DepartmentId = 0,
                    Email = suplier.Email,
                    Status = request.Status,
                    Phone = suplier.Phone,
                    ResetPassword = EncodeHelpers.MD5Hash(request.Password)
                };
                var id = await _userRepository.UpdateSuplierUser(user);


                return new JsonResult(new
                {
                    isSuccess = id > 0,
                    message = (request.Id > 0 ? "Cập nhật tài khoản" : "Thêm mới tài khoản") + " cho nhà cung cấp" + (id > 0 ? " thành công" : " thất bại"),
                    data = id
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - SupplierController: " + ex.Message);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        public async Task<IActionResult> SyncES()
        {
            try
            {
                var sp = await _supplierRepository.GetAll();
                if (sp != null && sp.Count > 0)
                {
                    try
                    {
                        string json = JsonConvert.SerializeObject(sp);
                        List<SupplierESModel> sp_es = JsonConvert.DeserializeObject<List<SupplierESModel>>(json.ToLower());
                        foreach(var detail in sp_es)
                        {
                            detail.id = detail.supplierid;
                        }

                        await _supplierESRepository.DeleteByIds(sp_es.Select(x => x.supplierid).ToList());
                        await _supplierESRepository.IndexMany(sp_es);
                    }
                    catch { }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SyncES - SupplierController: " + ex.ToString());
                return Ok(new
                {
                    is_success = false
                });
            }
            return Ok(new
            {
                is_success = true
            });
        }

    }
}
