$(document).ready(function () {
    ReservationListEvent();
});

function ReservationListEvent() {

    $('.stspopup').hover(function () {
        $(this).parents('.show-tooltip-left').find('.mCustomScrollBox').css('overflow', 'visible');
    }, function () {
        $(this).parents('.show-tooltip-left').find('.mCustomScrollBox').css('overflow', 'hidden');
    });

    $('.stspopup ul li').off().click(function (e) {
        $(this).parents('form').find('#StatusId').val($(this).find('#selectedResStatusId').val());
        $(this).parents('form').submit();
        e.stopPropagation();
    });

    $('.show-tooltip-left .row img.status-img').off().click(function (e) {
        e.stopPropagation();
    });
}

function ResetResStatus(source, status) {
    if (confirm("Are you sure?")) {
        var stsForm = $(source).parents('.row').find('form');
        stsForm.find('#StatusId').val(status);
        stsForm.submit();
    }
}

function UpadateReservationStatusImage(data, resId) {
    $('#resImg' + resId + ' img').attr('src', '/images/status-' + data.StatusName + '.png');
    $('#resL' + resId + ' img.statusImg').attr('src', '/images/' + data.StatusName + '.png');

    if ($.inArray(data.StatusId, [18, 19, 20]) > -1) {
        RefreshFloorTime();
    }
}

function CopyReservationFormValues(sourceFormId, targetFormId) {

    var $SourceForm = $('#' + sourceFormId);
    var $TargetForm = $('#' + targetFormId);

    $TargetForm.find('input[name = ReservationId]').val($SourceForm.find('input[name = ReservationId]').val());
    $TargetForm.find('input[name = Covers]').val($SourceForm.find('input[name = Covers]').val());
    $TargetForm.find('input[name = time]').val($SourceForm.find('input[name = time]').val());
    $TargetForm.find('input[name = ShiftId]').val($SourceForm.find('input[name = ShiftId]').val());
    $TargetForm.find('input[name = tableIdd]').val($SourceForm.find('input[name = tableIdd]').val());
    $TargetForm.find('input[name = resDate]').val($SourceForm.find('input[name = resDate]').val());
    $TargetForm.find('input[name = Status]').val($SourceForm.find('input[name = Status]').val());
    $TargetForm.find('input[name = Duration]').val($SourceForm.find('input[name = Duration]').val());
    $TargetForm.find('input[name = FloorPlanId]').val($SourceForm.find('input[name = FloorPlanId]').val());

    if ($(".table-main #FloorTableId[value='" + ($SourceForm.find('input[name = tableIdd]').val()) + "']").length > 0) {
        var floorTable = $(".table-main #FloorTableId[value='" + ($SourceForm.find('input[name = tableIdd]').val()) + "']");

        $TargetForm.find('input[name = TablePositionTop]').val(floorTable.parents('.table-main').css('top'));
        $TargetForm.find('input[name = TablePositionLeft]').val(floorTable.parents('.table-main').css('left'));
    }
}

function UpdateAddReservationFormValues() {
    //debugger;
    var $SourceForm = null;
    if ($('#floorSideAddRes').css('display') != 'none' && $('#AdResAddiUpdateForm').length > 0) {
        $SourceForm = $('#AdResAddiUpdateForm');
    }
    else {
        $SourceForm = $('#AdResUpdateForm');
    }

    var $TargetForm = $('#AdResSaveForm');

    $TargetForm.find('input[name = Covers]').val($SourceForm.find('input[name = Covers]').val());
    $TargetForm.find('input[name = time]').val($SourceForm.find('input[name = time]').val());
    $TargetForm.find('input[name = ShiftId]').val($SourceForm.find('input[name = ShiftId]').val());
    $TargetForm.find('input[name = tableIdd]').val($SourceForm.find('input[name = tableIdd]').val());

    if ($SourceForm.find('input[name = resDate]').val() !== null) {
        $TargetForm.find('input[name = resDate]').val($SourceForm.find('input[name = resDate]').val());
    }

    $TargetForm.find('input[name = Status]').val($SourceForm.find('input[name = Status]').val());
    $TargetForm.find('input[name = Duration]').val($SourceForm.find('input[name = Duration]').val());
    //$TargetForm.find('input[name = FloorPlanId]').val($SourceForm.find('input[name = FloorPlanId]').val());
    $TargetForm.find('input[name = FloorPlanId]').val(Floor.currentFloorId);

    if ($(".table-main #FloorTableId[value='" + ($SourceForm.find('input[name = tableIdd]').val()) + "']").length > 0) {
        var floorTable = $(".table-main #FloorTableId[value='" + ($SourceForm.find('input[name = tableIdd]').val()) + "']");

        $TargetForm.find('input[name = TablePositionTop]').val(floorTable.parents('.table-main').css('top'));
        $TargetForm.find('input[name = TablePositionLeft]').val(floorTable.parents('.table-main').css('left'));
    }
    else if ($(".table-main #MergedFloorTableId[value='" + ($TargetForm.find('input[name = MergeTableId]').val()) + "']").length > 0) {
        var floorTable = $(".table-main #MergedFloorTableId[value='" + ($TargetForm.find('input[name = MergeTableId]').val()) + "']");

        $TargetForm.find('input[name = TablePositionTop]').val(floorTable.parents('.table-main').css('top'));
        $TargetForm.find('input[name = TablePositionLeft]').val(floorTable.parents('.table-main').css('left'));
    }

    $TargetForm.find('input[name = enableMerging]').val($SourceForm.find('input[name = enableMerging]').val());
}
/*Reservation notes update functions*/

function EditReservationNote(parent) {
    $(parent).find('#resNote').hide();
    $(parent).find('#resNoteEdt').show();
    $(parent).find('#resNoteEdt textarea').val($(parent).find('#resNote pre').text()).focus();

    if ($('.popover-content ' + parent).length > 0) {
        $('.popover-content ' + parent).find('#resNote').hide();
        $('.popover-content ' + parent).find('#resNoteEdt').show();
        $('.popover-content ' + parent).find('#resNoteEdt textarea').val($('.popover-content ' + parent).find('#resNote pre').text()).show().focus();
    }
}

function SaveReservationNoteChanges(parent, target) {

    var gstNote = "";

    if ($('.popover-content ' + parent).length > 0) {
        gstNote = $('.popover-content ' + parent).find('#resNoteEdt textarea').val();
        $('.popover-content ' + parent).find('#resNote pre').text(gstNote);
        $('.popover-content ' + target).find('#ReservationNote').val(gstNote);
        //$('.cluetip-outer ' + parent).find('#resNoteEdt').hide();
        //$('.cluetip-outer ' + parent).find('#resNote').show();
    }

    if (gstNote.length === 0) {
        gstNote = $(parent).find('#resNoteEdt textarea').val();
    }

    $(parent).find('#resNote pre').text(gstNote);
    $(target).find('#ReservationNote').val(gstNote);
    //$(parent).find('#resNoteEdt').hide();
    //$(parent).find('#resNote').show();

}

function CancelReservationNoteChanges(parent) {
    $(parent).find('#resNoteEdt').hide();
    $(parent).find('#resNote').show();
    if ($('.popover-content ' + parent).length > 0) {
        $('.popover-content ' + parent).find('#resNoteEdt').hide();
        $('.popover-content ' + parent).find('#resNote').show();
    }
}

/***********/
/*Guest notes update functions*/

function EditGuestNote(parent) {
    // debugger;
    $(parent).find('#guestNote').hide();
    $(parent).find('#guestNoteEdt').show();
    $(parent).find('#guestNoteEdt textarea').val($(parent).find('#guestNote pre').text()).focus();

    if ($('.popover-content ' + parent).length > 0) {
        $('.popover-content ' + parent).find('#guestNote').hide();
        $('.popover-content ' + parent).find('#guestNoteEdt').show();
        $('.popover-content ' + parent).find('#guestNoteEdt textarea').val($('.popover-content ' + parent).find('#guestNote pre').text()).focus();
    }
}

function SaveGuestNoteChanges(parent, target) {

    var gstNote = "";

    if ($('.popover-content ' + parent).length > 0) {
        gstNote = $('.popover-content ' + parent).find('#guestNoteEdt textarea').val();
        $('.popover-content ' + parent).find('#guestNote pre').text(gstNote);
        $('.popover-content ' + target).find('#GuestNote').val(gstNote);
        //$('.cluetip-outer ' + parent).find('#guestNoteEdt').hide();
        //$('.cluetip-outer ' + parent).find('#guestNote').show();
    }

    if (gstNote.length === 0) {
        gstNote = $(parent).find('#guestNoteEdt textarea').val();
    }

    $(parent).find('#guestNote pre').text(gstNote);
    $(target).find('#GuestNote').val(gstNote);
    //$(parent).find('#guestNoteEdt').hide();
    //$(parent).find('#guestNote').show();

}

function CancelGuestNoteChanges(parent) {
    $(parent).find('#guestNoteEdt').hide();
    $(parent).find('#guestNote').show();
    if ($('.popover-content ' + parent).length > 0) {
        $('.popover-content ' + parent).find('#guestNoteEdt').hide();
        $('.popover-content ' + parent).find('#guestNote').show();
    }
}
/***********/

/*Reservation notes update functions*/

function EditWaitingNote(parent) {
    $(parent).find('#waitNote').hide();
    $(parent).find('#waitNoteEdt').show();
    $(parent).find('#waitNoteEdt textarea').val($(parent).find('#waitNote pre').text()).focus();

    if ($('.popover-content ' + parent).length > 0) {
        $('.popover-content ' + parent).find('#waitNote').hide();
        $('.popover-content ' + parent).find('#waitNoteEdt').show();
        $('.popover-content ' + parent).find('#waitNoteEdt textarea').val($('.popover-content ' + parent).find('#waitNote pre').text()).show().focus();
    }
}

function SaveWaitingNoteChanges(parent, target) {

    var waitNote = "";

    if ($('.popover-content ' + parent).length > 0) {
        waitNote = $('.popover-content ' + parent).find('#waitNoteEdt textarea').val();
        $('.popover-content ' + parent).find('#waitNote pre').text(waitNote);
        $('.popover-content ' + target).find('#Notes').val(waitNote);
    }

    if (waitNote.length === 0) {
        waitNote = $(parent).find('#resNoteEdt textarea').val();
    }

    $(parent).find('#waitNote pre').text(waitNote);
    $(target).find('#Notes').val(waitNote);
}

function CancelWaitingNoteChanges(parent) {
    $(parent).find('#waitNoteEdt').hide();
    $(parent).find('#waitNote').show();
    if ($('.popover-content ' + parent).length > 0) {
        $('.popover-content ' + parent).find('#waitNoteEdt').hide();
        $('.popover-content ' + parent).find('#waitNote').show();
    }
}

/***********/

/***** Phone Number Events*****/

function inputPNFocus(i) {
    if (i.value == i.defaultValue) { i.value = ""; }
}

function inputPNBlur(i) {
    if (i.value === "") { i.value = i.defaultValue; }
}

/***********/

/******* User PIN ******/

function PromptForPIN(formId, wrongPINCallback) {
    var isWalkin = $(formId + ' #MobileNumber').val() == '9999999999';

    if (!isWalkin && pinEnabled) {
        var pin = null;
        alertify.prompt('CONFIRM PIN', 'Please enter your PIN to continue', '', null, null)
            .set('onok', function (evt, value) {
                var isValid = false;
                pin = value;
                isValid = ($.isNumeric(pin) && pin.length == 4);

                if (!isValid) {
                    AlertInvlidPIN('Please enter a valid user PIN.', wrongPINCallback);
                }
                else {
                    $(formId + ' #PIN').val(pin);
                    $(formId).submit();
                }
            })
            .set({ onshow: function () { $('.ajs-input').val(''); } })
            .set('labels', { ok: 'Submit' })
            .set('resizable', true)
            .resizeTo(235, 195);
    }
    else {
        $(formId).submit();
    }
}

function AlertInvlidPIN(message, wrongPINCallback) {
    alertify.alert('ERROR', message)
    .set('onok', function () {
        if (message.indexOf('PIN') > -1) {
            if (wrongPINCallback && typeof (wrongPINCallback) === "function") {
                wrongPINCallback();
            }
        }
    })
    .set('resizable', true)
    .resizeTo(235, 195);
}

/***********/