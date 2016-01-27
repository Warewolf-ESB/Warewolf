/// <reference path="../../wwwroot/Scripts/Sources/WebSourceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

/// <reference path="../../wwwroot/Scripts/fx/jquery-1.9.1.min.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.Guid.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.mockjax.js" />
/// <reference path="../../wwwroot/Scripts/fx/knockout-2.2.1.js" />
/// <reference path="../../wwwroot/Scripts/fx/qunit-1.11.0.js" />

/// <reference path="../../wwwroot/Scripts/warewolf-globals.js" />
/// <reference path="../../wwwroot/Scripts/warewolf-utils.js" />

module("Model Passes Validation");

test("IsFormValidWithBareMinimumHttpAddressExpectedPassesValidation", function () {

    var model = new WebSourceViewModel();
    model.data.address("http:/");
    ok(model.isFormValid(), "Valid web address was declared invalid by web source model");
});

test("IsFormValidWithBareMinimumFtpAddressExpectedPassesValidation", function () {

    var model = new WebSourceViewModel();
    model.data.address("ftp:/");
    ok(model.isFormValid(), "Valid web address was declared invalid by web source model");
});

module("Model Fails Validation");

test("FormValidationWithBlankAddressExpectedFailsValidation", function () {

    var model = new WebSourceViewModel();
    model.data.address("");
    ok(!model.isFormValid(), "Did Form Fail Validation");
});

test("FormValidationWithInvalidAddressExpectedFailsValidation", function () {

    var model = new WebSourceViewModel();
    model.data.address("http::/www.google.co.za/");
    ok(!model.isFormValid(), "Did Form Fail Validation");
});

/*
Ashley: I removed address validation because it is not possible. There is no standard format that satisfies all web addresses.
test("CanUserViewResultsInBrowser", function () {

    var model = new WebSourceViewModel();
    model.data.address("http::/www.google.co.za/");
    ok(!model.viewInBrowser(), "Did Form Fail Validation");
});
*/