/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />
/// <reference path="../../wwwroot/Scripts/Services/ServiceData.js" />

module("Computed Bindings");

test("AttemptedResourceNameGetsResourceNameValueCorrectly", function () {

    var baseviewmodel = {};
    baseviewmodel.data = new ServiceData("test resource", "Workflow");
    var model = new SaveViewModel("", baseviewmodel, "test form", "");
    model.data.resourceName("test resource name");
    equal(model.attemptedResourceName(), 'test resource name', "The value displayed to the user for resource name cannot be resolved");
});

test("AttemptedResourceNameSetsResourceNameValueCorrectly", function () {

    var baseviewmodel = {};
    baseviewmodel.data = new ServiceData("test resource", "Workflow");
    var model = new SaveViewModel("", baseviewmodel, "test form", "");
    model.attemptedResourceName("[invalid\ test/ resource; name].");
    equal(model.data.resourceName(), 'invalid test resource name.', "The value displayed to the user for resource name cannot be resolved");
});

module("Handle 'Unassigned' folder");

test("RemoveUnassignedFolderThatDoesntExist", function () {

    var baseviewmodel = {};
    baseviewmodel.data = new ServiceData("test resource", "Workflow");
    var model = new SaveViewModel("", baseviewmodel, "test save form", "");
    model.resourceFolders(["First Category", "Second Category"]);
    //Execute Remove
    model.RemoveUnassignedFolder();
    equal(model.resourceFolders().length, 2, "HandleUnassignedFolder corrupted resourceFolders");
    notEqual(model.resourceFolders()[0], model.defaultFolderName, "Has ResourceFolders list changed at all");
    notEqual(model.resourceFolders()[1], model.defaultFolderName, "Has ResourceFolders list changed at all");
});

test("RemoveUnassignedFolderThatDoesExist", function () {

    var baseviewmodel = {};
    baseviewmodel.data = new ServiceData("test resource", "Workflow");
    var model = new SaveViewModel("", baseviewmodel, "test save form", "");
    model.resourceFolders(["First Category", "Second Category", "Unassigned", "UNASSIGNED"]);
    //Execute Remove
    model.RemoveUnassignedFolder();
    equal(model.resourceFolders().length, 2, "HandleUnassignedFolder corrupted resourceFolders");
    notEqual(model.resourceFolders()[0], model.defaultFolderName, "Has 'Unassigned' folder been removed?");
    notEqual(model.resourceFolders()[1], model.defaultFolderName, "Has 'Unassigned' folder been removed?");
});

test("AddUnassignedFolder", function () {

    var baseviewmodel = {};
    baseviewmodel.data = new ServiceData("test resource", "Workflow");
    var model = new SaveViewModel("", baseviewmodel, "test save form", "");
    model.resourceFolders(["First Category", "Second Category"]);
    //Execute Add
    model.AddUnassignedFolder();
    equal(model.resourceFolders().length, 3, "HandleUnassignedFolder corrupted resourceFolders");
    equal(model.resourceFolders()[0], model.defaultFolderName, "No 'Unassigned' folder at the top of the list");
    notEqual(model.resourceFolders()[1], model.defaultFolderName, "No Duplicate 'Unassigned' folder in resourceFolders list");
    notEqual(model.resourceFolders()[2], model.defaultFolderName, "No Duplicate 'Unassigned' folder in resourceFolders list");
});

test("SaveUnassignedResourceDoesNotSaveBlankCategory", function () {

    var baseviewmodel = {};
    baseviewmodel.data = new ServiceData("test resource", "Workflow");
    var model = new SaveViewModel("", baseviewmodel, "test save form", "");
    model.data.resourcePath(model.defaultFolderName);
    model.data.resourceName("Test Resource");
    //Execute Save
    model.save();
    equal(model.data.resourcePath(), "", "Saving unassigned resources should save with blank category");
});