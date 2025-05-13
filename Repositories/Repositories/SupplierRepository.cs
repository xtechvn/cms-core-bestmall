using Aspose.Cells;
using DAL;
using DAL.Funding;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.SupplierConfig;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using Repositories.Repositories.BaseRepos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {
        private readonly BankingAccountDAL bankingAccountDAL;
        private readonly SupplierDAL supplierDAL;
        private readonly AllCodeDAL allCodeDAL;

        private readonly string _UrlStaticImage;

        public SupplierRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig,
            IOptions<DomainConfig> domainConfig, IUserRepository userRepository, IConfiguration configuration) : base(context, dataBaseConfig, configuration, userRepository)
        {
            supplierDAL = new SupplierDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            allCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            bankingAccountDAL = new BankingAccountDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _UrlStaticImage = domainConfig.Value.ImageStatic;
        }

        public async Task<string> ExportSuppliers(SupplierSearchModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                searchModel.currentPage = -1;
                var suppliers = GetSuppliers(searchModel);

                if (suppliers != null && suppliers.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách nhà cung cấp";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 12);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 50);
                    cell.SetColumnWidth(3, 50);
                    cell.SetColumnWidth(4, 50);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 40);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
           

                    // Set header value
                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Mã ");
                    ws.Cells["C1"].PutValue("Tên nhà cung cấp");
                    ws.Cells["D1"].PutValue("Liên hệ");
                    ws.Cells["E1"].PutValue("Địa chỉ");
                    ws.Cells["F1"].PutValue("Dịch vụ");
                    ws.Cells["G1"].PutValue("Người tạo");
                    ws.Cells["H1"].PutValue("Ngày tạo");
                 
                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, suppliers.Count, 12);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["J2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    foreach (var item in suppliers)
                    {
                        RowIndex++;
                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 1);
                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["B" + RowIndex].PutValue(item.SupplierId);
                        ws.Cells["C" + RowIndex].PutValue(item.FullName);
                        ws.Cells["D" + RowIndex].PutValue("SĐT: "+ item.Phone + "Email: "+ item.Email);
                        ws.Cells["E" + RowIndex].PutValue(item.Address);
                        ws.Cells["F" + RowIndex].PutValue("N/A");
                        ws.Cells["G" + RowIndex].PutValue(item.CreatedName);
                        ws.Cells["H" + RowIndex].PutValue(item.CreatedDate.HasValue && item.CreatedDate != DateTime.MinValue ? item.CreatedDate.Value.ToString("dd-MM-yyyy HH:mm") : string.Empty);

                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportPaymentRequest - SupplierRepository: " + ex);
            }
            return pathResult;
        }

        public SupplierViewModel GetById(int supplierId)
        {
            var detail = supplierDAL.GetById(supplierId);
            SupplierViewModel supplierViewModel = new SupplierViewModel();
            detail.CopyProperties(supplierViewModel);
            return supplierViewModel;
        }

        public Supplier GetSuplierById(long supplierId)
        {
            var detail = supplierDAL.GetById(supplierId);

            return detail;
        }

        public List<SupplierViewModel> GetSuppliers(SupplierSearchModel searchModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchModel.FullName))
                {
                    searchModel.FullName = searchModel.FullName.ToLower();
                }

                var listSuppliers = supplierDAL.GetPagingList(searchModel);
                return listSuppliers.ToList<SupplierViewModel>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSuppliers - SupplierRepository: " + ex);
                return new List<SupplierViewModel>();
            }
        }

        public int Add(SupplierConfigUpsertModel model)
        {
            try
            {
                var exist_supplier_name = supplierDAL.CheckExistName(model.SupplierId, model.FullName);
                if (exist_supplier_name != null)
                {
                    //throw new Exception($"Tên nhà cung cấp [{exist_supplier_name.FullName}] đã tồn tại trên hệ thống. Vui lòng kiểm tra lại");
                    return -1;
                }

                model.CreatedBy = _SysUserModel.Id;

                var supplier_id = supplierDAL.CreateSupplier(model);

                if (supplier_id > 0)
                {
                    if (model.ContactName != null && model.ContactPhone != null && model.ContactEmail != null)
                        supplierDAL.InsertSupplierContact(new SupplierContactViewModel
                        {
                            Name = model.ContactName,
                            Email = model.ContactEmail,
                            Position = model.ContactPosition,
                            SupplierId = supplier_id,
                            Mobile = model.ContactPhone,
                            CreatedBy = _SysUserModel.Id,
                        });
                    if (model.BankId != null && model.BankId.Trim() != "")
                    {
                        bankingAccountDAL.InsertBankingAccount(new BankingAccount
                        {
                            BankId = model.BankId,
                            AccountName = model.BankAccountName,
                            AccountNumber = model.BankAccountNumber,
                            Branch = model.BankBranch,
                            CreatedBy = _SysUserModel.Id,
                            SupplierId = supplier_id,
                        });
                    }

                }
                return supplier_id;
            }
            catch
            {
                throw;
            }
        }

        public int Update(SupplierConfigUpsertModel model)
        {
            try
            {
                var exist_supplier_name = supplierDAL.CheckExistName(model.SupplierId, model.FullName);
                if (exist_supplier_name != null)
                {
                    throw new Exception($"Tên nhà cung cấp [{exist_supplier_name.FullName}] đã tồn tại trên hệ thống. Vui lòng kiểm tra lại");
                }

                model.UpdatedBy = _SysUserModel.Id;

                return supplierDAL.UpdateSupplier(model);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Supplier>> GetSuggestionList(string name)
        {
            List<Supplier> data = new List<Supplier>();
            try
            {
                var suppliers = await supplierDAL.GetAllAsync();
                data = suppliers;
                if (!string.IsNullOrEmpty(name))
                {
                    data = suppliers.Where(s =>
                    s.FullName.Trim().ToLower().Contains(name.Trim().ToLower())
                    || (!string.IsNullOrEmpty(s.ShortName) && s.ShortName.Trim().ToLower().Contains(name.Trim().ToLower()))
                    || (!string.IsNullOrEmpty(s.Email) && s.Email.Trim().ToLower().Contains(name.Trim().ToLower()))
                    || (!string.IsNullOrEmpty(s.Phone) && s.Phone.Trim().ToLower().Contains(name.Trim().ToLower()))
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSuggestionList - SupplierRepository: " + ex);
                data = new List<Supplier>();
            }
            return data;
        }

        public SupplierDetailViewModel GetDetailById(int supplierId)
        {
            try
            {
                var dataTable = supplierDAL.GetDetailById(supplierId);

                return dataTable.ToList<SupplierDetailViewModel>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }


        public IEnumerable<Supplier> GetSuggestSupplier(string text, int limit)
        {
            try
            {
                var dataTable = supplierDAL.GetSuggestSupplier(text, limit);
                return dataTable.ToList<Supplier>();
            }
            catch
            {
                throw;
            }
        }

        public IEnumerable<Supplier> GetSuggestSupplierForHotel(int hotel_id, string text, int limit)
        {
            try
            {
                var dataTable = supplierDAL.GetSuggestSupplierOfHotel(hotel_id, text, limit);
                return dataTable.ToList<Supplier>();
            }
            catch
            {
                throw;
            }
        }

        public int GetByIDOrName(int suplier_id, string name)
        {
            try
            {
                var data = supplierDAL.CheckExistName(suplier_id, name);

                if (data != null && data.SupplierId > 0)
                {
                    return data.SupplierId;
                }
            }
            catch
            {
            }
            return -1;

        }

        #region contact
        public IEnumerable<SupplierContactViewModel> GetSupplierContactList(int supplier_id)
        {
            try
            {
                var dataTable = supplierDAL.GetSupplierContactDataTable(supplier_id);
                return dataTable.ToList<SupplierContactViewModel>();
            }
            catch
            {
                throw;
            }
        }

        public SupplierContact GetSupplierContactById(int Id)
        {
            try
            {
                return supplierDAL.GetSupplierContactById(Id);
            }
            catch
            {
                throw;
            }
        }

        public int UpsertSupplierContact(SupplierContact model)
        {
            try
            {
                model.CreatedBy = _SysUserModel.Id;
                model.UpdatedBy = _SysUserModel.Id;
                if (model.Id > 0)
                {
                    return supplierDAL.UpdateSupplierContact(model);
                }
                else
                {
                    return supplierDAL.InsertSupplierContact(model);
                }
            }
            catch
            {
                throw;
            }
        }

        public long DeleteSupplierContact(long id)
        {
            try
            {
                return supplierDAL.DeleteSupplierContactById(id);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region payment

        public IEnumerable<SupplierPaymentViewModel> GetSupplierPaymentList(int supplier_id)
        {
            try
            {
                var dataTable = bankingAccountDAL.GetBankAccountDataTableBySupplierId(supplier_id);
                return dataTable.ToList<SupplierPaymentViewModel>();
            }
            catch
            {
                throw;
            }
        }

        public BankingAccount GetSupplierPaymentById(int Id)
        {
            try
            {
                return bankingAccountDAL.GetById(Id);
            }
            catch
            {
                throw;
            }
        }

        public int UpsertSupplierPayment(BankingAccount model)
        {
            try
            {
                model.CreatedBy = _SysUserModel.Id;
                model.UpdatedBy = _SysUserModel.Id;

                if (model.Id > 0)
                {
                    return bankingAccountDAL.UpdateBankingAccount(model);
                }
                else
                {
                    return bankingAccountDAL.InsertBankingAccount(model);
                }
            }
            catch
            {
                throw;
            }
        }

        public int DeleteSupplierPayment(int id)
        {
            try
            {
                bankingAccountDAL.Delete(id);
                return id;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region order
        public GenericViewModel<SupplierOrderGridViewModel> GetSupplierOrderList(SupplierOrderSearchModel model)
        {
            try
            {
                var modelData = new GenericViewModel<SupplierOrderGridViewModel>();

                var dataTable = supplierDAL.GetAllServiceBySupplierId(model.supplier_id);
                var datas = dataTable.ToList<SupplierOrderGridViewModel>();

                modelData.CurrentPage = model.page_index;
                modelData.ListData = datas;
                modelData.PageSize = model.page_size;
                modelData.TotalRecord = datas.Count();
                modelData.TotalPage = (int)Math.Ceiling((double)modelData.TotalRecord / modelData.PageSize);

                return modelData;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region ticket
        public GenericViewModel<SupplierTicketGridViewModel> GetSupplierTicketList(SupplierTicketSearchModel model)
        {
            try
            {
                var modelData = new GenericViewModel<SupplierTicketGridViewModel>();

                var dataTable = supplierDAL.GetListPaymentVoucherBySupplierId(model);
                var datas = dataTable.ToList<SupplierTicketGridViewModel>();

                modelData.CurrentPage = model.page_index;
                modelData.ListData = datas;
                modelData.PageSize = model.page_size;
                modelData.TotalRecord = datas.FirstOrDefault() != null ? datas.First().TotalRow : 0;
                modelData.TotalPage = (int)Math.Ceiling((double)modelData.TotalRecord / modelData.PageSize);

                return modelData;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region service
        public GenericViewModel<SupplierOrderGridViewModel> GetSupplierServiceList(SupplierServiceSearchModel model)
        {
            try
            {
                var modelData = new GenericViewModel<SupplierOrderGridViewModel>();

                var dataTable = supplierDAL.GetAllServiceBySupplierId(model.supplier_id);
                var datas = dataTable.ToList<SupplierOrderGridViewModel>();

                if (datas != null && datas.Any())
                {
                    datas = datas.Where(s => s.ServiceType == model.service_type).ToList();

                    if (!string.IsNullOrEmpty(model.service_name))
                        datas = datas.Where(s => s.ServiceName.Contains(model.service_name)).ToList();

                    modelData.CurrentPage = model.page_index;
                    modelData.ListData = datas;
                    modelData.PageSize = model.page_size;
                    modelData.TotalRecord = datas.Count();
                    modelData.TotalPage = (int)Math.Ceiling((double)modelData.TotalRecord / modelData.PageSize);
                }

                return modelData;
            }
            catch
            {
                throw;
            }
        }

      
       

        #endregion

        #region program
        public GenericViewModel<SupplierProgramGridViewModel> GetSupplierProgramList(SupplierProgramSearchModel model)
        {
            try
            {
                var modelData = new GenericViewModel<SupplierProgramGridViewModel>();

                var dataTable = supplierDAL.GetListPrograms(model);
                var datas = dataTable.ToList<SupplierProgramGridViewModel>();

                if (datas != null && datas.Any())
                {
                    modelData.CurrentPage = model.PageIndex;
                    modelData.ListData = datas;
                    modelData.PageSize = model.PageSize;
                    modelData.TotalRecord = datas.Count();
                    modelData.TotalPage = (int)Math.Ceiling((double)modelData.TotalRecord / modelData.PageSize);
                }
                return modelData;
            }
            catch
            {
                throw;
            }
        }



        #endregion
    }
}
