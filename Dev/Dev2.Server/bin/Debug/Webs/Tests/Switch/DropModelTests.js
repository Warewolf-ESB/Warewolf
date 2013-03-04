/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Switch/DropModel.js" />

module("Switch Drop Model Save Tests");

test("SaveWithAmpersandExpectedAmpersandRemoved", function () {
    var model = new AppViewModel();
    model.data.SwitchVariable("[[contains &&s]]");
    model.save();
    equal(model.data.SwitchVariable(), "[[contains s]]");
});

test("SaveWithoutBracesExpectedSquareBracketsIntroduced", function () {
    var model = new AppViewModel();
    model.data.SwitchVariable("does not contain braces");
    model.save();
    equal(model.data.SwitchVariable(), "[[does not contain braces]]");
});