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
        $('body').on('click', '#update-avatar-import', function () {
            var element = $(this)
            $('#update-avatar-input').click()

        });
        $('body').on('change', '#update-avatar-input', function () {
            if ($(this)[0].files[0] && label_detail.Data.Processing == false) {
                label_detail.UploadAvatar()
            }
        });

        //-- Banner:
        $('body').on('click', '#banner-main .col-md-3 .delete,#banner-sub .col-md-3 .delete ', function () {
            var element = $(this)
            element.closest('.col-md-3').find('.magnific_popup').hide()
            element.closest('.col-md-3').find('.choose').show()
            element.closest('.col-md-3').find('img').attr('src', '')

        })
        $('body').on('change', '.col-md-3 .banner-file', function () {
            var element = $(this)
            label_detail.AddImage(element)
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
    UploadAvatar: function () {
        label_detail.Data.Processing = true;
        var element = $('#update-avatar-input')
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
                $('#update-avatar-import img').attr('src', url)
                $('#update-avatar').val(url).trigger('change')
                $('#update-avatar-input').val(null).trigger('change')
                $('#update-avatar-import img').css('top', '0')
                $('#update-avatar-import img').css('object-fit', 'cover')
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
        var position = $("#update-position").val(); 
        var ShopMallPosition = $("#update-shopmall-position").val(); 
        var userSupplierId = $("#update-userSupplierId").val();
        var banner = $("#update-banner-import img") == undefined ? '' : $("#update-banner-import img").attr('src'); 
        var avt = $("#update-avatar-import img") == undefined ? '' : $("#update-avatar-import img").attr('src'); 


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
        if (!description) {
            _msgalert.error("Mô tả thương hiệu là bắt buộc.");
            $("#update-description").focus();
            return false;
        }
        if (description.length > 500) {
            _msgalert.error("Mô tả thương hiệu không được quá 500 ký tự.");
            $("#update-description").focus();
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
        formData.append("Avatar", avt == undefined ? '' : avt);
        formData.append("Position", position);
        formData.append("ShopMallPosition", ShopMallPosition);

        //-- Banner:

        var banner_main = []
        $('#banner-main .col-md-3').each(function (index, item) {
            var element_image = $(this)
            if (element_image.find('img').length > 0) {
                var data_src = element_image.find('img').attr('src')
                if (data_src == null || data_src == undefined || data_src.trim() == '') {
                    banner_main.push("")
                    return true
                }
                if (label_detail.CheckIfImageVideoIsLocal(data_src)) {
                    var result = label_detail.POSTSynchorus('/Product/SummitImages', { data_image: data_src })
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

        formData.append("BannerMain", banner_main == undefined ? '' : banner_main[0]);

        var banner_sub = []
        $('#banner-sub .col-md-3').each(function (index, item) {
            var element_image = $(this)
            if (element_image.find('img').length > 0) {
                var data_src = element_image.find('img').attr('src')
                if (data_src == null || data_src == undefined || data_src.trim() == '') {
                    banner_sub.push("")
                    return true

                }
                if (label_detail.CheckIfImageVideoIsLocal(data_src)) {
                    var result = label_detail.POSTSynchorus('/Product/SummitImages', { data_image: data_src })
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
        formData.append("BannerSub", banner_sub == undefined || banner_sub.length <= 0 ? '' : JSON.stringify(banner_sub));


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
    },
}