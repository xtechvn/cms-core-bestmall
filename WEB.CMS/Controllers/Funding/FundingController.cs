using Caching.Elasticsearch;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.BestMall.CMS.Controllers
{
    [CustomAuthorize]

    public class FundingController : Controller
    {
        private readonly IDepositHistoryRepository _depositHistoryRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IContractPayRepository _contractPayRepository;
        private ManagementUser _ManagementUser;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly ClientESRepository _clientESRepository;
        private readonly IConfiguration _configuration;

        public FundingController(IDepositHistoryRepository depositHistoryRepository, IAllCodeRepository allCodeRepository, ManagementUser ManagementUser,
            IClientRepository clientRepository, IContractPayRepository contractPayRepository, IWebHostEnvironment hostEnvironment, IUserRepository userRepository, 
            IConfiguration configuration, ClientESRepository clientESRepository)
        {
            _depositHistoryRepository = depositHistoryRepository;
            _allCodeRepository = allCodeRepository;
            _clientRepository = clientRepository;
            _contractPayRepository = contractPayRepository;
            _WebHostEnvironment = hostEnvironment;
            _ManagementUser = ManagementUser;
            _userRepository = userRepository;
            _configuration = configuration;
            _clientESRepository = clientESRepository;

        }

        public IActionResult Index()
        {
            ViewBag.allCode_SERVICE_TYPE = _allCodeRepository.GetListByType(AllCodeType.SERVICE_TYPE);
            ViewBag.allCode_PAYMENT_TYPE = _allCodeRepository.GetListByType(AllCodeType.PAYMENT_TYPE);
            ViewBag.allCode_DEPOSITHISTORY_TYPE = _allCodeRepository.GetListByType(AllCodeType.DEPOSITHISTORY_TYPE);
            ViewBag.allCode_DEPOSIT_STATUS = _allCodeRepository.GetListByType(AllCodeType.DEPOSIT_STATUS);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(FundingSearch searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<DepositFunding>();
            try
            {
                var fundingListOutput = _depositHistoryRepository.GetDepositHistories(searchModel, out long total,
                    out List<CountStatus> countStatus, currentPage, pageSize);
                model.CurrentPage = currentPage;
                model.ListData = fundingListOutput;
                model.PageSize = pageSize;
                model.TotalRecord = total;
                model.TotalPage = (int)Math.Ceiling((double)total / pageSize);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - FundingController: " + ex);
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> CountStatus(FundingSearch searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<DepositFunding>();
            try
            {
                var fundingListOutput = _depositHistoryRepository.GetDepositHistories(searchModel, out long total,
                    out List<CountStatus> countStatus, currentPage, pageSize);
                return Ok(new
                {
                    isSuccess = false,
                    data = countStatus
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountStatus - FundingController: " + ex);
                return Ok(new
                {
                    isSuccess = false,
                    data = new List<CountStatus>()
                });
            }
        }

        public async Task<IActionResult> Detail(int depositHistotyId)
        {
            DepositFunding depositFunding = _depositHistoryRepository.GetById(depositHistotyId);
            var current_user = _ManagementUser.GetCurrentUser();
            ViewBag.DUYET_NAP_QUY = 0;
            if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
            {
                var listRole = Array.ConvertAll(current_user.Role.Split(','), int.Parse);
                foreach (var item in listRole)
                {
                    //kiểm tra chức năng có đc phép sử dụng
                    var checkRolePermission = await _userRepository.CheckRolePermissionByUserAndRole(current_user.Id, item,
                        (int)SortOrder.DUYET, (int)MenuId.NAP_QUY);
                    if (checkRolePermission == true)
                    {
                        ViewBag.DUYET_NAP_QUY = 1;
                        break;
                    }
                }
            }
            return View(depositFunding);
        }

        public IActionResult DetailByNo(string depositNo)
        {
            DepositFunding depositFunding = _depositHistoryRepository.GetByNo(depositNo);
            return View(depositFunding);
        }

        public IActionResult Reject(int depositHistotyId)
        {
            DepositFunding depositFunding = _depositHistoryRepository.GetById(depositHistotyId);
            return PartialView(depositFunding);
        }

        [HttpPost]
        public async Task<IActionResult> RejectDeposit(string transNo, string note)
        {
            try
            {
                if (!string.IsNullOrEmpty(note) && note.Length > 300)
                {
                    return Ok(new
                    {
                        isSuccess = false,
                        message = "Nội dung từ chối không được lớn hơn 300 kí tự"
                    });
                }
                var client = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_TRANSACTION;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_B2B;
                HttpClient httpClient = new HttpClient();
                var userLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var j_param = new Dictionary<string, object>
                {
                     {"trans_no", transNo},
                     {"note", note},// ly do tu choi
                     {"is_verify", "4"}, // 0: dong y, 1 tu choi
                     {"user_verify", userLogin}
                };

                var data = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data, key_token_api);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                var response = await httpClient.PostAsync(apiPrefix, content);
                var result = await response.Content.ReadAsStringAsync();
                var output = JsonConvert.DeserializeObject<OutputAPI>(result);
                if (output.status == 1)
                    return Ok(new
                    {
                        isSuccess = false,
                        message = output.msg
                    });
                return Ok(new
                {
                    isSuccess = true,
                    message = "Từ chối giao dịch thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RejectDeposit - FundingController: " + ex);
                return Ok(new
                {
                    isSuccess = false,
                    message = "Từ chối giao dịch thất bại"
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string transNo, long contractPayId)
        {
            try
            {
                var client = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_TRANSACTION;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_B2B;
                HttpClient httpClient = new HttpClient();
                var userLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var j_param = new Dictionary<string, object>
                {
                     {"trans_no", transNo},
                     {"note", null},
                     {"is_verify", "3"}, // 0: dong y, 1 tu choi
                     {"user_verify", userLogin},
                     {"contract_pay_id", contractPayId},
                };
                var data = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data, key_token_api);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                var response = await httpClient.PostAsync(apiPrefix, content);
                var result = await response.Content.ReadAsStringAsync();
                var output = JsonConvert.DeserializeObject<OutputAPI>(result);
                if (output.status == 1)
                    return Ok(new
                    {
                        isSuccess = false,
                        message = output.msg
                    });
                return Ok(new
                {
                    isSuccess = true,
                    message = "Phê duyệt giao dịch thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Approve - FundingController: " + ex);
                return Ok(new
                {
                    isSuccess = false,
                    message = "Phê duyệt giao dịch thất bại"
                });
            }
        }

        public IActionResult AddContractPay(int depositHistotyId)
        {
            DepositFunding depositFunding = _depositHistoryRepository.GetById(depositHistotyId);
            ViewBag.GetContractPays = _depositHistoryRepository.GetContractPays();
            return PartialView(depositFunding);
        }

        [HttpPost]
        public async Task<IActionResult> AddContractPayJson(int depositId, int contractPayId, double totalAmount)
        {
            try
            {
                var contractPay = _contractPayRepository.GetById(contractPayId);
                var depositHisoty = _depositHistoryRepository.GetById(depositId);
                ContractPayCreateModel contractPayCreateModel = new ContractPayCreateModel();
                var client = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_URL + ReadFile.LoadConfig().API_ADD_CONTRACTPAY;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API_MANUAL;
                HttpClient httpClient = new HttpClient();
                var j_param = new ContractPayCreateModel()
                {
                    export_date = DateTime.Now,
                    total_amount = totalAmount,
                    data = new List<ContractPayCreateModelOrder>()
                    {
                        new ContractPayCreateModelOrder(){ amount=contractPay.Amount, data_id=depositId, note= "Thanh toán nạp quỹ " + depositHisoty.TransNo},
                    }

                };
                var data_product = JsonConvert.SerializeObject(j_param);
                var token = CommonHelper.Encode(data_product, key_token_api);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                var response = await httpClient.PostAsync(apiPrefix, content);
                var result = await response.Content.ReadAsStringAsync();
                var output = JsonConvert.DeserializeObject<OutputAPI>(result);
                if (output.status == 1)
                    return Ok(new
                    {
                        isSuccess = false,
                        message = output.msg
                    });
                return Ok(new
                {
                    isSuccess = true,
                    message = "Thêm phiếu thu thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Approve - FundingController: " + ex);
                return Ok(new
                {
                    isSuccess = false,
                    message = "Thêm phiếu thu thất bại"
                });
            }
        }

        public async Task<string> GetUserCreateSuggest(string name)
        {
            try
            {
                var accountClients = await _clientESRepository.GetClientSuggesstion(name);
                var suggestionlist = accountClients.Select(s => new
                {
                    id = s.Id,
                    name = s.ClientName,
                }).ToList();
                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserCreateSuggest - FundingController: " + ex);
                return null;
            }
        }

        [HttpPost]
        public IActionResult DeleteContactPay(int contractPayId)
        {
            try
            {
                var result = _depositHistoryRepository.DeleteContractPay(contractPayId);
                if (result < 0)
                    return Ok(new
                    {
                        isSuccess = true,
                        message = "Xóa phiếu thu thất bại"
                    });
                return Ok(new
                {
                    isSuccess = true,
                    message = "Xóa phiếu thu thành công"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteContactPay - FundingController: " + ex);
                return Ok(new
                {
                    isSuccess = false,
                    message = "Xóa phiếu thu thất bại"
                });
            }
        }

        [HttpPost]
        public IActionResult ExportExcel(FundingSearch searchModel, int currentPage = 1, int pageSize = 20)
        {
            try
            {
                string _FileName = "Deposit_" + Guid.NewGuid() + ".xlsx";
                string _UploadFolder = @"Template/Export";
                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                if (!Directory.Exists(_UploadDirectory))
                {
                    Directory.CreateDirectory(_UploadDirectory);
                }
                //delete all file in folder before export
                try
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(_UploadDirectory);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
                catch
                {
                }


                string FilePath = Path.Combine(_UploadDirectory, _FileName);

                var rsPath = _depositHistoryRepository.ExportDeposit(searchModel, FilePath);

                if (!string.IsNullOrEmpty(rsPath))
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xuất dữ liệu thành công",
                        path = "/" + _UploadFolder + "/" + _FileName
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xuất dữ liệu thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportExcel - FundingController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

    }
}
