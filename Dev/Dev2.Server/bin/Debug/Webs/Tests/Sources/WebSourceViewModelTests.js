///// <reference path="../../wwwroot/Scripts/_references.js" />
///// <reference path="../../wwwroot/Scripts/Sources/WebSourceViewModel.js" />
///// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

//module("Model Passes Validation");

//test("IsFormValidWithBareMinimumHttpAddressExpectedPassesValidation", function () {

//    var model = new WebSourceViewModel();
//    model.data.address("http:/");
//    ok(model.isFormValid(), "Valid web address was declared invalid by web source model");
//});

//test("IsFormValidWithBareMinimumFtpAddressExpectedPassesValidation", function () {

//    var model = new WebSourceViewModel();
//    model.data.address("ftp:/");
//    ok(model.isFormValid(), "Valid web address was declared invalid by web source model");
//});

//module("Model Fails Validation");

//test("FormValidationWithBlankAddressExpectedFailsValidation", function () {

//    var model = new WebSourceViewModel();
//    model.data.address("");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("FormValidationWithInvalidAddressExpectedFailsValidation", function () {

//    var model = new WebSourceViewModel();
//    model.data.address("http::/www.google.co.za/");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

///*
//test("CanUserViewResultsInBrowser", function () {

//    var model = new WebSourceViewModel();
//    model.data.address("http::/www.google.co.za/");
//    ok(!model.viewInBrowser(), "Did Form Fail Validation");
//});
//*/