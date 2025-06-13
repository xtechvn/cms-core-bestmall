using Elasticsearch.Net;
using Entities.Models;
using Entities.ViewModels.ElasticSearch;
using ENTITIES.ViewModels.ElasticSearch;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Caching.Elasticsearch
{
    public class OrderESRepository
    {
        public string index_name = "hulotoys_sp_getorder";
        private readonly IConfiguration configuration;
        private static ElasticClient _elasticClient;
        public OrderESRepository( IConfiguration _configuration) 
        {
            configuration = _configuration;
            index_name = _configuration["DataBaseConfig:Elastic:Index:Order"];
            var settings = new ConnectionSettings(new Uri(configuration["DataBaseConfig:Elastic:Host"]))
               .DefaultIndex(index_name);
            _elasticClient = new ElasticClient(settings);
        }
        public async Task<List<OrderElasticsearchViewModel>> GetOrderNoSuggesstion(string txt_search)
        {
            List<OrderElasticsearchViewModel> result = new List<OrderElasticsearchViewModel>();
            try
            {
                
                var query_param = "*" + txt_search.Trim().ToUpper() + "*";
                var searchDescriptor = new SearchDescriptor<object>()
            .Index(index_name)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .MatchPhrasePrefix(mp => mp
                            .Field("OrderNo")
                            .Query(txt_search.Trim()) // No wildcards here, let the analyzer handle it
                        )
                    )
                )
            )
            .From(0)
            .Size(10);

                var search_response = await _elasticClient.SearchAsync<object>(searchDescriptor);

                //var search_response = elasticClient.Search<OrderElasticsearchViewModel>(s => s
                //    .Index(index_name)
                //    .Size(top)
                //    .Query(q =>
                //        q.QueryString(qs => qs
                //            .Fields(fs => fs
                //                .Field(f => f.OrderNo)
                //            )
                //            .Query(query_param) // No wildcards here, let the analyzer handle it
                //            .DefaultOperator(Operator.And) // Or Operator.Or, depending on desired behavior
                //            .Analyzer("standard")
                //        )
                //    )
                //);

                if (search_response.IsValid)
                {
                    var json = JsonConvert.SerializeObject(search_response.Documents);
                    result = JsonConvert.DeserializeObject<List<OrderElasticsearchViewModel>>(json);
                   // result = search_response.Documents as List<OrderElasticsearchViewModel>;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderNoSuggesstion - OrderESRepository. " + ex);
                return null;
            }
            return result;

        }
        public async Task<List<OrderElasticsearchViewModel>> GetOrderNoSuggesstion2(string txt_search)
        {
            List<OrderElasticsearchViewModel> result = new List<OrderElasticsearchViewModel>();
            try
            {
                int top = 30;

                var search_response = _elasticClient.Search<OrderElasticsearchViewModel>(s => s
                          .Index(index_name)
                          .Size(top)
                          .Query(q =>
                           q.Bool(
                               qb => qb.Must(
                                   sh => sh.QueryString(qs => qs
                                   .Fields(new[] { "orderno" })
                                   .Query("*" + txt_search.ToUpper() + "*")
                                   .Analyzer("standard")

                            )
                           )
                          )));

                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<OrderElasticsearchViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderNoSuggesstion - OrderESRepository. " + ex);
                return null;
            }

        }
    }
}
