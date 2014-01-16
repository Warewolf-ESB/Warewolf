/// <reference path="../../wwwroot/Scripts/Decisions/DecisionModel.js" />
/// <reference path="../../wwwroot/Scripts/_references.js" />


/* 
    Travis.Frisinger - 06.02.2013
    Test Decision Model Functionality
    REF : http://www.testdrivenjs.com/getting-started/introduction-to-tests-in-qunit/
    REF : http://tjvantoll.com/2012/08/22/logging-test-failures-in-a-phantomjs-qunit-runner/
*/


module("Core DecisionModel Test");


test("ConstructorWithNoParamsExpectedDecisionStackInitialized", function () {

    var model = new DecisionViewModel();
    equal(model.data.TheStack().length, 0, "Did Decision Stack Initialize");
});

test("DecisionTypeChangedClearsText", function () {

    var model = new DecisionViewModel();
    
    var decision = { Col1: ko.observable("TextStuff"), Col2: ko.observable("TextStuff"), Col3: ko.observable("TextStuff"), PopulatedColumnCnt: ko.observable(3), EvaluationFn: ko.observable("IsNumeric") };
    model.rowChanged(decision, null);   
    equal(decision.Col2(), "", "Col2 is not empty");
    equal(decision.Col3(), "", "Col3 is not empty");
    equal(decision.PopulatedColumnCnt(), 1, "PopulationColumnCnt is wrong");
});

test("ConstructorWithNoParamsExpectedDecisionStackInitializedWithCorrectOrderOfDecisionFunctions", function () {

    var model = new DecisionViewModel();
    var decisionTypes = model.data.decisionFunctions();
    
    equal(model.data.decisionFunctions()[1].displayValue, "There Is An Error", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[2].displayValue, "There Is No Error", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[3].displayValue, "=", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[4].displayValue, ">", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[5].displayValue, "<", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[6].displayValue, "<> (Not Equal)", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[7].displayValue, ">=", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[8].displayValue, "<=", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[9].displayValue, "Starts With", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[10].displayValue, "Ends With", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[11].displayValue, "Contains", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[12].displayValue, "Is Alphanumeric", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[13].displayValue, "Is Base64", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[14].displayValue, "Is Between", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[15].displayValue, "Is Binary", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[16].displayValue, "Is Date", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[17].displayValue, "Is Email", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[18].displayValue, "Is Hex", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[19].displayValue, "Is Numeric", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[20].displayValue, "Is RegEx", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[21].displayValue, "Is Text", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[22].displayValue, "Is XML", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[23].displayValue, "Not Alphanumeric", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[24].displayValue, "Not Base64", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[25].displayValue, "Not Binary", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[26].displayValue, "Not Date", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[27].displayValue, "Not Email", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[28].displayValue, "Not Hex", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[29].displayValue, "Not Numeric", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[30].displayValue, "Not Text", "DecisionFunctions are in the wrong order");
    equal(model.data.decisionFunctions()[31].displayValue, "Not XML", "DecisionFunctions are in the wrong order");
    
});


test("ConstructorWithNoParamsExpectedCorrectNumberofDecisionFunctions", function () {

    var model = new DecisionViewModel();
    equal(model.data.decisionFunctions().length, 32, "Did Correct Number of Decision Functions Initialize");
});

test("AddRowExpectedDecisionStackLengthIncreased", function () {

    var model = new DecisionViewModel();
    var expected = model.data.TheStack.length + 1;
    model.addRow();
    equal(model.data.TheStack().length, expected, "Did Decision Stack Length Increase");
});

test("ToggleModeExpectedModeChanged", function () {

    var model = new DecisionViewModel();
    ok(model.toggleMode(), "Can Toggle Mode");
});

test("AddDecisionExpectedStackLengthIncreased", function () {

    var model = new DecisionViewModel();
    var expected = model.data.TheStack.length + 1;
    model.AddDecision({ Col1: '', Col2: '', Col3: '', PopulatedColumnCnt: 1, EvaluationFn: 'Choose...' });
    equal(model.data.TheStack().length, expected, "Did Stack Length Increase");
});