using APP_CHECKOUT.MongoDb;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.OrderManual;
using ENTITIES.ViewModels.ElasticSearch;
using HuloToys_Service.Controllers.Shipping.Business;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.BestMall.CMS.Service;
using WEB.CMS.Controllers.Elastic.Bussiness;
using WEB.CMS.Controllers.Order.Bussiness;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.Service.Carriers;

namespace WEB.CMS.Controllers.Order
{
    public class OrderManualController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIdentifierServiceRepository _identifierServiceRepository;
        private readonly IAccountClientRepository _accountClientRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private OrderESRepository _orderESRepository;
        private ShippingCarrierService _shippingCarrierService;
        private RedisConn _redisConn;
        private ElasticService _elasticService;
        private ViettelPostService _viettelPostService;
        public OrderManualController(IConfiguration configuration, IAllCodeRepository allCodeRepository, IOrderRepository orderRepository, IIdentifierServiceRepository identifierServiceRepository,
            IAccountClientRepository accountClientRepository, IUserRepository userRepository, IClientRepository clientRepository, RedisConn redisConn,
            OrderESRepository orderESRepository, LocationESService locationESService, ElasticService elasticService, ViettelPostService viettelPostService)
        {
            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
            _orderRepository = orderRepository;
            _identifierServiceRepository = identifierServiceRepository;
            _accountClientRepository = accountClientRepository;
            _userRepository = userRepository;
            _clientRepository = clientRepository;
            _orderESRepository = orderESRepository;
            _redisConn = redisConn;
            _redisConn.Connect();
            _shippingCarrierService = new ShippingCarrierService(configuration, _redisConn, locationESService   );
            _elasticService = elasticService;
            _viettelPostService= viettelPostService;
        }
        [HttpPost]
        public IActionResult CreateOrderManual()
        {
            ViewBag.Branch = _allCodeRepository.GetListByType(AllCodeType.BRANCH_CODE);
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UserSuggestion(string txt_search, int service_type = 0)
        {

            try
            {
                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (txt_search == null) txt_search = "";
                var data = await _userRepository.GetUserSuggesstion(txt_search);
                if(data!=null && data.Count > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = data.Select(x => new UserESViewModel() { email = x.Email, fullname = x.FullName, id = x.Id, phone = x.Phone, username = x.UserName, _id = x.Id }),
                        selected = _UserId
                    });
                }
               

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserSuggestion - OrderManualController: " + ex.ToString());
               
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                data = new List<CustomerESViewModel>()
            });

        }
        [HttpPost]
        public async Task<IActionResult> OrderNoSuggestion(string txt_search)
        {

            try
            {
                long _UserId = 0;
                var data = new List<OrderElasticsearchViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (txt_search != null)
                {
               data = await _orderESRepository.GetOrderNoSuggesstion(txt_search);
                    var result = data.Select(o => new {
                        id = o.Id,  // assuming OrderId is the ID you want
                        orderno = o.OrderNo
                    }).ToList();
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = result,
                        selected = _UserId
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = new List<OrderElasticsearchViewModel>()
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderNoSuggestion - OrderManualController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<OrderElasticsearchViewModel>()
                });
            }

        } 
        [HttpPost]
        public async Task<IActionResult> SendToCarrier(long id)
        {

            try
            {
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (id <= 0 || _UserId <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "ID đơn không chính xác / Người dùng chưa được xác thực, vui lòng thử lại / liên hệ bộ phận IT"
                    });
                }

                var order = await _orderRepository.GetByOrderId(id);
                if(order!=null && order.OrderId > 0 && order.PaymentStatus >=0 && order.OrderStatus == (int)OrderStatus.PROCESSING)
                {
                    order.UpdateLast = DateTime.Now;
                    order.UserUpdateId = _UserId;
                    order.OrderStatus = (int)OrderStatus.DELIVERY;
                    List<string> shipping_code = new List<string>();
                    try
                    {
                        switch (order.CarrierId) {
                            case 1:
                                {

                                }break;
                            case 2:
                                {

                                }break;
                            case 3: //VTP
                                {
                                    List<VTPOrderRequestModel> model = JsonConvert.DeserializeObject<List<VTPOrderRequestModel>>(order.ShippingToken);
                                  
                                    if (model != null && model.Count>0)
                                    {
                                        foreach (var item in model) {

                                            var response = await _viettelPostService.CreateVTPOrder(item);
                                            if (response != null && response.error==false) {
                                                shipping_code.Add(response.data.ORDER_NUMBER);
                                            }
                                        }
                                    }
                                }
                                break;
                        
                        
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.InsertLogTelegram("SendToCarrier - OrderManualController - SendTo CarrierID: "+order.CarrierId+" err: " + ex.ToString());

                    }
                    if (shipping_code.Count > 0) { 
                         order.ShippingCode=string.Join(",", shipping_code);
                    }
                    var updated = await _orderRepository.UpdateOrder(order);
                    _elasticService.PushToQueue("SP_GetOrder", order.OrderId);
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Cập nhật đơn hàng thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendToCarrier - OrderManualController: " + ex.ToString());
              
            }
            return Ok(new
            {
                is_success = true,
                msg = "Xử lý đơn hàng không thành công, vui lòng liên hệ bộ phận IT"
            });

        } 
        [HttpPost]
        public async Task<IActionResult> OrderReceivedPackage(long id)
        {

            try
            {
                int _UserId = 0;
                var data = new List<OrderElasticsearchViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (id <= 0 || _UserId <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "ID đơn không chính xác / Người dùng chưa được xác thực, vui lòng thử lại / liên hệ bộ phận IT"
                    });
                }

                var order = await _orderRepository.GetByOrderId(id);
                if(order!=null && order.OrderId > 0 && order.PaymentStatus >=0 && order.OrderStatus == (int)OrderStatus.DELIVERY)
                {
                   
                    order.UpdateLast = DateTime.Now;
                    order.UserUpdateId = _UserId;
                    order.OrderStatus = (int)OrderStatus.FINISHED_DELIVERY;
                    var updated=await _orderRepository.UpdateOrder(order);
                    _elasticService.PushToQueue("SP_GetOrder", order.OrderId);
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Cập nhật đơn hàng thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderNoSuggestion - OrderManualController: " + ex.ToString());
              
            }
            return Ok(new
            {
                is_success = true,
                msg = "Xử lý đơn hàng không thành công, vui lòng liên hệ bộ phận IT"
            });

        }

        [HttpPost]
        public async Task<IActionResult> OrderFinished(long id)
        {

            try
            {
                int _UserId = 0;
                var data = new List<OrderElasticsearchViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (id <= 0 || _UserId <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "ID đơn không chính xác / Người dùng chưa được xác thực, vui lòng thử lại / liên hệ bộ phận IT"
                    });
                }

                var order = await _orderRepository.GetByOrderId(id);
                if (order != null && order.OrderId > 0 && order.PaymentStatus >= 0 && order.OrderStatus == (int)OrderStatus.FINISHED_DELIVERY)
                {
                   
                    order.UpdateLast = DateTime.Now;
                    order.UserUpdateId = _UserId;
                    order.OrderStatus = (int)OrderStatus.FINISHED;
                    var updated = await _orderRepository.UpdateOrder(order);
                    _elasticService.PushToQueue("SP_GetOrder", order.OrderId);
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Cập nhật đơn hàng thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderNoSuggestion - OrderManualController: " + ex.ToString());

            }
            return Ok(new
            {
                is_success = true,
                msg = "Xử lý đơn hàng không thành công, vui lòng liên hệ bộ phận IT"
            });

        }  
        [HttpPost]
        public async Task<IActionResult> OrderCancel(long id)
        {

            try
            {
                int _UserId = 0;
                var data = new List<OrderElasticsearchViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (id <= 0 || _UserId <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "ID đơn không chính xác / Người dùng chưa được xác thực, vui lòng thử lại / liên hệ bộ phận IT"
                    });
                }

                var order = await _orderRepository.GetByOrderId(id);
                if (order != null && order.OrderId > 0 && order.PaymentStatus >= 0 && order.OrderStatus != (int)OrderStatus.CANCEL && order.OrderStatus != (int)OrderStatus.FINISHED)
                {
                   
                    order.UpdateLast = DateTime.Now;
                    order.UserUpdateId = _UserId;
                    order.OrderStatus = (int)OrderStatus.CANCEL;
                    var updated = await _orderRepository.UpdateOrder(order);
                    _elasticService.PushToQueue("SP_GetOrder", order.OrderId);
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Cập nhật đơn hàng thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderNoSuggestion - OrderManualController: " + ex.ToString());

            }
            return Ok(new
            {
                is_success = true,
                msg = "Xử lý đơn hàng không thành công, vui lòng liên hệ bộ phận IT"
            });

        }
        [HttpPost]
        public async Task<IActionResult> Refund(long id)
        {

            try
            {
                int _UserId = 0;
                var data = new List<OrderElasticsearchViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (id <= 0 || _UserId <= 0)
                {
                    return Ok(new
                    {
                        is_success = false,
                        msg = "ID đơn không chính xác / Người dùng chưa được xác thực, vui lòng thử lại / liên hệ bộ phận IT"
                    });
                }

                var order = await _orderRepository.GetByOrderId(id);
                if (order != null && order.OrderId > 0 && order.PaymentStatus >= 0 && order.OrderStatus != (int)OrderStatus.CANCEL && order.OrderStatus != (int)OrderStatus.FINISHED)
                {

                    order.UpdateLast = DateTime.Now;
                    order.UserUpdateId = _UserId;
                    order.OrderStatus = (int)OrderStatus.REFUND;
                    order.RefundStatus = (int)OrderRefundStatus.CONFIRM;
                    var updated = await _orderRepository.UpdateOrder(order);
                    _elasticService.PushToQueue("SP_GetOrder", order.OrderId);
                    return Ok(new
                    {
                        is_success = true,
                        msg = "Cập nhật đơn hàng thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderNoSuggestion - Refund: " + ex.ToString());

            }
            return Ok(new
            {
                is_success = true,
                msg = "Xử lý đơn hàng không thành công, vui lòng liên hệ bộ phận IT"
            });

        }
    }
}
