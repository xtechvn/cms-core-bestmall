
$(document).ready(function () {
    _client_contact.Initialization()
})
var _client_contact = {
    Model: {
        keyword: '',
        page_index: 1,
        page_size: 10,
        reached_end: false,
        on_excuting: false
    },
    Initialization: function () {
        var model = [{ url: '/', name: 'Trang chủ' }, { url: '/clientcontact', name: 'Thông tin liên hệ', activated: true }]
        _global_function.RenderBreadcumb(model)
        _client_contact.Listing();
        _client_contact.DynamicBind()
    },
    DynamicBind: function () {
        $('body').on('click', '.btn-search-product', function () {
            $('#client-contact-list').html('')
            _client_contact.ResetSearch()
            _client_contact.Listing();
        });

        $("#client-contact-keyword").on('keyup', function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                if (_client_contact.Model.reached_end == false) {
                    $('#client-contact-list').html('')
                    _client_contact.ResetSearch()
                    _client_contact.Listing();
                }
            }
        });

        $('body').on('select2:select', '#item-per-page', function () {
            _client_contact.ResetSearch()

            _client_contact.Listing();
        });
        //--scroll event
        $(window).scroll(function () {
            if ($(window).scrollTop() >= $('.main-products table').offset().top + $('.main-products table').outerHeight() - window.innerHeight) {
                _client_contact.Listing()
            }
        });
    },
    ResetSearch: function () {
        _client_contact.Model.page_index = 1;
        _client_contact.Model.page_index = 1;
        _client_contact.Model.on_excuting = false;
        _client_contact.Model.reached_end = false;
        $('.count').attr('data-value', '0')
        $('.count').text('0')
        $('#client-contact-list').closest('.table-responsive').addClass('placeholder')
        $('.hanmuc').closest('.flex-lg-nowrap').addClass('placeholder')
    },
    Listing: function () {
        if (_client_contact.Model.reached_end == true || _client_contact.Model.on_excuting == true) {
            return;
        }
        _client_contact.Model.on_excuting = true
        function normalizeText(input) {
            return input
                .normalize("NFC")

                //.replace(/[()]/g, "")             // Loại bỏ dấu ngoặc đơn
                .replace(/\s+/g, ' ')             // Xóa khoảng trắng thừa
                .trim();
        }
        var request = {
            keyword: normalizeText($('#client-contact-keyword').val()), // Làm sạch từ khóa
            page_index: _client_contact.Model.page_index,
            page_size: parseInt($('#item-per-page').find(':selected').val())
        }
        _client_contact.POST('/ClientContact/Search', request, function (result) {

            $('#client-contact-list').append(result)
            $('#client-contact-list').closest('.table-responsive').removeClass('placeholder')
            $('.hanmuc').closest('.flex-lg-nowrap').removeClass('placeholder')
            _client_contact.Model.on_excuting = false
            _client_contact.Model.page_index++
            // Gán tổng số sản phẩm vào phần tử với class 'count'
            var current_count = $('.count').attr('data-value')
            var recent_count = $('#search-count').html()
            if (current_count == undefined) current_count = '0';
            if (recent_count == undefined) recent_count = '0';
            if (result.total_count == undefined || result.total_count <= 0) {
                $('.count').attr('data-value', (parseFloat(recent_count) + parseFloat(current_count)))
            }
            $('.count').text(result.total_count || (parseFloat(recent_count) + parseFloat(current_count)));
            $('.hanmuc').closest('.flex-lg-nowrap').find('.count').html(parseFloat(recent_count))
            $('#search-count').remove()
        })
       

    },
    POST: function (url, model, callback) {
        $.ajax({
            url: url,
            type: "POST",
            data: model,
            success: function (result) {
                callback(result)
            },
            error: function (err) {
                console.log(err)
            }
        });
    },
}