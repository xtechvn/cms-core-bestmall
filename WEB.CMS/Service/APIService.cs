using Entities.ViewModels;
using Entities.ViewModels.API;
using Entities.ViewModels.ApiSever;

using ENTITIES.ViewModels.Notify;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Models;

namespace WEB.Adavigo.CMS.Service
{
    public class APIService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public APIService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;

        }
        
      
        public async Task<int> SendMessage(string user_id_send, string user_receiver_id,  string action_type, string Code, string link_redirect  )
        {
            try
            {

                var user = await _userRepository.GetDetailUser(Convert.ToInt32(user_id_send));
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, object>
                {
                   {"user_name_send", user.Entity.FullName.ToString()}, //tên người gửi
                    {"user_id_send", user_id_send}, //id người gửi
                    {"code", Code}, // mã đơn Hàng
                    {"link_redirect", link_redirect}, // Link mà khi người dùng click vào detail item notify sẽ chuyển sang đó
                    //{"module_type", module_type}, // loại module thực thi luồng notify. Ví dụ: Đơn hàng, khách hàng.......
                    {"action_type", action_type} // action thực hiện. Ví dụ: Duyệt, tạo mới, từ chối....
                   // {"role_type", role_type}, // quyền mà sẽ gửi tới
                   /// {"service_code", service_code}// mã dịch vụ
                    ,{"user_receiver_id", user_receiver_id} // id người nhận notify
                };
                var data_product = JsonConvert.SerializeObject(j_param);

                var token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:b2c"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = "http://api.best-mall.vn" + "/api/notify/message/send.json";
                var response = await httpClient.PostAsync(url, request);
                if (response.IsSuccessStatusCode)
                {
                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendMessage-apisever:" + ex.ToString());
                return 1;
            }
        }

        public async Task<NotifySummeryViewModel> GetListNotify(string user_id, int pageindex, int pagesize)
        {
            try
            {

                HttpClient httpClient = new HttpClient();
                NotifySummeryViewModel result = null;
                var j_param = new Dictionary<string, object>
                {
                       {"user_id", user_id},
                       {"pageindex", pageindex},
                       {"pagesize", pagesize}
                };
                var data_product = JsonConvert.SerializeObject(j_param);

                var token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:b2c"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().Notify_Get_List;
                var response = await httpClient.PostAsync(url, request);
                var stringResult = "";

                if (response.IsSuccessStatusCode)
                {
                    stringResult = response.Content.ReadAsStringAsync().Result;

                    var data = JsonConvert.DeserializeObject<NotifyRedisViewModel>(stringResult);
                    result = data.data;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListNotify:" + ex.ToString());
                return null;
            }
        }
        public async Task<int> UpdateNotify(string notify_id, string user_seen_id, string seen_status)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, object>
                {
                  /*  {"notify_id", "A1,A32"}, // 
                    {"user_seen_id", "222"},*/
                     {"notify_id", notify_id}, // 
                    {"user_seen_id", user_seen_id},
                    {"seen_status", seen_status}, // SEEN_ALL = 1: click vao chuông |    SEEN_DETAIL = 2  click vao item notify

                };
                var data = JsonConvert.SerializeObject(j_param);
                var a = _configuration["DataBaseConfig:key_api:b2c"];
                var token = EncodeHelpers.Encode(data, _configuration["DataBaseConfig:key_api:b2c"]);
                var request = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("token",token)
                });
                var url = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().Notify_update_status;
                var response = await httpClient.PostAsync(url, request);


                if (response.IsSuccessStatusCode)
                {

                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("apisever:" + ex.ToString());
                return 1;
            }
        }
       
    
    }
}
