function BindClueTip(selector) {
    $(selector).cluetip({
        cluetipClass: 'jtip',
        arrows: true,
        dropShadow: false,
        width: 352,
        sticky: true,
        mouseOutClose: true,
        local: true,
        multiple: false,
        cursor: 'pointer',
        ajaxCache: false,
        attribute: 'rel',
        closePosition: 'none',
        onShow: function (ct, ci) {
            //debugger;
            try {
                $(ci).find(".content_1").mCustomScrollbar();

                $(".table-tooltip-inner.content_1 .row").off('click').click(function (e) {
                    $(".table-tooltip-inner.content_1 .row").removeClass("rowSelected");
                    $(e.currentTarget).addClass("rowSelected");

                    var rid = $(e.currentTarget).attr('id');

                    var edit = $($(e.currentTarget)).parents('.cluetip-inner').find('li.edit-tool').find('a');
                    edit.attr('href', '/FloorPlan/FloorPlan?ReservationId=' + rid);

                    var del = $($(e.currentTarget)).parents('.cluetip-inner').find('.delete-tool').find('a');
                    del.click(function () {
                        deleteReservation(rid);
                    });
                });

            } catch (e) {
                //debugger;
            }
        }
    });
}

function SortCalendarReservation(source) {
    //debugger;
    var resList = $($(source).parents('table').find('tbody tr'));

    var sortedList = null;

    if ($(source).data('order') == "asc") {
        sortedList = resList.sort(SortReservationsByTimeAsc);
        $(source).data('order', 'desc');
    }
    else {
        sortedList = resList.sort(SortReservationsByTimeDesc);
        $(source).data('order', 'asc');
    }

    $(source).parents('table').find('tbody').html(sortedList);

    resList = null;
}

function SortReservationsByTimeAsc(a, b) {
    return (($(b).data('time')) < ($(a).data('time'))) ? 1 : -1;
}

function SortReservationsByTimeDesc(a, b) {
    return (($(b).data('time')) > ($(a).data('time'))) ? 1 : -1;
}

/**** shift notes****/


function showDayShiftNotes() {
    $("#shiftDiv").css("display", "block");
}
function hideDayShiftNotes() {
    $("#shiftDiv").css("display", "none");
    GetShiftNote();
    return false;
}
function successShiftNotes(data) {
    if (data.result) {
        alert(data.msz);
        GetShiftNote();
    } else {
        alert(data.msz);
    }
    hideDayShiftNotes();
}

function GetShiftNote() {
    var date = new Date();

    if ($("#datepicker").length > 0)
        date = $("#datepicker").datepicker('getDate');

    var dataObj = {
        Date: $.datepicker.formatDate('D, M d, yy', date),
        Type: $("#shiftDiv #Type").val()
    };

    $('#shiftDiv').load("/Calendar/GetShiftNote", dataObj, function () {
        $($(this).find('form')).removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse($($(this).find('form')));
    });
}

/**********************/