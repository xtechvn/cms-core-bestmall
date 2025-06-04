
$(document).ready(function () {
    flashsale.Initialization()
})
var flashsale = {
    Data: {
        Search: {
            fromdate: '01/01/2025 00:00',
            todate: '01/01/2025 00:00',
            page_index: 1,
            page_size: 10,
            status:0
        }
    },
    Initialization: function () {
        flashsale.DynamicBind()
        flashsale.SingleDateTimePicker($('#flashsale-search-fromdate'))
        flashsale.SingleDateTimePicker($('#flashsale-search-todate'))
        flashsale.SearchData()
    },
    DynamicBind: function () {

        $('body').on('click', '.flashsale-search-status', function () {
            var element = $(this);
            // Remove 'btn-active' from all buttons
            $('.flashsale-search-status').removeClass('btn-active');
            // Add 'btn-active' to the clicked button
            element.addClass('btn-active');
            flashsale.SearchData()
        });
        $('body').on('click', '.flashsale-search-status', function () {
            var element = $(this);
            // Remove 'btn-active' from all buttons
            $('.flashsale-search-status').removeClass('btn-active');
            // Add 'btn-active' to the clicked button
            element.addClass('btn-active');
            flashsale.SearchData()
        });
      
    },
    onSelectPageSize: function () {
        flashsale.Data.Search.page_size = $("#selectPaggingOptions").find(':selected').val()
        flashsale.SearchData();
    },
    OnPaging: function (value) {
        flashsale.Data.Search.page_index = value
        flashsale.SearchData();
    },
    SearchData: function () {
        var searchModel = flashsale.GetModel();
        this.Search(searchModel);
        $(document).load().scrollTop(0);

    },
    GetModel: function () {
        flashsale.Data.Search.fromdate=  _global_function.GetDayText($('#flashsale-search-fromdate').data('daterangepicker').startDate._d, true)
        flashsale.Data.Search.todate = _global_function.GetDayText($('#flashsale-search-todate').data('daterangepicker').startDate._d, true)
        $('.flashsale-search-status').each(function () {
            let self = $(this);
            if (self.hasClass('btn-active')) {
                flashsale.Data.Search.status = self.attr('data-id')
                return false
            }
        });
        return flashsale.Data.Search
    },
    Search: function (input) {
        $('.flashsale-search').addClass('placeholder')
        $.ajax({
            url: "/flashsale/Search",
            type: "post",
            data: input,
            success: function (result) {
                $('#flashsale-data').html(result)
                $('.flashsale-search').removeClass('placeholder')

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },

    SingleDateTimePicker: function (element) {

        var today = new Date();
        var yyyy = today.getFullYear();
        var yyyy_max = yyyy + 10;
        var current_date = element.val();
        var min_range = '01/01/2025 00:00';
        var max_range = '31/12/' + yyyy_max + ' 23:59';
        var mm = today.getMonth() + 1; // Months start at 0!
        var dd = today.getDate();
        var hh = today.getHours();
        var minutes = today.getMinutes()
        if (current_date == undefined || current_date == null || current_date.trim() == '') {
            current_date = dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + minutes
        }

        element.daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            showDropdowns: true,
            drops: 'down',
            minDate: min_range,
            maxDate: max_range,
            timePicker: true,
            timePicker24Hour: true,
            locale: {
                format: 'DD/MM/YYYY HH:mm',
                // Thêm các thuộc tính Việt hóa cho tháng và ngày
                applyLabel: 'Áp dụng',
                cancelLabel: 'Hủy bỏ',
                fromLabel: 'Từ',
                toLabel: 'Đến',
                customRangeLabel: 'Tùy chỉnh',
                weekLabel: 'W',
                daysOfWeek: [
                    'CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'
                ],
                monthNames: [
                    'Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6',
                    'Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12'
                ],
                firstDay: 1
            }
        }, function (start, end, label) {
            element.data('daterangepicker').setStartDate(start);
            flashsale.SearchData()

        });
        //element.data('daterangepicker').setStartDate(current_date);
    },
}