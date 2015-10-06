
//global variables
var Floor = new Floor();
var timeSlide;
var resEndTimer;
var timeSlideValue = 0;
var isAddEditResPanelOpen = false;
var timeSlidePaused = true;
var hidePopUpOnClick = false;
var closedPopUpOnClick = false;
//var updatedTime = (new Date()).getMinutes();
var updatedTime = (serverDateTime).getMinutes();
var isMouseOverTimer = false;
var isMouseOverDateRefresh = false;
var currentDateRefreshVal = 'Today';
var tempTimeRemaining = 60;
var pinEnabled = false;
var hideWaitPopUpOnClick = false;
var _alert;

// window onload code starts here

window.onload = function () {
    AssignLemonSlider('#bottomslider', false); // Bind lemmon slider to Sections at bottom of floor.
    AssignLemonSlider('#topslider', false);
};

// document ready code starts here
$(function () {
    $(".wrapper").addClass('wrapper-floor');
    $('body').css('background', 'url("../images/body-bg.png") repeat scroll 0 0 rgba(0, 0, 0, 0)');
    var className = $('.middle-section').attr('class');
    document.getElementById("aFloor").className = "active";
    if (className == "middle-section floor middle-section-toggle2 middle-section-toggle1" || className == "middle-section floor middle-section-toggle1 middle-section-toggle2") {
        $(".middle-section").addClass("middle-section-view");
    }
    else {
        $(".middle-section").removeClass("middle-section-view");
    }

    /***** Datepicker bindings *****/

    $("#cdate").datepicker({
        dateFormat: 'DD, d M, y'
    }).datepicker("setDate", initialDate);

    $('.dateStatus').click(function () {
        $('#cdate').datepicker("show");
    });

    $('#countDownRef').hover(function () {
        isMouseOverTimer = true;
        $(this).val('Refresh');
    }, function () {
        isMouseOverTimer = false;

        if (!timeSlidePaused) {
            //var secs = (60 - (new Date()).getSeconds());
            var secs = (60 - (serverDateTime).getSeconds());
            UpdateTimerButtonValue('0:' + ((secs < 10) ? ('0' + secs) : secs));
        }
        else {
            if (tempTimeRemaining < 60) {
                UpdateTimerButtonValue('0:' + ((tempTimeRemaining < 10) ? ('0' + tempTimeRemaining) : tempTimeRemaining));
            }
            else {
                UpdateTimerButtonValue('0:00');
            }
        }
    });

    $('#currentDateRef').hover(function () {
        isMouseOverDateRefresh = true;
        $(this).val('Today');
    }, function () {
        isMouseOverDateRefresh = false;
        $(this).val(currentDateRefreshVal);
    });

    e = $("#cdate").val();
    var da = e.replace(',', '').split(' ');
    $(".date").text(da[1]);
    $(".day").text(da[0].substring(0, 3));
    $(".month").text(da[2].replace(',', ''));

    // Binding top Level tabs events

    $('.top-bar ul li a').click(function () {
        //debugger;
        if (isAddEditResPanelOpen) {
            //            if (confirm("This will close the ADD/EDIT Reservation panel on right side and you will lost all the pending changes. Do you still want to continue?")) {
            //                isAddEditResPanelOpen = false;
            //                Floor.OpenReservationList();
            //            }

            var tblDDL;
            var lvlTables;
            var lvlText = ($(this).find('strong').text()).split('-')[0];

            if ($('#AdResAddiUpdateForm').length > 0) {
                tblDDL = $("#AdResAddiOpts #ddlLevel");
                lvlTables = tblDDL.find("option:contains('" + lvlText + "')");
            }
            else {
                tblDDL = $("#AdResDDL #ddlTable");
                lvlTables = tblDDL.find("option:contains('" + lvlText + "')");
            }

            if (lvlTables.length > 0) {
                var firstLvlTable = lvlTables.eq(0).val();
                tblDDL.val(firstLvlTable).change();
            }
            else {
                alert("Sorry, there is no table available for reservation on this floor. please select another floor for reservation.");
                var selectedIndex = $('.top-bar').find("li > a[class ='active']").parent().index();
                $('#topslider').trigger('slideTo', selectedIndex);
            }
        }
        else {
            var floorPlanId = +$(this).next().val();
            $('#SelectedFloorId').val(floorPlanId);
            Floor.currentFloorId = floorPlanId;
            RefreshFloorTime();
        }
    });

    //************ Drag & Drop tables on floor *************//

    $("#floorArea").droppable();

    $("#floorArea .table-main").live('mouseover', function () {
        $(this).draggable({
            containment: '#floorArea',
            cancel: ".table-main .show-tooltip-left"
        });
    });


    //************* Floor click events *****************//
    //$("#floorArea .quan-2-other1,.quan-2-1,.quan-4-1").live('click', function (e) {
    //    e.stopPropagation();
    //    $(".quan-2-other1,.quan-2-1,.quan-4-1").removeClass("secHover");
    //    $(this).addClass("secHover");
    //});

    $("#floorArea").live('click', function () {
        $(".seat,.quan-2-other1,.quan-2-1,.quan-4-1,.item img").removeClass("secHover");
        $('.show-tooltip-left').removeClass('show-tooltip-left-toggle');
        $(".frnt-right-popup").removeClass("df");
    });


    // Binding jquery UI tabs
    $("#tabs").tabs();
    $("#addResTabs").tabs();

    // Binding Cluetip popup plugin
    //BindClueTip('a.jt');

    // Binding styled select list design
    DesignStyledSelectList('.shiftStatus', false);

    // Binding custom design for dropdowns
    DesignCustomDropdowns();

    /***** Time Slider ****/

    var $slideMe = $("<div/>")
            .css({ position: 'absolute', top: 30, left: 0, border: '1px solid #E8867C', background: '#E8867C', color: '#FFFFFF', padding: '3px', 'border-radius': '2px', 'z-index': 99 })
            .text("04:00")
            .hide();

    //var d = new Date();
    var d = serverDateTime;

    var hr = (d.getHours() < 10 ? '0' + d.getHours() : d.getHours());
    var min = parseInt(d.getMinutes()) - parseInt((d.getMinutes() % 15));
    var defaultTime = parseInt((hr * 60) + min);
    timeSlideValue = defaultTime;

    min = (min < 10) ? "0" + min : min;

    Floor.currentTimeIn15MinSlot = hr + ':' + min;

    var tt = hr >= 12 ? 'PM' : 'AM';
    hr = hr <= 12 ? hr : hr - 12;
    hr = (hr < 10 ? '0' + hr : hr);

    $("#sldTime").text(hr + ':' + ((d.getMinutes() < 10) ? "0" + d.getMinutes() : d.getMinutes()) + tt);
    $slideMe.text(hr + ':' + ((d.getMinutes() < 10) ? "0" + d.getMinutes() : d.getMinutes()) + tt);

    $("#slider").slider({
        min: 0,
        max: 1439, // 4AM to 3:45AM = 1425 min
        step: 1,
        value: defaultTime, // 4AM
        slide: function (event, ui) {

            var curhours = Math.floor(ui.value / 60);
            var curtt = curhours >= 12 ? 'PM' : 'AM';
            curhours = curhours <= 12 ? curhours : curhours - 12;
            curhours = curhours < 10 ? '0' + curhours : curhours;
            var curminutes = (ui.value % 60) < 10 ? '0' + (ui.value % 60) : (ui.value % 60);

            var timeDiff = (ui.value % 15);
            var now = ui.value - timeDiff;

            var hours = Math.floor(now / 60) < 10 ? '0' + Math.floor(now / 60) : Math.floor(now / 60);
            var minutes = (now % 60) < 10 ? '0' + (now % 60) : (now % 60);

            $("#sldTime").text(curhours + ':' + curminutes + curtt);
            $slideMe.text(curhours + ':' + curminutes + curtt);
            Floor.currentTimeIn15MinSlot = hours + ':' + minutes;
        },
        change: function (event, ui) {

            var timeDiff = (ui.value % 15);
            var now = ui.value - timeDiff;

            if (event.originalEvent === undefined) {
                var curhours = Math.floor(ui.value / 60);
                var curtt = curhours >= 12 ? 'PM' : 'AM';
                curhours = curhours <= 12 ? curhours : curhours - 12;
                curhours = curhours < 10 ? '0' + curhours : curhours;
                var curminutes = (ui.value % 60) < 10 ? '0' + (ui.value % 60) : (ui.value % 60);

                var hours = Math.floor(now / 60) < 10 ? '0' + Math.floor(now / 60) : Math.floor(now / 60);
                var minutes = (now % 60) < 10 ? '0' + (now % 60) : (now % 60);

                $("#sldTime").text(curhours + ':' + curminutes + curtt);
                $slideMe.text(curhours + ':' + curminutes + curtt);

                timeSlideValue = ui.value;

                Floor.currentTimeIn15MinSlot = hours + ':' + minutes;
                //Floor.UpdateDataOnTimeChange(hours + ':' + minutes);
                Floor.UpdateFloorPlan(hours + ':' + minutes, 'TIMESLIDE');

                //                if ((ui.value % 15) == 14) {
                //                    //debugger;
                //                    $('#EndResPopUp #ResDate').val(Floor.currentDate);
                //                    $('#EndResPopUp #TimeInMin').val(ui.value);
                //                    $('#EndResPopUp').submit();
                //                }
            }
            else {
                if (!isAddEditResPanelOpen) {
                    StartTempTimer();
                }
            }
        },
        stop: function (event, ui) {
            var timeDiff = (ui.value % 15);
            var now = ui.value - timeDiff;

            var curhours = Math.floor(ui.value / 60);
            var curtt = curhours >= 12 ? 'PM' : 'AM';
            curhours = curhours <= 12 ? curhours : curhours - 12;
            curhours = curhours < 10 ? '0' + curhours : curhours;
            var curminutes = (ui.value % 60) < 10 ? '0' + (ui.value % 60) : (ui.value % 60);

            var hours = Math.floor(now / 60) < 10 ? '0' + Math.floor(now / 60) : Math.floor(now / 60);
            var minutes = (now % 60) < 10 ? '0' + (now % 60) : (now % 60);

            $("#sldTime").text(curhours + ':' + curminutes + curtt);
            $slideMe.text(curhours + ':' + curminutes + curtt);

            if (isAddEditResPanelOpen) {
                if (confirm("This will close the ADD/EDIT Reservation panel on right side and you will lost all the pending changes. Do you still want to continue?")) {
                    isAddEditResPanelOpen = false;
                    Floor.OpenReservationList();
                    timeSlideValue = ui.value;
                    Floor.currentTimeIn15MinSlot = hours + ':' + minutes;
                    Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
                    Floor.UpdateFloorPlan(hours + ':' + minutes, 'TIMESLIDE');
                }
                else {
                    $("#slider").slider({ value: timeSlideValue });
                }
            }
            else {
                Floor.currentTimeIn15MinSlot = hours + ':' + minutes;
                Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
                Floor.UpdateFloorPlan(hours + ':' + minutes, 'TIMESLIDE');
                //Floor.UpdateDataOnTimeChange(hours + ':' + minutes);
            }
        }
    })
    .find(".ui-slider-handle")
    .append($slideMe)
    .hover(function () {
        var timeText = $('#slider').slider("option", "value");
        var hours = Math.floor(timeText / 60);
        var tt = hours >= 12 ? 'PM' : 'AM';
        hours = hours <= 12 ? hours : hours - 12;
        hours = hours < 10 ? '0' + hours : hours;
        var minutes = (timeText % 60) < 10 ? '0' + (timeText % 60) : (timeText % 60);
        $slideMe.text(hours + ':' + minutes + tt);
        $slideMe.show();
    },
    function () {
        $slideMe.stop().hide();
    });

    // SlideTime(false);
    if (!isAddEditResPanelOpen) {
        StartSlideTimer();
    }
    // bind shift li click get time 
    $(".shiftStatus li").click(function () {
        Floor.UpdateFloorPlan('', 'SHIFTCHANGE');
        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
    });

    Floor.SetRotateToFloorItems();
    Floor.BindFloorTableEvents();

    // start check reservation end counter
    BindFullScreenButtonEvents();
    //StartCheckReservationEndTimer();
    ScrollToCurrentTime();
    BindALLPopovers('ul.reslist li.popUp', '.popUpContent');

    consume_alert();
});

function consume_alert() {
    if (_alert) return;
    _alert = window.alert;
    window.alert = function (message, options) {
        var defaultOptions = {
            title: false,
            text: message,
            addclass: 'custom',
            styling: 'fontawesome',
            nonblock: {
                nonblock: true,
                nonblock_opacity: .2
            },
            delay: 8000
        };

        if (arguments.length > 1)
            $.extend(defaultOptions, options);

        new PNotify(defaultOptions);
    };
}

function changeCdate(e) {
    e = $(e).val();
    var da = e.replace(',', '').split(' ');
    $(".date").text(da[1]);
    $(".day").text(da[0].substring(0, 3));
    $(".month").text(da[2].replace(',', ''));

    Floor.UpdateCurrentValues();
    Floor.IsDateChanged = IsDateNotToday();
    //debugger;
    if (Floor.IsDateChanged === true) {
        $('#currentDateRef').val((da[2].replace(',', '')) + ' ' + da[1]).css('background', 'Red');
        currentDateRefreshVal = (da[2].replace(',', '')) + ' ' + da[1];
    }
    else {
        $('#currentDateRef').val('Today').css('background', 'Green');
        currentDateRefreshVal = 'Today';
    }

    if ($('#floorSideAddRes').css('display') != 'none') { //($('#AdResDDL').length > 0) {
        $('#AdResUpdateForm #resDate').val(Floor.currentDate).change();
        if ($('#AdResAddiUpdateForm').length > 0) {
            $('#AdResAddiUpdateForm #resDate').val(Floor.currentDate).change();
        }
        else { }
    }
    else if ($('#floorSideAddWait').css('display') != 'none') {
        $('#AdWaitSaveForm #WaitDate').val(Floor.currentDate).change();
    }
    else {
        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
        Floor.UpdateFloorPlan('', 'SHIFTCHANGE');
    }
}


function SlideTime(isPageRefresh) {
    //var now = new Date();
    var now = serverDateTime;
    var timeNow = now.getHours() + ':' + now.getMinutes();
    now = (now.getHours() * 60) + now.getMinutes();

    //    if ((now % 15) == 0) {
    //        $("#slider").slider({ value: now });
    //        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
    //    }
    //    else if (isPageRefresh) {
    //        var timeDiff = (now % 15);
    //        now = now - timeDiff;
    //        $("#slider").slider({ value: now });
    //        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
    //    }

    if (isPageRefresh) {
        var timeDiff = (now % 15);
        now = now - timeDiff;

        $("#slider").slider({ value: now });
        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
    }
    else {
        $("#slider").slider({ value: now });
        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
    }

}

function IsDateNotToday() {
    var selectedDate = $('#cdate').datepicker('getDate');
    //var today = new Date();
    var today = new Date(serverDateTime.getFullYear(), serverDateTime.getMonth(), serverDateTime.getDate());
    today.setHours(0);
    today.setMinutes(0);
    today.setSeconds(0);
    if (Date.parse(today) == Date.parse(selectedDate)) {
        return false;
    } else {
        return true;
    }
}

function RefreshFloorDate() {

    if (isAddEditResPanelOpen) {
        if (confirm("This will close the ADD/EDIT Reservation panel on right side and you will lost all the pending changes. Do you still want to continue?")) {
            isAddEditResPanelOpen = false;
            Floor.OpenReservationList();
            RefreshFloorDate();
        }
        else { }
    }
    else {
        //$("#cdate").datepicker("setDate", new Date()).change();
        console.log(serverDateTime);
        $("#cdate").datepicker("setDate", serverDateTime).change();
        console.log(serverDateTime);
    }
}


/**** Time Slider functions ****/

function StartSlideTimer() {

    $('#countDownRef').css('background', 'Green');

    if (timeSlidePaused)
        timeSlide = setInterval(function () {
            UpdateTimer();
        }, 1000);

    timeSlidePaused = false;
}

function StopSlideTimer() {
    clearInterval(timeSlide);
    timeSlidePaused = true;

    UpdateTimerButtonValue('0:00');
}

function StartTempTimer() {
    tempTimeRemaining = 60;

    $('#countDownRef').css('background', 'Red');
    StopSlideTimer();

    timeSlide = setInterval(function () {
        UpdateTempTimer();
    }, 1000);

    timeSlidePaused = true;
}

function StopTempTimer() {
    clearInterval(timeSlide);
    //timeSlidePaused = false;
    $('#countDownRef').css('background', 'Green');

    UpdateTimerButtonValue('0:00');

    tempTimeRemaining = 60;

    StartSlideTimer();
}

function UpdateTimer() {
    //var curMin = (new Date()).getMinutes();
    var curMin = (serverDateTime).getMinutes();
    if (updatedTime != (curMin)) {
        //debugger;
        SlideTime(false);
        updatedTime = curMin;
    }

    //var secs = (60 - (new Date()).getSeconds());
    var secs = (60 - (serverDateTime).getSeconds());

    UpdateTimerButtonValue('0:' + ((secs < 10) ? ('0' + secs) : secs));
}

function UpdateTempTimer() {
    if (tempTimeRemaining > 0) {
        tempTimeRemaining -= 1;

        UpdateTimerButtonValue('0:' + ((tempTimeRemaining < 10) ? ('0' + tempTimeRemaining) : tempTimeRemaining));
    }
    else {
        SlideTime(false);
        StopTempTimer();
    }
}

function UpdateTimerButtonValue(value) {
    if (!isMouseOverTimer) {
        $('#countDownRef').val(value);
    }
}

function RefreshFloorTime() {

    if (isAddEditResPanelOpen) {
        if (confirm("This will close the ADD/EDIT Reservation panel on right side and you will lost all the pending changes. Do you still want to continue?")) {
            isAddEditResPanelOpen = false;
            Floor.OpenReservationList();
            RefreshFloorTime();
        }
        else { }
    }
    else {
        if (timeSlidePaused) {
            StopTempTimer();
        }
        else { }

        if ($('#floorSideAddRes').css('display') != 'none') {  //($('#AdResDDL').length > 0) {
            $('#AdResUpdateForm #resDate').val(Floor.currentDate).change();
            if ($('#AdResAddiUpdateForm').length > 0) {
                $('#AdResAddiUpdateForm #resDate').val(Floor.currentDate).change();
            }
            else { }
        }
        else { }

        SlideTime(false);
    }
}

/*******************/

/*******************/

function EditTable(e) {
    return false;
}

function AssignLemonSlider(selector, isRebuild) {
    if (isRebuild) {
        $(selector).lemmonSlider('destroy');
    }

    $(selector).lemmonSlider({
        infinite: false
    });
}

function BindClueTip(selector) {  //'a.jt'
    $(selector).cluetip({
        cluetipClass: 'jtip',
        arrows: true,
        dropShadow: false,
        width: 352,
        sticky: true,
        mouseOutClose: false,
        local: true,
        multiple: false,
        cursor: 'pointer',
        ajaxCache: false,
        attribute: 'rel',
        closePosition: 'title',
        closeText: '<img src="/images/btn-close.png" alt="close" />',
        onActivate: function (event) {
            if (hidePopUpOnClick) {
                return false;
            }
        },
        onHide: function (ct, ci) {
            if (hidePopUpOnClick) {
                RefreshFloorTime();
            }
            hidePopUpOnClick = false;
            $(this).parent().removeClass("active");
        }
    });

    $(selector).click(function () {
        if (!hidePopUpOnClick) {
            hidePopUpOnClick = true;
            $(this).parent().addClass("active");
            // 2015/01/21 changes;
            StopSlideTimer();
            var time = $(this).find('p:eq(1) span').clone()
            .children()
            .remove()
            .end()
            .text()
            .trim();

            var splitTime = time.split(':');
            var hrs = parseInt(splitTime[0]);
            var splitMin = splitTime[1].split(' ');
            var min = parseInt(splitMin[0]);
            var totalMin = ((((splitMin[1] == 'AM') ? hrs : (hrs + 12)) * 60) + min);

            $('#slider').slider({ value: totalMin });
        }
    });

    $(selector).mouseout(function (e) {
        if (!hidePopUpOnClick) {
            $(document).trigger('hideCluetip');
        }

        e.stopPropagation();
    });
}

function BindClueTipNEW(selector) {  //'a.jt'
    $(selector).mouseover(function (event) {
        //debugger;
        var self = $(this);

        if (!self.data('cluetip-initd')) {
            self.find('.frnt-right-popup').show();
            self.cluetip({
                cluetipClass: 'jtip',
                arrows: true,
                dropShadow: false,
                width: 352,
                sticky: true,
                mouseOutClose: false,
                local: true,
                multiple: false,
                cursor: 'pointer',
                ajaxCache: false,
                attribute: 'rel',
                closePosition: 'title',
                closeText: '<img src="/images/btn-close.png" alt="close" />',
                onActivate: function (event) {
                    if (hidePopUpOnClick) {
                        return false;
                    }
                },
                onHide: function () {
                    if (hidePopUpOnClick) {
                        //RefreshFloorTime();
                    }
                    hidePopUpOnClick = false;
                    $(this).parent().removeClass("active");
                }
            }).data('cluetip-initd', true);
        }

        //event.preventDefault();
    });

    $(selector).click(function () {
        if (!hidePopUpOnClick) {
            hidePopUpOnClick = true;
            $(this).parent().addClass("active");
            // 2015/01/21 changes;
            StopSlideTimer();
            var time = $(this).find('p:eq(1) span').clone()
            .children()
            .remove()
            .end()
            .text()
            .trim();

            var splitTime = time.split(':');
            var hrs = parseInt(splitTime[0]);
            var splitMin = splitTime[1].split(' ');
            var min = parseInt(splitMin[0]);
            var totalMin = ((((splitMin[1] == 'AM') ? hrs : (hrs + 12)) * 60) + min);

            $('#slider').slider({ value: totalMin });
        }
    });

    $(selector).mouseout(function (e) {
        if (!hidePopUpOnClick) {
            $(document).trigger('hideCluetip');
        }

        e.stopPropagation();
    });
}

function HideClueTip() {
    $(document).trigger('hideCluetip');
}

function BindPopovers(selector, contentSelector) {
    var popOver = $(selector);
    var popOverContent = popOver.find(contentSelector).eq(0);
    popOver.popover({
        html: true,
        container: 'body',
        content: function () {
            return popOverContent.html();
        },
        trigger: 'hover',
        placement: function (tip, element) {
            var $element, TopLimit, BottomLimit;
            $element = $(element);
            pos = $.extend({}, $element.offset(), {
                width: element.offsetWidth,
                height: element.offsetHeight
            });

            TopLimit = 265;
            BottomLimit = $(document).height() - TopLimit;

            if (TopLimit > pos.top) {
                return "leftTop";
            } else if (BottomLimit < pos.top) {
                return "leftBottom";
            }
            else {
                // default
                return "left";
            }
        }
    }).on('show.bs.popover', function (e) {
        if (hidePopUpOnClick) {
            return false;
        }
    }).on('shown.bs.popover', function (e) {
        $('div.popover .close').off('click').on('click', function () {
            closedPopUpOnClick = true;
            popOver.popover('hide');
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
            RefreshFloorTime();
        }
        hidePopUpOnClick = false;
        $(this).parent().removeClass("active");
    }).on('click', function (e) {
        if (!hidePopUpOnClick) {
            hidePopUpOnClick = true;
            $(this).addClass("active");
            // 2015/01/21 changes;
            StopSlideTimer();
            var time = $(this).find('p:eq(1) span').clone()
            .children()
            .remove()
            .end()
            .text()
            .trim();

            var splitTime = time.split(':');
            var hrs = parseInt(splitTime[0]);
            var splitMin = splitTime[1].split(' ');
            var min = parseInt(splitMin[0]);
            var totalMin = ((((splitMin[1] == 'PM' && hrs !== 12) ? (hrs + 12) : hrs) * 60) + min);

            $('#slider').slider({ value: totalMin });

            popOverContent.find('.resEditOption form').submit();
        }
        e.stopPropagation();
    });
}

function BindALLPopovers(selector, contentSelector) {
    $('body .popover').remove();
    var popOver = $(selector);
    popOver.each(function () {
        var self = this, doBind = function () {
            var popOverContent = $(self).find(contentSelector).eq(0);
            $(self).popover({
                html: true,
                container: 'body',
                content: function () {
                    return popOverContent.html();
                },
                trigger: 'hover',
                placement: function (tip, element) {
                    var $element, TopLimit, BottomLimit;
                    $element = $(element);
                    pos = $.extend({}, $element.offset(), {
                        width: element.offsetWidth,
                        height: element.offsetHeight
                    });

                    TopLimit = 265;
                    BottomLimit = $(document).height() - TopLimit;

                    if (TopLimit > pos.top) {
                        return "leftTop";
                    } else if (BottomLimit < pos.top) {
                        return "leftBottom";
                    }
                    else {
                        // default
                        return "left";
                    }
                }
            }).on('show.bs.popover', function (e) {
                if (hidePopUpOnClick) {
                    return false;
                }
            }).on('shown.bs.popover', function (e) {
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
                    RefreshFloorTime();
                }
                hidePopUpOnClick = false;
                $(this).parent().removeClass("active");
            }).on('click', function (e) {
                if (!hidePopUpOnClick) {
                    hidePopUpOnClick = true;
                    $(this).addClass("active");
                    // 2015/01/21 changes;
                    StopSlideTimer();
                    var time = $(this).find('p:eq(1) span').clone()
                    .children()
                    .remove()
                    .end()
                    .text()
                    .trim();

                    var splitTime = time.split(':');
                    var hrs = parseInt(splitTime[0]);
                    var splitMin = splitTime[1].split(' ');
                    var min = parseInt(splitMin[0]);
                    var totalMin = ((((splitMin[1] == 'PM' && hrs !== 12) ? (hrs + 12) : hrs) * 60) + min);

                    $('#slider').slider({ value: totalMin });

                    popOverContent.find('.resEditOption form').submit();
                }
                e.stopPropagation();
            });
        };
        $.queueEvent.add(doBind, this);
    });
}

function BindALLWaitListPopovers(selector, contentSelector) {
    $('body .popover').remove();
    var popOver = $(selector);
    popOver.each(function () {
        var self = this, doBind = function () {
            var popOverContent = $(self).find(contentSelector).eq(0);
            $(self).popover({
                html: true,
                container: 'body',
                content: function () {
                    return popOverContent.html();
                },
                trigger: 'hover',
                placement: function (tip, element) {
                    var $element, TopLimit, BottomLimit;
                    $element = $(element);
                    pos = $.extend({}, $element.offset(), {
                        width: element.offsetWidth,
                        height: element.offsetHeight
                    });

                    TopLimit = 265;
                    BottomLimit = $(document).height() - TopLimit;

                    if (TopLimit > pos.top) {
                        return "leftTop";
                    } else if (BottomLimit < pos.top) {
                        return "leftBottom";
                    }
                    else {
                        // default
                        return "left";
                    }
                }
            }).on('show.bs.popover', function (e) {
                if (hideWaitPopUpOnClick) {
                    return false;
                }
            }).on('shown.bs.popover', function (e) {
                $('div.popover .close').off('click').on('click', function () {
                    hideWaitPopUpOnClick = false;
                    $(self).popover('hide');
                });
            }).on('hide.bs.popover', function (e) {
                if (hideWaitPopUpOnClick) {
                    return false;
                }
            }).on('hidden.bs.popover', function (e) {
                hideWaitPopUpOnClick = false;
                $(self).removeClass("active");
            }).on('click', function (e) {
                if (!hideWaitPopUpOnClick) {
                    hideWaitPopUpOnClick = true;
                    $(self).addClass("active");
                }
                //e.stopPropagation();
            });
        };
        $.queueEvent.add(doBind, this);
    });
}

function HidePopovers() {
    closedPopUpOnClick = false;
    hidePopUpOnClick = false;
    hideWaitPopUpOnClick = false;
    $('.popUp').popover('hide');
}

function DesignStyledSelectList(selector, isRebuild) {
    if (isRebuild) {
        $(selector).find('.styledSelect').remove();
    }
    // Iterate over each select element
    $(selector).find('select').each(function () {  //.shiftStatus

        // Cache the number of options
        var $this = $(this);
        numberOfOptions = $(this).children('option').length;

        // Hides the select element
        $this.addClass('s-hidden');

        // Wrap the select element in a div
        $this.wrap('<div class="select"></div>');

        // Insert a styled div to sit over the top of the hidden select element
        $this.after('<div class="styledSelect"></div>');

        // Cache the styled div
        var $styledSelect = $this.next('div.styledSelect');

        // Show the first select option in the styled div
        $styledSelect.text($this.children('option').eq(0).text());

        // Insert an unordered list after the styled div and also cache the list
        var $list = $('<ul />', {
            'class': 'options'
        }).insertAfter($styledSelect);

        // Insert a list item into the unordered list for each select option
        for (var i = 0; i < numberOfOptions; i++) {
            $('<li />', {
                text: $this.children('option').eq(i).text(),
                rel: $this.children('option').eq(i).val()
            }).appendTo($list);
        }

        // Cache the list items
        var $listItems = $list.children('li');

        // Show the unordered list when the styled div is clicked (also hides it if the div is clicked again)
        $styledSelect.click(function (e) {
            e.stopPropagation();
            $('div.styledSelect.active').each(function () {
                $(this).removeClass('active').next('ul.options').hide();
            });
            $(this).toggleClass('active').next('ul.options').toggle();
        });

        // Hides the unordered list when a list item is clicked and updates the styled div to show the selected list item
        // Updates the select element to have the value of the equivalent option
        $listItems.click(function (e) {
            e.stopPropagation();
            $styledSelect.text($(this).text()).removeClass('active');
            $this.val($(this).attr('rel'));
            $list.hide();
            /* alert($this.val()); Uncomment this for demonstration! */
        });
        // Hides the unordered list when clicking outside of it
        $(document).click(function () {
            $styledSelect.removeClass('active');
            $list.hide();
        });
    });

}

function DesignCustomDropdowns(selector) {
    var location;

    if (selector) {
        location = $(selector + ' .custom-select');
    }
    else {
        location = $('.custom-select');
    }

    location.each(function () {
        var self = this;
        if ($(self).parents('.select-wrapper').length === 0) {
            $(self).wrap("<span class='select-wrapper'></span>");
            $(self).after("<span class='holder'></span>");
            var selectedOption = $(this).find(":selected").text();
            $(self).next(".holder").text(selectedOption);

            $(self).change(function () {
                var selectedOption = $(this).find(":selected").text();
                $(this).next(".holder").text(selectedOption);
            });
        }
    });

    //location.change(function () {
    //    var selectedOption = $(this).find(":selected").text();
    //    $(this).next(".holder").text(selectedOption);
    //});
}

function BindFullScreenButtonEvents() {
    $('.full-screen-view').click(function () {
        if ($('.full-screen-view').hasClass('normal-view')) {
            $('.menu-bar').removeClass('menu-bar-toggle');
            $('.m-left-btn').removeClass('m-left-btn-toggle');
            //$('.middle-section').toggleClass('middle-section-toggle1');
            $('.full-screen-view').removeClass('normal-view');
            $('.manage-section').removeClass('manage-section-toggle');
            $('.m-right-btn').removeClass('m-right-btn-toggle');
            //$('.middle-section').toggleClass('middle-section-toggle2');
            //$('.middle-section').toggleClass('middle-section-view');
            SetMiddleWidthOnSideButtonClick();
        }
        else {
            $('.menu-bar').addClass('menu-bar-toggle');
            $('.m-left-btn').addClass('m-left-btn-toggle');
            //$('.middle-section').toggleClass('middle-section-toggle1');
            $('.full-screen-view').addClass('normal-view');
            $('.manage-section').addClass('manage-section-toggle');
            $('.m-right-btn').addClass('m-right-btn-toggle');
            //$('.middle-section').toggleClass('middle-section-toggle2');
            //$('.middle-section').toggleClass('middle-section-view');
            SetMiddleWidthOnSideButtonClick();
        }
    });
}

function BindjQueryEditable(targetSelector, title, placement, editButtonSelector, targetInputToUpdate) {
    $(targetSelector).editable({
        send: 'never',
        title: title,
        placement: placement,
        toggle: 'manual',
        display: function (value) {
            $(targetSelector).text(value);
            if (targetInputToUpdate !== undefined && targetInputToUpdate !== null) {
                $(targetInputToUpdate).val(value);
            }
        }
    });

    $(editButtonSelector).click(function (e) {
        e.stopPropagation();
        $(targetSelector).editable('toggle');
    });
}

function EditReservationButtonEvent(resId) {
    hidePopUpOnClick = false;
    Floor.OpenAddReservation(resId, null, false);
}

/******** Check Reservation Ending methods *******/

//var resChkTimerMinute = (new Date()).getMinutes();
var resChkTimerMinute = (serverDateTime).getMinutes();

function StartCheckReservationEndTimer() {
    resEndTimer = setInterval(function () {
        CheckForReservationEnding();
    }, 5000);
}

function CheckForReservationEnding() {
    //var now = new Date();
    var now = serverDateTime;
    now = (now.getHours() * 60) + now.getMinutes();

    if ((now % 15) == 14) {
        //var eResCurMin = (new Date()).getMinutes();
        var eResCurMin = (serverDateTime).getMinutes();
        if (resChkTimerMinute != (eResCurMin)) {
            //debugger;
            //$('#EndResPopUp #ResDate').val(Floor.currentDate);
            //$('#EndResPopUp #ResDate').val($.datepicker.formatDate('DD, d M, y', new Date()));
            $('#EndResPopUp #ResDate').val($.datepicker.formatDate('DD, d M, y', serverDateTime));
            $('#EndResPopUp #TimeInMin').val(now);
            $('#EndResPopUp').submit();

            resChkTimerMinute = eResCurMin;
        }
    }
}

function StopCheckReservationEndTimer() {
    clearInterval(resEndTimer);
}


/*****************/

function AfterSaveOrCancelRes() {
    //var d = new Date();
    var d = serverDateTime;
    var hr = (d.getHours() < 10 ? '0' + d.getHours() : d.getHours());
    var min = parseInt(d.getMinutes()) - parseInt((d.getMinutes() % 15));
    min = (min < 10) ? "0" + min : min;

    Floor.currentTimeIn15MinSlot = hr + ':' + min;
}