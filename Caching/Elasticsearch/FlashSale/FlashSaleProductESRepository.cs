using IdGen;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Caching.Elasticsearch.FlashSale
{
    
    public class FlashSaleProductESRepository 
    {
        public string index = "hulotoys_sp_getflashsaleproduct";
        private static IConfiguration configuration;
        private readonly ElasticClient _client;

        public FlashSaleProductESRepository(IConfiguration _configuration)
        {

            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:FlashSaleProduct"];
            var settings = new ConnectionSettings(new Uri(configuration["DataBaseConfig:Elastic:Host"]))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        // 1. Function tìm kiếm data theo flashsale_id
        public async Task<FlashSaleProductESModel> GetByIdAsync(long flashsale_productid)
        {
            var response = await _client.SearchAsync<FlashSaleProductESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.flashsale_productid)
                        .Value(flashsale_productid)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }

        // 2. Function xóa theo flashsale_id
        public async Task<bool> DeleteByProductIdAsync(long flashsale_productid)
        {
            var response = await _client.DeleteByQueryAsync<FlashSaleProductESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field(f => f.flashsale_productid)
                        .Value(flashsale_productid)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }
        public async Task<bool> DeleteAll()
        {
            var response = await _client.DeleteByQueryAsync<FlashSaleESModel>(q => q
                .Query(rq => rq
                    .MatchAll()
                )
            );

            return response.IsValid && response.Deleted > 0;
        }
        // 3. Function insert vào index
        public async Task<bool> InsertAsync(FlashSaleProductESModel product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }
        public async Task<List<FlashSaleProductESModel>> GetByFlashsaleId(int flashsale_id)
        {
            var now = DateTime.Now; 

            var response = await _client.SearchAsync<FlashSaleProductESModel>(s => s
                .Query(q => q
                    .Term(t => t
                                .Field(f => f.flashsale_id)
                                .Value(flashsale_id)
                                )
                )
            );

            if (response.IsValid)
            {
                return response.Documents.ToList();
            }
            else
            {
                return new List<FlashSaleProductESModel>();
            }
        }
        public async Task<bool> DeleteByIds(List<long> flashsale_productids)
        {
            if (flashsale_productids == null || flashsale_productids.Count == 0)
            {
                return false;
            }

            var response = await _client.DeleteByQueryAsync<FlashSaleProductESModel>(q => q
                .Query(rq => rq
                    .Terms(t => t // Use .Terms for multiple values
                        .Field(f => f.flashsale_productid)
                        .Terms(flashsale_productids)
                    )
                )
            );

            if (response.IsValid)
            {
                return true;
            }
            return false;

        }
        public async Task<bool> IndexMany(List<FlashSaleProductESModel> flashSales)
        {
            if (flashSales == null || flashSales.Count == 0)
            {
                return false;
            }
            foreach (var item in flashSales)
            {
                await _client.IndexDocumentAsync(item);
            }
            return true;

        }
        public long GenerateId()
        {
            IdGenerator _generator = new(0); // Machine ID = 0
            return _generator.CreateId();
        }
        public async Task<List<FlashSaleProductESModel>> GetByProductId(string product_id)
        {
            var now = DateTime.Now;

            var response = await _client.SearchAsync<FlashSaleProductESModel>(s => s
                .Query(q => q
                    .Term(t => t
                                .Field(f => f.productid)
                                .Value(product_id)
                                )
                )
            );

            if (response.IsValid)
            {
                return response.Documents.ToList();
            }
            else
            {
                return new List<FlashSaleProductESModel>();
            }
        }
        public async Task<bool> UpdateFlashSaleGroup(string productId, string newGroupId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return false;
            }

            var updateResponse = await _client.UpdateAsync<FlashSaleProductESModel, object>(
                productId, 
                u => u.Doc(new { group_id = newGroupId }) 
                      .DocAsUpsert(false)
            );

            return updateResponse.IsValid;
        }
    }



}
