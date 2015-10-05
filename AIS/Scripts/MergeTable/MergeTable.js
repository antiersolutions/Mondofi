// global variable for merge table service
var MergeTableService = null;
var maxCoverLimit = 6;

function MergedTable() {
    // private fields
    var self = this;

    // public fileds
    self.SelectedTablesForMerging = [];
    self.AvailableTablesForMerging = [];

    // public Members
    self.AddTableToMergeList = function (tableId) {
        self.SelectedTablesForMerging.push((self.AvailableTablesForMerging.filter(function (table) {
            return table.tableId == tableId;
        }))[0]);

        self.RemoveTableFromAvailList(tableId);
    };

    self.AddTableToAvailList = function (tableId) {
        self.AvailableTablesForMerging.push((self.SelectedTablesForMerging.filter(function (table) {
            return table.tableId == tableId;
        }))[0]);

        self.RemoveTableFromMergeList(tableId);
    };

    self.RemoveTableFromMergeList = function (tableId) {
        self.SelectedTablesForMerging = self.SelectedTablesForMerging.filter(function (table) {
            return table.tableId != tableId;
        });

        // self.AddTableToAvailList(tableId);
    };

    self.RemoveTableFromAvailList = function (tableId) {
        self.AvailableTablesForMerging = self.AvailableTablesForMerging.filter(function (table) {
            return table.tableId != tableId;
        });

        //  self.AddTableToMergeList(tableId);
    };
}

$(function () {

});

function EnableMergeTableIfRequired(ddlCovers, floorId, time) {
    var covers = parseInt($(ddlCovers).val());
    if (covers > maxCoverLimit) {
        // debugger;
        EnableMergeTableOptions(ddlCovers, floorId, time);
        BindMergeTableEvents();
    }
    else {
        MergeTableService = null;
        Floor.BindFloorTableEvents();
    }
}

function EnableMergeTableOptions(ddlCovers, floorId, time) {
    var ReservationForm = $(ddlCovers).parents('.resOptParent');
    //debugger;
    var dataObj = {
        resDate: Floor.currentDate,
        time: ReservationForm.find('#time').val(),
        Covers: $(ddlCovers).val(),
        Duration: ReservationForm.find('#Duration').val(),
        ReservationId: ReservationForm.find('#ReservationId').val(),
        FloorPlanId: Floor.currentFloorId,
        enableMerging: ReservationForm.find('#enableMerging').val()
    };

    if (floorId !== undefined && floorId !== null && floorId.length > 0) {
        dataObj.FloorPlanId = floorId;
    }

    if (time !== undefined && time !== null && time.length > 0) {
        dataObj.time = time;
    }

    $.ajax({
        url: '/MergeTable/GetJSONTablesFreeToMerge',
        type: 'POST',
        data: dataObj,
        success: function (data) {
            //debugger;
            if (data.availTables !== null) {
                MergeTableService = new MergedTable();
                MergeTableService.AvailableTablesForMerging = data.availTables;
            } else {
                alert("Sorry, No table is available for merging.");
                MergeTableService = null;
            }

            if (data.mergedTables !== null) {
                MergeTableService.SelectedTablesForMerging = data.mergedTables;
            }
            else { }
        }
    });

    //$('#popOverlay .popOverlay-body').load("/MergeTable/MergeTablePartial", dataObj, function () {
    //    $('.custom_check').screwDefaultButtons({
    //        checked: "url(/images/checkbox_Checked.png)",
    //        unchecked: "url(/images/checkbox_Unchecked.png)",
    //        width: 26,
    //        height: 28
    //    });

    //    $('#mrgtblList input[type=checkbox]').change(function () {
    //        $(this).parents('form').submit();
    //    });

    //    UpdateResForms(ddlCovers, floorId, time);
    //    SetupMergeTablePopup();
    //});

    //ShowPopUp();
}

function BindMergeTableEvents() {
    $(".quan-2-other1,.quan-2-1,.quan-4-1").each(function () {
        if ($(this).hasClass('isMerged'))
            $(this).css('background-color', '#e8867c');
    });

    $(".quan-2-other1,.quan-2-1,.quan-4-1").off('click').on('click', function (e) {
        e.stopPropagation();

        var minCovers = 0;
        var maxCovers = 0;

        var selectedTableId = $(this).parents('.table-main').find('#FloorTableId').val();

        var mergedHasTable = MergeTableService.SelectedTablesForMerging.filter(function (table) {
            return table.tableId == selectedTableId;
        }).length > 0;

        var availHasTable = MergeTableService.AvailableTablesForMerging.filter(function (table) {
            return table.tableId == selectedTableId;
        }).length > 0;

        if (!($(this).hasClass('isMerged')) && !availHasTable) {
            //alert("Sorry, this table is not available for merging.");
            AlertInvlidPIN("Sorry, this table is not available for merging.");
        } else { }

        if (availHasTable) {

            //var selectedCovers = parseInt($('#AdResUpdateForm #Covers').val());

            //var availTable = MergeTableService.AvailableTablesForMerging.filter(function (table) {
            //    return table.tableId == selectedTableId;
            //})[0];

            //$.each(MergeTableService.SelectedTablesForMerging, function () {
            //    minCovers += this.minCovers;
            //    maxCovers += this.maxCovers;
            //});

            //if ((minCovers + availTable.minCovers) > selectedCovers) {
            //alert("Sorry, that table is too large to merge for a reservation of this size.");
            //AlertInvlidPIN("Sorry, that table is too large to merge for a reservation of this size.");
            //}
            //else {
            MergeTableService.AddTableToMergeList(selectedTableId);
            $(this).effect("transfer", { to: $("#tblSelectOption p") }, 500, function () { $(this).addClass('isMerged'); });
            //}
        }
        else if (mergedHasTable) {
            MergeTableService.AddTableToAvailList(selectedTableId);
            var tblDiv = $(this);
            $("#tblSelectOption p").effect("transfer", { to: $(this) }, 500, function () { $(tblDiv).removeClass('isMerged').css('background-color', ''); });
        }

        var outerSpan = '<span>';
        var tableNames = [];
        minCovers = 0;
        maxCovers = 0;

        $('#MergeTableForm #selectTablesDiv').empty();

        $.each(MergeTableService.SelectedTablesForMerging, function () {
            tableNames.push(this.tableName);
            $('#MergeTableForm #selectTablesDiv').append('<input id="selectedTables" type="hidden" value="' + this.tableId + '" name="selectedTables">');
            minCovers += this.minCovers;
            maxCovers += this.maxCovers;
        });

        if (tableNames.length === 0) {
            tableNames.push('Click on available tables on floor to merge.');
            outerSpan += tableNames + '</span>';
        }
        else {
            var tables = tableNames.join(' + ');
            outerSpan += tables; // + '</br>Total MinCovers = ' + minCovers + '</br>Total MaxCovers = ' + maxCovers + '</span>';
        }

        $("#tblSelectOption p").empty().html(outerSpan);
    });
}

//#region Old Merge Tbale Code 

//$(function () {

//    BindPopUpDraggable();

//});

//function OpenMergeTablePopUpIfRequired(ddlCovers, floorId, time) {
//    if ($(ddlCovers).val() > 6) {
//        OpenMergeTablePopUp(ddlCovers, floorId, time);
//    }
//}

//function OpenMergeTablePopUp(ddlCovers, floorId, time) {
//    var ReservationForm = $(ddlCovers).parents('.resOptParent');
//    //debugger;
//    var dataObj = {
//        resDate: Floor.currentDate,
//        time: ReservationForm.find('#time').val(),
//        Covers: $(ddlCovers).val(),
//        Duration: ReservationForm.find('#Duration').val(),
//        ReservationId: ReservationForm.find('#ReservationId').val(),
//        FloorPlanId: Floor.currentFloorId
//    };

//    if (floorId != undefined && floorId != null && floorId.length > 0) {
//        dataObj.FloorPlanId = floorId;
//    }

//    if (time != undefined && time != null && time.length > 0) {
//        dataObj.time = time;
//    }

//    $('#popOverlay .popOverlay-body').load("/MergeTable/MergeTablePartial", dataObj, function () {
//        $('.custom_check').screwDefaultButtons({
//            checked: "url(/images/checkbox_Checked.png)",
//            unchecked: "url(/images/checkbox_Unchecked.png)",
//            width: 26,
//            height: 28
//        });

//        $('#mrgtblList input[type=checkbox]').change(function () {
//            $(this).parents('form').submit();
//        });

//        UpdateResForms(ddlCovers, floorId, time);
//        SetupMergeTablePopup();
//    });

//    ShowPopUp();
//}

//function AddTableToFloor() {
//    if ($('#mrgtblPreview form').length == 0) {
//        alert("Please create a table before saving...");
//    }
//    else {
//        $('#mrgtblPreview form').submit();
//        HidePopUp();
//    }
//}

//function UpdateResForms(ddlCovers, floorId, time) {
//    if ($('#AdResAddiUpdateForm').length > 0) {
//        $('#AdResAddiUpdateForm #Covers').val($(ddlCovers).val());
//        $('#AdResAddiUpdateForm #time').val(time);
//        $('#AdResAddiUpdateForm #FloorPlanId').val(floorId);
//        $('#AdResAddiUpdateForm #isMerging').val(true);
//        $('#AdResAddiUpdateForm').submit();
//    }

//    $('#AdResUpdateForm #Covers').val($(ddlCovers).val());
//    $('#AdResUpdateForm #time').val(time);
//    $('#AdResUpdateForm #FloorPlanId').val(floorId);
//    $('#AdResUpdateForm #considerFloor').val(true);
//    $('#AdResUpdateForm #isMerging').val(true);
//    $('#AdResUpdateForm').submit();
//}

//function SetupMergeTablePopup() {
//    var popUp = $('#popOverlay');
//    popUp.find('.popOverlay-foot .pop-sec-btn-save').off('click').on('click', AddTableToFloor);
//    popUp.find('.popOverlay-head .popOverlay-close-btn a').off('click').on('click', HidePopUp);
//    popUp.find('.popOverlay-head-left').text('Merge Tables');
//}

///**** PopUp draggable code ****/

//function BindPopUpDraggable() {
//    var d = $('.dragme');

//    d.draggable({
//        containment: 'body',
//        start: function (event, ui) {
//            $(ui.helper).css('cursor', 'move');
//        },
//        stop: function (ev, ui) {
//            $(ui.helper).css('cursor', '');
//        }
//    });
//}

/**** End here ****/
//#endregion
