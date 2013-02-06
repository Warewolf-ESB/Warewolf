/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Sources/ServiceViewModel.js" />

module("SerivceViewModelTests", {
    setup: function() {
        $.mockjaxSettings = {
            contentType: "text/json",
            dataType: "json"
        };
    },
    teardown: function () {
        $.mockjaxClear();
    }
});


test("Constructor_With_InvalidResourcedID_Expected_IsEditingIsFalse", function () {

    var model = new SerivceViewModel("xxxx");
    ok(!model.isEditing);
});

test("Constructor_With_ValidResourcedID_Expected_IsEditingIsTrue", function () {

    var model = new SerivceViewModel("{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}");
    ok(model.isEditing);
});

test("Constructor_With_InvalidResourcedID_Expected_TitleContainsNew", function () {

    var model = new SerivceViewModel("xxxx");
    ok(model.title().indexOf("New") != -1);
});

test("Constructor_With_ValidResourcedID_Expected_TitleContainsEdit", function () {

    var model = new SerivceViewModel("{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}");
    ok(model.title().indexOf("Edit") != -1);
});

test("Constructor_With_InvalidResourcedID_Expected_DataResourceIDIsEmptyGuid", function () {

    var model = new SerivceViewModel("xxxx");
    equal(model.data.resourceID(), $.Guid.Empty());
});

test("Constructor_With_ValidResourcedID_Expected_DataResourceIDIsTheGivenResourceID", function () {

    var expectedID = "{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}";
    var model = new SerivceViewModel(expectedID);
    equal(model.data.resourceID(), expectedID);
});

test("Constructor_With_ResourcedType_Expected_DataResourceTypeIsTheGivenResourceType", function () {

    var expectedType = "SqlDatabase";
    var model = new SerivceViewModel(null, expectedType);
    equal(model.data.resourceType(), expectedType);
});

test("Constructor_With_ResourcedType_Expected_DataResourceTypeIsTheGivenResourceType", function () {

    var expectedType = "SqlDatabase";
    var model = new SerivceViewModel(null, expectedType);
    equal(model.data.resourceType(), expectedType);
});

asyncTest("Load_Expected_AjaxPostToServicesGetMethod", 1, function () {
    $.mockjax({
        url: "Service/Services/Get",
        type: "POST",
        response: function () {
            ok(true);
            start();
        }
    });
    
    var model = new SerivceViewModel();
    model.load();

});

//asyncTest("Load_Expected_UpdatesData", 1, function () {
//    var expectedRepsonse = {
//        ResourceID: "{8A49D826-0B3E-4DB5-8D01-490A75A1B698}",
//        ResourceType: "SqlDatabase",
//        ResourceName: "My Database",
//        ResourcePath: "My Category"
//    };
    
//    $.mockjax({
//        url: "Service/Services/Get",// + window.location.search,
//        type: "POST",
//        response: function () {
//            this.responseText = expectedRepsonse;
//            start();
//        }
//    });

//    var model = new SerivceViewModel();
//    model.load(function() {
//        equal(model.data.resourceID(), expectedRepsonse.ResourceID, "model.data.resourceID set");
//        equal(model.data.resourceType(), expectedRepsonse.ResourceType, "model.data.resourceType set");
//        equal(model.data.resourceName(), expectedRepsonse.ResourceName, "model.data.resourceName set");
//        equal(model.data.resourcePath(), expectedRepsonse.ResourcePath, "model.data.resourcePath set");
//    });

//});

