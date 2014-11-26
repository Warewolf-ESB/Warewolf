///<reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />
///<reference path="../../wwwroot/Scripts/Services/ServiceData.js" />

/// <reference path="../../wwwroot/Scripts/fx/jquery-1.9.1.min.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.Guid.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.mockjax.js" />
/// <reference path="../../wwwroot/Scripts/fx/knockout-2.2.1.js" />
/// <reference path="../../wwwroot/Scripts/fx/qunit-1.11.0.js" />

/// <reference path="../../wwwroot/Scripts/warewolf-globals.js" />
/// <reference path="../../wwwroot/Scripts/warewolf-utils.js" />

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