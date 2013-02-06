/* 
    Travis.Frisinger - 06.02.2013
    Test Decision Model Functionality
    REF : http://www.testdrivenjs.com/getting-started/introduction-to-tests-in-qunit/
    REF : http://tjvantoll.com/2012/08/22/logging-test-failures-in-a-phantomjs-qunit-runner/
*/

//require("../wwwroot/Views/Decisions/DecisionModel.js");


module("Core DecisionModel Test");


test("Model Can Init Test", function() {

    var vm = DecisionViewModel();
    
    equal(vm.data.TheStack.length,0, "Passed");

});

test("Model Can Add Row", function () {

    var vm = DecisionViewModel();

    vm.addRow();

    equal(vm.data.TheStack.length,1, "Passed");

});


test("Model Can Remove Row", function () {

    var vm = DecisionViewModel();

    vm.addRow();

    equal(vm.data.TheStack.length,1, "Passed");

});

test("Model Can Toggle Mode Between AND / OR", function () {

    var vm = DecisionViewModel();

    vm.addRow();

    equal(vm.data.TheStack.length, 1, "Passed");

});

test("Model Can Add Decision", function () {

    var vm = DecisionViewModel();

    vm.addRow();

    equal(vm.data.TheStack.length,1, "Passed");

});


test("Model Has Correct Number of Decision Functions", function () {

    var vm = DecisionViewModel();

    vm.addRow();

    equal(vm.data.decisionFunctions.length, 26, "Passed");

});