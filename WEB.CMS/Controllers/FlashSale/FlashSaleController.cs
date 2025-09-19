using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels.Products;
using HuloToys_Service.ElasticSearch;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Repositories.IRepositories;
using System.Globalization;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using Utilities.Contants.ProductV2;
using WEB.BestMall.CMS.Service;
using WEB.CMS.Controllers.Product.Bussiness;
using WEB.CMS.Customize;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers.FlashSale
{
    [CustomAuthorize]

    public class FlashSaleController : Controller
    {
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly ProductSpecificationMongoAccess _productSpecificationMongoAccess;
        private readonly IConfiguration _configuration;
        private readonly IGroupProductRepository _groupProductRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly RedisConn _redisConn;
        private readonly ProductDetailService productDetailService;
        private StaticAPIService _staticAPIService;
        private readonly int group_product_root = 31;
        private readonly int db_index = 9;
        private readonly ProductESRepository _productESRepository;
        private readonly FlashSaleESRepository _flashSaleESRepository;
        private readonly FlashSaleProductESRepository _flashSaleProductESRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IFlashSaleRepository _flashSaleRepository;
        private readonly IFlashSaleProductRepository _flashSaleProductRepository;
        public FlashSaleController(IConfiguration configuration, RedisConn redisConn, IGroupProductRepository groupProductRepository, ILabelRepository labelRepository,
            ISupplierRepository supplierRepository, IAllCodeRepository allCodeRepository, IFlashSaleRepository flashSaleRepository, IFlashSaleProductRepository flashSaleProductRepository
            , ProductDetailMongoAccess productV2DetailMongoAccess, ProductSpecificationMongoAccess productSpecificationMongoAccess,
            ProductESRepository productESRepository, FlashSaleESRepository flashSaleESRepository, FlashSaleProductESRepository flashSaleProductESRepository, RaitingESService raitingESService)
        {
            _productV2DetailMongoAccess = productV2DetailMongoAccess;
            _productSpecificationMongoAccess = productSpecificationMongoAccess;
            _staticAPIService = new StaticAPIService(configuration);
            _redisConn = redisConn;
            _redisConn.Connect();
            _groupProductRepository = groupProductRepository;
            db_index = Convert.ToInt32(configuration["Redis:Database:db_search_result"]);
            _configuration = configuration;
            productDetailService = new ProductDetailService(configuration,productV2DetailMongoAccess,productSpecificationMongoAccess,raitingESService);
            _productESRepository = productESRepository;
            _flashSaleESRepository = flashSaleESRepository;
            _flashSaleProductESRepository = flashSaleProductESRepository;
            _labelRepository = labelRepository;
            _supplierRepository = supplierRepository;
            _allCodeRepository = allCodeRepository;
            _flashSaleRepository = flashSaleRepository;
            _flashSaleProductRepository = flashSaleProductRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Search(DateTime? fromdate, DateTime? todate, int status,int page_index=1,int page_size=10)
        {
            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";

            if (fromdate >= todate)
            {
                fromdate = null;
                todate = null;
            }
            if (fromdate == DateTime.MinValue) fromdate = null;
            if (todate == DateTime.MinValue) todate = null;
            page_index = page_index<1? 1: page_index;
            page_size = page_size<1? 10: page_size; 
            status = status < 0? 0: status; 
            // Gọi SP thông qua repository
            var flashSales = await _flashSaleRepository.GetList(fromdate, todate, status, page_index, page_size);
            // Trả về View với dữ liệu đã lấy được
            return View(flashSales);
        }
        public async Task<IActionResult> Detail(int id=-1)
        {
            ViewBag.Detail = new Entities.Models.FlashSale();
            ViewBag.Products = new List<Entities.Models.FlashSaleProduct>();
            ViewBag.ProductsDetail = new List<ProductMongoDbModel>();
            ViewBag.Supplier = new Supplier();
            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
            if (id > 0)
            {
                var detail = await _flashSaleRepository.GetByID(id);
                if(detail!=null && detail.Id > 0)
                {
                    ViewBag.Detail = detail;
                    var product = await _flashSaleProductRepository.GetByFlashSaleID(id);
                    if(product!=null && product.Count > 0)
                    {
                        product = product.OrderBy(x => x.Position).ToList();
                        ViewBag.Products = product;
                        ViewBag.ProductsDetail = await _productV2DetailMongoAccess.ListByProducts(product.Select(x=>x.ProductId).ToList());
                    }
                }
                if (detail != null && detail.SupplierId != null && detail.SupplierId > 0)
                {
                    ViewBag.Supplier = _supplierRepository.GetById((int)detail.SupplierId);
                }
            }
            return View();
        }
        [HttpPost]
        public IActionResult ProductFlashSale(int id)
        {
            ViewBag.FlashSaleId = id;
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> ProductFlashSaleSearch(string keyword = "", int group_id = -1,int supplier_id=-1)
        {
            ViewBag.Main = new List<ProductMongoDbModel>();

            string static_domain = _configuration["DomainConfig:ImageStatic"];
            ViewBag.StaticDomain = static_domain != null && static_domain.EndsWith("/") ? static_domain : static_domain + "/";
            var main_products = await _productV2DetailMongoAccess.ListingProductFlashSale(keyword, group_id, supplier_id);
            ViewBag.Main = main_products;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Summit(Entities.Models.FlashSale flashsale, List<FlashSaleProduct> flashsale_product)
        {
            try
            {
                // 1. Validate FlashSale
                if (flashsale == null || !flashsale.SupplierId.HasValue|| flashsale.SupplierId==null|| flashsale.SupplierId<=0|| flashsale.FromDate == default(DateTime) || flashsale.ToDate == default(DateTime))
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại",
                    });
                }
                if (flashsale.FromDate >= flashsale.ToDate) 
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Thời gian bắt đầu chiến dịch không được lớn hơn thời gian kết thúc",
                    });
                }
                var exists_active_flashsale = await _flashSaleESRepository.GetActiveFlashsaleBySupplierID((int)flashsale.SupplierId, flashsale.FromDate, flashsale.ToDate);
                if (exists_active_flashsale != null && exists_active_flashsale.Count > 0)
                {
                    var id_compare = (flashsale.Id <= 0 ? 0 : flashsale.Id);
                    exists_active_flashsale = exists_active_flashsale.Where(x => x.flashsale_id != id_compare).ToList();
                    if (exists_active_flashsale != null && exists_active_flashsale.Count > 0)
                    {
                        return Ok(new
                        {
                            is_success = false,
                            msg = "Nhà cung cấp này đã có chương trình FlashSale khác đang hoạt động cùng 1 khoảng thời gian, vui lòng chọn nhà cung cấp khác",
                        });
                    }
                }

                    var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                flashsale.UserCreateId = _UserLogin;
                flashsale.CreateDate = DateTime.Now;
                flashsale.UserUpdateId = _UserLogin;
                flashsale.UpdateLast = DateTime.Now;
                List<FlashSaleProduct> exists_products= new List<FlashSaleProduct>();
                // 2. Create FlashSale
                if (flashsale.Id <= 0)
                {
                    _flashSaleRepository.CreateFlashSale(flashsale);
                }
                else
                {
                    var exists=await _flashSaleRepository.GetByID(flashsale.Id);
                    if(exists != null && exists.Id>0)
                    {
                        flashsale.UserCreateId = exists.UserCreateId;
                        flashsale.CreateDate = exists.CreateDate;
                        exists_products=await _flashSaleProductRepository.GetByFlashSaleID(flashsale.Id);
                        _flashSaleRepository.UpdateFlashSale(flashsale);
                    }
                    else
                    {
                        _flashSaleRepository.CreateFlashSale(flashsale);
                    }
                }
              
                if (flashsale.Id <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "Lỗi trong quá trình xử lý, vui lòng thử lại / liên hệ hỗ trợ kỹ thuật",
                    });
                }
                #region Sync ES
                await _flashSaleESRepository.DeleteByFlashsaleId(flashsale.Id);
                var supplier = _supplierRepository.GetSuplierById((int)flashsale.SupplierId);
                FlashSaleESModel flashsale_es = new FlashSaleESModel()
                {
                    id = _flashSaleESRepository.GenerateId(),
                    flashsale_id = flashsale.Id,
                    fromdate = flashsale.FromDate,
                    status = flashsale.Status,
                    supplierid = flashsale.SupplierId,
                    todate = flashsale.ToDate,
                    name = flashsale.Name,
                    banner = flashsale.Banner,
                    supplier_name = supplier.FullName,
                    created_date = flashsale.CreateDate,

                };
                await _flashSaleESRepository.InsertAsync(flashsale_es);
                //SyncES();
                #endregion


                // 3. Validate and Create FlashSale Products
                if (flashsale_product != null && flashsale_product.Any())
                {
                    if (exists_products != null && exists_products.Count > 0)
                    {
                        var new_list = flashsale_product.Select(x => x.Id);
                        var deleted = exists_products.Where(x => !new_list.Contains(x.Id));
                        if (deleted.Any())
                        {
                            foreach (var del in deleted)
                            {
                                del.CampaignId *= -1;
                                _flashSaleProductRepository.UpdateFlashSaleProduct(del);
                            }
                        }
                        await _flashSaleProductESRepository.DeleteByIds(exists_products.Select(x=>x.Id).ToList());

                    }
                    var new_items = new List<FlashSaleProductESModel>();
                    foreach (var product in flashsale_product)
                    {
                        product.CampaignId = flashsale.Id;
                        if (product.Id <= 0)
                        {
                            _flashSaleProductRepository.CreateFlashSaleProduct(product);
                        }
                        else
                        {
                            _flashSaleProductRepository.UpdateFlashSaleProduct(product);
                        }
                        var product_mongo = await _productV2DetailMongoAccess.GetByID(product.ProductId);
                        try
                        {
                            await _productV2DetailMongoAccess.UpdateProductFlashsale(product_mongo, flashsale, product);
                            var list_sub = await _productV2DetailMongoAccess.SubListing(product.ProductId);
                            if (list_sub != null && list_sub.Count > 0)
                            {
                                foreach (var sub in list_sub)
                                {
                                    await _productV2DetailMongoAccess.UpdateProductFlashsale(sub, flashsale, product);
                                }
                            }
                        }
                        catch { }
                        new_items.Add(new FlashSaleProductESModel()
                        {
                            valuetype=product.ValueType,
                            discountvalue=product.DiscountValue,
                            flashsale_id=product.CampaignId,
                            flashsale_productid=product.Id,
                            id=_flashSaleProductESRepository.GenerateId(),
                            position=product.Position,
                            productid=product.ProductId,
                            status=product.Status,
                            supersale = product.SuperSale,
                            badgetype=product.BadgeType,
                            group_id=(product_mongo == null|| product_mongo.group_product_id == null) ?"":product_mongo.group_product_id
                        });
                       // LogHelper.InsertLogTelegram("Summit - FlashSaleController [group_id]: " + ((product_mongo == null || product_mongo.group_product_id == null) ? "" : product_mongo.group_product_id));
                        _redisConn.clear(CacheName.PRODUCT_DETAIL + product.ProductId, db_index);
                        if (product_mongo!=null && product_mongo._id!=null)
                        {
                            string cache_name = "PRODUCT_LISTING_" + product_mongo.label_id;
                            await _redisConn.DeleteCacheByKeyword(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                            cache_name = "PRODUCT_LISTING_" + product_mongo.supplier_id;
                            await _redisConn.DeleteCacheByKeyword(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                        }

                    }
                    await _flashSaleProductESRepository.DeleteByIds(flashsale_product.Select(x => x.Id).ToList());
                    await _flashSaleProductESRepository.IndexMany(new_items);
                }
                #region Sync All:
                //var fsl = await _flashSaleRepository.GetAll();
                //if (fsl != null && fsl.Count > 0)
                //{
                //    List<FlashSaleESModel> all_fsl_es = fsl.Select(x => new FlashSaleESModel()
                //    {
                //        id = _flashSaleESRepository.GenerateId(),
                //        flashsale_id = x.Id,
                //        fromdate = x.FromDate,
                //        status = x.Status,
                //        supplierid = x.SupplierId,
                //        todate = x.ToDate,
                //        name = x.Name
                //    }).ToList();
                //    await _flashSaleESRepository.DeleteByIds(all_fsl_es.Select(x => x.flashsale_id).ToList());
                //    await _flashSaleESRepository.IndexMany(all_fsl_es);
                //}
                //var fspl = await _flashSaleProductRepository.GetAll();
                //if (fspl != null && fspl.Count > 0)
                //{
                //    List<FlashSaleProductESModel> all_fspl_es = fspl.Select(x => new FlashSaleProductESModel()
                //    {
                //        id = _flashSaleProductESRepository.GenerateId(),
                //        valuetype = x.ValueType,
                //        discountvalue = x.DiscountValue,
                //        flashsale_id = x.CampaignId,
                //        flashsale_productid = x.Id,
                //        position = x.Position,
                //        productid = x.ProductId,
                //        status = x.Status
                //    }).ToList();
                //    await _flashSaleProductESRepository.DeleteByIds(all_fspl_es.Select(x => x.flashsale_productid).ToList());
                //    await _flashSaleProductESRepository.IndexMany(all_fspl_es);
                //}
                #endregion
                await _redisConn.DeleteCacheByKeyword(CacheName.PRODUCT_LISTING, db_index);
                return Ok(new
                {
                    is_success = true,
                    msg = "Thêm mới / Cập nhật chiến dịch Flashsale thành công",
                    data = flashsale.Id,
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Summit - FlashSaleController: " + ex.ToString());
                return Ok(new
                {
                    is_success = false,
                    msg = "Thêm mới / Cập nhật sản phẩm thất bại, vui lòng liên hệ bộ phận IT",
                    err = ex.ToString(),
                });
            }
           
        }
        [HttpPost]
        public async Task<IActionResult> GetActiveFlashSaleCampaignBySupplier(int supplier_id=-1,int flash_sale_id=0)
        {
            try
            {
                if (supplier_id <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "NCC không tồn tại",
                        exists=false
                    });
                }

                var exists_active_flashsale = await _flashSaleESRepository.GetActiveFlashsaleBySupplierID(supplier_id);
                if (exists_active_flashsale == null || exists_active_flashsale.Count<=0)
                {
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Không có chương trình nào hoạt động, được phép thêm chương trình cho NCC này",
                        exists = false
                    });
                }
                else
                {
                    exists_active_flashsale = exists_active_flashsale.Where(x => x.flashsale_id != flash_sale_id).ToList();
                    if (exists_active_flashsale != null && exists_active_flashsale.Count > 0)
                    {
                        return Ok(new
                        {
                            is_success = true,
                            msg = "Nhà cung cấp này đã có chương trình FlashSale khác đang hoạt động, vui lòng chọn nhà cung cấp khác",
                            exists = true
                        });
                    }
                       
                }
                return Ok(new
                {
                    is_success = true,
                    msg = "Không có chương trình nào hoạt động, được phép thêm chương trình cho NCC này",
                    exists = false
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetActiveFlashSaleCampaignBySupplier - FlashSaleController: " + ex.ToString());
                return Ok(new
                {
                    is_success = false,
                    msg = "Thêm mới / Cập nhật sản phẩm thất bại, vui lòng liên hệ bộ phận IT",
                    err = ex.ToString(),
                });
            }
        }
        [HttpPost]

        public async Task<IActionResult> SyncES()
        {
            try
            {
                var fsl = await _flashSaleRepository.GetAll();
                if (fsl != null && fsl.Count > 0)
                {
                    List<FlashSaleESModel> all_fsl_es = fsl.Select(x => new FlashSaleESModel()
                    {
                        id = _flashSaleESRepository.GenerateId(),
                        flashsale_id = x.Id,
                        fromdate = x.FromDate,
                        status = x.Status,
                        supplierid = x.SupplierId,
                        todate = x.ToDate,
                        name = x.Name,
                        banner = x.Banner,
                        created_date=x.CreateDate,
                        supplier_name= _supplierRepository.GetSuplierById((int)x.SupplierId).FullName,
                    }).ToList();
                    await _flashSaleESRepository.DeleteByIds(all_fsl_es.Select(x => x.flashsale_id).ToList());
                    await _flashSaleESRepository.IndexMany(all_fsl_es);
                }
                var fspl = await _flashSaleProductRepository.GetAll();
                await _flashSaleProductESRepository.DeleteAll();
                if (fspl != null && fspl.Count > 0)
                {
                    List<FlashSaleProductESModel> all_fspl_es = fspl.Select(x => new FlashSaleProductESModel()
                    {
                        id = _flashSaleProductESRepository.GenerateId(),
                        valuetype = x.ValueType,
                        discountvalue = x.DiscountValue,
                        flashsale_id = x.CampaignId,
                        flashsale_productid = x.Id,
                        position = x.Position,
                        productid = x.ProductId,
                        status = x.Status,
                        supersale = x.SuperSale

                    }).ToList();

                    if (all_fspl_es.Count > 0)
                    {
                        foreach(var product in all_fspl_es)
                        {
                            var product_mongo = await _productV2DetailMongoAccess.GetByID(product.productid);
                            if (fsl != null)
                            {
                                var flashsale = fsl.FirstOrDefault(x => x.Id == product.flashsale_id && x.Status==1);
                                var flashsale_product = fspl.FirstOrDefault(x => x.Id == product.flashsale_productid && x.Status == 1);
                                if (flashsale != null && flashsale_product != null)
                                {
                                    await _productV2DetailMongoAccess.UpdateProductFlashsale(product_mongo, flashsale, flashsale_product);
                                    var list_sub = await _productV2DetailMongoAccess.SubListing(product.productid);
                                    if (list_sub != null && list_sub.Count > 0)
                                    {
                                        foreach (var sub in list_sub)
                                        {
                                            await _productV2DetailMongoAccess.UpdateProductFlashsale(sub, flashsale, flashsale_product);
                                        }
                                    }
                                }
                                else
                                {
                                    await _productV2DetailMongoAccess.UpdateProductFlashsale(product_mongo, null, null);
                                    var list_sub = await _productV2DetailMongoAccess.SubListing(product.productid);
                                    if (list_sub != null && list_sub.Count > 0)
                                    {
                                        foreach (var sub in list_sub)
                                        {
                                            await _productV2DetailMongoAccess.UpdateProductFlashsale(sub, null, null);
                                        }
                                    }
                                }
                            }
                            product.group_id = (product_mongo == null || product_mongo.group_product_id == null) ? "" : product_mongo.group_product_id;

                        }
                    }
                   // await _flashSaleProductESRepository.DeleteByIds(all_fspl_es.Select(x => x.flashsale_productid).ToList());
                    await _flashSaleProductESRepository.IndexMany(all_fspl_es);
                    string cache_name = "PRODUCT_LISTING_";
                    await _redisConn.DeleteCacheByKeyword(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_search_result"]));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SyncES - ProductController: " + ex.ToString());
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
