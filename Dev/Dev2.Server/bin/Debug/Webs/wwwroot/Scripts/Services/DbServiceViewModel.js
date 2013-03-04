function DbServiceViewModel(saveContainerID, resourceID, sourceName) {
    var self = this;
    
    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 343;
    var $sourceMethods = $("#sourceMethods");
    var $actionInspectorDialog = $("#actionInspectorDialog");
    var $tabs = $("#tabs");

    self.$dbSourceDialogContainer = $("#dbSourceDialogContainer");
    
    // TODO: reinstate this check when all resources use an ID 
    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    // TODO: remove this check: resourceID is either a GUID or a name to cater for legacy stuff
    //self.isEditing = resourceID ? !(resourceID === "" || $.Guid.IsEmpty(resourceID)) : false;

    self.data = {
        resourceID: ko.observable(""),
        //resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
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
     
    self.methodNameChanged = ko.observable(false);
    self.hasMethod = ko.computed(function () {
        return self.data.method.Name() !== "";
    });
    self.hasTestResults = ko.observable(false);    
    self.hasTestResultRecords = ko.observable(false);
    self.hasOutputs = ko.computed(function () {
        if (self.isEditing) {
            return !self.methodNameChanged() || self.hasTestResults();
        }
        return self.hasTestResults();
    });
    self.isFormValid = ko.computed(function () {
        if (self.hasOutputs()) {
            var isRecordsetNameOptional = self.data.recordset.Records().length <= 1;
            return isRecordsetNameOptional ? true : self.data.recordset.Name() !== "";
        }
        return false;
    });

    self.isSourceMethodsLoading = ko.observable(false);
    self.isTestResultsLoading = ko.observable(false);
    self.isTestEnabled = ko.computed(function () {
        return self.hasMethod() && !self.isTestResultsLoading();
    });
    self.isEditSourceEnabled = ko.computed(function () {
        return self.data.source();
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
    });

    self.getJsonData = function () {
        // Don't need to send records back!
        self.data.recordset.Records([]);
        self.data.recordset.HasErrors(false);
        self.data.recordset.ErrorMessage("");
        return ko.toJSON(self.data);
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
    
    self.updateRecordset = function (name, fields, records, hasErrors, errorMessage) {
        self.data.recordset.Name(name ? name.replace(".", "_") : "");
        self.data.recordset.Fields(fields ? fields : []);
        self.data.recordset.Records(records ? records : []);
        self.data.recordset.HasErrors(hasErrors ? hasErrors : false);
        self.data.recordset.ErrorMessage(errorMessage ? errorMessage : "");        
    };
    
    self.data.source.subscribe(function (newValue) {
        self.loadMethods(newValue);
    });
    
    self.data.method.Name.subscribe(function (newValue) {
        self.methodNameChanged(true);
        self.updateRecordset(newValue);
        self.hasTestResults(false);
        self.hasTestResultRecords(false);
    });

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
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            var found = sourceName && self.selectSourceByName(sourceName);           
            if (!found) {
                utils.IsNullOrEmptyGuid(result.Source.ResourceID)
                    ? self.selectSourceByName(result.Source.ResourceName)
                    : self.selectSourceByID(result.Source.ResourceID);
            }
            
            // MUST set these AFTER setting data.source otherwise they will be blanked!
            if (result.Method) {
                self.data.method.Name(result.Method.Name);
                self.data.method.Parameters(result.Method.Parameters);
            }
            if (result.Recordset) {
                self.updateRecordset(result.Recordset.Name, result.Recordset.Fields);
            }

            self.methodNameChanged(false); // reset so that we can track user changes!
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

        self.updateRecordset();

        self.sourceMethods([]);
        self.sourceMethodSearchTerm("");
        self.hasTestResults(false);
        self.hasTestResultRecords(false);
        self.isSourceMethodsLoading(true);
        
        $.post("Service/Services/DbMethods" + window.location.search, ko.toJSON(source), function (result) {
            self.isSourceMethodsLoading(false);
            self.sourceMethods(result.sort(utils.nameCaseInsensitiveSort));
            var methodName = self.data.method.Name();
            if (methodName !== "" && result.length > 0) {
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
            self.updateRecordset(result.Name, result.Fields, result.Records, result.HasErrors, result.ErrorMessage);
        });
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.saveViewModel = SaveViewModel.create("Service/Services/Save", self, saveContainerID);

    self.save = function () {
        self.saveViewModel.showDialog(true);
    };    

    self.showSource = function (theSourceName) {
        // 
        // dbSourceViewModel is a global variable instantiated in DbSource.htm
        //
        dbSourceViewModel.showDialog(theSourceName, function (result) {
            var id = result.ResourceID.toLowerCase();
            $.each(self.sources(), function (index, source) {
                if (source.ResourceID.toLowerCase() === id) {
                    self.sources.splice(index, 1, result);
                    return false;
                }
                return true;
            });
            self.sources.push(result);
            self.sources.sort(utils.resourceNameCaseInsensitiveSort);            
            self.data.source(result); // This will trigger a call to loadMethods
        });
    };
    
    self.load();    
};


DbServiceViewModel.create = function (dbServiceContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();
    $("#tabs").tabs();

    var dbServiceViewModel = new DbServiceViewModel(saveContainerID, getParameterByName("rid"), getParameterByName("sourceName"));

    ko.applyBindings(dbServiceViewModel, document.getElementById(dbServiceContainerID));
    

    $("#actionInspectorDialog").dialog({
        resizable: false,
        autoOpen: false,
        modal: true,
        position: utils.getDialogPosition(),
        width: 700,
        buttons: {
            "Close": function () {
                $(this).dialog("close");
            }
        }
    });

    // inject DbSourceDialog
    dbServiceViewModel.$dbSourceDialogContainer.load("Views/Sources/DbSource.htm", function () {
        // 
        // dbSourceViewModel is a global variable instantiated in DbSource.htm
        //
        dbSourceViewModel.createDialog(dbServiceViewModel.$dbSourceDialogContainer);
    });
    return dbServiceViewModel;
};