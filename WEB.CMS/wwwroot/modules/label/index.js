$(document).ready(function () {
    label_service.Init()
})
var label_service = {
    SearchParam: {

    },
    Init: function () {
        label_service.OnSearch();
    },
    OnSearch: function () {
        let objSearch = label_service.GetParam();
        label_service.SearchParam = objSearch;
        label_service.Search(objSearch);
    },
    OnPaging: function (value) {
        var objSearch = this.GetParam()
        objSearch.currentPage = value;
        label_service.SearchParam = objSearch
        label_service.Search(objSearch);
    },
    GetParam: function () {
        let objSearch = {
            name: $('#search-name').val() != undefined ? $('#search-name').val().trim().replaceAll(/  +/g, ' ') : null,
            code: $('#search-code').val() != undefined ? $('#search-code').val().trim().replaceAll(/  +/g, ' ') : null,
            status: $('#search-status').find(':selected') != null && $('#search-status').find(':selected') != undefined ? $('#search-status').find(':selected').val():'-1',
            currentPage: 1,
            pageSize: 10
        }
        return objSearch;
    },
    Search: function (input) {
        window.scrollTo(0, 0);
        $('#grid_data').addClass('placeholder');
        $.ajax({
            url: "/Label/Search",
            type: "Post",
            data: input,
            success: function (result) {
                $('#grid_data').html(result);
                $('#grid_data').removeClass('placeholder');
            }
        });
    },
    Update: function (id) {
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} thương hiệu `;
        let url = '/label/edit';

        $('#global_modal_popup').find('.modal-title').html(title);
        $('#global_modal_popup').find('.modal-dialog').css('max-width', '1200px');

        _ajax_caller.get(url, { label_id: id }, function (result) {
            $('#global_modal_popup').find('.modal-title').html(title);
            $('#global_modal_popup').find('.modal-body').html(result);
            $('#global_modal_popup').modal('show');
            $('.modal-backdrop').css('z-index', '0')

        });
    }
}