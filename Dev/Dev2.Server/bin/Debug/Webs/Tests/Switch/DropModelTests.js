///// <reference path="../../wwwroot/Scripts/_references.js" />
///// <reference path="../../wwwroot/Scripts/Switch/DropModel.js" />

//QUnit.init();
//QUnit.config.blocking = false;
//QUnit.config.autorun = true;
//QUnit.config.updateRate = 0;
//QUnit.log = function(result, message) {
//    print(result ? 'PASS' : 'FAIL', message);
//};

//module("Switch Drop Model Save Tests");

//test("SaveWithAmpersandsExpectedAmpersandsRemoved", function () {
//    var model = new AppViewModel();
//    model.data.SwitchVariable("[[contains &&s]]");
//    model.save();
//    equal(model.data.SwitchVariable(), "[[contains s]]");
//});

//test("SaveWithoutBracesExpectedSquareBracketsIntroduced", function () {
//    var model = new AppViewModel();
//    model.data.SwitchVariable("does not contain braces");
//    model.save();
//    equal(model.data.SwitchVariable(), "[[does not contain braces]]");
//});