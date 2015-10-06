$(function () {
    $('.custom_check').screwDefaultButtons({
        image: "url(/images/checkbox.png)",
        width: 24,
        height: 24
    });

    $('#all').change(function (event) {  //on click
        if (this.checked) { // check select status
            $('input[name = selectedTables]').not($('input[name = selectedTables][checked = checked]')).click();
        } else {
            $('input[name = selectedTables]').not($('input[name = selectedTables]:not([checked = checked])')).click();
        }
    });

    setTimeout(function () {
        $('.middle-section').css('height', 'auto');
    }, 500);
});