function DbSerivceViewModel(resourceID, resourceType) {
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
        method: {
            Name: ko.observable(""),
            SourceCode: ko.observable(""),
            Parameters: ko.observableArray()
        },
        recordset: {
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
    
    self.hasMethod = ko.computed(function () {
        return self.data.method.Name() !== "";
    });
    self.hasTestResults = ko.observable(false);
    self.isFormValid = ko.computed(function () {
        // TODO: FIX isFormValid
        var isRecordsetNameOptional = self.data.recordset.Records().length <= 1;
        console.log("isRecordsetNameOptional: " + isRecordsetNameOptional);
        return isRecordsetNameOptional ? true : self.data.recordset.Name !== "";
    });
    
    self.data.source.subscribe(function (newValue) {
        self.sourceMethodSearchTerm("");
        self.hasTestResults(false);

        self.data.method.Name("");
        self.data.method.SourceCode("");
        self.data.method.Parameters([]);

        self.data.recordset.Name("");
        self.data.recordset.Fields([]);
        self.data.recordset.Records([]);

        $inputsTable.hide();
        self.loadMethods(newValue);
    });   

    self.title = ko.observable("New Service");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });
    self.saveTitle = ko.computed(function () {
        return "<b>" + self.title() + "</b>";
    });

    utils.registerSelectHandler($sourceMethods, function (selectedItem) {
        self.data.method.Name(selectedItem.Name);
        self.data.method.SourceCode(utils.toHtml(selectedItem.SourceCode));
        self.data.method.Parameters(selectedItem.Parameters);

        self.data.recordset.Name(selectedItem.Name);

        $inputsTable.show();
    });

    self.getJsonData = function () {
        // Don't need to send records back!
        self.data.recordset.Records([]);
        return ko.toJSON(self.data);
    };
    
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

    self.loadMethods = function (source) {
        $.post("Service/Services/DbMethods" + window.location.search, ko.toJSON(source), function (result) {
            self.sourceMethods(result.sort(utils.nameCaseInsensitiveSort));
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
    
    self.testAction = function() {
        $.post("Service/Services/Test" + window.location.search, self.getJsonData(), function (result) {
            self.hasTestResults(true);          
            self.data.recordset.Name(result.Name);
            self.data.recordset.Fields(result.Fields);
            self.data.recordset.Records(result.Records);
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
