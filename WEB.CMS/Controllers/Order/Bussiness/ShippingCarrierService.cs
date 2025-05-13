using APP_CHECKOUT.MongoDb;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels.Orders;
using Utilities;
using WEB.CMS.Service.Carriers;

namespace WEB.CMS.Controllers.Order.Bussiness
{
    public class ShippingCarrierService
    {
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisConn;
        private readonly NinjaVanCarrierService ninjaVanCarrierService;
        private readonly OrderMongodbService orderMongodbService;

        public ShippingCarrierService(IConfiguration configuration, RedisConn redisConn)
        {
            _configuration = configuration;
            _redisConn = redisConn;
            ninjaVanCarrierService=new NinjaVanCarrierService(configuration, _redisConn);
            orderMongodbService = new OrderMongodbService(configuration);
        }
        public async Task<string> PushOrderToCarrier(Entities.Models.Order order,Client client)
        {
            try
            {
                switch (order.CarrierId) {
                    case 1: // Ninja Van
                        {
                            switch (order.ShippingType)
                            {
                                case 1: // Giao hang nhanh
                                case 4: // Giao hang tiet kiem
                                case 3: // Giao hang hoa toc
                                case 5: // COD
                                    {
                                        var order_mongo = await orderMongodbService.FindByOrderId(order.OrderId);
                                        return await ninjaVanCarrierService.CreateOrder(order, order_mongo, client);
                                    }
                                case 2: // Nhan hang tai cua hang , ko call DVVC
                                default: // Cannot Detect
                                    {
                                        return null;
                                    }
                            }
                        }
                    case 2: //SPX
                        {
                            return null;
                        }
                }
              
            }
            catch(Exception ex) {
                LogHelper.InsertLogTelegram("PushOrderToCarrier - ShippingCarrierService: " + ex.ToString());

            }
            return null;
        }
    }
}
