
$(document).ready(function () {
    flashsale_detail.Initialization()
})
var flashsale_detail = {

    Initialization: function () {
        flashsale_detail.DynamicBind()
        flashsale_detail.RenderFlashsale()
        flashsale_detail.SingleDateTimePicker($('#flashsale-search-fromdate'))
        flashsale_detail.SingleDateTimePicker($('#flashsale-search-todate'))
        flashsale_detail.Select2Supplier($('#supplier-id select'))

    },
    DynamicBind: function () {
        $('body').on('keyup', '.flashsale-data-tr-vnd, .flashsale-data-tr-percent,.flashsale-applyall-tr-vnd,.flashsale-applyall-tr-percent', function () {
            var element = $(this)
            var value = element.val()
            var tr = element.closest('tr')
            if (value == undefined || value.trim() == '') {
                return
            }
            if (element.hasClass('flashsale-data-tr-vnd')) {
                tr.find('.flashsale-data-tr-percent').val('').trigger('change')
            }
            if (element.hasClass('flashsale-data-tr-percent')) {
                tr.find('.flashsale-data-tr-vnd').val('').trigger('change')
            }
            if (element.hasClass('flashsale-applyall-tr-vnd')) {
                tr.find('.flashsale-applyall-tr-percent').val('').trigger('change')
            }
            if (element.hasClass('flashsale-applyall-tr-percent')) {
                tr.find('.flashsale-applyall-tr-vnd').val('').trigger('change')
            }
            value = _global_function.RemoveSpecialCharacter(value)
            element.val(_global_function.Comma(value)).trigger('change')
        });
        $('body').on('keyup', '.flashsale-data-tr-vnd, .flashsale-data-tr-percent', function () {
            var element = $(this)
            var value = element.val()
            var tr = element.closest('tr')
            var originalPrice = tr.attr('data-amount')
            var $productTr = tr
            flashsale_detail.updateDiscountedPrice($productTr, originalPrice);

        });
        $('body').on('click', '#add-flashsale-btn', function () {

            flashsale_detail.AddNewFlashsale()

        });
        $('body').on('click', '#add-flashsale .mfp-close, #add-flashsale-btn-cancel', function () {

            flashsale_detail.CloseAddNewFlashsale()

        });
        $('body').on('click', '#add-flashsale-search-confirm', function () {

            flashsale_detail.FlashsaleSearch()

        });
        $('body').on('click', '#add-flashsale td', function () {
            var element = $(this)
            var tr = element.closest('tr')
            var checked = tr.find('.check-product').prop('checked')
            if (!checked) tr.find('.check-product').prop('checked', true)
            else tr.find('.check-product').prop('checked', false)
        });
        $('body').on('click', '#add-flashsale-search-clear', function () {

            $('#add-flashsale-search-group').val('null').trigger('change')
            $('#add-flashsale-search-name').val('').trigger('change')
            flashsale_detail.FlashsaleSearch()

        });
        $('body').on('click', '#add-flashsale-btn-confirm', function () {

            flashsale_detail.ConfirmFlashsale()
            flashsale_detail.CloseAddNewFlashsale()

        });
        $('body').on('click', '#flashsale tbody tr .delete-row', function () {
            var element = $(this)

            _msgconfirm.openDialog('Xác nhận xóa sản phẩm', 'Sản phẩm này sẽ bị xóa khỏi chương trình FlashSale, bạn chắc chắn không?', function () {
                var tr = element.closest('tr')
                var id = tr.attr('data-id')
                var productid = tr.attr('data-product')
                $('#flashsale tbody .flashsale-data-tr-product').each(function (index, item) {
                    var compare = $(this)
                    var id_compare = compare.attr('data-id')
                    var product_id_compare = compare.attr('data-product')
                    if (id_compare.trim() == id.trim() && productid.trim() == product_id_compare.trim()) {
                        compare.remove()
                        return false
                    }
                })
                tr.remove()

            })

        });

        // --- Sự kiện cho nút "Cập nhật hàng loạt" ---
        $('body').on('click', '#flashsale-detail-apply-all .btn.btn-base.btn-round', function () {
            const applyAllVnd = $('#flashsale-detail-apply-all .flashsale-applyall-tr-vnd').val();
            const applyAllPercent = $('#flashsale-detail-apply-all .flashsale-applyall-tr-percent').val();
            const applyAllStatus = $('#flashsale-detail-apply-all .flashsale-applyall-tr-status').prop('checked');

            $('tbody tr.flashsale-data-tr-product').each(function () {
                const $productTr = $(this);
                const productId = $productTr.data('product');
                const $saleTr = $('tr.flashsale-data-tr-sale[data-product="' + productId + '"]');

                if ($saleTr.length > 0) {
                    const originalPrice = parseFloat($productTr.data('amount')); // Lấy giá gốc từ data-amount của productTr

                    // Áp dụng giá trị VND
                    const $vndInput = $saleTr.find('.flashsale-data-tr-vnd');
                    if ($vndInput.length > 0) {
                        $vndInput.val(applyAllVnd);
                    }

                    // Áp dụng giá trị phần trăm
                    const $percentInput = $saleTr.find('.flashsale-data-tr-percent');
                    if ($percentInput.length > 0) {
                        $percentInput.val(applyAllPercent);
                    }

                    // Áp dụng trạng thái checkbox
                    const $statusCheckbox = $saleTr.find('.flashsale-data-tr-status');
                    if ($statusCheckbox.length > 0) {
                        $statusCheckbox.prop('checked', applyAllStatus);
                    }

                    // Cập nhật "Giá đã giảm"
                   flashsale_detail.updateDiscountedPrice( $saleTr, originalPrice);
                }
            });

            console.log('Đã áp dụng cập nhật hàng loạt!');
        });

        // --- Sự kiện cho nút xóa hàng (.delete-row) ---
        $('body').on('click', '.flashsale-applyall-tr-deleteall', function () {
            _msgconfirm.openDialog('Xác nhận xóa sản phẩm', 'Tất cả sản phẩm sẽ bị xóa khỏi chương trình FlashSale, bạn chắc chắn không?', function () {
                $('#flashsale tbody').html('')
            })
        });

        // --- Logic để tính toán giá đã giảm khi nhập VND hoặc % ---
        $('body').on('input', '.flashsale-data-tr-vnd, .flashsale-data-tr-percent', function () {
            const $input = $(this);
            const $saleTr = $input.closest('tr.flashsale-data-tr-sale');
            if ($saleTr.length === 0) return;

            const productId = $saleTr.data('product');
            const $productTr = $('tr.flashsale-data-tr-product[data-product="' + productId + '"]');
            if ($productTr.length === 0) return;

            const originalPrice = parseFloat($productTr.data('amount')); // Lấy giá gốc từ data-amount của productTr

            flashsale_detail.updateDiscountedPrice( $saleTr, originalPrice);
        });

        // Sự kiện input và change cho các trường nhập liệu VND và Percent
        // Khi người dùng nhập hoặc thay đổi giá trị, xóa thông báo lỗi liên quan
        $('body').on('input change', '.flashsale-data-tr-vnd, .flashsale-data-tr-percent', function () {
            // Xóa class 'err' và thông báo lỗi ngay sau input hiện tại
            $(this).removeClass('err').next('.error-message').remove();
            // Đảm bảo rằng nếu có lỗi ở input còn lại, nó cũng bị xóa
            // (ví dụ: nếu người dùng nhập vào VND sau khi lỗi hiển thị ở cả 2)
            const $parentDiv = $(this).closest('.flex');
            $parentDiv.find('.flashsale-data-tr-vnd, .flashsale-data-tr-percent').removeClass('err').next('.error-message').remove();
        });
        $('body').on('click', '#flashsale-detail-cancel', function () {
            _msgconfirm.openDialog('Xác nhận hủy', 'Tất cả các thông tin chương trình sẽ không được lưu lại, bạn chắc chắn không?', function () {
                window.location.href='/flashsale'
            })
        });
        $('body').on('click', '#flashsale-detail-confirm', function () {
            var validate = flashsale_detail.validateProducts()
            if (!validate) return;
            var id = $('#flashsale-detail').val()
            var id_value = (id == undefined || isNaN(parseInt(id))) ? 0 : parseInt(id)
            _msgconfirm.openDialog('Xác nhận' + (id_value > 0 ? 'cập nhật chương trình' : 'thêm chương trình'), 'Thông tin chương trình Flashsale được ' + (id_value > 0 ? 'cập nhật' : 'thêm mới') + ', bạn chắc chắn không?', function () {
                flashsale_detail.Summit()
            })
        });
    },


    SingleDateTimePicker: function (element) {

        var today = new Date();
        var yyyy = today.getFullYear();
        var yyyy_max = yyyy + 10;
        var current_date = element.val();
        var min_range = '01/01/2025 00:00';
        var max_range = '31/12/' + yyyy_max + ' 23:59';
        var mm = today.getMonth() + 1; // Months start at 0!
        var dd = today.getDate();
        var hh = today.getHours();
        var minutes = today.getMinutes()
        if (current_date == undefined || current_date == null || current_date.trim() == '') {
            current_date = dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + minutes
        }

        element.daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            showDropdowns: true,
            drops: 'down',
            minDate: min_range,
            maxDate: max_range,
            timePicker: true,
            timePicker24Hour: true,
            locale: {
                format: 'DD/MM/YYYY HH:mm',
                // Thêm các thuộc tính Việt hóa cho tháng và ngày
                applyLabel: 'Áp dụng',
                cancelLabel: 'Hủy bỏ',
                fromLabel: 'Từ',
                toLabel: 'Đến',
                customRangeLabel: 'Tùy chỉnh',
                weekLabel: 'W',
                daysOfWeek: [
                    'CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'
                ],
                monthNames: [
                    'Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
                    'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'
                ],
                firstDay: 1
            }
        }, function (start, end, label) {
            element.data('daterangepicker').setStartDate(start);

        });
        element.data('daterangepicker').setStartDate(current_date);
    },
    AddNewFlashsale: function () {
        $('#add-flashsale').show()
        $('#add-flashsale').addClass('show')
        flashsale_detail.FlashsaleSearch()
    },
    CloseAddNewFlashsale: function () {
        $('#add-flashsale').hide()
        $('#add-flashsale').removeClass('show')
    },
    RenderFlashsale: function () {
        var model = {
            id: $('#flashsale-detail').attr('data-id')
        }
        flashsale_detail.POST('/FlashSale/ProductFlashSale', model, function (result) {
            $('body').append(result)
            flashsale_detail.Select2Flashsale($('#add-flashsale-search-group'))
        });
    },
    FlashsaleSearch: function () {
        var group_selected = $('#add-flashsale-search-group') == undefined ? '-1' : $('#add-flashsale-search-group').find(':selected').val()
        var model = {
            keyword: $('#add-flashsale-search-name').val(),
            group_id: group_selected == undefined ? '-1' : group_selected,

        }
        flashsale_detail.POST('/FlashSale/ProductFlashSaleSearch', model, function (result) {
            $('#add-flashsale tbody').html(result)
            $('#add-flashsale tbody tr').each(function (index, item) {
                var element = $(this)
                var product_id = element.attr('data-id')
                $('#flashsale tbody tr').each(function (index, item) {
                    var compare = $(this)
                    var product_id_compare = compare.attr('data-id')
                    if (product_id_compare.trim() == product_id) {
                        element.find('.check-product').prop('checked', true)
                        return false
                    }
                })

            })
        });
    },
    Select2Flashsale: function (element) {
        element.select2({
            ajax: {
                url: "/Product/SearchGroupProduct",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        keyword: params.term,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: '[' + item.id + '] - ' + item.name,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    ConfirmFlashsale: function () {
        var template = `
        <tr class="flashsale-data-tr-product" data-id="0" data-product="@item._id" data-amount="@amount">
    <td style="max-width: 0">
        <label class="check-list mb20 mr25">
            <input type="checkbox">
            <span class="checkmark"></span>
        </label>
    </td>
    <td colspan="9">
        <div class="item-order">
            <div class="img">
                <img src="@img_src" alt="">

            </div>
            <div class="info">
                <h3 class="name-product">
                    @item.name
                </h3>
            </div>
        </div>
    </td>
</tr>
<tr class="flashsale-data-tr-sale" data-id="0" data-product="@item._id" data-amount="@amount">
    <td colspan="2"></td>
    <td> đ @(item.amount_min == null ? item.amount.ToString("N0") : ((double)item.amount_min).ToString("N0"))
    </td>
    <td colspan="1">
        <div class="flex gap10 flex-nowrap align-items-center" style="place-self: anchor-center;">
            <div class="form-group mb-0 price">
                <input type="text" class="form-control flashsale-data-tr-vnd" placeholder="Nhập số">
                <span class="note">đ</span>
            </div>
            <span>hoặc</span>
            <div class="form-group mb-0 price">
                <input type="text" class="form-control flashsale-data-tr-percent" placeholder="Nhập số">
                <span class="note">%</span>
            </div>
        </div>
    </td>
    <td  class="flashsale-data-tr-final-amount">
       đ @(item.amount_min == null ? item.amount.ToString("N0") : ((double)item.amount_min).ToString("N0"))
    </td>
    <td>
        <label class="switch no-text">
            <input class="flashsale-data-tr-status" type="checkbox" checked>
            <span class="slider round"></span>
        </label>
    </td>
    <td>
        <div class="flex gap10 align-items-center justify-content-center">
            <a class="icon-action delete-row" href="javascript:;"><i class="icofont-trash"></i></a>
            <a class="icon-action flashsale-drag " href="javascript:;"><i class="icofont-drag"></i></a>
        </div>
    </td>
</tr>

        `;
        $('#add-flashsale tbody tr').each(function (index, item) {
            var element = $(this)
            var product_id = element.attr('data-id')
            var exists = false
            $('#flashsale tbody tr').each(function (index, item) {
                var compare = $(this)
                var product_id_compare = compare.attr('data-id')
                if (product_id_compare.trim() == product_id) {
                    exists = true
                    return false
                }
            })
            if (exists == false && element.find('.check-product').prop('checked') == true) {
                $('#flashsale tbody').append(
                    template.replaceAll('@item._id', element.attr('data-id'))
                        .replaceAll('@img_src', element.find('img').attr('src'))
                        .replaceAll('@item.name', element.find('.product-name').text())
                        .replaceAll('đ @(item.amount_min == null ? item.amount.ToString("N0") : ((double)item.amount_min).ToString("N0"))', element.find('.product-amount').text())
                        .replaceAll('@amount', element.find('.product-amount').attr('data-amount'))
                        .replaceAll('@item.quanity_of_stock', element.find('.product-stock').text())
                        .replaceAll('@item.code', element.find('.product-code').text())
                        .replaceAll('@variation_string', element.find('.product-variation').text())

                )
            }
        })


    },
    POST: function (url, model, callback) {
        $.ajax({
            url: url,
            type: "POST",
            data: model,
            success: function (result) {
                callback(result)
            },
            error: function (err) {
                console.log(err)
            }
        });
    },
    Select2Supplier: function (element) {
        element.select2({
            ajax: {
                url: "/Supplier/SearchSupplier",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        txt_search: params.term,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: ((item.supplierCode == null || item.supplierCode == undefined || item.supplierCode.trim() == '') ? '' : (item.supplierCode + ' - ')) + ' ' + item.fullName,
                                id: item.supplierId,
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    // --- Hàm hỗ trợ để cập nhật giá đã giảm ---
    updateDiscountedPrice: function ( $saleTr, originalPrice) {
        let finalPrice = originalPrice;

        const vndValue = parseFloat($saleTr.find('.flashsale-data-tr-vnd').val().replaceAll(',',''));
        const percentValue = parseFloat($saleTr.find('.flashsale-data-tr-percent').val().replaceAll(',', ''));

        // Kiểm tra thứ tự ưu tiên: nếu có VND thì ưu tiên VND, nếu không thì dùng %
        if (!isNaN(vndValue) && vndValue > 0) {
            finalPrice = originalPrice - vndValue;
            $saleTr.find('.flashsale-data-tr-percent').val(''); // Xóa giá trị % nếu nhập VND
        } else if (!isNaN(percentValue) && percentValue > 0) {
            finalPrice = originalPrice * (1 - percentValue / 100);
            $saleTr.find('.flashsale-data-tr-vnd').val(''); // Xóa giá trị VND nếu nhập %
        } else {
            finalPrice = originalPrice; // Nếu cả hai đều rỗng hoặc không hợp lệ
        }

        // Đảm bảo giá không âm
        if (finalPrice < 0) {
            finalPrice = 0;
        }

        // Cập nhật giá đã giảm (cột thứ 5 của flashsale-data-tr-sale)
        // Lưu ý: td:nth-child(5) có thể thay đổi nếu bạn thay đổi cấu trúc cột.
        // Đây là cách dựa trên index của cột.
        $saleTr.find('.flashsale-data-tr-final-amount').text('đ ' + finalPrice.toLocaleString('vi-VN'));
    },
    validateProducts: function () {
        let isValid = true; // Biến cờ để theo dõi trạng thái hợp lệ
        var fromdateElement = $('#flashsale-search-fromdate');
        var todateElement = $('#flashsale-search-todate');

        // Lấy đối tượng Moment.js từ daterangepicker
        var fromDatePickerObject = fromdateElement.data('daterangepicker');
        var toDatePickerObject = todateElement.data('daterangepicker');
        if (fromDatePickerObject && fromDatePickerObject.startDate &&
            toDatePickerObject && toDatePickerObject.startDate) {
            var fromDate = fromDatePickerObject.startDate.toDate();
            var toDate = toDatePickerObject.startDate.toDate();

            if (fromDate <= toDate) {
            } else {
                _msgalert.error("Khoảng ngày không hợp lệ: Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.")
                return false; // Trả về false nếu không hợp lệ
            }
        } else {
            _msgalert.error("Không thể lấy được thời gian chạy chương trình Flashsale. Vui lòng chọn thời gian chạy chương trình và thử lại.")
            return false;

        }
        var supplier_id = $('#supplier-id select').find(':selected')
        if (supplier_id == undefined || supplier_id.val() == undefined || supplier_id.val().trim() == '') {
            _msgalert.error("Vui lòng chọn nhà cung cấp")

            return false
        }

        $('tbody tr.flashsale-data-tr-product').each(function (index) {
            const $productRow = $(this); // Dòng sản phẩm hiện tại
            const $saleRow = $productRow.next('.flashsale-data-tr-sale'); // Dòng sale tương ứng

            // Tìm input trạng thái trong dòng sale
            const $statusInput = $saleRow.find('.flashsale-data-tr-status');
            // Tìm input VND và Percent trong dòng sale
            const $vndInput = $saleRow.find('.flashsale-data-tr-vnd');
            const $percentInput = $saleRow.find('.flashsale-data-tr-percent');

            // Xóa bỏ thông báo lỗi cũ (nếu có)
            $vndInput.removeClass('err').next('.error-message').remove();
            $percentInput.removeClass('err').next('.error-message').remove();

            // Kiểm tra nếu sản phẩm đang được kích hoạt (status checked)
            if ($statusInput.is(':checked')) {
                const vndValue = $vndInput.val().trim(); // Lấy giá trị input VND
                const percentValue = $percentInput.val().trim(); // Lấy giá trị input Percent

                // Nếu cả hai input đều không có giá trị
                if (vndValue === '' && percentValue === '') {
                    isValid = false; // Đặt cờ là không hợp lệ
                    // Thêm class 'err' và thông báo lỗi cho cả hai input
                    $vndInput.addClass('err').after('<span class="error-message" style="color: red;">Cần nhập 1 trong 2</span>');
                    $percentInput.addClass('err').after('<span class="error-message" style="color: red;">Cần nhập 1 trong 2</span>');
                }
            }
        });
        return isValid; // Trả về trạng thái hợp lệ tổng thể
    },
    collectProductInfo: function () {
        const productData = []; // Mảng để lưu trữ dữ liệu sản phẩm
        // Lặp qua từng cặp tr.flashsale-data-tr-product và tr.flashsale-data-tr-sale
        $('tbody tr.flashsale-data-tr-product').each(function (index) {
            const $productRow = $(this); // Dòng sản phẩm hiện tại
            const $saleRow = $productRow.next('.flashsale-data-tr-sale'); // Dòng sale tương ứng

            const id = $productRow.data('id'); // Lấy data-id từ dòng product
            const productId = $productRow.data('product'); // Lấy data-product từ dòng product

            let discountValue = 0; // Giá trị chiết khấu
            let valueType = 0; // Loại giá trị (0: VND, 1: Phần trăm)

            // Tìm input VND và Percent trong dòng sale
            const $vndInput = $saleRow.find('.flashsale-data-tr-vnd');
            const $percentInput = $saleRow.find('.flashsale-data-tr-percent');

            // Kiểm tra input VND có giá trị không
            if ($vndInput.val().trim() !== '') {
                discountValue = parseFloat($vndInput.val().trim()); // Chuyển đổi sang số thực
                valueType = 0; // Loại VND
            } else if ($percentInput.val().trim() !== '') {
                // Nếu input VND không có giá trị, kiểm tra input Percent
                discountValue = parseFloat($percentInput.val().trim()); // Chuyển đổi sang số thực
                valueType = 1; // Loại Phần trăm
            }

            // Lấy trạng thái của sản phẩm
            const status = $saleRow.find('.flashsale-data-tr-status').is(':checked') ? 1 : 0;

            // Vị trí của sản phẩm (dựa trên index của dòng product)
            const position = index + 1; // Vị trí bắt đầu từ 1

            // Thêm thông tin sản phẩm vào mảng
            productData.push({
                Id: id,
                ProductId: productId,
                DiscountValue: discountValue,
                ValueType: valueType,
                Status: status,
                Position: position
            });
        });
        return productData; // Trả về mảng dữ liệu sản phẩm
    },
    Summit: function () {
        var model = {
            flashsale_product: flashsale_detail.collectProductInfo(),
            flashsale: {
                Id: $('#flashsale-detail').val(),
                FromDate: _global_function.GetDayText($('#flashsale-search-fromdate').data('daterangepicker').startDate._d,true),
                ToDate: _global_function.GetDayText($('#flashsale-search-todate').data('daterangepicker').startDate._d, true),
                SupplierId: $('#supplier-id select').find(':selected').val(),
                Status: $('#status input').is(':checked')?1:0,
            }
        }
        flashsale_detail.POST('/FlashSale/Summit', model, function (result) {
            if (result.is_success) {
                _global_function.RemoveLoading()
                _msgalert.success(result.msg)
                setTimeout(function () {
                    window.location.href = '/FlashSale';
                }, 2000);
            }
            else {
                _global_function.RemoveLoading()
                _msgalert.error(result.msg)

            }
        });

    }
}
//-- Drag & Drop:
$(document).ready(function () {
    let $draggedProductTr = null;
    let $draggedSaleTr = null;
    let $placeholder = null;
    let $lastTargetTr = null; // Biến mới để lưu trữ TR mục tiêu cuối cùng

    // Hàm tìm cặp TR dựa trên data-product
    function findTrPair(productId) {
        const $productTr = $('tr.flashsale-data-tr-product[data-product="' + productId + '"]');
        const $saleTr = $('tr.flashsale-data-tr-sale[data-product="' + productId + '"]');
        return { $productTr, $saleTr };
    }

    // Gắn sự kiện dragstart
    $('body').on('dragstart', '.flashsale-drag', function (e) {
        const originalEvent = e.originalEvent;
        originalEvent.stopPropagation();

        const $currentSaleTr = $(this).closest('tr.flashsale-data-tr-sale');
        if ($currentSaleTr.length === 0) return;

        const productId = $currentSaleTr.data('product');
        const pair = findTrPair(productId);

        $draggedProductTr = pair.$productTr;
        $draggedSaleTr = pair.$saleTr;

        if ($draggedProductTr.length > 0 && $draggedSaleTr.length > 0) {
            originalEvent.dataTransfer.effectAllowed = 'move';

            $draggedProductTr.addClass('dragging-pair');
            $draggedSaleTr.addClass('dragging-pair');

            // Tạo một placeholder với colspan cho td bên trong
            $placeholder = $('<tr class="flashsale-placeholder"><td colspan="10"></td></tr>'); // Điều chỉnh colspan
            $placeholder.css({
                'height': ($draggedProductTr.outerHeight() + $draggedSaleTr.outerHeight()) + 'px',
                'background-color': '#e0e0e0',
                'border': '1px dashed #999',
                'visibility': 'hidden'
            });
            $lastTargetTr = null; // Reset last target
        } else {
            originalEvent.preventDefault();
        }
    });

    // Gắn sự kiện dragover cho tbody
    $('body').on('dragover', 'tbody', function (e) {
        const originalEvent = e.originalEvent;
        originalEvent.preventDefault();
        originalEvent.stopPropagation();

        if (!$draggedProductTr || !$draggedSaleTr || !$placeholder) return;

        const $targetTr = $(originalEvent.target).closest('tr');
        const $tbody = $(this);

        // Kiểm tra xem TR mục tiêu có phải là một phần của cặp đang kéo không
        if ($targetTr.length > 0 &&
            $targetTr[0] !== $draggedProductTr[0] &&
            $targetTr[0] !== $draggedSaleTr[0]) {

            // Nếu targetTr giống với lastTargetTr, không làm gì cả để tránh nhấp nháy
            if ($targetTr[0] === $lastTargetTr) {
                return;
            }
            $lastTargetTr = $targetTr[0]; // Cập nhật last target

            const targetProductId = $targetTr.data('product');
            const targetPair = findTrPair(targetProductId);

            let $referenceNode = null;

            if (targetPair.$productTr.length > 0) {
                const targetProductTrElement = targetPair.$productTr[0];
                const targetSaleTrElement = targetPair.$saleTr[0];

                const boundingBox = targetProductTrElement.getBoundingClientRect();
                const totalHeight = boundingBox.height + targetSaleTrElement.offsetHeight;
                // Sử dụng 0.5 để chia đôi, hoặc có thể điều chỉnh ngưỡng nếu cần
                const offset = boundingBox.y + (totalHeight / 2);

                if (originalEvent.clientY > offset) {
                    // Kéo xuống dưới cặp hàng mục tiêu
                    $referenceNode = targetPair.$saleTr.next('tr');
                } else {
                    // Kéo lên trên cặp hàng mục tiêu
                    $referenceNode = targetPair.$productTr;
                }
            } else {
                // Nếu không tìm thấy cặp hàng hợp lệ, có thể đang kéo vào khoảng trống
                // hoặc ra khỏi các nhóm chính, xử lý theo ý muốn
                if ($tbody[0].contains($targetTr[0])) { // Nếu vẫn trong tbody
                    // Có thể chèn vào cuối nếu kéo ra khỏi các cặp có sẵn
                    $referenceNode = null; // Đặt null để chèn vào cuối tbody
                } else {
                    return; // Không làm gì nếu không phải tr hợp lệ trong tbody
                }
            }

            // Chèn placeholder chỉ khi vị trí mục tiêu khác với vị trí hiện tại của placeholder
            // Hoặc nếu placeholder chưa được thêm vào DOM
            if ($placeholder.parent().length === 0 ||
                ($referenceNode && $placeholder.prev()[0] !== $referenceNode[0]) || // Placeholder đang ở trước referenceNode không phải là referenceNode đã xác định
                (!$referenceNode && $tbody.children().last()[0] !== $placeholder[0]) // Kéo xuống cuối, placeholder không phải là con cuối cùng
            ) {
                if ($referenceNode && $referenceNode.length > 0) {
                    $placeholder.insertBefore($referenceNode);
                } else { // Chèn vào cuối tbody
                    $tbody.append($placeholder);
                }
                $placeholder.css('visibility', 'visible');
            }
        } else if ($targetTr[0] === $draggedProductTr[0] || $targetTr[0] === $draggedSaleTr[0]) {
            // Nếu kéo trên chính nó, ẩn placeholder và reset lastTargetTr
            if ($placeholder && $placeholder.parent().length > 0) {
                $placeholder.css('visibility', 'hidden'); // Ẩn thay vì xóa để tránh reflow
            }
            $lastTargetTr = null; // Reset lastTargetTr
        }
    });

    // Gắn sự kiện drop cho tbody
    $('body').on('drop', 'tbody', function (e) {
        const originalEvent = e.originalEvent;
        originalEvent.preventDefault();
        originalEvent.stopPropagation();

        if (!$draggedProductTr || !$draggedSaleTr || !$placeholder) return;

        // Chèn cặp hàng vào vị trí của placeholder
        if ($placeholder.parent().length > 0) {
            $draggedProductTr.insertBefore($placeholder);
            $draggedSaleTr.insertBefore($placeholder);
            $placeholder.remove();
        }

        // Dọn dẹp
        $draggedProductTr.removeClass('dragging-pair');
        $draggedSaleTr.removeClass('dragging-pair');
        $draggedProductTr = null;
        $draggedSaleTr = null;
        $placeholder = null;
        $lastTargetTr = null;

        console.log('Cặp hàng đã được di chuyển.');
    });

    // Gắn sự kiện dragend cho tbody
    $('body').on('dragend', 'tbody', function (e) {
        if ($draggedProductTr) {
            $draggedProductTr.removeClass('dragging-pair');
        }
        if ($draggedSaleTr) {
            $draggedSaleTr.removeClass('dragging-pair');
        }
        if ($placeholder && $placeholder.parent().length > 0) {
            $placeholder.remove();
        }
        $draggedProductTr = null;
        $draggedSaleTr = null;
        $placeholder = null;
        $lastTargetTr = null;
    });
});