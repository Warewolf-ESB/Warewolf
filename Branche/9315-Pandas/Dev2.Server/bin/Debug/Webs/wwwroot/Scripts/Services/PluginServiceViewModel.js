function PluginServiceViewModel(saveContainerID, resourceID, sourceName) {
    var self = this;

    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 343;
    var $sourceMethods = $("#sourceMethods");
    var $actionInspectorDialog = $("#actionInspectorDialog");
    var $tabs = $("#tabs");

    self.$pluginSourceDialogContainer = $("#pluginSourceDialogContainer");

    // TODO: reinstate this check when all resources use an ID 
    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    // TODO: remove this check: resourceID is either a GUID or a name to cater for legacy stuff
    //self.isEditing = resourceID ? !(resourceID === "" || $.Guid.IsEmpty(resourceID)) : false;

    self.data = {
        resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable("PluginService"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),
        
        source: ko.observable(),
        method: {
            Name: ko.observable(""),
            SourceCode: ko.observable(""),
            Parameters: ko.observableArray()
        },
        recordsets: ko.observableArray(),
        recordset: {
            Name: ko.observable(""),
            Fields: ko.observableArray(),
            Records: ko.observableArray(),
            HasErrors: ko.observable(false),
            ErrorMessage: ko.observable("")
        }
    };
    
    self.sources = ko.observableArray();
    self.fullnames = ko.observableArray();
    self.fullNameSelected = ko.observable();
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
        if (self.hasTestResults()) {
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

    self.data.source.subscribe(function (newValue) {
        self.data.recordsets([]);
        self.loadFullNames(newValue);
    });

    self.fullNameSelected.subscribe(function (newValue) {
        self.data.recordsets([]);
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
        self.hasTestResults(false);
        self.hasTestResultRecords(false);
        self.data.recordsets([]);
    });

    self.getJsonData = function () {
        // Don't need to send records back!
        self.data.source().FullName = self.fullNameSelected().FullName;

        self.data.recordsets([]);
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
    

    self.selectFullNameByID = function (theID) {
        theID = theID.toLowerCase();
        var found = false;
        $.each(self.fullnames(), function (index, fullName) {
            if (fullName.ResourceID.toLowerCase() === theID) {
                found = true;
                self.data.fullname(fullName); // This will trigger a call to loadMethods
                return false;
            }
            return true;
        });
        return found;
    };

    self.selectFullNameByName = function (theName) {
        var found = false;
        if (theName) {
            theName = theName.toLowerCase();
            $.each(self.fullnames(), function (index, fullName) {
                if (fullName.fullname.toLowerCase() === theName) {
                    found = true;
                    self.data.fullname(fullName); // This will trigger a call to loadMethods
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
            resourceType: "PluginService"
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
                self.data.recordset.Name(result.Recordset.Name);
                self.data.recordset.Fields(result.Recordset.Fields);
            }

            self.title(self.isEditing ? "Edit Plugin Service - " + result.ResourceName : "New Plugin Service");
        });
    };

    self.loadSources = function (callback) {
        $.post("Service/Resources/Sources" + window.location.search, ko.toJSON({ resourceType: "PluginSource" }), function (result) {
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
        
        $.post("Service/Services/PluginMethods" + window.location.search, ko.toJSON(source), function (result) {
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
    
    self.loadFullNames = function (source,callback) {
        $.post("Service/Services/PluginFullNames" + window.location.search, ko.toJSON(source), function (result) {
            self.fullnames(result);
            self.fullnames.sort(utils.fullNameCaseInsensitiveSort);
        }).done(function () {
            
            if (callback) {
                callback();
            }
           
            
        });
    };

    self.showTab = function (tabIndex) {
        $tabs.tabs("option", "active", tabIndex);
    };
    
    self.showSource = function (theSourceName) {
        // 
        // pluginSourceViewModel is a global variable instantiated in PluginSource.htm
        //
        pluginSourceViewModel.showDialog(theSourceName, function (result) {
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
        $.post("Service/Services/PluginTest" + window.location.search, self.getJsonData(), function (result) {
            self.isTestResultsLoading(false);
            self.hasTestResultRecords(result.length > 0);
            self.hasTestResults(true);
            self.data.recordsets([]);
            result.forEach(function (entry) {
                $.each(entry.Fields, function (index, field) {
                    field.Alias = ko.observable(field.Alias);
                });
                var rs = {
                    Name: ko.observable(entry.Name),
                    DisplayName: ko.observable(entry.Name),
                    Fields: entry.Fields,
                    Records: ko.observableArray(entry.Records),
                    HasErrors: ko.observable(entry.HasErrors),
                    ErrorMessage: ko.observable(entry.ErrorMessage),
                    CanDisplayName: ko.computed(function () {
                        var b = entry.Name !== null;
                        return b;
                    })
                };
                
                rs.Name.subscribe(function (newValue) {
                    if (newValue.length > 20) {
                        var dispValue = "..." + newValue.substr(newValue.length - 17, newValue.length);
                        rs.DisplayName(dispValue);
                    } else {
                        rs.DisplayName(newValue);
                    }
                    $.each(rs.Fields, function (index, field) {
                        var alias = newValue + field.Name;
                        if (newValue.indexOf("()") == -1) {
                            field.Alias(alias);
                        } else {
                            alias = newValue +"."+ field.Name;
                            field.Alias(alias);
                        }
                    });
                });
                self.data.recordsets.push(rs);
            });
            
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

    self.load();
};



PluginServiceViewModel.create = function (pluginServiceContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();
    $("#tabs").tabs();

    var pluginServiceViewModel = new PluginServiceViewModel(saveContainerID, getParameterByName("rid"), getParameterByName("sourceName"));

    ko.applyBindings(pluginServiceViewModel, document.getElementById(pluginServiceContainerID));

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

    // inject pluginSourceDialog
    pluginServiceViewModel.$pluginSourceDialogContainer.load("Views/Sources/PluginSource.htm", function () {
        // 
        // pluginSourceViewModel is a global variable instantiated in PluginSource.htm
        //
        pluginSourceViewModel.createDialog(pluginServiceViewModel.$pluginSourceDialogContainer);
    });
    return PluginServiceViewModel;
};