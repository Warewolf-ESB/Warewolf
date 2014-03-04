var utils = namespace('Warewolf.Utils');

utils.caseInsensitiveSort = function (left, right) {
    return left.toLowerCase() == right.toLowerCase() ? 0 : (left.toLowerCase() < right.toLowerCase() ? -1 : 1);
};

utils.resourceNameCaseInsensitiveSort = function (left, right) {
    return left.ResourceName.toLowerCase() == right.ResourceName.toLowerCase() ? 0 : (left.ResourceName.toLowerCase() < right.ResourceName.toLowerCase() ? -1 : 1);
};

utils.fullNameCaseInsensitiveSort = function (left, right) {
    return left.FullName.toLowerCase() == right.FullName.toLowerCase() ? 0 : (left.FullName.toLowerCase() < right.FullName.toLowerCase() ? -1 : 1);
};

utils.nameCaseInsensitiveSort = function (left, right) {
    return left.Name.toLowerCase() == right.Name.toLowerCase() ? 0 : (left.Name.toLowerCase() < right.Name.toLowerCase() ? -1 : 1);
};

utils.textCaseInsensitiveSort = function (left, right) {
    return left.Text.toLowerCase() == right.Text.toLowerCase() ? 0 : (left.Text.toLowerCase() < right.Text.toLowerCase() ? -1 : 1);
};

utils.registerSelectHandler = function($ol, selectHandler, canOnlyRead) {
    if (!canOnlyRead) {
        $ol.on("click", "li.selectable", function() {
            utils.selectListItem($(this));
            var selectedItem = ko.dataFor(this);
            selectHandler(selectedItem);
        });
    }
};

utils.selectListItem = function ($li) {
    $li.addClass("ui-selected");
    $li.siblings().removeClass("ui-selected");
};

utils.selectAndScrollToListItem = function (itemText, $scrollBox, $scrollBoxHeight) {

    var itemTextLower = itemText.toLowerCase();

    // Find the element and select it - this way seems to be more reliable than using $("li.selectable span:contains('" + itemText + "')");
    var listItems = [];
    if ($scrollBox.length > 0) {
        $.each($scrollBox.get(0).children, function (index, child) {
            if (child.nodeName.toLowerCase() === "ol") {
                listItems = child.children;
                return true;
            }
        });
    }

    $.each(listItems, function (index, listItem) {

        // this was an index of issue ;(
        if (listItem.innerText.toLowerCase().trim() == itemTextLower) {
            var $li = $(listItem);
            utils.selectListItem($li);
            $scrollBox.scrollTo($li, $scrollBoxHeight);
        }
    });
};

utils.toHtml = function (str) {
    return str.replace(/\n/g, '<br />').replace(/\t/g, '&nbsp;&nbsp;&nbsp;&nbsp;');
};

utils.IsNullOrEmptyGuid = function (str) {
    return !$.Guid.IsValid(str) || $.Guid.IsEmpty(str);
};

utils.getDialogPosition = function () {
    var parentContainerID = $('div[id*=Container]').attr("id");
    return {
        my: "center center",
        of: "#" + parentContainerID
    };
};

utils.makeClearFilterButton = function (buttonID) {
    var $button = $("#" + buttonID);
    if ($button.length > 0) {
        $("#" + buttonID)
            .text("")
            .append('<img height="16px" width="16px" src="images/clear-filter.png" />')
            .button();
    }
};

utils.makeReloadButton = function (buttonID) {
    var $button = $("#" + buttonID);
    if ($button.length > 0) {
        $("#" + buttonID)
            .text("")
            .append('<img height="16px" width="16px" src="images/refresh.png" />')
            .button();
    }
};

utils.parseBaseURL = function (baseURL) {
    pathArray = baseURL.split('/');
    host = pathArray[0] + "//" + pathArray[1] + pathArray[2];

    return host;
};

utils.isValidEmail = function (email) {
    var result = email !== "" && /\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b/i.test(email);
    return result;
};

utils.isValidUrl = function (uri) {
    var result = uri !== "" && /^((http[s]?|ftp):\/)/i.test(uri);
    return result;
};

utils.decodeFullStops = function (expression) {
    while (expression.indexOf("%2E") > -1) {
        expression = expression.replace("%2E", ".");
    }
    return expression;
};

utils.appendEnvironmentSpan = function (selectorText, environment) {
    var $selectTitle = $("span.ui-dialog-title:contains('" + selectorText + "')", ".ui-dialog-titlebar");
    if ($selectTitle.length == 1) { // only ever append to just one selected title
        $selectTitle.css("width", "50%"); // shorten title to avoid overlapping the environment name
        $selectTitle.parent().append("<span id='inTitleEnvironmentSpan' class='inTitleSpan'>" + environment + "</span>"); // append to title span's parent div
    }
};

utils.appendEnvironmentSpanSave = function (selectorText, environment) {
    //var $selectTitle = $(".ui-dialog-titlebar span.ui-dialog-title:contains('Save')", ".ui-dialog:contains('" + selectorText + "')");
    var $selectTitle = $(".ui-dialog-titlebar span.ui-dialog-title:contains('Save')", ".ui-dialog:contains('" + selectorText + "')");
    if ($selectTitle.length == 1) { // only ever append to just one selected title
        //$selectTitle.css("width", "auto"); // save title is very short
        $selectTitle.parent().append("<span id='inTitleEnvironmentSpan' class='inSaveTitleSpan'>" + environment + "</span>"); // append to title span's parent div (with extra margin)
    }
};

utils.updateSaveValidationSpan = function (selectorText, helpText) {
    if (SaveViewModel.IsStandAlone) {
        var $standaloneHelp = $("span#inButtonBarHelpSpanStandalone.inSaveButtonBarSpan");
        if ($standaloneHelp.length == 1) {
            $standaloneHelp[0].innerHTML = helpText;
        }
    } else {
        var $inPaneHelp = $(".ui-dialog:contains('" + selectorText + "') .ui-dialog-buttonpane:last");
        var visible = $inPaneHelp.is(":visible");
        if (visible) {
            if ($inPaneHelp.length == 1) {
                var there = $inPaneHelp.find("#inButtonBarHelpSpanStandalone").size() > 0;
                if (there) {
                    $inPaneHelp.find("#inButtonBarHelpSpanStandalone")[0].innerHTML = helpText;
                } else {
                    $inPaneHelp.append("<span id='inButtonBarHelpSpanStandalone' class='inSaveButtonBarSpan'>" + helpText + "</span>");
                }
            }
        }
    }
};

//
// viewModel must have int property with the name given in timestampProp
//
utils.postTimestamped = function (viewModel, timestampProp, url, jsonData, callback, doneHandler) {
    var timestamp = new Date().valueOf();
    var searchStr = window.location.search;
    searchStr += (searchStr ? "&" : "?") + "t=" + timestamp;
    viewModel[timestampProp] = timestamp;

    $.post(url + searchStr, jsonData, function (result) {
        if (callback) {
            var requestTimeStr = getParameterByName("t", this.url);
            var requestTime = parseInt(requestTimeStr);
            if (requestTime == viewModel[timestampProp]) {
                callback(result);
            }
        };
    }).done(function() {
        if (doneHandler) {
            doneHandler();
        }
    });
};

utils.findRemoveListItems = function (data, name) {
    var arr = [], i;

    for (i = 0; i < data.length; i++) {
        if (data[i] != name) arr.push(data[i]);
    };

    return arr;
};

utils.isReadOnly = function(resourceId, callBack) {

    var args = ko.toJSON({
        resourceId: resourceId
    });

    $.post("Service/Services/IsReadOnly" + window.location.search, args, function(result) {
        if (callBack) {
            callBack(result.IsReadOnly);
        }
    });
};