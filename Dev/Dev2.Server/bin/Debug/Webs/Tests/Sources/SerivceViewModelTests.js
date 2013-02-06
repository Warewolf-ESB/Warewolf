/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Sources/ServiceViewModel.js" />

module("SerivceViewModelTests");

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
    equal($.Guid.Empty(), model.data.resourceID());
});

test("Constructor_With_ValidResourcedID_Expected_DataResourceIDIsTheGivenResourceID", function () {

    var expectedID = "{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}";
    var model = new SerivceViewModel(expectedID);
    equal(expectedID, model.data.resourceID());
});

test("Constructor_With_ResourcedType_Expected_DataResourceTypeIsTheGivenResourceType", function () {

    var expectedType = "SqlDatabase";
    var model = new SerivceViewModel(null, expectedType);
    equal(expectedType, model.data.resourceType());
});

test("Constructor_With_ResourcedType_Expected_DataResourceTypeIsTheGivenResourceType", function () {

    var expectedType = "SqlDatabase";
    var model = new SerivceViewModel(null, expectedType);
    equal(expectedType, model.data.resourceType());
});

asyncTest("Load_Expected_AjaxPostToServicesGet", function () {
    expect(1);

    $.mockjax({
        url: "Service/Services/Get" + window.location.search,
        type: "POST",
        responseTime: 750,
        response: function () {
            this.responseText = {
                ResourceID: "{8A49D826-0B3E-4DB5-8D01-490A75A1B698}",
                ResourceType: "SqlDatabase",
                ResourceName: "My Database",
                ResourcePath: "My Category"
            };
            ok(true, "Invoked Service/Services/Get");
            start();
        }
    });
    
    var expectedID = "{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}";
    var model = new SerivceViewModel(expectedID);
    model.load();

});


