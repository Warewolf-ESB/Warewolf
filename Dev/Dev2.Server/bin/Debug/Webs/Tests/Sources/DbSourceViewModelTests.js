/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Sources/DbSourceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Services/DbServiceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

module("DbSource Model Constructor");

test("ConstructorWithNoParametersExpectedIsEditingIsFalse", function () {

    var model = new DbSourceViewModel();
    equal(model.title(), "New Database Source", "Did Title Initialize");
});

//test("ConstructorWithNoParametersAndNewResourceNameExpectedResourceNameChanged", function() {

//    var dbSourceModel = new DbSourceViewModel();
//    dbSourceModel.data.resourceName("new resource name");
//    equal(dbSourceModel.data.resourceName(), "new resource name", "Resource Name Changed");
//});

//module("DbSource Model Form Passes Validation");

//test("AuthentificationTypeSubscriberWithChangingAuthentificationTypeToUserExpectedUserInputMadeVisible", function() {

//    var model = new DbSourceViewModel();
//    model.data.authenticationType("User");
//    ok(model.isUserInputVisible());
//});

//test("IsFormValidWithUserAuthentificationAndDatabaseNameExpectedFormIsValid", function () {

//    var model = new DbSourceViewModel();
//    model.data.server("set server address");
//    model.data.serverType("some server type");
//    model.data.authenticationType("User");
//    model.data.userID("user id");
//    model.data.password("password");
//    model.data.databaseName("database name");
//    ok(model.isFormValid(), "Did Form Pass Validation");
//});

//test("IsFormValidWithWindowsAuthentificationAndDatabaseNameExpectedFormIsValid", function () {

//    var model = new DbSourceViewModel();
//    model.data.server("set server address");
//    model.data.serverType("some server type");
//    model.data.authenticationType("Windows");
//    model.data.databaseName("database name");
//    ok(model.isFormValid(), "Did Form Pass Validation");
//});

//module("DbSource Model Form Fails Validation");

//test("IsFormValidWithUserAuthentificationAndNoDatabaseNameExpectedFormIsNotValid", function() {

//    var model = new DbSourceViewModel();
//    model.data.server("set server address");
//    model.data.serverType("some server type");
//    model.data.authenticationType("User");
//    model.data.userID("user id");
//    model.data.password("password");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("IsFormValidWithUserAuthentificationAndNoPasswordExpectedFormIsNotValid", function() {

//    var model = new DbSourceViewModel();
//    model.data.server("set server address");
//    model.data.serverType("some server type");
//    model.data.authenticationType("User");
//    model.data.userID("user id");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("IsFormValidWithUserAuthentificationAndNoUsernameExpectedFormIsNotValid", function () {

//    var model = new DbSourceViewModel();
//    model.data.server("set server address");
//    model.data.serverType("some server type");
//    model.data.authenticationType("User");
//    model.data.password("password");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("IsFormValidWithWindowsAuthentificationAndNoDatabaseNameExpectedFormIsNotValid", function () {

//    var model = new DbSourceViewModel();
//    model.data.server("set server address");
//    model.data.serverType("some server type");
//    model.data.authenticationType("Windows");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("IsFormValidWithNoServerTypeExpectedFormIsNotValid", function () {

//    var model = new DbSourceViewModel();
//    model.data.server("set server address only");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("IsFormValidWithNoServerAddressExpectedFormIsNotValid", function () {

//    var model = new DbSourceViewModel();
//    model.data.serverType("some server type only");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//module("DbSource to Save Data Model Binding");

//test("SaveDialogConstructorExpectedResourceTypeIsDbSource", function () {

//    var model = new DbSourceViewModel();
//    equal(model.saveViewModel.data.resourceType(), 'DbSource', "Is Resource Type Db Source");
//});

//test("ChangeResourceIDExpectedSaveModelResourceIDChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.resourceID($.Guid.New());
//    equal(model.saveViewModel.data.resourceID(), model.data.resourceID(), "Did Save Model Resource ID Change");
//});

//test("ChangeResourceNameExpectedSaveModelResourceNameChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.resourceName("change resource name");
//    equal(model.saveViewModel.data.resourceName(), model.data.resourceName(), "Did Save Model Resource Name Change");
//});

//test("ChangeResourcePathExpectedSaveModelResourcePathChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.resourcePath("change resource path");
//    equal(model.saveViewModel.data.resourcePath(), model.data.resourcePath(), "Did Save Model Resource Path Change");
//});

//test("ChangeServerExpectedSaveModelServerNameChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.server("change server name");
//    equal(model.saveViewModel.data.server(), model.data.server(), "Did SaveModel Server Name Change");
//});

//test("ChangeServerTypeExpectedSaveModelServerTypeChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.serverType("change server type");
//    equal(model.saveViewModel.data.serverType(), model.data.serverType(), "Did SaveModel Server Type Change");
//});

//test("ChangeDatabaseNameExpectedSaveModelDatabaseNameChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.databaseName("change database name");
//    equal(model.saveViewModel.data.databaseName(), model.data.databaseName(), "Did SaveModel Database Name Change");
//});

//test("ChangeAuthenticationTypeExpectedSaveModelAuthenticationTypeChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.authenticationType("change authentication type");
//    equal(model.saveViewModel.data.authenticationType(), model.data.authenticationType(), "Did SaveModel Authentication Type Change");
//});

//test("ChangeUserNameExpectedSaveModeluserIDChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.userID("change user name");
//    equal(model.saveViewModel.data.userID(), model.data.userID(), "Did SaveModel User Name Change");
//});

//test("ChangePasswordExpectedSaveModelpasswordChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.password("change password");
//    equal(model.saveViewModel.data.password(), model.data.password(), "Did SaveModel Password Change");
//});

//test("ChangePortExpectedSaveModelportChanged", function () {

//    var model = new DbSourceViewModel();
//    model.data.port("change port");
//    equal(model.saveViewModel.data.port(), model.data.port(), "Did SaveModel Port Change");
//});

//module("DbSource Model Cancel Test");

//test("DbSource_RegressionTest_CancelTest_isTestResultsLoadingIsFalseAndLastTestTimeIsSetCorrectly", function () {

//    var model = new DbSourceViewModel();
//    var testTimeTestValue = new Date().valueOf();
//    model.cancelTest();
//    ok(!model.isTestResultsLoading(), "Did Test Results Stop Loading");
//    ok(model.testTime == testTimeTestValue, "Is The Last Test Time Now");
//});

//module("DbSource Model Test Source");

//test("DbSource_RegressionTest_TestFunction_DatabaseNameNotClearedAfterTest", function () {

//    var model = new DbSourceViewModel();
//    model.data.databaseName("Test Database");
//    model.test();
//    ok(model.data.databaseName() == "Test Database", " Did testing a Db Source clear the database selection");
//});