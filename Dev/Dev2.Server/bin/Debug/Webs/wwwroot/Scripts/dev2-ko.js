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