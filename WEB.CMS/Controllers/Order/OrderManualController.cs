using APP_CHECKOUT.MongoDb;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.OrderManual;
using ENTITIES.ViewModels.ElasticSearch;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
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
        private UserESRepository _userESRepository;
        private readonly IUserRepository _userRepository;
        private OrderESRepository _orderESRepository;
        private ShippingCarrierService _shippingCarrierService;
        private RedisConn _redisConn;
        public OrderManualController(IConfiguration configuration, IAllCodeRepository allCodeRepository, IOrderRepository orderRepository, IIdentifierServiceRepository identifierServiceRepository,
            IAccountClientRepository accountClientRepository, IUserRepository userRepository, IClientRepository clientRepository, RedisConn redisConn)
        {
            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
            _orderRepository = orderRepository;
            _identifierServiceRepository = identifierServiceRepository;
            _accountClientRepository = accountClientRepository;
            _userESRepository = new UserESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _userRepository = userRepository;
            _clientRepository = clientRepository;
            _orderESRepository = new OrderESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _redisConn = redisConn;
            _redisConn.Connect();
            _shippingCarrierService = new ShippingCarrierService(configuration, _redisConn);
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
                var data = await _userESRepository.GetUserSuggesstion(txt_search);
                if (data == null || data.Count <= 0)
                {
                    var data_sql = await _userRepository.GetUserSuggesstion(txt_search);
                    data = new List<UserESViewModel>();
                    if (data_sql != null && data_sql.Count > 0)
                    {
                        data.AddRange(data_sql.Select(x => new UserESViewModel() { email = x.Email, fullname = x.FullName, id = x.Id, phone = x.Phone, username = x.UserName, _id = x.Id }));
                    }
                }

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = data,
                    selected = _UserId
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserSuggestion - OrderManualController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<CustomerESViewModel>()
                });
            }

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
                        id = o.orderid,  // assuming OrderId is the ID you want
                        orderno = o.orderno
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
                long _UserId = 0;
                var data = new List<OrderElasticsearchViewModel>();
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
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
                if(order!=null && order.OrderId > 0)
                {
                    var client = await _clientRepository.GetClientDetailByClientId(order.ClientId);
                    var shipping_code=await _shippingCarrierService.PushOrderToCarrier(order,client);
                    if (shipping_code != null) {
                        order.ShippingCode = shipping_code;
                        await _orderRepository.UpdateOrder(order);
                        return Ok(new
                        {
                            is_success = true,
                            msg = "Đơn hàng chuyển cho ĐVVC thành công, mã vận đơn: " + shipping_code
                        });
                    }
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


    }
}
