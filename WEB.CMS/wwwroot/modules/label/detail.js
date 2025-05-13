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
                label_detail.Data.Processing = false

            }
        });
    },
    Update: function () {

        // 1.  Get the values from the input fields
        var id = $("#update-detail").val();
        var labelCode = $("#update-code").val();
        var labelName = $("#update-name").val();
        var status = $("#update-status").val();
        var description = $("#update-description").val();
        var icon = $("#update-icon").val();
        var parentId = $("#update-parentid").val();  
        var level = $("#update-level").val(); 
        var userSupplierId = $("#update-userSupplierId").val(); 


        // 2.  Validation
        if (!labelCode) {
            _msgalert.error("Mã thương hiệu là bắt buộc.");
            $("#update-code").focus();
            return false;
        }

        if (!labelName) {
            _msgalert.error("Tên thương hiệu là bắt buộc.");
            $("#update-name").focus();
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


        // 4. AJAX request
        $.ajax({
            url: "/label/UpSert", //  Your API endpoint
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (result) {
                _msgalert.success(result.msg);
                $('#global_modal_popup').modal('hide');
                setTimeout(function () {
                    window.location.reload()
                }, 1000);
            },
            error: function (error) {
                _msgalert.error(result.msg);

            }
        });
    },

}