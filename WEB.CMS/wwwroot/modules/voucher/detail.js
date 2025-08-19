$(document).ready(function () {

    voucher_detail.Initialization()
})


var voucher_detail = {
    Initialization: function () {
        voucher_detail.DynamicBind()
        voucher_detail.RenderProductBuyWith()
        voucher_detail.Select2Group($('#voucher-campaign_id'))
        voucher_detail.SingleDateTimePicker($('#voucher-from-date'))
        voucher_detail.SingleDateTimePicker($('#voucher-to-date'))
        voucher_detail.ClientSelect2($('#voucher-GroupUserPriority'))
    },
    DynamicBind: function () {
        $('body').on('change', 'input[name="voucher-rule-type"]', function () {
            let value = $(this).val();
            switch (value.trim()) {
                default: {
                    $('#voucher-campaign_id').closest('.item').hide()
                    $('#voucher-campaign_id').val(null).trigger('change')
                } break;
                case '2': {
                    $('#voucher-campaign_id').closest('.item').show()
                } break;
            }
        });
        $('body').on('change', 'input[name="voucher-is_limit_voucher"]', function () {
            let value = $(this).val();
            switch (value.trim()) {
                default: {
                    $('#voucher-limit_total_discount-vnd').closest('.item').hide()
                    $('#voucher-limit_total_discount-vnd').val(null).trigger('change')
                } break;
                case 'true': {
                    $('#voucher-limit_total_discount-vnd').closest('.item').show()
                } break;
            }
        });
        //-- Product Buy With
        $('body').on('click', '#voucher-cancel', function () {

            var title = "Hủy thêm mới / Cập nhật mã giảm giá";
            var description = "Mọi thông tin sẽ không được lưu lại, bạn có chắc chắn không?";
            _msgconfirm.openDialog(title, description, function () {
                window.location.href ='/voucher'

            });

        });
        $('body').on('click', '#voucher-confirm', function () {
            if (voucher_detail.Validate() == true) {
                voucher_detail.Summit()

            }

        });
        $('body').on('click', '#voucher-store_apply-add', function () {

            voucher_detail.AddNewProductBuyWith()

        });
        $('body').on('click', '#add-product-buy-with .mfp-close, #add-product-buy-with-btn-cancel', function () {

            voucher_detail.CloseAddNewProductBuyWith()

        });
        $('body').on('click', '#add-product-buy-with-search-confirm', function () {

            voucher_detail.ProductBuyWithSearch()

        });
        $('body').on('click', '#add-product-buy-with td', function () {
            var element = $(this)
            var tr = element.closest('tr')
            var checked = tr.find('.check-product').prop('checked')
            if (!checked) tr.find('.check-product').prop('checked', true)
            else tr.find('.check-product').prop('checked', false)
        });
        $('body').on('click', '#add-product-buy-with-search-clear', function () {

            $('#add-product-buy-with-search-group').val('null').trigger('change')
            $('#add-product-buy-with-search-name').val('').trigger('change')
            voucher_detail.ProductBuyWithSearch()

        });
        $('body').on('click', '#add-product-buy-with-btn-confirm', function () {

            voucher_detail.ConfirmProductBuyWith()
            voucher_detail.CloseAddNewProductBuyWith()

        });
        $('body').on('click', '#product-buy-with tbody tr .delete-row', function () {
            var element = $(this)
            element.closest('tr').remove()
            voucher_detail.RenderStoreApplyValue()

        });
        $('body').on('keyup', '.input-price', function () {
            var element = $(this)
            var value = parseFloat(element.val().replaceAll(',', ''))
            if (isNaN(value)) value = 0
            element.val(voucher_detail.Comma(value))
        });
    },
    AddNewProductBuyWith: function () {
        $('#add-product-buy-with').show()
        $('#add-product-buy-with').addClass('show')
        voucher_detail.ProductBuyWithSearch()
    },
    CloseAddNewProductBuyWith: function () {
        $('#add-product-buy-with').hide()
        $('#add-product-buy-with').removeClass('show')
    },
    RenderProductBuyWith: function () {
        var model = {
            id: $('#product_detail').attr('data-id')
        }
        voucher_detail.POST('/Product/ProductBuyWith', model, function (result) {
            $('body').append(result)
            voucher_detail.Select2BuyWith($('#add-product-buy-with-search-group'))
        });
    },
    ProductBuyWithSearch: function () {
        var group_selected = $('#add-product-buy-with-search-group') == undefined ? '-1' : $('#add-product-buy-with-search-group').find(':selected').val()

        var model = {
            keyword: $('#add-product-buy-with-search-name').val(),
            group_id: group_selected,
            current_id: $('#voucher-store_apply').val().split(','),
            main_product_requirement: true

        }
        voucher_detail.POST('/Product/ProductBuyWithSearch', model, function (result) {
            $('#add-product-buy-with tbody').html(result)
            $('#add-product-buy-with tbody tr').each(function (index, item) {
                var element = $(this)
                var product_id = element.attr('data-id')
                $('#product-buy-with tbody tr').each(function (index, item) {
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
    Select2BuyWith: function (element) {
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
    ConfirmProductBuyWith: function () {
        var template = `

         <tr data-id="@item._id">
                            <td class="name_img">
                                <div class="flex gap10 align-center">
                                    <div class="ava tooltip-img">
                                        <span class="thumb_img thumb_5x5" style="width: 40px;">
                                            <img src="@img_src">
                                        </span>
                                    </div>
                                    <div class="content">
                                        @item.name
                                    </div>
                                </div>
                            </td>
                            <td>
                               đ @(item.amount_min == null ? item.amount.ToString("N0") : ((double)item.amount_min).ToString("N0"))
                            </td>
                            <td>@item.quanity_of_stock</td>
                            <td>
                                <a href="javascript:;" class="delete-row">
                                                <i class="icofont-trash"></i>
                                            </a>
                            </td>
                        </tr>
        `;
        $('#add-product-buy-with tbody tr').each(function (index, item) {
            var element = $(this)
            var product_id = element.attr('data-id')
            var exists = false
            $('#product-buy-with tbody tr').each(function (index, item) {
                var compare = $(this)
                var product_id_compare = compare.attr('data-id')
                if (product_id_compare.trim() == product_id) {
                    exists = true
                    return false
                }
            })
           
            if (exists == false && element.find('.check-product').prop('checked') == true) {
                $('#product-buy-with tbody').append(
                    template.replaceAll('@item._id', element.attr('data-id'))
                        .replaceAll('@img_src', element.find('img').attr('src'))
                        .replaceAll('@item.name', element.find('.product-name').text())
                        .replaceAll('đ @(item.amount_min == null ? item.amount.ToString("N0") : ((double)item.amount_min).ToString("N0"))', element.find('.product-amount').text())
                        .replaceAll('@item.quanity_of_stock', element.find('.product-stock').text())
                        .replaceAll('@item.code', element.find('.product-code').text())
                        .replaceAll('@variation_string', element.find('.product-variation').text())

                )
               
            }
        })
        voucher_detail.RenderStoreApplyValue()

    },
    RenderStoreApplyValue: function () {
        var product_id_selected = '';
        var product_count = 0;

        $('#product-buy-with tbody tr').each(function (index, item) {
            var element = $(this)
            var product_id = element.attr('data-id')
            if (product_id_selected.trim() != '') product_id_selected += ',';
            product_id_selected += product_id;
            product_count++;
        })
        $('#voucher-store_apply').val(product_id_selected).trigger('change')
        $('#voucher-product-count').html(product_count)
    },
    Select2Group: function (element) {
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
    SingleDateTimePicker: function (element) {

        var today = new Date();
        var yyyy = today.getFullYear();
        var yyyy_max = yyyy + 10;
        var current_date = element.val();
        var mm = today.getMonth() + 1; // Months start at 0!
        var dd = today.getDate();
        var hh = today.getHours();
        var minutes = today.getMinutes()

        var time_now = dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + minutes;

        var min_range = time_now;
        var max_range = dd + '/' + mm + '/' + yyyy_max + ' ' + hh + ':' + minutes;
       
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
                format: 'DD/MM/YYYY HH:mm'
            }
        }, function (start, end, label) {


        });
        element.data('daterangepicker').setStartDate(current_date);
    },
    Validate: function () {
        let isValid = true;
        let errors = [];

        // Loại mã (RuleType)
        let ruleType = $('input[name="voucher-rule-type"]:checked').val();
        if (ruleType === undefined) {
            isValid = false;
            errors.push("Vui lòng chọn loại mã");
        }

        // Tên chương trình giảm giá
        let name = $('#voucher-name').val().trim();
        if (name.length === 0) {
            isValid = false;
            errors.push("Vui lòng nhập tên chương trình giảm giá");
        }

        // Mã voucher
        let code = $('#voucher-code').val().trim();
        if (code.length === 0) {
            isValid = false;
            errors.push("Vui lòng nhập mã voucher");
        } else if (!/^[A-Za-z0-9]+$/.test(code)) {
            isValid = false;
            errors.push("Mã voucher chỉ gồm chữ không dấu hoặc số");
        }

        // Thời gian sử dụng
        let fromDate = $('#voucher-from-date').val().trim();
        let toDate = $('#voucher-to-date').val().trim();
        if (fromDate.length === 0 || toDate.length === 0) {
            isValid = false;
            errors.push("Vui lòng chọn thời gian bắt đầu và kết thúc");
        }

        // Giá trị giảm
        let unit = $('input[name="voucher-unit"]:checked').val();
        let priceSales = unit === 'vnd'
            ? parseFloat($('#voucher-price_sales-vnd').val().replaceAll(',', ''))
            : parseFloat($('#voucher-price_sales-percent').val().replaceAll(',', ''));
        if (isNaN(priceSales) || priceSales <= 0) {
            isValid = false;
            errors.push("Vui lòng nhập mức giảm hợp lệ");
        }

        // Tổng lượt sử dụng
        let limitUse = parseInt($('#voucher-limitUse-vnd').val().replaceAll(',', ''));
        if (isNaN(limitUse) || limitUse <= 0) {
            isValid = false;
            errors.push("Vui lòng nhập tổng lượt sử dụng hợp lệ");
        }

        // Nếu có giới hạn số tiền giảm
        let isLimitVoucher = $('input[name="voucher-is_limit_voucher"]:checked').val() === "true";
        if (isLimitVoucher) {
            let limitTotalDiscount = parseFloat($('#voucher-limit_total_discount-vnd').val().replaceAll(',', ''));
            if (isNaN(limitTotalDiscount) || limitTotalDiscount <= 0) {
                isValid = false;
                errors.push("Vui lòng nhập số tiền tối đa được giảm hợp lệ");
            }
        }

        if (!isValid) {
            alert(errors.join("\n"));
        }
        return isValid;
    },
    Summit: function () {
        let unit = $('input[name="voucher-unit"]:checked').val();
        let isLimitVoucher = $('input[name="voucher-is_limit_voucher"]:checked').val() === "true";
        var from_date = $('#voucher-from-date').data('daterangepicker').startDate._d
        var to_date = $('#voucher-to-date').data('daterangepicker').startDate._d
        var group_user = $('#voucher-GroupUserPriority').val() || []
        var input= {
            Id: $('#voucher-id').val(),
            Code: $('#voucher-code').val().trim(),
            Udate: voucher_detail.GetDayTextDotNet(from_date),
            EDate: voucher_detail.GetDayTextDotNet(to_date), // Date string, backend convert sang DateTime
            LimitUse: parseInt($('#voucher-limitUse-vnd').val().replaceAll(',','')) || 0,
            PriceSales: unit === 'vnd'
                ? parseFloat($('#voucher-price_sales-vnd').val().replaceAll(',', '')) || 0
                : parseFloat($('#voucher-price_sales-percent').val().replaceAll(',', '')) || 0,
            Unit: unit,
            RuleType: parseInt($('input[name="voucher-rule-type"]:checked').val()) || 0,
            GroupUserPriority: JSON.stringify(group_user),
            IsPublic: $('#voucher-is-public').is(':checked'),
            Description: $('#voucher-description').val(),
            IsLimitVoucher: isLimitVoucher,
            LimitTotalDiscount: isLimitVoucher
                ? parseFloat($('#voucher-limit_total_discount-vnd').val().replaceAll(',', '')) || 0
                : null,
            StoreApply: $('#voucher-store_apply').val(),
            IsMaxPriceProduct: null,
            MinTotalAmount: parseFloat($('#voucher-min-product-amount').val().replaceAll(',', '')) || 0,
            CampaignId: $('#voucher-campaign_id').val() || null,
            Name: $('#voucher-name').val().trim()
        };
        voucher_detail.POST('/Voucher/Summit', { request :input}, function (result) {
            if (result.is_success) {
                _msgalert.success(result.msg);

                setTimeout(function () {
                    window.location.href = '/voucher'
                }, 2000);
            } else {
                _msgalert.error(result.msg);
            }

        });
    },
    GetDayTextDotNet: function (date) {
        var text = ("0" + (date.getMonth() + 1)).slice(-2) + '/' + ("0" + date.getDate()).slice(-2) + '/' + date.getFullYear() + ' ' + ("0" + date.getHours()).slice(-2) + ':' + ("0" + date.getMinutes()).slice(-2);
        return text;
    },
    ClientSelect2: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Tên KH, Điện Thoại, Email",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            ajax: {
                url: "/client/ClientSuggestion",
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
                                text: item.clientName + ' - ' + item.email + ' - ' + item.phone,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    Comma: function (number) { //function to add commas to textboxes
        number = ('' + number).replace(/[^0-9.,]+/g, '');
        number += '';
        number = number.replaceAll(',', '');
        x = number.split('.');
        x1 = x[0];
        x2 = x.length > 1 ? '.' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1))
            x1 = x1.replace(rgx, '$1' + ',' + '$2');
        return x1 + x2;
    },
}
