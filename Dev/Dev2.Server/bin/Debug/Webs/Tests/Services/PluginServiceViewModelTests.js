/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Services/ServiceData.js" />
/// <reference path="../../wwwroot/Scripts/Services/PluginServiceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Sources/PluginSourceViewModel.js" />
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

module("Plugin Service Model Constructor");

test("ConstructorWithInvalidResourcedIDExpectedIsEditingIsFalse", function () {

    var model = new PluginServiceViewModel("pluginServiceSaveDialogContainer", "xxxx");
    ok(!model.isEditing, "Is IsEditing False");
});

test("ConstructorWithValidResourcedIDExpectedIsEditingIsTrue", function () {

    var model = new PluginServiceViewModel("pluginServiceSaveDialogContainer", "{97A9EFED-4127-4421-BCE8-1AC90CAFB7D4}");
    ok(model.isEditing, "Is IsEditing True");
});

test("ConstructorWithInvalidResourcedIDExpectedTitleContainsNew", function () {

    var model = new PluginServiceViewModel("pluginServiceSaveDialogContainer", "xxxx");
    ok(model.title().indexOf("New") != -1, "Does Title Contain New");
});

test("ConstructorExpectedModelResourceTypeIsDbService", function () {

    var model = new PluginServiceViewModel();
    equal(model.data.resourceType(), "PluginService", "Is Resource Type PluginService");
});

module("Plugin Service Model Form Validation");

test("FormValidationWithIsNotEdittingAndMethodNameChangedAndHasTestResultsAndOneRecordSetExpectedFormIsValid", function () {

    var model = new PluginServiceViewModel();
    model.isEditing = false;
    model.methodNameChanged(true);
    model.hasTestResults(true);
    model.data.recordset.Records(["testvar"]);
    ok(model.isFormValid(), "Is Form Valid" );
});

test("FormValidationWithIsEdittingAndMethodNameChangedAndHasTestResultsAndOneRecordSetExpectedFormIsValid", function () {

    var model = new PluginServiceViewModel();
    model.isEditing = true;
    model.methodNameChanged(true);
    model.hasTestResults(true);
    model.data.recordset.Records(["testvar"]);
    ok(model.isFormValid(), "Is Form Valid");
});

test("FormValidationWithIsEdittingAndMethodNameNotChangedAndOneRecordSetExpectedFormIsValid", function () {

    var model = new PluginServiceViewModel();
    model.isEditing = true;
    model.hasTestResults(true);
    model.data.recordset.Records(["testvar"]);
    ok(model.isFormValid(), "Is Form Valid");
});

test("IsFormValidWithoutTestResultsExpectedFormIsNotValid", function () {

    var model = new PluginServiceViewModel();
    model.isEditing = false;
    model.hasTestResults(false);
    model.data.recordset.Records(["testvar"]);
    ok(!model.isFormValid(), "Is Form Not Valid");
});

test("MethodNameSubscriberWithChangingMethodNameExpectedSubscriberChangesMethodName", function () {

    var model = new PluginServiceViewModel();
    model.data.method.Name("change method name");
    ok(model.methodNameChanged(), "Did Subscriber Change Method Name");
});

test("IsFormValidWithIsEdittingAndMethodNameChangedAndHasNoTestResultsExpectedFormIsNotValid", function () {

    var model = new PluginServiceViewModel();
    model.isEditing = true;
    model.methodNameChanged(true);
    model.data.recordset.Records(["testvar"]);
    ok(!model.isFormValid(), "Is Form Not Valid");
});

test("MethodSearchResultsWithoutTermExpectedMethodsReturned", function() {

    var model = new PluginServiceViewModel();
    model.sourceMethods(["test method", "another test method"]);
    ok(model.sourceMethodSearchResults()[1], "Did Methods Return");
});

test("SourceNameSubscriberWithChangingSourceNameExpectedSubscriberResetsMethodName", function () {
    
    var model = new PluginServiceViewModel();
    model.data.method.Name("new method name");
    model.data.source("change source name");
    equal(model.data.method.Name(), "", "Did Methods Return");
});

module("Plugin Service Model to Save Model Binding");

test("SaveDialogConstructorExpectedResourceTypeIsDbService", function () {

    var model = new PluginServiceViewModel();
    equal(model.saveViewModel.data.resourceType(), 'PluginService', "Is Resource Type Plugin Service");
});

test("ChangeServiceModelResourceIDExpectedSaveModelResourceIDChanged", function () {

    var model = new PluginServiceViewModel();
    var resourceId = $.Guid.New();
    model.data.resourceID(resourceId);
    equal(model.saveViewModel.data.resourceID(), resourceId, "Did SaveModel Resource ID Change");
});

test("ChangeServiceModelResourceNameExpectedSaveModelResourceNameChanged", function () {

    var model = new PluginServiceViewModel();
    var resourceName = "new database service";
    model.data.resourceName(resourceName);
    equal(model.saveViewModel.data.resourceName(), resourceName, "Did SaveModel Resource Name Change");
});

test("ChangeServiceModelResourcePathExpectedSaveModelResourcePathChanged", function () {

    var model = new PluginServiceViewModel();
    var resourcePath = "new resource path";
    model.data.resourcePath(resourcePath);
    equal(model.saveViewModel.data.resourcePath(), resourcePath, "Did SaveModel Resource Path Change");
});

test("ChangeServiceModelSourceExpectedSaveModelSourceChanged", function () {

    var model = new PluginServiceViewModel();
    var source = "new source";
    model.data.source(source);
    equal(model.saveViewModel.data.source(), source, "Did SaveModel Source Name Change");
});

test("ChangeServiceModelMethodExpectedSaveModelMethodDataChanged", function () {

    var model = new PluginServiceViewModel();
    
    var methodName = "change method name";
    model.data.method.Name(methodName);
    equal(model.saveViewModel.data.method.Name(), methodName, "Did Save Model Method Name Change");
    
    var methodSourceCode = "change method source code";
    model.data.method.SourceCode(methodSourceCode);
    equal(model.saveViewModel.data.method.SourceCode(), methodSourceCode, "Did Save Model Method Source Code Change");
    
    var methodParameters = "change method param";
    model.data.method.Parameters(methodParameters);
    equal(model.saveViewModel.data.method.Parameters(), methodParameters, "Did Save Model Method Parameter Change");
    });
    
test("ChangeServiceModelRecordsetExpectedSaveModelRecordsetDataChanged", function () {

    var model = new PluginServiceViewModel();
    
    var recordsetName = "change recordset name";
    model.data.recordset.Name(recordsetName);
    equal(model.saveViewModel.data.recordset.Name(), recordsetName, "Did Save Model Recordset Name Change");

    var recordsetFields = ["change recordset field", "another recordset field"];
    model.data.recordset.Fields(recordsetFields);
    equal(model.saveViewModel.data.recordset.Fields(), recordsetFields, "Did Save Model Recordset Fields Change");

    var recordsetRecords = ["change recordset record", "a second recordset record"];
    model.data.recordset.Records(recordsetRecords);
    deepEqual(model.saveViewModel.data.recordset.Records(), recordsetRecords, "Did Save Model Recordset Records Change");

    model.data.recordset.HasErrors(true);
    equal(model.saveViewModel.data.recordset.HasErrors(), true, "Did Save Model Recordset HasError Change");

    var errorMessage = "change recordset error";
    model.data.recordset.ErrorMessage(errorMessage);
    equal(model.saveViewModel.data.recordset.ErrorMessage(), errorMessage, "Did Save Model Recordset Error Message Change");
});

module("Plugin Service Model to Plugin Source Model Binding");

[ignore]
test("SaveDialogConstructorExpectedResourceTypeIsDbService", function () {

    var model = new PluginServiceViewModel();
    
    model.source.data = new PluginSourceViewModel().data;
    equal(model.source.data.resourceType(), 'PluginSource', "Is Resource Type Plugin Source");
});