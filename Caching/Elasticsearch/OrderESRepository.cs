using Elasticsearch.Net;
using Entities.Models;
using Entities.ViewModels.ElasticSearch;
using ENTITIES.ViewModels.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Caching.Elasticsearch
{
    public class OrderESRepository : ESRepository<OrderElasticsearchViewModel>
    {
        public string index_name = "hulotoys_sp_getorder";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;
        private static ElasticClient _elasticClient;
        public OrderESRepository(string Host, IConfiguration _configuration) : base(Host)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index_name = _configuration["DataBaseConfig:Elastic:Index:Order"];
            var nodes = new Uri[] { new Uri(_ElasticHost) };
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(index_name);
            _elasticClient = new ElasticClient(connectionSettings);
        }
        public async Task<List<OrderElasticsearchViewModel>> GetOrderNoSuggesstion(string txt_search)
        {
            List<OrderElasticsearchViewModel> result = new List<OrderElasticsearchViewModel>();
            try
            {
                
                var query_param = "*" + txt_search.Trim().ToUpper() + "*";
                var searchDescriptor = new SearchDescriptor<OrderElasticsearchViewModel>()
            .Index(index_name)
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .MatchPhrasePrefix(mp => mp
                            .Field(f => f.OrderNo)
                            .Query(txt_search.Trim()) // No wildcards here, let the analyzer handle it
                        )
                    )
                )
            )
            .From(0)
            .Size(10);

                var search_response = await _elasticClient.SearchAsync<OrderElasticsearchViewModel>(searchDescriptor);

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
                    result = search_response.Documents as List<OrderElasticsearchViewModel>;
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
                          .Index(index_name + (_company_type.Trim() == "0" ? "" : "_" + _company_type.Trim()))
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
