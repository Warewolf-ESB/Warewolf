// Make this available to chrome debugger
//@ sourceURL=WebServiceViewModel.js  

function WebServiceViewModel(saveContainerID, resourceID, sourceName) {
    var self = this;
    var SRC_URL = 0;
    var SRC_BODY = 1;
    
    var $sourceMethodsScrollBox = $("#sourceMethodsScrollBox");
    var $sourceMethodsScrollBoxHeight = 343;
    var $sourceMethods = $("#sourceMethods");
    var $actionInspectorDialog = $("#actionInspectorDialog");
    var $tabs = $("#tabs");
    var $sourceAddress = $("#sourceAddress");
    var $requestUrl = $("#requestUrl");
        
    self.$webSourceDialogContainer = $("#webSourceDialogContainer");
    
    // TODO: reinstate this check when all resources use an ID 
    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    // TODO: remove this check: resourceID is either a GUID or a name to cater for legacy stuff
    //self.isEditing = resourceID ? !(resourceID === "" || $.Guid.IsEmpty(resourceID)) : false;

    self.data = {
        resourceID: ko.observable(""),
        //resourceID: ko.observable(self.isEditing ? resourceID : $.Guid.Empty()),
        resourceType: ko.observable("WebService"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        source: ko.observable(),
        requestUrl: ko.observable(""),
        requestMethod: ko.observable(""),
        requestHeaders: ko.observable(""),
        requestBody: ko.observable(""),
        requestResponse: ko.observable(""),
        requestVariables: ko.observableArray(),
    };

    self.updateVariables = function (varSrc, newValue) {
        $.each(self.data.requestVariables(), function (index, requestVar) {
            if (requestVar.Src == varSrc) {
                self.data.requestVariables.splice(index, 1);
            }
            return true;
        });        
        
        
        var varNames = new Array();
        var varValues = new Array();

        if (varSrc == SRC_URL) {
            // regex should include a lookbehind to exclude leading char but lookbehind ?<= is not supported!

            // ideal regex: (?<=[?&#]).*?(?==) 
            var paramNames = newValue.match(/([?&#]).*?(?==)/g);

            // ideal regex: (?<==)([^&#]*)
            var paramValues = newValue.match(/=([^&#]*)/g);
            
            $.each(paramNames, function (index, paramName) {
                var varName = "[[" + paramName.slice(1) + "]]";
                var paramValue = paramValues[index];
                paramValue = paramValue ? "" : paramValue.slice(1);
                newValue.replace(paramValue, varName);
                self.data.requestVariables.push({ Name: varName, Src: varSrc, Value: paramValue });
                return true;
            });

        } else {
            //varNames = newValue.match(/\[\[\w*\]\]/g);  // match our variables!
            //varValues = new Array(varNames ? varNames.length : 0);
        }

       
        self.data.requestVariables.sort(utils.nameCaseInsensitiveSort);
    };

    self.data.requestUrl.subscribe(function (newValue) {
        if (newValue) {
            self.updateVariables(SRC_URL, newValue);
        }
    });
    
    self.afterAddRequestVariable = function (element, index, data) {
        //var $elem = $(element);
        //if ($elem.nodeName == "TR") {
        //    $elem.tooltip();
        //    var tooltipElem = $elem.children[0].firstChild;
        //    $(tooltipElem).tooltip();
        //}
    };
    
    self.requestMethods = ko.observableArray(["GET", "POST", "PUT", "DELETE", "TRACE"]);  
    
    self.sources = ko.observableArray();
    self.upsertSources = function (result) {
        var id = result.ResourceID.toLowerCase();
        var name = result.ResourceName.toLowerCase();
        var idx = -1;
        var replace = false;
        $.each(self.sources(), function (index, source) {
            if (source.ResourceID.toLowerCase() === id) {
                idx = index;
                replace = true;
                return false;
            }           
            if (idx == -1 && name < source.ResourceName.toLowerCase()) {
                idx = index;
            }
            return true;
        });
        
        if (idx != -1) {
            if (replace) {
                self.sources()[idx] = result;
            } else {
                self.sources.splice(idx, 0, result);
            }
        } else {
            self.sources.push(result);
        }
    };
    
    self.data.source.subscribe(function(newValue) {
        // our sources is a list of Resource's and NOT WebSource's 
        // so we have to load it
        if (!newValue.Address) {
            self.loadSource(newValue.ResourceID);
            return;
        }
        self.onSourceChanged(newValue);
    });

    self.onSourceChanged = function (newValue) {
        self.data.requestUrl(newValue.DefaultQuery);

        $sourceAddress.text(newValue.Address);

        var addressWidth = $sourceAddress.width() + 5;
        $requestUrl.width(903 - addressWidth);
        $requestUrl.css("padding-left", addressWidth);
    };

    self.selectSourceByID = function (theID) {
        theID = theID.toLowerCase();
        var found = false;
        $.each(self.sources(), function (index, source) {
            if (source.ResourceID.toLowerCase() === theID) {
                found = true;
                self.data.source(source);
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
                    self.data.source(source);
                    return false;
                }
                return true;
            });
        }
        return found;
    };

    self.isRequestBodyRequired = ko.computed(function () {
        return self.data.requestMethod() != "GET";
    });
    
    self.hasTestResults = ko.observable(false);
    self.hasVariables = ko.computed(function() {
        return self.data.requestVariables().length > 0;
    });
    
    self.isFormValid = ko.computed(function () {
        
        return false;
    });

    self.isTestEnabled = ko.computed(function () {
        return false;
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
        return ko.toJSON(self.data);
    };

    self.load = function () {
        self.loadSources(
            self.loadService());
    };
    
    self.loadService = function () {
        var args = ko.toJSON({
            resourceID: resourceID,
            resourceType: "WebService"
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
           
            self.title(self.isEditing ? "Edit Web Service - " + result.ResourceName : "New Web Service");
        });
    };

    self.loadSources = function (callback) {
        $.post("Service/Resources/Sources" + window.location.search, ko.toJSON({ resourceType: "WebSource" }), function (result) {
            self.sources(result);
            self.sources.sort(utils.resourceNameCaseInsensitiveSort);
        }).done(function () {
            if (callback) {
                callback();
            }
        });
    };
    
    self.loadSource = function (sourceID) {
        $.post("Service/WebSources/Get" + window.location.search, sourceID, function (result) {
            self.upsertSources(result);
            self.onSourceChanged(result);
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
    
    self.testWebService = function () {
        self.isTestResultsLoading(true);
        $.post("Service/Services/WebTest" + window.location.search, self.getJsonData(), function (result) {
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
        // webSourceViewModel is a global variable instantiated in WebSource.htm
        //
        webSourceViewModel.showDialog(theSourceName, function (result) {
            self.upsertSources(result);
            self.data.source(result);            
        });
    };
    
    self.load();    
};


WebServiceViewModel.create = function (webServiceContainerID, saveContainerID) {
    $("button").button();
    $("#tabs").tabs();

    var webServiceViewModel = new WebServiceViewModel(saveContainerID, getParameterByName("rid"), getParameterByName("sourceName"));

    ko.applyBindings(webServiceViewModel, document.getElementById(webServiceContainerID));


    //var theName = "CountryName";
    //for (var i = 0; i < 10; i++) {
    //    theName += "" + theName;
    //    webServiceViewModel.addVariable("[[" + theName + "]]");
    //}
    
    // inject WebSourceDialog
    webServiceViewModel.$webSourceDialogContainer.load("Views/Sources/WebSource.htm", function () {
        // 
        // webSourceViewModel is a global variable instantiated in WebSource.htm
        //
        webSourceViewModel.createDialog(webServiceViewModel.$webSourceDialogContainer);
    });
    return webServiceViewModel;
};