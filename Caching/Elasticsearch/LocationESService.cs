using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Entities.Models;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Caching.Elasticsearch
{
    public class LocationESService : ESRepository<Province>
    {
        public string index_province = "provinces_store";
        public string index_district = "districts_store";
        public string index_wards = "wards_store";
        private readonly IConfiguration configuration;
        private static string _ElasticHost;

        public LocationESService(string Host, IConfiguration _configuration) : base(Host)
        {
            _ElasticHost = Host;
            configuration = _configuration;

        }
        public List<Province> GetAllProvinces()
        {
            List<Province> result = new List<Province>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
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
                    //result = query.Documents as List<Province>;
                    result = JsonConvert.DeserializeObject<List<Province>>(JsonConvert.SerializeObject(query.Documents));
                    return result;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public Province GetProvincesByProvinceId(string provinces_id)
        {
            List<Province> result = new List<Province>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_province)
                            .Size(4000)
                           .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("ProvinceId").Query(provinces_id)
                                   )

                                )))
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    //result = query.Documents as List<Province>;
                    result = JsonConvert.DeserializeObject<List<Province>>(JsonConvert.SerializeObject(query.Documents));
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public Province GetProvincesByID(int provinces_id)
        {
            List<Province> result = new List<Province>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_province)
                            .Size(4000)
                           .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Term(m => m.Field("Id").Value(provinces_id))
                                   )

                                ))
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    //result = query.Documents as List<Province>;
                    result = JsonConvert.DeserializeObject<List<Province>>(JsonConvert.SerializeObject(query.Documents));
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        public List<District> GetAllDistrict()
        {
            List<District> result = new List<District>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
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
                    //result = query.Documents as List<District>;
                    result = JsonConvert.DeserializeObject<List<District>>(JsonConvert.SerializeObject(query.Documents));

                    return result;
                }
            }
            catch (Exception ex)
            {
              
            }
            return null;
        }
        public List<District> GetAllDistrictByProvinces(string provinces_id)
        {
            List<District> result = new List<District>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_district)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("ProvinceId").Query(provinces_id)
                                   )

                                )))
                            );

                if (!query.IsValid)
                {
                    return result;
                }
                else
                {
                    //result = query.Documents as List<District>;
                    result = JsonConvert.DeserializeObject<List<District>>(JsonConvert.SerializeObject(query.Documents));

                    return result;
                }
            }
            catch (Exception ex)
            {
                
            }
            return null;
        }
        public District GetDistrictByDistrictId(string district_id)
        {
            List<District> result = new List<District>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_district)
                            .Size(4000)
                           .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("DistrictId").Query(district_id)
                                   )

                                ))
                                )
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    //result = query.Documents as List<District>;
                    result = JsonConvert.DeserializeObject<List<District>>(JsonConvert.SerializeObject(query.Documents));

                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
              
            }
            return null;
        }
        public District GetDistrictById(int district_id)
        {
            List<District> result = new List<District>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_district)
                            .Size(4000)
                           .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Term(m => m.Field("Id").Value(district_id))
                                   )

                                ))
                                
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    //result = query.Documents as List<District>;
                    result = JsonConvert.DeserializeObject<List<District>>(JsonConvert.SerializeObject(query.Documents));

                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public List<Ward> GetAllWards()
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
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
                    //result = query.Documents as List<Ward>;
                    result = JsonConvert.DeserializeObject<List<Ward>>(JsonConvert.SerializeObject(query.Documents));

                    return result;
                }
            }
            catch (Exception ex)
            {
                
            }
            return null;
        }
        public List<Ward> GetAllWardsByDistrictId(string district_id)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("DistrictId").Query(district_id)
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
                    //result = query.Documents as List<Ward>;
                    result = JsonConvert.DeserializeObject<List<Ward>>(JsonConvert.SerializeObject(query.Documents));

                    return result;
                }
            }
            catch (Exception ex)
            {
               
            }
            return null;
        }
        public Ward GetWardsByWardId(string ward_id)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Match(m => m.Field("WardId").Query(ward_id)
                                   )

                                ))
                                )
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    //result = query.Documents as List<Ward>;
                    result = JsonConvert.DeserializeObject<List<Ward>>(JsonConvert.SerializeObject(query.Documents));

                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
               
            }
            return null;
        }
        public Ward GetWardsById(int ward_id)
        {
            List<Ward> result = new List<Ward>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var query = elasticClient.Search<dynamic>(sd => sd
                            .Index(index_wards)
                            .Size(4000)
                            .Query(q => q.Bool(
                               qb => qb.Must(
                                  q => q.Term(m => m.Field("Id").Value(ward_id))
                                   )

                                ))
                                
                            );

                if (!query.IsValid)
                {
                    return null;
                }
                else
                {
                    //result = query.Documents as List<Ward>;
                    result = JsonConvert.DeserializeObject<List<Ward>>(JsonConvert.SerializeObject(query.Documents));

                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
               
            }
            return null;
        }
    }
}
