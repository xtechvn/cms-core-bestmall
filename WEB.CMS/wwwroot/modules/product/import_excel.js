$(document).ready(function () {
    product_import_excel.Initialization()

})

var product_import_excel = {
    Initialization: function (first_time = true) {
        if (first_time) {
            product_import_excel.DynamicBind()
        } else {
            $('.product-mass-upload-step1').show()
            $('.product-mass-upload-step2').hide()
            $('.product-mass-upload-step3').hide()
            $('.product-mass-upload-breadcumb-step1').css('font-weight', 'bold')
            $('.product-mass-upload-breadcumb-step2').css('font-weight', 'normal')
            $('.product-mass-upload-breadcumb-step3').css('font-weight', 'normal')
        }
      
    },
    DynamicBind: function () {
        $('body').on('change', '#import-file-product', function (e) {
            product_import_excel.ProductListing()
        });
        $('body').on('click', '#confirm-ws-import', function (e) {
            product_import_excel.ConfirmImport()
        });
    },
    ProductListing: function () {
        let url = '/Product/ImportProductListing';
        let file = document.getElementById("import-file-product").files[0];
        if (file == undefined) {
            _msgalert.error('Vui lòng chọn lại tệp tin khác.')
            $('.product-mass-upload-step1').show()
            $('.product-mass-upload-step2').hide()
            $('.product-mass-upload-step3').hide()
            $('.product-mass-upload-breadcumb-step1').css('font-weight', 'bold')
            $('.product-mass-upload-breadcumb-step2').css('font-weight', 'normal')
            $('.product-mass-upload-breadcumb-step3').css('font-weight', 'normal')
        }
        var file_type = file['type'];
        if (file_type !== "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
            $('#import-error').removeClass('mfp-hide');
            $('#grid_ws').html('');
            return;
        }
        $('#import-error').hide()

        let formData = new FormData();
        formData.append("file", file)

        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (result) {
                if (result != null) {
                    $('#grid_ws').html(result);
                    $('#confirm-ws-import').show()
                    $('.product-mass-upload-step1').hide()
                    $('.product-mass-upload-step2').show()
                    $('.product-mass-upload-step3').hide()
                    $('.product-mass-upload-breadcumb-step1').css('font-weight', 'normal')
                    $('.product-mass-upload-breadcumb-step2').css('font-weight', 'bold')
                    $('.product-mass-upload-breadcumb-step3').css('font-weight', 'normal')
                } else {
                    $('#import-error').removeClass('mfp-hide');
                }
            }, error: function (error) {
                console.log(error);
                $('#import-ws-error').removeClass('mfp-hide');
            }
        });
    },
    ConfirmImport: function () {
        $('.loading-gif').show()
        let url = '/Product/ConfirmProductExcel';
        let data = [];
        $('#import-error').hide()
        $('#confirm-ws-import').prop('disabled', true)
        console.log($('#grid_ws tbody tr').first().find('.group_product_id').text())
        $('#grid_ws tbody tr').each(function () {
            let seft = $(this);
            data.push({
                group_product_id: seft.find('.group_product_id').text(),
                name: seft.find('.name').text(),
                description: seft.find('.description').text(),
                sku: seft.find('.sku').text(),
                product_code: seft.find('.product_code').text(),
                attribute_1_name: seft.find('.attribute_1_name').text(),
                variation_1_name: seft.find('.variation_1_name').text(),
                variation_images: seft.find('.variation_images').text(),
                attribute_2_name: seft.find('.attribute_2_name').text(),
                variation_2_name: seft.find('.variation_2_name').text(),
                variation_sku: seft.find('.variation_sku').text(),
                price: seft.find('.price').text(),
                profit: seft.find('.profit').text(),
                amount: seft.find('.amount').text(),
                stock: seft.find('.stock').text(),
                avatar: seft.find('.avatar').text(),
                video: seft.find('.video').text(),
                image_1: seft.find('.image_1').text(),
                image_2: seft.find('.image_2').text(),
                image_3: seft.find('.image_3').text(),
                image_4: seft.find('.image_4').text(),
                image_5: seft.find('.image_5').text(),
                image_6: seft.find('.image_6').text(),
                image_7: seft.find('.image_7').text(),
                image_8: seft.find('.image_8').text(),
                weight: seft.find('.weight').text(),
                width: seft.find('.width').text(),
                height: seft.find('.height').text(),
                depth: seft.find('.depth').text(),
                brand: seft.find('.brand').text(),
            });

        });
        $.ajax({
            url: url,
            type: "POST",
            data: { model_json: JSON.stringify(data) },
            success: function (result) {
                $('#grid_ws').html(result);
                $('#confirm-ws-import').hide()
                $('.product-mass-upload-step1').hide()
                $('.product-mass-upload-step2').hide()
                $('.product-mass-upload-step3').show()
                $('.loading-gif').hide()

                $('.product-mass-upload-breadcumb-step1').css('font-weight', 'normal')
                $('.product-mass-upload-breadcumb-step2').css('font-weight', 'normal')
                $('.product-mass-upload-breadcumb-step3').css('font-weight', 'bold')
            }
        });
    }
}