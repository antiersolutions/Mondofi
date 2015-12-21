var counter = 0, oneGero = 0, tableno = 1, totalMaxCover = 0, totalMinCover = 0;

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
                UpdateItem(ui.helper);
            }
        });
    });



    //************ Rotate Clockwise  *************//

    $(".rotateClock").live('click', function () {
        RotateClock(this);
    });



    //************ Rotate AntiClockwise  *************//
    //var degAnti = 0;
    $(".rotateAnti").live('click', function () {
        RotateAnti(this);
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
        $(".seat,.quan-2-other1,.quan-2-1,.quan-4-1,.item img").removeClass("secHover");
        $(".rotate").hide();
        $('.table-tabs .nav-m ul.main li:first-child a').click();
        ClearEditPanel();
    });

    // hide edit panel on click any floor element other than tables
    $("#floor .item img").live("click", function (e) {
        e.stopPropagation();
        $(".rotate").hide();
        $(".seat,.quan-2-other1,.quan-2-1,.quan-4-1,.item img").removeClass("secHover");
        $(this).addClass("secHover");
        $(this).parents(".table-main").find(".rotate").show();
        $('.table-tabs .nav-m ul.main li:first-child a').click();
        ClearEditPanel();
    });
});

function ClearEditPanel() {
    $('#TName').val('');
    $('#hdnTShape').val('');
    $('.TShape').removeClass('active');
    $('#TAngle').val('');
}

// Table Methods 

function CreateTable(tableName, design, minCover, maxCover) {
    $("#totalTable").text(tableno);
    $("#tblMaxCvr").text(totalMaxCover + maxCover);
    $("#tblMinCvr").text(totalMinCover + minCover);

    tableName = 'T-' + tableno;

    switch (design) {

        //******* Round  ******//                                                                                   
        case 'Round':

            var radius = (maxCover * 15) + 25;
            // 
            if (radius < 100) {
                radius = 100;
            }

            var cx, cy;
            cx = cy = (radius - 20) / 2;
            var angle = (360 / maxCover);
            var sumAngl = 0;

            $("#floor").append('<div class="table-main" id="table' + tableno + '"><div class="c-container" style ="height: ' + (radius + 3) + 'px; width: ' + (radius + 3) + 'px;"><div onclick="EditTable(this);" class="quan-2-1 table" style ="height: ' + (radius - 30) + 'px; width: ' + (radius - 30) + 'px; left:8px; top:14px;"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><h3>' + tableName + '</h3><p>' + minCover + '/' + maxCover + '</p></div></div></div>');
            // alert(radius+' '+cx+' '+angle);

            //                  var cradius = ($("#table"+tableno+" .circleContainer").width()/2)*.90;
            //                  var p = $("#table"+tableno+" .circleContainer").position();
            //                  var cx = p.left + cradius;
            //                  var cy = p.top + cradius;

            for (var n = 0; n < maxCover; n++) {
                var x = parseInt(cx + (cx * Math.cos(sumAngl * Math.PI / 180)));
                var y = parseInt(cx + (cx * Math.sin(sumAngl * Math.PI / 180)));

                // alert(radius+" "+x+" > "+y+" > "+Math.cos(sumAngl * Math.PI / 180));


                $("#table" + tableno).find('.c-container').append('<div class="seat right-chair seat' + n + '" style="float:left; top:' + y + 'px; left:' + x + 'px; position:absolute;margin: 0px;"></div>');
                $("#table" + tableno).find(".seat" + n).css({
                    '-moz-transform': 'rotate(' + (sumAngl) + 'deg)',
                    '-webkit-transform': 'rotate(' + (sumAngl) + 'deg)',
                    '-o-transform': 'rotate(' + (sumAngl) + 'deg)',
                    '-ms-transform': 'rotate(' + (sumAngl) + 'deg)'
                });

                sumAngl = sumAngl + angle;
            }

            $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="' + tableName + '"/><input id="tbldesign" name="tbldesign" type="hidden" value="' + design + '"/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="' + minCover + '"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="' + maxCover + '"/>');

            break;


        //******* Square  ******//                                                                                   
        case 'Square':
            // 
            var leftSeat = 0, rightSeat = 0, topSeat = 0, bottomSeat = 0;

            var remainder = maxCover % 4;
            var divison = (maxCover - remainder) / 4;
            leftSeat = rightSeat = topSeat = bottomSeat = divison;

            if (remainder == 1) { topSeat += 1 }
            if (remainder == 2) { topSeat += 1; rightSeat += 1 }
            if (remainder == 3) { topSeat += 1; rightSeat += 1; bottomSeat += 1 }
            // alert(remainder+''+topSeat+''+rightSeat+''+bottomSeat+''+leftSeat);


            $("#floor").append('<div id="table' + tableno + '" class="table table-main"><div class ="top-seat1"> </div></div>');
            for (var i = 0; i < topSeat; i++) {
                $("#table" + tableno + ' .top-seat1').append('<div class="seat top-chair" style="float:left;"></div>');
            }


            $("#table" + tableno).append('<div class="seat-mid-section"><div class="left-seats-sec" style="float:left;"></div><div class="right-seats-sec" style="float:right;"></div></div><div class ="bottom-seat1"> </div>');
            //var midSection = $(".seat-mid-section").last().width();

            //            if (midSection < 35) {
            //                midSection = 35;
            //            }

            var seatWidth = $('#table' + tableno + ' .seat').width();
            $("#table" + tableno + ' .seat-mid-section .left-seats-sec').after('<div  onclick="EditTable(this);" class="quan-2-other1" style="position: relative;width:' + ((topSeat * seatWidth) + 12) + 'px; float:left;height: ' + ((topSeat * seatWidth) + 12) + 'px; border:1px solid #979191;"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div> <h3>' + tableName + '</h3><p>' + minCover + '/' + maxCover + '</p> </div></div>');

            for (var k = 0; k < leftSeat; k++) {
                $("#table" + tableno + ' .left-seats-sec').append('<div class="seat left-chair" style="display:block; clear:both;"></div>');
            }

            for (var m = 0; m < rightSeat; m++) {
                $("#table" + tableno + ' .right-seats-sec').append('<div class="seat right-chair" style="display:block; clear:both;"></div>');
            }

            for (var j = 0; j < bottomSeat; j++) {
                $("#table" + tableno + ' .bottom-seat1').append('<div class="seat bottom-chair" style="float:left;"></div>');
            }

            $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="' + tableName + '"/><input id="tbldesign" name="tbldesign" type="hidden" value="' + design + '"/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="' + minCover + '"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="' + maxCover + '"/>');

            break;



        //******* Rectangle  ******//                              
        case 'Rectangular':
            // 
            var sideSeat = 0, topSeat = 0, bottomSeat = 0;
            //var contWidth = (20*maxCover)+(((maxCover-2)/2)*5);
            //alert(contWidth);
            sideSeat = maxCover - 2;
            if (sideSeat % 2 === 0) {
                topSeat = sideSeat / 2;
                bottomSeat = topSeat;
            }
            else {
                topSeat = (sideSeat + 1) / 2;
                bottomSeat = topSeat - 1;
            }
            //alert(sideSeat+''+topSeat+''+bottomSeat);

            $("#floor").append('<div id="table' + tableno + '" class="table table-main"><div class ="top-seat1"> </div></div>');
            for (var i = 0; i < topSeat; i++) {
                $("#table" + tableno + ' .top-seat1').append('<div class="seat top-chair" style="float:left;"></div>');
            }


            $("#table" + tableno).append('<div class="seat-mid-section"><div class="seat l-seat left-chair" style="float:left;"></div><div class="seat r-seat right-chair" style="float:right;"></div></div><div class ="bottom-seat1"> </div>');
            //var midSection = $(".seat-mid-section").last().width();

            //            if (midSection < 35) {
            //                midSection = 35;
            //            }

            var seatWidth = $('#table' + tableno + ' .seat').width();
            $("#table" + tableno + ' .seat-mid-section .l-seat').after('<div onclick="EditTable(this);" class="quan-4-1" style="position: relative;width:' + ((topSeat * seatWidth) + 12) + 'px; float:left;height: 32px; border:1px solid #979191;"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div>  <h3>' + tableName + '</h3> <p>' + minCover + '/' + maxCover + '</p></div></div>');


            for (var j = 0; j < bottomSeat; j++) {
                $("#table" + tableno + ' .bottom-seat1').append('<div class="seat bottom-chair" style="float:left;"></div>');
            }

            $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="' + tableName + '"/><input id="tbldesign" name="tbldesign" type="hidden" value="' + design + '"/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="' + minCover + '"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="' + maxCover + '"/>');

            break;


    }

    $('.table').click(function () {
        // 
        //        var table = $(this);
        //        var tableName = table.find('#tblname').val();
        //        var tableShape = table.find('#tbldesign').val();
        //        var tableAngle = table.find('#tblangle').val();
        //        var tableMinCover = table.find('#tblmincover').val();
        //        var tableMaxCover = table.find('#tblmaxcover').val();

        //        TableOptions.TableName = tableName;
        //        TableOptions.TableShape = tableShape;
        //        TableOptions.TableAngle = tableAngle;
        //        TableOptions.TableMinCover = tableMinCover;
        //        TableOptions.TableMaxCover = tableMaxCover;

        //        $("#tabs").tabs({ active: 3 });

    });

    UpdateItem("#table" + tableno);
    tableno++;
}

function RotateAnti(item) {
    var deg = parseInt($(item).parents('.table-main').find('#tblangle').val());
    var tableId = $(item).attr("title");
    //if(deg == -360){deg = 0;}
    deg -= 45;
    $("#" + tableId).css('');
    $("#" + tableId).css({
        '-moz-transform': 'rotate(' + (deg) + 'deg)',
        '-webkit-transform': 'rotate(' + (deg) + 'deg)',
        '-o-transform': 'rotate(' + (deg) + 'deg)',
        '-ms-transform': 'rotate(' + (deg) + 'deg)'
    });

    $(item).parents('.table-main').find('#tblangle').val(deg);
    UpdateItem('#' + $($(item).parents('.table-main')[0]).attr('id'));
}

function RotateClock(item) {
    //var deg = 0;
    var deg = parseInt($(item).parents('.table-main').find('#tblangle').val());
    var tableId = $(item).attr("title");

    if (deg == 360) { deg = 0; }
    deg += 45;
    $("#" + tableId).css('');
    $("#" + tableId).css({
        '-moz-transform': 'rotate(' + (deg) + 'deg)',
        '-webkit-transform': 'rotate(' + (deg) + 'deg)',
        '-o-transform': 'rotate(' + (deg) + 'deg)',
        '-ms-transform': 'rotate(' + (deg) + 'deg)'
    });

    $(item).parents('.table-main').find('#tblangle').val(deg);

    UpdateItem('#' + $($(item).parents('.table-main')[0]).attr('id'));
}



function CreateSofa() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/sofa.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="Sofa-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}

function CreateChair() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/single-chair.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="Chair-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}

function CreateTwoSideRectangle(tableName, minCover, maxCover) {
    $("#totalTable").text(tableno);
    $("#tblMaxCvr").text(totalMaxCover + maxCover);
    $("#tblMinCvr").text(totalMinCover + minCover);

    var design = 'Rectangular';
    tableName = 'T-' + tableno;

    var topSeat = 0, bottomSeat = 0;
    //var contWidth = (20*tableSeat)+(((tableSeat-2)/2)*5);
    //alert(contWidth);

    if (maxCover % 2 === 0) {
        topSeat = maxCover / 2;
        bottomSeat = topSeat;
    }
    else {
        topSeat = (maxCover + 1) / 2;
        bottomSeat = topSeat - 1;
    }
    // alert(tableSeat+''+topSeat+''+bottomSeat);

    $("#floor").append('<div id="table' + tableno + '" class="table table-main"><div class ="top-seat1" style="margin:0"> </div></div>');
    for (var i = 0; i < topSeat; i++) {
        $("#table" + tableno + ' .top-seat1').append('<div class="seat top-chair" style="float:left;"></div>');
    }


    $("#table" + tableno).append('<div class="seat-mid-section"></div><div class ="bottom-seat1" style="margin:0"> </div>');
    var seatWidth = $('#table' + tableno + ' .seat').width();

    $("#table" + tableno + ' .seat-mid-section').append('<div onclick="EditTable(this);" class="quan-4-1" style="position: relative;width:' + ((topSeat * seatWidth) + 12) + 'px; float:left;height: 32px; border:1px solid #979191;"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div>  <h3>' + tableName + '</h3> <p>' + minCover + '/' + maxCover + '</p></div></div>');


    for (var j = 0; j < bottomSeat; j++) {
        $("#table" + tableno + ' .bottom-seat1').append('<div class="seat bottom-chair" style="float:left;"></div>');
    }

    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="T-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value="' + design + '"/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="' + minCover + '"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="' + maxCover + '"/>');

    UpdateItem("#table" + tableno);
    tableno++;
}


function CreateStaticTable() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/single-table.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="SofaTable-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}

function CreateWall() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/wall1.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="Wall-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}
function CreateSolidWall() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/solid-wall.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="SolidWall-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}
function CreateGlassWall() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/glass-wall.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="GlassWall-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}
function CreateBarTable() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/bar-table.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="BarTable-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}
function CreateFence() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/fence.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="Fence-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}
function CreatePillar() {
    $("#floor").append('<div id="table' + tableno + '" class="table-main table item"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="dltItem" onclick="DeleteItem(this);">X</a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><img alt="" src="/images/pillar.png"></div>');
    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="Pillar-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
    UpdateItem("#table" + tableno);
    tableno++;
}

//function AddItem(item) {

//    var TableObj = {
//        TableName: $(item).find('#TName').val(),
//        TableDesign: $(item).outerHTML(),
//        Angle: $(item).find('#TAngle').val(),
//        MinCover: $(item).find('#TMinCover').val(),
//        MaxCover: $(item).find('#TMinCover').val(),
//        TLeft: $(item).css('left').val(),
//        TTop: $(item).css('left').val()
//    };

//    $.ajax({
//        type: "POST",
//        url: "/Floor/AddItem",
//        data: JSON.stringify(TableObj),
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        success: function (result) {
//            alert(result.Status);
//            console.log(result);
//        }
//    });

//}



//function CreateSofa() {
//    // 
//    $("#floor").append('<div id="table' + tableno + '" class="walls-main table-main table item "><div  class="sofa"><div class="sofa-inner"><div class="s-left"></div><div class="s-right"></div><div class="s-bottom"></div></div><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div></div></div>');
//    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="Sofa-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="3"/>');
//    tableno++;
//}

//function CreateChair() {
//    $("#floor").append('<div id="table' + tableno + '" class="walls-main table-main table item "><div  class="o-s-chair"><div class="o-s-chair-inner"><div class="c-left"></div><div class="c-right"></div><div class="c-top"></div></div><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div></div></div>');
//    $("#table" + tableno).append('<input id="TempFloorTableId" name="TempFloorTableId" type="hidden" value=""/><input id="tblname" name="tblname" type="hidden" value="Chair-' + tableno + '"/><input id="tbldesign" name="tbldesign" type="hidden" value=""/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="1"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="1"/>');
//    tableno++;
//}


//$(document).ready(function () {
//    $("#addTable").click(function () {
//        var tableName = $("#TName").val();
//        var design = $('#hdnTShape').val();
//        var minCover = $('#TMinCover').val();
//        var maxCover = $('#TMaxCover').val();

//        if (design != '') {
//            switch (design) {

//                //******* Round  ******//                                                               
//                case 'Round':

//                    var radius = (maxCover * 15) + 25;
//                    // 
//                    if (radius < 88) {
//                        radius = 88;
//                    }

//                    var cx, cy;
//                    cx = cy = (radius - 20) / 2;
//                    var angle = (360 / maxCover);
//                    var sumAngl = 0;

//                    $("#floor").append('<div class="table-main" id= ""><div class="c-container" style ="height: ' + (radius + 3) + 'px; width: ' + (radius + 3) + 'px;"><div id="table' + tableno + '" class="quan-2-1 table" style ="height: ' + (radius - 45) + 'px; width: ' + (radius - 45) + 'px; left:22px; top:22px;"><div class="lblCircleTable lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><a style="position: absolute;" title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div></div>');
//                    // alert(radius+' '+cx+' '+angle);

//                    //                  var cradius = ($("#table"+tableno+" .circleContainer").width()/2)*.90;
//                    //                  var p = $("#table"+tableno+" .circleContainer").position();
//                    //                  var cx = p.left + cradius;
//                    //                  var cy = p.top + cradius;

//                    for (var n = 0; n < maxCover; n++) {
//                        var x = parseInt(cx + (cx * Math.cos(sumAngl * Math.PI / 180)));
//                        var y = parseInt(cx + (cx * Math.sin(sumAngl * Math.PI / 180)));

//                        // alert(radius+" "+x+" > "+y+" > "+Math.cos(sumAngl * Math.PI / 180));


//                        $("#table" + tableno).parent().append('<div class="right-chair seat seat' + n + '" style="float:left; top:' + y + 'px; left:' + x + 'px; position:absolute;"></div>');
//                        $("#table" + tableno).parent().find(".seat" + n).css({
//                            '-moz-transform': 'rotate(' + (sumAngl) + 'deg)',
//                            '-webkit-transform': 'rotate(' + (sumAngl) + 'deg)',
//                            '-o-transform': 'rotate(' + (sumAngl) + 'deg)',
//                            '-ms-transform': 'rotate(' + (sumAngl) + 'deg)'
//                        });

//                        sumAngl = sumAngl + angle;
//                    }

//                    $("#table" + tableno).append('<input id="tblname" name="tblname" type="hidden" value="' + tableName + '"/><input id="tbldesign" name="tbldesign" type="hidden" value="' + design + '"/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="' + minCover + '"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="' + maxCover + '"/>');

//                    break;


//                //******* Square  ******//                                                               
//                case 'Square':
//                    // 
//                    var leftSeat = 0, rightSeat = 0, topSeat = 0, bottomSeat = 0;

//                    var remainder = maxCover % 4;
//                    var divison = (maxCover - remainder) / 4;
//                    leftSeat = rightSeat = topSeat = bottomSeat = divison;

//                    if (remainder == 1) { topSeat += 1 }
//                    if (remainder == 2) { topSeat += 1; rightSeat += 1 }
//                    if (remainder == 3) { topSeat += 1; rightSeat += 1; bottomSeat += 1 }
//                    // alert(remainder+''+topSeat+''+rightSeat+''+bottomSeat+''+leftSeat);


//                    $("#floor").append('<div id="table' + tableno + '" class="table table-main"><div class ="top-seat1"> </div></div>');
//                    for (var i = 0; i < topSeat; i++) {
//                        $("#table" + tableno + ' .top-seat1').append('<div class="seat top-chair" style="float:left;"></div>');
//                    }


//                    $("#table" + tableno).append('<div class="seat-mid-section"><div class="left-seats-sec" style="float:left;"></div><div class="right-seats-sec" style="float:right;"></div></div><div class ="bottom-seat1"> </div>');
//                    var midSection = $(".seat-mid-section").last().width();

//                    if (midSection < 35) {
//                        midSection = 35;
//                    }

//                    var seatWidth = $(".seat").width();
//                    $("#table" + tableno + ' .seat-mid-section .left-seats-sec').after('<div class="quan-2-other1" style="position: relative;width:' + (midSection - (2 * seatWidth)) + 'px; float:left;min-height: ' + (rightSeat * seatWidth) + 'px; border:1px solid #979191;"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><div class="lblTable1 lblTable">' + tableName + '</div><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');

//                    for (var k = 0; k < leftSeat; k++) {
//                        $("#table" + tableno + ' .left-seats-sec').append('<div class="seat left-chair" style="display:block; clear:both;"></div>');
//                    }

//                    for (var m = 0; m < rightSeat; m++) {
//                        $("#table" + tableno + ' .right-seats-sec').append('<div class="seat right-chair" style="display:block; clear:both;"></div>');
//                    }

//                    for (var j = 0; j < bottomSeat; j++) {
//                        $("#table" + tableno + ' .bottom-seat1').append('<div class="seat bottom-chair" style="float:left;"></div>');
//                    }

//                    $("#table" + tableno).append('<input id="tblname" name="tblname" type="hidden" value="' + tableName + '"/><input id="tbldesign" name="tbldesign" type="hidden" value="' + design + '"/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="' + minCover + '"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="' + maxCover + '"/>');

//                    break;



//                //******* Rectangle  ******//                                           
//                case 'Rectangular':
//                    var sideSeat = 0, topSeat = 0, bottomSeat = 0;
//                    //var contWidth = (20*maxCover)+(((maxCover-2)/2)*5);
//                    //alert(contWidth);
//                    sideSeat = maxCover - 2;
//                    if (sideSeat % 2 === 0) {
//                        topSeat = sideSeat / 2;
//                        bottomSeat = topSeat;
//                    }
//                    else {
//                        topSeat = (sideSeat + 1) / 2;
//                        bottomSeat = topSeat - 1;
//                    }
//                    //alert(sideSeat+''+topSeat+''+bottomSeat);

//                    $("#floor").append('<div id="table' + tableno + '" class="table table-main"><div class ="top-seat1"> </div></div>');
//                    for (var i = 0; i < topSeat; i++) {
//                        $("#table" + tableno + ' .top-seat1').append('<div class="seat top-chair" style="float:left;"></div>');
//                    }


//                    $("#table" + tableno).append('<div class="seat-mid-section"><div class="seat l-seat left-chair" style="float:left;"></div><div class="seat r-seat right-chair" style="float:right;"></div></div><div class ="bottom-seat1"> </div>');
//                    var midSection = $(".seat-mid-section").last().width();

//                    if (midSection < 35) {
//                        midSection = 35;
//                    }

//                    var seatWidth = $(".seat").width();
//                    $("#table" + tableno + ' .seat-mid-section .l-seat').after('<div class="quan-4-1" style="position: relative;width:' + (midSection - (2 * seatWidth)) + 'px; float:left;height: 32px; border:1px solid #979191;"><div class ="rotate" style="display:none;"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><div class="lblTable1 lblTable">' + tableName + '</div><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');


//                    for (var j = 0; j < bottomSeat; j++) {
//                        $("#table" + tableno + ' .bottom-seat1').append('<div class="seat bottom-chair" style="float:left;"></div>');
//                    }

//                    $("#table" + tableno).append('<input id="tblname" name="tblname" type="hidden" value="' + tableName + '"/><input id="tbldesign" name="tbldesign" type="hidden" value="' + design + '"/><input id="tblangle" name="tblangle" type="hidden" value="0"/><input id="tblmincover" name="tblmincover" type="hidden" value="' + minCover + '"/><input id="tblmaxcover" name="tblmaxcover" type="hidden" value="' + maxCover + '"/>');

//                    break;


//            }

//            $('.table').click(function () {
//                // 
//                var table = $(this);
//                var tableName = table.find('#tblname').val();
//                var tableShape = table.find('#tbldesign').val();
//                var tableAngle = table.find('#tblangle').val();
//                var tableMinCover = table.find('#tblmincover').val();
//                var tableMaxCover = table.find('#tblmaxcover').val();

//                TableOptions.TableName = tableName;
//                TableOptions.TableShape = tableShape;
//                TableOptions.TableAngle = tableAngle;
//                TableOptions.TableMinCover = tableMinCover;
//                TableOptions.TableMaxCover = tableMaxCover;

//                $("#tabs").tabs({ active: 3 });

//            });
//        }
//        else {
//            alert("Please select a design for table.");
//        }
//        //alert(maxCover+''+tableName);
//        tableno++;
//        $("#tableName").val("table" + tableno);
//    });
//});