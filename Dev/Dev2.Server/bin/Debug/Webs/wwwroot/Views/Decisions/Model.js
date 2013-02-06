function AppViewModel() {
    var self = this;
    var $mainForm = $("#mainForm");
    var $decisionView = $("#decisionView");

    // set title
    document.title = "Decision Flow";
		
    self.data = {
		
        /* Injected Data */
        Mode : ko.observable("AND"),
        TrueArmText : ko.observable("True"),
        FalseArmText : ko.observable("False"),
        IsAnd : ko.observable(true),
        TheStack : ko.observableArray([]),
			
        /* End Injected	Data */
			
        /* The data here must match the server's decision functions */
        decisionFunctions : ko.observableArray([
				
            { displayValue : "There Is An Error", optionValue : "IsError", columnCount : 0 },
            { displayValue : "There Is No Error", optionValue : "IsNotError", columnCount : 0 },
				
            { displayValue : "Is Numeric", optionValue : "IsNumeric", columnCount : 1 },
            { displayValue : "Is Not Numeric", optionValue : "IsNotNumeric", columnCount : 1 },
            { displayValue : "Is Text", optionValue : "IsText", columnCount : 1 },
            { displayValue : "Is Not Text", optionValue : "IsNotText", columnCount : 1 },
            { displayValue : "Is Alphanumeric", optionValue : "IsAlphanumeric", columnCount : 1 },
            { displayValue : "Is Not Alphanumeric", optionValue : "IsNotAlphanumeric", columnCount : 1 },
            { displayValue : "Is XML", optionValue : "IsXML", columnCount : 1 },
            { displayValue : "Is Not XML", optionValue : "IsNotXML", columnCount : 1 },
            { displayValue : "Is Date", optionValue : "IsDate", columnCount : 1 },
            { displayValue : "Is Not Date", optionValue : "IsNotDate", columnCount : 1 },
            { displayValue : "Is Email", optionValue : "IsEmail", columnCount : 1 },
            { displayValue : "Is Not Email", optionValue : "IsNotEmail", columnCount : 1 },
				
				
            { displayValue : "Is RegEx", optionValue : "IsRegEx", columnCount : 2 },
            { displayValue : "Is Equal", optionValue : "IsEqual", columnCount : 2 },
            { displayValue : "Is Not Equal", optionValue : "IsNotEqual", columnCount : 2 },
            { displayValue : "Is Less Than", optionValue : "IsLessThan", columnCount : 2 },
            { displayValue : "Is Less Than or Equal", optionValue : "IsLessThanOrEqual", columnCount : 2 },
            { displayValue : "Is Greater Than", optionValue : "IsGreaterThan", columnCount : 2 },
            { displayValue : "Is Greater Than or Equal", optionValue : "IsGreaterThanOrEqual", columnCount : 2 },
				
				
            { displayValue : "Starts With", optionValue : "IsStartsWith", columnCount : 2 },
            { displayValue : "Ends With", optionValue : "IsEndsWith", columnCount : 2 },
            { displayValue : "Contains", optionValue : "IsContains", columnCount : 2 },
				
            { displayValue : "Is Between", optionValue : "IsBetween", columnCount : 3 },
        ])
    }