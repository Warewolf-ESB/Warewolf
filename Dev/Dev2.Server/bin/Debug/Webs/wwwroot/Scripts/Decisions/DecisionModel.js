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
            { displayValue: "Is Numeric", optionValue: "IsNumeric", columnCount: 1 },
            { displayValue: "Is Not Numeric", optionValue: "IsNotNumeric", columnCount: 1 },
            { displayValue: "Is Text", optionValue: "IsText", columnCount: 1 },
            { displayValue: "Is Not Text", optionValue: "IsNotText", columnCount: 1 },
            { displayValue: "Is Alphanumeric", optionValue: "IsAlphanumeric", columnCount: 1 },
            { displayValue: "Is Not Alphanumeric", optionValue: "IsNotAlphanumeric", columnCount: 1 },
            { displayValue: "Is XML", optionValue: "IsXML", columnCount: 1 },
            { displayValue: "Is Not XML", optionValue: "IsNotXML", columnCount: 1 },
            { displayValue: "Is Date", optionValue: "IsDate", columnCount: 1 },
            { displayValue: "Is Not Date", optionValue: "IsNotDate", columnCount: 1 },
            { displayValue: "Is Email", optionValue: "IsEmail", columnCount: 1 },
            { displayValue: "Is Not Email", optionValue: "IsNotEmail", columnCount: 1 },
            // PBI : 8741
            { displayValue: "Is Binary", optionValue: "IsBinary", columnCount: 1 },
            { displayValue: "Is Not Binary", optionValue: "IsNotBinary", columnCount: 1 },
            { displayValue: "Is Hex", optionValue: "IsHex", columnCount: 1 },
            { displayValue: "Is Not Hex", optionValue: "IsNotHex", columnCount: 1 },
            { displayValue: "Is Base64", optionValue: "IsBase64", columnCount: 1 },
            { displayValue: "Is Not Base64", optionValue: "IsNotBase64", columnCount: 1 },

            { displayValue: "Is RegEx", optionValue: "IsRegEx", columnCount: 2 },
            { displayValue: "Is Equal", optionValue: "IsEqual", columnCount: 2 },
            { displayValue: "Is Not Equal", optionValue: "IsNotEqual", columnCount: 2 },
            { displayValue: "Is Less Than", optionValue: "IsLessThan", columnCount: 2 },
            { displayValue: "Is Less Than or Equal", optionValue: "IsLessThanOrEqual", columnCount: 2 },
            { displayValue: "Is Greater Than", optionValue: "IsGreaterThan", columnCount: 2 },
            { displayValue: "Is Greater Than or Equal", optionValue: "IsGreaterThanOrEqual", columnCount: 2 },
            { displayValue: "Starts With", optionValue: "IsStartsWith", columnCount: 2 },
            { displayValue: "Ends With", optionValue: "IsEndsWith", columnCount: 2 },
            { displayValue: "Contains", optionValue: "IsContains", columnCount: 2 },
            { displayValue: "Is Between", optionValue: "IsBetween", columnCount: 3 },
        ])
    };
    
    self.intellisenseOptions = [];

    self.ShowDelete = function (idx) {
        var result = false;

        if (self.data.TheStack().length > 1 && idx >= 0) {
            result = true;
        }

        return result;
    };

    self.addRow = function () {
        // Default Row ;)
		// { displayValue: "Choose...", optionValue: "Choose...", columnCount: 0 },
        self.data.TheStack.push({ Col1: '', Col2: '', Col3: '', PopulatedColumnCnt: 1, EvaluationFn: 'Choose...' });

        // Find the element and select it. -- decisionRow
        var $span = $("#decisionRow:last-child");

        //var $li = $span.parent();
        //self.selectResourcePathElement($span);
        $decisionView.scrollTo($span, 280); // height of container

        // apply jquery-ui themes
        $("input[type=submit], a, button").button();

    };

    self.removeRow = function () {

        // always keep at least one ;)
        //if(self.data.TheStack().length > 1){
        self.data.TheStack.remove(this);
        //}

    };

    self.rowChanged = function (elm) {

        var idx = self.data.TheStack().indexOf(elm);
        var val = elm.EvaluationFn;

        // find function element to use ;)
        var cnt = ko.utils.arrayFirst(self.data.decisionFunctions(), function (item) {
            return elm.EvaluationFn === item.optionValue;
        });

        self.data.TheStack.splice(idx, 1, { Col1: elm.Col1, Col2: elm.Col2, Col3: elm.Col3, PopulatedColumnCnt: cnt.columnCount, EvaluationFn: val });

        // apply jquery-ui themes
        $("input[type=submit], a, button").button();

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

        //var dlID = window.location.search;
        var jsonData = ko.toJSON(self.data);

        studio.setValue(jsonData);
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.AddDecision = function (data) {
        self.data.TheStack.push(data);
        // apply jquery-ui themes
        $("input[type=submit], a, button").button();
    };

    self.afterRowAdd = function (element, index, data) {
        $(element).filter("input").autocomplete({
            source: self.intellisenseOptions
        });
    };

    self.afterRowRender = function (elements) {
        $(elements[1]).find("input").autocomplete({
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

        if (response.TheStack != undefined) {
            // load decisions
            for (var i = 0; i < response.TheStack.length; i++) {
                self.AddDecision({ Col1: response.TheStack[i].Col1, Col2: response.TheStack[i].Col2, Col3: response.TheStack[i].Col3, PopulatedColumnCnt: response.TheStack[i].PopulatedColumnCount, EvaluationFn: response.TheStack[i].EvaluationFn });
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

        } else {
            // Add a decision
            self.AddDecision({ Col1: '', Col2: '', Col3: '', PopulatedColumnCnt: 1, EvaluationFn: 'Choose...' });
        }
    };

    self.onKeyPress = function (event) {
        if (event.which == 13)
            event.preventDefault();
    };
}