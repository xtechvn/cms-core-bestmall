
$(document).ready(function () {
    product_index.Initialization()
})
var product_index = {
    Model: {
        keyword: '',
        group_id: -1,
        page_index: 1,
        page_size: 10,
        reached_end: false,
        on_excuting: false
    },
    Initialization: function () {
        var model = [{ url: '/', name: 'Trang chủ' }, { url: '/product', name: 'Quản lý sản phẩm', activated: true }]
        _global_function.RenderBreadcumb(model)
        product_index.Listing();
        product_index.DynamicBind()
        $('#product_list').closest('.table-responsive').addClass('placeholder')
        $('.hanmuc').closest('.flex-lg-nowrap').addClass('placeholder')
        product_index.Select2Group($('#search-group'))
    },
    DynamicBind: function () {
        $('body').on('click', '.btn-search-product', function () {
            $('#product_list').html('')
            product_index.ResetSearch()
            product_index.Listing();
        });

        $("#input-search-product-name").on('keyup', function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                if (product_index.Model.reached_end == false) {
                    $('#product_list').html('')
                    product_index.ResetSearch()

                    product_index.Listing();
                }
            }
        });
        $('body').on('click', '.btn-add-product', function () {
            window.location.href = '/product/detail'
        });

        $('body').on('click', '.product-edit, .name-product', function () {
            var element = $(this)
            var product_id = element.closest('tr').attr('data-id')
            if (product_id != null && product_id != undefined && product_id.trim() != '') {
                window.location.href = '/product/detail/' + product_id

            }
        });

        $('body').on('click', '.product-remove-sp', function () {
            var element = $(this)
            var product_id = element.closest('tr').attr('data-id')

            var title = 'Xác nhận ẩn sản phẩm';
            var description = 'Bạn có chắc chắn muốn ẩn sản phẩm này?';

            _msgconfirm.openDialog(title, description, function () {
                if (product_id != null && product_id != undefined && product_id.trim() != '') {
                    _product_function.POST('/Product/UpdateProductStatus', { product_id: product_id, status: 2 }, function (result) {
                        if (result.is_success) {
                            _msgalert.success('Ẩn sản phẩm thành công')
                            setTimeout(function () {
                                window.location.href = '/product'
                            }, 1500);
                        }
                        else {
                            _msgalert.error(result.msg)
                        }
                    });
                }

            });

        });
        $('body').on('click', '.product-open-sp', function () {
            var element = $(this)
            var product_id = element.closest('tr').attr('data-id')

            var title = 'Xác nhận hiển thị sản phẩm';
            var description = 'Bạn có chắc chắn muốn hiển thị sản phẩm này?';

            _msgconfirm.openDialog(title, description, function () {
                if (product_id != null && product_id != undefined && product_id.trim() != '') {
                    _product_function.POST('/Product/UpdateProductStatus', { product_id: product_id, status: 1 }, function (result) {
                        if (result.is_success) {
                            _msgalert.success('Hiển thị sản phẩm thành công')
                            setTimeout(function () {
                                window.location.href = '/product'
                            }, 1500);
                        }
                        else {
                            _msgalert.error(result.msg)
                        }
                    });
                }

            });

        });
        $('body').on('click', '.product-remove-sp2', function () {
            var element = $(this)
            var product_id = element.closest('tr').attr('data-id')
            var title = 'Xác nhận xóa sản phẩm';
            var description = 'Bạn có chắc chắn muốn xóa sản phẩm này?';
            _msgconfirm.openDialog(title, description, function () {
                if (product_id != null && product_id != undefined && product_id.trim() != '') {
                    _product_function.POST('/Product/UpdateProductStatus', { product_id: product_id, status: 3 }, function (result) {
                        if (result.is_success) {
                            _msgalert.success('Xóa sản phẩm thành công')
                            setTimeout(function () {
                                window.location.href = '/product'
                            }, 1000);
                        }
                        else {
                            _msgalert.error(result.msg)
                        }
                    });
                }
            });
        });
        $('body').on('click', '.product-copy-sp', function () {
            var element = $(this)
            var product_id = element.closest('tr').attr('data-id')
            var title = 'Xác nhận sao chép sản phẩm';
            var description = 'Bạn có chắc chắn muốn sao chép sản phẩm này?';
            _msgconfirm.openDialog(title, description, function () {
                if (product_id != null && product_id != undefined && product_id.trim() != '') {
                    _product_function.POST('/Product/CopyProductByID', { product_id: product_id }, function (result) {
                        if (result.is_success) {
                            _msgalert.success('Sao chép sản phẩm thành công')
                            setTimeout(function () {
                                window.location.href = '/product'
                            }, 1000);
                        }
                        else {
                            _msgalert.error(result.msg)
                        }
                    });
                }
            });

        });
        $('body').on('click', '.product-copy', function () {
            var element = $(this)
            var product_id = element.closest('tr').attr('data-id')
            if (product_id != null && product_id != undefined && product_id.trim() != '') {
                window.location.href = '/product/CopyProductByID/' + product_id

            }
        });
        $('body').on('click', '.sub-product-viewmore .xemthem', function () {
            var max_show_per_click = 5;
            var element = $(this)
            var data_id = element.closest('.sub-product-viewmore').attr('data-main-id');
            var max_sub = element.closest('.sub-product-viewmore').attr('data-count');
            var max_sub_value = 0;
            if (max_sub != undefined && max_sub.trim() != '' && !isNaN(parseInt(max_sub)) && parseInt(max_sub)>0) {
                max_sub_value = parseInt(max_sub)
            }
            var count_show = 0;
            $('#product_list .sub-product').each(function (index, item) {
                var element_compare = $(this)
                if (count_show > max_show_per_click) return false;
                else if (element_compare.hasClass('sub-product-viewmore') || element_compare.hasClass('sub-product-collapse')) return true;
                else  if (element_compare.attr('data-main-id') != undefined && element_compare.attr('data-main-id') == data_id) {
                    if (element_compare.is(':hidden')) {
                        element_compare.show()
                    }
                    count_show++;

                }
            })
            if (count_show >= max_sub_value) {
                element.find('span').html(`
                                Thu gọn
                                <i class="icofont-simple-up"></i>

                `)
                element.closest('.sub-product-viewmore').addClass('sub-product-collapse')
                element.closest('.sub-product-viewmore').removeClass('sub-product-viewmore')
            } else {
                element.find('span').html(`
                Xem thêm (còn <nw class="remain-sub">`+ (max_sub_value - count_show)+`</nw>  phân loại)
                                <i class="icofont-simple-down"></i>

                `)
            }
        });
        $('body').on('click', '.sub-product-collapse .xemthem', function () {
            var max_show_per_click = 2;
            var element = $(this)
            var data_id = element.closest('.sub-product-collapse').attr('data-main-id');
            var max_sub = element.closest('.sub-product-collapse').attr('data-count');
            var max_sub_value = 0;
            if (max_sub != undefined && max_sub.trim() != '' && !isNaN(parseInt(max_sub)) && parseInt(max_sub) > 0) {
                max_sub_value = parseInt(max_sub)
            }
            var count_show = 0;
            $('#product_list .sub-product').each(function (index, item) {
                var element_compare = $(this)
                if (element_compare.hasClass('sub-product-viewmore') || element_compare.hasClass('sub-product-collapse')) return true;
                else if (element_compare.attr('data-main-id') != undefined && element_compare.attr('data-main-id') == data_id) {
                    if (!element_compare.is(':hidden') && count_show < max_show_per_click) {
                        count_show++;
                        return true;
                    }
                    element_compare.hide()
                }
            })
            element.find('span').html(`
                Xem thêm (còn <nw class="remain-sub">`+ (max_sub_value - count_show) + `</nw>  phân loại)
                                <i class="icofont-simple-down"></i>

                `)
            element.closest('.sub-product-collapse').addClass('sub-product-viewmore')
            element.closest('.sub-product-collapse').removeClass('sub-product-collapse')

        });
        //-- Import Excel:
        $('body').on('click', '.product-import', function () {
            var element = $(this)
            if (element.find('.box-action').is(':hidden')) {
                element.find('.box-action').fadeIn()
            } else {
                element.find('.box-action').fadeOut()
            }
        });
        $('body').on('click', '.product-import-add', function () {
            var title = 'Thêm sản phẩm hàng loạt ';
            let url = '/Product/ImportExcel';
            let param = {

            };

            _magnific.OpenSmallPopup(title, url, param);
        });
        $('body').on('select2:select', '#item-per-page', function () {
            product_index.ResetSearch()

            product_index.Listing();
        });
        //--scroll event
        //$(window).scroll(function () {
        //    if ($(window).scrollTop() >= $('.main-products table').offset().top + $('.main-products table').outerHeight() - window.innerHeight) {
        //        product_index.Listing()
        //    }
        //});
        //$('#product_list').on('scroll', function () {
        //    if ($(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight) {
        //        product_index.Listing();
        //    }
        //});
        $('body').on('click', '.product-search-tab', function () {
            var element = $(this)
            $('.product-search-tab').removeClass('active')
            element.addClass('active')
            product_index.ResetSearch()
            product_index.Listing();
        });
        $('body').on('click', '#btn-search-product-clear-search', function () {
            _msgconfirm.openDialog('Xác nhận xóa bộ lọc', 'Bộ lọc sản phẩm sẽ được đặt về giá trị mặc định, bạn chắc chắn không?', function () {
                $('#search-group').val('0').trigger('change')
                $('#input-search-product-name').val('').trigger('change')
                //$('.product-search-tab').first().trigger('click')

                product_index.ResetSearch()
                product_index.Listing();
            })

        });
        $('body').on('click', '#export-excel', function () {
            var element = $(this)
            _msgconfirm.openDialog('Xác nhận xuất file Excel', 'Danh sách sản phẩm theo bộ lọc sẽ được xuất ra file Excel, bạn có chắc chắn không?', function () {
                element.prop('disabled', true);
                element.html('<i id="icon-export" class="icofont-file-excel"></i> Vui lòng chờ...');
                $('#icon-export').removeClass('icofont-file-excel');
                var request = product_index.GetSearchModel()
                _product_function.POST('/Product/ExportExcel', request, function (result) {

                    _global_function.RemoveLoading()
                    element.prop('disabled', false);
                    element.html('<i id="icon-export" class="icofont-file-excel"></i>Xuất Excel');
                    if (result.is_success) {
                        _msgalert.success(result.msg);
                        window.location.href = result.path;
                    } else {
                        _msgalert.error(result.msg);
                    }
                    //$('#icon-export').addClass('icofont-file-excel');
                })
            })
        });
    },
    ResetSearch: function () {
        product_index.Model.page_index = 1;
        product_index.Model.page_index = 1;
        product_index.Model.on_excuting = false;
        product_index.Model.reached_end = false;
        if ($('.count').attr('data-value') == undefined || $('.count').attr('data-value') <= 0)
            $('.count').attr('data-value', '0')
        $('.count').text('0')
        $('#product_list').closest('.table-responsive').addClass('placeholder')
        $('.hanmuc').closest('.flex-lg-nowrap').addClass('placeholder')
        $('#product_list').html('')
    },

    Listing: function () {
        if (product_index.Model.reached_end == true || product_index.Model.on_excuting == true) {
            return;
        }
        product_index.Model.on_excuting = true
        //function normalizeText(input) {
        //    return input
        //        .normalize("NFC")

        //        //.replace(/[()]/g, "")             // Loại bỏ dấu ngoặc đơn
        //        .replace(/\s+/g, ' ')             // Xóa khoảng trắng thừa
        //        .trim();
        //}
        //var active_tab = $('#product-search-tab-container .active');
        //var status = -1;

        //if (active_tab != null && active_tab != undefined) {
        //    status = active_tab.attr('data-status')
        //}
        //var group_id = $('#search-group').find(':selected');
        //var group_id_value = '-1';
        //if (group_id != null && group_id != undefined) {
        //    group_id_value = group_id.val()
        //}

        //var request = {
        //    keyword: normalizeText($('#input-search-product-name').val()), // Làm sạch từ khóa
        //    group_id: group_id_value,
        //    page_index: product_index.Model.page_index,
        //    page_size: parseInt($('#item-per-page').find(':selected').val()),
        //    status: status
        //}
        var request = product_index.GetSearchModel()
        _product_function.POST('/Product/Search', request, function (result) {

            $('#product_list').html(result)
            $('#product_list').closest('.table-responsive').removeClass('placeholder')
            $('.hanmuc').closest('.flex-lg-nowrap').removeClass('placeholder')
            product_index.Model.on_excuting = false
            product_index.Model.page_index++
            var product_count = $('#search-count').val()
            var current_count = $('.count').attr('data-value')
            if (current_count == undefined) current_count = '0';
            if (product_count == undefined) product_count = '0';
            if (result.total_count == undefined || result.total_count <= 0) {
                $('.count').attr('data-value', (parseFloat(product_count) + parseFloat(current_count)))
            }
            $('.count').text(result.total_count || (parseFloat(product_count) + parseFloat(current_count)));
            $('.hanmuc').closest('.flex-lg-nowrap').find('.count').html(parseFloat(product_count))
            if (product_count != null && product_count != undefined && product_count.trim() != '') {
                $('#count-product').attr('data-value', product_count)
                $('#count-product').html(product_count)
            }
            $('#search-count').closest('tr').remove()
        })
        //_product_function.POST('/Product/ProductListing', request, function (result) {
        //    if (result.is_success && result.data && result.data.length > 0 && JSON.parse(result.data).length > 0) {
        //        product_index.RenderSearch(JSON.parse(result.data), JSON.parse(result.subdata))
        //        // Gán tổng số sản phẩm vào phần tử với class 'count'
        //        var current_count = $('.count').attr('data-value')
        //        if (current_count == undefined) current_count = '0';
        //        if (result.total_count == undefined || result.total_count <= 0) {
        //            $('.count').attr('data-value', (JSON.parse(result.data).length + parseFloat(current_count)))
        //        }
        //        $('.count').text(result.total_count || (JSON.parse(result.data).length + parseFloat(current_count)));
        //        $('.hanmuc').closest('.flex-lg-nowrap').find('.count').html(JSON.parse(result.data).length)

        //    }
        //    else {
        //        product_index.Model.reached_end = true
        //    }
        //    $('#product_list').closest('.table-responsive').removeClass('placeholder')
        //    $('.hanmuc').closest('.flex-lg-nowrap').removeClass('placeholder')
        //    product_index.Model.on_excuting = false
        //    product_index.Model.page_index++

        //});

    },
    Select2Group: function (element) {
        var element_placeholder = element.attr('placeholder')
        element.select2({
            placeholder: element_placeholder,
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
    GetSearchModel: function () {
        
        var active_tab = $('#product-search-tab-container .active');
        var status = -1;

        if (active_tab != null && active_tab != undefined) {
            status = active_tab.attr('data-status')
        }
        var group_id = $('#search-group').find(':selected');
        var group_id_value = '-1';
        if (group_id != null && group_id != undefined) {
            group_id_value = group_id.val()
        }

        var request = {
            keyword: product_index.normalizeText($('#input-search-product-name').val()), // Làm sạch từ khóa
            group_id: group_id_value,
            page_index: product_index.Model.page_index,
            page_size: parseInt($('#selectPaggingOptions').find(':selected').val()),
            status: status
        }
        return request
    },
    normalizeText: function (input) {
        return input
            .normalize("NFC")

            //.replace(/[()]/g, "")             // Loại bỏ dấu ngoặc đơn
            .replace(/\s+/g, ' ')             // Xóa khoảng trắng thừa
            .trim();
    },
    OnPaging: function (value) {
        pageSize = 1;
        if (value > 0) {
            product_index.Model.page_index = value;
            this.Listing();
        }
    },
    onSelectPageSize: function () {
        product_index.Model.page_index = 1;
        this.Listing();
    },
}
