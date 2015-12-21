
// floor class containing floor specific functions and fields
function Floor() {
    var self = this;
    var baseUrl = '/FloorPlan/';
    var updatingReservationList = null;
    var updatingFloorPlan = null;
    self.currentDate = '';
    self.currentTimeIn15MinSlot = '';
    self.currentShift = '';
    self.currentFloorId = '';
    self.currentResTab = 'RESERVATION';
    self.currentResFilterTab = 'ALL';
    self.IsDateChanged = false;
    self.selectedDuration = null;

    self.UpdateCurrentValues = function () {
        self.currentDate = $("#cdate").val();
        self.currentShift = $("#shift").next().text();
        //self.currentFloorId = $("#SelectedFloorId").val();
    };

    self.OpenWaitingList = function () {
        self.OpenReservationList('WAITING');
    };

    self.OpenReservationList = function (openTab) {
        self.selectedDuration = null;
        //StartSlideTimer();
        //RefreshFloorTime();

        if (openTab === null || openTab === undefined) {
            openTab = '';
        }

        self.UpdateCurrentValues();
        //var url = baseUrl + "GetReservtionListPartial";
        var url = baseUrl + "GetReservtionListPartial";
        var dataObj = {
            Date: self.currentDate
        };

        if ($('.shiftStatus #shift').val() !== '') {
            dataObj.ShiftId = $('.shiftStatus #shift').val();
        }

        $('#floorSideTabs').show();
        $('#floorSideAddWait').hide();
        $('#floorSideAddRes').hide();

        $.ajax({
            type: "POST",
            data: dataObj,
            url: url,
            success: function (data) {
                HidePopovers();
                $('#rightContent #floorSideTabs').empty().append(data);
            },
            complete: function () {
                $(".frnt-right-rowb #tabs-1 .resInfo").css('height', ($(".manage-section").height() - 282));
                StartSlideTimer();
                //HideClueTip();
                $("#tabs").tabs();

                BindScroll('#rightContent', false);

                DesignCustomDropdowns();

                //BindClueTip('a.jt');

                if (openTab.length > 0) {
                    $('#tab' + openTab).click();
                }
                else if (self.currentResTab != 'RESERVATION') {
                    $('#tab' + self.currentResTab).click();
                }

                isAddEditResPanelOpen = false;

                $("#addTabs").tabs();

                //$('#floorSideTabs').show();
                //$('#floorSideAddRes').hide();
               
                BindALLPopovers('ul.reslist li.popUp', '.popUpContent');
            }
        });
    };

    self.OpenAddReservation = function (reservationId, covers, isWalkIn) {

        if (isWalkIn !== undefined && isWalkIn !== null && isWalkIn === true && self.IsDateChanged) {
            if ($("#cdate").datepicker('getDate') > serverDateTime) {
                //if ($("#cdate").datepicker('getDate') > new Date()) {
                //alert("WalkIns cannot be booked for a future date.");
                AlertInvlidPIN("WalkIns cannot be booked for a future date.");
            }
            else {
                //alert("WalkIns cannot be booked for a past date.");
                AlertInvlidPIN("WalkIns cannot be booked for a past date.");
            }
        }
        else {

            // 
            var useReservationFloor = false;
            var reloadOptions = false;
            StopSlideTimer();
            self.UpdateCurrentValues();
            // 
            var url = baseUrl + "GetAddReservtionPartial";

            var dataObj = {
                resDate: self.currentDate,
                time: self.currentTimeIn15MinSlot, //$('#AdResDDL #ddlTime').val(),
                tableIdd: '', //$('').val()
                isDateChanged: self.IsDateChanged
            };

            if (reservationId !== undefined && reservationId !== null) {
                dataObj.ReservationId = reservationId;
                useReservationFloor = true;
                reloadOptions = true;
            }
            else {
                //var d = new Date();
                var d = serverDateTime;
                var hr = (d.getHours() < 10 ? '0' + d.getHours() : d.getHours());
                var min = parseInt(d.getMinutes()) - parseInt((d.getMinutes() % 15));
                min = (min < 10) ? "0" + min : min;
                self.currentTimeIn15MinSlot = hr + ':' + min;

                dataObj.time = self.currentTimeIn15MinSlot;
            }

            if (covers !== undefined && covers !== null) {
                dataObj.Covers = covers;
                reloadOptions = true;
            }

            if (isWalkIn !== undefined && isWalkIn !== null) {
                dataObj.isWalkIn = isWalkIn;
            }

            $('#floorSideTabs').hide();
            $('#floorSideAddWait').hide();
            $('#floorSideAddRes').show();

            $.ajax({
                url: url,
                type: 'POST',
                data: dataObj,
                beforeSend: function () {
                    HidePopovers();
                    $('#rightContent .content_1').mCustomScrollbar("destroy");
                },
                success: function (data) {
                    $('#rightContent #floorSideAddRes').html(data);
                },
                complete: function () {
                    $(".frnt-right-rowb #addResTabs .content_1").css('height', ($(".manage-section").height() - 232));
                    //HideClueTip();

                    $("#addResTabs").tabs();
                    BindScroll('#rightContent', false);
                    self.IsDateChanged = false;
                    $('#AdResSaveForm').removeData('unobtrusiveValidation');
                    $.validator.unobtrusive.parse('#AdResSaveForm');
                    //            var validator = $("#AdResSaveForm").data('validator');
                    //            validator.settings.ignore = "";
                    //if ($('#AdResSaveForm #FloorPlanId').val() > 0) {
                    //self.currentFloorId = $('#AdResSaveForm #FloorPlanId').val();
                    AddReservationTimeChange($('#AdResDDL #ddlTime'), useReservationFloor);

                    self.selectedDuration = $('#AdResDDL #Duration').val();
                    //}
                    //else { }

                    isAddEditResPanelOpen = true;

                    $('#floorSideAddRes').show();
                    $('#floorSideAddWait').hide();
                    $('#floorSideTabs').hide();
                }
            });
        }
    };

    self.OpenAddWaiting = function (covers) {
        //StopSlideTimer();
        self.UpdateCurrentValues();
        // 
        var url = "/Waiting/GetAddToWaiting";

        var dataObj = {
            WaitDate: self.currentDate
        };

        if (covers !== undefined && covers !== null) {
            dataObj.Covers = covers;
        }

        $('#floorSideAddRes').hide();
        $('#floorSideTabs').hide();
        $('#floorSideAddWait').show();

        $.ajax({
            url: url,
            type: 'POST',
            data: dataObj,
            beforeSend: function () {
                HidePopovers();
                $('#rightContent .content_1').mCustomScrollbar("destroy");
            },
            success: function (data) {
                $('#rightContent #floorSideAddWait').html(data);
            },
            complete: function () {
                //HideClueTip();
                BindScroll('#rightContent', false);
                $('#AdWaitSaveForm').removeData('unobtrusiveValidation');
                $.validator.unobtrusive.parse('#AdWaitSaveForm');
            }
        });

        //$('#rightContent #floorSideAddWait').load(url, dataObj, function () {
        //    //HideClueTip();
        //    HidePopovers();
        //    BindScroll('#rightContent', false);
        //    $('#AdWaitSaveForm').removeData('unobtrusiveValidation');
        //    $.validator.unobtrusive.parse('#AdWaitSaveForm');
        //});
    };

    self.GetAddResAdditionalDetail = function () {
        self.UpdateCurrentValues();
        var url = baseUrl + "GetAddResAdditionalDetailPartial";

        UpdateAddReservationFormValues();

        $('#addResTabs-2').load(url, $('#AdResSaveForm').serialize(), function () {
            DesignCustomDropdowns();
            BindScroll('#addResTabs-2', false);
        });
    };

    self.UpdateWaitingList = function () {
        self.UpdateCurrentValues();
        // 
        var url = "/Waiting/GetAllWaitingList";

        var dataObj = {
            WaitDate: self.currentDate
        };

        $('#floorSideAddWait').hide();
        $('#floorSideAddRes').hide();
        $('#floorSideTabs').show();

        updatingReservationList = $.ajax({
            url: url,
            data: dataObj,
            beforeSend: function () {
                // 
                if (updatingReservationList != null) {
                    updatingReservationList.abort();
                }
            },
            success: function (data) {
                HidePopovers();
                $('#ResWaitList').html(data);
            },
            complete: function () {
                $(".frnt-right-rowb #tabs-2 .resInfo").css('height', ($(".manage-section").height() - 282));
                //HideClueTip();
                BindScroll('#ResWaitList', false);
                DesignCustomDropdowns('#ResWaitList ul.reslist');
                BindALLWaitListPopovers('#ResWaitList ul.reslist li.popUp', '.popUpContent');
            }
        });
    };

    self.UpdateReservationList = function (time, listfor, filter, isIncludeShift) {
        self.UpdateCurrentValues();

        var url = baseUrl;
        var targetElementUpdate = '';
        var dataObj = {
            Date: self.currentDate,
            Filter: filter
            //FloorPlanId: currentFloorId
        };

        if (time !== null && $.trim(time).length > 0) {
            dataObj.Time = time;
        }
            //else if (filter == 'UPCOMING' || filter == 'SEATED') {
            //    dataObj.Time = self.currentTimeIn15MinSlot;
            //}
            //else { }

            /**** 2015-06-17 *****/
        else {
            dataObj.Time = self.currentTimeIn15MinSlot;
        }
        /*************/

        switch (listfor.toUpperCase()) {
            case "RESERVATION":
                {
                    //url += 'GetAllReservationList';
                    url += 'GetAllReservationList20150617';
                    targetElementUpdate = '#ResAllList';
                    break;
                }
            case "WAITING":
                {
                    self.currentResTab = listfor.toUpperCase();
                    if ($('#floorSideAddWait').is(':hidden') && !hideWaitPopUpOnClick) {
                        self.UpdateWaitingList();
                    }

                    return;
                }
            case "STAFF":
                {
                    return;
                }

            default:
                break;
        }

        if (isIncludeShift) {
            dataObj.ShiftId = $('.shiftStatus #shift').val();
        }

        if (url.length > baseUrl.length) {
            updatingReservationList = $.ajax({
                type: "POST",
                data: dataObj,
                url: url,
                beforeSend: function () {
                    // 
                    if (updatingReservationList != null) {
                        updatingReservationList.abort();
                    }
                },
                success: function (data) {
                    //alert(data);
                    self.currentResTab = listfor.toUpperCase();
                    self.currentResFilterTab = filter;

                    $(targetElementUpdate).parents('.RTabs').find('ul.navStatus li a').removeClass('active');
                    $(targetElementUpdate).parents('.RTabs').find('ul.navStatus li a.res' + self.currentResFilterTab).addClass('active');

                    HidePopovers();

                    $('.frnt-right-rowb #tabs ' + targetElementUpdate)
                        .empty()
                        .append(data);
                    //document.getElementById('ResAllList').innerHTML = data;

                    //$(".frnt-right-popup .addDet").click(function () {
                    //    $(this).toggleClass("addexp");
                    //    $(".frnt-right-popup .addCont").toggleClass("show", 200);
                    //});
                },
                complete: function () {
                    DesignCustomDropdowns(targetElementUpdate + ' ul.reslist');
                    ScrollToCurrentTime();
                    BindALLPopovers(targetElementUpdate + ' ul.reslist li.popUp', '.popUpContent');
                }
            });
        }
    };

    self.UpdateReservationJSONList = function (time, listfor, filter, isIncludeShift) {
        self.UpdateCurrentValues();

        var url = baseUrl;
        var targetElementUpdate = '';
        var dataObj = {
            Date: self.currentDate,
            Filter: filter
        };

        if (time !== null && $.trim(time).length > 0) {
            dataObj.Time = time;
        }
        else {
            dataObj.Time = self.currentTimeIn15MinSlot;
        }

        switch (listfor.toUpperCase()) {
            case "RESERVATION":
                {
                    url += 'GetJSONAllReservationList';
                    targetElementUpdate = '#ResAllList';
                    break;
                }
            case "WAITING":
                {
                    self.currentResTab = listfor.toUpperCase();
                    self.UpdateWaitingList();
                    return;
                }
            case "STAFF":
                {
                    return;
                }

            default:
                break;
        }

        if (isIncludeShift) {
            dataObj.ShiftId = $('.shiftStatus #shift').val();
        }

        if (url.length > baseUrl.length) {
            updatingReservationList = $.ajax({
                type: "POST",
                data: dataObj,
                url: url,
                beforeSend: function () {
                    // 
                    if (updatingReservationList != null) {
                        updatingReservationList.abort();
                    }
                },
                success: function (data) {
                    //alert(data);
                    self.currentResTab = listfor.toUpperCase();
                    self.currentResFilterTab = filter;

                    $(targetElementUpdate).parents('.RTabs').find('ul.navStatus li a').removeClass('active');
                    $(targetElementUpdate).parents('.RTabs').find('ul.navStatus li a.res' + self.currentResFilterTab).addClass('active');

                    //HideClueTip();
                    HidePopovers();

                    // 
                    $('.frnt-right-rowb #tabs ' + targetElementUpdate)
                        .empty()
                        .append(data.HTMLArray.join(''));

                    $(".covStatus .sc").text(data.Covers);

                    $(".addDet").click(function () {
                        $(this).toggleClass("addexp");
                        $(".addCont").toggleClass("show", 200);
                    });
                },
                complete: function () {
                    DesignCustomDropdowns('ul.reslist');
                    ScrollToCurrentTime();
                    BindALLPopovers('ul.reslist li.popUp', '.popUpContent');
                }
            });
        }
    };

    self.UpdateFloorPlan = function (time, UpdateType) {
        self.UpdateCurrentValues();
        var url = baseUrl;

        var dataObj = {
            date: self.currentDate,
            FloorPlanId: self.currentFloorId,
        };

        if (self.selectedDuration !== null)
            dataObj.duration = self.selectedDuration;

        switch (UpdateType.toUpperCase()) {
            case "TIMESLIDE":
                {
                    url += 'UpdateFloorPlanOnTimeSlide';

                    if (time !== null && $.trim(time).length > 0) {
                        dataObj.time = time;
                    }
                    else {
                        dataObj.time = self.currentTimeIn15MinSlot;
                    }

                    break;
                }
                //            case "SHIFTCHANGE":
                //                {
                //                     
                //                    url += 'UpdateFloorPlan';
                //                    dataObj.time = $('#open :selected').val();
                //                    dataObj.shift = $("#shift").next().text();
                //                    break;
                //                }
            default:
                {
                    url += '_FloorPlanPartial';
                    dataObj.startTime = self.currentDate;
                    dataObj.shift = $("#shift").next().text();
                    break;
                }
        }

        updatingFloorPlan = $.ajax({
            type: "POST",
            data: dataObj,
            url: url,
            beforeSend: function () {
                // 
                if (updatingFloorPlan != null) {
                    updatingFloorPlan.abort()
                }
            },
            success: function (data) {
                $('#floorArea').empty().append(data);
                //document.getElementById('floorArea').innerHTML = data;
            },
            complete: function () {
                if (isAddEditResPanelOpen) {
                    QuickTableChangeService = null;
                    $('#InfoTop').fadeOut();
                    EnableMergeTableIfRequired($('#AdResDDL #ddlCover'));
                    //BindMergeTableEvents();
                }
                else if (quickTableUpdateModeEnabled) {
                    BindQuickTableUpdateEvents();
                }
                else {
                    QuickTableChangeService = null;
                    $('#InfoTop').fadeOut();
                    self.BindFloorTableEvents();
                }

                DesignStyledSelectList('#floorArea', true);
                self.SetRotateToFloorItems();
                ReservationListEvent();
                AssignLemonSlider('#bottomslider', false);

                QuickTableUpdateProcessing = false;
            }
        });

        //$('#floorArea').load(url, dataObj, function () {
        //    // 
        //    //if (MergeTableService == null) {
        //    //    self.BindFloorTableEvents();
        //    //}
        //    //else {
        //    //    BindMergeTableEvents();
        //    //}

        //    if (isAddEditResPanelOpen) {
        //        QuickTableChangeService = null;
        //        $('#InfoTop').fadeOut();
        //        EnableMergeTableIfRequired($('#AdResDDL #ddlCover'));
        //        //BindMergeTableEvents();
        //    }
        //    else if (quickTableUpdateModeEnabled) {
        //        BindQuickTableUpdateEvents();
        //    }
        //    else {
        //        QuickTableChangeService = null;
        //        $('#InfoTop').fadeOut();
        //        self.BindFloorTableEvents();
        //    }

        //    DesignStyledSelectList('#floorArea', true);
        //    self.SetRotateToFloorItems();
        //    ReservationListEvent();
        //    AssignLemonSlider('#bottomslider', false);

        //    QuickTableUpdateProcessing = false;
        //});
    };

    self.UpdateDataOnTimeChange = function (time) {
        self.UpdateFloorPlan(time, 'TIMESLIDE');
        self.UpdateReservationList(time, 'RESERVATION', '', false);
    };

    self.SetRotateToFloorItems = function () {
        $('.table').each(function (index, element) {
            var angle;
            if ($(this).parents('.c-container').length > 0) {
                angle = $(this).parents('.table-main').find('#Angle').val();
                $(this).parents('.table-main').find('.tblRotate').css({
                    '-moz-transform': 'rotate(' + (angle) + 'deg)',
                    '-webkit-transform': 'rotate(' + (angle) + 'deg)',
                    '-o-transform': 'rotate(' + (angle) + 'deg)',
                    '-ms-transform': 'rotate(' + (angle) + 'deg)'
                });

                self.SetRotateToTableSeats(this);
            }
            else {
                angle = $(this).find('#Angle').val();
                $(this).find('.tblRotate').css({
                    '-moz-transform': 'rotate(' + (angle) + 'deg)',
                    '-webkit-transform': 'rotate(' + (angle) + 'deg)',
                    '-o-transform': 'rotate(' + (angle) + 'deg)',
                    '-ms-transform': 'rotate(' + (angle) + 'deg)'
                });
            }
        });
    };

    self.SetRotateToTableSeats = function (table) {
        // 
        var seatCount = $(table).parents('.table-main').find('.seat').length;
        var angle = (360 / seatCount);
        var sumAngl = 0;

        for (var n = 0; n < seatCount; n++) {

            $(table).parents('.table-main').find(".seat" + n).css({
                '-moz-transform': 'rotate(' + (sumAngl) + 'deg)',
                '-webkit-transform': 'rotate(' + (sumAngl) + 'deg)',
                '-o-transform': 'rotate(' + (sumAngl) + 'deg)',
                '-ms-transform': 'rotate(' + (sumAngl) + 'deg)'
            });

            sumAngl = sumAngl + angle;
        }
    };

    self.BindFloorTableEvents = function () {
        $(".quan-2-other1,.quan-2-1,.quan-4-1").off('click').on('click', function (e) {

            e.stopPropagation();

            $(".quan-2-other1,.quan-2-1,.quan-4-1").removeClass("secHover");
            $(this).addClass("secHover");

            $('.table-main').css('z-index', '');

            var position = $(this).parents('.table-main').position();
            var tblWidth = $(this).parents('.table-main').width();

            if ((position.left + (tblWidth / 2)) < 512) {
                $(this).parents('.table-main').css('z-index', 9999).find('.show-tooltip-left').css('left', '100%').css('right', '');
            }
            else {
                $(this).parents('.table-main').css('z-index', 9999).find('.show-tooltip-left').css('left', '').css('right', '100%');
            }

            if (isAddEditResPanelOpen) {
                SelectTableInDDL(this);
            }
            else if (quickTableUpdateModeEnabled) {

            }
            else {
                $('.show-tooltip-left').not($(this).parents('.table-main').find('.show-tooltip-left')).removeClass('show-tooltip-left-toggle');
                $(this).parents('.table-main').find('.show-tooltip-left').toggleClass('show-tooltip-left-toggle');
            }
        });

        $(".quan-2-other1,.quan-2-1,.quan-4-1").off('hover').hover(function () {
            $(this).parents('.table-main').find('.tooltip_time-p').show();
        },
        function () {
            $(this).parents('.table-main').find('.tooltip_time-p').hide();
        });
    };
}

function SortReservation(sortby, source) {
    var resListUL = $(source).parents('.RTabs').find('ul.reslist');
    var resListLI = $(source).parents('.RTabs').find('ul.reslist li');

    console.info(resListLI.length);
    console.time("sorting resListLI");

    switch (sortby) {
        case "cover":
            {
                sortedList = null;

                if ($(source).data('order') == "asc") {
                    resListLI.sort(SortReservationsByCoversAsc).appendTo(resListUL);
                    $(source).data('order', 'desc');
                }
                else {
                    resListLI.sort(SortReservationsByCoversDesc).appendTo(resListUL);
                    $(source).data('order', 'asc');
                }

                break;
            }
        case "time":
            {
                sortedList = null;

                if ($(source).data('order') == "asc") {
                    resListLI.sort(SortReservationsByTimeAsc).appendTo(resListUL);
                    $(source).data('order', 'desc');
                }
                else {
                    resListLI.sort(SortReservationsByTimeDesc).appendTo(resListUL);
                    $(source).data('order', 'asc');
                }

                break;
            }
        default:
            break;
    }

    console.timeEnd("sorting resListLI");

    resListLI = null;
}

function SortReservationsByCoversAsc(a, b) {
    return (($(b).data('cover')) < ($(a).data('cover'))) ? 1 : -1;
}

function SortReservationsByTimeAsc(a, b) {
    return (($(b).data('time')) < ($(a).data('time'))) ? 1 : -1;
}

function SortReservationsByCoversDesc(a, b) {
    return (($(b).data('cover')) > ($(a).data('cover'))) ? 1 : -1;
}

function SortReservationsByTimeDesc(a, b) {
    return (($(b).data('time')) > ($(a).data('time'))) ? 1 : -1;
}

function UpadateReservationSuccess(data, resId) {
    if (data.Status == 'Success') {
        $('#resL' + resId).html(data.ListItem);
        //Floor.UpdateFloorPlan('', 'TIMESLIDE');
        //Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
        RefreshFloorTime();
        //DesignCustomDropdowns('.reslist');
        //alert('Reservation updated successfully...');
    }
    else {
        alert(data.Message);
    }
}

function DeleteReservationSuccess(data, resId) {
    if (data.Status == 'Success') {
        //alert('Reservation deleted successfully...');
        $('#resL' + resId).remove();
        Floor.UpdateFloorPlan('', 'TIMESLIDE');
        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
    }
    else {
        alert(data.Message);
    }
}

function EditReservationButtonEvent(resId) {
    window.location.href = '/FloorPlan/FloorPlan?ReservationId=' + resId;
}

function AddReservationTimeChange(ddlTime, considerResFloor) {
    // 
    var parentForm = $(ddlTime).parents('.resOptParent');

    if (considerResFloor) {
        if (parentForm.find('#FloorPlanId').val() > 0) {
            Floor.currentFloorId = parentForm.find('#FloorPlanId').val();
        }
        else { }
    }
    else { }

    var selectedTime = $(ddlTime).val();
    var splitTime = (selectedTime.split(' - '))[0].substr(8);
    var hrs = parseInt(splitTime.substr(0, 2));
    var min = parseInt(splitTime.substr(2, 2));
    var TT = splitTime.substr(4);
    var totalMin = ((((TT == 'AM' || hrs == 12) ? hrs : (hrs + 12)) * 60) + min);

    //    var selectedTime = $(ddlTime).find('option:selected').text();
    //    var splitTime = selectedTime.split(':');
    //    var hrs = parseInt(splitTime[0]);
    //    var splitMin = splitTime[1].split(' ');
    //    var min = parseInt(splitMin[0]);
    //    var totalMin = ((((splitMin[1] == 'AM') ? hrs : (hrs + 12)) * 60) + min);
    $('#slider').slider({ value: totalMin });
}

function SelectTableInDDL(sourceTable) {
    // 
    if ($(sourceTable).parents('.table-main').find('.show-tooltip-left').length === 0) {
        if ($('#floorSideAddRes').css('display') != 'none') {
            var adResTbl = $('#AdResDDL #ddlTable');

            //            if ($('#AdResAddiUpdateForm').length > 0) {
            //                var adResAddTbl = $('#AdResAddiUpdateForm #tableIdd');

            //                if (adResAddTbl.val() != $(sourceTable).parents('.table-main').find('#FloorTableId').val()) {
            //                    adResAddTbl.val($(sourceTable).parents('.table-main').find('#FloorTableId').val());
            //                    //                    adResAddTbl.val($(sourceTable).parents('.table-main').find('#FloorTableId').val()).change();
            //                    //                    $('#AdResAddiUpdateForm').submit();
            //                }
            //            }

            var ddlVal = $(sourceTable).parents('.table-main').find('#FloorTableId').val();

            if (adResTbl.val() != ddlVal && IsValueExistInDDl(adResTbl, ddlVal)) {
                //  
                adResTbl.val(ddlVal).change();
            }
        }
    }
}

function PromptForReservationEnding(data) {
    $('#popOverlay-bodyId').html(data.Container);
    $('#popOverlay-bodyId').find('#mrgtblList').html(data.Reslist);
    $('#popOverlay-bodyId .custom_check').screwDefaultButtons({
        //checked: 'url(/images/checkbox_Checked.png)',
        image: "url(/images/checkbox.png)",
        width: 24,
        height: 24
    });

    SetupPromptEndResPopup();

    StopSlideTimer();
    ShowPopUp();
}

function UpdateReservationTime() {
    $('#UpadteEndRes').submit();
}

function SetupPromptEndResPopup() {
    var popUp = $('#popOverlay');
    popUp.find('.popOverlay-foot .svbtn').off('click').on('click', UpdateReservationTime);
    popUp.find('.popOverlay-head .popOverlay-close-btn a, .popOverlay-foot .clsbtn').off('click').on('click', CloseResEndingPrompt);
    popUp.find('.popOverlay-head-left').text('Ending Reservations');
}

function CloseResEndingPrompt() {
    HidePopUp();

    if (!isAddEditResPanelOpen) {
        StartSlideTimer();
    }
}

function IsValueExistInDDl(ddl, val) {
    var exists = false;
    $(ddl).find('option').each(function () {
        if (this.value == val) {
            exists = true;
            return false;
        }
    });

    return exists;
}

function SaveReservation() {
    if (MergeTableService === null) {
        UpdateAddReservationFormValues();
        if ($('#AdResSaveForm').valid()) {
            PromptForPIN('#AdResSaveForm', function () { SaveReservation(); });
        }
        else {
            alert("Please fill the required fields before saving reservation.");
        }
    }
    else {
        //if (MergeTableService.SelectedTablesForMerging.length > 1) {
        if (MergeTableService.SelectedTablesForMerging.length > 0) {    // change #506
            if ($('#AdResSaveForm').valid()) {
                //var maxCovers = 0;
                //var selectedCovers = parseInt($('#AdResUpdateForm #Covers').val());

                //$.each(MergeTableService.SelectedTablesForMerging, function () {
                //    maxCovers += this.maxCovers;
                //});

                //if (maxCovers < selectedCovers) {
                //    AlertInvlidPIN("Max Covers should be more than " + selectedCovers + ".");
                //}
                //else {
                $('#MergeTableForm #MobileNumber').val($('#AdResSaveForm #MobileNumber').val());
                PromptForPIN('#MergeTableForm', function () { SaveReservation(); });
                //}
            }
            else {
                alert("Please fill the required fields before saving reservation.");
            }
        }
        else {
            alert("Please select atleast one table before saving reservation");
        }
    }
}

function DeleteReservation(isFirstTime) {
    if (isFirstTime) {
        alertify.confirm("Are you sure, you want to delete this reservation?", function () {
            PromptForPIN('#AdResDltForm', function () { DeleteReservation(false); });
        });
    }
    else {
        PromptForPIN('#AdResDltForm', function () { DeleteReservation(false); });
    }
}

function ScrollToCurrentTime() {
    var currentSlideTimeValue = $('#slider').slider("option", "value");
    var currentResItem = $("#ResAllList ul li").filter(function () {
        return $(this).attr("data-time") < currentSlideTimeValue;
    }).last();
    // 
    if (currentResItem.length > 0) {
        var top = currentResItem.offset().top - 138;
        var scrollTopToValue = $("#ResAllList").scrollTop() + top;
        $("#ResAllList").scrollTop(scrollTopToValue);
    }
}

function ExpandDetails(src) {
    $(src).toggleClass("addexp");
    $(src).parent().find(".addCont").toggleClass("show", 200);
};

function UpadateWaitingSuccess(data, waitId) {
    if (data.Status == 'Success') {
        alert(data.Message);
        HidePopovers();
        Floor.UpdateReservationList('', Floor.currentResTab, Floor.currentResFilterTab, true);
    }
    else {
        alert(data.Message);
    }
}