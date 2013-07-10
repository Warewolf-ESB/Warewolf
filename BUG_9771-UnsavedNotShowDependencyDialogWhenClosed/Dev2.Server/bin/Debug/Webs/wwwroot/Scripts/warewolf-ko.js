// A filtered view of an observable array
//
// The filterByProperty function will become available 
// on all subsequently-created ko.observableArray instances
ko.observableArray.fn.filterByProperty = function (propName, matchValue) {
    return ko.computed(function () {
        var allItems = this(), matchingItems = [];
        for (var i = 0; i < allItems.length; i++) {
            var current = allItems[i];
            if (ko.utils.unwrapObservable(current[propName]) === matchValue)
                matchingItems.push(current);
        }
        return matchingItems;
    }, this);
};


// Get Knockout to enable/disable jQuery buttons using the Knockout 'enable' bindingHandler. 
// This enables/disables the underlying element that has had the .button() method run on it
//
// Usage: data-bind="jEnable: isEnabled"
if (ko && ko.bindingHandlers) {
    ko.bindingHandlers['jEnable'] = {
        'update': function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            var $element = $(element);
            $element.prop("disabled", !value);

            if ($element.hasClass("ui-button")) {
                $element.button("option", "disabled", !value);
            }
        }
    };
}