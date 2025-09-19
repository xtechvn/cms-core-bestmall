using Azure.Core;
using Elasticsearch.Net;
using Entities.Models;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Products;
using IdGen;
using MongoDB.Bson;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;
using Utilities.Contants;
using Utilities.Contants.ProductV2;

namespace WEB.CMS.Models.Product
{
    public class ProductDetailMongoAccess
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<ProductMongoDbModel> _productDetailCollection;
        List<int> status_sub = new List<int>() { (int)ProductStatus.ACTIVE, (int)ProductStatus.DEACTIVE, (int)ProductStatus.ON_WAITING_CONFIRM };

        public ProductDetailMongoAccess(IConfiguration configuration)
        {
            _configuration = configuration;
            //mongodb://adavigolog_writer:adavigolog_2022@103.163.216.42:27017/?authSource=HoanBds
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:user"] +
                ":" + configuration["DataBaseConfig:MongoServer:pwd"] +
                "@" + configuration["DataBaseConfig:MongoServer:Host"] +
                ":" + configuration["DataBaseConfig:MongoServer:Port"] +
                "/?authSource=" + configuration["DataBaseConfig:MongoServer:catalog"] + "";

            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            _productDetailCollection = db.GetCollection<ProductMongoDbModel>("ProductDetail");
        }
        public async Task<string> AddNewAsync(ProductMongoDbModel model)
        {
            try
            {
                model.GenID();
                await _productDetailCollection.InsertOneAsync(model);
                return model._id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewAsync - ProductDetailMongoAccess: " + ex.ToString());
                return null;
            }
        }
        public async Task<string> UpdateAsync(ProductMongoDbModel model)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await _productDetailCollection.FindOneAndReplaceAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAsync - ProductDetailMongoAccess: " + ex.ToString());
                return null;
            }
        }


        public async Task<ProductMongoDbModel> GetByID(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x._id, id); ;
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - GetByID Error: " + ex);
                return null;
            }
        }


        // Thêm method này vào ProductV2DetailMongoAccess
        public async Task<List<ProductMongoDbModel>> GetAllProducts()
        {
            try
            {
                var allProducts = await _productDetailCollection
                    .Find(_ => true)
                    .ToListAsync();

                // In ra console để kiểm tra
                foreach (var product in allProducts)
                {
                    Console.WriteLine($"Product in DB: {product.name}");
                }

                return allProducts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products: {ex.Message}");
                return new List<ProductMongoDbModel>();
            }
        }

        public async Task<List<ProductMongoDbModel>> Listing(string keyword = "", int group_id = -1,int status=-1, int page_index = 1, int page_size = 10,bool export_all=false)
        {
            try
            {
                keyword = Regex.Escape(keyword.Trim());

                var regex = new MongoDB.Bson.BsonRegularExpression(keyword.Trim(), "i");

                var filter = Builders<ProductMongoDbModel>.Filter.Or(
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.name, regex),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.sku, regex),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.code, regex)

                                    );
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                filter &= Builders<ProductMongoDbModel>.Filter.Ne(s => s.status ,(int)ProductStatus.REMOVE);
                if (group_id > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, new BsonRegularExpression($@"\b{group_id}\b"));

                }
                if (status > 0)
                {
                    switch (status)
                    {
                        case (int)ProductStatus.ON_WAITING_CONFIRM:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.status, (int)ProductStatus.ON_WAITING_CONFIRM),
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.supplier_status, (int)SUPPLIER_STATUS.ON_WAITING_CONFIRMATION)
                                );
                            }
                            break;
                        case (int)ProductStatus.ACTIVE:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.And(
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.status, (int)ProductStatus.ACTIVE),
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.supplier_status, (int)SUPPLIER_STATUS.CONFIRMED)
                                );
                            }
                            break;
                        case (int)ProductStatus.DEACTIVE:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.status, (int)ProductStatus.DEACTIVE),
                                   Builders<ProductMongoDbModel>.Filter.Ne(p => p.supplier_status, (int)SUPPLIER_STATUS.CONFIRMED)
                                );
                            }
                            break;
                        default:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, status);
                            }
                            break;
                    }
                   
                }
                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filter).Sort(sort_filter_definition);
                if (page_size > 0 && page_index > 0)
                {
                    model.Options.Skip = page_index < 1 ? 0 : (page_index - 1) * page_size;
                    model.Options.Limit = page_size;
                }
                else if(export_all==false) 
                {
                
                    model.Options.Skip = 0;
                    model.Options.Limit = 10;
                }
                //// Retrieve products from MongoDB
                //var result1 = await _productDetailCollection.Find(filterDefinition).Sort(sort_filter_definition).ToListAsync();

                //// Log each product's name to confirm normalization
                //foreach (var product in result1)
                //{
                //    Console.WriteLine("Product in DB: " + product.name);
                //}
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - Listing Error: " + ex);
                return null;
            }
        }
        public async Task<long> CountListing(string keyword = "", int group_id = -1, int status = -1)
        {
            try
            {
                keyword = Regex.Escape(keyword.Trim());

                var regex = new MongoDB.Bson.BsonRegularExpression(keyword.Trim(), "i");

                var filter = Builders<ProductMongoDbModel>.Filter.Or(
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.name, regex),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.sku, regex),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.code, regex)

                                    );
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                filter &= Builders<ProductMongoDbModel>.Filter.Ne(s => s.status, (int)ProductStatus.REMOVE);
                if (group_id > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, new BsonRegularExpression($@"\b{group_id}\b"));

                }
                if (status > 0)
                {
                    switch (status)
                    {
                        case (int)ProductStatus.ON_WAITING_CONFIRM:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.status, (int)ProductStatus.ON_WAITING_CONFIRM),
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.supplier_status, (int)SUPPLIER_STATUS.ON_WAITING_CONFIRMATION)
                                );
                            }
                            break;
                        case (int)ProductStatus.ACTIVE:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.And(
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.status, (int)ProductStatus.ACTIVE),
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.supplier_status, (int)SUPPLIER_STATUS.CONFIRMED)
                                );
                            }
                            break;
                        case (int)ProductStatus.DEACTIVE:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                                   Builders<ProductMongoDbModel>.Filter.Eq(p => p.status, (int)ProductStatus.DEACTIVE),
                                   Builders<ProductMongoDbModel>.Filter.Ne(p => p.supplier_status, (int)SUPPLIER_STATUS.CONFIRMED)
                                );
                            }
                            break;
                        default:
                            {
                                filter &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, status);
                            }
                            break;
                    }

                }
                var model = await _productDetailCollection.CountDocumentsAsync(filter);
                
                return model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - Listing Error: " + ex);
                return 0;
            }
        }

        // Hàm chuẩn hóa từ khóa tìm kiếm, giữ lại dấu ngoặc và các ký tự cần thiết
        private string NormalizeTextForSearch(string input)
        {
            return input
                .Normalize(NormalizationForm.FormC)
                .ToLower()
                .Trim();
        }
        public async Task<List<ProductMongoDbModel>> ListSubListing(List<string> parents_id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.In(x => x.parent_product_id, parents_id);
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, (int)ProductStatus.ACTIVE); ;

                var model = _productDetailCollection.Find(filterDefinition);
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - SubListing Error: " + ex);
                return null;
            }
        }
        public async Task<List<ProductMongoDbModel>> SubListing(string parent_id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.parent_product_id, parent_id);
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.In(x => x.status, status_sub);

                var model = _productDetailCollection.Find(filterDefinition);
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - SubListing Error: " + ex);
                return null;
            }
        }
        public async Task<List<ProductMongoDbModel>> SubListing(IEnumerable<string> parent_id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.In(x => x.status, status_sub);
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.In(x => x.parent_product_id, parent_id);

                var model = _productDetailCollection.Find(filterDefinition);
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - SubListing Error: " + ex);
                return null;
            }
        }

        public async Task<string> DeactiveByParentId(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.parent_product_id, id);
                var update = Builders<ProductMongoDbModel>.Update.Set(x => x.status, (int)ProductStatus.DEACTIVE);

                var updated_item = await _productDetailCollection.UpdateManyAsync(filterDefinition, update);
                return id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - DeactiveByParentId Error: " + ex);
            }
            return null;

        }
        public async Task<string> RemoveSubProductByParentId(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.parent_product_id, id);
                var update = Builders<ProductMongoDbModel>.Update.Set(x => x.status, (int)ProductStatus.REMOVE);

                var updated_item = await _productDetailCollection.UpdateManyAsync(filterDefinition, update);
                return id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - DeactiveByParentId Error: " + ex);
            }
            return null;

        }
        public async Task<List<ProductMongoDbModel>> GetListByIds(string ids)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Where(x => ids.Contains(x._id));

                var model = _productDetailCollection.Find(filterDefinition);
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - GetListByIds Error: " + ex);
            }
            return null;

        }

        public async Task<string> Delete(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x._id, id);

                var updated_item = await _productDetailCollection.DeleteOneAsync(filterDefinition);
                return id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - DeactiveByParentId Error: " + ex);
            }
            return null;

        }
        public async Task<string> DeleteByParentId(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.parent_product_id, id);

                var updated_item = await _productDetailCollection.DeleteManyAsync(filterDefinition);
                return id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - DeactiveByParentId Error: " + ex);
            }
            return null;

        }
        public async Task<string> DeleteInactiveByParentId(string id)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.parent_product_id, id);
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.status, (int)ProductStatus.DEACTIVE);

                var updated_item = await _productDetailCollection.DeleteManyAsync(filterDefinition);
                return id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - DeactiveByParentId Error: " + ex);
            }
            return null;

        }
        public async Task<ProductMongoDbModel> GetByNameAndSKU(string name, string sku)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.name, name); ;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.sku, sku); ;
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("GetByNameAndSKU - GetByID Error: " + ex);
                return null;
            }
        }
        public async Task UpdateProductAndChildrenStatus(string productId, int productStatus)
        {
            try
            {
                // Tạo filter để tìm sản phẩm có _id trùng với productId
                var parentFilter = Builders<ProductMongoDbModel>.Filter.Eq(p => p._id, productId);

                // Tạo update definition để cập nhật status
                var updateDefinition = Builders<ProductMongoDbModel>.Update.Set(p => p.status, productStatus);

                // Cập nhật trạng thái của sản phẩm cha
                var updateParentResult = await _productDetailCollection.UpdateOneAsync(parentFilter, updateDefinition);


                // Tạo filter để tìm tất cả sản phẩm có parent_product_id trùng với productId
                var childrenFilter = Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, productId);
                childrenFilter &= Builders<ProductMongoDbModel>.Filter.In(p => p.status, status_sub);
                // Cập nhật trạng thái của tất cả sản phẩm con
                var updateChildrenResult = await _productDetailCollection.UpdateManyAsync(childrenFilter, updateDefinition);
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - UpdateProductAndChildrenStatus Error: " + ex);
            }
        }
        public async Task<bool> UpdateStatusBySupplierId(int supplierId, int newStatus)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                // Create a filter to match documents by supplier_id
                var filterDefinition = filter.Eq(x => x.supplier_id, supplierId);
                var update = Builders<ProductMongoDbModel>.Update;
                // Create an update definition to set the new status
                var updateDefinition = update.Set(x => x.supplier_status, newStatus);

                // Execute the update operation for multiple documents
                var result = await _productDetailCollection.UpdateManyAsync(filterDefinition, updateDefinition);

                // Check if any documents were modified
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // Log the error using your utility helper
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - UpdateStatusBySupplierId Error: " + ex);
                return false;
            }
        }
        public async Task<List<ProductMongoDbModel>> ListByProducts(List<string> ids)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoDbModel>.Filter.In(x => x._id, ids);

                var model = _productDetailCollection.Find(filterDefinition);
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - SubListing Error: " + ex);
                return null;
            }
        }
        public async Task<List<ProductMongoDbModel>> ListingProductBuyWith(string keyword = "", int group_id = -1, List<string>? current_id = null, bool main_product_requirement = false)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter.Or(
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.name, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.sku, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.code, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i"))

                                    );

                filter &= Builders<ProductMongoDbModel>.Filter.Eq(s => s.status, (int)ProductStatus.ACTIVE);
                filter &= Builders<ProductMongoDbModel>.Filter.Eq(p => p.supplier_status, (int)SUPPLIER_STATUS.CONFIRMED);
                if (group_id > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, group_id.ToString());
                }
                if (main_product_requirement == false)
                {
                    // Điều kiện cho Trường hợp 1: parent_product_id là null hoặc rỗng, VÀ không có variation_detail
                    var condition1_ParentIdNullOrEmpty = Builders<ProductMongoDbModel>.Filter.Or(
                        Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                        Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                    );

                    // Trường hợp không có variation_detail (null hoặc rỗng)
                    var condition1_NoVariationDetail = Builders<ProductMongoDbModel>.Filter.Or(
                        Builders<ProductMongoDbModel>.Filter.Eq(p => p.variation_detail, null),
                        Builders<ProductMongoDbModel>.Filter.Size(p => p.variation_detail, 0)
                    );
                    // Hoặc cách này cũng hiệu quả và thường được dùng:
                    // var condition1_NoVariationDetail = Builders<ProductMongoDbModel>.Filter.Where(p => p.variation_detail == null || !p.variation_detail.Any());


                    var case1Filter = Builders<ProductMongoDbModel>.Filter.And(
                        condition1_ParentIdNullOrEmpty,
                        condition1_NoVariationDetail
                    );

                    // Điều kiện cho Trường hợp 2: Có parent_product_id VÀ cũng có variation_detail
                    var condition2_HasParentId = Builders<ProductMongoDbModel>.Filter.And(
                        Builders<ProductMongoDbModel>.Filter.Ne(p => p.parent_product_id, null),
                        Builders<ProductMongoDbModel>.Filter.Ne(p => p.parent_product_id, string.Empty)
                    );

                    // Trường hợp có variation_detail (không null và không rỗng)
                    var condition2_HasVariationDetail = Builders<ProductMongoDbModel>.Filter.ElemMatch(p => p.variation_detail, Builders<ProductDetailVariationAttributesMongoDbModel>.Filter.Exists(x => x.name));

                    // Hoặc cách này:
                    // var condition2_HasVariationDetail = Builders<ProductMongoDbModel>.Filter.Where(p => p.variation_detail != null && p.variation_detail.Any());


                    var case2Filter = Builders<ProductMongoDbModel>.Filter.And(
                        condition2_HasParentId,
                        condition2_HasVariationDetail
                    );

                    // Kết hợp hai trường hợp bằng toán tử OR
                    filter &= Builders<ProductMongoDbModel>.Filter.Or(
                        case1Filter,
                        case2Filter
                    );
                }
                else
                {
                    var condition1_ParentIdNullOrEmpty = Builders<ProductMongoDbModel>.Filter.Or(
                        Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                        Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                    );
                    filter &= condition1_ParentIdNullOrEmpty;

                }


                filter &= Builders<ProductMongoDbModel>.Filter.Gt(p => p.amount, 0);
                if (current_id != null && current_id.Count > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Nin(p => p._id, current_id);
                    filter &= Builders<ProductMongoDbModel>.Filter.Nin(p => p.parent_product_id, current_id);

                }
                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filter).Sort(sort_filter_definition);
                model.Options.Skip = 0;
                model.Options.Limit = 10;
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - Listing Error: " + ex);
                return null;
            }
        }

        public async Task<List<ProductMongoDbModel>> ListingProductFlashSale(string keyword = "", int group_id = -1, int supplier_id = -1)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter.Or(
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.name, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.sku, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.code, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i"))

                                    );

                filter &= Builders<ProductMongoDbModel>.Filter.Where(s => s.status != (int)ProductStatus.REMOVE);
                if (group_id > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, group_id.ToString());
                }
                if (supplier_id > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.supplier_id, supplier_id);
                }
                // chỉ lấy sản phẩm chính
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filter).Sort(sort_filter_definition);
                model.Options.Skip = 0;
                model.Options.Limit = 10;
                var result = await model.ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - Listing Error: " + ex);
                return null;
            }
        }
        public async Task<long> CountByGroupId(int group_id)
        {
            try
            {
                var filterDefinition = Builders<ProductMongoDbModel>.Filter;
                var filter = filterDefinition.Empty;
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                filter &= Builders<ProductMongoDbModel>.Filter.Where(s => s.status != (int)ProductStatus.REMOVE);
                if (group_id > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, group_id.ToString());
                }

                return await _productDetailCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - CountByGroupId Error: " + ex);
            }
            return 0;
        }

		public async Task<long> GetCountProducts()
        {
            try
            {
                var filterDefinition = Builders<ProductMongoDbModel>.Filter;
                var filter = filterDefinition.Empty;
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                filter &= Builders<ProductMongoDbModel>.Filter.Where(s => s.status != (int)ProductStatus.REMOVE);


                return await _productDetailCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products: {ex.Message}");
                return 0;
            }
        }
        public async Task<List<ProductMongoDbModel>> GetBySupplierId(int supplier_id)
        {
            try
            {
                var filterDefinition = Builders<ProductMongoDbModel>.Filter;
                var filter = filterDefinition.Empty;
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                filter &= Builders<ProductMongoDbModel>.Filter.Where(s => s.status != (int)ProductStatus.REMOVE);
                filter &= Builders<ProductMongoDbModel>.Filter.Eq(x => x.supplier_id, supplier_id);


                return await _productDetailCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - GetBySupplierId Error: " + ex);
            }
            return null;
        }
        public async Task<ProductMongoDbModel> UpdateProductFlashsale(ProductMongoDbModel item, FlashSale? active_flashsale, FlashSaleProduct? active_flashsale_product)
        {
            try
            {
                if (item == null || item._id == null) return null;

                string product_id_compare = item._id;
                if (item.parent_product_id != null && item.parent_product_id.Trim() != "")
                {
                    product_id_compare = item.parent_product_id;
                }
                if (active_flashsale != null && active_flashsale_product !=null
                    && active_flashsale_product.Id >0 && active_flashsale_product.DiscountValue != null && active_flashsale_product.ValueType != null)
                {
                    double total_discount = 0;

                    double percent = Convert.ToDouble(active_flashsale_product.DiscountValue);
                    //if(item._id== "687dc3dfb1292b3f9b42b9c2")
                    //{
                    //    string console = "Match";
                    //}
                    var amount_product = item.amount;
                    if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
                    {
                        amount_product = (double)item.amount_min;

                    }
                    //double old_price = item.old_price == null || item.old_price <= 0 ? amount_product : (double)item.old_price;
                    //if (old_price <= 0)
                    //{
                    //    old_price = amount_product;
                    //}
                    switch (active_flashsale_product.ValueType)
                    {
                        case 1:
                            total_discount += (amount_product * Convert.ToDouble(percent / 100));
                            break;
                        case 0:
                            total_discount += percent;
                            break;

                        default: break;
                    }
                    total_discount = Math.Round(total_discount, 0);
                    item.exists_flashsale_id = active_flashsale.Id;
                    item.flash_sale_fromdate = active_flashsale.FromDate;
                    item.flash_sale_todate = active_flashsale.ToDate;
                    item.exists_flashsale_name = active_flashsale.Name;
                    item.amount_after_flashsale = amount_product - total_discount;
                    //item.profit -= total_discount;
                    if (item.amount <= 0 && item.amount_min != null && item.amount_min > 0)
                    {
                      item.flash_sale_amount_min=  item.amount_min - total_discount;
                        item.flash_sale_amount_min  = Math.Ceiling((double)item.flash_sale_amount_min);

                    }
                    if (item.amount <= 0 && item.amount_max != null && item.amount_max > 0)
                    {
                        item.flash_sale_amount_max = item.amount_max - total_discount;
                        item.flash_sale_amount_max = Math.Ceiling((double)item.flash_sale_amount_max);

                    }
                    item.flash_sale_discount = Math.Round(((amount_product - (double)item.amount_after_flashsale) / amount_product * 100), 0);
                    item.flash_sale_discount = item.flash_sale_discount <= 0 ? 0 : item.flash_sale_discount;
                    // item.price = amount_product- item.profit;
                    item.flash_sale_old_price = amount_product;
                    item.amount_after_flashsale = Math.Ceiling((double)item.amount_after_flashsale);
                    //item.profit = NumberHelpers.RoundUpToHundredsDouble((double)item.profit);
                    //item.flashsale_badge_type = exists_flash_sale_product.badgetype;
                    //has_badge = true;
                    item.flash_sale_unit = active_flashsale_product.ValueType;
                    item.flash_sale_price_sales = Convert.ToDecimal(active_flashsale_product.DiscountValue);
                    await UpdateAsync(item);
                    return item;
                }
                else
                {
                    item.flash_sale_amount_max = null;
                    item.flash_sale_amount_max = null;
                    item.flash_sale_discount = null;
                    item.exists_flashsale_id = null;
                    item.exists_flashsale_name = null;
                    item.amount_after_flashsale = null;
                    item.flash_sale_discount = null;
                    item.flash_sale_fromdate = null;
                    item.flash_sale_todate = null;
                    item.flash_sale_price_sales = null;
                    item.flash_sale_unit = null;
                    item.flash_sale_old_price = null;
                }
                //var base_product = await _productDetailMongoAccess.GetByID(item._id);
                //if (base_product != null && base_product._id != null)
                //{
                //    item = JsonConvert.DeserializeObject<ProductMongoDbModelFEResponse>(JsonConvert.SerializeObject(base_product));

                //}
                //if (!has_badge) {
                //    if (group_types != null && group_types.Count>0 && item.group_product_id!=null && item.group_product_id.Trim()!="" ) {
                //        try
                //        {
                //            var exists = group_types.FirstOrDefault(x => x.Id == Convert.ToInt32(item.group_product_id.Trim().Split(",")[0]));
                //            if (exists != null && exists.Id>0) { item.flashsale_badge_type = exists.Id; }
                //        }
                //        catch { }

                //    }
                //}
            }
            catch (Exception ex)
            {
                string error_msg = Assembly.GetExecutingAssembly().GetName().Name + "->" + MethodBase.GetCurrentMethod().Name + "=>" + ex.ToString();
                LogHelper.InsertLogTelegramByUrl(_configuration["BotSetting:bot_token"], _configuration["BotSetting:bot_group_id"], error_msg);
            }
            return item;
        }

    }
}
