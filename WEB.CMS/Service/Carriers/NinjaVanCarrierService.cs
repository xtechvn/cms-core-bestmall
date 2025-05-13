using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels.Orders;
using Newtonsoft.Json;
using Utilities;
using Utilities.Contants;

namespace WEB.CMS.Service.Carriers
{

    public class NinjaVanCarrierService
    {
        private readonly IConfiguration _configuration;
        private readonly RedisConn _redisConn;
        private readonly LocationESService _locationESService;

        public NinjaVanCarrierService(IConfiguration configuration, RedisConn redisConn)
        {
            _configuration = configuration;
            _redisConn = redisConn;
            _locationESService = new LocationESService(configuration["DataBaseConfig:Elastic:Host"], configuration);
        }
        /// <summary>
        /// Timeslot available: 09:00 - 12:00, 09:00 - 18:00, 09:00 - 22:00, 12:00 - 15:00, 15:00 - 18:00, 18:00 - 22:00, 
        /// Timezone availale: "Asia/Singapore" "Asia/Kuala_Lumpur" "Asia/Jakarta" "Asia/Jayapura" "Asia/Makassar" "Asia/Bangkok" "Asia/Manila" "Asia/Ho_Chi_Minh" "Asia/Yangon"
        /// </summary>
        /// <param name="order"></param>
        /// <param name="order_mongo"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<string> CreateOrder(Order order, OrderDetailMongoDbModel order_mongo,Client client) {
            string carrier_packages_id = null;
            try
            {
                string service_level = "Standard";
                switch (order.ShippingType)
                {
                    case 1: // Giao hang nhanh
                    case 4: // Giao hang tiet kiem
                        {

                        }
                        break;
                    case 3: // Giao hang hoa toc
                        {
                            service_level = "Express";
                        }
                        break;
                    case 5: // COD
                        {
                            service_level = "Standard";
                        }
                        break;
                    case 2: // Nhan hang tai cua hang , ko call DVVC
                    default: // Cannot Detect
                        {
                            return carrier_packages_id;
                        }
                }
                if(order.ProvinceId==null || order.DistrictId==null || order.WardId == null|| order.Address == null|| order.Address.Trim()=="")
                {
                    LogHelper.InsertLogTelegram("PushOrder - NinjaVanCarrierService - Order Adress is null: "
                        + "[" + (order.ProvinceId == null ? "NULL" : (int)order.ProvinceId) + "]"
                         + "[" + (order.DistrictId == null ? "NULL" : (int)order.DistrictId) + "]"
                          + "[" + (order.WardId == null ? "NULL" : (int)order.WardId) + "]"
                           + "[" + (order.Address == null ? "NULL" : order.Address) + "]");
                    return carrier_packages_id;
                }
                var provinces = _locationESService.GetProvincesByID((int)order.ProvinceId);
                var district = _locationESService.GetDistrictById((int)order.DistrictId);
                var ward = _locationESService.GetWardsById((int)order.WardId);
                var http_client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["Carrier:NinjaVan:Domain"] + _configuration["Carrier:NinjaVan:APIs:CreateOrder"]);
                request.Headers.Add("Authorization", "Bearer "+await GetBearerToken());
                var item = new List<dynamic>();
                foreach(var p in order_mongo.carts)
                {
                    item.Add(new {
                        item_description=p.product.name,
                        quantity=p.quanity,
                        is_dangerous_good=false
                    });
                }
                var request_model = new
                {
                    service_type = "Parcel",
                    service_level = service_level,
                    requested_tracking_number = "",
                    reference = new
                    {
                        merchant_order_number = ""
                    },
                    from = new
                    {
                        name = _configuration["Carrier:NinjaVan:Address:name"],
                        phone_number = _configuration["Carrier:NinjaVan:Address:phone_number"],
                        email = _configuration["Carrier:NinjaVan:Address:email"],
                        address = new
                        {
                            address1 = _configuration["Carrier:NinjaVan:Address:address:address1"],
                            address2 = _configuration["Carrier:NinjaVan:Address:address:address2"],
                            area = _configuration["Carrier:NinjaVan:Address:address:area"],
                            city = _configuration["Carrier:NinjaVan:Address:address:city"],
                            state = _configuration["Carrier:NinjaVan:Address:address:state"],
                            address_type = _configuration["Carrier:NinjaVan:Address:address:address_type"],
                            country = _configuration["Carrier:NinjaVan:Address:address:country"],
                            postcode = _configuration["Carrier:NinjaVan:Address:address:postcode"]
                        }
                    },
                    to = new
                    {
                        name = order.ReceiverName,
                        phone_number = order.Phone,
                        email = client.Email,
                        address = new
                        {
                            address1 = order.Address,
                            address2 = "",
                            area = ward.Name,
                            city = district.Name,
                            state = provinces.Name,
                            address_type = "home",
                            country = "VN",
                            postcode = "100000"
                        }
                    },
                    parcel_job = new {
                        is_pickup_required = true,
                        pickup_service_type = "Scheduled",
                        pickup_service_level = service_level,
                        pickup_date = DateTime.Now.ToString("yyyy-MM-dd"),
                        pickup_timeslot = new
                        {
                            start_time= _configuration["Carrier:NinjaVan:TimeSlot:Pickup:Start"],
                            end_time= _configuration["Carrier:NinjaVan:TimeSlot:Pickup:End"],
                            timezone = _configuration["Carrier:NinjaVan:TimeSlot:Pickup:Timezone"]
                        },
                        pickup_instructions="",
                        delivery_instructions="",
                        delivery_start_date= DateTime.Now.ToString("yyyy-MM-dd"),
                        delivery_timeslot = new
                        {
                            start_time = _configuration["Carrier:NinjaVan:TimeSlot:Delivery:Start"],
                            end_time = _configuration["Carrier:NinjaVan:TimeSlot:Delivery:End"],
                            timezone = _configuration["Carrier:NinjaVan:TimeSlot:Delivery:Timezone"]
                        },
                        cash_on_delivery=order.ShippingFee,
                        dimensions = new
                        {
                            weight= order.PackageWeight
                        },
                        items= item
                    }

                };
                var json = JsonConvert.SerializeObject(request_model);
                var content = new StringContent(json, null, "application/json");
                request.Content = content;
                var response = await http_client.SendAsync(request);
                //response.EnsureSuccessStatusCode();
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    carrier_packages_id = resultContent.tracking_number;

                }

            }
            catch (Exception ex) {

                LogHelper.InsertLogTelegram("PushOrder - NinjaVanCarrierService: " + ex.ToString());

            }
            return carrier_packages_id;

        }
        public async Task<string> GetBearerToken()
        {
            string token = null;
            string encoded=null;
            try
            {
                encoded= _redisConn.Get(CacheName.NINJAVAN_OAUTH, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                if(encoded!=null && encoded.Trim() != "")
                {
                    token = CommonHelper.Decode(token, _configuration["DataBaseConfig:key_api:private_key"]);
                    if(token!=null&& token.Trim() != "")
                    {
                        return token;
                    }
                }
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["Carrier:NinjaVan:Domain"]+ _configuration["Carrier:NinjaVan:APIs:Authentication"]);
                var request_model = new
                {
                    client_id = _configuration["Carrier:NinjaVan:ClientId"],
                    client_secret = _configuration["Carrier:NinjaVan:ClientSecret"],
                    grant_type= "client_credentials"
                };
                var content = new StringContent(JsonConvert.SerializeObject(request_model), null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                //response.EnsureSuccessStatusCode();
                //Console.WriteLine(await response.Content.ReadAsStringAsync());
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    token = resultContent.access_token;
                    if (token != null && token.Trim() != "")
                    {
                        encoded = CommonHelper.Encode(token, _configuration["DataBaseConfig:key_api:private_key"]);
                        _redisConn.Set(CacheName.NINJAVAN_OAUTH, encoded, DateTime.Now.AddSeconds(Convert.ToDouble(resultContent.expires_in) - 60), Convert.ToInt32(_configuration["Redis:Database:db_common"]));

                    }
                }


            }
            catch (Exception ex) {
                LogHelper.InsertLogTelegram("GetBearerToken - NinjaVanCarrierService: " + ex.ToString());
            }
            return token;
        }
    }
}
