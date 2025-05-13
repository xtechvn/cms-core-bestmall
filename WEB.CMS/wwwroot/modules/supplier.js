
var _supplier_service = {
    Init: function () {
        this.modal_element = $('#global_modal_popup');
        this.OnSearch();
    },

    GetParam: function () {
        let services = $('#sl_search_service').val();
        //let provinces = $('#sl_search_province').val();
        //let stars = $('#sl_search_star').val();
        //let brands = $('#sl_search_brand').val();
        let users = $('#sl_search_suggest_user').val();

        let objSearch = {
            FullName: $('#ip_search_fullname').val() != undefined ? $('#ip_search_fullname').val().trim().replaceAll(/  +/g, ' ') : null,
            ServiceType: services != null ? services.join(',') : "",
            //ProvinceId: provinces != null ? provinces.join(',') : "",
            //RatingStar: stars != null ? stars.join(',') : "",
            //ChainBrands: brands != null ? brands.join(',') : "",
            SalerId: users != null ? users.join(',') : "",
            currentPage: 1,
            pageSize: 10
        }
        return objSearch;
    },

    Search: function (input) {
        window.scrollTo(0, 0);
        $('#imgLoading').show();
        $.ajax({
            url: "/Supplier/Search",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid_data').html(result);
            }
        });
    },

    OnSearch: function () {
        let objSearch = this.GetParam();
        this.SearchParam = objSearch;
        this.Search(objSearch);
    },

    OnPaging: function (value) {
        var objSearch = this.GetParam()
        objSearch.currentPage = value;
        this.SearchParam = objSearch
        this.Search(objSearch);
    },

    ReLoad: function () {
        this.Search(this.SearchParam);
    },

    OnChangeFullNameSearch: function (value) {
        var searchobj = this.SearchParam;
        searchobj.FullName = value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeServiceSearch: function (value) {
        var searchobj = this.SearchParam;
        searchobj.ServiceType = value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeSalerSearch: function (value) {
        var searchobj = this.SearchParam;
        searchobj.SalerId = value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    GetFormData: function ($form) {
        var unindexed_array = $form.serializeArray();
        var indexed_array = {};

        $.map(unindexed_array, function (n, i) {
            indexed_array[n['name']] = n['value'];
        });

        return indexed_array;
    },

    Export: function () {
        $('#btnExport').prop('disabled', true);
        $('#icon-export').removeClass('fa-file-excel-o');
        $('#icon-export').addClass('fa-spinner fa-pulse');
        var objSearch = this.GetParam()
        objSearch.currentPage = 1;
        objSearch.pageSize = 100000000;
        this.SearchParam = objSearch
        $.ajax({
            url: "/Supplier/ExportExcel",
            type: "Post",
            data: this.SearchParam,
            success: function (result) {
                $('#btnExport').prop('disabled', false);
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.message);
                }
                $('#icon-export').removeClass('fa-spinner fa-pulse');
                $('#icon-export').addClass('fa-file-excel-o');
            }
        });
    },

    ShowAddOrUpdate: function (id) {
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} nhà cung cấp`;
        let url = '/Supplier/AddOrUpdate';

        $('#global_modal_popup').find('.modal-title').html(title);
        $('#global_modal_popup').find('.modal-dialog').css('max-width', '1200px');

        _ajax_caller.get(url, { id: id }, function (result) {

            _supplier_service.modal_element.find('.modal-title').html(title);
            _supplier_service.modal_element.find('.modal-body').html(result);
            _supplier_service.modal_element.modal('show');
            $('.modal-backdrop').css('z-index', '0')

        });
    },

    OnAdd: function () {
        let Form = $('#form_supplier');
        Form.validate({
            rules: {
                FullName: "required",
                Email: {
                    email: true
                },
                Phone: {
                    minlength: 10,
                    maxlength: 11,
                    digits: true
                }
                //ContactName: "required",
                //ContactPhone: {
                //    required: true,
                //    exactlength: 10,
                //    digits: true
                //},
                //ContactEmail: {
                //    email: true
                //},
                //BankAccountName: "required",
                //BankAccountNumber: {
                //    required: true,
                //    maxlength: 20,
                //    digits: true
                //},
                //BankId: "required"
            },
            messages: {
                FullName: "Vui lòng nhập tên nhà cung cấp",
                Email: {
                    email: 'Email không đúng định dạng'
                },
                Phone: {
                    exactlength: "Số điện thoại phải nhập đúng 10 / 11 kí tự dạng số",
                    digits: "Số điện thoại phải là kí tự dạng số"
                }
                //ContactName: "Vui lòng nhập tên liên hệ",
                //ContactPhone: {
                //    required: "Vui lòng nhập số điện thoại",
                //    exactlength: "Số điện thoại phải nhập đúng 10 kí tự dạng số",
                //    digits: "Số điện thoại phải là kí tự dạng số"
                //},
                //ContactEmail: {
                //    email: 'Email không đúng định dạng'
                //},
                //BankAccountName: "Vui lòng nhập chủ tài khoản ngân hàng",
                //BankAccountNumber: {
                //    required: "Vui lòng nhập số tài khoản",
                //    maxlength: "Số tài khoản chỉ chứa tối đa 20 kí tự",
                //    digits: "Số tài khoản phải là kí tự dạng số"
                //},
                //BankId: "Vui lòng nhập tên ngân hàng"
            }
        });

        if (!Form.valid()) { return; }

        let formData = this.GetFormData(Form);

        formData['SupplierCode'] = 'SUPPLIER_CODE';

        let url = formData.SupplierId > 0 ? "/Supplier/Update" : "/Supplier/Create";
        _global_function.AddLoading()
        _ajax_caller.post(url, { model: formData }, function (result) {
            _global_function.RemoveLoading()

            if (result.isSuccess) {
                _msgalert.success(result.message);
                _supplier_service.modal_element.modal('hide');
                _supplier_service.ReLoad();
            } else {
                _msgalert.error(result.message);
            }

        });
    },

    ShowHideColumn: function () {
        $('.checkbox-tb-column').each(function () {
            let seft = $(this);
            let id = seft.data('id');
            if (seft.is(':checked')) {
                $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
            } else {
                $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
            }
        });
    },

    checkShowHide: function () {
        if (fields.STT === true) {
            $('#STT').prop('checked', true);
        } else {
            $('#STT').prop('checked', false);
        }
        if (fields.SupplierId === true) {
            $('#SupplierId').prop('checked', true);
        } else {
            $('#SupplierId').prop('checked', false);
        }
        if (fields.SupplierName === true) {
            $('#SupplierName').prop('checked', true);
        } else {
            $('#SupplierName').prop('checked', false);
        }
        if (fields.EstablistYear === true) {
            $('#EstablistYear').prop('checked', true);
        } else {
            $('#EstablistYear').prop('checked', false);
        }
        if (fields.Address === true) {
            $('#Address').prop('checked', true);
        } else {
            $('#Address').prop('checked', false);
        }
        if (fields.Contact === true) {
            $('#Contact').prop('checked', true);
        } else {
            $('#Contact').prop('checked', false);
        }
        if (fields.Service === true) {
            $('#Service').prop('checked', true);
        } else {
            $('#Service').prop('checked', false);
        }
        if (fields.SalerId === true) {
            $('#SalerId').prop('checked', true);
        } else {
            $('#SalerId').prop('checked', false);
        }
        if (fields.CreateBy === true) {
            $('#CreateBy').prop('checked', true);
        } else {
            $('#CreateBy').prop('checked', false);
        }
        if (fields.CreateDate === true) {
            $('#CreateDate').prop('checked', true);
        } else {
            $('#CreateDate').prop('checked', false);
        }
        if (fields.UpdatedBy === true) {
            $('#UpdatedBy').prop('checked', true);
        } else {
            $('#UpdatedBy').prop('checked', false);
        }
        if (fields.UpdatedDate === true) {
            $('#UpdatedDate').prop('checked', true);
        } else {
            $('#UpdatedDate').prop('checked', false);
        }
    },

    ChangeSetting: function (position) {
        this.ShowHideColumn();
        switch (position) {
            case 1:
                if ($('#STT').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 2:
                if ($('#SupplierId').is(":checked")) {
                    fields.SupplierId = true
                } else {
                    fields.SupplierId = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 3:
                if ($('#SupplierName').is(":checked")) {
                    fields.SupplierName = true
                } else {
                    fields.SupplierName = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 4:
                if ($('#EstablistYear').is(":checked")) {
                    fields.EstablistYear = true
                } else {
                    fields.EstablistYear = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 5:
                if ($('#Address').is(":checked")) {
                    fields.Address = true
                } else {
                    fields.Address = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 6:
                if ($('#Contact').is(":checked")) {
                    fields.Contact = true
                } else {
                    fields.Contact = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 7:
                if ($('#Service').is(":checked")) {
                    fields.Service = true
                } else {
                    fields.Service = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;

            case 8:
                if ($('#SalerId').is(":checked")) {
                    fields.SalerId = true
                } else {
                    fields.SalerId = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 9:
                if ($('#CreateBy').is(":checked")) {
                    fields.CreateBy = true
                } else {
                    fields.CreateBy = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;

            case 10:
                if ($('#CreateDate').is(":checked")) {
                    fields.CreateDate = true
                } else {
                    fields.CreateDate = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 11:
                if ($('#UpdatedBy').is(":checked")) {
                    fields.UpdatedBy = true
                } else {
                    fields.UpdatedBy = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;

            case 12:
                if ($('#UpdatedDate').is(":checked")) {
                    fields.UpdatedDate = true
                } else {
                    fields.UpdatedDate = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            default:
        }
    },

    setCookie: function (name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + (value || "") + expires + "; path=/";
    },

    getCookie: function (name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },

    eraseCookie: function (name) {
        document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    },

    saveCookieFilter: function () {
        this.setCookie(cookieFilterName, JSON.stringify(this.getModel()), 1)
    },
}

var _changeInterval = null;
$("#ip_search_fullname").keyup(function (e) {
    if (e.which === 13) {
        _supplier_service.OnChangeFullNameSearch(e.target.value);
    } else {
        clearInterval(_changeInterval);
        _changeInterval = setInterval(function () {
            _supplier_service.OnChangeFullNameSearch(e.target.value);
            clearInterval(_changeInterval);
        }, 1000);
    }
});

$('#sl_search_service').change(function () {
    let values = $(this).val();
    let str_values = values != null ? values.join(',') : "";
    _supplier_service.OnChangeServiceSearch(str_values);
});

$('#sl_search_suggest_user').change(function () {
    let values = $(this).val();
    let str_values = values != null ? values.join(',') : "";
    _supplier_service.OnChangeSalerSearch(str_values);
});

$('.select-service-type').click(function (e) {
    var seft = $(this);
    seft.toggleClass("open");
});

$(document).ready(function () {
    $('input').attr('autocomplete', 'off');
    _supplier_service.Init();

    $("#sl_search_service").select2({
        placeholder: "Tất cả dịch vụ",
        multiple: true
    });

    $("#sl_search_suggest_user").select2({
        //theme: 'bootstrap4',
        placeholder: "Người tạo",
        multiple: true,
        maximumSelectionLength: 3,
        ajax: {
            url: "/Order/UserSuggestion",
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
                            text: `${item.fullname} - ${item.username}`,
                            id: item.id,
                        }
                    })
                };
            }
        }
    });

});
