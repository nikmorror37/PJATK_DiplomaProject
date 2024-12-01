

$(window).on('resize', mobileVersion);
$(window).on('load', mobileVersion);

function mobileVersion() {
    if ($(window).width() < 990) {
        $('#heading').css('fontSize', '2rem');
        $('#search-btn').show();
        $('#search-form').addClass('collapse');
        $('.custom-file').removeClass('p-4');
        $('.custom-file').addClass('p-3');
        $('.hide-icon').hide();
        $('#submit-btn').addClass('btn-block');
        $('#filter-button').show();
        $('#filter-form-id').addClass('collapse');
        $('#filter-title').hide();
        $('.payment-input').removeClass('col-3');
        $('.mobile-toggle').hide();
        $('.button-group-icons').show();
    }
    else {
        $('#heading').css('fontSize', '3rem');
        $('#search-btn').hide();
        $('#search-form').removeClass('collapse');
        $('.custom-file').addClass('p-4');
        $('.custom-file').removeClass('p-3');
        $('.hide-icon').show();
        $('#submit-btn').removeClass('btn-block');
        $('#filter-button').hide();
        $('#filter-form-id').removeClass('collapse');
        $('#filter-title').show();
        $('.payment-input').addClass('col-3');
        $('.mobile-toggle').show();
        $('.button-group-icons').hide();
    }
}

$(document).ready(function () {
    var checkin = $("#check-in").attr('placeholder');
    var checkout = $("#check-out").attr('placeholder');

    $("#check-in").datepicker({
        showAnim: 'drop',
        numberOfMonths: 1,
        minDate: 0,
        maxDate: '+1Y',
        dateFormat: 'yy-MM-dd',
        onSelect: function () {
            var date = $(this).datepicker('getDate');
            if (date) {
                date.setDate(date.getDate() + 1);
                chosenCheckin = date;
            }
            $('#check-out').datepicker('option', 'minDate', date || 1);
        }
    }).val(checkin);
    $("#check-out").datepicker({
        showAnim: 'drop',
        numberOfMonths: 1,
        minDate: 1,
        maxDate: '+1Y',
        dateFormat: 'yy-MM-dd'
    }).val(checkout);
});

