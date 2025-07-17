using Entities.ViewModels.ElasticSearch;
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
    
    public class SupplierESRepository
    {
        public string index = "hulotoys_sp_getsupplier";
        private static IConfiguration configuration;
        private readonly ElasticClient _client;
       
        public SupplierESRepository(IConfiguration _configuration)
        {
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:Supplier"];
            var settings = new ConnectionSettings(new Uri(configuration["DataBaseConfig:Elastic:Host"]))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }

        // 1. Function tìm kiếm data theo flashsale_id
        public async Task<SupplierESModel> GetByIdAsync(int flashsaleId)
        {
            var response = await _client.SearchAsync<SupplierESModel>(s => s
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.supplierid)
                        .Value(flashsaleId)
                    )
                )
            );

            return response.Documents.FirstOrDefault();
        }

        // 2. Function xóa theo flashsale_id
        public async Task<bool> DeleteById(int supplierid)
        {
            var response = await _client.DeleteByQueryAsync<SupplierESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field(f => f.supplierid)
                        .Value(supplierid)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }

        // 3. Function insert vào index
        public async Task<bool> InsertAsync(SupplierESModel product)
        {
            var response = await _client.IndexDocumentAsync(product);
            return response.IsValid;
        }
        public async Task<bool> DeleteByIds(List<int> supplierids)
        {
            if (supplierids == null || supplierids.Count == 0)
            {
                return false;
            }

            var response = await _client.DeleteByQueryAsync<SupplierESModel>(q => q
                .Query(rq => rq
                    .Terms(t => t // Use .Terms for multiple values
                        .Field(f => f.supplierid)
                        .Terms(supplierids)
                    )
                )
            );

            if (response.IsValid)
            {
                return true;
            }
            return false;

        }
        public async Task<bool> IndexMany(List<SupplierESModel> flashSales)
        {
            if (flashSales == null || flashSales.Count == 0)
            {
                return false;
            }
            foreach(var item in flashSales)
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
    }



}
