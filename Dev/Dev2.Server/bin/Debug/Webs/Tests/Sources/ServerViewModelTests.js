///// <reference path="../../wwwroot/Scripts/_references.js" />
///// <reference path="../../wwwroot/Scripts/Sources/ServerViewModel.js" />
///// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

//module("Model Initialization");

//test("ConstructorWithNoParametersExpectedResourceTypeInitialized", function() {

//    var model = new ServerViewModel();
//    equal(model.data.resourceType(), "Server", "Resource Type Initialized");
//});

//test("ConstructorWithNoParametersAndNewResourceNameExpectedResourceNameChanged", function () {

//    var serverModel = new ServerViewModel();
//    serverModel.data.resourceName("new resource name");
//    equal(serverModel.data.resourceName(), "new resource name", "Resource Name Changed");
//});

//module("Server Model Form Passes Validation");

//test("AuthentificationTypeSubscriberWithChangingAuthentificationTypeToUserExpectedUserInputMadeVisible", function () {

//    var model = new ServerViewModel();
//    model.data.authenticationType("User");
//    ok(model.isUserInputVisible());
//});

//test("AuthentificationTypeSubscriberWithChangingAuthentificationTypeToWindowsExpectedUserInputMadeInvisible", function () {

//    var model = new ServerViewModel();
//    model.data.authenticationType("Windows");
//    ok(!model.isUserInputVisible());
//});

//test("IsFormValidWithValidUserAuthentificationExpectedFormIsValid", function () {

//    var model = new ServerViewModel();
//    model.data.address("set server address");
//    model.data.authenticationType("User");
//    model.data.userName("user id");
//    model.data.password("password");
//    model.showTestResults(true);
//    ok(model.isFormValid(), "Did Form Pass Validation");
//});

//test("IsFormValidWithWindowsAuthentificationExpectedFormIsValid", function () {

//    var model = new ServerViewModel();
//    model.data.address("set server address");
//    model.data.authenticationType("Windows");
//    model.showTestResults(true);
//    ok(model.isFormValid(), "Did Form Pass Validation");
//});

//module("Server Model Form Fails Validation");

//test("IsFormValidWithUserAuthentificationAndNoPasswordExpectedFormIsNotValid", function () {

//    var model = new ServerViewModel();
//    model.data.address("set server address");
//    model.data.authenticationType("User");
//    model.data.userName("user id");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("IsFormValidWithUserAuthentificationAndNoUsernameExpectedFormIsNotValid", function () {

//    var model = new ServerViewModel();
//    model.data.address("set server address");
//    model.data.authenticationType("User");
//    model.data.password("password");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//test("IsFormValidWithAddressOnlyExpectedFormIsNotValid", function () {

//    var model = new ServerViewModel();
//    model.data.address("set server address only");
//    ok(!model.isFormValid(), "Did Form Fail Validation");
//});

//module("Server to Save Data Model Binding");

//test("SaveDialogConstructorExpectedResourceTypeIsDbSource", function () {

//    var model = new ServerViewModel();
//    equal(model.saveViewModel.data.resourceType(), 'Server', "Is Resource Type Server");
//});

//test("ChangeResourceIDExpectedSaveModelResourceIDChanged", function () {

//    var model = new ServerViewModel();
//    model.data.resourceID($.Guid.New());
//    equal(model.saveViewModel.data.resourceID(), model.data.resourceID(), "Did Save Model Resource ID Change");
//});

//test("ChangeResourceNameExpectedSaveModelResourceNameChanged", function () {

//    var model = new ServerViewModel();
//    model.data.resourceName("change resource name");
//    equal(model.saveViewModel.data.resourceName(), model.data.resourceName(), "Did Save Model Resource Name Change");
//});

//test("ChangeResourcePathExpectedSaveModelResourcePathChanged", function () {

//    var model = new ServerViewModel();
//    model.data.resourcePath("change resource path");
//    equal(model.saveViewModel.data.resourcePath(), model.data.resourcePath(), "Did Save Model Resource Path Change");
//});

//test("ChangeServerAddressExpectedSaveModelAddressChanged", function () {

//    var model = new ServerViewModel();
//    model.data.address("change server name");
//    equal(model.saveViewModel.data.address(), model.data.address(), "Did SaveModel Server Address Change");
//});

//test("ChangeAuthenticationTypeExpectedSaveModelAuthenticationTypeChanged", function () {

//    var model = new ServerViewModel();
//    model.data.authenticationType("change authentication type");
//    equal(model.saveViewModel.data.authenticationType(), model.data.authenticationType(), "Did SaveModel Authentication Type Change");
//});

//test("ChangeUserNameExpectedSaveModeluserIDChanged", function () {

//    var model = new ServerViewModel();
//    model.data.userName("change user name");
//    equal(model.saveViewModel.data.userName(), model.data.userName(), "Did SaveModel User Name Change");
//});

//test("ChangePasswordExpectedSaveModelpasswordChanged", function () {

//    var model = new ServerViewModel();
//    model.data.password("change password");
//    equal(model.saveViewModel.data.password(), model.data.password(), "Did SaveModel Password Change");
//});

//test("ChangePortExpectedSaveModelportChanged", function () {

//    var model = new ServerViewModel();
//    model.data.webServerPort("change port");
//    equal(model.saveViewModel.data.webServerPort(), model.data.webServerPort(), "Did SaveModel Port Change");
//});