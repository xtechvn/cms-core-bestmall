using Caching.Elasticsearch;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.Products;
using HuloToys_Service.MongoDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models.MongoDb;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace WEB.BestMall.CMS.Controllers
{
    public class ClientContactController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IAllCodeRepository _allCodeRepository;
        private ClientContactMongodbService clientContactMongodbService;

        public ClientContactController(IConfiguration configuration, IAllCodeRepository allCodeRepository, ClientContactMongodbService _clientContactMongodbService)
        {

            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
            clientContactMongodbService = _clientContactMongodbService;

        }
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string keyword = "", int page_index = 1, int page_size = 10)
        {

            if (page_size <= 0) page_size = 10;
            if (page_index < 1) page_index = 1;
            // Kiểm tra encoding
            var bytes = System.Text.Encoding.UTF8.GetBytes(keyword);
            var normalizedKeyword = keyword.Normalize(NormalizationForm.FormC);
            var main_products = await clientContactMongodbService.GetList (keyword, page_index, page_size);
            ViewBag.Main = main_products;
          
            return View();
        }
    }
}
