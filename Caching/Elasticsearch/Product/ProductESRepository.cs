using Elasticsearch.Net;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Products;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Caching.Elasticsearch
{
    //https://www.steps2code.com/post/how-to-use-elasticsearch-in-csharp
    public class ProductESRepository : ESRepository<ProductESModel>
    {
        public string index = "hulotoys_mongodb_product";
        private static string _ElasticHost;
        private static IConfiguration configuration;
        private readonly ElasticClient _client;

        public ProductESRepository(string Host, IConfiguration _configuration) : base(Host)
        {
            _ElasticHost = Host;
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:Product"];
            var settings = new ConnectionSettings(new Uri(_ElasticHost))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        // 1. Function tìm kiếm data theo product_id
        public async Task<ProductESModel> GetByProductIdAsync(string productId)
        {
            var response = await _client.SearchAsync<ProductESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.product_id)
                        .Value(productId)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }

        // 2. Function xóa theo product_id
        public async Task<bool> DeleteByProductIdAsync(string productId)
        {
            var response = await _client.DeleteByQueryAsync<ProductESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field(f => f.product_id)
                        .Value(productId)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }

        // 3. Function insert vào index
        public async Task<bool> InsertAsync(ProductESModel product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }
        // Tìm kiếm theo keyword trên trường name và product_code
        public async Task<List<ProductESModel>> SearchByKeywordAsync(string keyword)
        {
            var response = await _client.SearchAsync<ProductESModel>(s => s
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(p => p.name)
                            .Field(p => p.product_code)
                        )
                        .Query(keyword)
                        .Type(TextQueryType.BestFields)
                        .Analyzer("standard") // Sử dụng analyzer chuẩn hỗ trợ Unicode
                        .Fuzziness(Fuzziness.Auto) // Cho phép tìm gần đúng
                    )
                )
            );

            return response.Documents.ToList();
        }


    }




}
