function SerivceViewModel(resourceID, resourceType) {
    var self = this;

    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 275;
    var $sourceMethods = $("#sourceMethods");

    self.saveUri = "Service/Services/Save";
    
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);

    self.data = {
        resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable(resourceType),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        source: ko.observable(),
        methodName: ko.observable(""),
        methodParameters: ko.observableArray()
    };

    self.sources = ko.observableArray();
    self.sourceMethods = ko.observableArray();
    self.sourceMethodSearchTerm = ko.observable("");
    self.sourceMethodSearchResults = ko.computed(function () {
        var term = self.sourceMethodSearchTerm().toLowerCase();
        if (term == "") {
            return self.sourceMethods();
        }
        return ko.utils.arrayFilter(self.sourceMethods(), function (serviceAction) {
            return serviceAction.Name.toLowerCase().indexOf(term) !== -1;
        });
    });
    
    self.data.source.subscribe(function (newValue) {
        self.data.methodName("");
        self.data.methodParameters([]);
        
        $.post("Service/Services/Methods" + window.location.search, ko.toJSON(newValue), function (result) {
            self.sourceMethods(result);
            self.sourceMethods.sort(utils.nameCaseInsensitiveSort);
        });
    });   

    self.title = ko.observable("New Service");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });
    self.helpDictionaryID = "Service";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    
    utils.registerSelectHandler($sourceMethods, function (selectedItem) {
        self.data.methodName(selectedItem.Name);
        self.data.methodParameters(selectedItem.Parameters);
        console.log(selectedItem);
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

            self.title(self.isEditing ? "Edit Database Service - " + result.ResourceName : "New Database Service");
        });
        $.post("Service/Resources/Sources" + window.location.search, args, function (result) {
            self.sources(result);
            self.sources.sort(utils.resourceNameCaseInsensitiveSort);
        });
        $.post("Service/Help/GetDictionary" + window.location.search, self.helpDictionaryID, function (result) {
            self.helpDictionary = result;
            self.updateHelpText("default");
        });
    };
    
    self.updateHelpText = function (id) {
        var text = self.helpDictionary[id];
        text = text ? text : self.helpDictionary.default;
        text = text ? text : "";
        self.helpText(text);
    };

    $(":input").focus(function () {
        self.updateHelpText(this.id);
    });
    
    self.editSource = function () {
    };

    self.newSource = function () {
    };

    self.testAction = function () {
    };

    self.cancel = function () {
    };

    self.save = function () {
        //if (!self.validate()) {
        //    return;
        //}
        
        var args = ko.toJSON(self.data);
        $.post("Service/Services/Save" + window.location.search, args, function (result) {
            if (!result.IsValid) {
                Dev2Awesomium.Dev2SetValue(JSON.stringify(result));
            }
            // TODO: ShowError
        });
    };

    self.initialize = function () {
    };
    
    self.initialize();
    self.load();
};
