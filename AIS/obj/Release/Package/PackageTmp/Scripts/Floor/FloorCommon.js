var counter = 0, oneGero = 0, tableno = 1;
var ajaxUrl = "/jquerydnd/";
$(document).ready(function () {
    //Counter

    $(".drag").live('mouseover', function () {
        counter = $("#maxcount").val();
    });

    //Make element draggable
    $(".drag").draggable({
        helper: 'clone',
        containment: '#wrapper',

        //When first dragged
        stop: function (ev, ui) {
            var pos = $(ui.helper).offset();
            objName = "#clonediv" + counter
            $(objName).css({
                "left": pos.left,
                "top": pos.top,
                "float": "left"
            });
            $(objName).removeClass("drag");


            //When an existiung object is dragged
            $(objName).draggable({
                containment: 'parent',
                stop: function (ev, ui) {
                    var pos = $(ui.helper).offset();
                    console.log($(this).attr("id"));
                    console.log(pos.left)
                    console.log(pos.top)
                }
            });
        }
    });
    //Make element droppable
    $("#floor").droppable({
        // accept: '.table1',
        drop: function (ev, ui) {
            if (ui.draggable.attr('id').search(/drag[0-9]/) != -1) {
                counter++;
                var element = $(ui.draggable).clone();
                element.addClass("tempclass");
                $(this).append(element);
                $(".tempclass").attr("id", "clonediv" + counter);
                $("#clonediv" + counter).removeClass("tempclass");

                //Get the dynamically item id
                draggedNumber = ui.draggable.attr('id').search(/drag([0-9])/)
                itemDragged = "dragged" + RegExp.$1
                console.log(itemDragged)

                $("#clonediv" + counter).addClass(itemDragged);
                $("#maxcount").val(counter);
            }
        }
    });

    //    $("#floor .ui-draggable").live('mouseover', function(){
    //       // $(this).hide();
    //        $(this).draggable({
    //             containment: '#floor'
    //        });
    //    });

    $("#floor .node, #floor .table1, #floor .table").live('mouseover', function () {

        $(this).draggable({
            containment: '#floor'
        });

    });



    $("#floor .table1").live('hover', function () {
        $(this).droppable({
            drop: function (ev, ui) {
                alert('hi');
            }
        });
    });


    //*************  Saving Design ********************//

    $("#save").click(function () {
        var filename = prompt("File Name", '');
        $(".section").removeClass("secHover");
        $(".row").removeClass("secHover");
        $(".seat").removeClass("secHover");

        if (filename != '' && filename != null) {
            var content = $("#floor").html();
            var maxcount = $("#maxcount").val();
            $.post("main/saveframe", { 'filename': filename, 'content': content, 'maxcount': maxcount },
                function (data) {
                    alert(data);
                }, "json");
        }
    });


    //***********  open Design  ********************//

    $(".design").click(function () {
        var design_id = $(this).attr("title");

        $.post("main/open", { 'design_id': design_id },
                function (data) {
                    $("#floor").html(data.content);
                    $("#maxcount").val(data.maxcount);
                }, "json");
    });


    //*************  Node Information Form ********************//

    $("#floor .ui-draggable, .seat").live('dblclick', function () {

        var node_id = $(this).attr("id");
        var node_name = $("#" + node_id + " label[for = name]").text();
        var node_des = $("#" + node_id + " label[for = des]").text();

        $.post('main/nodefrom', { 'node_id': node_id, 'node_name': node_name, 'node_des': node_des }, function (data) {
            $('#form_info').html(data);
        }, "json");
    });


    $("#node_save").live('click', function () {
        var node_id = $("#node_id").val();
        var name = $("#name").val();
        var des = $("#des").val();

        $("#" + node_id + " label[for = name]").text(name);
        $("#" + node_id + " label[for = des]").text(des);

        $("#form_info").html('');
    });



    //*************  Category Selector  ***************//

    // $("#design-option").hide();
    $("#floor-plan").hide();

    $("#category input:radio").click(function () {
        var category = $(this).val();

        $("#floor").html('');
        $("#form_info").html('');
        tableno = 1;
        $("#tableName").val("table" + tableno);

        if (category == 'Auditorium') {
            $("#floor-plan").hide();
            $("#design-option").show();
        }
        else if (category == 'Floor') {
            $("#design-option").hide();
            $("#floor-plan").show();
        }
    });

    //*************  Populate Design  *********************//

    $("#go").click(function () {
        var section = $("#section").val();
        var rows = $("#rows").val();
        var seat = $("#seat").val();
        var frameWidth = $("#floor").width();

        var secWidth = Math.floor((frameWidth - 5) / section);
        var seatWidth = Math.floor((secWidth - 30) / seat);

        $("#floor").html('');

        $("#floor").append('<div class = "stage"><div class="lblStage">Stage</div></div>');
        $("#floor").append('<div id = "seat-plane"></div>');

        for (var i = 1; i <= section; i++) {

            $("#seat-plane").append('<div class = "section" id ="sec_' + i + '" style="float:left; overflow: hidden;  margin:0px 1px; width: ' + (secWidth - 2) + 'px">S' + i + '</div>');
            for (var j = 1; j <= rows; j++) {
                $("#sec_" + i).append('<div class="row" style="padding:7px 0px; margin:5px 5px; border: 1px solid #A5A5A5; overflow: hidden;" id="row_' + i + '_' + j + '"></div>');

                var sec_id = "#sec_" + i;
                var row_id = "row_" + i + "_" + j;
                $("#" + row_id).css('clear', 'both');

                for (var k = 1; k <= seat; k++) {
                    $("#" + sec_id + " " + "#row_" + i + '_' + j).append('<div class = "seat seat-bottom" style="float:left; margin:0px 1px; border: 0px solid #b2b2b2; width: ' + (seatWidth - 0) + 'px" id ="seat_' + i + '_' + j + '_' + k + '"><label for="name"> &nbsp;</label><label for="des"> </label><input class="bookingInfo" type="hidden" alt="notconfirm" title="none" value="available" name="bookingInfo"></div>');
                }
            }
        }

    });

    //************* for Section, Row, Seat Sortable   *****************//

    $("#floor .section").live('mouseover', function () {

        $(".section").sortable({ axis: 'y' });
        $(".section").disableSelection();

    });

    $("#floor .row").live('mouseover', function () {

        $(".row").sortable();
        $(".row").disableSelection();

    });

    $("#floor #seat-plane").live('mouseover', function () {

        $("#seat-plane").sortable();
        $("#seat-plane").disableSelection();

    });


    //************* for Row, Seat edit option  *****************//

    $("#floor .seat").live('click', function () {
        $(".seat").removeClass("secHover");
        $(this).addClass("secHover");
        $("#form_info").html('<div id="sec_option" class ="edit_option"> <ul><li><a id="node_delete" href="javascript:void(0);" title="' + this.id + '" >Delete Seat</a></li></ul></div>');

    });

    $("#floor .row").live('dblclick', function () {
        $(".row").removeClass("secHover");
        $(".seat").removeClass("secHover");
        $(this).addClass("secHover");
        $("#form_info").html('<div id="sec_option" class ="edit_option"> <ul><li><a id="node_delete" href="javascript:void(0);" title="' + this.id + '" >Delete Row</a></li></ul></div>');

    });


    //************* for Row, Seat Selection  *****************//

    $("#floor .section").live('click', function () {
        $("#floor .section").removeClass("secHover");
        $(this).addClass("secHover");
    });

    $("#floor .row").live('click', function () {
        $("#floor .row").removeClass("secHover");
        $(this).addClass("secHover");
    });

    $("#floor1 .seat").live('click', function () {
        $("#floor1 .seat").removeClass("secHover");
        $(this).addClass("secHover");
    });


    //************* for Row, Seat Delete  *****************//

    $("#node_delete").live('click', function () {
        var delnode_id = $(this).attr('title');
        $("#" + delnode_id).css('visibility', 'hidden');
        $("#form_info").html('');
    });

    //     $('#box1').css({
    //     '-moz-transform':'rotate(45deg)',
    //     '-webkit-transform':'rotate(88deg)',
    //     '-o-transform':'rotate(88deg)',
    //     '-ms-transform':'rotate(88deg)'
    //    });

    //************************* End of Auditorium Sectio  **********************************//




    //***********************************  Floor Plan Section  *************************************//



    $("#addTable").click(function () {
        var tableSeat = parseInt($("#tableSeat").val());
        var tableName = $("#tableName").val();
        var design = $('input:radio[name=design]:checked').val();

        switch (design) {

            //******* Four Side  ******//  
            case 'fourside':
                var sideSeat = 0, topSeat = 0, bottomSeat = 0;
                //var contWidth = (20*tableSeat)+(((tableSeat-2)/2)*5);
                //alert(contWidth);
                sideSeat = tableSeat - 2;
                if (sideSeat % 2 === 0) {
                    topSeat = sideSeat / 2;
                    bottomSeat = topSeat;
                }
                else {
                    topSeat = (sideSeat + 1) / 2;
                    bottomSeat = topSeat - 1;
                }
                //alert(sideSeat+''+topSeat+''+bottomSeat);

                $("#floor").append('<div id="table' + tableno + '" class="table bothSideSeat"><div class ="topSeat1"> </div></div>');
                for (var i = 0; i < topSeat; i++) {
                    $("#table" + tableno + ' .topSeat1').append('<div class="seat seat-top" style="float:left;"></div>');
                }


                $("#table" + tableno).append('<div class="midSection"><div class="seat lrSeat leftSeat seat-left" style="float:left;"></div><div class="seat lrSeat seat-right" style="float:right;"></div></div><div class ="bottomSeat1"> </div>');
                var midSection = $(".midSection").last().width();
                var seatWidth = $(".seat").width();
                $("#table" + tableno + ' .midSection .leftSeat').after('<div style="width:' + (midSection - (2 * seatWidth)) + 'px; float:left; height: 32px; border:1px solid #979191;"><div class ="rotate"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><div class="lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="transform"></a><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');


                for (var j = 0; j < bottomSeat; j++) {
                    $("#table" + tableno + ' .bottomSeat1').append('<div class="seat seat-bottom" style="float:left;"></div>');
                }
                break;


            //******* one side  ******//  
            case 'oneside':
                $("#floor").append('<div id="table' + tableno + '" class="table oneSideSeat"></div>');
                for (var i = 0; i < tableSeat; i++) {
                    $("#table" + tableno).append('<div class="seat seat-top" style="float:left;"></div>');
                }

                $("#table" + tableno).append('<div class="midSection" style="height: 32px; clear:both; border:1px solid #979191;"><div class ="rotate"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><div class="lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="transform"></a><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');
                break;



            //******* Two Side  ******//  
            case 'twoside':
                var topSeat = 0, bottomSeat = 0;
                //var contWidth = (20*tableSeat)+(((tableSeat-2)/2)*5);
                //alert(contWidth);

                if (tableSeat % 2 === 0) {
                    topSeat = tableSeat / 2;
                    bottomSeat = topSeat;
                }
                else {
                    topSeat = (tableSeat + 1) / 2;
                    bottomSeat = topSeat - 1;
                }
                // alert(tableSeat+''+topSeat+''+bottomSeat);

                $("#floor").append('<div id="table' + tableno + '" class="table bothSideSeat"><div class ="topSeat"> </div></div>');
                for (var i = 0; i < topSeat; i++) {
                    $("#table" + tableno + ' .topSeat').append('<div class="seat seat-top" style="float:left;"></div>');
                }


                $("#table" + tableno).append('<div class="midSection"></div><div class ="bottomSeat"> </div>');
                var midSection = $(".midSection").last().width();

                $("#table" + tableno + ' .midSection').append('<div style="width:' + (midSection) + 'px; float:left; height: 32px; border:1px solid #979191;"><div class ="rotate"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><div class="lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="transform"></a><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');


                for (var j = 0; j < bottomSeat; j++) {
                    $("#table" + tableno + ' .bottomSeat').append('<div class="seat seat-bottom" style="float:left;"></div>');
                }

                break;

            //******* Three Side  ******//  
            case 'threeside':
                var sideSeat = 0, topSeat = 0, bottomSeat = 0;
                //var contWidth = (20*tableSeat)+(((tableSeat-2)/2)*5);
                //alert(contWidth);
                sideSeat = tableSeat - 2;

                topSeat = sideSeat;

                //alert(sideSeat+''+topSeat+''+bottomSeat);

                $("#floor").append('<div id="table' + tableno + '" class="table oneSideSeat"><div class ="topSeat1"> </div></div>');
                for (var i = 0; i < topSeat; i++) {
                    $("#table" + tableno + ' .topSeat1').append('<div class="seat seat-top" style="float:left;"></div>');
                }


                $("#table" + tableno).append('<div class="midSection"><div class="seat lrSeat leftSeat seat-left" style="float:left;"></div><div class="seat circle lrSeat seat-right" style="float:right;"></div></div>');
                var midSection = $(".midSection").last().width();
                var seatWidth = $(".seat").width();
                $("#table" + tableno + ' .midSection .leftSeat').after('<div style="width:' + (midSection - (2 * seatWidth)) + 'px; float:left;height: 32px; border:1px solid #979191;"><div class ="rotate"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><div class="lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="transform"></a><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');

                break;


            //******* Three Side with Boss  ******//  
            case 'threesideboss':
                var sideSeat = 0, topSeat = 0, bottomSeat = 0;
                //var contWidth = (20*tableSeat)+(((tableSeat-2)/2)*5);
                //alert(contWidth);
                sideSeat = tableSeat - 1;
                if (sideSeat % 2 === 0) {
                    topSeat = sideSeat / 2;
                    bottomSeat = topSeat;
                }
                else {
                    topSeat = (sideSeat + 1) / 2;
                    bottomSeat = topSeat - 1;
                }
                //alert(sideSeat+''+topSeat+''+bottomSeat);

                $("#floor").append('<div id="table' + tableno + '" class="table bothSideSeat"><div class ="topSeat2"> </div></div>');
                for (var i = 0; i < topSeat; i++) {
                    $("#table" + tableno + ' .topSeat2').append('<div class="seat seat-top" style="float:left;"></div>');
                }


                $("#table" + tableno).append('<div class="midSection"><div class="seat lrSeat seat-right" style="float:right;"></div></div><div class ="bottomSeat2"> </div>');
                var midSection = $(".midSection").last().width();
                var seatWidth = $(".seat").width();
                $("#table" + tableno + ' .midSection .lrSeat').before('<div style="width:' + (midSection - (1 * seatWidth)) + 'px; float:left; height: 32px; border:1px solid #979191;"><div class ="rotate"><a title="table' + tableno + '" href="javascript:void(0);" class="rotateAnti"> </a><a title="table' + tableno + '" href="javascript:void(0);" class="rotateClock"> </a></div><div class="lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="transform"></a><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');


                for (var j = 0; j < bottomSeat; j++) {
                    $("#table" + tableno + ' .bottomSeat2').append('<div class="seat seat-bottom" style="float:right;"></div>');
                }
                break;


            //******* Square Side  ******//  
            case 'square':
                var leftSeat = 0, rightSeat = 0, topSeat = 0, bottomSeat = 0;

                var remainder = tableSeat % 4;
                var divison = (tableSeat - remainder) / 4;
                leftSeat = rightSeat = topSeat = bottomSeat = divison;

                if (remainder == 1) { topSeat += 1 }
                if (remainder == 2) { topSeat += 1; rightSeat += 1 }
                if (remainder == 3) { topSeat += 1; rightSeat += 1; bottomSeat += 1 }
                // alert(remainder+''+topSeat+''+rightSeat+''+bottomSeat+''+leftSeat);


                $("#floor").append('<div id="table' + tableno + '" class="table bothSideSeat"><div class ="topSeat1"> </div></div>');
                for (var i = 0; i < topSeat; i++) {
                    $("#table" + tableno + ' .topSeat1').append('<div class="seat seat-top" style="float:left;"></div>');
                }


                $("#table" + tableno).append('<div class="midSection"><div class="leftSeat" style="float:left;"></div><div class="rightSeat" style="float:right;"></div></div><div class ="bottomSeat1"> </div>');
                var midSection = $(".midSection").last().width();
                var seatWidth = $(".seat").width();
                $("#table" + tableno + ' .midSection .leftSeat').after('<div style="width:' + (midSection - (2 * seatWidth)) + 'px; float:left;min-height: ' + (rightSeat * seatWidth) + 'px; border:1px solid #979191;"><div class="lblTable1 lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><div class ="editTransform"><a title="table' + tableno + '" href="javascript:void(0);" class="editTable">Edit</a></div></div>');

                for (var k = 0; k < leftSeat; k++) {
                    $("#table" + tableno + ' .leftSeat').append('<div class="seat seat-left" style="display:block; clear:both;"></div>');
                }

                for (var m = 0; m < rightSeat; m++) {
                    $("#table" + tableno + ' .rightSeat').append('<div class="seat seat-right" style="display:block; clear:both;"></div>');
                }

                for (var j = 0; j < bottomSeat; j++) {
                    $("#table" + tableno + ' .bottomSeat1').append('<div class="seat seat-bottom" style="float:left;"></div>');
                }
                break;


            //******* Circle View  ******//  
            case 'circle':
                var radius = (tableSeat * 15) + 25;
                var cx, cy;
                cx = cy = (radius - 20) / 2;
                var angle = (360 / tableSeat);
                var sumAngl = 0;
                $("#floor").append('<div id="table' + tableno + '" class="table bothSideSeat"><div class ="circleContainer" style ="height: ' + (radius) + 'px; width: ' + (radius) + 'px;"><div class ="tableCircle" style ="left: 22px; top: 22px; right: 22px; bottom: 22px; position: absolute;"><div class="lblCircleTable lblTable">' + tableName + '</div><input type="hidden" class="bookingInfo" name="bookingInfo" value="available" title ="none" alt ="notconfirm"/><a title="table' + tableno + '" href="javascript:void(0);" class="lblCircleEdit">Edit</a> </div></div></div>');
                // alert(radius+' '+cx+' '+angle);

                //                  var cradius = ($("#table"+tableno+" .circleContainer").width()/2)*.90;
                //                  var p = $("#table"+tableno+" .circleContainer").position();
                //                  var cx = p.left + cradius;
                //                  var cy = p.top + cradius;

                for (var n = 0; n < tableSeat; n++) {
                    var x = parseInt(cx + (cx * Math.cos(sumAngl * Math.PI / 180)));
                    var y = parseInt(cx + (cx * Math.sin(sumAngl * Math.PI / 180)));

                    // alert(radius+" "+x+" > "+y+" > "+Math.cos(sumAngl * Math.PI / 180));


                    $("#table" + tableno + " .circleContainer").append('<div class="seat seat-right posAub seat' + n + '" style="float:left; top:' + y + 'px; left:' + x + 'px"></div>');
                    $("#table" + tableno + " .seat" + n).css({
                        '-moz-transform': 'rotate(' + (sumAngl) + 'deg)',
                        '-webkit-transform': 'rotate(' + (sumAngl) + 'deg)',
                        '-o-transform': 'rotate(' + (sumAngl) + 'deg)',
                        '-ms-transform': 'rotate(' + (sumAngl) + 'deg)'
                    });

                    sumAngl = sumAngl + angle;

                }

                break;

        }



        //alert(tableSeat+''+tableName);
        tableno++;
        $("#tableName").val("table" + tableno);
    });


    //************ Rotate Clockwise  *************//
    var deg = 0;
    $(".rotateClock").live('click', function () {

        var tableId = $(this).attr("title");

        if (deg == 360) { deg = 0; }
        deg += 90;
        $("#" + tableId).css('');
        $("#" + tableId).css({
            '-moz-transform': 'rotate(' + (deg) + 'deg)',
            '-webkit-transform': 'rotate(' + (deg) + 'deg)',
            '-o-transform': 'rotate(' + (deg) + 'deg)',
            '-ms-transform': 'rotate(' + (deg) + 'deg)'
        });

    });

    //************ Rotate AntiClockwise  *************//
    //var degAnti = 0;
    $(".rotateAnti").live('click', function () {

        var tableId = $(this).attr("title");
        //if(deg == -360){deg = 0;}
        deg -= 90;
        $("#" + tableId).css('');
        $("#" + tableId).css({
            '-moz-transform': 'rotate(' + (deg) + 'deg)',
            '-webkit-transform': 'rotate(' + (deg) + 'deg)',
            '-o-transform': 'rotate(' + (deg) + 'deg)',
            '-ms-transform': 'rotate(' + (deg) + 'deg)'
        });

    });


    //************ Table Transform  *************//

    $(".transform").live('mouseover', function () {
        var table_id = $(this).attr('title');
        //alert('hi');
        $("#" + table_id).transformable({ containment: 'parent' });
        $(".transformable-handle-skew").hide();

    });


    //************ Table Information Edit  *************//

    $(".editTable, .lblCircleEdit").live('click', function () {
        var table_id = $(this).attr('title');
        $("#form_info").html('');
        $("#form_info").html('<div class="tbledit-form"><div class ="title">Edit Table</div><input type="hidden" id="table_id" name="table_id" value="' + table_id + '" />' + '<ul><li><a id=rename-table>Rename Table</a></li><li><a id="delete-table">Delete Table</a></li></ul>' + '</div>')

    });


    //************* Edit Table  ****************//
    var formFlag = 0, formFlag1 = 0;
    $("#delete-table").live('click', function () {
        var table_id = $("#table_id").val();
        $("#" + table_id).remove();
        $("#form_info").html('');
    });

    $("#rename-table").live('click', function () {
        var table_id = $("#table_id").val();
        var link_id = $(this).attr('id');
        var tblName = $("#" + table_id + " .lblTable").text();
        formFlag++;



        if (formFlag == 1) {
            $("#" + link_id).after('<div id="rename-form"><span><input id="retblName" maxlength="7" type="text" value="' + tblName + '" name="retblName"></span><span><a id="renameTable" class="button" href="javascript:void(0);">Ok</a></span></div>');
        }
        else {
            $("#rename-form").remove();
            formFlag = 0;
        }
        //          $("#form_info").html('');
    });


    $("#renameTable").live('click', function () {
        var tblName = $("#retblName").val();
        var table_id = $("#table_id").val();

        $("#" + table_id + " .lblTable").text(tblName);
        formFlag = 0;

        $("#form_info").html('');
    });


    //     $("#reserve-table").live('click', function(){
    //             var table_id = $("#table_id").val();
    //             var link_id = $(this).attr('id');
    //             var tblName = $("#"+table_id+" .lblTable").text();
    //
    //             var reserveTo = $("#"+table_id+"  .bookingInfo").attr('title')
    //             var reserveStatus = $("#"+table_id+"  .bookingInfo").val();
    //
    //
    //             formFlag1++;
    //
    //             if(formFlag1 == 1)
    //             {
    //                  $("#"+link_id).after('<div id="reserve-form"><div><label for="available"><input type="radio" name="reserve" value="available" id ="available">Available</label><label for="reserve"><input type="radio" name="reserve" value="reserved" id="reserved">Reserve</label></div><div><label for="reserve">Reserved to</label><input type="text" name="reserveTo" id="reserveTo" value ="'+reserveTo+'"></div><div><a id="reserveTable" class="button" href="javascript:void(0);">Ok</a></div></div>');
    //                  if(reserveStatus != 'available')
    //                  {
    //                      $("#reserved").attr('checked', 'checked');
    //                  }
    //                  else
    //                  {
    //                       $("#available").attr('checked', 'checked');
    //                  }
    //
    //             }
    //             else
    //             {
    //                   $("#reserve-form").remove();
    //                   formFlag1 = 0;
    //             }
    //     });

    //     $("#reserveTable").live('click', function(){
    //
    //            var reserveStatus = $("input:radio[name = reserve]:checked").val();
    //            var reserveTo = $("#reserveTo").val();
    //            var table_id = $("#table_id").val();
    //
    //            $("#"+table_id+"  .bookingInfo").attr('title', reserveTo)
    //            $("#"+table_id+"  .bookingInfo").val(reserveStatus);
    //
    //
    //            $("#form_info").html('');
    //     });



    //***************  Design Preview Section Start *********************//

    $("#selectDesign").change(function () {  //******  Open Design ***//
        var design_id = $(this).val();
        $("#design-tableProperty").html('');
        $("#bookingDate").val('');

        $.post("../main/open", { 'design_id': design_id },
            function (data) {
                $("#floor1").html(data.content);

                $("#floor1 .lblCircleEdit, #floor1 .editTransform, #floor1 .rotate, #floor1 .transformable-handle-scale, #floor1 .transformable-handle-rotate").remove();
                $(".lblTable").after('<div class="booking-status available"></div>');
            }, "json");
    });


    $("#floor1 .table, #floor1 .seat").live('click', function () {
        var table_id = $(this).attr('id');
        var reserveTo = $("#" + table_id + "  .bookingInfo").attr('title');
        var ynConfirm = $("#" + table_id + "  .bookingInfo").attr('alt');

        var reserveStatus = $("#" + table_id + "  .bookingInfo").val();
        if (reserveStatus != 'reserved') {
            $("#design-tableProperty").html('<div id="reserve-form"><div class="title">Reserve Information</div><div><label>Name: </label><span>' + table_id + '</span></div><input type="hidden" id="table_id" name="table_id" value="' + table_id + '" /><div><label for="reserveDate"> </label></div><div><label for="available"><input type="radio" name="reserve" value="available" id ="available">Available</label><label for="reserve"><input type="radio" name="reserve" value="reserved" id="reserved">Reserve</label></div><div><label for="reserveto">Reserved to</label><input type="text" name="reserveTo" id="reserveTo" value ="' + reserveTo + '"></div><div><label for="confirm"><input type="radio" name="ynConfirm" value="confirm" id ="confirm" checked>Confirm</label><label for="notConfirm"><input type="radio" name="ynConfirm" value="notconfirm" id="notConfirm">Not Confirm</label></div><div><a id="reserveTable" class="button" href="javascript:void(0);">Ok</a></div></div>');
        }
        else {
            $("#design-tableProperty").html('<div id="reserve-form"><div class="title">Reserve Information</div><div><span>' + table_id + ' is Reserved</span></div></div>');
        }

        if (reserveStatus != 'available') {
            $("#reserved").attr('checked', 'checked');
        }
        else {
            $("#available").attr('checked', 'checked');
        }

        if (ynConfirm == 'confirm') {
            $("#confirm").attr('checked', 'checked');
        }
        else if (ynConfirm == 'notconfirm') {
            $("#notConfirm").attr('checked', 'checked');
        }

    });


    $("#reserveTable").live('click', function () {

        var reserveStatus = $("input:radio[name = reserve]:checked").val();
        var reserveTo = $("#reserveTo").val();
        var table_id = $("#table_id").val();
        var ynConfirm = $("input:radio[name = ynConfirm]:checked").val();
        var reserveId = $("#reserveId").val();
        var tableClassAll = $("#" + table_id).attr('class');
        var as = tableClassAll.split(" ", 2);
        var seatPos = as[1].split("-");
        var seatType = seatPos[1];
        var a = $("#floor1 #" + table_id).hasClass('seat');

        if (reserveStatus == 'reserved') {
            if (ynConfirm == 'confirm') { $("#floor1 #" + table_id + "  .booking-status").removeClass('available notConfirm reserved').addClass('reserved'); }
            if (ynConfirm == 'notconfirm') { $("#floor1 #" + table_id + "  .booking-status").removeClass('available notConfirm reserved').addClass('notConfirm'); }
            if (a) {
                $("#floor1 #" + table_id).removeClass('seat-bottom').addClass('bottom-red')
            }
        }
        else {
            $("#" + table_id + "  .booking-status").removeClass('available notConfirm reserved').addClass('available');
            reserveTo = 'none';
        }

        $("#" + table_id + "  .bookingInfo").attr('title', reserveTo);
        $("#" + table_id + "  .bookingInfo").attr('alt', ynConfirm);
        $("#" + table_id + "  .bookingInfo").val(reserveStatus);

        $.post("../main/save_reserveinfo", { 'reserveId': reserveId, 'table_id': table_id, 'reserveTo': reserveTo, 'ynConfirm': ynConfirm },   //saving reserve information
            function (data) {
                alert(data);
            }, "json");

        $("#design-tableProperty").html('');
    });


    $("#bookingDate").live('change', function () {
        var bookingDate = $(this).val();
        var design_id = $("#selectDesign").val();
        $("#design-tableProperty").html('');

        $.post("../main/get_reserveid", { 'design_id': design_id, 'bookingDate': bookingDate },
           function (data) {

               $("#reserveId").val(data);
               var reserveId = data;

               $("#floor1 .table").each(function () {
                   var table_id = $(this).attr('id');

                   $.post("../main/get_seatinfo", { 'reserveId': reserveId, 'table_id': table_id },
                                     function (data) {

                                         if (data.found == 1) {
                                             if (data.ynConfirm == 1) {
                                                 $("#floor1 #" + table_id + " .booking-status").removeClass('available notConfirm reserved').addClass('reserved');
                                                 $("#floor1 #" + table_id + " .bookingInfo").attr('alt', 'confirm');
                                             }
                                             else if (data.ynConfirm == 0) {
                                                 $("#floor1 #" + table_id + " .booking-status").removeClass('available notConfirm reserved').addClass('notConfirm');
                                                 $("#floor1 #" + table_id + " .bookingInfo").attr('alt', 'notconfirm');
                                             }


                                             $("#floor1 #" + table_id + " .bookingInfo").attr('title', data.bookedBy);
                                             $("#floor1 #" + table_id + " .bookingInfo").val('reserved');
                                         }
                                         else if (data.found == 0) {
                                             $("#floor1 #" + table_id + " .booking-status").removeClass('available reserved notConfirm').addClass('available');
                                         }

                                     }, "json");
               });

           }, "json");



    });




});