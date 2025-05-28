$(document).ready(function () {
    _supplier_detail.DynamicBind()
    // $('input').attr('autocomplete', 'off');
    //_supplier_detail.Init();
    //_supplier_contact.Init();
    //_supplier_payment.Init();
    // _supplier_order.Init();
    //  _supplier_ticket.Init();
    _suplier_user.Initialization()
});
var _supplier_detail = {
    Data: {
        Processing: false,
        Images: `<div class="col-sm-3 col-4 mb10 file" style="max-width:150px;" data-path="@item.Path" data-id="@item.Id" data-ext="@item.Ext">
                        <button type="button" class="delete-file" onclick="$(this).closest('.file').remove()"
                        style=" background: #ED5C6A; color: #fff; position: absolute; right: -7px; top: -7px; z-index: 2; border: 0; width: 20px; height: 20px; border-radius: 50%; outline: 0; cursor: pointer; ">
                        x
                        </button>
                        <div class="choose-ava lightgallery">
                            <img src="@item.Path" />
                        </div>
                        <p style=" overflow: hidden; "><span>@(file_name)</span> </p>
                    </div>`
    },
    Init: function () {
        this.elModal = $('#global_modal_popup');
        this.supplier_id = $('#global_supplier_id').val();
        this.validImageTypes = ['image/gif', 'image/jpeg', 'image/png'];
        this.validImageSize = 5 * 1024 * 1024;
        this.noImageSrc = "/images/icons/noimage.png";
    },

    GetFormData: function ($form) {
        var unindexed_array = $form.serializeArray();
        var indexed_array = {};

        $.map(unindexed_array, function (n, i) {
            indexed_array[n['name']] = n['value'];
        });

        return indexed_array;
    },
    DynamicBind: function () {
        $('body').on('change', '.form-group-attachment .attachfile-add', function () {
            var element = $(this)
            if ($(this)[0].files[0] && _supplier_detail.Data.Processing == false) {
                _supplier_detail.Upload(element)
            }
        })
    },
    Upload: function (element) {
        _supplier_detail.Data.Processing = true;
        var formData = new FormData()
        $(element[0].files).each(function (index, item) {
            formData.append("files", item);
        });

        $.ajax({
            url: "/AttachFile/Upload",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (result) {
                $(result.data).each(function (index, item) {
                    var path = item.split('.')
                    element.closest('.form-group-attachment').find('.attachment-file-gallery').append(
                        _supplier_detail.Data.Images
                            .replaceAll('@item.Path', item)
                            .replaceAll('@(file_name)', item)
                            .replaceAll('@item.Id', '0')
                            .replaceAll('@item.Ext', path[path.length - 1])

                    )

                });
                element.val(null).trigger('change')
                _supplier_detail.Data.Processing = false

            }
        });
    },

}

var _supplier_contact = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this.search_params = {
            supplier_id: $('#global_supplier_id').val()
        }
        this.ReloadListing(this.search_params);
    },

    OnAddOrUpdate: function (id) {
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} liên hệ`;
        let url = '/Supplier/ContactUpsert';

        this.modal.find('.modal-title').html(title);
        this.modal.find('.modal-dialog').css('max-width', '600px');

        _ajax_caller.get(url, { id: id, supplier_id: this.search_params.supplier_id }, function (result) {
            _supplier_contact.modal.find('.modal-title').html(title);
            _supplier_contact.modal.find('.modal-body').html(result);
            _supplier_contact.modal.modal('show');
        });
    },

    Upsert: function () {
        let url = '/Supplier/ContactUpsert';
        let Form = $('#form_supplier_contact');
        Form.validate({
            rules: {
                Name: "required",
                Mobile: {
                    required: true,
                    minlength: 10,
                    maxlength: 11,
                    digits: true
                },
                Email: {
                    email: true
                },
            },
            messages: {
                Name: "Vui lòng nhập tên liên hệ",
                Mobile: {
                    required: "Vui lòng nhập số điện thoại",
                    exactlength: "Số điện thoại phải nhập đúng 10 / 11 kí tự dạng số",
                    digits: "Số điện thoại phải là kí tự dạng số"
                },
                Email: {
                    email: 'Email không đúng định dạng'
                }
            }
        });

        if (!Form.valid()) return;

        let formData = _supplier_detail.GetFormData(Form);

        _ajax_caller.post(url, { model: formData }, function (result) {
            if (result.isSuccess) {
                _msgalert.success(result.message);
                _supplier_contact.modal.modal('hide');
                _supplier_contact.ReloadListing();
            } else {
                _msgalert.error(result.message);
            }
        });
    },

    Delete: function (id) {
        let url = '/Supplier/ContactDelete';
        let title = 'Xác nhận xóa liên hệ';
        let description = 'Bạn có chắc chắn muốn thông tin?';
        _msgconfirm.openDialog(title, description, function () {
            _ajax_caller.post(url, { id: id }, function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    _supplier_contact.ReloadListing();
                } else {
                    _msgalert.error(result.message);
                }
            });
        });
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/ContactListing", input, function (result) {
            $('#grid_supplier_contact').html(result);
        });
    },

    ReloadListing: function () {
        //this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    }
}

var _supplier_payment = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this.search_params = {
            supplier_id: $('#global_supplier_id').val()
        }
        this.ReloadListing(this.search_params);
    },

    OnAddOrUpdate: function (id) {
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} thông tin thanh toán`;
        let url = '/Supplier/PaymentUpsert';

        this.modal.find('.modal-title').html(title);
        this.modal.find('.modal-dialog').css('max-width', '600px');

        _ajax_caller.get(url, { id: id, supplier_id: this.search_params.supplier_id }, function (result) {
            _supplier_payment.modal.find('.modal-title').html(title);
            _supplier_payment.modal.find('.modal-body').html(result);
            _supplier_payment.modal.modal('show');
        });
    },

    Upsert: function () {
        let url = '/Supplier/PaymentUpsert';
        let Form = $('#form_supplier_payment');
        Form.validate({
            rules: {
                AccountName: "required",
                AccountNumber: {
                    required: true,
                    maxlength: 20,
                    /*  digits: true*/
                },
                BankId: "required"
            },
            messages: {
                AccountName: "Vui lòng nhập chủ tài khoản ngân hàng",
                AccountNumber: {
                    required: "Vui lòng nhập số tài khoản",
                    maxlength: "Số tài khoản chỉ chứa tối đa 20 kí tự",
                    /* digits: "Số tài khoản phải là kí tự dạng số"*/
                },
                BankId: "Vui lòng nhập tên ngân hàng"
            }
        });

        if (!Form.valid()) return;

        let formData = _supplier_detail.GetFormData(Form);

        _ajax_caller.post(url, { model: formData }, function (result) {
            if (result.isSuccess) {
                _msgalert.success(result.message);
                _supplier_payment.modal.modal('hide');
                _supplier_payment.ReloadListing();
            } else {
                _msgalert.error(result.message);
            }
        });
    },

    Delete: function (id) {
        let url = '/Supplier/PaymentDelete';
        let title = 'Xác nhận xóa liên hệ';
        let description = 'Bạn có chắc chắn muốn thông tin?';
        _msgconfirm.openDialog(title, description, function () {
            _ajax_caller.post(url, { id: id }, function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    _supplier_payment.ReloadListing();
                } else {
                    _msgalert.error(result.message);
                }
            });
        });
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/PaymentListing", input, function (result) {
            $('#grid_supplier_payment').html(result);
        });
    },

    ReloadListing: function () {
        //this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    }
}

var _supplier_order = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this.search_params = {
            supplier_id: $('#global_supplier_id').val(),
            page_index: 1,
            page_size: 10
        }
        this.ReloadListing(this.search_params);
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/OrderListing", input, function (result) {
            $('#grid_supplier_order').html(result);
        });
    },

    ReloadListing: function () {
        //this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    }
}

var _suplier_user = {
    HTML: `
      <tr class="tab-users-tr tab-users-tr-new" data-id="0">
                                                    <td class="count">
                                                        @(++count)
                                                    </td>
                                                    <td class="fullname">
                                                        <div class="flex gap10 flex-nowrap align-items-center justify-content-center">
                                                            <div class="form-group mb-0" style=" width: 100%; ">
                                                                <input type="text" class="form-control" value="" />
                                                                  <span class="error" style="padding: 0 3px;display:none;">
                                                                    Vui lòng không để trống
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td class="username">
                                                        <div class="flex gap10 flex-nowrap align-items-center justify-content-center">
                                                            <div class="form-group mb-0" style=" width: 100%; ">
                                                               <nw style=" display: flex; ">
                                                                    <span style=" align-content: center; "> ncc@Model.SupplierId.</span>
                                                                    <input type="text" class="form-control" value="" style="margin-left: 0;">
                                                                </nw>
                                                                <span class="error" style="padding: 0 3px;display:none;">
                                                                    Vui lòng không để trống
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td class="password">
                                                        <div class="flex gap10 flex-nowrap align-items-center justify-content-center">
                                                            <div class="form-group mb-0" style=" width: 100%; ">
                                                                <input type="password" class="form-control" value="" />
                                                                  <span class="error" style="padding: 0 3px;display:none;">
                                                                    Vui lòng không để trống
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td  class="status">
                                                        <div class="wrap_input">
                                                            <div class="form-group mb-0" style=" width: 100%; ">
                                                                <select class="select2 w-100">
                                                                   <option value="0">Hoạt động</option>
                                                                    <option value="1">Tạm dừng</option>

                                                                </select>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td>
                                                        <div class="flex gap10 flex-nowrap align-items-center justify-content-center">
                                                            <a href="javascript:;" class="tab-users-tr-edit" style="display:none;"><i class="fa fa-edit blue"></i></a>
                                                            <a href="javascript:;" class="tab-users-tr-confirm"><i class="fa fa-check green"></i></a>
                                                            <a href="javascript:;" class="tab-users-tr-new-del"><i class="icofont-trash"></i></a>
                                                        </div>


                                                    </td>
                                                </tr>

    `,
    Initialization: function () {
        _suplier_user.DynamicBind()
    },
    DynamicBind: function () {
        $('body').on('click', '.tab-users-tr-edit', function () {
            var element = $(this)
            var parent = element.closest('tr')
            element.hide()
            parent.find('.tab-users-tr-confirm').show()
            parent.find('input').prop("disabled", false);
            parent.find('select').prop("disabled", false);
            parent.find('.password').find('input').val('').trigger('change');
        })
        $('body').on('click', '.tab-users-tr-confirm', function () {
            var element = $(this)
            var parent = element.closest('tr')
            var id = parent.attr('data-id')
            var is_update = (id != undefined && id.trim() != '' && parseInt(id) != undefined && parseInt(id) > 0)
            var title = is_update ? "Xác nhận cập nhật tài khoản" : "Xác nhận tạo tài khoản";
            var description = "Tài khoản thuộc NCC này sẽ được " + (is_update ? "cập nhật" : "tạo mới") + ", bạn có chắc chắn không? " + (is_update ? "" : "(Sau khi nhấn đồng ý sẽ không thể xóa tài khoản)")
            if (_suplier_user.Validate(parent)) {
                _msgconfirm.openDialog(title, description, function () {
                    element.hide()
                    parent.find('.tab-users-tr-edit').show()
                    parent.find('input').prop('disabled', true)
                    parent.find('select').prop('disabled', true)
                    parent.find('.tab-users-tr-new-del').remove()
                    _suplier_user.UpSert(parent)
                });

            } else {
                parent.find('.tab-users-tr-confirm').show()
                parent.find('input').prop("disabled", false);
                parent.find('select').prop("disabled", false);
                parent.find('.password').find('input').val('').trigger('change');
            }
        })
        $('body').on('click', '.tab-users-tr-new-del', function () {
            var element = $(this)
            element.closest('tr').remove()
        })
        $('body').on('click', '#tab-users-add', function () {
            $('#tab-users tbody').append(_suplier_user.HTML.replaceAll('@Model.SupplierId', $('#global_supplier_id').val()))
            $('.tab-users-tr-new').find('select').select2()
            $('.tab-users-tr-new').removeClass('tab-users-tr-new')
            _suplier_user.ReArrangeIndex()
        })
        $('body').on('click', '#tab-users-search', function () {
            var element = $(this)
            var input = $('#tab-users-search-input').val()
            input = _suplier_user.removeSpecialCharacters(input).trim()
            $('.tab-users-tr').each(function (index, item) {
                var tr = $(this)
                if (
                    tr.find('.fullname').find('input').val().includes(input) ||
                    tr.find('.username').find('input').val().includes(input)
                ) {
                    tr.show()
                } else {
                    tr.hide()
                }
            })
        })
        $('body').on('click', '.tab-users-tr input, .tab-users-tr select', function () {
            var element = $(this)
            element.closest('.form-group').find('.error').hide() 
            element.closest('.form-group').find('.error').html('Vui lòng không để trống') 
        })
    },
    ReArrangeIndex: function () {
        var count = 0;
        $('.tab-users-tr').each(function (index, item) {
            count++;
            var element = $(this)
            element.find('.count').html(count)
        })
    },
    Validate: function (rowElement) {
        let isValid = true;

        // Validate text inputs (FullName, UserName, Password)
        $(rowElement).find('input[type="text"], input[type="password"]').each(function () {
            const value = $(this).val();
            if (value === null || value === undefined || value.trim() === '') {
                isValid = false;
                $(this).closest('.form-group').find('.error').show() 
                return false; 
            }
        });
        if (!isValid) return isValid
        //-- Validate username
        const regex = /^[a-zA-Z0-9._@-]+$/;
        var username = rowElement.find('.username').find('input').val()
        isValid = regex.test(username)
        if (!isValid) {
            rowElement.find('.username').find('input').closest('.form-group').find('.error').show()
            rowElement.find('.username').find('input').closest('.form-group').find('.error').html('Tên tài khoản chỉ cho phép: chữ, số, các ký tự (.)(-)(_)(@)') 
            return false
        } 
        if (!isValid) return isValid

        $(rowElement).find('select').each(function () {
            const value = $(this).val();
            if (value === null || value === undefined || value.trim() === '') {
                isValid = false;
                $(this).closest('.form-group').find('.error').show() 
                return false;
            }
        });
        if (!isValid) return isValid

        return isValid;
    },
    UpSert: function (parent) {
        var user_name = _suplier_user.removeSpecialCharactersUsername(parent.find('.username').find('input').val().toLowerCase())
        var request = {
            Id: parent.attr('data-id'),
            UserName: user_name,
            FullName: parent.find('.fullname').find('input').val(),
            Password: parent.find('.password').find('input').val(),
            Status: parent.find('.status').find('select:selected').val(),
            SupplierId: $('#global_supplier_id').val()
        }

        $.ajax({
            url: "/Supplier/UpdateSuplierUser",
            data: { request: request },
            type: "POST",
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    parent.attr('data-id', result.data)
                } else {
                    _msgalert.error(result.message);
                }

            }
        });
    },
    removeSpecialCharacters: function (str) {
        return str.replace(/[^a-zA-Z0-9_\-.]/g, '');
    },
    removeSpecialCharactersUsername: function (str) {
        return str.replace(/[^a-zA-Z0-9._@-]/g, '');
    }

}

