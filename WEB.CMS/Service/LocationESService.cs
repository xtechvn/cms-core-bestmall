using Elasticsearch.Net;
using Entities.Models;
using Nest;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using System.Configuration;
using System.Reflection;
using Utilities;

namespace Caching.Elasticsearch
{
    public class LocationESService
    {
        public string index_province = "provinces_store";
        public string index_district = "districts_store";
        public string index_wards = "wards_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;
        private static ElasticClient elasticClient;

        public LocationESService(IConfiguration _configuration)
        {
            configuration = _configuration;
            _ElasticHost = _configuration["DataBaseConfig:Elastic:Host"];
            configuration = _configuration;
            index_province = _configuration["DataBaseConfig:Elastic:Index:Provinces"];
            index_district = _configuration["DataBaseConfig:Elastic:Index:Districts"];
            index_wards = _configuration["DataBaseConfig:Elastic:Index:Wards"];
            var nodes = new Uri[] { new Uri(_ElasticHost) };
            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
            elasticClient = new ElasticClient(connectionSettings);
        }
        public List<Province> GetAllProvinces()
        {
            List<Province> result = new List<Province>();
            try
            {
                var query = elasticClient.Search<Province>(sd => sd
                            .Index(index_province)
                            .Size(4000)
                            .Query(q => q
                                .MatchAll()
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Province>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public Province GetProvincesByProvinceId(string provinces_id)
        {
            List<Province> result = new List<Province>();
            try
            {
              
                ////var query = elasticClient.Search<Province>(sd => sd
                ////            .Index(index_province)
                ////            .Size(4000)
                ////           .Query(q => q.Bool(
                ////               qb => qb.Must(
                ////                  q => q.Match(m => m.Field("ProvinceId").Query(provinces_id)
                ////                   )

                ////                )))
                ////            );
                var query = elasticClient.Search<Province>(sd => sd
                    .Index(index_province)
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .Match(match => match
                                    .Field("ProvinceId") 
                                    .Query(provinces_id)       
                                )
                            )
                        )
                    )
                );
                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    result = query.Documents as List<Province>;
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public Province GetProvincesById(int id)
        {
            try
            {
                var query = elasticClient.Search<Province>(sd => sd
                    .Index(index_province)
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .Term(match => match
                                    .Field(x=>x.Id)
                                    .Value(id)
                                )
                            )
                        )
                    )
                );
                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                   var result = query.Documents as List<Province>;
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<District> GetAllDistrict()
        {
            List<District> result = new List<District>();
            try
            {
                var query = elasticClient.Search<District>(sd => sd
                            .Index(index_district)
                             .Size(4000)
                            .Query(q => q
                                .MatchAll()
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<District>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<District> GetAllDistrictByProvinces(string provinces_id)
        {
            List<District> result = new List<District>();
            try
            {
                var query = elasticClient.Search<District>(sd => sd
                            .Index(index_district)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field(y => y.ProvinceId).Query(provinces_id)
                                   )

                                )))
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<District>;

                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public District GetDistrictByDistrictId(string district_id)
        {
            List<District> result = new List<District>();
            try
            {
                //var query = elasticClient.Search<District> (sd => sd
                //            .Index(index_district)
                //            .Size(4000)
                //           .Query(q => q.Bool(
                //               qb => qb.Must(
                //                  q => q.Match(m => m.Field("DistrictId").Query(district_id)
                //                   )

                //                ))
                //                )
                //            );
                var query = elasticClient.Search<District>(sd => sd
                    .Index(index_district)
                  .Query(q => q
                      .Bool(b => b
                          .Must(m => m
                              .Match(match => match
                                  .Field("DistrictId")
                                  .Query(district_id.Trim())
                              )
                          )
                      )
                  )
              );
                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    result = query.Documents as List<District>;

                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public District GetDistrictById(int id)
        {
            try
            {
                var query = elasticClient.Search<District>(sd => sd
                     .Index(index_district)
                     .Query(q => q
                         .Bool(b => b
                             .Must(m => m
                                 .Term(match => match
                                     .Field(x => x.Id)
                                     .Value(id)
                                 )
                             )
                         )
                     )
                 );
                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                   var result = query.Documents as List<District>;
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }

        public List<Ward> GetAllWards()
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var query = elasticClient.Search<Ward>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q
                                .MatchAll()
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Ward>;

                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<Ward> GetAllWardsByDistrictId(string district_id)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var query = elasticClient.Search<Ward>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field(y => y.DistrictId).Query(district_id)
                                   )

                                ))
                                )
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Ward>;

                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public Ward GetWardsByWardId(string ward_id)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                //var query = elasticClient.Search<Ward>(sd => sd
                //            .Index(index_wards)
                //            .Size(4000)
                //            .Query(q => q.Bool(
                //               qb => qb.Must(
                //                  q => q.Match(m => m.Field(y=>y.WardId).Query(ward_id)
                //                   )

                //                ))
                //                )
                //            );
                var query = elasticClient.Search<Ward>(sd => sd
                            .Index(index_wards)
                 .Query(q => q
                     .Bool(b => b
                         .Must(m => m
                             .Match(match => match
                                 .Field("WardId")
                                 .Query(ward_id.Trim())
                             )
                         )
                     )
                 )
                );
                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    result = query.Documents as List<Ward>;

                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public Ward GetWardById(int id)
        {
            try
            {
                var query = elasticClient.Search<Ward>(sd => sd
                     .Index(index_wards)
                     .Query(q => q
                         .Bool(b => b
                             .Must(m => m
                                 .Term(match => match
                                     .Field(x => x.Id)
                                     .Value(id)
                                 )
                             )
                         )
                     )
                 );
                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    var result = query.Documents as List<Ward>;
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }

        public List<Ward> WardSuggestion(string keyword, int district_id = -1)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var query = elasticClient.Search<Ward>(s => s
                     .Index(index_wards)
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .Match(t => t
                                    .Field(f => f.DistrictId) // Giả định ProvinceId là một trường trong YourDocumentType
                                    .Query(district_id.ToString())
                                )
                            )
                           .Should(sh => sh // `Should` nghĩa là một trong các điều kiện này nên khớp
                                .QueryString(qs => qs
                                    .DefaultField(f => f.Name)
                                    .Query($"*{keyword}*") // Tìm kiếm với wildcard cho "ha noi"
                                )
                                || sh
                                .QueryString(qs => qs
                                    .DefaultField(f => f.NameNonUnicode)
                                    .Query($"*{keyword}*") // Tìm kiếm với wildcard cho "ha noi"
                                )
                            )
                        )
                    )
                    .From(0)
                    .Size(100)
                );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Ward>;

                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<District> DistrictSuggestion(string keyword, int province_id = -1)
        {
            List<District> result = new List<District>();
            try
            {
                var query = elasticClient.Search<District>(s => s
                     .Index(index_wards)
                    .Query(q => q
                        .Bool(b => b
                            .Must(m => m
                                .Match(t => t
                                    .Field(f => f.ProvinceId) // Giả định ProvinceId là một trường trong YourDocumentType
                                    .Query(province_id.ToString())
                                )
                            )
                           .Should(sh => sh // `Should` nghĩa là một trong các điều kiện này nên khớp
                                .QueryString(qs => qs
                                    .DefaultField(f => f.Name)
                                    .Query($"*{keyword}*") // Tìm kiếm với wildcard cho "ha noi"
                                )
                                || sh
                                .QueryString(qs => qs
                                    .DefaultField(f => f.NameNonUnicode)
                                    .Query($"*{keyword}*") // Tìm kiếm với wildcard cho "ha noi"
                                )
                            )
                        )
                    )
                    .From(0)
                    .Size(100)
                );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<District>;

                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }
        public List<Province> ProvinceSuggestion(string keyword)
        {
            List<Province> result = new List<Province>();
            try
            {
                var query = elasticClient.Search<Province>(sd => sd
                  .Index(index_province)
                  .Size(100)
                  .Query(q => q.Bool(
                      qb => qb.Should(
                          q1 => q1.QueryString(m => m
                              .DefaultField(f => f.Name) // Chỉ tìm kiếm trên trường Name
                              .Query("*" + keyword.Trim() + "*")
                              .AnalyzeWildcard(true)
                          ),
                          q2 => q2.QueryString(m => m
                              .DefaultField(f => f.NameNonUnicode) // Chỉ tìm kiếm trên trường NameNonUnicode
                              .Query("*" + StringHelpers.RemoveUnicode(keyword.Trim()) + "*")
                              .AnalyzeWildcard(true)
                          )
                      ).MinimumShouldMatch(1) // Chỉ cần khớp với ít nhất một trong hai điều kiện
                  ))
              );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    result = query.Documents as List<Province>;

                    return result;
                }
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(configuration["BotSetting:bot_token"], configuration["BotSetting:bot_group_id"], error_msg);
            }
            return null;
        }

    }
}
