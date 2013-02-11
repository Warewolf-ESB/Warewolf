function SerivceViewModel(resourceID, resourceType) {
    var self = this;

    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 275;
    var $sourceMethods = $("#sourceMethods");
    var $inputsTable = $("#inputsTable");
    
    self.saveUri = "Service/Services/Save";
    
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    
    self.data = {
        resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable(resourceType),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        source: ko.observable(),
        methodName: ko.observable(""),
        methodParameters: ko.observableArray(),
        methodOutputs: ko.observableArray(),
        testData: ko.observable(""),
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
        $inputsTable.hide();
        $.post("Service/Services/Methods" + window.location.search, ko.toJSON(newValue), function (result) {
            self.sourceMethods(result);
            self.sourceMethods.sort(utils.nameCaseInsensitiveSort);
        });
    });   

    self.title = ko.observable("New Service");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });
    self.saveTitle = ko.computed(function () {
        return "<b>" + self.title() + "</b>";
    });
    self.helpDictionaryID = "Service";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    
    self.isFormValid = ko.computed(function () {
        var isEnabled = self.data.source() !== ""
            && self.data.methodName() !== "";
        return isEnabled;
    });

    utils.registerSelectHandler($sourceMethods, function (selectedItem) {
        self.data.methodName(selectedItem.Name);
        self.data.methodParameters(selectedItem.Parameters);
        $inputsTable.show();
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
        var args = ko.toJSON(self.data);
        $.post("Service/Services/Test" + window.location.search, args, function (result) {
            self.data.testData(result);
        });
        $("#testDialogContainer").dialog("open");
    };

    self.cancel = function () {
        Dev2Awesomium.Cancel();
        return true;
    };

    self.save = function () {        
        $("#saveForm").dialog("open");
    };

    self.initialize = function () {
        $inputsTable.hide();
    };
    
    self.initialize();
    self.load();
};
