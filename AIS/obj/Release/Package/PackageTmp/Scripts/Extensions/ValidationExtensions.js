
//Custom jQuery validation unobtrusive script and adapters
jQuery.validator.unobtrusive.adapters.add("checkboxtrue", function (options) {
    if (options.element.tagName.toUpperCase() == "INPUT" && options.element.type.toUpperCase() == "CHECKBOX") {
        options.rules["required"] = true;
        if (options.message) {
            options.messages["required"] = options.message;
        }
    }
});

//$(function () {
//    if ($(".selectpicker")[0]) {
//        $(".selectpicker").selectpicker();
//        $('.selectpicker').parents('form:first').validate().settings.ignore = ':not(select:hidden, input:visible, textarea:visible)';
//    }
//});