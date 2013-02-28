var utils = namespace('Dev2.Utils');

utils.caseInsensitiveSort = function(left, right) {
     return left.toLowerCase() == right.toLowerCase() ? 0 : (left.toLowerCase() < right.toLowerCase() ? -1 : 1);
};

utils.resourceNameCaseInsensitiveSort = function (left, right) {
    return left.ResourceName.toLowerCase() == right.ResourceName.toLowerCase() ? 0 : (left.ResourceName.toLowerCase() < right.ResourceName.toLowerCase() ? -1 : 1);
};

utils.nameCaseInsensitiveSort = function (left, right) {
    return left.Name.toLowerCase() == right.Name.toLowerCase() ? 0 : (left.Name.toLowerCase() < right.Name.toLowerCase() ? -1 : 1);
};



utils.registerSelectHandler = function ($ol, selectHandler) {
    $ol.on("click", "li.selectable", function () {
        utils.selectListItem($(this));
        var selectedItem = ko.dataFor(this);
        selectHandler(selectedItem);
    });
};

utils.selectListItem = function ($li) {
    $li.addClass("ui-selected").siblings().removeClass("ui-selected");
};

utils.selectAndScrollToListItem = function (itemText, $scrollBox, $scrollBoxHeight) {
    // Find the element and select it.
    var $span = $("li.selectable span:contains('" + itemText + "')");
    var $li = $span.parent();
    utils.selectListItem($li);
    $scrollBox.scrollTo($li, $scrollBoxHeight);
};

utils.toHtml = function (str) {
    return str.replace(/\n/g, '<br />').replace(/\t/g, '&nbsp;&nbsp;&nbsp;&nbsp;');
};

utils.IsNullOrEmptyGuid = function (str) {
    return !$.Guid.IsValid(str) || $.Guid.IsEmpty(str);
};

utils.GetDataAndIntellisense = function () {
    var result = {
        data: {},
        intellisenseOptions: []
    };
    
    var data = "";
    var intellisenseData = "";
    
    if (typeof Dev2Awesomium != 'undefined') {
        data = Dev2Awesomium.FetchData("");
        intellisenseData = Dev2Awesomium.GetIntellisenseResults("[[", 0);
    }
    //else {
    //    // Some test data
    //    data = '{"TheStack":[{"Col1":"[[ResultType]]","Col2":"Managers","Col3":"","PopulatedColumnCount":2,"EvaluationFn":"IsEqual"}],"TotalDecisions":1,"ModelName":"Dev2DecisionStack","Mode":"AND","TrueArmText":"Managers","FalseArmText":"Employees"}';
    //    intellisenseData = '["[[SplitChar]]", "[[StringToSplit]]", "[[ResultType]]", "[[Employees()]]", "[[Employees().Number]]", "[[Employees().Title]]", "[[Employees().FirstName]]", "[[Employees().LastName]]", "[[Employees().JobTitle]]", "[[Employees().Tel]]"]';
    //}

    if (data !== '')
    {
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