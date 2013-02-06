function SerivceViewModel(resourceID, resourceType) {
    var self = this;

    self.saveUri = "Service/Services/Save";
    
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    self.title = ko.observable(self.isEditing ? "Edit Service" : "New Service");

    self.data = {
        resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable(resourceType),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),
        /*

        sources: ko.observableArray(""),
        actions: ko.observableArray(),
        source: ko.observable(""),
        action: ko.observable("")
    */
    };

    self.load = function() {
        $.post("Service/Services/Get" + window.location.search, self.data.resourceID(), function (result) {
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            if (self.isEditing) {
                self.title("Edit Server - " + result.ResourceName);
            }
        });
    };
};
