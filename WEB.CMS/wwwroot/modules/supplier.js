
var _supplier_service = {
    Init: function () {
        this.modal_element = $('#global_modal_popup');
        this.OnSearch();
        _supplier_service.DynamicBind()
    },
    DynamicBind: function () {
        $('body').on('change', '#banner-main input,#banner-sub input ', function () {
            var element = $(this)
            _supplier_service.AddImage(element)
        })
        $('body').on('click', '#banner-main .col-md-3 .delete,#banner-sub .col-md-3 .delete ', function () {
            var element = $(this)
            element.closest('.col-md-3').find('.magnific_popup').hide()
            element.closest('.col-md-3').find('.choose').show()
            element.closest('.col-md-3').find('.magnific_popup').find('img').attr('src', '')

        })
    },
    GetParam: function () {
        let services = $('#sl_search_service').val();
        //let provinces = $('#sl_search_province').val();
        //let stars = $('#sl_search_star').val();
        //let brands = $('#sl_search_brand').val();
        let users = $('#sl_search_suggest_user').val();

        let objSearch = {
            FullName: $('#ip_search_fullname').val() != undefined ? $('#ip_search_fullname').val().trim().replaceAll(/  +/g, ' ') : null,
           // ServiceType: services != null ? services.join(',') : "",
            //ProvinceId: provinces != null ? provinces.join(',') : "",
            //RatingStar: stars != null ? stars.join(',') : "",
            //ChainBrands: brands != null ? brands.join(',') : "",
            //SalerId: users != null ? users.join(',') : "",
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
            _supplier_service.Select2Location()
        });
    },

    OnAdd: function () {
        let Form = $('#form_supplier');
        // 1. Thêm phương thức validation tùy chỉnh
        $.validator.addMethod("noSpecialChars", function (value, element) {
            // Regex cho phép chữ cái tiếng Việt có dấu, chữ số, và dấu cách
            // Thay đổi Regex này tùy theo nhu cầu của bạn
            const regex = /^[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝĐàáâãèéêìíòóôõùúýđẢẠẮẶẤẬẦẨẪẬẺẸẼỀẾỆỈỊỎỌỒỔỖỘƠỜỚỞỠỢƯỪỨỬỮỰỲỴỶỸ\s0-9]+$/;

            // this.optional(element) cho phép trường rỗng nếu nó không phải là required
            return this.optional(element) || regex.test(value);
        }, "Tên nhà cung cấp không được chứa ký tự đặc biệt.");
        Form.validate({
            rules: {
                "FullName": {
                    required: true,
                    noSpecialChars: true
                },
                "Email": {
                    required: true,
                    email: true
                },
                "Phone": {
                    minlength: 10,
                    maxlength: 11,
                    digits: true
                },
                //ContactName: "required",
                //ContactPhone: {
                //    required: true,
                //    exactlength: 10,
                //    digits: true
                //},
                //ContactEmail: {
                //    required: true,
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
                "FullName": {
                    required: "Vui lòng nhập tên nhà cung cấp",
                    noSpecialChars: "Tên nhà cung cấp không được chứa ký tự đặc biệt"
                },
                "Email": {
                    required: "Vui lòng nhập Email",
                    email: 'Email không đúng định dạng'
                },
                "Phone": {
                    exactlength: "Số điện thoại phải nhập đúng 10 / 11 kí tự dạng số",
                    maxlength: "Số điện thoại phải nhập đúng 10 / 11 kí tự dạng số",
                    minlength: "Số điện thoại phải nhập đúng 10 / 11 kí tự dạng số",
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
        var province = $('#supplier-province').find(':selected')
        if (province == null || province == undefined || province.length<=0) {
            _msgalert.error('', "Vui lòng chọn tỉnh cho địa chỉ kho hàng ");
            $("#supplier-province").get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            return
        }
        var district = $('#supplier-district').find(':selected')
        if (district == null || district == undefined || district.length <= 0) {
            _msgalert.error('', "Vui lòng chọn huyện trong địa chỉ kho hàng");
            $("#supplier-district").get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            return
        }
        var ward = $('#supplier-ward').find(':selected')
        if (ward == null || ward == undefined || ward.length <= 0) {
            _msgalert.error('',"Vui lòng phường / xã trong địa chỉ kho hàng ");
            $("#supplier-ward").get(0).scrollIntoView({ block: 'center', behavior: 'smooth' });
            return
        }
        var status_attachment = 0;
        $('.form-group-attachment').each(function (index, item) {
            var element = $(this)
            var model = {
                files: [],
                data_id: 0,
                type: element.attr('data-type-id')
            }
            var attachment_files = element.find('.attachment-file-gallery').find('.file').length;
            if (index == 0 && (element.find('.attachment-file-gallery').find('.file') == undefined || element.find('.attachment-file-gallery').find('.file').length == undefined || element.find('.attachment-file-gallery').find('.file').length <= 0)) {
                _msgalert.error("Vui lòng thêm giấy tờ thương hiệu");
                status_attachment = 1;
                return false;;
                
            }
            if (index == 1 && (element.find('.attachment-file-gallery').find('.file') == undefined || element.find('.attachment-file-gallery').find('.file').length == undefined || element.find('.attachment-file-gallery').find('.file').length <= 0)) {
                _msgalert.error("Vui lòng thêm giấy tờ hàng hóa");
                status_attachment = 1;
                return false;;
              
            }
            if (index == 2 && (element.find('.attachment-file-gallery').find('.file') == undefined || element.find('.attachment-file-gallery').find('.file').length == undefined || element.find('.attachment-file-gallery').find('.file').length <= 0)) {
                _msgalert.error("Vui lòng thêm giấy tờ nhà phân phối");
                status_attachment = 1;
                return false;;
               
            }
            if (index == 3 && (element.find('.attachment-file-gallery').find('.file') == undefined || element.find('.attachment-file-gallery').find('.file').length == undefined || element.find('.attachment-file-gallery').find('.file').length <= 0)) {
                _msgalert.error("Vui lòng thêm giấy tờ xác nhận");
                status_attachment = 1;
                return false; 
            }
        });
        if (status_attachment == 1) { return; }
        let formData = this.GetFormData(Form);

        formData['SupplierCode'] = 'SUPPLIER_CODE';
        formData['ProvinceId'] = $('#supplier-province').find(':selected').val();
        formData['DistrictId'] = $('#supplier-district').find(':selected').val();
        formData['WardId'] = $('#supplier-ward').find(':selected').val();
        //banking
        formData['BankAccountId'] = $('#bank-id').val();
        formData['BankAccountNumber'] = $('#bank-account-number').val();
        formData['BankId'] = $('#bank-bank-name').val();
        formData['BankAccountName'] = $('#bank-account-name').val();
        formData['BankBranch'] = $('#bank-branch').val();

        var banner_main = []
        $('#banner-main .col-md-3 .magnific_popup').each(function (index, item) {
            var element_image = $(this)
            if (element_image.find('img').length > 0) {
                var data_src = element_image.find('img').attr('src')
                if (data_src == null || data_src == undefined || data_src.trim() == '') return true
                if (_supplier_service.CheckIfImageVideoIsLocal(data_src)) {
                    var result = _supplier_service.POSTSynchorus('/Product/SummitImages', { data_image: data_src })
                    if (result != undefined && result.data != undefined && result.data.trim() != '') {
                        banner_main.push(result.data)
                    } else {
                        banner_main.push(data_src)
                    }
                }
                else {
                    banner_main.push(data_src)
                }
            }
        })
        if (banner_main.length > 0) {
            formData['BannerMain'] = JSON.stringify(banner_main);
        }
        var banner_sub = []
        $('#banner-sub .col-md-3 .magnific_popup').each(function (index, item) {
            var element_image = $(this)
            if (element_image.find('img').length > 0) {
                var data_src = element_image.find('img').attr('src')
                if (data_src == null || data_src == undefined || data_src.trim() == '') return true
                if (_supplier_service.CheckIfImageVideoIsLocal(data_src)) {
                    var result = _supplier_service.POSTSynchorus('/Product/SummitImages', { data_image: data_src })
                    if (result != undefined && result.data != undefined && result.data.trim() != '') {
                        banner_sub.push(result.data)
                    } else {
                        banner_sub.push(data_src)
                    }
                }
                else {
                    banner_sub.push(data_src)
                }
            }
        })
        if (banner_sub.length > 0) {
            formData['BannerSub'] = JSON.stringify(banner_sub);
        }

        let url = formData.SupplierId > 0 ? "/Supplier/Update" : "/Supplier/Create";
        _global_function.AddLoading()
        _ajax_caller.post(url, { model: formData }, function (result) {
            _global_function.RemoveLoading()

            if (result.isSuccess) {
                _supplier_service.ConfirmAttachment(result.data);

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
    ConfirmAttachment: function (id) {
        $('.form-group-attachment').each(function (index, item) {
            var element = $(this)
            var model = {
                files: [],
                data_id: id,
                type: element.attr('data-type-id')
            }
            element.find('.attachment-file-gallery').find('.file').each(function (index, item) {
                var element = $(this)
                model.files.push({
                    id: element.attr('data-id'),
                    path: element.attr('data-path'),
                    ext: element.attr('data-ext')
                })
            });
            $.ajax({
                url: "/AttachFile/ConfirmAttachFile",
                data: model,
                type: "Post",
                success: function (result) {

                }
            });
        });


    },
    ValidateSupplier: function (id, status) {
        var title = "Xác nhận duyệt nhà cung cấp";
        var description = "Nhà cung cấp này sẽ được duyệt, bạn có chắc chắn không?"
        switch (status) {
            case 2:
                {
                    description = "Nhà cung cấp này sẽ bị xóa, bạn có chắc chắn không?"
                }
                break;
            case 0:
                {
                    description = "Bỏ duyệt Nhà cung cấp này sẽ đưa về trạng thái chờ duyệt, bạn có chắc chắn không?"
                }
                break;
            case 1:
                {
                    description = "Nhà cung cấp này sẽ được duyệt, bạn có chắc chắn không?"
                }
                break;
        }
        _msgconfirm.openDialog(title, description, function () {
            var model = {
                data_id: id,
                status: status
            }
            $.ajax({
                url: "/Supplier/ChangetStausSupplier",
                data: model,
                type: "Post",
                success: function (result) {
                    if (result.isSuccess) {
                        $('button').prop('disabled', true)
                        $('button').css('background-color', 'lightgray')
                        $('button').css('border', '1px solid lightgray')
                        _msgalert.success(result.message);
                        setTimeout(function () {
                            window.location.reload();

                        }, 1500)
                    } else {
                        _msgalert.error(result.message);
                    }
                }
            });
        });
    },
    Select2Location: function () {
        var selected_ward = $('#supplier-ward').find(':selected').val()
        var selected_district = $('#supplier-district').find(':selected').val()
        var selected_province = $('#supplier-province').find(':selected').val()
        $('#supplier-ward').select2({
            ajax: {
                url: "/Location/WardSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        keyword: params.term,
                        district_id: $('#supplier-district').find(':selected').val()
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
        $('#supplier-district').select2({
            ajax: {
                url: "/Location/DistrictSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        keyword: params.term,
                        province_id: $('#supplier-province').find(':selected').val()
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
        $('#supplier-province').select2({
            ajax: {
                url: "/Location/ProvinceSuggestion",
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
        if (selected_district != undefined) {
            $('#supplier-district').val(selected_district).trigger('change')
        }
        if (selected_province != undefined) {
            $('#supplier-province').val(selected_province).trigger('change')
        }
        if (selected_ward != undefined) {
            $('#supplier-ward').val(selected_ward).trigger('change')
        }
        // --- Lắng nghe sự kiện khi Tỉnh thay đổi ---
        $('#supplier-province').on('select2:select', function (e) {
            const provinceId = e.params.data.id;

            // Reset và vô hiệu hóa Huyện và Xã
            $('#supplier-district').val(null).trigger('change'); // Xóa chọn hiện tại
            $('#supplier-district').prop('disabled', false); // Bật lại ô Huyện
            $('#supplier-ward').val(null).trigger('change'); // Xóa chọn hiện tại
            $('#supplier-ward').prop('disabled', true); // Vô hiệu hóa ô Xã

            // Khởi tạo và tải dữ liệu cho Huyện dựa trên Tỉnh đã chọn
            $('#supplier-district').select2({
                ajax: {
                    url: "/Location/DistrictSuggestion",
                    type: "post",
                    dataType: 'json',
                    delay: 250,
                    data: function (params) {
                        var query = {
                            keyword: params.term,
                            province_id: $('#supplier-province').find(':selected').val()
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
        });

        // --- Lắng nghe sự kiện khi Huyện thay đổi ---
        $('#supplier-district').on('select2:select', function (e) {
            const districtId = e.params.data.id;

            // Reset và vô hiệu hóa Xã
            $('#supplier-ward').val(null).trigger('change'); // Xóa chọn hiện tại
            $('#supplier-ward').prop('disabled', false); 
            $('#supplier-ward').removeProp('disabled');

            // Khởi tạo và tải dữ liệu cho Xã dựa trên Huyện đã chọn
            $('#supplier-ward').select2({
                ajax: {
                    url: "/Location/WardSuggestion",
                    type: "post",
                    dataType: 'json',
                    delay: 250,
                    data: function (params) {
                        var query = {
                            keyword: params.term,
                            district_id: $('#supplier-district').find(':selected').val()
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
        });

        // --- Xử lý khi người dùng xóa lựa chọn (Clear) ---
        $('#supplier-province').on('select2:unselect', function (e) {
            // Khi bỏ chọn Tỉnh, reset và vô hiệu hóa Huyện và Xã
            $('#supplier-district').val(null).trigger('change').prop('disabled', true);
            $('#supplier-ward').val(null).trigger('change').prop('disabled', true);
        });

        $('#supplier-district').on('select2:unselect', function (e) {
            // Khi bỏ chọn Huyện, reset và vô hiệu hóa Xã
            $('#supplier-ward').val(null).trigger('change').prop('disabled', true);
        });
        //$('body').on('select2:select', '#supplier-province', function (e) {
        //    $('#supplier-district').select2({
        //        ajax: {
        //            url: "/Location/DistrictSuggestion",
        //            type: "post",
        //            dataType: 'json',
        //            delay: 250,
        //            data: function (params) {
        //                var query = {
        //                    keyword: params.term,
        //                    province_id: $('#supplier-province').find(':selected').val()
        //                }
        //                return query;
        //            },
        //            processResults: function (response) {
        //                return {
        //                    results: $.map(response.data, function (item) {
        //                        return {
        //                            text: '[' + item.id + '] - ' + item.name,
        //                            id: item.id,
        //                        }
        //                    })
        //                };
        //            },
        //            cache: true
        //        }
        //    });
        //});
        //$('body').on('select2:select', '#supplier-district', function (e) {
        //    $('#supplier-ward').select2({
        //        ajax: {
        //            url: "/Location/WardSuggestion",
        //            type: "post",
        //            dataType: 'json',
        //            delay: 250,
        //            data: function (params) {
        //                var query = {
        //                    keyword: params.term,
        //                    district_id: $('#supplier-district').find(':selected').val()
        //                }
        //                return query;
        //            },
        //            processResults: function (response) {
        //                return {
        //                    results: $.map(response.data, function (item) {
        //                        return {
        //                            text: '[' + item.id + '] - ' + item.name,
        //                            id: item.id,
        //                        }
        //                    })
        //                };
        //            },
        //            cache: true
        //        }
        //    });
        //});
    },
    AddImage: function (element) {
        var ImageExtension = ['jpeg', 'jpg', 'png', 'bmp']

        if ($.inArray(element.val().split('.').pop().toLowerCase(), ImageExtension) == -1) {
            _msgalert.error("Vui lòng chỉ upload các định dạng sau: " + ImageExtension.join(', '));
            return
        }

        $(element[0].files).each(function (index, item) {

            var reader = new FileReader();
            reader.onload = function (e) {
                var src = e.target.result;
                element.closest('.col-md-3').find('.magnific_popup').find('img').attr('src', src)
            }
            element.closest('.col-md-3').find('.magnific_popup').show()
            element.closest('.col-md-3').find('.choose').hide()
            reader.readAsDataURL(item);
            return false;
        });
        element.val(null)

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
    CheckIfImageVideoIsLocal: function (data) {
        if (data != undefined && (data.includes("data:image") || data.includes("data:video") || data.includes("base64,"))) {
            return true
        }
        else {
            return false
        }
    }
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
            url: "/OrderManual/UserSuggestion",
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
$(document).on('select2:open', () => {
    document.querySelector('.select2-search__field').focus();
});