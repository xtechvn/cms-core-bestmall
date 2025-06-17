$(document).ready(function () {
    _news.Init();
});

$('.btn-toggle-cate').click(function () {
    var seft = $(this);
    if (seft.hasClass("plus")) {
        seft.siblings('ul.lever2').show();
        seft.removeClass('plus').addClass('minus');
    } else {
        seft.siblings('ul.lever2').hide();
        seft.removeClass('minus').addClass('plus');
    }
});

$('.ckb-news-cate').click(function () {
    var seft = $(this);
    var ul_lever2 = seft.parent().siblings('ul.lever2');
    if (ul_lever2.length > 0) {
        var btn_toggle = seft.parent().siblings('a.btn-toggle-cate');
        if (seft.is(":checked")) {
            ul_lever2.find('.ckb-news-cate').prop('checked', true);
            if (btn_toggle.hasClass('plus')) {
                btn_toggle.trigger('click');
            }
        } else {
            if (btn_toggle.hasClass('minus')) {
                btn_toggle.trigger('click');
            }
            ul_lever2.find('.ckb-news-cate').prop('checked', false);
        }
    }
});

var _news = {
    Init: function () {
        let _searchModel = {
            Title: null,
            ArticleId: -1,
            FromDate: null,
            ToDate: null,
            AuthorId: -1,
            ArticleStatus: -1,
            ArrCategoryId: null,
            SearchType: 0
        };

        let objSearch = {
            searchModel: _searchModel,
            currentPage: 1,
            pageSize: 20
        };
        //Ai
        this.modal_element = $('#global_modal_popup');

        this.SearchParam = objSearch;
        this.Search(objSearch);
    },

    ShowAddOrUpdate: function (id, parent_id = 0) {
        debugger
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} Bài Viết AI`;
        let url = '/news/AddOrUpdate';
        _news.modal_element.find('.modal-title').html(title);
        _news.modal_element.find('.modal-dialog').css('max-width', '680px');
        _ajax_caller.get(url, { id: id, parent_id: parent_id }, function (result) {
            _news.modal_element.find('.modal-body').html(result);
            _news.modal_element.modal('show');
        });
    },

    OnSave: function () {
        debugger

        const data = {
            Id: parseInt($('#Id').val()) || 0, // vẫn lấy Id từ view chính
            CampaignName: $('#modal-CampaignName').val(),
            PlatForm: parseInt($('input[name="PlatForm"]:checked', _news.modal_element).val()),
            AiContent: $('#modal-AiContent').val(),
            AimodelType: 1
        };
        if (!data.AiContent) {
            alert("Bạn cần nhập nội dung để gửi lên AI.");
            return;
        }
        const platformText = data.PlatForm === 1 ? "facebook" : "web";
        // 🔥 Bắn lên N8n
        const payload = {
            chatInput: data.AiContent,
            platform: platformText,
        };
        // ✨ Show loading
        $('#loadingOverlay').show();



        $.ajax({
            url: "https://n8n.adavigo.com/webhook/send-message",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                debugger
                // ✅ Gán kết quả AI trả về
                $('#loadingOverlay').hide(); // ✅ Hide loading
                data.AiResult = res.content;
                data.Title = res.title || "";            // Nếu có tiêu đề từ AI
                data.Lead = res.lead || "";              // Nếu có mô tả từ AI
                // 🔥 Chỉ lấy tối đa 5 ảnh đầu tiên nếu có
                data.Images = (res.img_lst || []).slice(0, 10);
                data.Keywords = res.keyword || [];       // Từ khóa AI sinh ra
                console.log("✅ Phản hồi từ N8n:", res);

                // ✅ Lưu vào localStorage
                let aiArticles = JSON.parse(localStorage.getItem('aiArticles') || '[]');

                // Xoá bài trùng theo Id nếu có
                aiArticles = aiArticles.filter(item => item.Id !== data.Id);

                // Thêm bài mới vào cuối
                aiArticles.push(data);

                // Ghi lại
                localStorage.setItem('aiArticles', JSON.stringify(aiArticles));



                // ✅ Điều hướng sang trang khác để render nội dung
                if (data.PlatForm === 0) {
                    window.location.href = "/news/detail/0?fromAI=true&platform=" + data.PlatForm + "&AimodelType=1";
                } else if (data.PlatForm === 1) {
                    window.location.href = "/news/detail/0?fromAI=true&platform=" + data.PlatForm + "&AimodelType=1";
                }

            },
            error: function (xhr, status, err) {
                $('#loadingOverlay').hide(); // ❌ Hide on error
                console.error("❌ Gửi thất bại:", err);
                alert("❌ Lỗi khi gửi lên AI. Kiểm tra console để xem chi tiết.");
            }
        });
    },

    Search: function (input) {
        $.ajax({
            url: "/news/search",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid-data').html(result);
                var total = $('#data-total-record').val();
                $('#total-article-filter').text(total);
            }
        });
    },

    OnPaging: function (value) {
        var objSearch = this.SearchParam;
        objSearch.currentPage = value;
        this.Search(objSearch);
    },

    BasicSearch: function () {
        var objSearch = this.SearchParam;
        objSearch.searchModel.Title = $('#BasicTitle').val().trim();
        objSearch.searchModel.AuthorId = -1;
        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    AdvanceSearch: function () {
        var objSearch = this.SearchParam;

        var _arrCateGory = [];
        $('.ckb-news-cate').each(function () {
            if ($(this).is(":checked")) {
                _arrCateGory.push(parseInt($(this).val()));
            }
        });

        objSearch.searchModel.Title = $('#AdvanceTitle').val().trim();
        objSearch.searchModel.ArticleId = $('#ArticleId').val() <= 0 ? -1 : $('#ArticleId').val();
        objSearch.searchModel.FromDate = $('#FromDate').val();
        objSearch.searchModel.ToDate = $('#ToDate').val();
        objSearch.searchModel.AuthorId = $('#AuthorId').val();
        objSearch.searchModel.ArticleStatus = $('#ArticleStatus').val();
        objSearch.searchModel.ArrCategoryId = _arrCateGory;
        objSearch.searchModel.SearchType = 1;

        objSearch.currentPage = 1;
        this.Search(objSearch);
    },

    OnOpenAdvanceSearch: function () {
        $('.fillter-search').slideDown();
        $('.dynamic-dom').hide();
    },

    OnCloseAdvanceSearch: function () {

        this.SearchParam.searchModel.Title = null;
        this.SearchParam.searchModel.ArticleId = -1;
        this.SearchParam.searchModel.FromDate = null;
        this.SearchParam.searchModel.ToDate = null;
        this.SearchParam.searchModel.AuthorId = -1;
        this.SearchParam.searchModel.ArticleStatus = -1;
        this.SearchParam.searchModel.ArrCategoryId = null;
        this.SearchParam.searchModel.SearchType = 0;

        $('.fillter-search').slideUp();
        $('#form-advance-search').trigger("reset");
        $('.dynamic-dom').show();
    },

    OpenCategoryPanel: function () {
        $('#panel-category-filter ul.lever2').show();
        $('#panel-category-filter .btn-toggle-cate').removeClass('plus').addClass('minus');
    },

    CloseCategoryPanel: function () {
        $('#panel-category-filter ul.lever2').hide();
        $('#panel-category-filter .btn-toggle-cate').removeClass('minus').addClass('plus');
    },
    LoadPageView: function (list_id) {
        if (list_id != undefined && list_id.length>0) {
            $.ajax({
                url: '/News/GetPageViewByList',
                type: 'POST',
                data: {
                    article_id: list_id,
                },
                success: function (result) {
                    if (result != undefined && result.length > 0) {
                        
                        result.forEach(function (item) {
                            $('#pv_' + item.articleID).html(item.pageview);
                        });
                    }
                },
                error: function (jqXHR) {
                },
            });
        }
       
    }
};