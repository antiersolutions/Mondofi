/// <reference path="../../PNotify/ProgressNotifier.js" />

var UpdateEndingReservationProgress = null;

$(function () {
    // SignalR
    wireUpSignalR();
});

// SignalR
function wireUpSignalR() {
    // Connect to hub.
    var floorPlanHub = $.connection.FloorPlanHub;

    //start progress
    floorPlanHub.client.startProgress = function () {
        UpdateEndingReservationProgress = new ProgressNotifier();
        UpdateEndingReservationProgress.Start();
    }

    //update progress
    floorPlanHub.client.updateProgress = function (percent) {
        if (UpdateEndingReservationProgress)
            UpdateEndingReservationProgress.Update(percent);
    }

    // Update display when updated.
    floorPlanHub.client.updateEndingReservation = function (message, type) {
        alert(message, {
            type: type,
            hide: type == 'success' ? true : false,
            nonblock: {
                nonblock: false
            },
            buttons: {
                closer: true,
                sticker: true
            },
            delay: 5000
        });
    };

    $.connection.hub.start().done(function () {

    });
}