// Make this available to chrome debugger
//@ sourceURL=PluginServiceViewModel.js  

function PluginServiceViewModel(saveContainerID, resourceID, sourceName, environment, resourcePath) {
    var self = this;

    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 343;
    var $sourceMethods = $("#sourceMethods");
    var $actionInspectorDialog = $("#actionInspectorDialog");
    var $tabs = $("#tabs");

    self.$pluginSourceDialogContainer = $("#pluginSourceDialogContainer");

    self.currentEnvironment = ko.observable(environment); //2013.06.08: Ashley Lewis for PBI 9458 - Show server
    self.titleSearchString = "Plugin Service";

    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    self.isLoading = false;  // BUG 9500 - 2013.05.31 - TWR : added
    self.inputMappingLink = "Please select an saveaction first (Step 3)";
    self.outputMappingLink = "Please run a test first (Step 4)";
    self.isReadOnly = false;

    self.data = new ServiceData(self.isEditing ? resourceID : $.Guid.Empty(), "PluginService");
    self.data.namespace = ko.observable("");

    self.sources = ko.observableArray();
    self.namespaces = ko.observableArray();
    self.namespaceSelected = ko.observable();
    self.namespaceSelected.subscribe(function (newValue) {
        if (newValue) {
            self.data.namespace(newValue.FullName);
            self.loadMethods();
        }
    });

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

    self.hasInputs = ko.computed(function () {
        return self.hasMethod();
    });

    self.hasTestResults = ko.observable(false);
    self.hasTestResultRecords = ko.observable(false);
    self.testErrorMessage = ko.observable("");
    self.hasTestErrors = ko.observable(false);
    self.isFormValid = ko.computed(function () {
        return self.hasTestResults() && !self.isReadOnly;
    });

    self.isSourceMethodsLoading = ko.observable(false);
    self.isTestResultsLoading = ko.observable(false);
    self.isTestEnabled = ko.computed(function () {
        if (self.isReadOnly) {
            return false;
        }
        return self.hasMethod() && !self.isTestResultsLoading();
    });
    self.isEditSourceEnabled = ko.computed(function () {
        if (self.isReadOnly) {
            return false;
        }
        return self.data.source();
    });

    self.data.source.subscribe(function (newValue) {
        self.loadNamespaces(newValue);
    });

    self.data.method.Name.subscribe(function () {
        self.pushRecordsets([]);
    });

    self.clearSelectedMethod = function () {
        if (!self.isLoading) {
            self.data.method.Name("");
            self.data.method.SourceCode("");
            self.data.method.Parameters.removeAll();
            self.pushRecordsets([]);
        }
    };

    self.title = ko.observable("New Service");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });
    self.saveTitle = ko.computed(function () {
        return "<b>" + self.title() + "</b>";
    });

    utils.isReadOnly(resourceID, function (isReadOnly) {
        self.isReadOnly = isReadOnly;
        utils.registerSelectHandler($sourceMethods, function (selectedItem) {
            self.data.method.Name(selectedItem.Name);
            self.data.method.SourceCode(utils.toHtml(selectedItem.SourceCode));
            self.data.method.Parameters(selectedItem.Parameters);
            self.hasTestResults(false);
            self.hasTestResultRecords(false);
        }, self.isReadOnly);
    });
    
    self.getJsonData = function () {
        // BUG 9500 - 2013.05.31 - TWR : fixed
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
            $.each(self.sources(), function (index, source) {
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

    self.selectNamespaceByName = function (theName) {
        var found = false;
        if (theName) {
            theName = theName.toLowerCase();
            $.each(self.namespaces(), function (index, namespaceItem) {
                // BUG 9500 - 2013.05.31 - TWR : fixed
                if (namespaceItem.FullName.toLowerCase() === theName) {
                    found = true;
                    self.namespaceSelected(namespaceItem); // This will trigger a call to loadMethods
                    return false;
                }
                return true;
            });
        }
        return found;
    };


    self.pushRecordsets = function (result) {
        var hasResults = result.length > 0;

        self.hasTestResultRecords(hasResults);
        self.hasTestResults(hasResults);

        recordsets.pushResult(self.data.recordsets, result);
    };

    self.load = function () {
        self.isLoading = true; // BUG 9500 - 2013.05.31 - TWR : added
        self.loadSources(function () {
            self.loadService();
        });
    };

    self.loadService = function () {
        var args = ko.toJSON({
            resourceID: resourceID,
            resourceType: "PluginService"
        });
        $.post("Service/PluginServices/Get" + window.location.search, args, function (result) {
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            if (!result.ResourcePath && resourcePath) {
                self.data.resourcePath(resourcePath);
            }

            // BUG 9500 - 2013.05.31 - TWR : added           
            self.data.namespace(result.Namespace);

            var found = sourceName && self.selectSourceByName(sourceName);
            if (!found) {
                if (!utils.IsNullOrEmptyGuid(result.Source.ResourceID)) {
                    self.selectSourceByID(result.Source.ResourceID);
                } else {
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
            }

            // BUG 9500 - 2013.05.31 - TWR : added           
            self.pushRecordsets(result.Recordsets);

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

    self.loadMethods = function () {
        // BUG 9500 - 2013.05.31 - TWR : DO NOT empty self.data.method properties otherwise action selection does not work!!!
        self.clearSelectedMethod();
        self.sourceMethods.removeAll();
        self.sourceMethodSearchTerm("");
        self.isSourceMethodsLoading(true);

        // BUG 9500 - 2013.05.31 - TWR : PluginMethods changed to use PluginService as args
        $.post("Service/PluginServices/Methods" + window.location.search, self.getJsonData(), function (result) {
            self.isSourceMethodsLoading(false);
            self.sourceMethods(result.sort(utils.nameCaseInsensitiveSort));
            var methodName = self.data.method.Name();
            if (methodName !== "") {
                utils.selectAndScrollToListItem(methodName, $sourceMethodsScrollBox, $sourceMethodsScrollBoxHeight);

                $.each(self.sourceMethods(), function (index, method) {
                    if (method.Name.toLowerCase() === methodName.toLowerCase()) {
                        self.data.method.SourceCode(utils.toHtml(method.SourceCode));

                        // BUG 9500 - 2013.05.31 - TWR : added
                        self.data.method.Parameters(method.Parameters);
                        return false;
                    }
                    return true;
                });
            }
        }).done(function () {
            self.isLoading = false; // BUG 9500 - 2013.05.31 - TWR : added
        });
    };

    self.loadNamespaces = function (source) {
        self.clearSelectedMethod();
        self.sourceMethods.removeAll();
        self.sourceMethodSearchTerm("");

        // BUG 9500 - 2013.05.31 - TWR : name changed to Plugin.Namespaces
        $.post("Service/PluginServices/Namespaces" + window.location.search, ko.toJSON(source), function (result) {
            self.namespaces(result);
            self.namespaces.sort(utils.fullNameCaseInsensitiveSort);

            // BUG 9500 - 2013.05.31 - TWR : added namespace selection
            var namespace = self.data.namespace();
            if (namespace) {
                self.selectNamespaceByName(namespace);
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
        var regex = new RegExp("__COMMA__", "g");
        $.post("Service/PluginServices/Test" + window.location.search, self.getJsonData(), function (result) {
            self.isTestResultsLoading(false);
            
            try {
                // clean things up a bit ;)
                for (var i = 0; i < result.Records.length; i++) {
                    for (var q = 0; q < result.Records[i].Cells.length; q++) {
                        result.Records[i].Cells[q].Value = result.Records[i].Cells[q].Value.replace(regex, ",");
                    }
                }
            } catch (e) {
                // Just unpack it and replace ;)
                result = JSON.parse(JSON.stringify(result).replace(regex, ","));
            }

            self.pushRecordsets(result);
            var hasErrors = self.data.recordsets().length > 0 && self.data.recordsets()[0].HasErrors();
            if (hasErrors) {
                self.hasTestErrors(true);
                self.testErrorMessage(self.data.recordsets()[0].ErrorMessage());
            } else {
                self.hasTestErrors(false);
                self.testErrorMessage("");
            }
        });
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.saveViewModel = SaveViewModel.create("Service/PluginServices/Save", self, saveContainerID, true);

    self.save = function () {

        // if new do old action
        // Guid.IsEmpty does not work!
        if (self.data.resourceID() == "00000000-0000-0000-0000-000000000000") {
            self.saveViewModel.showDialog(true);
        } else {
            // else use new action ;)
            self.saveViewModel.IsDialogLess = true;
            self.saveViewModel.save();
        }
    };

    self.load();
};



PluginServiceViewModel.create = function (pluginServiceContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();
    $("#tabs").tabs();

    var pluginServiceViewModel = new PluginServiceViewModel(saveContainerID, getParameterByName("rid"), getParameterByName("sourceName"), utils.decodeFullStops(getParameterByName("envir")), getParameterByName("path"));

    ko.applyBindings(pluginServiceViewModel, document.getElementById(pluginServiceContainerID));

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

    // inject pluginSourceDialog
    pluginServiceViewModel.$pluginSourceDialogContainer.load("Views/Sources/PluginSource.htm", function () {
        // 
        // pluginSourceViewModel is a global variable instantiated in PluginSource.htm
        //
        pluginSourceViewModel.createDialog(pluginServiceViewModel.$pluginSourceDialogContainer);
    });
    return PluginServiceViewModel;
};