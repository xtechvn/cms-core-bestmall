using Utilities.Contants;
using Utilities.Contants.ProductV2;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers.Bussiness
{
    public class SupplierService
    {
        private readonly IConfiguration _configuration;
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;

        public SupplierService(IConfiguration configuration, ProductDetailMongoAccess productV2DetailMongoAccess) {
            _configuration=configuration;
            _productV2DetailMongoAccess= productV2DetailMongoAccess;
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
    }
}
