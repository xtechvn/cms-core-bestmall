
$(document).ready(function () {
    flashsale_detail.Initialization()
})
var flashsale_detail = {
    Processing: false,
    Initialization: function () {
        flashsale_detail.DynamicBind()
        flashsale_detail.RenderFlashsale()
        flashsale_detail.RenderDragAndDrop()
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
            var tr = element.closest('.flashsale-data-tr-product')
            flashsale_detail.updateDiscountedPrice(tr);

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
        $('body').on('click', '#flashsale-data-tr-product-body .delete-row', function () {
            var element = $(this)
            _msgconfirm.openDialog('Xác nhận xóa sản phẩm', 'Sản phẩm này sẽ bị xóa khỏi chương trình FlashSale, bạn chắc chắn không?', function () {
                element.closest('.flashsale-data-tr-product').remove()

            })

        });

        // --- Sự kiện cho nút "Cập nhật hàng loạt" ---
        $('body').on('click', '#flashsale-detail-apply-all .btn.btn-base.btn-round', function () {
            const applyAllVnd = $('#flashsale-detail-apply-all .flashsale-applyall-tr-vnd').val();
            const applyAllPercent = $('#flashsale-detail-apply-all .flashsale-applyall-tr-percent').val();
            const applyAllStatus = $('#flashsale-detail-apply-all .flashsale-applyall-tr-status').prop('checked');

            $('#flashsale-data-tr-product-body .flashsale-data-tr-product').each(function () {
                const tr = $(this);

                const $vndInput = tr.find('.flashsale-data-tr-vnd');
                const $percentInput = tr.find('.flashsale-data-tr-percent');

                // Giá trị VND
                if (applyAllPercent == null || applyAllPercent == undefined || applyAllPercent.trim() == '') {
                    $vndInput.val(applyAllVnd);
                    tr.find('.flashsale-data-tr-percent').val('').trigger('change')
                }
                // Giá trị phần trăm
                else if (applyAllVnd == null || applyAllVnd == undefined || applyAllVnd.trim() == '') {
                    $percentInput.val(applyAllPercent);
                    tr.find('.flashsale-data-tr-vnd').val('').trigger('change')
                }

                // Áp dụng trạng thái checkbox
                const $statusCheckbox = tr.find('.flashsale-data-tr-status');
                if ($statusCheckbox.length > 0) {
                    $statusCheckbox.prop('checked', applyAllStatus);
                }
                // Cập nhật "Giá đã giảm"
                flashsale_detail.updateDiscountedPrice(tr);
            });

            console.log('Đã áp dụng cập nhật hàng loạt!');
        });

        // --- Sự kiện cho nút xóa hàng (.delete-row) ---
        $('body').on('click', '.flashsale-applyall-tr-deleteall', function () {
            _msgconfirm.openDialog('Xác nhận xóa sản phẩm', 'Tất cả sản phẩm sẽ bị xóa khỏi chương trình FlashSale, bạn chắc chắn không?', function () {
                $('#flashsale-data-tr-product-body').html('')
            })
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
                window.location.href = '/flashsale'
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

        $('body').on('click', '#update-icon-import', function () {
            $('#update-icon-input').click()

        });
        $('body').on('change', '#update-icon-input', function () {
            flashsale_detail.Upload()

        })
        $('body').on('select2:select', '#supplier-id select', function () {
            var element = $(this)
            element.prop('disabled', true);
            var model = {
                supplier_id: element.find(':selected').val(),
                flash_sale_id: $('#flashsale-detail').val()
            }
            flashsale_detail.POST('/FlashSale/GetActiveFlashSaleCampaignBySupplier', model, function (result) {
                element.prop('disabled', false);
                if (result.is_success && result.exists) {
                    _msgalert.error(result.msg)
                }
            });
        });
    },
    Upload: function () {
        flashsale_detail.Processing = true;
        var element = $('#update-icon-input')
        var ImageExtension = ['jpeg', 'jpg', 'png', 'bmp'];

        if ($.inArray(element.val().split('.').pop().toLowerCase(), ImageExtension) == -1) {
            _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + ImageExtension.join(', '));
            $('#update-icon-input').val(null).trigger('change')
            return
        }
        $(element[0].files).each(function (index, item) {

            var reader = new FileReader();
            reader.onload = function (e) {
                $('#update-icon-import img').attr('src', e.target.result)
                $('#update-icon-import img').css('top', '0')
                $('#update-icon-import img').css('object-fit', 'fill')
            }
            reader.readAsDataURL(item);
            return false
        });
        $('#update-icon-import img').attr('src', url)
        $('#update-icon').val(url).trigger('change')
        $('#update-icon-input').val(null).trigger('change')
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
        var supplier_id = '-1';
        var supplier = $('#supplier-id select').find(':selected')
        if (supplier != undefined && supplier.val() != undefined && supplier.val().trim() != '') {
            supplier_id = supplier.val()
        }
        var model = {
            keyword: $('#add-flashsale-search-name').val(),
            group_id: group_selected == undefined ? '-1' : group_selected,
            supplier_id: supplier_id
        }
        flashsale_detail.POST('/FlashSale/ProductFlashSaleSearch', model, function (result) {
            $('#add-flashsale tbody').html(result)
            $('#add-flashsale tbody tr').each(function (index, item) {
                var element = $(this)
                var product_id = element.attr('data-id')
                $('#flashsale-data-tr-product-body .flashsale-data-tr-product').each(function (index, item) {
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
                                        <td colspan="10" class="p-0">
                                            <table class="table text-center table-nowrap  w-100">
                                                <tbody>
                                                    <tr>

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
                                                    <tr>
                                                        <td colspan="2"></td>
                                                        <td class="text-right" style="min-width: 180px;">@amount_text</td>
                                                        <td colspan="2">
                                                            <div class="flex gap10 flex-nowrap align-items-center "style="justify-self: anchor-center;">
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
                                                        <td class="flashsale-data-tr-final-amount">
                                                            @amount_text 
                                                        </td>
                                                        <td class="tr-index"> @item.Position</td>
                                                        <td>
                                                            <label class="switch no-text">
                                                                <input class="flashsale-data-tr-status" type="checkbox"checked>
                                                                <span class="slider round"></span>
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <div class="flex gap10 align-items-center justify-content-center">
                                                                <a class="icon-action  delete-row" href="javascript:;"><i class="icofont-trash"></i></a>
                                                                <a class="icon-action flashsale-drag" href="javascript:;"><i class="icofont-drag"></i></a>
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </td>

                                    </tr>


        `;
        var count = flashsale_detail.CountFlashSaleProduct();

        $('#add-flashsale tbody tr').each(function (index, item) {
            var element = $(this)
            var product_id = element.attr('data-id')
            var exists = false
            if (element.find('.check-product').prop('checked') == false) {
                return true
            }
            $('#flashsale-data-tr-product-body .flashsale-data-tr-product').each(function (index, item) {
                var compare = $(this)
                var product_id_compare = compare.attr('data-product')
                if (product_id_compare.trim() == product_id) {
                    exists = true
                    return false
                }
            })
            if (exists == false) {
                $('#flashsale-data-tr-product-body').append(
                    template.replaceAll('@item._id', element.attr('data-id'))
                        .replaceAll('@item.Position', ++count)
                        .replaceAll('@img_src', element.find('img').attr('src'))
                        .replaceAll('@item.name', element.find('.product-name').text())
                        .replaceAll('@amount_text', element.find('.product-amount').text())
                        .replaceAll('@amount', element.find('.product-amount').attr('data-amount'))
                        .replaceAll('@item.quanity_of_stock', element.find('.product-stock').text())
                        .replaceAll('@item.code', element.find('.product-code').text())
                        .replaceAll('@variation_string', element.find('.product-variation').text())

                )
            }
        })


    },
    CountFlashSaleProduct: function () {
        var element = $('.flashsale-data-tr-product').length
        return element == undefined ? 0 : element
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
    POSTSynchorus: function (url, model) {
        var data = undefined
        $.ajax({
            url: url,
            type: "POST",
            data: model,
            success: function (result) {
                data = result;
            },
            error: function (err) {
                console.log(err)
            },
            async: false
        });
        return data
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
    updateDiscountedPrice: function (element) {
        var original_amount = element.attr('data-amount');
        let finalPrice = original_amount;
        var vnd = element.find('.flashsale-data-tr-vnd').val()
        var percent = element.find('.flashsale-data-tr-percent').val()
        const vndValue = parseFloat(vnd == undefined ? '-1' : vnd.replaceAll(',', ''));
        const percentValue = parseFloat(percent == undefined ? '-1' : percent.replaceAll(',', ''));
        // Kiểm tra thứ tự ưu tiên: nếu có VND thì ưu tiên VND, nếu không thì dùng %
        if (!isNaN(vndValue) && vndValue > 0) {
            finalPrice = original_amount - vndValue;
        } else if (!isNaN(percentValue) && percentValue > 0) {
            finalPrice = original_amount * (100 - percentValue) / 100;
        } else {
            finalPrice = original_amount; // Nếu cả hai đều rỗng hoặc không hợp lệ
        }
        // Đảm bảo giá không âm
        if (finalPrice < 0) {
            finalPrice = 0;
        }
        finalPrice = flashsale_detail.roundDownToNearestFiveHundreds(finalPrice)
        element.find('.flashsale-data-tr-final-amount').text('đ ' + _global_function.Comma(finalPrice));
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

        //--validate other active flashsale
        var model = {
            supplier_id: $('#supplier-id select').find(':selected').val(),
            flash_sale_id: $('#flashsale-detail').val()
        }
        var result = flashsale_detail.POSTSynchorus('/FlashSale/GetActiveFlashSaleCampaignBySupplier', model)
        if (result.is_success && result.exists) {
            _msgalert.error(result.msg)
            return false
        }


        var name = $('#flashsale-name').val()
        if (name == undefined || name.trim() == '') {
            _msgalert.error("Tên chương trình không được để trống")
            return false
        }
        $('tbody tr.flashsale-data-tr-product').each(function (index) {
            const $productRow = $(this); // Dòng sản phẩm hiện tại

            // Tìm input trạng thái trong dòng sale
            const $statusInput = $productRow.find('.flashsale-data-tr-status');
            // Tìm input VND và Percent trong dòng sale
            const $vndInput = $productRow.find('.flashsale-data-tr-vnd');
            const $percentInput = $productRow.find('.flashsale-data-tr-percent');

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
        $('tbody tr.flashsale-data-tr-product').each(function (index) {
            const $productRow = $(this); // Dòng sản phẩm hiện tại

            const id = $productRow.data('id'); // Lấy data-id từ dòng product
            const productId = $productRow.data('product'); // Lấy data-product từ dòng product

            let discountValue = 0; // Giá trị chiết khấu
            let valueType = 0; // Loại giá trị (0: VND, 1: Phần trăm)

            // Tìm input VND và Percent trong dòng sale
            const $vndInput = $productRow.find('.flashsale-data-tr-vnd');
            const $percentInput = $productRow.find('.flashsale-data-tr-percent');

            // Kiểm tra input VND có giá trị không
            if ($vndInput.val() != undefined && $vndInput.val().trim() !== '') {
                discountValue = parseFloat($vndInput.val().trim().replaceAll(',','')); // Chuyển đổi sang số thực
                valueType = 0; // Loại VND
            } else if ($percentInput.val() != undefined &&$percentInput.val().trim() !== '') {
                // Nếu input VND không có giá trị, kiểm tra input Percent
                discountValue = parseFloat($percentInput.val().trim().replaceAll(',', '')); // Chuyển đổi sang số thực
                valueType = 1; // Loại Phần trăm
            }

            // Lấy trạng thái của sản phẩm
            const status = $productRow.find('.flashsale-data-tr-status').is(':checked') ? 1 : 0;

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
        var loading = '<img src="/images/icons/loading.gif" style="width:24px;">'
        $('#flashsale-detail-confirm').html(loading)
        $('#flashsale-detail-confirm').prop('disabled', true)
        var model = {
            flashsale_product: flashsale_detail.collectProductInfo(),
            flashsale: {
                Id: $('#flashsale-detail').val(),
                FromDate: _global_function.GetDayText($('#flashsale-search-fromdate').data('daterangepicker').startDate._d, true),
                ToDate: _global_function.GetDayText($('#flashsale-search-todate').data('daterangepicker').startDate._d, true),
                SupplierId: $('#supplier-id select').find(':selected').val(),
                Status: $('#status input').is(':checked') ? 1 : 0,
                Name: $('#flashsale-name').val(),
            }
        }
        model.flashsale.Banner = ''
        var element_image = $('#update-icon-import')
        if (element_image.find('img').length > 0) {
            var data_src = element_image.find('img').attr('src')
            if (data_src != null && data_src != undefined && data_src.trim() != '' && flashsale_detail.CheckIfImageVideoIsLocal(data_src)) {
                var result = flashsale_detail.POSTSynchorus('/Files/SummitImages', { data_image: data_src, width: 1220, height: 300 })
                if (result != undefined && result.data != undefined && result.data.trim() != '') {
                    model.flashsale.Banner = result.data
                } else {
                    model.flashsale.Banner = data_src
                }
            }
            else {
                model.flashsale.Banner = data_src
            }
        }
        flashsale_detail.POST('/FlashSale/Summit', model, function (result) {
            if (result.is_success) {
                _global_function.RemoveLoading()
                _msgalert.success(result.msg)
                $('#flashsale-detail-confirm').html('Thành công')
                setTimeout(function () {
                    window.location.href = '/FlashSale';
                }, 2000);
            }
            else {
                _global_function.RemoveLoading()
                _msgalert.error(result.msg)
                $('#flashsale-detail-confirm').html('Xác nhận')
                $('#flashsale-detail-confirm').prop('disabled', false)

            }
        });

    },
    AddProductMedia: function (element) {
        switch (element.attr('data-type')) {
            case 'images':
                {
                    if ($.inArray(element.val().split('.').pop().toLowerCase(), _product_constants.VALUES.ImageExtension) == -1) {
                        _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + _product_constants.VALUES.ImageExtension.join(', '));
                        return
                    }
                    $(element[0].files).each(function (index, item) {

                        var reader = new FileReader();
                        reader.onload = function (e) {
                            element.closest('.list').prepend(_product_constants.HTML.ProductDetail_Images_Item.replaceAll('{src}', e.target.result).replaceAll('{id}', '-1'))
                            element.closest('.items').find('.count').html(element.closest('.list').find('.magnific_popup').length)

                        }
                        reader.readAsDataURL(item);
                    });
                    element.val(null)
                } break
            case 'avatar':
            case 'avatar':
                {
                    if ($.inArray(element.val().split('.').pop().toLowerCase(), _product_constants.VALUES.ImageExtension) == -1) {
                        _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + _product_constants.VALUES.ImageExtension.join(', '));
                        return
                    }
                    $(element[0].files).each(function (index, item) {

                        var reader = new FileReader();
                        reader.onload = function (e) {
                            element.closest('.list').prepend(_product_constants.HTML.ProductDetail_Images_Item.replaceAll('{src}', e.target.result).replaceAll('{id}', '-1'))
                            element.closest('.items').find('.count').html(element.closest('.list').find('.magnific_popup').length)

                        }
                        reader.readAsDataURL(item);
                    });
                    element.val(null)
                } break
            case 'videos': {
                if ($.inArray(element.val().split('.').pop().toLowerCase(), _product_constants.VALUES.VideoExtension) == -1) {
                    _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + _product_constants.VALUES.VideoExtension.join(', '));
                    return
                }
                if (typeof FileReader !== "undefined") {
                    var size = element[0].files[0].size;
                    if (size > _product_constants.VALUES.VideoMaxSize) {
                        _msgalert.error("Vui lòng chỉ upload video có dung lượng dưới 30MB.");
                        return
                    }
                }
                $(element[0].files).each(function (index, item) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        element.closest('.list').prepend(_product_constants.HTML.ProductDetail_Video_Item.replaceAll('{src}', e.target.result).replaceAll('{id}', '-1'))
                        element.closest('.items').find('.count').html(element.closest('.list').find('.magnific_popup').length)

                    }
                    reader.readAsDataURL(item);
                });
                element.val(null)


            } break
            case 'image_row_item': {

                if ($.inArray(element.val().split('.').pop().toLowerCase(), _product_constants.VALUES.ImageExtension) == -1) {
                    _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + _product_constants.VALUES.ImageExtension.join(', '));
                    return
                }
                var reader = new FileReader();
                reader.onload = function (e) {
                    element.closest('.choose').find('.choose-content').html(_product_constants.HTML.ProductDetail_Images_Item.replaceAll('{src}', e.target.result).replaceAll('{id}', '-1'))
                }
                reader.readAsDataURL(element[0].files[0]);
                element.val(null)
            } break
        }
        element.closest('.choose-wrap').find('.count').html(_product_function.Comma(element.closest('.list').find('.items').length))


    },
    CheckIfImageVideoIsLocal: function (data) {
        if (data != undefined && (data.includes("data:image") || data.includes("data:video") || data.includes("base64,"))) {
            return true
        }
        else {
            return false
        }
    },
    ReindexFlashsaleProduct: function () {
        var count = 0;
        $('#flashsale-data-tr-product-body .flashsale-data-tr-product').each(function () {
            var element = $(this)
            element.find('.tr-index').text(++count)
        })
    },
    RenderDragAndDrop: function () {
        // Make the tbody sortable
        $('#flashsale-data-tr-product-body').sortable({
            handle: '.flashsale-drag', // The handle to drag the rows
            items: '.flashsale-data-tr-product', // Only allow dragging of these items
            axis: 'y', // Restrict dragging to the vertical axis
            opacity: 0.7, // Opacity of the helper while dragging
            placeholder: 'ui-state-highlight', // Class added to the placeholder
            start: function (event, ui) {
                // Optional: Add a class to the item being dragged
                ui.item.addClass('dragging');
            },
            stop: function (event, ui) {
                // Optional: Remove the dragging class when dragging stops
                ui.item.removeClass('dragging');
                // You can also get the new order of items here if needed
                // var newOrder = $(this).sortable('toArray', {attribute: 'data-id'});
                // console.log(newOrder);
                flashsale_detail.ReindexFlashsaleProduct()
            }
        }).disableSelection(); // Prevents text selection during drag
    },
    roundDownToNearestFiveHundreds: function (amount) {
        // Chia số tiền cho 500, làm tròn xuống bằng Math.floor, sau đó nhân lại với 500.
        return Math.floor(amount / 500) * 500;
	},
    SyncES: function () {
        flashsale_detail.POST('/flashsale/SyncES', {}, function (result) {
            if (result.is_success) {
                _msgalert.success('Sync ES Successfully')
            }
            else {
                _msgalert.error('Sync ES Failed')
            }
        });
    }
}
