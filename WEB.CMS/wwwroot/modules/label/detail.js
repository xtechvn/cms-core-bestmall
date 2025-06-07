$(document).ready(function () {
    label_detail.Initialization()
})
var label_detail = {
    Data: {
        Processing: false
    },
    Initialization: function () {
        label_detail.DynamicBind()
    },
    DynamicBind: function () {
        $('body').on('click', '#update-banner-import', function () {
            var element = $(this)
            $('#update-banner-input').click()

        });
        $('body').on('change', '#update-banner-input', function () {
            if ($(this)[0].files[0] && label_detail.Data.Processing == false) {
                label_detail.UploadBanner()
            }
        })
        $('body').on('click', '#update-icon-import', function () {
            var element = $(this)
            $('#update-icon-input').click()

        });
        $('body').on('change', '#update-icon-input', function () {
            if ($(this)[0].files[0] && label_detail.Data.Processing==false) {
                label_detail.Upload()
            }
        })
       
    },
    Upload: function () {
        label_detail.Data.Processing = true;
        var element = $('#update-icon-input')
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
                var url = result.data[0]
                $('#update-icon-import img').attr('src',url)
                $('#update-icon').val(url).trigger('change')
                $('#update-icon-input').val(null).trigger('change')
                $('#update-icon-import img').css('top','0')
                $('#update-icon-import img').css('object-fit','cover')
                label_detail.Data.Processing = false

            }
        });
    },
    UploadBanner: function () {
        label_detail.Data.Processing = true;
        var element = $('#update-banner-input')
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
                var url = result.data[0]
                $('#update-banner-import img').attr('src', url)
                $('#update-banner').val(url).trigger('change')
                $('#update-banner-input').val(null).trigger('change')
                $('#update-banner-import img').css('top', '0')
                $('#update-banner-import img').css('object-fit', 'fill')
                label_detail.Data.Processing = false

            }
        });
    },
    Update: function () {

        // 1.  Get the values from the input fields
        var id = $("#update-detail").val();
        var labelCode = $("#update-code").val().toUpperCase();
        var labelName = $("#update-name").val();
        var status = $("#update-status").val();
        var description = $("#update-description").val();
        var icon = $("#update-icon").val();
        var parentId = $("#update-parentid").val();  
        var level = $("#update-level").val(); 
        var userSupplierId = $("#update-userSupplierId").val();
        var banner = $("#update-banner-import img") == undefined ? '' : $("#update-banner-import img").attr('src'); 


        // 2.  Validation
        if (!labelCode) {
            _msgalert.error("Mã thương hiệu là bắt buộc.");
            $("#update-code").focus();
            return false;
        }
        if (labelCode.length > 100) {
            _msgalert.error("Mã thương hiệu không được quá 100 ký tự.");
            $("#update-code").focus();
            return false;
        }
        if (!labelName) {
            _msgalert.error("Tên thương hiệu là bắt buộc.");
            $("#update-name").focus();
            return false;
        }
        if (labelName.length > 200) {
            _msgalert.error("Tên thương hiệu không được quá 200 ký tự.");
            $("#update-code").focus();
            return false;
        }
        if (icon == null || icon == undefined || icon == 'null' || icon == 'undefined'|| icon.trim() == '') {
            _msgalert.error("Ảnh đại diện thương hiệu là bắt buộc");
            $("#update-icon").focus();
            return false;
        }
        // 3. Create FormData object to send data
        var formData = new FormData();

        formData.append("Id", id);
        formData.append("LabelCode", labelCode);
        formData.append("LabelName", labelName);
        formData.append("Status", status);
        formData.append("Description", description);
        formData.append("Icon", icon);
        formData.append("ParentId", parentId);
        formData.append("Level", level);
        formData.append("UserSupplierId", userSupplierId);
        formData.append("Banner", banner == undefined ? '' : banner);


        // 4. AJAX request
        $.ajax({
            url: "/label/UpSert", //  Your API endpoint
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.msg);
                    $('#global_modal_popup').modal('hide');
                    setTimeout(function () {
                        window.location.reload()
                    }, 1000);
                } else {
                    _msgalert.error(result.msg);
                }
               
            },
            error: function (error) {

            }
        });
    },

}