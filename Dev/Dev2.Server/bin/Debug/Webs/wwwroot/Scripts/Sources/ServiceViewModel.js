function SerivceViewModel(resourceID, loadedCallback) {
    var self = this;

    self.saveUri = "Service/Services/Save";
    
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    //self.title = ko.observable(self.isEditing ? "Edit Service" : "New Service");

    self.data = {
    /*
        resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable("Dev2Server"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        sources: ko.observableArray(""),
        actions: ko.observableArray(),
        source: ko.observable(""),
        action: ko.observable("")
    */
    };
};
