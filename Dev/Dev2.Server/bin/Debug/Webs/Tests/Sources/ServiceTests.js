/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Sources/Service.js" />

module("Source Tests");

test("SerivceViewModel_With_InvalidResourcedID_Expected_IsEditingIsFalse", function () {

    var model = new SerivceViewModel("xxxx");
    ok(!model.isEditing);
});

test("SerivceViewModel_With_ValidResourcedID_Expected_IsEditingIsTrue", function () {

    var model = new SerivceViewModel("{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}");
    ok(model.isEditing);
});

