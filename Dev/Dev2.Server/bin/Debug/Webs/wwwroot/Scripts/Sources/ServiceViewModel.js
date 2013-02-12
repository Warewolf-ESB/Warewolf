function SerivceViewModel(resourceID, resourceType) {
    var self = this;

    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 275;
    var $sourceMethods = $("#sourceMethods");
    var $inputsTable = $("#inputsTable");
    var $actionInspectorDialog = $("#actionInspectorDialog");
    var $tabs = $("#tabs");

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
        methodRecordset: {
            Name: ko.observable(""),
            Fields: ko.observableArray(),
            Records: ko.observableArray()
        }
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
    
    self.hasMethodName = ko.computed(function () {
        return self.data.methodName() !== "";
    });
    self.hasMethodResults = ko.observable(false);
    self.isFormValid = ko.computed(function () {
        return self.hasMethodName() && self.hasMethodResults();
    });
    
    self.data.source.subscribe(function (newValue) {
        self.data.methodName("");
        self.data.methodParameters([]);
        self.sourceMethodSearchTerm("");
        self.hasMethodResults(false);
        self.data.methodRecordset.Name("");
        self.data.methodRecordset.Fields([]);
        self.data.methodRecordset.Records([]);

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
    };

    self.showTab = function (tabIndex) {
        $tabs.tabs("option", "active", tabIndex);
    };
    
    self.editSource = function () {
    };

    self.newSource = function () {
    };
    
    self.showAction = function () {
        $actionInspectorDialog.dialog("open");
    };
    
    self.testAction = function () {
        var args = ko.toJSON(self.data);
        $.post("Service/Services/Test" + window.location.search, args, function(result) {
            self.hasMethodResults(true);          
            self.data.methodRecordset.Name(result.Name);
            self.data.methodRecordset.Fields(result.Fields);
            self.data.methodRecordset.Records(result.Records);
        });        
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
