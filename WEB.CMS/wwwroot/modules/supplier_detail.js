$(document).ready(function () {
    _supplier_detail.DynamicBind()
})
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
                            .replaceAll('@item.Ext', path[path.length -1])

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




$(document).ready(function () {
    $('input').attr('autocomplete', 'off');
    //_supplier_detail.Init();
    _supplier_contact.Init();
    _supplier_payment.Init();
    _supplier_order.Init();
  //  _supplier_ticket.Init();
});