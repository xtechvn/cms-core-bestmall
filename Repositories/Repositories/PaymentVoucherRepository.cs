using Aspose.Cells;
using DAL;
using DAL.Funding;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.TransferSms;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class PaymentVoucherRepository : IPaymentVoucherRepository
    {
        private readonly PaymentVoucherDAL paymentVoucherDAL;
        private readonly PaymentRequestDAL paymentRequestDAL;
        private readonly string _UrlStaticImage;
        private readonly AttachFileDAL attachFileDAL;

        public PaymentVoucherRepository(IOptions<DataBaseConfig> dataBaseConfig, IOptions<DomainConfig> domainConfig)
        {
            paymentVoucherDAL = new PaymentVoucherDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            paymentRequestDAL = new PaymentRequestDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _UrlStaticImage = domainConfig.Value.ImageStatic;
            attachFileDAL = new AttachFileDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public int CreatePaymentVoucher(PaymentVoucherViewModel model, out string msg)
        {
            msg = string.Empty;
            var entity = paymentVoucherDAL.GetByPaymentCode(model.PaymentCode);
            if (entity != null)
                return -2;
            var checkExists = paymentVoucherDAL.CheckExistsPaymentRequest(model.PaymentRequestDetails.Select(n => n.Id).ToList(),
                 StoreProcedureConstant.SP_CheckExistsPaymentVoucherByRequestId);
            if (checkExists != null && checkExists.Count > 0)
            {
                msg = string.Join(',', checkExists.Select(n => n.PaymentCode).ToList());
                return -3;
            }
            if (model.OtherImages != null && model.OtherImages.Count() > 0)
            {
                var arrImage = new List<string>();
                foreach (var image in model.OtherImages)
                {
                    arrImage.Add(UpLoadHelper.UploadBase64Src(image, _UrlStaticImage).Result);
                }
                model.AttachFiles = String.Join(",", arrImage);
            }
            return paymentVoucherDAL.CreatePaymentVoucher(model);
        }

        public string ExportPaymentVouchers(PaymentVoucherViewModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                var paymentVouchers = GetPaymentVouchers(searchModel, out long total, 1, -1);
                if (paymentVouchers != null && paymentVouchers.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách phiếu chi";
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
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 20);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 40);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);

                    // Set header value
                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Mã phiếu");
                    ws.Cells["C1"].PutValue("Loại nghiệp vụ");
                    ws.Cells["D1"].PutValue("Hình thức");
                    ws.Cells["E1"].PutValue("Nhà cung cấp/Khách hàng");
                    ws.Cells["F1"].PutValue("Số tiền");
                    ws.Cells["G1"].PutValue("Phiếu yêu cầu chi liên quan");
                    ws.Cells["H1"].PutValue("Ngày tạo");
                    ws.Cells["I1"].PutValue("Người tạo");
                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, paymentVouchers.Count, 12);
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

                    foreach (var item in paymentVouchers)
                    {
                        RowIndex++;
                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 1);
                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["B" + RowIndex].PutValue(item.PaymentCode);
                        ws.Cells["C" + RowIndex].PutValue(item.PaymentVoucherType);
                        ws.Cells["D" + RowIndex].PutValue(item.PaymentTypeStr);
                        ws.Cells["E" + RowIndex].PutValue(!string.IsNullOrEmpty(item.SupplierName) ?
                            item.SupplierName : item.ClientName);
                        ws.Cells["F" + RowIndex].PutValue(item.Amount.ToString("N0"));
                        ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                        ws.Cells["G" + RowIndex].PutValue(item.PaymentRequestCode);
                        ws.Cells["H" + RowIndex].PutValue(item.CreatedDate.Value.ToString("dd/MM/yyyy"));
                        ws.Cells["I" + RowIndex].PutValue(item.UserName);
                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportPaymentVouchers - PaymentVoucherRepository: " + ex);
            }
            return pathResult;
        }

        public PaymentVoucherViewModel GetDetail(int paymentVoucherId)
        {
            try
            {
                var details = paymentVoucherDAL.GetDetail(paymentVoucherId, StoreProcedureConstant.sp_GetDetailPaymentVoucher)
                    .ToList<PaymentVoucherViewModel>();
                PaymentVoucherViewModel model = details.FirstOrDefault();
                model.PaymentRequestDetails = new List<PaymentRequestViewModel>();
                foreach (var item in details)
                {
                    PaymentRequestViewModel paymentRequest = new PaymentRequestViewModel();
                    item.CopyProperties(paymentRequest);
                    model.PaymentRequestDetails.Add(paymentRequest);
                }
                model.AttachFile = attachFileDAL.GetListByDataID(paymentVoucherId, (int)AttachmentType.Payment_Voucher).Result;
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - PaymentVoucherRepository: " + ex);
                return new PaymentVoucherViewModel();
            }
        }

        public List<PaymentVoucherViewModel> GetPaymentVouchers(PaymentVoucherViewModel searchModel, out long total, int currentPage = 1, int pageSize = 20)
        {
            total = 0;
            try
            {
                var listPaymentRequests = paymentVoucherDAL.GetPagingList(searchModel, currentPage, pageSize,
                StoreProcedureConstant.SP_GetListPaymentVoucher).ToList<PaymentVoucherViewModel>();
                var listPaymentRequestCode = new List<string>();
                foreach (var item in listPaymentRequests)
                {
                    var paymentCodes = item.PaymentRequestCode.Split(",");
                    foreach (var code in paymentCodes)
                    {
                        if (!string.IsNullOrEmpty(code.Trim()))
                            listPaymentRequestCode.Add(code.Trim());
                    }
                }
                var listPaymentRequest = paymentRequestDAL.GetByPaymentCodes(listPaymentRequestCode);
                foreach (var item in listPaymentRequests)
                {
                    item.RelateRequest = new List<PaymentRequest>();
                    var listRequest = item.PaymentRequestCode.Split(",");
                    foreach (var requestCode in listRequest)
                    {
                        var request = listPaymentRequest.FirstOrDefault(n => n.PaymentCode == requestCode.Trim());
                        if (request != null)
                            item.RelateRequest.Add(request);
                    }
                }
                if (listPaymentRequests.FirstOrDefault() != null)
                    total = listPaymentRequests.FirstOrDefault().TotalRow;
                return listPaymentRequests;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPaymentVouchers - PaymentVoucherRepository: " + ex);
                return new List<PaymentVoucherViewModel>();
            }
        }

        public int UpdatePaymentVoucher(PaymentVoucherViewModel model)
        {
            if (model.OtherImages != null && model.OtherImages.Count() > 0)
            {
                var arrImage = new List<string>();
                foreach (var image in model.OtherImages)
                {
                    arrImage.Add(UpLoadHelper.UploadBase64Src(image, _UrlStaticImage).Result);
                }
                model.AttachFiles = String.Join(",", arrImage);
            }
            return paymentVoucherDAL.UpdatePaymentVoucher(model);
        }
        public double GetTotalAmountPaymentVoucherByDate(TransferSmsSearchModel searchModel)
        {
            try
            {
                var data = paymentVoucherDAL.GetTotalAmountPaymentVoucherByDate(searchModel);
                if (data != null)
                {
                    return data[0].Amount;
                }
                else
                {
                    return 0;
                }
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalAmountPaymentVoucherByDate - PaymentVoucherRepository: " + ex);
            }
            return 0;
            
        } 
        public double GetCountAmountPaymentVoucherByDate(TransferSmsSearchModel searchModel)
        {
            try
            {
                var data = paymentVoucherDAL.GetTotalAmountPaymentVoucherByDate(searchModel);
                if (data != null)
                {
                    return data[0].Total;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetCountAmountPaymentVoucherByDate - PaymentVoucherRepository: " + ex);
            }
            return 0;
          
        }
        public List<TransferSmsTotalModel> GetListAmountPaymentVoucherByDate(TransferSmsSearchModel searchModel)
        {
            try
            {
                var data = paymentVoucherDAL.GetTotalAmountPaymentVoucherByDate(searchModel);
                if (data != null)
                {
                    return data;
                }
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetCountAmountPaymentVoucherByDate - PaymentVoucherRepository: " + ex);
            }
            return null;

        }
    }
}
