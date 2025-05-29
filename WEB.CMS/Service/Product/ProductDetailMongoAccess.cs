using Azure.Core;
using Entities.ViewModels.Products;
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
        List<int> status_sub = new List<int>() { (int)ProductStatus.ACTIVE,(int)ProductStatus.DEACTIVE, (int)ProductStatus.ON_WAITING_CONFIRM };

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

        public async Task<List<ProductMongoDbModel>> Listing(string keyword = "", int group_id = -1, int page_index = 1, int page_size = 10)
        {
            try
            {
                var filter = Builders<ProductMongoDbModel>.Filter.Or(
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.name, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.sku, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i")),
                                    Builders<ProductMongoDbModel>.Filter.Regex(p => p.code, new MongoDB.Bson.BsonRegularExpression(keyword.Trim().ToLower(), "i"))

                                    );
                filter &= Builders<ProductMongoDbModel>.Filter.Or(
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, null),
                    Builders<ProductMongoDbModel>.Filter.Eq(p => p.parent_product_id, "")
                );
                filter &= Builders<ProductMongoDbModel>.Filter.Where(s => s.status != (int)ProductStatus.REMOVE);
                if (group_id > 0)
                {
                    filter &= Builders<ProductMongoDbModel>.Filter.Regex(x => x.group_product_id, group_id.ToString());
                }
                var sort_filter = Builders<ProductMongoDbModel>.Sort;
                var sort_filter_definition = sort_filter.Descending(x => x.updated_last);
                var model = _productDetailCollection.Find(filter).Sort(sort_filter_definition);
                model.Options.Skip = page_index < 1 ? 0 : (page_index - 1) * page_size;
                model.Options.Limit = page_size;
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
        public async Task<string> RemoveSubProductByParentId (string id)
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
    }
}
