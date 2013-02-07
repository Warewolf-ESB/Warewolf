function SerivceViewModel(resourceID, resourceType) {
    var self = this;

    var $sourceActionsScrollBox = $("#sourceActionsScrollBox");
    var $sourceActionsScrollBoxHeight = 275;
    var $sourceActions = $("#sourceActions");

    self.saveUri = "Service/Services/Save";
    
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    self.title = ko.observable(self.isEditing ? "Edit Service" : "New Service");

    self.data = {
        resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable(resourceType),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        source: ko.observable(),
        action: ko.observable()
    };

    self.sources = ko.observableArray();
    self.sourceActions = ko.observableArray();
    self.sourceActionSearchTerm = ko.observable("");
    self.sourceActionSearchResults = ko.computed(function () {
        var term = self.sourceActionSearchTerm().toLowerCase();
        if (term == "") {
            return self.sourceActions();
        }
        return ko.utils.arrayFilter(self.sourceActions(), function (serviceAction) {
            return serviceAction.Name.toLowerCase().indexOf(term) !== -1;
        });
    });
    self.data.source.subscribe(function (newValue) {
        $.post("Service/Services/Actions" + window.location.search, ko.toJSON(newValue), function (result) {
            self.sourceActions(result);
            self.sourceActions.sort(utils.nameCaseInsensitiveSort);
        });
    });
    
    utils.registerSelectHandler($sourceActions, function (selectedItem) {
        self.data.action(selectedItem);
    });
    
    self.load = function () {
        var args = ko.toJSON({
            resourceID: self.data.resourceID(),
            resourceType: self.data.resourceType()
        });
        $.post("Service/Services/Get" + window.location.search, args, function (result) {
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            if (self.isEditing) {
                self.title("Edit Server - " + result.ResourceName);
            }  
        });
        $.post("Service/Services/Sources" + window.location.search, args, function (result) {
            self.sources(result);
            self.sources.sort(utils.resourceNameCaseInsensitiveSort);
        });
    };
    
    self.editSource = function () {
    };

    self.newSource = function () {
    };

    self.testAction = function () {
    };

    self.cancel = function () {
    };

    self.save = function () {
    };

    self.initialize = function () {
    };
    
    self.initialize();
    self.load();
};
