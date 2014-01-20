// Make this available to chrome debugger
//@ sourceURL=DbServiceViewModel.js  

function DbServiceViewModel(saveContainerID, resourceID, sourceName, environment, resourcePath) {
    var self = this;
    
    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 343;
    var $sourceMethods = $("#sourceMethods");
    var $actionInspectorDialog = $("#actionInspectorDialog");
    var $tabs = $("#tabs");
    var $recsetNote = $("#recordsetNameNote");
    var $recsetName = $("#recordsetName");

    self.$dbSourceDialogContainer = $("#dbSourceDialogContainer");
    
    self.currentEnvironment = ko.observable(environment); //2013.06.08: Ashley Lewis for PBI 9458 - Show server
    self.titleSearchString = "Database Service";

    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    self.isLoading = false; // BUG 9772 - 2013.06.19 - TWR : added
    self.inputMappingLink = "Please select an action first (Step 2)";
    self.outputMappingLink = "Please run a test first (Step 3)";
    self.isReadOnly = false;

    self.data = {
        resourceID: ko.observable(""),
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
            Alias: ko.observable(""),
            Fields: ko.observableArray(),
            Records: ko.observableArray(),
            HasErrors: ko.observable(false),
            ErrorMessage: ko.observable("")
        }
    };
    self.data.recordset.Alias.subscribe(function (newValue) {       
        $.each(self.data.recordset.Fields(), function (index, field) {
            field.RecordsetAlias = newValue;
        });
    });
    
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

    self.clearFilter = function () {
        self.sourceMethodSearchTerm("");
    };
    self.hasFilter = ko.computed(function () {
        return self.sourceMethodSearchTerm() !== "";
    });
    
    self.canReload = ko.computed(function () {
        return (self.sourceMethods().length >= 1);
    });

    self.reloadActions = function() {
        // force a reload of the current source ;)
        self.data.source().ReloadActions = true;
        self.loadMethods(self.data.source(), true);
        self.data.source().ReloadActions = false;
    };

    utils.makeClearFilterButton("clearDbServiceFilterButton");

    self.methodNameChanged = ko.observable(false);
    self.hasMethod = ko.computed(function () {
        return self.data.method.Name() !== "";
    });

    self.hasInputs = ko.computed(function () {
        return self.hasMethod();
    });
    
    self.hasTestResults = ko.observable(false);    
    self.hasTestResultRecords = ko.observable(false);
    self.hasOutputs = ko.computed(function () {
        if (self.isEditing) {
            return !self.methodNameChanged() || self.hasTestResults();
        }
        return self.hasTestResults();
    });
    self.recsetNote = ko.observable("<b>Note:</b> Recordset name is optional if only returning 1 record.");
    self.SetRecsetError = function(setErrorStateTo) {
        if (setErrorStateTo) {
            self.recsetNote("<b>Note:</b> Recordset name cannot be the same as an Input name.");
            if ($recsetNote.length > 0 && $recsetName.length > 0) {
                $recsetName.addClass("error");
                $recsetNote.addClass("error");
            }
        } else {
            self.recsetNote("<b>Note:</b> Recordset name is optional if only returning 1 record.");
            if ($recsetNote.length > 0 && $recsetName.length > 0) {
                $recsetName.removeClass("error");
                $recsetNote.removeClass("error");
            }
        }
    };
    self.doMethodParamsContain = function (toFind) {
        var result = false;
        if (toFind) {
            self.SetRecsetError(false);
            for (var i = 0; i < self.data.method.Parameters().length; i++) {
                if (self.data.method.Parameters()[i].Name && self.data.method.Parameters()[i].Name.toLowerCase() == toFind.toLowerCase()) {
                    result = true;
                    self.SetRecsetError(true);
                }
            }
        }
        return result;
    };
    self.isFormValid = ko.computed(function () {
        if (self.isReadOnly) {
            return false;
        }

        if (self.hasOutputs() && !self.doMethodParamsContain(self.data.recordset.Name())) {
            var isRecordsetNameOptional = self.data.recordset.Records().length <= 1;
            return isRecordsetNameOptional ? true : self.data.recordset.Name() !== "";
        }
        return false;
    });

    self.isSourceMethodsLoading = ko.observable(false);
    self.isTestResultsLoading = ko.observable(false);
    self.isTestEnabled = ko.computed(function () {
        return self.hasMethod() && !self.isTestResultsLoading() && !self.isReadOnly;
    });
    self.isEditSourceEnabled = ko.computed(function () {
        return self.data.source() && !self.isReadOnly;
    });
    
    self.title = ko.observable("New Service");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });
    self.saveTitle = ko.computed(function () {
        return "<b>" + self.title() + "</b>";
    });

    utils.isReadOnly(resourceID, function (isReadOnly) {
        self.isReadOnly = isReadOnly;
        
        if (self.isReadOnly) {
            $('#reloadDbServiceButton').hide();
        }else{
            utils.makeReloadButton("reloadDbServiceButton");
        }

        utils.registerSelectHandler($sourceMethods, function (selectedItem) {
            self.data.method.Name(selectedItem.Name);
            self.data.method.SourceCode(utils.toHtml(selectedItem.SourceCode));
            self.data.method.Parameters(selectedItem.Parameters);
            utils.toggleUIReadOnlyState(self.isReadOnly);
        }, self.isReadOnly);
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
    
    self.selectSourceByName = function (theName) {
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
        self.data.recordset.Name(name ? self.formatRecordsetName(name) : "");
        self.data.recordset.Fields(fields ? fields : []);
        self.data.recordset.Records(records ? records : []);
        self.data.recordset.HasErrors(hasErrors ? hasErrors : false);
        self.data.recordset.ErrorMessage(errorMessage ? errorMessage : "");        
        
        // MUST do this last as it will update field aliases
        var recordsetAlias = fields && fields.length > 0 ? fields[0].RecordsetAlias : null;
        if (!recordsetAlias) {
            recordsetAlias = self.data.recordset.Name();
        }
        self.data.recordset.Alias(recordsetAlias);

        $.each(self.data.recordset.Fields(), function (index, field) {
            if (!field.RecordsetAlias) {
                field.RecordsetAlias = recordsetAlias;
            }
        });
    };

    self.formatRecordsetName = function(name) {
        var result = name.replace(".", "_");
        if (self.doMethodParamsContain(result)) {
            result = result + "_recordset";
        }
        if ($("#recordsetNameNote").length > 0) {
            $("#recordsetNameNote")[0].innerHTML = "<b>Note:</b> Recordset name is optional if only returning 1 record.";
        }
        return result;
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
        self.isLoading = true; // BUG 9772 - 2013.06.19 - TWR : added
        self.loadSources(
            function() {
                self.loadService();
                utils.toggleUIReadOnlyState(self.isReadOnly);
            });
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
            
            if (!result.ResourcePath && resourcePath) {
                self.data.resourcePath(resourcePath);
            }

            var found = sourceName && self.selectSourceByName(sourceName);           
            if (!found) {
                // BUG 9772 - 2013.06.19 - TWR : added
                if (!utils.IsNullOrEmptyGuid(result.Source.ResourceID)) {
                    found = self.selectSourceByID(result.Source.ResourceID);
                }
                if(!found){
                    if (result.Source.ResourceName) {
                        self.selectSourceByName(result.Source.ResourceName);
                    } else {
                        self.isLoading = false;
                    }
                }
            }
            
            // MUST set these AFTER setting data.source otherwise they will be blanked!
            if (result.Method) {
                self.data.method.Name(result.Method.Name);
                self.data.method.Parameters(result.Method.Parameters);
                utils.toggleUIReadOnlyState(self.isReadOnly);
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

    self.clearSelectedMethod = function () {
        if (!self.isLoading) {
            self.data.method.Name("");
            self.data.method.SourceCode("");
            self.data.method.Parameters.removeAll();
        }
    };

    self.loadMethodsTime = 0;

    self.loadMethods = function (source, isReload, doneHandler) {
        
        if (isReload) {
            self.sourceMethods.removeAll();
            self.sourceMethodSearchTerm("");
            self.isSourceMethodsLoading(true);
            self.hasTestResults(false);
            self.hasTestResultRecords(false);
        } else {
            self.clearSelectedMethod();
            self.updateRecordset();
            self.sourceMethods.removeAll();
            self.sourceMethodSearchTerm("");
            self.isSourceMethodsLoading(true);
            self.hasTestResults(false);
            self.hasTestResultRecords(false);
        }
        
        var jsonData = ko.toJSON(source);

        utils.postTimestamped(self, "loadMethodsTime", "Service/Services/DbMethods", jsonData, function(result) {
            self.isSourceMethodsLoading(false);
            self.sourceMethods(result.sort(utils.nameCaseInsensitiveSort));
            var methodName = self.data.method.Name();


            if (methodName !== "" && result.length > 0) {
                $.each(self.sourceMethods(), function(index, method) {
                    if (method.Name.toLowerCase() === methodName.toLowerCase()) {
                        self.data.method.SourceCode(utils.toHtml(method.SourceCode));
                        return false;
                    }
                    return true;
                });


                utils.selectAndScrollToListItem(methodName, $sourceMethodsScrollBox, $sourceMethodsScrollBoxHeight);
            }
        }, function() {
            self.isLoading = false; // BUG 9772 - 2013.06.19 - TWR : added
            if (doneHandler) {
                doneHandler();
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
        var regex = new RegExp("__COMMA__", "g");
        $.post("Service/Services/DbTest" + window.location.search, self.getJsonData(), function (result) {

            try {
                // clean things up a bit ;)
                for (var i = 0; i < result.Records.length; i++) {
                    for (var q = 0; q < result.Records[i].Cells.length; q++) {
                        result.Records[i].Cells[q].Value = result.Records[i].Cells[q].Value.replace(regex, ",");
                    }
                }
            } catch(e) {}

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

        // if new do old action, CANNOT USE Guid.IsEmpty as this does not work!
        if (self.data.resourceID() == "00000000-0000-0000-0000-000000000000") {
            self.saveViewModel.showDialog(true);
        } else {
            // else use new action ;)
            self.saveViewModel.IsDialogLess = true;
            self.saveViewModel.save();
        }
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
    $("button").button();
    $("#tabs").tabs();

    var dbServiceViewModel = new DbServiceViewModel(saveContainerID, getParameterByName("rid"), getParameterByName("sourceName"), utils.decodeFullStops(getParameterByName("envir")), getParameterByName("path"));

    ko.applyBindings(dbServiceViewModel, document.getElementById(dbServiceContainerID));
    

    $("#actionInspectorDialog").dialog({
        resizable: false,
        autoOpen: false,
        modal: true,
        position: utils.getDialogPosition(),
        width: 700,
        buttons: {
            "Close": {
                text: "Close",
                id: "dialogCloseBtn",
                click: function () {
                    $(this).dialog("close");
                }
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