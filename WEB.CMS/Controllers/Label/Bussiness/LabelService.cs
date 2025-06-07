using Utilities.Contants;
using Utilities.Contants.ProductV2;
using WEB.Adavigo.CMS.Service;
using WEB.CMS.Models.Product;

namespace WEB.CMS.Controllers.Bussiness
{
    public class LabelService
    {
        private readonly IConfiguration _configuration;
        private readonly ProductDetailMongoAccess _productV2DetailMongoAccess;
        private readonly StaticAPIService staticAPIService;

        public LabelService(IConfiguration configuration) {
            _configuration=configuration;
            _productV2DetailMongoAccess=new ProductDetailMongoAccess(configuration);
            staticAPIService = new StaticAPIService(configuration);
        }
        public async Task<string> UploadLabelImage(string imagePath)
        {
            string full_path = Directory.GetCurrentDirectory() + "\\wwwroot\\" + imagePath.Replace("/", "\\");
            string url = imagePath;
            try
            {
                // Đọc toàn bộ nội dung của file ảnh dưới dạng byte array
                byte[] imageBytes = System.IO.File.ReadAllBytes(full_path);

                // Chuyển đổi byte array thành chuỗi Base64
                string base64String = Convert.ToBase64String(imageBytes);

                var path = imagePath.Split(".");

                Utilities.ViewModels.Article.ImageBase64 image = new()
                {
                    ImageData = base64String,
                    ImageExtension = path[path.Length - 1]
                };
                url = await staticAPIService.UploadImageBase64(image);
                
            }
            catch
            {

            }
            try { System.IO.File.Delete(full_path); } catch { }
            return url;

        }
    }
}
