$(function () {
    $.ajaxSetup({
        statusCode: {
            401: function () {
                window.location.reload();
            }
        }
    });
});