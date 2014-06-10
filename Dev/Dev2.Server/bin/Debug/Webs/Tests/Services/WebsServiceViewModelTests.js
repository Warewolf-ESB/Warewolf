/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Services/ServiceData.js" />
/// <reference path="../../wwwroot/Scripts/Services/WebServiceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Sources/WebSourceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

//module("ServiceViewModelTests", {
//    setup: function() {
//        $.mockjaxSettings = {
//            contentType: "text/json",
//            dataType: "json"
//        };
//    },
//    teardown: function () {
//        $.mockjaxClear();
//    }
//});

module("Constructor Tests");

test("ConstructorExpectedModelResourceWebServiceOkDisabledOnLoad", function () {

    var model = new WebServiceViewModel();
    ok(!model.isFormValid(), "WebService", "Ok Enabled By Default");
});