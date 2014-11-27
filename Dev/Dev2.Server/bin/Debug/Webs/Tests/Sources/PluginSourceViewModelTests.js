/// <reference path="../../wwwroot/Scripts/Sources/PluginSourceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Services/PluginServiceViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

/// <reference path="../../wwwroot/Scripts/fx/jquery-1.9.1.min.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.Guid.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.mockjax.js" />
/// <reference path="../../wwwroot/Scripts/fx/knockout-2.2.1.js" />
/// <reference path="../../wwwroot/Scripts/fx/qunit-1.11.0.js" />

/// <reference path="../../wwwroot/Scripts/warewolf-globals.js" />
/// <reference path="../../wwwroot/Scripts/warewolf-utils.js" />

//module("PluginSource Model Constructor");

/*
test("ConstructorWithNoParametersExpectedIsEditingIsFalse", function () {

    var model = new PluginSourceViewModel();
    equal(model.title(), "New Plugin Source", "Did Title Initialize");
});

test("ConstructorWithNoParametersAndNewResourceNameExpectedResourceNameChanged", function() {

    var pluginSourceModel = new PluginSourceViewModel();
    pluginSourceModel.data.resourceName("new resource name");
    equal(pluginSourceModel.data.resourceName(), "new resource name", "Resource Name Changed");
});

test("ConstructorWithNoParametersExpectedHelpDictionaryIDIsPluginSource", function () {

    var model = new PluginSourceViewModel();
    equal(model.helpDictionaryID, "pluginSource", "Did Help Dictionary ID Initialize");
});
*/

//module("PluginSource Form Passes Validation");

/*
self.pluginSourceWithLoadedGacList = function() {

    var model = new PluginSourceViewModel();
    model.allGacAssemblies($.parseJSON("[{ \"Text\": \"Test.Assembly 6.1.0.0\" }, { \"Text\": \"Another.Test.Assembly 2.0.0.0\" }, { \"Text\": \"More.Testing.Assemblies 2.0.0.0\" }, { \"Text\": \"Assemblies.Test 6.1.0.0\" }, { \"Text\": \"Assemblies.Need.Tests 6.1.0.0\" }, { \"Text\": \"Test.Gac.List 6.1.0.0\" }, { \"Text\": \"Microsoft-Windows-HomeGroupDiagnostic.NetListMgr.Interop 6.1.0.0\" }]"));
    return model;
};

test("IsFormValidWithGacAssemblyAndIsInGacListExpectedFormIsValid", function () {

    var model = pluginSourceWithLoadedGacList();
    model.data.assemblyLocation("GAC:Test.Assembly 6.1.0.0");
    ok(model.isFormValid(), "Did Form Pass Validation");
});
*/

/*
test("IsFormValidWithAssemblyFileAndVerifiedAssemblyFileExpectedFormIsValid", function () {

    var model = new PluginSourceViewModel();
    model.data.assemblyLocation("set dll location");
    model.isAssemblyFileValid(true);
    ok(model.isFormValid(), "Did Form Pass Validation");
});
*/

//module("PluginSource Form Fails Validation");

/*
test("IsFormValidWithNoAssemblyLocationExpectedFormIsNotValid", function() {

    var model = new PluginSourceViewModel();
    ok(!model.isFormValid(), "Did Form Fail Validation");
});


test("IsFormValidWithBadAssemblyLocationExpectedFormIsNotValid", function () {

    var model = new PluginSourceViewModel();
    model.data.assemblyLocation("C:\\Sierra\\Half-Life\\hl.dll");
    ok(!model.isFormValid(), "Did Form Fail Validation");
});

test("IsFormValidWithGacAssemblyAndIsNotInGacListExpectedFormIsNotValid", function () {

    var model = pluginSourceWithLoadedGacList();
    model.data.assemblyLocation("GAC:Not in Gac List 0.0.0.0");
    ok(!model.isFormValid(), "Did Form Fail Validation");
});

module("PluginSource to Save Data Model Binding");

/*
test("SaveDialogConstructorExpectedResourceTypeIsPluginSource", function () {

    var model = new PluginSourceViewModel();
    equal(model.saveViewModel.data.resourceType(), 'PluginSource', "Is Resource Type Plugin Source");
});
*/

test("ChangeResourceIDExpectedSaveModelResourceIDChanged", function () {

    var model = new PluginSourceViewModel();
    model.data.resourceID($.Guid.New());
    equal(model.saveViewModel.data.resourceID(), model.data.resourceID(), "Did Save Model Resource ID Change");
});

test("ChangeResourceNameExpectedSaveModelResourceNameChanged", function () {

    var model = new PluginSourceViewModel();
    model.data.resourceName("change resource name");
    equal(model.saveViewModel.data.resourceName(), model.data.resourceName(), "Did Save Model Resource Name Change");
});

test("ChangeResourcePathExpectedSaveModelResourcePathChanged", function () {

    var model = new PluginSourceViewModel();
    model.data.resourcePath("change resource path");
    equal(model.saveViewModel.data.resourcePath(), model.data.resourcePath(), "Did Save Model Resource Path Change");
});

module("Update Help Text");

self.pluginSourceWithLoadedHelpDic = function () {

    var model = new PluginSourceViewModel();
    model.helpDictionary = $.parseJSON("{\"default\":\"<h4>Plugin File</h4><p>Select a Dll file to connect to</p>\",\"pluginAssemblyFileLocation\":\"Enter the plugin <b>file address.</b> e.g.\",\"pluginAssemblyGACLocation\":\"Enter the plugin <b>assembly name</b> starting with 'GAC:' and followed by the verion number e.g. 'GAC:Microsoft.Email.Client.Library 2.0.0.0'\",\"tab 0\":\"<h4>Plugin File</h4><p>Select a Dll file to connect to</p>\",\"tab 1\":\"<h4>Global Cache</h4><p>Select an assembly from the <b>Global Assemblies Cache</b></p>\",\"GACList\":\"<h4>Global Cache</h4><p>Select an assembly from the <b>Global Assemblies Cache</b></p>\",\"gacSearchTerm\":\"<h4>Global Cache</h4><p>You are viewing all assemblies</p>\"}");
    return model;
};

test("UpdateHelpTextWithDefaultExpectedHelpTextSet", function () {

    var loadedModel = self.pluginSourceWithLoadedHelpDic();
    loadedModel.updateHelpText("default");
    equal(loadedModel.helpText(), "<h4>Plugin File</h4><p>Select a Dll file to connect to</p>", "Is Default Help Text Set");
});

test("UpdateHelpTextWithFileTextBoxExpectedFileTextBoxHelpTextSet", function () {

    var loadedModel = self.pluginSourceWithLoadedHelpDic();
    loadedModel.updateHelpText("pluginAssemblyFileLocation");
    equal(loadedModel.helpText(), "Enter the plugin <b>file address.</b> e.g.", "Is File Textbox Help Text Set");
});

test("UpdateHelpTextWithGACTextBoxExpectedGACTextBoxHelpTextSet", function () {

    var loadedModel = self.pluginSourceWithLoadedHelpDic();
    loadedModel.updateHelpText("pluginAssemblyGACLocation");
    equal(loadedModel.helpText(), "Enter the plugin <b>assembly name</b> starting with 'GAC:' and followed by the verion number e.g. 'GAC:Microsoft.Email.Client.Library 2.0.0.0'", "Is GAC Textbox Help Text Set");
});

test("UpdateHelpTextWithFileTabExpectedDefaultFileHelpTextSet", function () {

    var loadedModel = self.pluginSourceWithLoadedHelpDic();
    loadedModel.updateHelpText("tab 0");
    equal(loadedModel.helpText(), "<h4>Plugin File</h4><p>Select a Dll file to connect to</p>", "Is Default File Help Text Set");
});

test("UpdateHelpTextWithGACTabExpectedDefaultGACHelpTextSet", function () {

    var loadedModel = self.pluginSourceWithLoadedHelpDic();
    loadedModel.updateHelpText("tab 1");
    equal(loadedModel.helpText(), "<h4>Global Cache</h4><p>Select an assembly from the <b>Global Assemblies Cache</b></p>", "Is Default GAC Help Text Set");
});

test("UpdateHelpTextWithGACListExpectedGACListHelpTextSet", function () {

    var loadedModel = self.pluginSourceWithLoadedHelpDic();
    loadedModel.updateHelpText("GACList");
    equal(loadedModel.helpText(), "<h4>Global Cache</h4><p>Select an assembly from the <b>Global Assemblies Cache</b></p>", "Is Default GAC Help Text Set When Update help text is passed 'GACList'");
});

test("UpdateHelpTextWithGACListExpectedGACListHelpTextSet", function () {

    var loadedModel = self.pluginSourceWithLoadedHelpDic();
    loadedModel.updateHelpText("gacSearchTerm");
    equal(loadedModel.helpText(), "<h4>Global Cache</h4><p>You are viewing all assemblies</p>", "Is Search Term Help Text Set");
});

//self.pluginSourceWithLoadedFileData = function() {

//    var model = new PluginSourceViewModel();
//    model.data = $.parseJSON("{\"AssemblyLocation\":\"C:\\Development\\DEV2 SCRUM Project\\Branches\\Ashley.Lewis-PBI8721\\Dev2.Server\\bin\\Debug\\Plugins\\Dev2.AnytingToXmlHook.Plugin.dll\",\"AssemblyName\":\"Dev2.AnytingToXmlHook.Plugin\",\"FullName\":null,\"ResourceID\":\"2f93aa19-d507-4ed0-9b7e-a8b1b07ce12f\",\"Version\":\"1.0\",\"ResourceType\":\"PluginSource\",\"ResourceName\":\"Anything To Xml Hook Plugin\",\"ResourcePath\":\"Conversion\"}");
//    return model;
//};
