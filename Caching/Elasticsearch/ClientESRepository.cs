using Elasticsearch.Net;
using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Caching.Elasticsearch
{
    public class ClientESRepository 
    {
        public string index = "hulotoys_sp_getclient";
        private readonly ElasticClient _client;

        public ClientESRepository(IConfiguration configuration)  {

            index = configuration["DataBaseConfig:Elastic:Index:Client"];
            var settings = new ConnectionSettings(new Uri(configuration["DataBaseConfig:Elastic:Host"]))
                 .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }


        public async Task<List<CustomerESViewModel>> GetClientSuggesstion(string txt_search)
        {
            List<CustomerESViewModel> result = new List<CustomerESViewModel>();
            try
            {
                int top = 4000;
               
                if (txt_search == null)
                {
                    var result_all = _client.Search<object>(s => s
                          .Size(30)
                          .Query(q => q.MatchAll()

                           ));
                    var json = JsonConvert.SerializeObject(result_all.Documents);
                    result = JsonConvert.DeserializeObject<List<CustomerESViewModel>>(json) ;
                    return result;
                }
                var search_response = _client.Search<object>(s => s
                          .Size(top)
                          .Query(q =>
                            q.Bool(
                                qb => qb.Should(
                                    sh => sh.QueryString(m => m
                                    .DefaultField("Phone")
                                    .Query("*" + txt_search + "*")),
                                    sh => sh.QueryString(m => m
                                    .DefaultField("Email")
                                    .Query("*" + txt_search + "*")),
                                    sh => sh.QueryString(m => m
                                    .DefaultField("ClientName")
                                    .Query("*" + txt_search + "*"))

                                ))
                           ));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    var json = JsonConvert.SerializeObject(search_response.Documents);
                    result = JsonConvert.DeserializeObject<List<CustomerESViewModel>>(json);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    
        public int UpSert(ClientESViewModel entity)
        {
            try
            {

                var indexResponse = _client.Index(new IndexRequest<ClientESViewModel>(entity,index));

                if (!indexResponse.IsValid)
                {

                    return 0;
                }

                return 1;
            }
            catch
            {
                return 0;
            }
        }
    }
    public class esService
    {
        private IConfiguration configuration;
        public static string _company_type;


        public esService(IConfiguration _configuration)
        {
            configuration = _configuration;
            AppSettings _appconfig = new AppSettings();
            using (StreamReader r = new StreamReader("appsettings.json"))
            {
                string json = r.ReadToEnd();
                _appconfig = JsonConvert.DeserializeObject<AppSettings>(json);
                _company_type = _appconfig.CompanyType;
            }
        }

        //cuonglv
        // Tìm kiếm thông tin  theo từ khóa trong nhiều cột
        /// <summary>
        /// ///
        /// </summary>
        /// <param name="keyword">từ khóa cần tìm kiếm</param>
        /// <param name="file_name">file chứa input cần tìm kiếm chuẩn json</param>        
        /// <returns></returns>
        public async Task<string> search(string keyword, string file_name)
        {
            try
            {
                string endpoint = string.Empty;
                string url_es = configuration["DataBaseConfig:Elastic:Host"];
                string Client = configuration["DataBaseConfig:Elastic:Index:Client"];
                var workingDirectory = Environment.CurrentDirectory;
                //  var currentDirectory = Directory.GetParent(workingDirectory);
                var query = workingDirectory + @"\QueryEs\" + file_name;

                var body_raw_input = File.ReadAllText(query);
                body_raw_input = body_raw_input.Replace("{index_name}", Client);

                var j_input = JObject.Parse(body_raw_input);
                endpoint = j_input["endpoint"].ToString();
                body_raw_input = j_input["input_query"].ToString();




                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url_es + endpoint);
                    var content = new StringContent(body_raw_input.Replace("{keyword}", keyword), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url_es + endpoint, content);
                    var data= await response.Content.ReadAsStringAsync();
                    return data;
                }
                return string.Empty;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(" esService - searchMultiMatch: " + ex.ToString());
                return string.Empty;
            }
        }
    }
}
