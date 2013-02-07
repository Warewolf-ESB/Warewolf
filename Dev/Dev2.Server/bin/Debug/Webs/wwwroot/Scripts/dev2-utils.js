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
