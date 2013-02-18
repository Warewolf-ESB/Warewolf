function DbSerivceViewModel(resourceID, sourceName) {
    var self = this;

    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 343;
    var $sourceMethods = $("#sourceMethods");
    var $actionInspectorDialog = $("#actionInspectorDialog");
    var $tabs = $("#tabs");

    self.saveUri = "Service/Services/Save";
        
    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);    

    self.data = {
        resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable("DbService"),
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
            Records: ko.observableArray(),
            HasErrors: ko.observable(false),
            ErrorMessage: ko.observable("")
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
    self.hasTestResultRecords = ko.observable(false);
    self.isFormValid = ko.computed(function () {
        // TODO: FIX isFormValid
        var isRecordsetNameOptional = self.data.recordset.Records().length <= 1;
        //console.log("isRecordsetNameOptional: " + isRecordsetNameOptional);
        return isRecordsetNameOptional ? true : self.data.recordset.Name !== "";
    });

    self.isSourceMethodsLoading = ko.observable(false);
    self.isTestResultsLoading = ko.observable(false);
    self.isTestEnabled = ko.computed(function () {
        return self.hasMethod() && !self.isTestResultsLoading();
    });

    self.data.source.subscribe(function (newValue) {
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
    });

    self.getJsonData = function () {
        // Don't need to send records back!
        self.data.recordset.Records([]);
        self.data.recordset.HasErrors(false);
        self.data.recordset.ErrorMessage("");
        return ko.toJSON(self.data);
    };

    self.selectSource = function (theSource) {
        var found = utils.IsNullOrEmptyGuid(theSource.ResourceID)
            ? self.selectSourceByName(theSource.ResourceName)
            : self.selectSourceByID(theSource.ResourceID);
        if (!found && sourceName) {
            self.selectSourceByName(sourceName);
        }
    };
    
    self.selectSourceByID = function (theID) {
        theID = theID.toLowerCase();
        var found = false;
        $.each(self.sources(), function (index, source) {
            if (source.ResourceID.toLowerCase() === theID) {
                found = true;               
                self.data.source(source); // This will trigger a call to loadMethods
                return false;
            }
            return true;
        });
        return found;
    };
    self.selectSourceByName = function(theName) {
        var found = false;
        if (theName) {
            theName = theName.toLowerCase();
            $.each(self.sources(), function(index, source) {
                if (source.ResourceName.toLowerCase() === theName) {
                    found = true;
                    self.data.source(source); // This will trigger a call to loadMethods
                    return false;
                }
                return true;
            });
        }
        return found;
    };

    self.load = function () {
        self.loadSources(
            self.loadService());
    };
    
    self.loadService = function () {
        var args = ko.toJSON({
            resourceID: resourceID,
            resourceType: "DbService"
        });
        $.post("Service/Services/Get" + window.location.search, args, function (result) {
            console.log(result);
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            self.selectSource(result.Source);

            // MUST set these AFTER setting data.source otherwise they will be blanked!
            if (result.Method) {
                self.data.method.Name(result.Method.Name);
                self.data.method.Parameters(result.Method.Parameters);
            }
            if (result.Recordset) {
                self.data.recordset.Name(result.Recordset.Name);
                self.data.recordset.Fields(result.Recordset.Fields);
            }

            self.title(self.isEditing ? "Edit Database Service - " + result.ResourceName : "New Database Service");
        });
    };

    self.loadSources = function (callback) {
        $.post("Service/Resources/Sources" + window.location.search, ko.toJSON({ resourceType: "DbSource" }), function (result) {
            self.sources(result);
            self.sources.sort(utils.resourceNameCaseInsensitiveSort);
        }).done(function () {
            if (callback) {
                callback();
            }
        });
    };
    
    self.loadMethods = function (source) {
        self.data.method.Name("");
        self.data.method.SourceCode("");
        self.data.method.Parameters([]);

        self.data.recordset.Name("");
        self.data.recordset.Fields([]);
        self.data.recordset.Records([]);

        self.sourceMethods([]);
        self.sourceMethodSearchTerm("");
        self.hasTestResults(false);
        self.hasTestResultRecords(false);
        self.isSourceMethodsLoading(true);
        
        $.post("Service/Services/DbMethods" + window.location.search, ko.toJSON(source), function (result) {
            self.isSourceMethodsLoading(false);
            self.sourceMethods(result.sort(utils.nameCaseInsensitiveSort));
            var methodName = self.data.method.Name();
            if (methodName !== "") {
                utils.selectAndScrollToListItem(methodName, $sourceMethodsScrollBox, $sourceMethodsScrollBoxHeight);

                $.each(self.sourceMethods(), function (index, method) {
                    if (method.Name.toLowerCase() === methodName.toLowerCase()) {
                        self.data.method.SourceCode(utils.toHtml(method.SourceCode));
                        return false;
                    }
                    return true;
                });
            }
        });
    };

    self.showTab = function (tabIndex) {
        $tabs.tabs("option", "active", tabIndex);
    };

    self.showSource = function(sourceName) {
        var args = ko.toJSON({
            ResourceName: sourceName
        });
        var returnUri = "" + window.location;
        try {
            Dev2Awesomium.NavigateTo("", args, returnUri);
        } catch (e) {
            alert(e);
        } 
        return true;
    };
    
    self.editSource = function () {
        return self.showSource(self.data.source().ResourceName);
    };

    self.newSource = function () {
        return self.showSource("");
    };
    
    self.showAction = function () {
        $actionInspectorDialog.dialog("open");
    };
    
    self.testAction = function () {
        self.isTestResultsLoading(true);
        $.post("Service/Services/DbTest" + window.location.search, self.getJsonData(), function (result) {
            self.isTestResultsLoading(false);
            self.hasTestResultRecords(result.Records.length > 0);
            self.hasTestResults(!result.HasErrors);
            self.data.recordset.Name(result.Name);
            self.data.recordset.Fields(result.Fields);
            self.data.recordset.Records(result.Records);
            self.data.recordset.HasErrors(result.HasErrors);
            self.data.recordset.ErrorMessage(result.ErrorMessage);
        });
    };

    self.cancel = function () {
        Dev2Awesomium.Cancel();
        return true;
    };

    self.save = function () {        
        $("#saveForm").dialog("open");
    };

    self.load();
};
