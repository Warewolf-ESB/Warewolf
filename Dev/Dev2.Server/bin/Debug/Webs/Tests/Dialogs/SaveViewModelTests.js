///<reference path="../../wwwroot/Scripts/_references.js" />
///<reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />
///<reference path="../../wwwroot/Scripts/Services/ServiceData.js" />

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