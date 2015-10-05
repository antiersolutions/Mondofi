// global variable for merge table service
var StaffManager = null

$(function () {
    $("#addTabs").tabs();
    //BindStaffEvents();
    StaffManager = new StaffService();
});

function StaffService() {
    // private fields
    var self = this;

    // public fileds

    // public Members
    self.GetStaffList = function () {
        $.ajax({
            url: '/Staff/GetStaffList',
            type: 'GET',
            beforeSend: function () { BeforeGetStaffList(); },
            success: function (data) { $("#Addsection").html(data) },
            complete: function () { BindStaffListEvents(); }
        });
    };

    self.GetStaffSummary = function () {
        $.ajax({
            url: '/Staff/GetStaffSummary',
            type: 'GET',
            beforeSend: function () { BeforeGetStaffSummary(); },
            success: function (data) { $("#Addsummary").html(data) },
            complete: function () { BindStaffListSummary(); }
        });
    };
}

function BindStaffEvents() {
    $("#addTabs").tabs();

    BindStaffListEvents();

}

function BindStaffListEvents() {

    // console.log("start: $('.custom_check').screwDefaultButtons() : " + new Date().getTime());

    var selector = $('input.custom_check');
    $(selector).screwDefaultButtons({
        image: "url(/images/checkbox.png)",
        //unchecked: "url(/images/check-add-table.png)",
        width: 24,
        height: 24
    });

    //console.log("end: $('.custom_check').screwDefaultButtons() : " + new Date().getTime());
    //console.log("start: $('.addTab').click() : " + new Date().getTime());

    /*---toggle ---*/
    $('.addTab').click(function () {
        if ($(this).hasClass('show')) {
            $('.col-assTable, .addTab').removeClass('show');
        }
        else {
            $('.col-assTable, .addTab').removeClass('show');
            $(this).addClass('show');
            $(this).parent().find('.col-assTable').addClass('show');
        }
    });

    //console.log("end: $('.addTab').click() : " + new Date().getTime());
    //console.log("start: $('.selTables').click() : " + new Date().getTime());

    $('.selTables').click(function () {
        $(this).parent().find('.boxSelect').toggleClass('show')
    });

    //console.log("end: $('.selTables').click() : " + new Date().getTime());
    //console.log("start: $('input[name=selectedSectionIds]').on('change') : " + new Date().getTime());

    $('input[name=selectedSectionIds]').on("change", function () {
        if (this.checked) {
            $(this).parents('form').find('input[type=checkbox].sec' + this.value).screwDefaultButtons("check");
        }
        else {
            $(this).parents('form').find('input[type=checkbox].sec' + this.value).screwDefaultButtons("uncheck");
        }
    });

    //console.log("end: $('input[name=selectedSectionIds]').on('change') : " + new Date().getTime());
    //console.log("start: $('input#selectedSectionIdsNone').on('change') : " + new Date().getTime());

    $('input#selectedSectionIdsNone').on("change", function () {
        if (this.checked) {
            $(this).parents('form').find('input[name=selectedSectionIds], input[name=selectedFloorTableIds]').screwDefaultButtons("uncheck").screwDefaultButtons("disable");
        }
        else {
            $(this).parents('form').find('input[name=selectedSectionIds], input[name=selectedFloorTableIds]').screwDefaultButtons("enable");
        }
    });

    //console.log("end: $('input#selectedSectionIdsNone').on('change') : " + new Date().getTime());
    //console.log("start:  $('input#selectedFloorTableIdsNone').on('change') : " + new Date().getTime());

    $('input#selectedFloorTableIdsNone').on("change", function () {
        if (this.checked) {
            $(this).parents('form').find('input[name=selectedSectionIds], input[name=selectedFloorTableIds]').screwDefaultButtons("uncheck").screwDefaultButtons("disable");
        }
        else {
            $(this).parents('form').find('input[name=selectedSectionIds], input[name=selectedFloorTableIds]').screwDefaultButtons("enable");
        }
    });

    //console.log("end: $('input#selectedFloorTableIdsNone').on('change') : " + new Date().getTime());
    //console.log("start: $('#Addsection .content_1').mCustomScrollbar() : " + new Date().getTime());

    (function ($) {
        $(".frnt-right-rowb #tabs-3 .resInfo").css('height', ($(".manage-section").height() - 205));
        $("#Addsection .content_1").mCustomScrollbar({ updateOnContentResize: true });
    })(jQuery);

    //console.log("end: $('#Addsection .content_1').mCustomScrollbar() : " + new Date().getTime());
    //console.log("start: $('.styledCheckbox').attr('title', ''); : " + new Date().getTime());

    $('.styledCheckbox').attr('title', '');

    //console.log("end: $('.styledCheckbox').attr('title', ''); : " + new Date().getTime());
    //$(".server-color-pick").colorpicker({ strings: "Theme Colors,Standard Colors,More Colors,Less Colors" });
}

function BeforeGetStaffList() {
    $("#Addsection .content_1").mCustomScrollbar("destroy");
}

function BeforeGetStaffSummary() {
    $("#Addsummary .content_1").mCustomScrollbar("destroy");
}

function BindStaffListSummary() {
    (function ($) {
        var tabHeight = ($(".manage-section").height() - 238);
        $("#sumScroll").css('max-height', tabHeight);
        $("#Addsummary .content_1").mCustomScrollbar({ updateOnContentResize: true });
    })(jQuery);
}