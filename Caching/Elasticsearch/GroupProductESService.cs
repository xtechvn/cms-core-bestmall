using Caching.Elasticsearch;
using Elasticsearch.Net;
using Entities.Models;
using Entities.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utilities;
using Utilities.Contants;

namespace HuloToys_Service.ElasticSearch.NewEs
{
    public class GroupProductESService
    {
        public string index = "group_product_hulotoys_store";
        private readonly IConfiguration configuration;
        private readonly ElasticClient _client;

        public GroupProductESService(IConfiguration _configuration)
        {
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:GroupProduct"];
            var settings = new ConnectionSettings(new Uri(configuration["DataBaseConfig:Elastic:Host"]))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }
        public List<GroupProduct> GetListGroupProductByParentId(long parent_id)
        {
            try
            {
                var query = _client.Search<GroupProductESModel>(sd => sd
                               .Index(index)
                               .Size(4000)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                   sh => sh.Match(m => m.Field(y=>y.ParentId).Query(parent_id.ToString())
                                   )
                                   )
                               )
                          ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<GroupProductESModel>;
                    var result = JsonConvert.DeserializeObject<List<GroupProduct>>(JsonConvert.SerializeObject(data));
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegram( error_msg);
            }
            return null;
        }
        public GroupProduct GetDetailGroupProductById(long id)
        {
            try
            {
                var query = _client.Search<GroupProductESModel>(sd => sd
                               .Index(index)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field(y=>y.Status).Query(((int)ArticleStatus.PUBLISH).ToString())),
                                   sh => sh.Match(m => m.Field(y=>y.Id).Query(id.ToString())
                                   )
                                   )
                               )
                          ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<GroupProductESModel>;
                    var result = JsonConvert.DeserializeObject<List<GroupProduct>>(JsonConvert.SerializeObject(data));

                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegram(error_msg);
            }
            return null;
        }
        public List<GroupProduct> GetAll()
        {
            try
            {
                var query = _client.Search<GroupProductESModel>(sd => sd
                               .Index(index)
                               .Size(4000)
                          .Query(q =>
                          q.MatchAll()));

                if (query.IsValid)
                {
                    var data = query.Documents as List<GroupProductESModel>;
                    var result = JsonConvert.DeserializeObject<List<GroupProduct>>(JsonConvert.SerializeObject(data));

                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.Message;
                LogHelper.InsertLogTelegram(error_msg);
            }
            return null;
        }

    }
}
