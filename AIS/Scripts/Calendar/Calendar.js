function BindClueTip(selector) {

    $(selector).cluetip({
        cluetipClass: 'jtip',
        arrows: true,
        dropShadow: false,
        width: 352,
        sticky: true,
        //mouseOutClose: true,
        local: true,
        hoverIntent: false,
        multiple: false,
        cursor: 'pointer',
        ajaxCache: false,
        attribute: 'rel',
        closePosition: 'none',
        activation: 'click',

        //hoverClass: '',
        onShow: function (ct, ci) {
            // 
            try {
                //$(".ddd").tooltip();

                $(ci).find(".tooltipstable").tooltip({
                    html: true,
                    placement: 'top',
                    container: 'body',
                    //trigger: 'click',
                    template: '<div class="tooltip blackToltop" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',

                });


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
                $(".table-tooltip-inner.content_1 .row").off('click').click(function (e) {



                });

            } catch (e) {
                // 
            }
        }
    });
}

function SortCalendarReservation(source) {
    // 
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



function BindALLPopovers(selector, contentSelector) {
    var hidePopUpOnClick = false;
    var closedPopUpOnClick = false;
    $('body .popover').remove();
    var popOver = $(selector);
    popOver.each(function () {

        var self = this, doBind = function () {
            var popOverContent = $(self).parent().find(contentSelector).eq(0);
            $(self).popover({
                html: true,
                container: 'body',
                content: function () {

                    return popOverContent.html();
                },
                trigger: 'hover',
                //placement: function (tip, element) {
                //    $(tip).find(".content_1").mCustomScrollbar();
                //    var $element, TopLimit, BottomLimit;
                //    $element = $(element);
                //    pos = $.extend({}, $element.offset(), {
                //        width: element.offsetWidth,
                //        height: element.offsetHeight
                //    });
                //    //alert("11")
                //    TopLimit = 265;
                //    BottomLimit = $(document).height() - TopLimit;

                //    if (TopLimit > pos.top) {
                //        return "leftTop";
                //    } else if (BottomLimit < pos.top) {
                //        return "leftBottom";
                //    }
                //    else {
                //        // default
                //        return "left";
                //    }
                //}
            }).on('show.bs.popover', function (e) {

                $(".popover-title").hide();
                closedPopUpOnClick = false;

                if (hidePopUpOnClick) {
                    return false;
                }
            }).on('shown.bs.popover', function (e) {
                   
                $('div.popover').find('.content_1').mCustomScrollbar();
                $('div.popover').find(".tooltipstable").tooltip({
                    html: true,
                    placement: 'top',
                    container: 'body',
                    //trigger: 'click',
                    template: '<div class="tooltip blackToltop" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',

                });
                $(".popover-title").hide();
                $('div.popover .close').off('click').on('click', function () {
                    closedPopUpOnClick = true;
                    $(self).popover('hide');

                });
            }).on('hide.bs.popover', function (e) {
                if (hidePopUpOnClick) {
                    if (!closedPopUpOnClick) {
                        return false;
                    }
                }
            }).on('hidden.bs.popover', function (e) {

                if (hidePopUpOnClick && closedPopUpOnClick) {
                    closedPopUpOnClick = false;

                }
                hidePopUpOnClick = false;
                $(this).parent().removeClass("active");
            }).on('click', function (e) {
                if (!hidePopUpOnClick) {
                    hidePopUpOnClick = true;
                    $(this).addClass("active");


                }
                e.stopPropagation();
            });
        };
        $.queueEvent.add(doBind, this);
    });
}



function SetRow(e) {

    $(e).parent().find('.rowSelected').removeClass("rowSelected");
    $(e).addClass("rowSelected");

    var rid = $(e.currentTarget).attr('id');

    var edit = $($(e.currentTarget)).parents('.cluetip-inner').find('li.edit-tool').find('a');
    edit.attr('href', '/FloorPlan/FloorPlan?ReservationId=' + rid);

    var del = $($(e.currentTarget)).parents('.cluetip-inner').find('.delete-tool').find('a');
    del.click(function () {
        deleteReservation(rid);
    });
}


function EditRow() {

    var rid = $(".row.rowSelected").attr('id')
    if (typeof (rid) === "undefined" || rid === ' ') { }
    else {
        window.location.href = '/FloorPlan/FloorPlan?ReservationId=' + rid
    }
}

function deleteReservation() {

    var id = $(".row.rowSelected").attr('id')

    if (typeof (id) === "undefined" || id === ' ') { }
    else {
        alertify.confirm("Delete Reservations", "Are you sure you want to delete.", function (e) {
            if (e) {
                $.ajax({
                    type: 'POST',
                    data: { ReservationId: id },
                    url: '/Reservation/DeleteReservation',
                    success: function (data) {
                        getMonthList();
                        $("#" + id).remove();
                    }
                });

            } else {

            }

        }, function () {
        }).set('labels', { ok: 'Ok!', cancel: 'cancel' }).set('movable', true);

    }


}





/**********************/