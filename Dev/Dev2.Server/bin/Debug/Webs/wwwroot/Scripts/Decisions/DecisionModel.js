// Make this available to chrome debugger
//@ sourceURL=DecisionModel.js

/* Model for Decision Wizard */
function DecisionViewModel() {
    var self = this;
    var $mainForm = $("#mainForm");
    var $decisionView = $("#decisionView");

    // set title
    document.title = "Decision Flow";
    self.data = {
        /* Injected Data */
        Mode: ko.observable("AND"),
        DisplayText: ko.observable("Is true?"),
        TrueArmText: ko.observable("True"),
        FalseArmText: ko.observable("False"),
        IsAnd: ko.observable(true),
        TheStack: ko.observableArray([]),

        /* End Injected	Data */

        /* The data here must match the server's decision functions */
        decisionFunctions: ko.observableArray([
			/* Not a valid type, here for user friendlyness */
			{ displayValue: "Choose...", optionValue: "Choose...", columnCount: 1 },
			/* Start valid types */
            { displayValue: "There Is An Error", optionValue: "IsError", columnCount: 0 },
            { displayValue: "There Is No Error", optionValue: "IsNotError", columnCount: 0 },
            { displayValue: "=", optionValue: "IsEqual", columnCount: 2 },
            { displayValue: ">", optionValue: "IsGreaterThan", columnCount: 2 },
            { displayValue: "<", optionValue: "IsLessThan", columnCount: 2 },
            { displayValue: "<> (Not Equal)", optionValue: "IsNotEqual", columnCount: 2 },
            { displayValue: ">=", optionValue: "IsGreaterThanOrEqual", columnCount: 2 },
            { displayValue: "<=", optionValue: "IsLessThanOrEqual", columnCount: 2 },
            { displayValue: "Starts With", optionValue: "IsStartsWith", columnCount: 2 },
            { displayValue: "Ends With", optionValue: "IsEndsWith", columnCount: 2 },
            { displayValue: "Contains", optionValue: "IsContains", columnCount: 2 },
            { displayValue: "Doesn't Start With", optionValue: "NotStartsWith", columnCount: 2 },
            { displayValue: "Doesn't End With", optionValue: "NotEndsWith", columnCount: 2 },
            { displayValue: "Doesn't Contain", optionValue: "NotContain", columnCount: 2 },
            { displayValue: "Is Alphanumeric", optionValue: "IsAlphanumeric", columnCount: 1 },
            { displayValue: "Is Base64", optionValue: "IsBase64", columnCount: 1 },
            { displayValue: "Is Between", optionValue: "IsBetween", columnCount: 3 },
            { displayValue: "Is Binary", optionValue: "IsBinary", columnCount: 1 },
            { displayValue: "Is Date", optionValue: "IsDate", columnCount: 1 },
            { displayValue: "Is Email", optionValue: "IsEmail", columnCount: 1 },
            { displayValue: "Is Hex", optionValue: "IsHex", columnCount: 1 },
            { displayValue: "Is Numeric", optionValue: "IsNumeric", columnCount: 1 },
            { displayValue: "Is RegEx", optionValue: "IsRegEx", columnCount: 2 },
            { displayValue: "Is Text", optionValue: "IsText", columnCount: 1 },
            { displayValue: "Is XML", optionValue: "IsXML", columnCount: 1 },
            { displayValue: "Not Alphanumeric", optionValue: "IsNotAlphanumeric", columnCount: 1 },
            { displayValue: "Not Base64", optionValue: "IsNotBase64", columnCount: 1 },
            { displayValue: "Not Between", optionValue: "NotBetween", columnCount: 3 },
            { displayValue: "Not Binary", optionValue: "IsNotBinary", columnCount: 1 },
            { displayValue: "Not Date", optionValue: "IsNotDate", columnCount: 1 },
            { displayValue: "Not Email", optionValue: "IsNotEmail", columnCount: 1 },
            { displayValue: "Not Hex", optionValue: "IsNotHex", columnCount: 1 },
            { displayValue: "Not Numeric", optionValue: "IsNotNumeric", columnCount: 1 },
            { displayValue: "Not RegEx", optionValue: "NotRegEx", columnCount: 2 },
            { displayValue: "Not Text", optionValue: "IsNotText", columnCount: 1 },
            { displayValue: "Not XML", optionValue: "IsNotXML", columnCount: 1 }            
            ])
    };
    
    self.intellisenseOptions = [];

    self.createEmptyDecision = function () {
        return { Col1: ko.observable(""), Col2: ko.observable(""), Col3: ko.observable(""), PopulatedColumnCnt: ko.observable(1), EvaluationFn: ko.observable("Choose...") };
    };

    self.ShowDelete = function (idx) {
        var result = false;

        if (self.data.TheStack().length > 1 && idx >= 0) {
            result = true;
        }

        return result;
    };

    self.addRow = function () {
        // Default Row ;)
        self.data.TheStack.push(self.createEmptyDecision());

        // Find the element and select it. -- decisionRow
        var $span = $("#decisionRow:last-child");
        
        $decisionView.scrollTo($span, 280); // height of container

        // apply jquery-ui themes
        $("input[type=submit], a, button").button();

    };

    self.removeRow = function () {
        self.data.TheStack.remove(this);
    };

    self.rowChanged = self.rowChanged = function (elm, event) {

        // find function element to use ;)
        var cnt = ko.utils.arrayFirst(self.data.decisionFunctions(), function (item) {
            return elm.EvaluationFn() === item.optionValue;
        });        
        elm.PopulatedColumnCnt(cnt.columnCount);
        if(cnt.columnCount == 0) {
            elm.Col1("");
            elm.Col2("");
            elm.Col3("");
        }
        else if (cnt.columnCount == 1) {
            elm.Col2("");
            elm.Col3("");
        }
        else if (cnt.columnCount == 2) {
            elm.Col3("");
        }
  
        // apply jquery-ui themes
        //Trev said this is fine, its for testing
        var selector = $("input[type=submit], a, button");
        if(selector.button != undefined)
        {
            selector.button();    
        }        
    };

    self.toggleMode = function () {

        if (!self.data.IsAnd()) {
            self.data.Mode("OR");
            self.data.IsAnd(false);
        } else {
            self.data.Mode("AND");
            self.data.IsAnd(true);
        }

        return true;
    };

    self.save = function () {
        if (!$mainForm.validate().form()) {
            return;
        }
        
        var jsonData = ko.toJSON(self.data);
        studio.setValue(jsonData);
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.AddDecision = function (data) {
        self.data.TheStack.push(data);
        self.rowChanged(data);
        // apply jquery-ui themes
        $("input[type=submit], a, button").button();
    };

    self.afterRowAdd = function (element, index, data) {
        $(element).filter("input").autocomplete({
            source: self.intellisenseOptions
        });
        $(element).filter("textarea").autocomplete({
            source: self.intellisenseOptions
        });
    };

    self.afterRowRender = function (elements) {
        $(elements[1]).find("input").autocomplete({
            source: self.intellisenseOptions
        });
        $(elements[1]).find("textarea").autocomplete({
            source: self.intellisenseOptions
        });
    };
    
    self.Load = function() {

        //BUG 8377 Add intellisense
        var response = {};
        if (studio) {
            var dai = studio.getDataAndIntellisense();
            self.intellisenseOptions = dai.intellisenseOptions;
            response = dai.data;
        }

        var responseDisplayText = "";
        if (response.TheStack != undefined) {
            // load decisions
            for (var i = 0; i < response.TheStack.length; i++) {
                var decision = ko.mapping.fromJS({ Col1: response.TheStack[i].Col1, Col2: response.TheStack[i].Col2, Col3: response.TheStack[i].Col3, PopulatedColumnCnt: response.TheStack[i].PopulatedColumnCount, EvaluationFn: response.TheStack[i].EvaluationFn });
                self.AddDecision(decision);               
            }

            // set Stack data
            self.data.Mode(response.Mode);

            if (response.Mode == "AND") {
                self.data.IsAnd(true);
            } else {
                self.data.IsAnd(false);
            }

            // set the arms
            self.data.TrueArmText(response.TrueArmText);
            self.data.FalseArmText(response.FalseArmText);
            
            responseDisplayText = response.DisplayText;
        }else{
            // Add a decision
            self.AddDecision(self.createEmptyDecision());
        }
        
        self.displayTextComputed = ko.computed(function () {
            var firstDecision = self.data.TheStack()[0];
            var option = ko.utils.arrayFirst(self.data.decisionFunctions(), function (item) {
                return firstDecision.EvaluationFn() === item.optionValue;
            });

            var result = "";
            switch (option.columnCount) {
                case 0:
                    result = "If " + option.displayValue;
                    break;
                case 2:
                    result = "If " + firstDecision.Col1() + " " + option.displayValue + " " + firstDecision.Col2();
                    break;
                case 3:
                    result = "If " + firstDecision.Col1() + " " + option.displayValue + " " + firstDecision.Col2() + " and " + firstDecision.Col3();
                    break;
                default:
                    result = "If " + firstDecision.Col1() + " " + option.displayValue;
            }
            self.data.DisplayText(result);
            return result;
        });

        if (responseDisplayText != "") {
            self.data.DisplayText(response.DisplayText);
        }
    };

    self.onKeyDown = function (model, event) {
        var key = event.keyCode;

        if (event.ctrlKey && key == 13) {
            self.addRow();
        } else {
            return true;
        }
    };
}