var counter = 0, oneGero = 0, tableno = 1, totalMaxCover = 0, totalMinCover = 0, lastTableId = 0;
var isPendingChanges = false;

(function ($) {
    $.fn.outerHTML = function () {
        return $(this).clone().wrap('<div></div>').parent().html();
    }
})(jQuery);

$(document).ready(function () {
    $("#floor").droppable();

    $("#floor .table-main").live('mouseover', function () {
        $(this).draggable({
            containment: '#floor',
            stop: function (ev, ui) {
                $(ui.helper).find('#TLeft').val($(ui.helper).css('left'));
                $(ui.helper).find('#TTop').val($(ui.helper).css('top'));

                $(ui.helper).find('form').submit();

                //UpdateItem(ui.helper);
            }
        });
    });



    //************ Rotate Clockwise  *************//

    $(".rotateClock").live('click', function (e) {
        var isTemp = false;

        if (e.originalEvent === undefined) {
            isTemp = true;
        }

        RotateClock(this, isTemp);
    });



    //************ Rotate AntiClockwise  *************//
    //var degAnti = 0;
    $(".rotateAnti").live('click', function (e) {
        var isTemp = false;

        if (e.originalEvent === undefined) {
            isTemp = true;
        }

        RotateAnti(this, isTemp);
    });



    //************* for Row, Seat edit option  *****************//



    $("#floor .seat,.quan-2-other1,.quan-2-1,.quan-4-1").live('click', function (e) {
        e.stopPropagation();
        $(".rotate").hide();
        $(".seat,.quan-2-other1,.quan-2-1,.quan-4-1,.item img").removeClass("secHover");
        $(this).addClass("secHover");
        $(this).parents(".table-main").find(".rotate").show();
        //$("#form_info").html('<div id="sec_option" class ="edit_option"> <ul><li><a id="node_delete" href="javascript:void(0);" title="' + this.id + '" >Delete Seat</a></li></ul></div>');

    });

    $("#floor").live('click', function () {

        HideEditPanel();

        $(".rotate").hide();
        $('.tblAddPanel').click();

    });

    // hide edit panel on click any floor element other than tables
    $("#floor .item img").live("click", function (e) {
        e.stopPropagation();

        HideEditPanel();

        $(".rotate").hide();
        $(this).addClass("secHover");
        $(this).parents(".table-main").find(".rotate").show();
        $('.tblAddPanel').click();
    });
});

//function ClearEditPanel() {
//    $('#TName').val('');
//    $('#hdnTShape').val('');
//    $('.TShape').removeClass('active');
//    $('#TAngle').val('');
//}

function HideEditPanel() {
    if (isPendingChanges) {
        if (confirm('You have pending changes for table: "' + $('#tblEditForm #TableName').val() + '. Do you want to save changes?"')) {
            $('#tblEditForm #SaveChanges').val(true);
            $('#tblEditForm').submit();
            $(".seat,.quan-2-other1,.quan-2-1,.quan-4-1,.item img").removeClass("secHover");
            ClearEditPanel();
        }
        else {
            $('#tblEditForm #CancelChanges').val(true);
            $('#tblEditForm').submit();
            $(".seat,.quan-2-other1,.quan-2-1,.quan-4-1,.item img").removeClass("secHover");
            ClearEditPanel();
        }
    }
    else {
        $(".seat,.quan-2-other1,.quan-2-1,.quan-4-1,.item img").removeClass("secHover");
        ClearEditPanel();
    }
}

function ClearEditPanel() {
    isPendingChanges = false;
    $('.TShape').removeClass('active');

    var $EditForm = $('#tblEditForm');

    $EditForm.find("#FloorTableId").val('');
    $EditForm.find("#FloorPlanId").val('');
    $EditForm.find("#SectionId").val(''); 
    $EditForm.find("#TableName").val('');
    $EditForm.find("#HtmlId").val('');
    $EditForm.find("#Shape").val('');
    $EditForm.find("#Size").val('');
    $EditForm.find("#MinCover").val('');
    $EditForm.find("#MaxCover").val('');
    $EditForm.find("#Angle").val('');
    $EditForm.find("#TTop").val('');
    $EditForm.find("#TLeft").val('');
    $EditForm.find("#IsTemporary").val('');

}

// Table Methods

function RotateAnti(item, isTemp) {
    var deg = parseInt($(item).parents('.table-main').find('#Angle').val());
    var tableId = $(item).attr("title");
    if (deg == -360) { deg = 0; }
    deg -= 45;
    $("#" + tableId).find('.tblRotate').css('');
    $("#" + tableId).find('.tblRotate').css({
        '-moz-transform': 'rotate(' + (deg) + 'deg)',
        '-webkit-transform': 'rotate(' + (deg) + 'deg)',
        '-o-transform': 'rotate(' + (deg) + 'deg)',
        '-ms-transform': 'rotate(' + (deg) + 'deg)',
        'transform': 'rotate(' + (deg) + 'deg)'
    });

    //$(item).parents('.table-main').find('#Angle').val(deg);
    $(item).parents('.table-main').find('#Angle').val(deg);

    if (isTemp) {
        $('#tblEditForm #Angle').val(deg);
    }
    else {
        $(item).parents('.table-main').find('form').submit();
    }
}

function RotateClock(item, isTemp) {
    //var deg = 0;
    var deg = parseInt($(item).parents('.table-main').find('#Angle').val());
    var tableId = $(item).attr("title");

    if (deg == 360) { deg = 0; }
    deg += 45;
    $("#" + tableId).find('.tblRotate').css('');
    $("#" + tableId).find('.tblRotate').css({
        '-moz-transform': 'rotate(' + (deg) + 'deg)',
        '-webkit-transform': 'rotate(' + (deg) + 'deg)',
        '-o-transform': 'rotate(' + (deg) + 'deg)',
        '-ms-transform': 'rotate(' + (deg) + 'deg)',
        'transform': 'rotate(' + (deg) + 'deg)'
    });

    //$(item).parents('.table-main').find('#Angle').val(deg);
    $(item).parents('.table-main').find('#Angle').val(deg);

    if (isTemp) {
        $('#tblEditForm #Angle').val(deg);
    }
    else {
        $(item).parents('.table-main').find('form').submit();
    }
}

function SetRotateToFloorItems() {
    $('.table').each(function (index, element) {

        if ($(this).parents('.c-container').length > 0) {
            var angle = $(this).parents('.table-main').find('#Angle').val();
            $(this).parents('.table-main').find('.tblRotate').css({
                '-moz-transform': 'rotate(' + (angle) + 'deg)',
                '-webkit-transform': 'rotate(' + (angle) + 'deg)',
                '-o-transform': 'rotate(' + (angle) + 'deg)',
                '-ms-transform': 'rotate(' + (angle) + 'deg)'
            });

            SetRotateToTableSeats(this);
        }
        else {
            var angle = $(this).find('#Angle').val();
            $(this).find('.tblRotate').css({
                '-moz-transform': 'rotate(' + (angle) + 'deg)',
                '-webkit-transform': 'rotate(' + (angle) + 'deg)',
                '-o-transform': 'rotate(' + (angle) + 'deg)',
                '-ms-transform': 'rotate(' + (angle) + 'deg)'
            });
        }
    });
}

function SetRotateToTableSeats(table) {
    //debugger;
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
}

function SetRotateToParticularTable(TableUniqueId) {
    if ($(TableUniqueId).find('.c-container').length > 0) {
        var angle = $(TableUniqueId).find('#Angle').val();
        $(TableUniqueId).find('.tblRotate').css({
            '-moz-transform': 'rotate(' + (angle) + 'deg)',
            '-webkit-transform': 'rotate(' + (angle) + 'deg)',
            '-o-transform': 'rotate(' + (angle) + 'deg)',
            '-ms-transform': 'rotate(' + (angle) + 'deg)'
        });

        SetRotateToTableSeats($(TableUniqueId).find('.table'));
    }
    else {
        var angle = $(TableUniqueId).find('#Angle').val();
        $(TableUniqueId).find('.tblRotate').css({
            '-moz-transform': 'rotate(' + (angle) + 'deg)',
            '-webkit-transform': 'rotate(' + (angle) + 'deg)',
            '-o-transform': 'rotate(' + (angle) + 'deg)',
            '-ms-transform': 'rotate(' + (angle) + 'deg)'
        });
    }
}

function AddNewItemToFloor(minCover, maxCover, size, shape, isTemp) {
    //AddElement(minCover, maxCover, size, shape);
    if (isTemp) {
        url = '/FloorItem/AddTempItem';
    }
    else {
        url = '/FloorItem/AddFloorItem';
    }


    var TableObj = {
        FloorPlanId: $('#FloorPlanId').val(),
        Shape: shape,
        MinCover: minCover,
        MaxCover: maxCover,
        Size: size
    };

    $.ajax({
        type: "POST",
        async: false,
        url: url,
        data: JSON.stringify(TableObj),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            //console.log(result);
            if (result.Status == "Success") {
                lastTableId = result.ItemId;
                tableno = result.totalTables;
                $('#floor').append(result.Template);
                SetRotateToParticularTable('#' + result.HtmlId);
                UpdateItemCounts(result.totalTables, result.totalMinCovers, result.totalMaxCovers);
            }
            else {
                alert("Error while creating floor item, please try later...");
            }
        }
    });

}

function EditTable(item) {
    if (!($(item).parents('.table-main').attr('id') == $('#tblEditForm #HtmlId').val())) {
        if (isPendingChanges) {
            if (confirm('You have pending changes for table: "' + $('#tblEditForm #TableName').val() + '. Do you want to save changes?"')) {
                $('#tblEditForm #SaveChanges').val(true);
                $('#tblEditForm').submit();
                ClearEditPanel();
                UpdateEditTableFormValues(item);
            }
            else {
                $('#tblEditForm #CancelChanges').val(true);
                $('#tblEditForm').submit();
                ClearEditPanel();
                UpdateEditTableFormValues(item);
            }
        }
        else {
            ClearEditPanel();
            UpdateEditTableFormValues(item);
        }
    }
}

function UpdateEditTableFormValues(item) {

    var $EditForm = $('#tblEditForm');
    var $Table = $(item).parents('.table-main');

    $EditForm.find("#FloorTableId").val($Table.find("#FloorTableId").val());
    $EditForm.find("#FloorPlanId").val($Table.find("#FloorPlanId").val());
    $EditForm.find("#SectionId").val($Table.find("#SectionId").val());
    $EditForm.find("#TableName").val($Table.find("#TableName").val());
    $EditForm.find("#HtmlId").val($Table.find("#HtmlId").val());
    $EditForm.find("#Shape").val($Table.find("#Shape").val());
    $EditForm.find("#Size").val($Table.find("#Size").val());
    $EditForm.find("#MinCover").val($Table.find("#MinCover").val());
    $EditForm.find("#MaxCover").val($Table.find("#MaxCover").val());
    $EditForm.find("#Angle").val($Table.find("#Angle").val());
    $EditForm.find("#TTop").val($Table.find("#TTop").val());
    $EditForm.find("#TLeft").val($Table.find("#TLeft").val());
    $EditForm.find("#IsTemporary").val($Table.find("#IsTemporary").val());
    $EditForm.find("#SaveChanges").val(false);
    $EditForm.find("#CancelChanges").val(false);

    $('#TName').val($Table.find("#TableName").val());
    $('#OriginalTName').val($Table.find("#TableName").val());
    $('#TTempEdt').val($Table.find("#IsTemporary").val());
    $('.TShape#T' + $Table.find("#Shape").val()).addClass('active');
    $('#TSizeEdt').val($Table.find("#Size").val());
    $('#TMinCover').val($Table.find("#MinCover").val());
    $('#TMaxCover').val($Table.find("#MaxCover").val());

    $('#dltTable').off('click').click(function () {
        DeleteTable($Table.find("#FloorTableId").val(), item);
    });

    $('#rotAnti').off('click').click(function () {
        $Table.find('.rotateAnti').click();
    });

    $('#rotClock').off('click').click(function () {
        $Table.find('.rotateClock').click();
    });

    $('.tblEditPanel').click();

}

function UpdateItemCounts(totalTables, totalMinCovers, totalMaxCovers) {
    $("#totalTable").text(totalTables);
    $("#tblMaxCvr").text(totalMaxCovers);
    $("#tblMinCvr").text(totalMinCovers);
}