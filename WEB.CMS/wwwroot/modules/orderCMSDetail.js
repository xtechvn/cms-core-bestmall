let input = $('#order_Id').val();
let type = 7;
$(document).ready(function () {
   
    _orderDetail.LoadPackages(input);
    _orderDetail.LoadContractPay(input);
    _orderDetail.LoadBillVAT(input);
    _orderDetail.LoadFile(input, type);
    _orderDetail.LoadPersonInCharge(input);
    _orderDetail.DynamicBind();
});
var _orderDetail = {
    LoadOeederDetail: function () {
        _orderDetail.LoadPackages(input);
        _orderDetail.LoadContractPay(input);
        _orderDetail.LoadBillVAT(input);
        _orderDetail.LoadFile(input, type);
        _orderDetail.LoadPersonInCharge(input);
    },

    LoadPackages: function (input) {
        $.ajax({
            url: "/Order/Packages",
            type: "Post",
            data: { orderId: input},
            success: function (result) {
                $('#imgLoading_Packages').hide();
                $('#grid_data_Packages').html(result);
            }
        });
    },
    LoadContractPay: function (input) {
        $.ajax({
            url: "/Order/ContractPay",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_ContractPay').hide();
                $('#grid_data_ContractPay').html(result);
            }
        });
    },
    LoadBillVAT: function (input) {
        $.ajax({
            url: "/Order/BillVAT",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_BillVAT').hide();
                $('#grid_data_BillVAT').html(result);
            }
        });
    },
    LoadListPassenger: function (input) {
        $.ajax({
            url: "/Order/ListPassenger",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_ListPassenger').hide();
                $('#grid_data_ListPassenger').html(result);
            }
        });
    },
    LoadFile: function (input, type) {
        _global_function.RenderFileAttachment($('#grid_data_File'), input,type)
        $('#imgLoading_File').hide();
        /*
        $.ajax({
            url: "/Order/File",
            type: "Post",
            data: { orderId: input, type: type },
            success: function (result) {
                $('#imgLoading_File').hide();
                $('#grid_data_File').html(result);
            }
        });
        */
    },
    LoadPersonInCharge: function (input) {
        $.ajax({
            url: "/Order/PersonInCharge",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_PersonInCharge').hide();
                $('#grid_data_PersonInCharge').html(result);
            }
        });
    },
    ChangeOrderSaler: function (order_id, order_no) {
       
        var title = 'Nhận xử lý đơn hàng';
        var description = 'Bạn có chắc chắn muốn nhận xử lý đơn hàng này không?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/Order/ChangeOrderSaler",
                type: "Post",
                data: { order_id: order_id, saleid: 0, OrderNo: order_no},
                success: function (result) {
                    if (result.status === 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 1000);
                    }
                    else {
                        _msgalert.error(result.msg);

                    }
                }
            });
        });
    },
    EditAddress: function (order_id) {
        var title = 'Cập nhật địa chỉ giao hàng';
        let url = '/Order/EditAddress';
        let param = {
            orderId: order_id,
        };
      
        _magnific.OpenSmallPopup(title, url, param);
    },
    UpdateAddress: function () {
        var FromAddress = $('#form_Update_Address');
        FromAddress.validate({
            rules: {

                "ProvinceId": "required",

                "DistrictId": "required",
                "WardId": "required",
                "Address": "required",
  
            },
            messages: {
                "ProvinceId": "Tỉnh/Huyện không được bỏ trống",

                "DistrictId": "Thành phố không được bỏ trống",
                "WardId": "Phường/Xã không được bỏ trống",
                "Address": "Địa chỉ  không được bỏ trống",
            }
        });
        if (FromAddress.valid()) {
            var model = {
                OrderId: $('#OrderId').val(),
                ProvinceId: $('#ProvinceId').val(),
                DistrictId: $('#DistrictId').val(),
                WardId: $('#WardId').val(),
                Address: $('#Address').val(),
            }
            $.ajax({
                url: "/Order/UpdateAddress",
                type: "Post",
                data: { model: model },
                success: function (result) {
                    if (result.status === 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 1000);
                    } else {
                        _msgalert.error(result.msg);
                    }
                }
            });
        }
  
    },
    loadingDistrict: function () {
        var id = $('#ProvinceId').val();
        var row = '';
        $("#DistrictId").empty();
        $("#DistrictId").append('<option value="">Chọn địa điểm</option>');

        $("#WardId").empty();
        $("#WardId").append('<option value="">Chọn địa điểm</option>');
  
        $.ajax({
            url: "/Order/SuggestDistrict",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.status === 0) {
                    for (var i = 0; i < result.data.length; i++) {
                        $("#DistrictId").append(' <option value="' + result.data[i].id + '">' + result.data[i].name + '</option>')
                    }
            
                }
            }
        });

    },
    loadingWard: function () {
        var id = $('#DistrictId').val();
        var row = '';
        $("#WardId").empty();
        $("#WardId").append('<option value="">Chọn địa điểm</option>');
        $.ajax({
            url: "/Order/SuggestWard",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.status === 0) {
                    for (var i = 0; i < result.data.length; i++) {
                        $("#WardId").append(' <option value="' + result.data[i].id + '">' + result.data[i].name + '</option>')
                    }
                }
            }
        });

    },
    DynamicBind: function () {
        $('body').on('click', '#send-order-to-carrier', function () {
            _orderDetail.SendToCarrier()

        });
    },
    SendToCarrier: function () {
        var title = 'Chuyển đơn hàng sang cho ĐVVC';
        var description = 'Đơn hàng này sẽ được chuyển sang cho ĐVVC, bạn có chắc chắn không?';
        _msgconfirm.openDialog(title, description, function () {
            var id = $('#order_Id').val()
            $.ajax({
                url: "/OrderManual/SendToCarrier",
                type: "Post",
                data: { id: id },
                success: function (result) {
                    if (result.is_success) {
                        _msgalert.success(result.msg);
                    } else {
                        _msgalert.error(result.msg);
                    }
                }
            });
        });
       
    }
    
}
