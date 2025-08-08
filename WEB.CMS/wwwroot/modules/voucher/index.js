
$(document).ready(function () {

    voucher_index.Init();

})

var voucher_index = {
    Init: function () {
        this.OnSearch();
        voucher_index.DynamicBind()
    },
    DynamicBind: function () {
        $('body').on('click', '.search-tab', function () {
            var element=$(this)
            $('.search-tab').removeClass('active')
            element.addClass('active')
        })
        $('body').on('click', '.search-tab, #search-confirm', function () {
            voucher_index.OnSearch();

        })
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
    GetParam: function () {
        var status = 0;
        $('.search-tab').each(function (index, item) {
            var element = $(this)
            if (element.hasClass('active')) {
                status = element.attr('data-status')
                return false;
            }
        })
        let objSearch = {
            keyword: $('#search-keyword').val().trim(), 
            status: status,
            pageIndex: 1,
            pageSize: 15
        }
        return objSearch;
    },
    ReLoad: function () {
        this.Search(this.SearchParam);
    },
    Search: function (input) {
        window.scrollTo(0, 0);
        $('#imgLoading').show();
        $.ajax({
            url: "/Voucher/Search",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid_data').html(result);
            }
        });
    },

}