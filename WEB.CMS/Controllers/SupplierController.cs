using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.BankingAccount;
using Entities.ViewModels.Funding;
using Entities.ViewModels.SupplierConfig;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Repositories.IRepositories;
using System.Security.Claims;
using Utilities;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.Adavigo.CMS.Controllers.Order
{
    [CustomAuthorize]
    public class SupplierController : Controller
    {

        private readonly IConfiguration configuration;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly ISupplierRepository _supplierRepository;
        private readonly WEB.CMS.Models.AppSettings config;

        public SupplierController(IAllCodeRepository allCodeRepository, ISupplierRepository supplierRepository,
            ICommonRepository commonRepository, IConfiguration _configuration, IWebHostEnvironment webHostEnvironment)
        {
            _allCodeRepository = allCodeRepository;
            _supplierRepository = supplierRepository;
            _commonRepository = commonRepository;
            config = ReadFile.LoadConfig();
            configuration = _configuration;
            _WebHostEnvironment = webHostEnvironment;
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

        public IActionResult AddOrUpdate(int id)
        {
            var model = new SupplierViewModel();

            if (id > 0) model = _supplierRepository.GetById(id);

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(SupplierConfigUpsertModel model)
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
                            message = "Nhà cung cấp đã tồn tại"
                        });
                    }
                }
                var result = _supplierRepository.Add(model);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Thêm nhà cung cấp thành công"
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
        public IActionResult Update(SupplierConfigUpsertModel model)
        {
            try
            {
                var result = _supplierRepository.Update(model);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật nhà cung cấp thành công"
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
                System.IO.DirectoryInfo di = new DirectoryInfo(_UploadDirectory);
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

        public IActionResult Detail(int id)
        {
            var model = _supplierRepository.GetDetailById(id);
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
        public  IActionResult SearchSupplier(string txt_search)
        {
            try
            {
                var list= _supplierRepository.GetSuggestSupplier(txt_search,20);
                return new JsonResult(new
                {
                    isSuccess = true,
                    data=list
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
    }
}
