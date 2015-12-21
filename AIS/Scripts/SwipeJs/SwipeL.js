
// TOUCH-EVENTS SINGLE-FINGER SWIPE-SENSING JAVASCRIPT
// Courtesy of PADILICIOUS.COM and MACOSXAUTOMATION.COM

// this script can be used with one or more page elements to perform actions based on them being swiped with a single finger

var triggerElementIDL = null; // this variable is used to identity the triggering element
var fingerCountL = 0;
var startXL = 0;
var startYL = 0;
var curXL = 0;
var curYL = 0;
var deltaXL = 0;
var deltaYL = 0;
var horzDiffL = 0;
var vertDiffL = 0;
var minLengthL = 72; // the shortest distance the user may swipe
var swipeLengthL = 0;
var swipeAngleL = null;
var swipeDirectionL = null;

// The 4 Touch Event Handlers

// NOTE: the touchStartL handler should also receive the ID of the triggering element
// make sure its ID is passed in the event call placed in the element declaration, like:
// <div id="picture-frame" ontouchStartL="touchStartL(event,'picture-frame');"  ontouchEndL="touchEndL(event);" ontouchMoveL="touchMoveL(event);" ontouchCancelL="touchCancelL(event);">

function touchStartL(event, passedName) {
    // disable the standard ability to select the touched object
    event.preventDefault();
    // get the total number of fingers touching the screen
    fingerCountL = event.touches.length;
    // since we're looking for a swipe (single finger) and not a gesture (multiple fingers),
    // check that only one finger was used
    if (fingerCountL == 1) {
        // get the coordinates of the touch
        startXL = event.touches[0].pageX;
        startYL = event.touches[0].pageY;
        // store the triggering element ID
        triggerElementIDL = passedName;
    } else {
        // more than one finger touched so cancel
        touchCancelL(event);
    }
}

function touchMoveL(event) {
    event.preventDefault();
    if (event.touches.length == 1) {
        curXL = event.touches[0].pageX;
        curYL = event.touches[0].pageY;
    } else {
        touchCancelL(event);
    }
}

function touchEndL(event) {
    event.preventDefault();
    // check to see if more than one finger was used and that there is an ending coordinate
    if (fingerCountL == 1 && curXL != 0) {
        // use the Distance Formula to determine the length of the swipe
        swipeLengthL = Math.round(Math.sqrt(Math.pow(curXL - startXL, 2) + Math.pow(curYL - startYL, 2)));
        // if the user swiped more than the minimum length, perform the appropriate action
        if (swipeLengthL >= minLengthL) {
            caluculateAngleL();
            determineswipeDirectionLL();
            processingRoutineL();
            touchCancelL(event); // reset the variables
        } else {
            touchCancelL(event);
        }
    } else {
        touchCancelL(event);
    }
}

function touchCancelL(event) {
    // reset the variables back to default values
    fingerCountL = 0;
    startXL = 0;
    startYL = 0;
    curXL = 0;
    curYL = 0;
    deltaXL = 0;
    deltaYL = 0;
    horzDiffL = 0;
    vertDiffL = 0;
    swipeLengthL = 0;
    swipeAngleL = null;
    swipeDirectionL = null;
    triggerElementIDL = null;
}

function caluculateAngleL() {
    var X = startXL - curXL;
    var Y = curYL - startYL;
    var Z = Math.round(Math.sqrt(Math.pow(X, 2) + Math.pow(Y, 2))); //the distance - rounded - in pixels
    var r = Math.atan2(Y, X); //angle in radians (Cartesian system)
    swipeAngleL = Math.round(r * 180 / Math.PI); //angle in degrees
    if (swipeAngleL < 0) { swipeAngleL = 360 - Math.abs(swipeAngleL); }
}

function determineswipeDirectionLL() {
    if ((swipeAngleL <= 45) && (swipeAngleL >= 0)) {
        swipeDirectionL = 'left';
    } else if ((swipeAngleL <= 360) && (swipeAngleL >= 315)) {
        swipeDirectionL = 'left';
    } else if ((swipeAngleL >= 135) && (swipeAngleL <= 225)) {
        swipeDirectionL = 'right';
    } else if ((swipeAngleL > 45) && (swipeAngleL < 135)) {
        swipeDirectionL = 'down';
    } else {
        swipeDirectionL = 'up';
    }
}

function processingRoutineL() {
    var swipedElement = document.getElementById(triggerElementIDL);
    if (swipeDirectionL == 'left') {
        $('.menu-bar').toggleClass('menu-bar-toggle');
        $('.m-left-btn').toggleClass('m-left-btn-toggle');

        SetMiddleWidthOnSideButtonClick();

    } else if (swipeDirectionL == 'right') {
        //$('.manage-section').toggleClass('manage-section-toggle');
        //$('.m-right-btn').toggleClass('m-right-btn-toggle');

        $('.menu-bar').toggleClass('menu-bar-toggle');
        $('.m-left-btn').toggleClass('m-left-btn-toggle');
        //

        SetMiddleWidthOnSideButtonClick();
    }
}

