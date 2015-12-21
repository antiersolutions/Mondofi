var quickTableUpdateModeEnabled = false;
var QuickTableChangeService = null;
var QuickTableUpdateProcessing = false;

function QuickTableChange(resId, tableId, tableName) {
    // private fields
    var self = this;

    // public fileds
    self.currentTableId = tableId;
    self.currentTableName = tableName;
    self.curretReservationId = resId;
    self.AvailableTablesForUpdate = [];
    self.UpcomingTableIds = [];
    self.SmallTableIds = [];

    self.IsSmallTable = function (tableId) {
        var isSmall = false;

        isSmall = self.SmallTableIds.filter(function (smallTableId) {
            return smallTableId == tableId;
        }).length > 0;

        return isSmall;
    },
    self.IsUpComingTable = function (tableId) {
        var isUpcoming = false;

        isUpcoming = self.UpcomingTableIds.filter(function (upcomingTableId) {
            return upcomingTableId == tableId;
        }).length > 0;

        return isUpcoming;
    }
}


function EnableQuickTableUpdateMode(e, resId, totalMin) {
    e.stopPropagation();

    $.ajax({
        url: '/FloorPlan/GetAvailableTablesForUpdate',
        type: 'GET',
        data: { resId: resId },
        success: function (data) {
            // 
            if (data.availTables !== null) {
                QuickTableChangeService = new QuickTableChange(resId, data.currentTableId, data.currentTableName);
                QuickTableChangeService.AvailableTablesForUpdate = data.availTables;

                if (data.upcomingTableIds !== null) {
                    QuickTableChangeService.UpcomingTableIds = data.upcomingTableIds;
                }

                if (data.smallTableIds !== null) {
                    QuickTableChangeService.SmallTableIds = data.smallTableIds;
                }

                quickTableUpdateModeEnabled = true;
                hidePopUpOnClick = true;
                StopSlideTimer();
                $('#slider').slider({ value: totalMin });
            } else {
                alert("Sorry, No table is available for merging.");
                QuickTableChangeService = null;
            }
        },
        error: function () {
            alert("An error occured while enabling quick table change mode, please try later.");
            QuickTableChangeService = null;
        }
    });
}

function BindQuickTableUpdateEvents() {
    $('#InfoTop > span').html('Please click on a table to update reservation' +
        '<br>OR<br> click "' + QuickTableChangeService.currentTableName + '" again to quit edit mode.');
    $('#InfoTop').fadeIn();

    $(".quan-2-other1,.quan-2-1,.quan-4-1").off('click').on('click', function (e) {
        if (!QuickTableUpdateProcessing) {
            QuickTableUpdateProcessing = true;
            e.stopPropagation();
            var tableObj = $(this).parents('.table-main');
            var selectedTableId = tableObj.find('#FloorTableId').val();
            var top = tableObj.css('top');
            var left = tableObj.css('left');
            var sameTable = selectedTableId == QuickTableChangeService.currentTableId;
            var availHasTable = QuickTableChangeService.AvailableTablesForUpdate.filter(function (table) {
                return table.tableId == selectedTableId;
            }).length > 0;

            if (sameTable) {
                //AlertInvlidPIN("Same table ");
                $('#InfoTop > span').text('Closing Quick reservation table update mode.');
                RefreshFloorTime();
            }
            else if (availHasTable) {
                //AlertInvlidPIN("Update table ");
                $('#InfoTop > span').text('Updating reservation. Please wait...');
                UpdateReservationTable(selectedTableId, top, left);
            }
            else {
                if (QuickTableChangeService.IsUpComingTable(selectedTableId)) {
                    AlertInvlidPIN("The destination table has an upcoming booking. Please select another.");
                }
                else if (QuickTableChangeService.IsSmallTable(selectedTableId)) {
                    AlertInvlidPIN("The destination table is too small to move this group to. Please select another.");
                }
                else {
                    AlertInvlidPIN("Sorry, this table is not available for update. Please select another table.");
                }

                QuickTableUpdateProcessing = false;
            }
        }
    });

    quickTableUpdateModeEnabled = false;
}

function UpdateReservationTable(tableId, top, left) {

    var resId = QuickTableChangeService.curretReservationId;

    $.ajax({
        url: '/Reservation/QuickUpdateReservationTable',
        type: 'POST',
        data: { resId: resId, tableId: tableId, top: top, left: left },
        success: function (data) {
            // 
            if (data.Status == 'Success') {
                $('#InfoTop > span').text(data.Message);
                RefreshFloorTime();
            } else {
                alert(data.Message);
            }
        },
        error: function () {
            alert("An error occured while updating table, please try later.");
        }
    });
}

