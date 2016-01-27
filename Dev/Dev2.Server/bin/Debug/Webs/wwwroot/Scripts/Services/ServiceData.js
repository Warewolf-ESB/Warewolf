// Make this available to chrome debugger
//@ sourceURL=ServiceData.js  

function ServiceData(resourceID, resourceType) {
    var self = this;
    
    self.resourceID = ko.observable(resourceID);
    self.resourceType = ko.observable(resourceType);
    self.resourceName = ko.observable("");
    self.resourcePath = ko.observable("");

    self.source = ko.observable();
    self.method = {
        Name: ko.observable(""),
        SourceCode: ko.observable(""),
        Parameters: ko.observableArray()
    };
    self.recordsets = ko.observableArray();
}

ServiceData.prototype.toJSON = function () {
    
    // easy way to get a clean copy
    var copy = ko.toJS(this);

    // remove extra properties

    //return the copy to be serialized
    return copy;
};