var utils = namespace('Dev2.Utils');

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

utils.getDialogPosition = function () {
    var parentContainerID = $('div[id*=Container]').attr("id");
    return {
        my: "center center",
        of: "#" + parentContainerID
    };
};

utils.makeClearFilterButton = function (buttonID) {
    $("#" + buttonID)
      .text("")
      .append('<img height="16px" width="16px" src="images/clear-filter.png" />')
      .button();
};

utils.parseBaseURL = function(baseURL){
	pathArray = baseURL.split( '/' );
	host = pathArray[0] + "//"+ pathArray[1] + pathArray[2];
	
	return host;
}