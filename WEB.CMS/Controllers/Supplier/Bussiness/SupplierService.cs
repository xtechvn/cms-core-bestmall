using Caching.Elasticsearch;
using Caching.Elasticsearch.FlashSale;
using Microsoft.AspNetCore.Mvc;
using Utilities;
using Utilities.Contants;
using Utilities.Contants.ProductV2;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers.Bussiness
{
    public class SupplierService
    {
        private readonly IConfiguration _configuration;
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly ProductESRepository _productESRepository;

        public SupplierService(IConfiguration configuration, ProductDetailMongoAccess productV2DetailMongoAccess, ProductESRepository productESRepository) {
            _configuration=configuration;
            _productV2DetailMongoAccess= productV2DetailMongoAccess;
            _productESRepository= productESRepository;
        }
        public async Task<bool> UpdateSuplierAllProductStatus(int supplier_id,int supplier_status)
        {
            try
            {
                await _productV2DetailMongoAccess.UpdateStatusBySupplierId(supplier_id, supplier_status);
                return true;
            }
            catch { }
            return false;
        }
        public async Task<bool> SyncES(int supplier_id)
        {
            try
            {
                await _productESRepository.DeleteBySupplier(supplier_id);

                var products = await _productV2DetailMongoAccess.GetBySupplierId(supplier_id);
                if (products != null && products.Count > 0)
                {
                    products = products.Where(x => (x.parent_product_id == null || x.parent_product_id == "") && x.status == (int)ProductStatus.ACTIVE).ToList();
                    products = products.GroupBy(x => x.name).Select(x => x.First()).ToList();
                    foreach (var product in products)
                    {
                        await _productESRepository.DeleteByProductId(product._id);

                        ProductESModel product_es = new ProductESModel()
                        {
                            id = _productESRepository.GenerateId(),
                            amount = product.amount_min == null ? product.amount : (double)product.amount_min,
                            description = product.description,
                            name = product.name,
                            product_code = product.code,
                            product_id = product._id,
                            product_name_no_tv = CommonHelper.RemoveSpecialCharacters(StringHelpers.RemoveUnicode(product.name).ToLower().Replace(" ", "").Trim()),
                            avatar = product.avatar,
                            status = product.status,
                            supplier_status = product.supplier_status,
                            group_id = product.group_product_id
                        };
                        await _productESRepository.InsertAsync(product_es);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SyncES - ProductController: " + ex.ToString());
                return false;
            }
            return true;
        }

    }
}
