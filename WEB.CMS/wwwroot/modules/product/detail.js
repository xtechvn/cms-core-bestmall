$(document).ready(function () {
    if (window.history && window.history.pushState) {
        $(window).on('popstate', function () {
            window.location.reload()
        });

    }
    product_detail_new.Initialization()
})
var product_detail_new = {
    ValidateProcessing: false,
    Initialization: function () {
        product_detail_new.RenderHeader()
        product_detail_new.RenderSelectGroupProduct()
        product_detail_new.ShowProductTab()
        product_detail_new.DynamicBind()
        product_detail_new.RenderAttributesPrice()
        product_detail_new.ReRenderAttributesDetail()

        product_detail_new.RenderProductBuyWith()
        product_detail_new.Select2Supplier($('#supplier-id select'))
        product_detail_new.Select2Label($('#label-id select'))
        product_detail_new.Select2Spec($('#description-specification select'))
        $('#specifications-list .spec-value').attr('readonly', 'readonly')
        _common.tinyMce('#description-textarea')
        _common.tinyMce('#description-ingredients-textarea')
        _common.tinyMce('#description-effect-textarea')
        _common.tinyMce('#description-usepolicy-textarea')
    },
    DynamicBind: function () {
        $('body').on('click', '.change-tab', function () {
            var element = $(this)
            switch (element.attr('data-id')) {
                case '1': {
                    $("#images").get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

                } break
                case '2': {
                    $("#selling-information").get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

                } break
                case '3': {
                    $("#other-information").get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

                } break
                case '4': {
                    $("#other-information").get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

                } break
            }
        });
        $('body').on('click', '.magnific_popup .delete', function () {
            var element = $(this)
            var parent = element.closest('.list')
            parent.find('.count').html((parent.find('.items').length - 2))
            element.closest('.list').find('.import').show()
            element.closest('.items').remove()

        });
        $('body').on('change', '#images input', function (e) {
            var element = $(this)
            if (($('#images .list .items').length - 1 + (element[0].files.length)) > _product_constants.VALUES.ProductDetail_Max_Image) {
                _msgalert.error('Số lượng ảnh sản phẩm không được vượt quá ' + _product_function.Comma(_product_constants.VALUES.ProductDetail_Max_Image) + ' ảnh')
            }
            else {

                element.attr('data-type', 'images')
                product_detail_new.AddProductMedia(element)
            }
        });
        $('body').on('change', '#avatar input', function (e) {
            var element = $(this)
            if (($('#avatar .list .items').length - 1 + (element[0].files.length)) > _product_constants.VALUES.ProductDetail_Max_Avt) {
                _msgalert.error('Số lượng ảnh sản phẩm không được vượt quá ' + _product_function.Comma(_product_constants.VALUES.ProductDetail_Max_Avt) + ' ảnh')
            }
            else {

                element.attr('data-type', 'avatar')
                product_detail_new.AddProductMedia(element)
            }
        });
        $('body').on('change', '#videos input', function (e) {
            var element = $(this)
            if (($('#videos .list .items').length - 1 + (element[0].files.length)) > _product_constants.VALUES.ProductDetail_Max_Avt) {
                _msgalert.error('Số lượng ảnh sản phẩm không được vượt quá ' + _product_function.Comma(_product_constants.VALUES.ProductDetail_Max_Avt) + ' ảnh')
            }
            else {
                element.attr('data-type', 'videos')
                product_detail_new.AddProductMedia(element)

            }
        });
        $('body').on('keyup', 'input', function () {
            var element = $(this)
            var count_element = element.closest('.relative').find('.count')
            if (count_element.length > 0) {
                count_element.html(_product_function.Comma(element.val().length))
            } else {
                count_element = element.closest('.wrap_input').find('.count')
                if (count_element.length > 0) {
                    count_element.html(_product_function.Comma(element.val().length))
                }
            }

        });
        $('body').on('click', '.attribute-item-draggable', function () {
            var element = $(this)
            element.closest('.row-attributes-value').sortable({
                group: "row-attributes-value",
                animation: 150,
                ghostClass: "row-attributes-value",
                update: function (event, ui) {
                    product_detail_new.RenderAttributesPrice()
                    product_detail_new.ReRenderAttributesDetail()
                }
            });

        });
        //--specification
        $('body').on('click', '#specifications-list .spec-value', function () {
            var element = $(this)
            var parent = element.closest('.col-md-6').find('.select-option')
            $('#specifications-list .select-option').each(function (index, item) {
                var compare = $(this)
                if (compare.is(parent)) {
                    if (parent.is(':hidden')) {
                        parent.fadeIn()
                        product_detail_new.RenderSpecificationLi(compare)
                    } else {
                        parent.fadeOut()
                    }
                } else {
                    compare.fadeOut()
                }


            })

        });
        $('body').on('click', '.them-chatlieu .checkbox-option', function () {
            var element = $(this)
            product_detail_new.RenderSpecificationSelectOption(element)

        });
        $('body').on('click', '.specifications-list .col-md-6 .add-specificaion-value', function (e) {
            var element = $(this)
            element.closest('.border-top').find('.add-specificaion-value-box').show()
            element.hide()

        });
        $('body').on('click', '.specifications-list .col-md-6 .add-specificaion-value-add , .specifications-list .col-md-6 .add-specificaion-value-cancel ', function (e) {
            var element = $(this)
            element.closest('.border-top').find('.add-specificaion-value-box').hide()
            element.closest('.border-top').find('.add-specificaion-value').show()

        });
        $('body').on('click', '.specifications-list .col-md-6 .add-specificaion-value-add ', function (e) {
            var element = $(this)
            var parent = element.closest('.them-chatlieu').find('ul')
            var name = element.closest('.add-specificaion-value-box').find('input').val()
            parent.prepend(_product_constants.HTML.ProductDetail_Specification_Row_Item_SelectOptions_NewOptions
                .replaceAll('{checked}', 'checked')
                .replaceAll('{value}', 'undefined')
                .replaceAll('{name}', name)
            )
            element.closest('.add-specificaion-value-box').find('input').val('')
            product_detail_new.RenderSpecificationSelectOption(element.closest('.them-chatlieu').find('ul').find('li'))
            var type = element.closest('.col-md-6').find('.item').attr('data-type')

            _product_function.POST('/Product/AddProductSpecification', { type: type, name: name }, function (result) {

            });
        });
        $('body').on('keyup', '#specifications .them-chatlieu .input_search', function () {
            var element = $(this)
            element.closest('.item').find('ul').addClass('placeholder')

            setTimeout(function () {
                product_detail_new.RenderSpecificationLi(element)
            }, 500);
        });
        $('body').on('click', '#specifications .them-chatlieu .input_search', function () {
            setTimeout(function () {
                product_detail_new.RenderSpecificationLi(element)
            }, 500);
        });
        //--group product:
        $('body').on('click', '#them-nganhhang li', function () {
            var element = $(this)
            element.closest('.col-md-4').find('li').removeClass('active')
            element.addClass('active')
            product_detail_new.RenderOnSelectGroupProduct(element)

        });
        $('body').on('keyup', '.col-md-6 .select-option', function (e) {
            e.preventDefault()
        });
        $('body').on('click', '.action .btn-outline-base', function () {
            $.magnificPopup.close()

        });
        $('body').on('click', '#them-nganhhang-confirm', function () {
            var selected_count = $('#them-nganhhang .col-md-4 .active').length
            var max_group_length = $('#them-nganhhang .col-md-4').length
            if (selected_count < max_group_length) {
                _msgalert.error('Ngành hàng sản phẩm phải chọn đủ ' + max_group_length + ' cấp')
                return
            }
            product_detail_new.RenderSelectedGroupProduct()
            $.magnificPopup.close()
        });
        //--Attributes:
        $('body').on('focusout', '.attributes-detail .form-control', function () {
            var element = $(this)
            product_detail_new.RenderAddNewAttribute(element.closest('.attributes-list'), element)
        })
        $('body').on('keyup', '.attributes-detail .form-control', function () {
            var element = $(this)
            if (product_detail_new.ValidateProcessing == false) {
                setTimeout(function () {
                    product_detail_new.ValidateProcessing = true;
                    var value = element.val()
                    if (value == null || value == undefined || value.trim() == '') {
                        _msgalert.error('Vui lòng nhập đầy đủ tên biến thể')
                        element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                        product_detail_new.ValidateProcessing = false;
                        return
                    } else if (value.trim().length > 14) {
                        element.val(value.substring(0, 14));
                        _msgalert.error('Tên biến thể không được quá 14 ký tự')
                        element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                        product_detail_new.ValidateProcessing = false;
                        return
                    }
                    product_detail_new.RenderAddNewAttribute(element.closest('.attributes-list'), element, false)
                    product_detail_new.RenderAttributesPrice()
                    product_detail_new.ValidateProcessing = false;

                }, 1000);
            }
        })
        $('body').on('click', '.attributes-list .open-edit', function () {
            var element = $(this)
            element.closest('h6').find('b').hide()
            element.closest('h6').find('.change-attribute-name').show()
            var old_value = element.closest('h6').find('.change-attribute-name').find('input').val()
            element.closest('h6').find('.change-attribute-name').find('input').attr('data-old', old_value)
            element.hide()

        })
        $('body').on('click', '.attributes-list .confirm-edit', function () {
            var element = $(this)
            var value = element.closest('h6').find('.change-attribute-name').find('input').val()
            element.closest('h6').find('.change-attribute-name').find('input').attr('data-old', value)
            element.closest('h6').find('b').html(value)
            element.closest('h6').find('.change-attribute-name').hide()
            element.closest('h6').find('b').show()
            element.closest('h6').find('.open-edit').show()

        })
        $('body').on('click', '.attributes-list .cancel-edit', function () {
            var element = $(this)
            var value = element.closest('h6').find('.change-attribute-name').find('input').attr('data-old')
            element.closest('h6').find('.change-attribute-name').find('input').attr('data-old', value)
            element.closest('h6').find('.change-attribute-name').find('input').val(value)
            element.closest('h6').find('b').html(value)
            element.closest('h6').find('.change-attribute-name').hide()
            element.closest('h6').find('b').show()
            element.closest('h6').find('.open-edit').show()

        })
        $('body').on('click', '#product-attributes-add', function () {
            var element = $(this)
            var count = $('#product-attributes .attributes-list').length
            if (count >= 2) {
                element.hide()
                return
            }
            _product_function.POST('/Product/AttributeDetail', { item_index: (++count) }, function (result) {
                if (result != undefined) $('#product-attributes').append(result)
                if ($('#product-attributes .attributes-list').length > 0) {
                    $('#product-attributes-table').show()
                    $('#product-attributes').show()
                    $('#single-product-amount').hide()
                } else {
                    $('#product-attributes-table').hide()
                    $('#product-attributes').hide()
                    $('#single-product-amount').show()
                }
            })
        });
        $('body').on('click', '.attributes-list .delete ', function () {
            var element = $(this)
            element.closest('.attributes-list').remove()
            product_detail_new.RenderAttributesPrice()
            $('#product-attributes-add').show()
            $('#product-attributes .attributes-list').each(function (index, item) {
                var attr_element = $(this)
                attr_element.find('.label').html('Phân loại hàng ' + (index + 1))

            })
            if ($('#product-attributes .attributes-list').length > 0) {
                $('#product-attributes-table').show()
                $('#product-attributes').show()
                $('#single-product-amount').hide()
            } else {
                $('#product-attributes-table').hide()
                $('#product-attributes').hide()
                $('#single-product-amount').show()
            }
        });
        $('body').on('click', '.attributes-list .delete-attribute-detail ', function () {
            var element = $(this)
            element.closest('.attributes-detail').remove()
            product_detail_new.RenderAttributesPrice()

        });
        $('body').on('change', '.attributes-detail .choose input', function (e) {
            var element = $(this)
            element.attr('data-type', 'image_row_item')
            product_detail_new.AddProductMedia(element)
        });
        //--Attribute table:
        $('body').on('click', '#product-attributes-apply .btn-all', function () {
            product_detail_new.ApplyAllPriceToTable()
            $('.btn-all').css('background-color', '')
            $('.btn-all').css('border-color', '')
        });
        $('body').on('keyup', '#product-attributes-apply input', function () {
            $('.btn-all').css('background-color', '#343E7A !important')
            $('.btn-all').css('border-color', '#343E7A!important')

        });
        //--discount group buy:
        $('body').on('click', '#discount-groupbuy .btn-add', function () {
            $('#discount-groupbuy').show()
            var id = $('#discount-groupbuy tbody tr').length - 1
            var html = _product_constants_2.DiscountGroupBuy.Tr
                .replaceAll('@i', id)
                .replaceAll('@(++i)', (id + 1))
            $(html).insertBefore('#discount-groupbuy .summary')
        });
        $('body').on('click', '#discount-groupbuy .delete-row', function () {
            var element = $(this)
            element.closest('tr').remove()
        });

        //-- Weight:
        $('body').on('click', '#single-weight .switch-weight', function () {
            var element = $(this)
            if (!element.is(':checked')) {
                $('.th-weight').show()
                $('.th-dismension').show()
                $('.td-dismenssion').show()
                $('.td-weight').show()
                $('#single-weight .box-input-ship input').each(function (index, item) {
                    var element_input = $(this)
                    element_input.attr('data-old', element_input.val())
                    element_input.val('')
                    element_input.attr('readonly', 'readonly')
                })
                $('#product-attributes-apply')[0].scrollIntoView({ block: 'center', behavior: 'smooth' });
                $('#single-weight .box-input-ship').hide()
            } else {
                $('.th-weight').hide()
                $('.th-dismension').hide()
                $('.td-dismenssion').hide()
                $('.td-weight').hide()
                $('#single-weight .box-input-ship input').each(function (index, item) {
                    var element_input = $(this)
                    element_input.val(element_input.attr('data-old'))
                    element_input.removeAttr('readonly')
                })
                $('#single-weight')[0].scrollIntoView({ block: 'center', behavior: 'smooth' });
                $('#single-weight .box-input-ship').show()

            }
        });
        //--global click event:
        $('body').on('click', function (e) {
            var location = $(e.target)
            if (location.closest('.specifications-list').length === 0) {
                $('.specifications-list .col-md-6 .select-option').fadeOut()
            }
            else if (location.closest('.col-md-6').find('.select-option').length === 0) {
                $('.specifications-list .col-md-6 .select-option').fadeOut()
            }
        });
        $('body').on('keyup', '.input-price', function () {
            var element = $(this)
            var value = parseFloat(element.val().replaceAll(',', ''))
            if (isNaN(value)) value = 0
            element.val(_product_function.Comma(value))
        });
        $('body').on('keyup', '#product-attributes-prices input', function () {
            var element = $(this)
            product_detail_new.RenderRowData(element.closest('tr'))
        });
        $('body').on('click', '#product-detail-cancel', function () {
            let title = 'Xác nhận hủy';
            let description = 'Dữ liệu đã chỉnh sửa sẽ không được lưu, bạn có chắc chắn không?';
            _msgconfirm.openDialog(title, description, function () {
                window.location.href = '/product'

            });
        });
        $('body').on('click', '#product-detail-hide', function () {
            let title = 'Xác nhận ẩn sản phẩm';
            let description = 'Sản phẩm sẽ không còn được hiển thị ngoài trang sản phẩm, bạn có chắc chắn không?';
            _msgconfirm.openDialog(title, description, function () {
                _product_function.POST('/Product/ConfirmHideProduct', { product_id: $('#product_detail').attr('data-id') }, function (result) {
                    if (result.is_success) {
                        _msgalert.success('Ẩn sản phẩm thành công')
                        setTimeout(function () {
                            window.location.href = '/product'
                        }, 2000);
                    }
                    else {
                    }
                });

            });
        });
        $('body').on('click', '#product-detail-show', function () {
            let title = 'Xác nhận hiển thị sản phẩm';
            let description = 'Sản phẩm sẽ được hiển thị ngoài trang sản phẩm, bạn có chắc chắn không?';
            _msgconfirm.openDialog(title, description, function () {
                _product_function.POST('/Product/ConfirmShowProduct', { product_id: $('#product_detail').attr('data-id') }, function (result) {
                    if (result.is_success) {
                        _msgalert.success('Hiển thị sản phẩm thành công')
                        setTimeout(function () {
                            window.location.href = '/product'
                        }, 2000);
                    }
                    else {
                    }
                });

            });
        });
        $('body').on('click', '#product-detail-confirm', function () {
            product_detail_new.Summit()
        });
        $('body').on('click', '#product-detail-confirm-admin', function () {
            _msgconfirm.openDialog("Duyệt sản phẩm", "Sản phẩm này sẽ được duyệt, bạn chắc chắn không?", function () {
                product_detail_new.ActiveProduct()

            });
        });
        $('body').on('keyup', '#single-product-amount input', function () {
            var price = isNaN(parseFloat($('#main-price').find('input').val().replaceAll(',', ''))) ? 0 : parseFloat($('#main-price').find('input').val().replaceAll(',', ''))
            var profit = isNaN(parseFloat($('#main-profit').find('input').val().replaceAll(',', ''))) ? 0 : parseFloat($('#main-profit').find('input').val().replaceAll(',', ''))
            $('#main-amount').find('input').val(_product_function.Comma(price + profit))
        });
        $('body').on('keyup', '#old-price input', function (e) {
            var element = $(this)
            product_detail_new.CalucateDiscount()
        });
        $('body').on('click', '#description-specification .summary .btn-add', function (e) {
            var template = _product_constants.HTML.ProductDetail_Description_Specification
            var html = template
            $(html).insertBefore('#description-specification .summary')
            product_detail_new.Select2Spec($('#description-specification .tr-new select'))
            $('#description-specification .tr-new').removeClass('tr-new')
        });
        $('body').on('click', '#description-specification table .delete-row', function () {
            var element = $(this)
            element.closest('tr').remove()
        });
        //-- Product Buy With
        $('body').on('click', '#add-product-buy-with-btn', function () {

            product_detail_new.AddNewProductBuyWith()

        });
        $('body').on('click', '#add-product-buy-with .mfp-close, #add-product-buy-with-btn-cancel', function () {

            product_detail_new.CloseAddNewProductBuyWith()

        });
        $('body').on('click', '#add-product-buy-with-search-confirm', function () {

            product_detail_new.ProductBuyWithSearch()

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
            product_detail_new.ProductBuyWithSearch()

        });
        $('body').on('click', '#add-product-buy-with-btn-confirm', function () {

            product_detail_new.ConfirmProductBuyWith()
            product_detail_new.CloseAddNewProductBuyWith()

        });
        $('body').on('click', '#product-buy-with tbody tr .delete-row', function () {
            var element = $(this)
            element.closest('tr').remove()
        });
    },
    ShowProductTab: function () {
        $('#specification-disabled').hide()
        $('#selling-information-disabled').hide()
        $('#other-information-disabled').hide()

        $('#specifications').show()
        $('#selling-information').show()
        $('#other-information').show()
    },
    RenderHeader: function () {
        var model = [{ url: '/', name: 'Trang chủ' }, { url: '/product', name: 'Quản lý sản phẩm' }, { url: 'javascript:;', name: 'Chi tiết sản phẩm', activated: true }]
        _global_function.RenderBreadcumb(model)
        $('.add-product').removeClass('placeholder')
        $('.add-product').removeClass('box-placeholder')
    },
    RenderSelectGroupProduct: function () {
        $('.select-group-product').magnificPopup({
            type: 'inline',
            midClick: true,
            closeOnBgClick: false, // tránh đóng khi người dùng nhấn vào vùng ngoài
            mainClass: 'mfp-with-zoom',
            fixedContentPos: false,
            fixedBgPos: true,
            overflowY: 'auto',
            closeBtnInside: true,
            preloader: false,
            removalDelay: 300,
        });
        //-- Group Product
        var group_product_id = $('#group-id input').attr('data-id')
        $('#group-product-selection').attr('data-id', group_product_id)
        _product_function.POST('/Product/GroupProduct', { group_id: _product_constants_2.Values.GroupProduct }, function (result) {
            if (result.is_success && result.data) {
                $('#them-nganhhang .bg-box .row').html('')
                var html = _product_constants.HTML.ProductDetail_GroupProduct_colmd4
                var html_item = ''
                $(result.data).each(function (index, item) {
                    html_item += _product_constants.HTML.ProductDetail_GroupProduct_colmd4_Li
                        .replaceAll('{id}', item.id).replaceAll('{name}', item.name)
                })
                //-- Best Choice
                html_item += _product_constants.HTML.ProductDetail_GroupProduct_colmd4_Li
                    .replaceAll('{id}', '114').replaceAll('{name}', 'Best Choice')
                html = html.replace('{li}', html_item).replaceAll('{name}', _product_constants_2.Values.GroupProductName).replaceAll('{level}', '0')
                $('#them-nganhhang .bg-box .row').html(html)
                let _group_name = $('#them-nganhhang li[data-id="' + group_product_id.split(',')[0] + '"]').attr('data-name')
                $(group_product_id.split(',')).each(function (level, item_id) {
                    if (item_id == undefined || item_id.trim() == '' || isNaN(parseInt(item_id))) return true
                    _product_function.POST('/Product/GroupProduct', { group_id: parseInt(item_id) }, function (result) {
                        if (result.is_success && result.data && result.data.length > 0) {
                            var html = _product_constants.HTML.ProductDetail_GroupProduct_colmd4
                            var html_item = ''
                            var name = ''
                            $(result.data).each(function (index, item) {
                                html_item += _product_constants.HTML.ProductDetail_GroupProduct_colmd4_Li
                                    .replaceAll('{id}', item.id).replaceAll('{name}', item.name)
                                if (parseInt(item_id) == item.parentId) name = item.name;
                            })
                            html = html.replace('{li}', html_item).replaceAll('{name}', _group_name).replaceAll('{level}', (level))
                            $('#them-nganhhang .bg-box .row').append(html)
                            _group_name = name;
                        }
                    });
                    setTimeout(function () {
                        $('#them-nganhhang li[data-id="' + item_id + '"]').addClass('active')
                        var html_selected_popup = ''
                        $('#them-nganhhang .col-md-4').each(function (index, item) {
                            var element = $(this)
                            var selected = element.find('ul').find('.active').attr('data-name')
                            if (index >= ($('#them-nganhhang .col-md-4').length - 1)) {
                                html_selected_popup += _product_constants.HTML.ProductDetail_GroupProduct_ResultSelected.replaceAll('{name}', element.find('ul').find('.active').attr('data-name'))
                            } else {

                                html_selected_popup += _product_constants.HTML.ProductDetail_GroupProduct_ResultDirection.replaceAll('{name}', selected)
                            }

                            lastest_group_id = element.find('.active').attr('data-id')
                            level = index
                            lastest_group_name = element.find('ul').find('.active').attr('data-name')
                        })
                        $('#group-product-selection').html(html_selected_popup)
                        /* $('#them-nganhhang li[data-id="' + item + '"]').click()*/
                    }, 500);
                })
            }
        });


    },
    RenderAttributesPrice: function () {
        if ($('#product-attributes-prices tr').length <= 0) {
            var request = {
                "product_id": $('#product_detail').attr('data-id'),
                "is_one_weight": $('#single-weight').find('input[type=checkbox]').is(":checked"),
                "attributes": [],
                "attributes_detail": [],
                "sub_product": []
            }
            _product_function.POST('/Product/AttributesPrice', request, function (result) {
                $('#product-attributes-prices').html(result)

            });
        } else {
            var request = product_detail_new.GetVariationDetail()
            _product_function.POST('/Product/AttributesPrice', request, function (result) {
                $('#product-attributes-prices').html(result)

            });
        }
    },
    ReRenderAttributesDetail: function () {
        $('.attributes-list').each(function (index, item) {
            var parent = $(this)
            var first = true
            parent.find('.attributes-detail').each(function (index, item) {
                var element = $(this)
                if (first) {
                    element.find('.delete-attribute-detail').hide()
                    first = false;
                } else {
                    element.find('.delete-attribute-detail').show()
                }
            })
        })
    },
    GetVariationDetail: function () {
        var request = {
            "is_one_weight": $('#single-weight').find('input[type=checkbox]').is(":checked"),
            "attributes": [],
            "attributes_detail": [],
            "sub_product": []
        }
        $('.attributes-list').each(function (index, item) {
            var element = $(this)

            request.attributes.push({
                "_id": index,
                "name": element.find('.attr-name').val() == undefined || element.find('.attr-name').val().trim() == '' ? '' : element.find('.attr-name').val().trim()
            })
            element.find('.attributes-detail').each(function (index_detail, item_detail) {
                var element_detail = $(this)
                if (element_detail.find('.form-control').val() == undefined || element_detail.find('.form-control').val().trim() == '')
                    return true
                request.attributes_detail.push({
                    "attribute_id": index,
                    "name": element_detail.find('.form-control').val()
                })
            })
        })
        $('#product-attributes-prices tr').each(function (index, item) {
            var element = $(this)
            if (element.find('.td-price').find('input').val() == undefined || isNaN(parseFloat(element.find('.td-price').find('input').val())) || parseFloat(element.find('.td-price').find('input').val()) <= 0)
                return true
            request.sub_product.push({
                "_id": element.attr('data-id'),
                "variation_detail": [
                    {
                        "_id": '0',
                        "name": element.attr('data-attribute-0')
                    },
                    {
                        "_id": '1',
                        "name": element.attr('data-attribute-1')
                    }],
                "price": element.find('.td-price').find('input').val() == undefined ? '0' : element.find('.td-price').find('input').val().replaceAll(',', ''),
                "profit": element.find('.td-profit').find('input').val() == undefined ? '0' : element.find('.td-profit').find('input').val().replaceAll(',', ''),
                "amount": element.find('.td-amount').find('input').val() == undefined ? '0' : element.find('.td-amount').find('input').val().replaceAll(',', ''),
                "quanity_of_stock": element.find('.td-stock').find('input').val() == undefined ? '0' : element.find('.td-stock').find('input').val().replaceAll(',', ''),
                "weight": element.find('.td-weight').find('input').val() == undefined ? '0' : element.find('.td-weight').find('input').val().replaceAll(',', ''),
                "package_height": element.find('.td-dismenssion-height').find('input').val() == undefined ? '0' : element.find('.td-dismenssion-height').find('input').val().replaceAll(',', ''),
                "package_width": element.find('.td-dismenssion-width').find('input').val() == undefined ? '0' : element.find('.td-dismenssion-width').find('input').val().replaceAll(',', ''),
                "package_depth": element.find('.td-dismenssion-depth').find('input').val() == undefined ? '0' : element.find('.td-dismenssion-depth').find('input').val().replaceAll(',', ''),
                "sku": element.find('.td-sku').find('input').val(),

            })
        })
        return request
    },
    AddProductMedia: function (element) {
        switch (element.attr('data-type')) {
            case 'images':
                {
                    if ($.inArray(element.val().split('.').pop().toLowerCase(), _product_constants.VALUES.ImageExtension) == -1) {
                        _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + _product_constants.VALUES.ImageExtension.join(', '));
                        return
                    }
                    if (($('#images .list .items').length - 1 + (element[0].files.length)) == _product_constants.VALUES.ProductDetail_Max_Image) {
                        $('#images .list .import').hide()
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
                {
                    if ($.inArray(element.val().split('.').pop().toLowerCase(), _product_constants.VALUES.ImageExtension) == -1) {
                        _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + _product_constants.VALUES.ImageExtension.join(', '));
                        return
                    }
                    if (($('#avatar .list .items').length - 1 + (element[0].files.length)) == _product_constants.VALUES.ProductDetail_Max_Avt) {
                        $('#avatar .list .import').hide()
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
                if (($('#videos .list .items').length - 1 + (element[0].files.length)) == _product_constants.VALUES.ProductDetail_Max_Avt) {
                    $('#videos .list .import').hide()
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
                    element.closest('.choose').find('.choose-content').html(_product_constants.HTML.ProductDetail_Images_Row_Item.replaceAll('{src}', e.target.result).replaceAll('{id}', '-1'))
                }
                reader.readAsDataURL(element[0].files[0]);
                element.val(null)
            } break
        }
        element.closest('.choose-wrap').find('.count').html(_product_function.Comma(element.closest('.list').find('.items').length))


    },
    RenderSpecificationLi: function (element) {
        var html = ''
        var type = element.closest('.item').attr('data-id')
        var name = ''
        if (element.val() != null && element.val() != undefined && element.val().trim() != '') name = element.val()
        _product_function.POST('/Product/GetSpecificationByName', { type: type, name: name }, function (result) {
            if (result.is_success && result.data && result.data.length > 0) {
                var current_value = element.closest('.col-md-6').find('.spec-value').val()
                if (current_value == undefined) current_value = ''
                var current_value2 = current_value.split(',')
                $(result.data).each(function (index, item) {
                    html += _product_constants.HTML.ProductDetail_Specification_Row_Item_SelectOptions_NewOptions
                        .replaceAll('{option-name}', 'specification-' + type)
                        .replaceAll('{value}', item._id)
                        .replaceAll('{checked}', current_value2.includes(item.attribute_name) ? 'checked' : '')
                        .replaceAll('{name}', item.attribute_name)
                })
            }
            element.closest('.col-md-6').find('ul').html(html)
            element.closest('.item').find('ul').removeClass('placeholder')

        });
    },
    RenderOnSelectGroupProduct: function (element_selected) {
        var html_selected_popup = ''
        var lastest_group_id = 0
        var level = 0
        var lastest_group_name = ''
        var group_selected = ''
        var selected_md4_level = parseInt(element_selected.closest('.col-md-4').attr('data-level'))
        $('#them-nganhhang .col-md-4').each(function (index, item) {
            var element = $(this)
            if (index > parseInt(selected_md4_level)) {
                element.remove()
            }
        })
        $('#them-nganhhang .col-md-4').each(function (index, item) {
            var element = $(this)
            var selected = element.find('ul').find('.active').attr('data-name')
            if (index >= ($('#them-nganhhang .col-md-4').length - 1)) {
                html_selected_popup += _product_constants.HTML.ProductDetail_GroupProduct_ResultSelected.replaceAll('{name}', element.find('ul').find('.active').attr('data-name'))
                group_selected += element.find('ul').find('.active').attr('data-id')

            } else {
                group_selected += element.find('ul').find('.active').attr('data-id') + ','

                html_selected_popup += _product_constants.HTML.ProductDetail_GroupProduct_ResultDirection.replaceAll('{name}', selected)
            }

            lastest_group_id = element.find('.active').attr('data-id')
            level = index
            lastest_group_name = element.find('ul').find('.active').attr('data-name')
        })
        $('#group-product-selection').html(html_selected_popup)
        $('#group-product-selection').attr('data-id', group_selected)

        _product_function.POST('/Product/GroupProduct', { group_id: parseInt(lastest_group_id) }, function (result) {
            if (result.is_success && result.data && result.data.length > 0) {
                var html = _product_constants.HTML.ProductDetail_GroupProduct_colmd4
                var html_item = ''
                $(result.data).each(function (index, item) {
                    html_item += _product_constants.HTML.ProductDetail_GroupProduct_colmd4_Li
                        .replaceAll('{id}', item.id).replaceAll('{name}', item.name)
                })
                html = html.replace('{li}', html_item).replaceAll('{name}', lastest_group_name).replaceAll('{level}', (level + 1))
                $('#them-nganhhang .bg-box .row').append(html)

            }

        });
    },
    RenderSpecificationSelectOption: function (element) {
        var value = ''
        var html = ''
        element.closest('ul').find('input:checked').each(function (index, item) {
            var checkbox_element = $(this)
            value += checkbox_element.val()
            html += checkbox_element.closest('li').find('span').text()
            if (index < (element.closest('ul').find('input:checked').length - 1)) {
                value += ','
                html += ','
            }
        });
        element.closest('.col-md-6').find('.namesp').find('input').attr('data-value', value)
        element.closest('.col-md-6').find('.namesp').find('input').val(html)
    },
    RenderAddNewAttribute: function (parent, element, need_validate = true) {
        var exists = false
        var name = element.val()
        var has_element_no_value = false
        async function processElements() {
            parent.find('.form-control').each(function (index, item) {
                var compare = $(this)
                if (compare.is(element)) return true
                if (need_validate == true) {
                    if (name != undefined && name.toLowerCase().trim() != '' && name.toLowerCase().trim() == compare.val().toLowerCase().trim()) {
                        _msgalert.error("Tên phân loại " + name + "  đã có ")
                        exists = true
                        return false
                    }
                }
                if (compare.val() == undefined || compare.val() == null || compare.val().trim() == '') {
                    has_element_no_value = true
                    return false
                }
            })
        }
        processElements()

        if (exists == false && need_validate == false && has_element_no_value == false) {
            parent.find('.row-attributes-value').append(_product_constants_2.Attributes.Input)
        }

    },
    RenderRowData: function (tr) {
        if (tr.find('.td-price').length > 0 && tr.find('.td-profit').length > 0 && tr.find('.td-amount').length > 0) {
            var price = isNaN(parseFloat(tr.find('.td-price').find('input').val().replaceAll(',', ''))) ? 0 : parseFloat(tr.find('.td-price').find('input').val().replaceAll(',', ''))
            var profit = isNaN(parseFloat(tr.find('.td-profit').find('input').val().replaceAll(',', ''))) ? 0 : parseFloat(tr.find('.td-profit').find('input').val().replaceAll(',', ''))

            tr.find('.td-amount').find('input').val(_product_function.Comma(price + profit))
        }

    },
    ApplyAllPriceToTable: function () {
        $('#product-attributes-prices .td-price input').val(_product_function.Comma($('#product-attributes-apply .td-price input').val()))
        $('#product-attributes-prices .td-profit input').val(_product_function.Comma($('#product-attributes-apply .td-profit input').val()))
        $('#product-attributes-prices .td-stock input').val(_product_function.Comma($('#product-attributes-apply .td-stock input').val()))
        $('#product-attributes-prices .td-sku input').val($('#product-attributes-apply .td-sku input').val())
        $('#product-attributes-prices .td-weight input').val($('#product-attributes-apply .td-weight input').val())
        $('#product-attributes-prices .td-dismenssion-height input').val($('#product-attributes-apply .td-dismenssion-height input').val())
        $('#product-attributes-prices .td-dismenssion-width input').val($('#product-attributes-apply .td-dismenssion-width input').val())
        $('#product-attributes-prices .td-dismenssion-depth input').val($('#product-attributes-apply .td-dismenssion-depth input').val())
        var price = isNaN(parseFloat($('#product-attributes-apply .td-price input').val().replaceAll(',', ''))) ? 0 : parseFloat($('#product-attributes-apply .td-price input').val().replaceAll(',', ''))
        var profit = isNaN(parseFloat($('#product-attributes-apply .td-profit input').val().replaceAll(',', ''))) ? 0 : parseFloat($('#product-attributes-apply .td-profit input').val().replaceAll(',', ''))
        $('#product-attributes-prices .td-amount input').val(_product_function.Comma(price + profit))
    },
    Summit: function () {
        var validate = product_detail_new.ValidateProduct()
        if (validate == false) {
            return
        }
        _global_function.AddLoading();


        var model = {
            _id: $('#product_detail').attr('data-id') == undefined || $('#product_detail').attr('data-id').trim() == '' ? null : $('#product_detail').attr('data-id'),
            status: 1,
            code: $('#product_detail').attr('data-id') == undefined || $('#product_detail').attr('data-id').trim() == '' ? null : $('#product_detail').attr('data-id'),
            price: $('#main-price input').val() == undefined || $('#main-price input').val().trim() == '' ? 0 : parseFloat($('#main-price input').val().replaceAll(',', '')),
            profit: $('#main-profit input').val() == undefined || $('#main-profit input').val().trim() == '' ? 0 : parseFloat($('#main-profit input').val().replaceAll(',', '')),
            amount: $('#main-amount input').val() == undefined || $('#main-amount input').val().trim() == '' ? 0 : parseFloat($('#main-amount input').val().replaceAll(',', '')),
            discount: $('#discount input').val() == undefined || $('#discount input').val().trim() == '' ? 0 : parseFloat($('#discount input').val().replaceAll(',', '')),
            old_price: $('#old-price input').val() == undefined || $('#old-price input').val().trim() == '' ? 0 : parseFloat($('#old-price input').val().replaceAll(',', '')),
            quanity_of_stock: $('#main-stock input').val() == undefined || $('#main-stock input').val().trim() == '' ? 0 : parseInt($('#main-stock input').val().replaceAll(',', '')),
            label_id: $('#label-id select').find(':selected').val() == undefined || $('#label-id select').find(':selected').val().trim() == '' ? 0 : $('#label-id select').find(':selected').val(),
            supplier_id: $('#supplier-id select').find(':selected').val() == undefined || $('#supplier-id select').find(':selected').val().trim() == '' ? 0 : $('#supplier-id select').find(':selected').val(),
            review_count: $('#review-count input').val() == undefined || $('#review-count input').val().trim() == '' ? 0 : parseInt($('#review-count input').val().replaceAll(',', '')),
            rating: $('#rating input').val() == undefined || $('#rating input').val().trim() == '' ? 0 : parseFloat($('#rating input').val().replaceAll(',', '')),
            total_sold: $('#total-sold input').val() == undefined || $('#total-sold input').val().trim() == '' ? 0 : parseInt($('#total-sold input').val().replaceAll(',', '')),

        }
        model.images = []
        $('#images .list .items').each(function (index, item) {
            var element_image = $(this)
            if (element_image.find('img').length > 0) {
                //model.images.push(element_image.find('img').attr('src'))
                var data_src = element_image.find('img').attr('src')
                if (data_src == null || data_src == undefined || data_src.trim() == '') return true
                if (_product_function.CheckIfImageVideoIsLocal(data_src)) {
                    var result = _product_function.POSTSynchorus('/Product/SummitImages', { data_image: data_src })
                    if (result != undefined && result.data != undefined && result.data.trim() != '') {
                        model.images.push(result.data)
                    } else {
                        model.images.push(data_src)
                    }
                }
                else {
                    model.images.push(data_src)
                }
            }
        })
        model.avatar = $('#avatar .list .items').first().find('img').attr('src')
        if (_product_function.CheckIfImageVideoIsLocal(model.avatar)) {
            var result = _product_function.POSTSynchorus('/Product/SummitImages',
                {
                    data_image: model.avatar,
                    width: _product_constants.VALUES.AvatarSize.Width,
                    height: _product_constants.VALUES.AvatarSize.Height
                })
            if (result != undefined && result.data != undefined && result.data.trim() != '') {
                model.avatar = result.data
            }
        }
        var result = _product_function.POSTSynchorus('/Files/SummitImages', { data_image: $('#avatar .list .items').first().find('img').attr('src') })
        if (result != undefined && result.data != undefined && result.data.trim() != '') {
            model.avatar = result.data
        } else {
            model.avatar = $('#avatar .list .items').first().find('img').attr('src')
        }

        model.videos = []
        $('#videos .items .magnific_thumb').each(function (index, item) {
            var element_image = $(this)
            //model.videos.push(element_image.find('video').find('source').attr('src'))
            var data_src = element_image.find('video').find('source').attr('src')
            if (data_src == null || data_src == undefined || data_src.trim() == '') return true
            if (_product_function.CheckIfImageVideoIsLocal(data_src)) {
                const byteCharacters = atob(data_src.split('base64,')[1]);
                const byteNumbers = new Array(byteCharacters.length);
                for (let i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNumbers);
                const blob = new Blob([byteArray], { type: "video/mp4" });

                //// Create a FormData object to send via AJAX
                var formData = new FormData();
                formData.append('request', blob, 'video.mp4'); // Append the Blob as a file

                var result = _product_function.POSTFileSynchorus('/Files/SummitVideo', formData)

                if (result != undefined && result.data != undefined && result.data.trim() != '') {
                    model.videos.push(result.data)
                }
            }

        })
        function normalizeText(input) {
            return input
                .normalize("NFC")

                //.replace(/[()]/g, "")             // Loại bỏ dấu ngoặc đơn
                .replace(/\s+/g, ' ')             // Xóa khoảng trắng thừa
                .trim();
        }

        model.name = normalizeText($('#product-name input').val());
        //console.log("Normalized Product Name before sending:", model.name);
        //Console.WriteLine("Received Product Name: " + model.name);
        model.group_product_id = $('#group-id input').attr('data-id')
        // model.description = $('#description textarea').val()
        model.description = tinymce.get('description-textarea').getContent()
        model.description_ingredients = tinymce.get('description-ingredients-textarea').getContent()
        model.description_effect = tinymce.get('description-effect-textarea').getContent()
        model.description_usepolicy = tinymce.get('description-usepolicy-textarea').getContent()
        //model.specification = []
        //$('#specifications .col-md-6').each(function (index, item) {
        //    var element = $(this)

        //    model.specification.push({
        //        _id: '-1',
        //        attribute_id: element.find('.item').attr('data-id'),
        //        value_type: element.find('.item').attr('data-type'),
        //        value: element.find('.item').find('.namesp').find('input').val(),
        //        type_ids: element.find('.item').find('.namesp').find('input').attr('data-value'),
        //    })

        //})

        model.detail_specification = []
        $('#description-specification tbody tr').each(function (index, item) {
            var element = $(this)
            if (element.hasClass('summary')) { return true }
            var selected_key = element.find('select').find(':selected')
            var selected_value = element.find('input')
            if (selected_key != null && selected_key != undefined) {
                model.detail_specification.push({
                    key: selected_key.val(),
                    value: selected_value.val()
                })
            }


        })

        model.discount_group_buy = []
        $('#discount-groupbuy tbody .discount-groupbuy-row').each(function (index, item) {
            var element = $(this)
            var from = element.find('.quanity-from').find('input').val() == undefined || element.find('.quanity-from').find('input').val().trim() == '' ? 0 : parseInt(element.find('.quanity-from').find('input').val().replaceAll(',', ''))
            var to = element.find('.quanity-to').find('input').val() == undefined || element.find('.quanity-to').find('input').val().trim() == '' ? 0 : parseInt(element.find('.quanity-to').find('input').val().replaceAll(',', ''))
            var to = element.find('.quanity-to').find('input').val() == undefined || element.find('.quanity-to').find('input').val().trim() == '' ? 0 : parseInt(element.find('.quanity-to').find('input').val().replaceAll(',', ''))
            var checkbox_value = element.find('input[name="discount-type-' + (index + 1) + '"]:checked').val()

            var discount = 0
            switch (checkbox_value) {
                case '0': {
                    discount = parseFloat(element.find('.discount-number').find('input').val().replaceAll(',', ''))
                } break
                case '1': {
                    discount = parseFloat(element.find('.discount-percent').find('input').val().replaceAll(',', ''))
                    if (discount > 100) {
                        _msgalert.error("Chiết khấu tối đa 100%")
                    }
                    return false
                } break
            }
            model.discount_group_buy.push({
                from: from,
                to: to,
                discount: discount,
                type: parseInt(checkbox_value)
            })
        })
        var weight = parseFloat($('#single-weight .weight').val().replaceAll(',', ''))
        var package_width = parseFloat($('#single-weight .dismenssion-width').val().replaceAll(',', ''))
        var package_height = parseFloat($('#single-weight .dismenssion-height').val().replaceAll(',', ''))
        var package_depth = parseFloat($('#single-weight .dismenssion-depth').val().replaceAll(',', ''))
        model.weight = (weight == undefined || isNaN(weight) || weight <= 0) ? null : weight;
        model.package_width = (package_width == undefined || isNaN(package_width) || package_width <= 0) ? null : package_width;
        model.package_height = (package_height == undefined || isNaN(package_height) || package_height <= 0) ? null : package_height;
        model.package_depth = (package_depth == undefined || isNaN(package_depth) || package_depth <= 0) ? null : package_depth;
        model.is_one_weight = ($('#single-weight .switch-weight').is(':checked'))

        model.variations = []
        if (!$('#product-attributes-price').closest('.item-edit').is(':hidden')) {
            var attribute_model = product_detail_new.GetAttributeItem()
            model.attributes = attribute_model.attributes
            model.attributes_detail = attribute_model.attributes_detail
            $('#product-attributes-prices tbody tr').each(function (index, index) {
                var element = $(this)
                var var_id = element.attr('data-id')
                if (var_id == undefined) var_id = ''
                var price = parseFloat(element.find('.td-price').find('input').val().replaceAll(',', ''))
                var profit = parseFloat(element.find('.td-profit').find('input').val().replaceAll(',', ''))
                var amount = parseFloat(element.find('.td-amount').find('input').val().replaceAll(',', ''))
                var quanity_of_stock = parseFloat(element.find('.td-stock').find('input').val().replaceAll(',', ''))
                var weight = parseFloat(element.find('.td-weight').find('input').val().replaceAll(',', ''))
                var package_width = parseFloat(element.find('.td-dismenssion-width').find('input').val().replaceAll(',', ''))
                var package_height = parseFloat(element.find('.td-dismenssion-height').find('input').val().replaceAll(',', ''))
                var package_depth = parseFloat(element.find('.td-dismenssion-depth').find('input').val().replaceAll(',', ''))
                var variation = {
                    _id: var_id,
                    variation_attributes: [],
                    price: (price == undefined || isNaN(price) || price <= 0) ? null : price,
                    profit: (profit == undefined || isNaN(profit) || profit <= 0) ? null : profit,
                    amount: (amount == undefined || isNaN(amount) || amount <= 0) ? null : amount,
                    quanity_of_stock: (quanity_of_stock == undefined || isNaN(quanity_of_stock) || quanity_of_stock <= 0) ? null : quanity_of_stock,
                    sku: element.find('.td-sku').find('input').val(),
                    weight: (weight == undefined || isNaN(weight) || weight <= 0) ? model.weight : weight,
                    package_width: (package_width == undefined || isNaN(package_width) || package_width <= 0) ? model.package_width : package_width,
                    package_height: (package_height == undefined || isNaN(package_height) || package_height <= 0) ? model.package_height : package_height,
                    package_depth: (package_depth == undefined || isNaN(package_depth) || package_depth <= 0) ? model.package_depth : package_depth,

                }
                if (model.is_one_weight == true) {
                    variation.weight = model.weight
                    variation.package_width = model.package_width
                    variation.package_height = model.package_height
                    variation.package_depth = model.package_depth
                } else {

                }
                for (var i = 0; i < $('.attributes-list').length; i++) {
                    var attr_value = element.attr('data-attribute-' + i)

                    variation.variation_attributes.push({
                        id: i,
                        _id: i,
                        name: attr_value
                    })
                }
                model.variations.push(variation)
            })

        }


        model.preorder_status = $('input[name="preorder_status"]:checked').val() == '1' ? 1 : 0
        model.condition_of_product = $('#condition_of_product').find(':selected').val()
        model.sku = $('#sku input').val()

        model.products_buy_with = []
        $('#product-buy-with tbody tr').each(function (index, item) {
            var compare = $(this)
            var product_id_compare = compare.attr('data-id')
            if (product_id_compare != undefined && product_id_compare.trim() != '') {
                model.products_buy_with.push(product_id_compare)
            }
        })

        _product_function.POST('/Product/Summit', { request: model }, function (result) {
            if (result.is_success) {
                _global_function.RemoveLoading()
                _msgalert.success(result.msg)
                setTimeout(function () {
                    window.location.href = '/product';
                }, 2000);
            }
            else {
                _global_function.RemoveLoading()

                _msgalert.error(result.msg)

            }
        });

    },
    ValidateProduct: function () {
        var success = true;
        //-- images:
        var max_item = _product_constants.VALUES.ProductDetail_Max_Image
        if ($('#images .flex-lg-nowrap .magnific_popup').length >= max_item) {
            _msgalert.error('Số lượng ảnh vượt quá giới hạn')
            $('#images .flex-lg-nowrap .magnific_popup').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

            success = false
        } else if ($('#images .magnific_popup').length == 0) {
            _msgalert.error('Chưa có ảnh sản phẩm')
            $('#images .flex-lg-nowrap .magnific_popup').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

            success = false
        }

        //-- avt
        max_item = _product_constants.VALUES.ProductDetail_Max_Avt
        if ($('#avatar .flex-lg-nowrap .magnific_popup').length >= max_item) {
            _msgalert.error('Số lượng ảnh đại diện vượt quá giới hạn')
            $('#avatar .flex-lg-nowrap .magnific_popup').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

            success = false
        }
        else if ($('#avatar .magnific_popup').length == 0) {
            _msgalert.error('Chưa có ảnh đại diện sản phẩm')
            $('#avatar .flex-lg-nowrap .magnific_popup').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

            success = false
        }
        if (!success) return success
        //-- videos
        max_item = _product_constants.VALUES.ProductDetail_Max_Avt
        if ($('#videos .flex-lg-nowrap .magnific_popup').length >= max_item) {
            _msgalert.error('Số lượng video vượt quá giới hạn')
            $('#videos .flex-lg-nowrap .magnific_popup').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

            success = false
        }
        if (!success) return success
        //-- product-name:
        var value = $('#product-name input').val()
        if (value == undefined || value.trim() == '') {
            _msgalert.error('Tên sản phẩm không được bỏ trống')
            $('#product-name input').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
        } else if (value.length > 120) {
            _msgalert.error('Tên sản phẩm không được quá 120 ký tự')
            $('#product-name input').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
        }
        if (!success) return success

        //--group id
        if ($('#group-id .namesp input').val() == undefined
            || $('#group-id .namesp input').val().trim() == ''
            || $('#group-id .namesp input').attr('data-id') == undefined
            || $('#group-id .namesp input').attr('data-id').trim() == '') {
            _msgalert.error('Vui lòng chọn ngành hàng cho sản phẩm')
            $('#group-id .namesp input').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });

            success = false
        } else {
            var group_list = $('#group-id .namesp input').attr('data-id')
            var max_group_count = $('#group-id input').attr('data-group-count')
            var max_group_count_value = (max_group_count == undefined || max_group_count.trim() == '' || isNaN(parseInt(max_group_count)) || parseInt(max_group_count) <= 0) ? 3 : parseInt(max_group_count)
            if (!group_list.includes('114') && group_list.split(',').length < max_group_count_value) {
                _msgalert.error('Ngành hàng sản phẩm phải đủ ' + max_group_count_value + ' cấp')
                $('#group-id .namesp input').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
            }
        }
        //--label
        var element = $('#label-id select')
        if (element == null || element == undefined || element.find(':selected') == null || element.find(':selected') == undefined
            || element.find(':selected').val() == '' || element.find(':selected').val() == undefined) {
            _msgalert.error('Vui lòng chọn nhãn hiệu')
            $('#label-id select').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
            return false
        }
        //--supplier
        element = $('#supplier-id select')
        if (element == null || element == undefined || element.find(':selected') == null || element.find(':selected') == undefined
            || element.find(':selected').val() == '' || element.find(':selected').val() == undefined) {
            _msgalert.error('Vui lòng chọn nhà cung cấp')
            $('#supplier-id select').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
            return false
        }

        if (!success) return success
        //Mô tả chung 
        if (tinymce.get('description-textarea').getContent() == undefined || tinymce.get('description-textarea').getContent().trim() == '') {
            _msgalert.error('Vui lòng nhập Mô tả chung')
            $('#description-textarea').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
        }
        if (!success) return success
        //Thành phần
        if (tinymce.get('description-ingredients-textarea').getContent() == undefined || tinymce.get('description-ingredients-textarea').getContent().trim() == '') {
            _msgalert.error('Vui lòng nhập Thành phần')
            $('#description-ingredients-textarea').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
        }
        if (!success) return success
        //Công dụng
        if (tinymce.get('description-effect-textarea').getContent() == undefined || tinymce.get('description-effect-textarea').getContent().trim() == '') {
            _msgalert.error('Vui lòng nhập Công dụng')
            $('#description-effect-textarea').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
        }
        if (!success) return success
        //Cách dùng
        if (tinymce.get('description-usepolicy-textarea').getContent() == undefined || tinymce.get('description-usepolicy-textarea').getContent().trim() == '') {
            _msgalert.error('Vui lòng nhập Cách dùng')
            $('#description-usepolicy-textarea').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
        }
        if (!success) return success
        //--specification:

        if ($('#description-specification tbody tr') == undefined || $('#description-specification tbody tr').length <= 0) {
            _msgalert.error('Vui lòng nhập Thông tin về sản phẩm trong mục [Thông tin về sản phẩm]')
            $('#description-specification tbody').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            success = false
            return false
        } else {
            $('#description-specification tbody tr').each(function (index, item) {
                var element = $(this)
                if (element.hasClass('summary')) { return true }
                var selected_key = element.find('select').find(':selected')
                var selected_value = element.find('input')
                if (selected_key == null || selected_key == undefined) {
                    _msgalert.error('Vui lòng chọn tên thông tin về sản phẩm trong bảng [Thông tin về sản phẩm]')
                    element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                    success = false
                    return false

                }
                if (selected_value == null || selected_value == undefined || selected_value.val() == undefined || selected_value.val().trim() == '') {

                    _msgalert.error('Vui lòng nhập đầy đủ giá trị trong bảng [Thông tin về sản phẩm]')
                    element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                    success = false
                    return false
                }

            })
        }
        if (!success) return success
        //-- attributes detail
        $('.attributes-list').each(function (index, item) {
            var element = $(this)
            var name = element.find('h6').find('input').val()
            if (name == null || name == undefined || name.trim() == '') {
                _msgalert.error('Vui lòng nhập đầy đủ tên phân loại')
                element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
                return false
            }
            var max_length_attr_detail = element.find('.attributes-detail').length
            if (max_length_attr_detail == undefined || max_length_attr_detail <= 0) {
                _msgalert.error('Vui lòng nhập ít nhất 1 biến thể ứng với phân loại')
                element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
                return false
            }
            if (max_length_attr_detail == 1
                && (element.find('.attributes-detail').first().find('.relative') == undefined
                    || element.find('.attributes-detail').first().find('.relative').find('input').val() == undefined
                    || element.find('.attributes-detail').first().find('.relative').find('input').val().trim() == '')

            ) {
                _msgalert.error('Vui lòng nhập ít nhất 1 biến thể ứng với phân loại')
                element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
                return false
            }
            element.find('.attributes-detail').each(function (index_2, item_2) {
                if (index_2 >= (max_length_attr_detail - 1)) return false
                var element_detail = $(this)
                var value = element_detail.find('.relative').find('input').val()
                if (value == null || value == undefined || value.trim() == '') {
                    _msgalert.error('Vui lòng nhập đầy đủ tên biến thể')
                    element_detail.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                    success = false
                    return false
                } else if (value.trim().length > 14) {
                    _msgalert.error('Tên biến thể không được quá 14 ký tự')
                    element_detail.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                    success = false
                    return false
                }
            })
            if (success == false) return false;
        })
        if (!success) return success

        //--attributes prices:
        if ($('#product-attributes-table').is(':hidden')) {
            var price = parseFloat($('#main-price').find('input').val().replaceAll(',', ''))
            if (price == undefined || isNaN(price) || price <= 0) {
                _msgalert.error('Vui lòng nhập đầy đủ giá nhập cho sản phẩm')
                $('#main-price').find('input').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
            }
            var profit = parseFloat($('#main-profit').find('input').val().replaceAll(',', ''))
            if (profit == undefined || isNaN(profit) || profit <= 0) {
                _msgalert.error('Vui lòng nhập đầy đủ giá nhập cho sản phẩm')
                $('#main-profit').find('input').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
            }
            var stock = parseFloat($('#main-stock').find('input').val().replaceAll(',', ''))
            if (stock == undefined || isNaN(stock) || stock <= 0) {
                _msgalert.error('Vui lòng nhập đầy đủ số lượng sản phẩm trong kho hàng cho sản phẩm')
                $('#main-stock').find('input').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
            }

        } else {
            var is_one_weight = ($('#single-weight .switch-weight').is(':checked'))
            $('#product-attributes-prices tbody tr').each(function (index, index) {
                var element = $(this)
                var price = parseFloat(element.find('.td-price').find('input').val().replaceAll(',', ''))
                var profit = parseFloat(element.find('.td-profit').find('input').val().replaceAll(',', ''))
                var amount = parseFloat(element.find('.td-amount').find('input').val().replaceAll(',', ''))
                if (price == undefined || isNaN(price) || price <= 0) {
                    _msgalert.error('Vui lòng nhập đầy đủ Giá cho tất cả các biến thể của sản phẩm')
                    success = false
                    return false
                }
                if (profit == undefined || isNaN(profit) || profit < 0) {
                    _msgalert.error('Vui lòng nhập đầy đủ Lợi nhuận cho tất cả các biến thể của sản phẩm')
                    success = false
                    return false
                }
                if (amount == undefined || isNaN(amount) || amount < 0) {
                    _msgalert.error('Vui lòng nhập đầy đủ Giá bán cho tất cả các biến thể của sản phẩm')
                    success = false
                    return false
                }
                if (is_one_weight == false) {
                    var weight = parseFloat(element.find('.td-weight').find('input').val().replaceAll(',', ''))
                    if (weight == undefined || isNaN(weight) || weight <= 0) {
                        _msgalert.error('Vui lòng nhập đầy đủ cân nặng (sau khi đóng gói) cho tất cả các biến thể của sản phẩm')
                        element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                        success = false
                        return false
                    }

                    var package_width = parseFloat(element.find('.td-dismenssion-width').find('input').val().replaceAll(',', ''))
                    if (package_width == undefined || isNaN(package_width) || package_width <= 0) {
                        _msgalert.error('Vui lòng nhập đầy đủ Kích thước đóng gói - Chiều dài gói hàng cho tất cả các biến thể của sản phẩm')
                        element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                        success = false
                        return false
                    }
                    var package_height = parseFloat(element.find('.td-dismenssion-height').find('input').val().replaceAll(',', ''))
                    if (package_height == undefined || isNaN(package_height) || package_height <= 0) {
                        _msgalert.error('Vui lòng nhập đầy đủ Kích thước đóng gói - Chiều rộng gói hàng cho tất cả các biến thể của sản phẩm')
                        element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                        success = false
                        return false
                    }
                    var package_depth = parseFloat(element.find('.td-dismenssion-depth').find('input').val().replaceAll(',', ''))
                    if (package_depth == undefined || isNaN(package_depth) || package_depth <= 0) {
                        _msgalert.error('Vui lòng nhập đầy đủ Kích thước đóng gói - Chiều cao gói hàng cho tất cả các biến thể của sản phẩm')
                        element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                        success = false
                        return false
                    }
                    var quanity_of_stock = parseFloat(element.find('.td-stock').find('input').val().replaceAll(',', ''))
                    if (quanity_of_stock == undefined || isNaN(quanity_of_stock) || quanity_of_stock <= 0) {
                        _msgalert.error('Vui lòng nhập đầy đủ Kho hàng cho tất cả các biến thể của sản phẩm')
                        element.get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                        success = false
                        return false
                    }
                }
            })
        }
        if (!success) return success
        //-- one weight:
        if (is_one_weight == true) {
            var weight = parseFloat($('#single-weight').find('.weight').val().replaceAll(',', ''))
            if (weight == undefined || isNaN(weight) || weight <= 0) {
                _msgalert.error('Vui lòng nhập đầy đủ khối lượng hàng trong phần vận chuyển')
                $('#single-weight').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
                return false
            }

            var package_width = parseFloat($('#single-weight').find('.dismenssion-width').val().replaceAll(',', ''))
            if (package_width == undefined || isNaN(package_width) || package_width <= 0) {
                _msgalert.error('Vui lòng nhập đầy đủ kích thước chiều dài gói hàng trong phần vận chuyển')
                $('#single-weight').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
                return false
            }
            var package_height = parseFloat($('#single-weight').find('.dismenssion-height').val().replaceAll(',', ''))
            if (package_height == undefined || isNaN(package_height) || package_height <= 0) {
                _msgalert.error('Vui lòng nhập đầy đủ kích thước chiều rộng gói hàng trong phần vận chuyển')
                $('#single-weight').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
                return false
            }
            var package_depth = parseFloat($('#single-weight').find('.dismenssion-depth').val().replaceAll(',', ''))
            if (package_depth == undefined || isNaN(package_depth) || package_depth <= 0) {
                _msgalert.error('Vui lòng nhập đầy đủ kích thước chiều cao gói hàng trong phần vận chuyển')
                $('#single-weight').get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
                success = false
                return false
            }
        }
        if (!success) return success
        //-- discount
        if ($('#discount-groupbuy tbody tr').length > 0) {
            $('#discount-groupbuy tbody tr').each(function (index, item) {
                var element = $(this)
                var from = parseFloat(element.find('.td-from').find('input').val().replaceAll(',', ''))
                var to = parseFloat(element.find('.td-to').find('input').val().replaceAll(',', ''))
                if (from == undefined || isNaN(from) || from <= 0 || to == undefined || isNaN(to) || to <= 0) {
                    _msgalert.error('Vui lòng nhập đầy đủ số lượng sản phẩm')
                    success = false
                    return false
                }
                if (from >= to) {
                    _msgalert.error('Khoảng số lượng không hợp lệ')
                    success = false
                    return false
                }
                var checkbox_value = element.find('input[name="discount-type-' + (index) + '"]:checked').val()
                if (checkbox_value == undefined || checkbox_value.trim() == '') {
                    _msgalert.error('Vui lòng chọn loại chiết khấu')
                    success = false
                    return false
                }
                var number = parseFloat(element.find('.discount-number').find('input').val().replaceAll(',', ''))
                if ((number == undefined || isNaN(number) || number <= 0) && checkbox_value == 0) {
                    _msgalert.error('Vui lòng nhập Chiết khấu')
                    success = false
                    return false
                }
                var percent = parseFloat(element.find('.discount-percent').find('input').val().replaceAll(',', ''))
                if ((percent == undefined || isNaN(percent) || percent <= 0) && checkbox_value == 1) {
                    _msgalert.error('Vui lòng nhập Chiết khấu')
                    success = false
                    return false
                }
            })
        }
        if (!success) return success


        return success
    },
    GetAttributeItem: function () {
        var model = {
            attributes: [],
            attributes_detail: []
        }
        $('.attributes-list').each(function (index, item) {
            var element = $(this)

            model.attributes.push({
                _id: index,
                name: element.find('h6').find('input').val(),
            })
            element.find('.attributes-detail').each(function (index_2, item_2) {
                var element_detail = $(this)
                var img_src = element_detail.find('.choose-content').find('img').attr('src')
                if (img_src != undefined && _product_function.CheckIfImageVideoIsLocal(img_src)) {
                    var result = _product_function.POSTSynchorus('/Product/SummitImages', { data_image: img_src })
                    if (result != undefined && result.data != undefined && result.data.trim() != '') {
                        img_src = result.data
                    }
                }
                var value = element_detail.find('.relative').find('input').val()
                if (value != undefined && value.trim() != '') {
                    model.attributes_detail.push({
                        attribute_id: index,
                        img: img_src == undefined ? '' : img_src,
                        name: value
                    })
                }
            })
        })

        return model
    },
    RenderSelectedGroupProduct: function () {
        var html_selected_input = ''
        var group_selected = ''
        var old_group_product_selected_text = $('#group-id input').val()
        var old_group_product_selected_id = $('#group-id input').attr('data-id')
        $('#them-nganhhang .col-md-4').each(function (index, item) {
            var element = $(this)
            var selected = element.find('ul').find('.active').attr('data-name')
            if (element.find('ul').find('.active').attr('data-id') == undefined) return true
            if (index >= ($('#them-nganhhang .col-md-4').length - 1)) {
                html_selected_input += selected
                group_selected += element.find('ul').find('.active').attr('data-id')
            } else {

                html_selected_input += selected + ' > '
                group_selected += element.find('ul').find('.active').attr('data-id') + ','

            }
        })
        $('#group-id input').val(html_selected_input)
        $('#group-id input').attr('data-id', group_selected)
        $('#group-id input').attr('data-group-count', $('#them-nganhhang .col-md-4').length)
        $('#group-id input').attr('data-old-text', old_group_product_selected_text)
        $('#group-id input').attr('data-old-id', old_group_product_selected_id)

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
    Select2Label: function (element) {
        element.select2({
            ajax: {
                url: "/Label/SearchLabel",
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
                                text: ((item.labelCode == null || item.labelCode == undefined || item.labelCode.trim() == '') ? '' : (item.labelCode + ' - ')) + ' ' + item.labelName,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    Select2Spec: function (selector) {
        selector.each(function (index, item) {
            var element = $(this)
            element.select2({
                ajax: {
                    url: "/product/SpecificationKeySearch",
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
                                    text: item.description,
                                    id: item.description,
                                }
                            })
                        };
                    },
                    cache: true
                }
            });
        })

    },
    ActiveProduct: function () {
        _global_function.AddLoading()

        var model = {
            product_id: $('#product_detail').attr('data-id') == undefined || $('#product_detail').attr('data-id').trim() == '' ? null : $('#product_detail').attr('data-id'),
        }
        _product_function.POST('/Product/ConfirmActiveProduct', model, function (result) {
            if (result.is_success) {
                _global_function.RemoveLoading()
                _msgalert.success(result.msg)
                setTimeout(function () {
                    window.location.href = '/product';
                }, 2000);
            }
            else {
                _global_function.RemoveLoading()

                _msgalert.error(result.msg)

            }
        });
    },
    CalucateDiscount: function () {
        var min_price = $('#main-amount input').val() == undefined || $('#main-amount input').val().trim() == '' ? 0 : parseFloat($('#main-amount input').val().replaceAll(',', ''));
        var old_price = $('#old-price input').val() == undefined || $('#old-price input').val().trim() == '' ? 0 : parseFloat($('#old-price input').val().replaceAll(',', ''));
        if (!$('#product-attributes-table').is(':hidden')) {
            min_price = -1
            $('#product-attributes-prices tbody tr').each(function (index, index) {
                var element = $(this)
                var amount = element.find('.td-amount').find('input').val() == undefined || element.find('.td-amount').find('input').val().trim() == '' ? 0 : parseFloat(element.find('.td-amount').find('input').val().replaceAll(',', ''))
                if (min_price < 0 || min_price > amount) {
                    min_price = amount
                }
            })
        }
        var discount_value = ((old_price - min_price) / old_price) * 100
        var discount = Math.round(discount_value <= 0 ? 0 : discount_value)
        if (discount == undefined|| isNaN(discount)) discount = 0
        $('#discount input').val(discount).trigger('change')
    },
    AddNewProductBuyWith: function () {
        $('#add-product-buy-with').show()
        $('#add-product-buy-with').addClass('show')
        product_detail_new.ProductBuyWithSearch()
    },
    CloseAddNewProductBuyWith: function () {
        $('#add-product-buy-with').hide()
        $('#add-product-buy-with').removeClass('show')
    },
    RenderProductBuyWith: function () {
        var model = {
            id: $('#product_detail').attr('data-id')
        }
        _product_function.POST('/Product/ProductBuyWith', model, function (result) {
            $('body').append(result)
            product_detail_new.Select2BuyWith($('#add-product-buy-with-search-group'))
        });
    },
    ProductBuyWithSearch: function () {
        var group_selected = $('#add-product-buy-with-search-group') == undefined ? '-1' : $('#add-product-buy-with-search-group').find(':selected').val()
        var model = {
            keyword: $('#add-product-buy-with-search-name').val(),
            group_id: group_selected == undefined ? '-1' : group_selected,
            current_id: [
                $('#product_detail').attr('data-id')
            ]
        }
        _product_function.POST('/Product/ProductBuyWithSearch', model, function (result) {
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
                                        <td style="max-width: 50%;">
                                            <div class="item-order text-left" style=" display: flex; ">
                                                <div class="img" style=" margin-right: 5px; width:80px;">
                                                    <img src="@img_src" alt="" style=" width: 80px; ">
                                                </div>
                                                <div class="info">
                                                    <h6 class="name-product"> @item.name </h6>
                                                    <p class="text-secondary mb-0">Mã: @item.code</p>
                                                    <p class="text-secondary mb-0">Phân loại: <nw class="product-variation"> @variation_string</nw></p>

                                                </div>
                                            </div>
                                        </td>
                                        <td>
                                            đ @(item.amount_min == null ? item.amount.ToString("N0") : ((double)item.amount_min).ToString("N0"))
                                        </td>
                                        <td>@item.quanity_of_stock</td>
                                        <td class="text-center">
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


    },
    SyncES: function () {
        _product_function.POST('/Product/SyncES', {}, function (result) {
            if (result.is_success) {
                _msgalert.success('Sync ES Successfully')
            }
            else {
                _msgalert.error('Sync ES Failed')
            }
        });
    }
}