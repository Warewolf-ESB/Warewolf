///<reference path="../../wwwroot/Scripts/Services/WebServiceViewModel.js" />
///<reference path="../../wwwroot/Scripts/Services/ServiceData.js" />
///<reference path="../../wwwroot/Scripts/Sources/WebSourceViewModel.js" />
///<reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

/// <reference path="../../wwwroot/Scripts/fx/jquery-1.9.1.min.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.Guid.js" />
/// <reference path="../../wwwroot/Scripts/fx/jquery.mockjax.js" />
/// <reference path="../../wwwroot/Scripts/fx/knockout-2.2.1.js" />
/// <reference path="../../wwwroot/Scripts/fx/qunit-1.11.0.js" />

/// <reference path="../../wwwroot/Scripts/warewolf-globals.js" />
/// <reference path="../../wwwroot/Scripts/warewolf-utils.js" />

module("Constructor Tests");

test("ConstructorExpectedModelResourceWebServiceOkDisabledOnLoad", function () {

    var model = new WebServiceViewModel();
    ok(!model.isFormValid(), "WebService", "Ok Enabled By Default");
});

module("Pasting Datalist Fragments");

test("'isPaste'SubscriberSets'hasSourceSelectionChanged'ToTrue", function () {

    var model = new WebServiceViewModel();
    model.isPaste(true);
    ok(model.hasSourceSelectionChanged, "hasSourceSelectionChanged not true after pasting data");
});

test("'isPaste'SubscriberSets'hasSourceSelectionChanged'ToFalse", function () {

    var model = new WebServiceViewModel();
    model.isPaste(false);
    ok(!model.hasSourceSelectionChanged, "hasSourceSelectionChanged not false after typing data");
});

test("RequestUrlKeyDownEventCanDetectPaste", function () {

    var model = new WebServiceViewModel();
    model.RequestUrlOnKeyDownEvent(null, { ctrlKey: true, keyCode: 86 });
    ok(model.isPaste(), "Can Request Url detect a paste");
});

test("RequestUrlKeyDownEventCanDetectNoPaste", function () {

    var model = new WebServiceViewModel();
    model.RequestUrlOnKeyDownEvent(null, { keyCode: 9000 });
    ok(!model.isPaste(), "Can Request Url detect when not a paste");
});

test("'isPaste'SubscriberWithTrueIsPasteClearsRequestVariables", function() {

    var model = new WebServiceViewModel();
    model.data.method.Parameters(["TestVariable","AnotherTestVariable"]);
    model.isPaste(true);
    equal(model.data.method.Parameters().length, 0, "Request params should be cleared after paste");
});

test("'isPaste'SubscriberWithFalseIsCutClearsRequestVariables", function () {

    var expected = ["TestVariable", "AnotherTestVariable"];
    var model = new WebServiceViewModel();
    model.data.method.Parameters(expected);
    model.isPaste(false);
    equal(model.data.method.Parameters(), expected, "Request params should not be cleared after paste has completed");
});

module("Cutting Datalist Fragments");

test("'isCut'SubscriberSets'hasSourceSelectionChanged'ToTrue", function () {

    var model = new WebServiceViewModel();
    model.isCut(true);
    ok(model.hasSourceSelectionChanged, "hasSourceSelectionChanged not true after pasting data");
});

test("'isCut'SubscriberSets'hasSourceSelectionChanged'ToFalse", function () {

    var model = new WebServiceViewModel();
    model.isCut(false);
    ok(!model.hasSourceSelectionChanged, "hasSourceSelectionChanged not false after typing data");
});

test("RequestUrlKeyDownEventCanDetectCut", function () {

    var model = new WebServiceViewModel();
    model.RequestUrlOnKeyDownEvent(null, { ctrlKey: true, keyCode: 88 });
    ok(model.isCut(), "Can Request Url detect a Cut");
});

test("RequestUrlKeyDownEventCanDetectNoCut", function () {

    var model = new WebServiceViewModel();
    model.RequestUrlOnKeyDownEvent(null, { keyCode: 9000 });
    ok(!model.isCut(), "Can Request Url detect when not a Cut");
});

test("'isCut'SubscriberWithTrueIsCutClearsRequestVariables", function () {

    var model = new WebServiceViewModel();
    model.data.method.Parameters(["TestVariable", "AnotherTestVariable"]);
    model.isCut(true);
    equal(model.data.method.Parameters().length, 0, "Request params should be cleared after paste");
});

test("'isCut'SubscriberWithFalseIsCutClearsRequestVariables", function () {

    var expected = ["TestVariable", "AnotherTestVariable"];
    var model = new WebServiceViewModel();
    model.data.method.Parameters(expected);
    model.isCut(false);
    equal(model.data.method.Parameters(), expected, "Request params should not be cleared after paste has completed");
});
