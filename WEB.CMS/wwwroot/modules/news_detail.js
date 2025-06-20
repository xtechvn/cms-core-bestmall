var _wrapperImage = $("#video-content");
var _attachfile = $("#lightgallery");
$(document).ready(function () {
    /*
    $('.datepicker-input').Zebra_DatePicker({
        format: 'd/m/Y H:i',
        onSelect: function () {
            $(this).change();
        }
    }).removeAttr('readonly');*/
    $('input[name="single_pick_date"]').daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        drops: 'down',
        timePicker: true,
        timePicker24Hour: true,
        minDate: '01/01/2023 00:00',
        maxDate: '31/12/2052 23:59',
        locale: {
            format: 'DD/MM/YYYY HH:mm'
        }
    }, function (start, end, label) {

    });
    var val = $('#PublishDate').attr('value');
    $('#PublishDate').data('daterangepicker').setStartDate(val);

    if ($('#ArticleType:checked').val() == 1) {
        $('#normal_post').hide();
        $('#video_post').show();

        $('#video-preview').hide();
    };

    _wrapperImage.lightGallery();
    var News_Status = $('#News_Status').val();
    if (News_Status == 0) {
        _newsDetail.disabledView();
    }
});

_common.tinyMce('#text-editor');

$('#detail-cate-panel .btn-toggle-cate').click(function () {
    var seft = $(this);
    if (seft.hasClass("plus")) {
        seft.siblings('ul.lever2').show();
        seft.removeClass('plus').addClass('minus');
    } else {
        seft.siblings('ul.lever2').hide();
        seft.removeClass('minus').addClass('plus');
    }
});

$('#news-tag').tagsinput({
    typeahead: {
        source: function (query) {
            var dataList = $.ajax({
                type: 'Post',
                url: "/News/GetSuggestionTag",
                async: false,
                dataType: 'json',
                data: {
                    name: query,
                },
                done: function (data) {
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                }
            }).responseJSON;
            return dataList;
        }, afterSelect: function () {
            this.$element[0].value = '';
        }
    }
});
const iconLoading = document.getElementById("loading");
const showLoading = () => {
    iconLoading.style.display = "flex";
};
const hideLoading = () => {
    iconLoading.style.display = "none";
};
var uploadCrop = $('#croppie-content').croppie({
    viewport: {
        width: 200,
        height: 150,
        type: 'square'
    },
    boundary: {
        width: 250,
        height: 250
    },
    url: '/images/icons/noimage.png'
});


$('.sl-image-size').change(function (e) {
    var value = e.target.value;
    var width = parseInt(value.split("x")[0]);
    var height = parseInt(value.split("x")[1]);
    var filedata = $('#image_file')[0].files[0];
    $('#croppie-content').croppie('destroy');
    if (filedata) {
        $('.wrap-croppie').show();
        $('.wrap-image-preview').hide();
        $('#btn-cropimage').show();
        var reader = new FileReader();
        reader.readAsDataURL(filedata);
        reader.onload = function () {
            $('#croppie-content').croppie({
                viewport: {
                    width: width,
                    height: height,
                    type: 'square'
                },
                boundary: {
                    width: 250,
                    height: 250
                },
                url: reader.result
            });
        };
    } else {
        $('#croppie-content').croppie({
            viewport: {
                width: width,
                height: height,
                type: 'square'
            },
            boundary: {
                width: 250,
                height: 250
            },
            url: '/images/icons/noimage.png'
        });
    }
});

$('#image_file').change(function (event) {
    var _validFileExtensions = ["jpg", "jpeg", "bmp", "gif", "png"];

    if (event.target.files && event.target.files[0]) {
        var fileType = event.target.files[0].name.split('.').pop();

        if (event.target.files[0].size > (1024 * 1024)) {
            _msgalert.error('File upload hiện tại có kích thước (' + Math.round(event.target.files[0].size / 1024 / 1024, 2) + ' Mb) vượt quá 1 Mb. Bạn hãy chọn lại ảnh khác');
            $(this).val('');
        }

        if (!_validFileExtensions.includes(fileType)) {
            _msgalert.error('File upload phải thuộc các định dạng : jpg, jpeg, bmp, gif, png');
            $(this).val('');
        }

        if (_validFileExtensions.includes(fileType) && event.target.files[0].size <= (1024 * 1024)) {
            $('.wrap-croppie').show();
            $('.wrap-image-preview').hide();
            $('#btn-cropimage').show();
            $('#btn-cancel-crop').show();
            var reader = new FileReader();
            reader.onload = function (e) {
                uploadCrop.croppie('bind', {
                    url: e.target.result
                });
            };
            reader.readAsDataURL(event.target.files[0]);
        }
    }
});

$('#video-file').change(function (event) {
    var _validFileExtensions = ["mp4"];
    if (event.target.files && event.target.files[0]) {
        var fileType = event.target.files[0].name.split('.').pop();

        if (event.target.files[0].size > (100 * 1024 * 1024)) {
            _msgalert.error('File upload hiện tại có kích thước (' + Math.round(event.target.files[0].size / 1024 / 1024, 2) + ' Mb) vượt quá 100 Mb. Bạn hãy chọn lại ảnh khác');
            $(this).val('');
        }

        if (!_validFileExtensions.includes(fileType)) {
            _msgalert.error('File upload phải thuộc các định dạng : mp4');
            $(this).val('');
        }

        if (_validFileExtensions.includes(fileType) && event.target.files[0].size <= (100 * 1024 * 1024)) {
            var reader = new FileReader();
            $('#video-content').show();
            $('#video-croppie').show();

            $('#video-preview').hide();
            showLoading();
            setTimeout(() => {
                $('#uploadvieo').show();
                reader.onload = function (e) {
                    _wrapperImage.append('<video  class="col-md" id="iframe-video"  src="' + reader.result + '" controls></video>'

                        /*'<iframe style="width: 100 %;" id="iframe-video" src="' + reader.result + '"></iframe>'*/
                    );
                };
                reader.readAsDataURL(event.target.files[0]);
                hideLoading();
            }, 2000);

        }
    }
});

$('#btn-cropimage').click(function () {
    var size = $('.sl-image-size').val();
    if (size == "") {
        _msgalert.error('Bạn phải chọn kích thước để crop ảnh.');
    } else {
        uploadCrop.croppie('result', {
            type: 'canvas',
            size: 'original'
        }).then(function (base64img) {
            // $('.wrap-croppie').hide();
            // $('#btn-upload-image').show();
            // $('#btn-cropimage').hide();
            // $('#btn-cancel-crop').hide();

            switch (size) {
                case "250x250":
                    $('#img_1x1').attr('src', base64img);
                    $('#img_1x1').closest('.image-croped').removeClass('mfp-hide');
                    break;
                case "250x187":
                    $('#img_4x3').attr('src', base64img);
                    $('#img_4x3').closest('.image-croped').removeClass('mfp-hide');
                    break;
                case "250x140":
                    $('#img_16x9').attr('src', base64img);
                    $('#img_16x9').closest('.image-croped').removeClass('mfp-hide');
                    break;
            }
            // $('.wrap-image-preview').show();
            // $('#image_file').val('');
            // $('.btn-dynamic-enable').prop('disabled', false);
        });
    }
});

$('#btn-cancel-crop').click(function () {
    $('.wrap-croppie').hide();
    // $('#btn-upload-image').show();
    $('#btn-cropimage').hide();
    $('#btn-cancel-crop').hide();
    $('.wrap-image-preview').show();
    $('#image_file').val('');
    $('.sl-image-size').val('');
    // $('.btn-dynamic-enable').prop('disabled', false);
});
var _news = {
    modal_element: $('.modal'),
    ShowAddOrUpdate: function (id, parent_id = 0) {
        debugger
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} menu`;
        let url = '/news/AddOrUpdate';
        _news.modal_element.find('.modal-title').html(title);
        _news.modal_element.find('.modal-dialog').css('max-width', '680px');
        _ajax_caller.get(url, { id: id, parent_id: parent_id }, function (result) {
            _news.modal_element.find('.modal-body').html(result);
            _news.modal_element.modal('show');
        });
    },


    ShowAddOrUpdateFromAI: function () {
        debugger;

        const aiArticles = JSON.parse(localStorage.getItem('aiArticles') || '[]');
        const lastAI = aiArticles.length > 0 ? aiArticles[aiArticles.length - 1] : null;
        const currentId = parseInt($('#Id').val()) || 0;

        // Ưu tiên sửa bài đang mở (đã lưu DB), fallback dùng bài AI trong local
        const openId = currentId > 0 ? currentId : (lastAI?.Id ? parseInt(lastAI.Id) : 0);

        if (openId <= 0) {
            _msgalert.error("Không tìm thấy bài viết nào để sửa.");
            return;
        }

        _news.modal_element.find('.modal-title').html("Sửa bài viết");
        _news.modal_element.find('.modal-dialog').css('max-width', '680px');

        _ajax_caller.get('/news/AddOrUpdate', { id: openId }, function (result) {
            _news.modal_element.find('.modal-body').html(result);

            // ✅ Nếu có AI tương ứng bài đang sửa → gán dữ liệu AI
            if (lastAI && parseInt(lastAI.Id) === openId) {
                setTimeout(() => {
                    $('#CampaignName').val(lastAI.CampaignName || "");
                    $('#AiContent').val(lastAI.AiContent || "");
                    $(`input[name="PlatForm"][value="${lastAI.PlatForm}"]`).prop("checked", true);
                }, 300);
            }

            _news.modal_element.modal('show');
        });
    },


    // Cập nhật Ai
    OnSave: function () {
        debugger;

        const data = {
            Id: parseInt($('#Id').val()) || 0, // vẫn lấy Id từ view chính
            CampaignName: $('#modal-CampaignName').val(),
            PlatForm: parseInt($('input[name="PlatForm"]:checked', _news.modal_element).val()),
            AiContent: $('#modal-AiContent').val(),
            AimodelType: 1
        };

        // ⚠️ Nếu chọn web → reset flag IsPostedToFanpage
        if (data.PlatForm !== 1) {
            $('#IsPostedToFanpage').val(0);
        }

        if (!data.AiContent) {
            _msgalert.error("Bạn cần nhập nội dung để gửi lên AI.");
            return;
        }


        const platformText = data.PlatForm === 1 ? "facebook" : "web";
        const payload = {
            chatInput: data.AiContent,
            platform: platformText
        };

        $('#loadingOverlay').show();
        localStorage.removeItem('aiArticles');

        $.ajax({
            url: "https://n8n.adavigo.com/webhook/send-message",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                debugger
                $('#loadingOverlay').hide();

                // ✅ Gán kết quả AI trả về
                data.AiResult = res.content;
                data.Title = res.title || "";
                data.Lead = res.lead || "";
                data.Images = (res.img_lst || []).slice(0, 10);
                data.Keywords = res.keyword || [];

                // ✅ Lưu lại vào localStorage
                let aiArticles = JSON.parse(localStorage.getItem('aiArticles') || '[]');

                // Xoá nếu trùng Id (đã lưu)
                aiArticles = aiArticles.filter(item => parseInt(item.Id) !== data.Id);

                // Ghi lại bài mới
                aiArticles.push(data);
                localStorage.setItem('aiArticles', JSON.stringify(aiArticles));


                // ✅ Redirect tới trang chi tiết bài viết, gắn lại các tham số
                const redirectUrl = `/news/detail/${data.Id || 0}?fromAI=true&platform=${data.PlatForm}&AimodelType=1`;
                window.location.href = redirectUrl;
            },
            error: function (xhr, status, err) {
                $('#loadingOverlay').hide();
                console.error("❌ Gửi thất bại:", err);
                _msgalert.error("❌ Lỗi khi gửi lên AI.");
            }
        });
    },


    postToFanpage: async function (articleId) {
        const defaultImage = "https://static-image.adavigo.com/uploads/images/2025/1/23/9ac274a4-56ab-47fe-bbba-5a94daa84510.png";

        // ✅ Hiện loading
        $('#loadingPostFanpage').addClass('show');

        let images = JSON.parse($('#selectedImagesForFanpage').val() || '[]');

        if (images.length === 0) {
            $('#fanpage-db-wrapper .fanpage-img-option').each(function () {
                const url = $(this).attr('src');
                if (url) images.push(url);
            });
        }

        if (images.length === 0) {
            images = [defaultImage];
        }

        const imagesNeedConvert = images.filter(x => !x.includes("static-image.adavigo.com"));

        if (imagesNeedConvert.length > 0) {
            try {
                const res = await $.ajax({
                    url: '/news/ConvertImagesBeforePost',
                    method: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(imagesNeedConvert)
                });

                const staticImages = images.filter(x => x.includes("static-image.adavigo.com"));
                images = [...staticImages, ...res];
            } catch (err) {
                $('#loadingPostFanpage').removeClass('show');
                _msgalert.error("❌ Lỗi chuẩn hóa ảnh: " + err.message);
                return;
            }
        }

        $('#IsPostedToFanpage').val(1);

        _newsDetail.OnSave(0, function (savedId) {
            if (!savedId) {
                $('#loadingPostFanpage').removeClass('show');
                _msgalert.error("❌ Không thể lưu bài viết trước khi đăng.");
                return;
            }

            const rawContent = tinymce.activeEditor.getContent();
            const plainText = convertTinyMCEToPlainText(rawContent);
            const payload = {
                content: plainText,
                image_list: images
            };

            $.ajax({
                url: "https://n8n.adavigo.com/webhook/post-facebook",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {
                    $('#loadingPostFanpage').removeClass('show');

                    if (res.post_supports_client_mutation_id) {
                        _msgalert.success("✅ Đăng bài lên Fanpage thành công!");
                        $('#btn-post-fanpage').replaceWith('<button class="btn btn-success" disabled>Đã đăng lên Fanpage</button>');
                        window.location.href = "/news/detail/" + savedId;
                    } else {
                        _msgalert.error("❌ Đăng lên Fanpage thất bại.");
                    }
                },
                error: function () {
                    $('#loadingPostFanpage').removeClass('show');
                    _msgalert.error("❌ Lỗi khi kết nối tới N8n.");
                }
            });
        });
    }


};

var _newsDetail = {

    OnOpenRelationForm: function (id) {
        let title = 'Chèn tin liên quan';
        let url = '/News/RelationArticle';
        let param = { Id: id };
        _magnific.OpenLargerPopup(title, url, param);
    },
    RefreshlightGallery: function () {
        _wrapperImage.data('lightGallery').destroy(true);
        _wrapperImage.lightGallery();
    },

    //Lưu Bài Viết
    OnSave: function (articleStatus, callback = null) {
        debugger
        let formvalid = $('#form-news');

        // ✅ Kiểm tra xem bài viết là Facebook (PlatForm = 1) hay không
        const aiArticles = JSON.parse(localStorage.getItem('aiArticles') || '[]');
        const lastAI = aiArticles.length > 0 ? aiArticles[aiArticles.length - 1] : {};

        // 👇 Ưu tiên lấy từ DB (input hidden), nếu không có thì lấy từ localStorage
        const platformVal = parseInt($('#PlatForm').val() || lastAI?.PlatForm || 0);
        const aimodelTypeVal = parseInt($('#AimodelType').val() || lastAI?.AimodelType || 0);
        const campaignNameVal = $('#CampaignName').val() || lastAI?.CampaignName || "";
        const aiContentVal = $('#AiContent').val() || lastAI?.AiContent || "";
        const articleId = $('#Id').val() || lastAI?.Id || 0;
        const isPostedToFanpage = parseInt($('#IsPostedToFanpage').val() || 0);

        const isFanpage = platformVal === 1 && aimodelTypeVal === 1;
        const isWebsiteAI = platformVal === 0 && aimodelTypeVal === 1;
        const isManual = aimodelTypeVal === 0;
        // 👇 Thêm dòng này ngay sau xác định isFanpage
        if (!isFanpage) {
            $('#IsPostedToFanpage').val(0); // Nếu không còn là fanpage → reset lại
        }

        var max_pos = $('#ArticleType:checked').val() == "0" ? 7 : 8;

        formvalid.validate({
            rules: {
                Title: {
                    required: !isFanpage, // ⚠️ Nếu không phải fanpage thì bắt buộc
                    maxlength: 300
                },
                Lead: {
                    required: !isFanpage,
                    maxlength: 400
                },
                Position: {
                    min: 0,
                    max: max_pos
                }
            },
            messages: {
                Title:
                {
                    required: "Vui lòng nhập tiêu đề cho bài viết",
                    maxlength: "Tiêu đề không được vượt quá 300 ký tự"
                },
                Lead: {
                    required: "Vui lòng nhập mô tả ngắn cho bài viết",
                    maxlength: "Mô tả không được vượt quá 400 ký tự"
                },
                Position: {
                    min: "Vị trí phải trong khoảng 0 đến " + max_pos,
                    max: "Vị trí phải trong khoảng 0 đến " + max_pos
                }
            }
        });

        if (formvalid.valid() || isFanpage) {
            var _body = tinymce.activeEditor.getContent();
            var _tags = $('#news-tag').tagsinput?.('items') || [];
            var _categories = [];
            var _articleIdList = [];

            // ✅ Nếu KHÔNG PHẢI fanpage thì bắt buộc chọn chuyên mục
            if (!isFanpage && $('.ckb-news-cate:checked').length <= 0) {
                _msgalert.error('Bạn phải chọn chuyên mục cho bài viết');
                return false;
            }

            $('.ckb-news-cate:checked').each(function () {
                _categories.push($(this).val());
            });

            $('.item-related-article').each(function () {
                _articleIdList.push(parseFloat($(this).data('id')));
            });

            var _model = {
                Id: articleId,
                Title: $('#Title').val() || lastAI?.Title || "",
                Lead: $('#Lead').val() || lastAI?.Lead || "",
                Body: _body,
                Status: articleStatus,
                ArticleType: $('#ArticleType:checked').val(),
                Image169: $('#img_16x9').attr('src') || "",
                Image43: $('#img_4x3').attr('src') || "",
                Image11: $('#img_1x1').attr('src') || "",
                Tags: _tags,
                Categories: _categories,
                RelatedArticleIds: _articleIdList,
                PublishDate: ConvertToJSONDateTime($('#PublishDate').val()),
                DownTime: ConvertToJSONDateTime($('#DowntimeDate').val()),
                Position: $('#Position').val(),
                PlatForm: platformVal,
                CampaignName: campaignNameVal,
                AiContent: aiContentVal,
                AimodelType: aimodelTypeVal,
                IsPostedToFanpage: parseInt($('#IsPostedToFanpage').val())
            };

            // ✅ Nếu là bài video, dùng link video
            if (_model.ArticleType == 1) {
                _model.Body = $('#link-video').attr('src') || "";
            }

            // ✅ Fanpage yêu cầu có ảnh (ít nhất 1 ảnh); không cần tiêu đề, mô tả
            if (_model.Image169 == "" && _model.Image43 == "" && _model.Image11 == "") {
                _msgalert.error('Bạn phải upload ít nhất một ảnh đại diện cho tin bài');
                return false;
            }

            $.ajax({
                url: '/news/upsert',
                type: 'POST',
                data: JSON.stringify(_model),
                dataType: 'JSON',
                contentType: "application/json",
                traditional: true,
                success: function (result) {
                    debugger
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        const savedId = result.dataId;
                        // ✅ Lưu danh sách ảnh Fanpage nếu có
                        if (isFanpage) {
                            const selectedImages = JSON.parse($('#selectedImagesForFanpage').val() || '[]');
                            if (selectedImages.length > 0) {
                                $.ajax({
                                    url: "/news/SaveFanpageImages",
                                    type: "POST",
                                    contentType: "application/json",
                                    data: JSON.stringify({
                                        articleId: savedId,
                                        images: selectedImages
                                    }),
                                    success: function () {
                                        debugger
                                        console.log("✅ Đã lưu ảnh Fanpage vào DB");
                                    },
                                    error: function () {
                                        _msgalert.warn("⚠️ Không thể lưu ảnh Fanpage.");
                                    }
                                });
                            }
                        }
                        //localStorage.removeItem('aiArticles');

                        // ✅ callback nhận Id nếu có
                        if (typeof callback === "function") {
                            callback(result.dataId);
                        } else {
                            setTimeout(function () {
                                window.location.href = "/news/detail/" + result.dataId;
                            }, 300);
                        }

                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function () {
                    _msgalert.error('Đã xảy ra lỗi khi lưu bài viết.');
                }
            });
        } else {
            _msgalert.error('Bạn phải nhập thông tin đầy đủ và chính xác cho bài viết');
        }
    },

    OnChangeArticleStatus: function (id, status) {
        let actionName = '';
        let title = 'Cập nhật trạng thái bài viết';

        switch (parseInt(status)) {
            case 0:
                actionName = "đăng bài viết";
                break;
            case 2:
                actionName = "hạ bài viết";
                break;
        }

        let description = 'Bạn có chắc chắn muốn ' + actionName + '?';

        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: '/news/ChangeArticleStatus',
                type: 'POST',
                data: { Id: id, articleStatus: status },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        setTimeout(function () {
                            window.location.href = "/news/detail/" + result.dataId;

                        }, 200);
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (jqXHR) {
                }
            });
        });
    },

    OnDelete: function (id) {
        let title = 'Xác nhận xóa bài viết';
        let description = 'Bạn có chắc chắn muốn xóa bài viết này?';

        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: '/news/DeleteArticle',
                type: 'POST',
                data: { Id: id },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        setTimeout(function () {
                            window.location.href = "/news";
                        }, 200);
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (jqXHR) {
                }
            });
        });
    },
    Onchen: function (input) {
        var iframevideo = $('#iframe-video').attr('src') == undefined ? "" : $('#iframe-video').attr('src')
        if (input == 0 && iframevideo == "") {

            $('#normal_post').show();

            $('#video_post').hide();
        }
        if (input == 1 && iframevideo == "") {
            $('#normal_post').hide();
            $('#video_post').show();
            $('#video-preview').show();
        }
        if (input == 0 && iframevideo != "") {
            var result = confirm("Dữ liệu bài video sẽ bị xóa. Bạn có chắc chắn không ?");
            if (result == true) {
                $('#normal_post').show();
                $('#iframe-video').remove();
                location.reload('#video_post');
                $('#video_post').hide();
            }

        }
        if (input == 1 && iframevideo != "") {
            var result = confirm("Dữ liệu bài thường sẽ bị xóa. Bạn có chắc chắn không ?");
            if (result == true) {
                $('#normal_post').hide();
                $('#iframe-video').remove();
                $('#video_post').show();

            }
        }
        if (input == 0) {
            $('#Position').attr('max', '7');
        }
        if (input == 1) {
            $('#Position').attr('max', '8');
        }
    },
    DeleteVideo: function () {
        $('#iframe-video').remove();
        $('#video-preview').show();
        $('#video_post').show();
    },
    OnseverVideo: function () {
        var _model = {

            Body: $('#iframe-video').attr('src') == undefined ? "" : $('#iframe-video').attr('src'),
            ArticleType: $('#ArticleType:checked').val(),
        }
        console.log(_model.Body);

        $.ajax({
            url: '/AttachFile/UploadFileVideo',
            type: 'POST',
            data: JSON.stringify(_model),
            dataType: 'JSON',
            contentType: "application/json",
            traditional: true,
            // data: { model: _model },
            success: function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);

                    _attachfile.append('<video style="display:none"  class="col-md" id="link-video"  src="' + result.dataId + '" controls></video>');

                    /**/
                    _newsDetail.RefreshlightGallery();
                } else {
                    _msgalert.error(result.message);
                }
            },
            error: function (jqXHR) {

            }
        });
    },
    disabledView: function () {
        $('input[type="text"], input[type="checkbox"], input[type="radio"], select, button, textarea').prop('disabled', true);
        $('#ha_bai_viet').prop('disabled', false);
    },
    EditNewDetail: function (id, status) {
        let title = 'Xác nhận hạ bài viết';
        let description = 'Bài viết đang hiển thị ngoài mặt trang sẽ bị hạ.Bạn có đồng ý không?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: '/news/ChangeArticleStatus',
                type: 'POST',
                data: { Id: id, articleStatus: status },
                success: function (result) {
                    if (result.isSuccess) {
                        _msgalert.success(result.message);
                        setTimeout(function () {
                            window.location.href = "/news/detail/" + result.dataId;

                        }, 200);
                    } else {
                        _msgalert.error(result.message);
                    }
                },
                error: function (jqXHR) {
                }
            });
        });
    },
}