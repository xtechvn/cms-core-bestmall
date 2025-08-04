using Aspose.Cells;
using DAL;
using DAL.OrderDetail;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.OrderDetail;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAL _OrderDal;
        private readonly ClientDAL _clientDAL;
        private readonly AllCodeDAL allCodeDAL;
        private readonly UserDAL userDAL;
        private readonly OrderDetailDAL _orderDetailDAL;
        private readonly ContractPayDAL contractPayDAL;



        public OrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OrderDal = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            allCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            userDAL = new UserDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _orderDetailDAL = new OrderDetailDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);


        }
        public async Task<GenericViewModel<OrderViewModel>> GetList(OrderViewSearchModel searchModel)
        {
            var model = new GenericViewModel<OrderViewModel>();

            try
            {
                DataTable dt = await _OrderDal.GetPagingList(searchModel, ProcedureConstants.GETALLORDER_SEARCH);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<OrderViewModel>();
                    model.ListData = data;
                    model.CurrentPage = searchModel.PageIndex;
                    model.PageSize = searchModel.pageSize;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - OrderRepository: " + ex);
            }
            return model;
        }
        public async Task<OrderDetailViewModel> GetOrderDetailByOrderId(long OrderId)
        {
            try
            {
                return await _OrderDal.GetDetailOrderByOrderId(OrderId);
                
               
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetailByOrderId - OrderRepository: " + ex);
            }
            return null;
        }
        public async Task<TotalCountSumOrder> GetTotalCountSumOrder(OrderViewSearchModel searchModel)
        {
            var model = new TotalCountSumOrder();
            try
            {
                searchModel.PageIndex = -1;
                DataTable dt = await _OrderDal.GetPagingList(searchModel, ProcedureConstants.GET_TOTALCOUNT_ORDER);
                if (dt != null && dt.Rows.Count > 0)
                {


                    model.Profit = dt.Rows[0]["Profit"].Equals(DBNull.Value) ? 0 : Convert.ToDouble(dt.Rows[0]["Profit"]);
                    model.Amount = dt.Rows[0]["Amount"].Equals(DBNull.Value) ? 0 : Convert.ToDouble(dt.Rows[0]["Amount"]);
                    model.Price = dt.Rows[0]["Price"].Equals(DBNull.Value) ? 0 : Convert.ToDouble(dt.Rows[0]["Price"]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalCountSumOrder in OrderRepository: " + ex);
            }
            return model;
        }
        public async Task<long> UpdateOrder(Order model)
        {
            try
            {
                return await _OrderDal.UpdateOrder(model);
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalCountSumOrder in OrderRepository: " + ex);
            }
            return -1;
        }
        public async Task<List<ListOrderDetailViewModel>> GetListOrderDetail(long orderid)
        {
            try
            {
                return await _orderDetailDAL.GetListOrderDetail(orderid);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalCountSumOrder in OrderRepository: " + ex);
            }
            return null;
        }
        public async Task<Order> GetOrderByOrderNo(string orderNo)
        {
            try
            {
                return _OrderDal.GetByOrderNo(orderNo);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderByOrderNo - OrderRepository: " + ex);
            }
            return null;
        }
        public async Task<List<OrderViewModel>>  GetByClientId(long clientId, int payId = 0, int status = 0)
        {
            try
            {
                var listOrder = new List<OrderViewModel>();
                var listOrderOutput = new List<OrderViewModel>();
                var dt = _OrderDal.GetListOrderByClientId(clientId, ProcedureConstants.SP_GetDetailOrderByClientId, status);
                if (dt != null && dt.Rows.Count > 0)
                {
                    listOrder = dt.ToList<OrderViewModel>();
                    var listContractPayDetail = contractPayDAL.GetByContractDataIds(listOrder.Select(n => Convert.ToInt64(n.OrderId)).ToList());
                    foreach (var item in listOrder)
                    {
                        OrderViewModel orderViewModel = new OrderViewModel();
                        var detail = listContractPayDetail.Where(n => n.DataId != null
                                && n.DataId.Value == Convert.ToInt64(item.OrderId) && n.PayId == payId).FirstOrDefault();
                        var TotalDisarmed = listContractPayDetail.Where(n => n.DataId != null
                                && n.DataId.Value == Convert.ToInt64(item.OrderId)).ToList().Sum(n => n.Amount);
                        item.TotalDisarmed = (double)TotalDisarmed;
                        item.TotalAmount = (double)item.Amount;
                        item.TotalNeedPayment = (double)item.Amount - item.TotalDisarmed;
                        item.CopyProperties(orderViewModel);
                        if (detail != null)
                        {
                            orderViewModel.PayDetailId = detail.Id;
                            orderViewModel.IsChecked = true;
                            orderViewModel.Amount = (double)detail?.Amount;
                            orderViewModel.Payment = (double)detail?.Amount;
                        }

                        if (item.TotalNeedPayment > 0 || (item.Amount == 0 && item.IsFinishPayment == 0))
                        {
                            if (payId <= 0)
                                orderViewModel.Amount = item.TotalNeedPayment;
                            listOrderOutput.Add(orderViewModel);
                        }

                    }
                    if (payId != 0)
                    {
                        var allCode_ORDER_STATUS = allCodeDAL.GetListByType(AllCodeType.ORDER_STATUS);
                        var listOrderId = listOrderOutput.Select(n => Convert.ToInt64(n.OrderId)).ToList();
                        listContractPayDetail = contractPayDAL.GetByContractPayIds(new List<int>() { payId });
                        var listOrderDisable = listContractPayDetail.Where(n => !listOrderId.Contains(n.DataId.Value)).ToList();
                        foreach (var item in listOrderDisable)
                        {
                            OrderViewModel orderViewModel = new OrderViewModel();
                            var order = listOrder.FirstOrDefault(n => Convert.ToInt64(n.OrderId) == item.DataId);
                            if (order != null)
                            {
                                order.CopyProperties(orderViewModel);
                                orderViewModel.Amount = (double)item?.Amount;
                                orderViewModel.Payment = (double)item?.Amount;
                                orderViewModel.TotalDisarmed = (double)order.Amount;
                                orderViewModel.TotalAmount = (double)order.Amount;
                            }
                            else
                            {
                                var orderInfo =await _OrderDal.GetDetailOrderByOrderId(item.DataId.Value);
                                if (orderInfo != null)
                                {
                                    orderViewModel.OrderId = orderInfo.OrderId;
                                    orderViewModel.OrderNo = orderInfo.OrderNo;
                                    orderViewModel.StartDate = orderInfo.StartDate != null ?
                                        orderInfo.StartDate.ToString("dd:MM:yyyy") : string.Empty;
                                    orderViewModel.EndDate = orderInfo.EndDate != null ?
                                        orderInfo.EndDate.ToString("dd:MM:yyyy") : string.Empty;
                                    orderViewModel.Status = allCode_ORDER_STATUS.FirstOrDefault(n => n.CodeValue == orderInfo.OrderStatus)?.Description;
                                    orderViewModel.SalerName = userDAL.GetById(orderInfo.SalerId != null ? orderInfo.SalerId : 0).Result?.FullName;
                                    orderViewModel.Amount = (double)item?.Amount;
                                    orderViewModel.Payment = (double)item?.Amount;
                                    orderViewModel.TotalDisarmed = (double)orderInfo.Amount;
                                    orderViewModel.TotalAmount = (double)orderInfo.Amount;
                                }
                            }
                            orderViewModel.PayDetailId = item.Id;
                            orderViewModel.IsChecked = true;

                            orderViewModel.IsDisabled = true;
                            listOrderOutput.Add(orderViewModel);
                        }
                    }
                }
                return listOrderOutput;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - OrderRepository: " + ex);
            }
            return new List<OrderViewModel>();
        }
        public async Task<Order> GetByOrderId(long order_id)
        {
            try
            {
                return _OrderDal.GetByOrderId(order_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderId - OrderRepository: " + ex);
            }
            return null;
        }
        public async Task<List<Entities.Models.OrderDetail>> GetDetailByOrderId(long order_id)
        {
            try
            {
                return await _orderDetailDAL.GetByOrderId(order_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderId - OrderRepository: " + ex);
            }
            return null;
        }
        public async Task<string> ExportDeposit(OrderViewSearchModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                var data = new List<OrderViewModel>();
                DataTable dt = await _OrderDal.GetPagingList(searchModel, ProcedureConstants.GETALLORDER_SEARCH);
                if (dt != null && dt.Rows.Count > 0)
                {
                    data = dt.ToList<OrderViewModel>();
                }
                else
                {
                    LogHelper.InsertLogTelegram("GetList -  OrderRepository: No Order Count with" + JsonConvert.SerializeObject(searchModel));
                }

                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách đơn hàng";
                    Cells cell = ws.Cells;

                    // Define headers and their corresponding OrderViewModel properties
                    // Tuple: Item1 = Header Text, Item2 = Property Name (for easy access via reflection or switch)
                    var headers = new List<Tuple<string, string>>
            {
                Tuple.Create("ID đơn hàng", "OrderId"),
                Tuple.Create("Mã đơn hàng", "OrderNo"),
                Tuple.Create("Số điện thoại khách hàng", "Phone"),
                Tuple.Create("Trạng thái đơn hàng", "Status"),
                Tuple.Create("Tên khách hàng", "ClientName"),
                Tuple.Create("Email khách hàng", "ClientEmail"),
                Tuple.Create("Giá", "Price"),
                Tuple.Create("Lợi nhuận", "Profit"),
                Tuple.Create("Giảm giá", "Discount"),
               Tuple.Create("Phí vận chuyển", "ShippingFee"),
                Tuple.Create("Doanh thu", "Amount"),
                Tuple.Create("Ngày tạo", "CreatedDate"),

                Tuple.Create("Trạng thái thanh toán", "PaymentStatusName"),

                Tuple.Create("Phương thức vận chuyển", "ShippingTypeName"),
                Tuple.Create("Đơn vị vận chuyển", "CarrierTypeName"),
                Tuple.Create("Mã vận đơn", "ShippingTypeCode"),


                Tuple.Create("Tên người nhận", "ReceiverName"),
                Tuple.Create("Số điện thoại người nhận hàng", "Phone"),
                Tuple.Create("Tỉnh / Thành phố ", "ProvinceName"),
                Tuple.Create("Quận / Huyện", "DistrictName"),
                Tuple.Create("Xã / Phường", "DistrictName"),
                Tuple.Create("Địa chỉ", "WardName")
,
                };

                    // Set up header row
                    ws.Cells["A1"].PutValue("STT");
                    cell.SetColumnWidth(0, 8);
                    for (int i = 0; i < headers.Count; i++)
                    {
                        cell.SetColumnWidth(i + 1, 30);
                        ws.Cells[0, i + 1].PutValue(headers[i].Item1);
                    }

                    // Set header style
                    //var headerStyle = ws.Cells.CreateRange(0, 0, 1, headers.Count + 1).GetStyle();
                    //headerStyle.Font.IsBold = true;
                    //headerStyle.IsTextWrapped = true;
                    //headerStyle.ForegroundColor = Color.FromArgb(33, 88, 103);
                    //headerStyle.BackgroundColor = Color.FromArgb(33, 88, 103);
                    //headerStyle.Pattern = BackgroundType.Solid;
                    //headerStyle.Font.Color = Color.White;
                    //headerStyle.VerticalAlignment = TextAlignmentType.Center;
                    ////headerStyle.Borders.SetBorders(BorderType.AllBorder, CellBorderType.Thin, Color.Black);
                    //headerStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    //headerStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    //headerStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    //headerStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;

                    // Set header style
                    // Lấy Style từ ô A1
                    var headerStyle = ws.Cells["A1"].GetStyle();

                    // Thiết lập các thuộc tính cho headerStyle
                    headerStyle.Font.IsBold = true;
                    headerStyle.IsTextWrapped = true;
                    headerStyle.ForegroundColor = Color.FromArgb(33, 88, 103);
                    headerStyle.BackgroundColor = Color.FromArgb(33, 88, 103);
                    headerStyle.Pattern = BackgroundType.Solid;
                    headerStyle.Font.Color = Color.White;
                    headerStyle.VerticalAlignment = TextAlignmentType.Center;

                    // Sử dụng cú pháp đúng để thiết lập viền cho toàn bộ range
                    // Tạo range
                    var headerRange = ws.Cells.CreateRange(0, 0, 1, headers.Count + 1);
                    // Đặt viền cho range
                    headerRange.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
                    headerRange.SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
                    headerRange.SetOutlineBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
                    headerRange.SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);

                    // Áp dụng style đã có cho toàn bộ range
                    StyleFlag st = new();
                    st.All = true;
                    headerRange.ApplyStyle(headerStyle, st);

                    //ws.Cells.CreateRange(0, 0, 1, headers.Count + 1).ApplyStyle(headerStyle, new StyleFlag() { All = true });

                    // Fill data rows
                    int rowIndex = 1;
                    foreach (var item in data)
                    {
                        rowIndex++;
                        ws.Cells["A" + rowIndex].PutValue(rowIndex - 1);

                        for (int colIndex = 0; colIndex < headers.Count; colIndex++)
                        {
                            var propertyName = headers[colIndex].Item2;
                            var propertyValue = item.GetType().GetProperty(propertyName)?.GetValue(item, null);
                            string cellValue = propertyValue?.ToString() ?? string.Empty;

                            // Handle special formatting for specific fields
                            if (propertyName == "SalerName")
                            {
                                cellValue = $"{item.SalerName}\n{item.SalerUserName}\n{item.SalerEmail}";
                            }
                            else if (propertyName == "PermisionTypeName")
                            {
                                cellValue = $"{(string.IsNullOrEmpty(item.PermisionTypeName) ? "Không công nợ" : item.PermisionTypeName)} - {item.PaymentStatusName}";
                            }

                            ws.Cells[rowIndex - 1, colIndex + 1].PutValue(cellValue);
                        }
                    }

                    // Set body style
                    var bodyRange = cell.CreateRange(1, 0, data.Count, headers.Count + 1);
                    var bodyStyle = ws.Cells["A2"].GetStyle();
                    //bodyStyle.Borders.SetBorders(BorderType.AllBorder, CellBorderType.Thin, Color.Black);
                    bodyStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    bodyStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    bodyStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    bodyStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    bodyStyle.VerticalAlignment = TextAlignmentType.Center;
                    bodyRange.ApplyStyle(bodyStyle, new StyleFlag() { All = true });

                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportDeposit - OrderRepository: " + ex);
            }
            return pathResult;
        }
    }
}
