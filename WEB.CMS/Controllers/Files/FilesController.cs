using HuloToys_Front_End.Controllers.Files.Bussiness;
using Microsoft.AspNetCore.Mvc;
using Utilities.Common;

namespace HuloToys_Front_End.Controllers.Files
{
    public class FilesController : Controller
    {
        private StaticAPIService _staticAPIService;
        private readonly IConfiguration _configuration;

        public FilesController(IConfiguration configuration)
        {
            _staticAPIService = new StaticAPIService(configuration);
            _configuration=configuration;
        }
        public async Task<IActionResult> SummitImages(string data_image)
        {
            try
            {
                if (
                    data_image == null || data_image.Trim() == ""
                    )
                {
                    return Ok(new
                    {
                        is_success = false,

                    });
                }
                var data_img = _staticAPIService.GetImageSrcBase64Object(data_image);
                if (data_img != null)
                {
                    var url = await _staticAPIService.UploadImageBase64(data_img);
                    return Ok(new
                    {
                        is_success = true,
                        data = url
                    });
                }

            }
            catch (Exception ex)
            {

            }
            return Ok(new
            {
                is_success = false,
            });
        }
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 100*1024*1024)]
        public async Task<IActionResult> SummitVideo(IFormFile request)
        {
            try
            {
                string url = await _staticAPIService.UploadVideo(request);
                return Ok(new
                {
                    is_success = (url!=null && url.Trim()!=""),
                    data = url
                });
            }
            catch (Exception ex)
            {

            }
            return Ok(new
            {
                is_success = false,
                data=""
            });
        }

        [HttpPost]
        public async Task<IActionResult> SummitImages(string data_image, int width = -1, int height = -1)
        {
            try
            {
                if (
                    data_image == null || data_image.Trim() == ""
                    )
                {
                    return Ok(new
                    {
                        is_success = false,

                    });
                }
                try
                {
                    if (width > 20 && height > 20)
                    {
                        var resized = ImageResizerLegacy.ResizeImageBase64Legacy(data_image, width, height);
                        var base64Data = data_image.Split(',')[0];
                        if (resized != null && resized.Trim() != "") data_image = base64Data + "," + resized;
                    }
                }
                catch { }
                var data_img = _staticAPIService.GetImageSrcBase64Object(data_image);
                if (data_img != null)
                {
                    var url = await _staticAPIService.UploadImageBase64(data_img);
                    return Ok(new
                    {
                        is_success = true,
                        data = url
                    });
                }

            }
            catch (Exception ex)
            {

            }
            return Ok(new
            {
                is_success = false,
            });
        }
    }
}
