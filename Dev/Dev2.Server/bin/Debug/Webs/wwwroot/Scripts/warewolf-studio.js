var studio = namespace('Warewolf.Studio');


studio.isAvailable = function () {
    return typeof Dev2Awesomium != 'undefined';
};

studio.cancel = function () {
    if (studio.isAvailable()) {
        Dev2Awesomium.Cancel();
    }
};

studio.close = function () {
    if (studio.isAvailable()) {
        Dev2Awesomium.Close();
    }
};

studio.save = function (value) {
    if (studio.isAvailable()) {
        Dev2Awesomium.Save(JSON.stringify(value), false);
    }
};

studio.saveAndClose = function (value) {
    
    if (studio.isAvailable()) {
        Dev2Awesomium.Save(JSON.stringify(value), true);
    }
};

studio.setValue = function (args) {
    if (studio.isAvailable()) {
        Dev2Awesomium.Dev2SetValue(args);
    }
};

studio.fetchData = function (args) {
    if (studio.isAvailable()) {
        return Dev2Awesomium.FetchData(args);
    }
    return "";
};

studio.getIntellisenseResults = function (searchTerm, caretPosition) {
    if (studio.isAvailable()) {
        return Dev2Awesomium.GetIntellisenseResults(searchTerm, caretPosition);
    }
    return "";
};

studio.getDataAndIntellisense = function () {
    var result = {
        data: {},
        intellisenseOptions: []
    };

    var data = "";
    var intellisenseData = "";

    if (studio.isAvailable()) {
        data = studio.fetchData("");
        intellisenseData = studio.getIntellisenseResults("[[", 0);
    }
    //else {
    //    // Some test data
    //    data = '{"TheStack":[{"Col1":"[[ResultType]]","Col2":"Managers","Col3":"","PopulatedColumnCount":2,"EvaluationFn":"IsEqual"}],"TotalDecisions":1,"ModelName":"Dev2DecisionStack","Mode":"AND","TrueArmText":"Managers","FalseArmText":"Employees"}';
    //    intellisenseData = '["[[SplitChar]]", "[[StringToSplit]]", "[[ResultType]]", "[[Employees()]]", "[[Employees().Number]]", "[[Employees().Title]]", "[[Employees().FirstName]]", "[[Employees().LastName]]", "[[Employees().JobTitle]]", "[[Employees().Tel]]"]';
    //}

    if (data !== '') {
        result.data = $.parseJSON(data);
    }
    if (intellisenseData !== '') {
        result.intellisenseOptions = $.parseJSON(intellisenseData);
    }

    // remove invalid selections
    $.each(result.intellisenseOptions, function (index, value) {
        if (typeof value != 'undefined' && value != null) {
            if (value.indexOf("()]]") != -1) {
                result.intellisenseOptions.splice(index, 1);
            }
        }
    });

    return result;
};
