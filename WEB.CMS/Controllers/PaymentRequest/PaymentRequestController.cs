using Caching.Elasticsearch;
using Entities.ViewModels.Funding;
using Entities.ViewModels.SupplierConfig;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Utilities;
using WEB.CMS.Controllers.Elastic.Bussiness;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers.PaymentRequest
{
    [CustomAuthorize]
    public class PaymentRequestController : Controller
    {
        private readonly IAllCodeRepository _allCodeRepository;

        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IClientRepository _clientRepository;

        private readonly IContractPayRepository _contractPayRepository;
        private IIdentifierServiceRepository identifierServiceRepository;
        private readonly IUserRepository _userRepository;
        private ManagementUser _ManagementUser;
        private readonly IOrderRepository _orderRepository;
        private readonly IDepositHistoryRepository _depositHistoryRepository;
        private readonly WEB.CMS.Models.AppSettings config;
        private readonly IConfiguration _configuration;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IBankingAccountRepository _bankingAccountRepository;
        private readonly ClientESRepository clientESRepository;
        private ElasticService _elasticService;

        public PaymentRequestController(IContractPayRepository contractPayRepository, IAllCodeRepository allCodeRepository, IWebHostEnvironment hostEnvironment,
          IClientRepository clientRepository, IDepositHistoryRepository depositHistoryRepository, IOrderRepository orderRepository, ManagementUser ManagementUser,
           IUserRepository userRepository, IIdentifierServiceRepository _identifierServiceRepository, IPaymentRequestRepository paymentRequestRepository,
           IConfiguration configuration, ISupplierRepository supplierRepository, IBankingAccountRepository bankingAccountRepository, ClientESRepository _clientESRepository,
           ElasticService elasticService)
        {

            _WebHostEnvironment = hostEnvironment;
            _clientRepository = clientRepository;
            _allCodeRepository = allCodeRepository;
            _contractPayRepository = contractPayRepository;
            _orderRepository = orderRepository;
            _ManagementUser = ManagementUser;
            _userRepository = userRepository;
            identifierServiceRepository = _identifierServiceRepository;
            _depositHistoryRepository = depositHistoryRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _configuration = configuration;
            _supplierRepository = supplierRepository;
            _bankingAccountRepository = bankingAccountRepository;
            clientESRepository = _clientESRepository;
            config = ReadFile.LoadConfig();
            _elasticService = elasticService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<string> GetSuppliersSuggest(string name)
        {
            try
            {
                var supplierList = await _supplierRepository.GetSuggestionList(name);
                var suggestionlist = supplierList.Select(s => new SupplierViewModel
                {
                    SupplierId=s.SupplierId,
                    FullName= s.FullName,
                    Email = s.Email,
                    Phone = s.Phone,
                }).ToList();
                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSuppliersSuggest - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return null;
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetListBankAccountByClientID(int clientId)
        {
            try
            {
                var listPayment = _bankingAccountRepository.GetBankAccountByClientId(clientId);
                return Ok(new
                {
                    isSuccess = true,
                    message = "Thành công",
                    data = listPayment
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListBankAccountBySupplierId - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Thất bại",
                    data = new List<SupplierPaymentViewModel>()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetListBankAccountBySupplierId(int supplierId)
        {
            try
            {
                var listPayment = _supplierRepository.GetSupplierPaymentList(supplierId);
                return Ok(new
                {
                    isSuccess = true,
                    message = "Thành công",
                    data = listPayment
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListBankAccountBySupplierId - PaymentRequestController: " + ex + ". Đã có lỗi xảy ra");
                return Ok(new
                {
                    isSuccess = false,
                    message = "Thất bại",
                    data = new List<SupplierPaymentViewModel>()
                });
            }
        }
    }
}
