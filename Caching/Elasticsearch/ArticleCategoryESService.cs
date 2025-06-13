using Caching.Elasticsearch;
using Elasticsearch.Net;
using Entities.Models;
using HuloToys_Service.Models.Article;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Utilities;

namespace HuloToys_Service.ElasticSearch
{
    public class ArticleCategoryESService 
    {
        public string index = "hulotoys_mongodb_product";
        private static IConfiguration configuration;
        private readonly ElasticClient _client;

        public ArticleCategoryESService(IConfiguration _configuration) 
        {
            configuration = _configuration;
            index = _configuration["DataBaseConfig:Elastic:Index:ArticleCategory"];
            var settings = new ConnectionSettings(new Uri(configuration["DataBaseConfig:Elastic:Host"]))
                .DefaultIndex(index);
            _client = new ElasticClient(settings);
        }
        public List<ArticleCategoryESModel> GetByArticleId(long id)
        {
            try
            {
                var query = _client.Search<ArticleCategoryESModel>(sd => sd
                               .Size(4000)
                               .Query(q => q
                                   .Match(m => m.Field("ArticleId").Query(id.ToString())
                               )));

                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleCategoryESModel>;

                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegram( error_msg);
            }
            return null;
        }
        public List<ArticleCategoryESModel> GetListArticleCategory()
        {
            try
            {

                var query = _client.Search<ArticleCategoryESModel>(sd => sd
                               .Size(4000)
                               .Query(q => q.MatchAll()
                               ));

                if (query.IsValid)
                {
                    var data = query.Documents as List<ArticleCategoryESModel>;
           
                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegram( error_msg);
            }
            return null;
        }
        public List<ArticleCategoryESModel> GetByCategoryId(long CategoryId)
        {
            try
            {

                var query = _client.Search<ArticleCategoryESModel>(sd => sd
                               .Query(q => q
                                   .Term(m => m.Field("CategoryId").Value(CategoryId)
                               )));

                if (query.IsValid)
                {

                    var data = query.Documents as List<ArticleCategoryESModel>;
                    //var result = data.Select(a => new ArticleCategoryViewModel
                    //{

                    //    Id = a.id,
                    //    ArticleId = a.articleid,
                    //    CategoryId = a.categoryid,


                    //}).ToList();
                    return data;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegram( error_msg);
            }
            return null;
        }
        public async Task<bool> DeleteByArticleId(long articleid)
        {

            var response = await _client.DeleteByQueryAsync<ArticleCategoryESModel>(q => q
                .Query(rq => rq
                    .Term(t => t
                        .Field("ArticleId")
                        .Value(articleid)
                    )
                )
            );

            return response.IsValid && response.Deleted > 0;
        }
        public async Task<bool> Insert(ArticleCategoryESModel item)
        {
            var response = await _client.IndexDocumentAsync(item);
            return response.IsValid;
        }
    }
}
