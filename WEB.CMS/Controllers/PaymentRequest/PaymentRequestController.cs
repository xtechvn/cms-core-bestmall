using Caching.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
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
    }
}
