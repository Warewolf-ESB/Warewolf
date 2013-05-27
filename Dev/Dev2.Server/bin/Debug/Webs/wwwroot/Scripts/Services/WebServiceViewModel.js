// Make this available to chrome debugger
//@ sourceURL=WebServiceViewModel.js  

function WebServiceViewModel(saveContainerID, resourceID, sourceName) {
    var self = this;
    var SRC_URL = 0;
    var SRC_BODY = 1;
    
    var $tabs = $("#tabs");
    var $sourceAddress = $("#sourceAddress");
    var $requestUrl = $("#requestUrl");
    var $addResponseDialog = $("#addResponseDialog");
    
    $("#addResponseButton")
      .text("")
      .append('<img height="16px" width="16px" src="images/paste.png" />')
      .button();
    
    self.$webSourceDialogContainer = $("#webSourceDialogContainer");

    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    self.onLoadSourceCompleted = null;
    
    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("WebService"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        source: ko.observable(),
        method: {
            Name: ko.observable(""),
            Parameters: ko.observableArray()
        },
        
        requestUrl: ko.observable(""),
        requestMethod: ko.observable(""),
        requestHeaders: ko.observable(""),
        requestBody: ko.observable(""),
        requestResponse: ko.observable(""),
        
        recordsets: ko.observableArray()
    };

    self.sourceAddress = ko.observable("");
    
    self.placeHolderRequestBody = ko.computed(function() {
        return self.data.source() ? "" : "e.g. CountryName=[[CountryName]]";
    });
    self.placeHolderRequestUrl = ko.computed(function () {
        return self.data.source() ? "" : "e.g. http://www.webservicex.net/globalweather.asmx/GetCitiesByCountry?CountryName=[[CountryName]]";
    });
    
    $requestUrl.keydown(function (e) {
        self.isBackspacePressed = e.keyCode == 8;
    });
    
    self.isUpdatingVariables = false;
    self.isBackspacePressed = false;

    self.pushRequestVariable = function (varName, varSrc, varValue) {
        self.data.method.Parameters.push({ Name: varName, Src: varSrc, Value: varValue, DefaultValue: varValue, IsRequired: false, EmptyToNull: false });
    };

    self.getOutputDisplayName = function (name) {
        return name && name.length > 20 ? ("..." + name.substr(name.length - 17, name.length)) : name;
    };
    
    self.pushRecordsets = function (result) {
        self.data.recordsets().length = 0;

        var hasResults = result.Recordsets && result.Recordsets.length > 0;
        self.hasTestResults(hasResults);

        if (!hasResults) {
            return;
        }

        result.Recordsets.forEach(function (entry) {
            $.each(entry.Fields, function (index, field) {
                field.DisplayName = self.getOutputDisplayName(field.Name);
                field.Alias = ko.observable(field.Alias);
            });
            
            var rs = {
                Name: ko.observable(entry.Name),
                DisplayName: ko.observable(self.getOutputDisplayName(entry.Name)),
                OriginalName: ko.observable(entry.Name),
                Fields: entry.Fields,
                Records: ko.observableArray(entry.Records),
                HasErrors: ko.observable(entry.HasErrors),
                ErrorMessage: ko.observable(entry.ErrorMessage),
                CanDisplayName: ko.observable(entry.Name !== null)
            };

            rs.Name.subscribe(function (newValue) {               
                $.each(rs.Fields, function (index, field) {
                    var alias = newValue + field.Name;
                    if (newValue.indexOf("()") == -1) {
                        field.Alias(alias);
                    } else {
                        alias = newValue + "." + field.Name;
                        field.Alias(alias);
                    }
                });
            });            
            self.data.recordsets.push(rs);
        });
    };
    
    self.updateVariables = function (varSrc, newValue) {
        if (self.isBackspacePressed) {
            return;
        }
        if (!newValue) {
            newValue = "";
        }

        // find indices of src variables to be deleted
        var indices = [];
        var oldVars = [];
        for (var i = 0; i < self.data.method.Parameters().length; i++) {
            var requestVar = self.data.method.Parameters()[i];
            if (requestVar.Src == varSrc) {
                indices.push(i);
                oldVars.push(requestVar);
            }
        }

        var paramNames = [];
        var paramValues = [];
        
        var paramVars = newValue.match(/\[\[\w*\]\]/g);    // match our variables!
        if (!paramVars) {
            paramVars = [];
        }
        if (varSrc == SRC_URL) {
            // regex should include a lookbehind to exclude leading char but lookbehind ?<= is not supported!            
            paramNames = newValue.match(/([?&#]).*?(?==)/g);    // ideal regex: (?<=[?&#]).*?(?==) 
            paramValues = newValue.match(/=([^&#]*)/g);         // ideal regex: (?<==)([^&#]*) 
            if (!paramNames) {
                paramNames = [];
            }
            if (!paramValues) {
                paramValues = [];
            }
            paramNames = paramNames.concat(paramVars);
            paramValues = paramValues.concat(new Array(paramVars.length));
        } else {
            paramNames = paramVars;
            paramValues = new Array(paramNames.length);
        }              

        $.each(paramNames, function (index, paramName) {
            var varName = paramName;
            if (varSrc == SRC_URL) {
                if (paramName.substr(0, 2) != "[[") {
                    paramName = paramName ? paramName.slice(1) : "";
                    varName = "[[" + paramName + "]]";
                } else {
                    paramName = paramName.slice(2).slice(0, paramName.length - 4);
                }
            } else {
                paramName = paramName.slice(2).slice(0, paramName.length - 4);
            }
            var paramValue = paramValues[index];            

            var oldVar = $.grep(self.data.method.Parameters(), function (e) { return e.Name == paramName; });
            if (oldVar.length > 0) {
                var oldIdx = oldVars.indexOf(oldVar[0]);
                oldVars.splice(oldIdx, 1);
            } else {
                self.pushRequestVariable(paramName, varSrc, paramValue ? paramValue.slice(1) : ""); // remove = prefix from paramValue
            }

            if (varSrc == SRC_URL) {
                var replaceStr = paramName + "=" + varName;
                if (newValue.indexOf(replaceStr) == -1) {
                    newValue = newValue.replace(paramName + paramValue, paramName + "=" + varName);
                }
            }

            try {
                self.isUpdatingVariables = true;
                if (varSrc == SRC_URL) {
                    self.data.requestUrl(newValue);
                } else {
                    self.data.requestBody(newValue);
                }
            } finally {
                self.isUpdatingVariables = false;
            }
            return true;
        });
        
        for (var j = 0; j < oldVars.length; j++) {
            var idx = self.data.method.Parameters.indexOf(oldVars[j]);
            self.data.method.Parameters.splice(idx, 1);
        }
        
        self.data.method.Parameters.sort(utils.nameCaseInsensitiveSort);
    };

    self.data.requestUrl.subscribe(function (newValue) {
        if (!self.isUpdatingVariables) {            
            self.updateVariables(SRC_URL, newValue);
        }
    });
    
    self.data.requestBody.subscribe(function (newValue) {
        if (!self.isUpdatingVariables) {
            self.updateVariables(SRC_BODY, newValue);
        }
    });

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
    
    self.data.source.subscribe(function (newValue) {             
        // our sources is a list of Resource's and NOT WebSource's 
        // so we have to load it
        if (newValue && !newValue.Address) {
            self.loadSource(newValue.ResourceID);
            return;
        }
        self.onSourceChanged(newValue);
    });

    self.onSourceChanged = function (newValue) {
        self.data.requestBody("");
        self.data.requestResponse("");
        self.data.method.Parameters().length = 0;
        self.data.recordsets().length = 0;
        self.data.requestUrl(newValue ? newValue.DefaultQuery : ""); // triggers a call updateVariables()
        self.hasTestResults(false);
        self.sourceAddress(newValue ? newValue.Address : "");

        var addressWidth = 0;
        if(newValue)
        {
            $sourceAddress.css("display", "inline-block");
            addressWidth = $sourceAddress.width() + 1;
            $sourceAddress.css("display", "table-cell");
        }
        $sourceAddress.css("width", addressWidth);
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
    
    self.hasTestResults = ko.observable(false);
    
    self.hasVariables = ko.computed(function() {
        return self.data.method.Parameters().length > 0;
    });
    
    self.isFormValid = ko.computed(function () {
        return self.hasTestResults();
    });
    
    self.isTestVisible = ko.observable(true);
    self.isTestEnabled = ko.computed(function () {
        return self.data.source() ? true : false;
    });
    self.isTestResultsLoading = ko.observable(false);
    self.isEditSourceEnabled = ko.computed(function () {
        return self.data.source();
    });

    self.isRequestBodyRequired = ko.computed(function () {
        return self.data.requestMethod() != "GET" && self.isTestEnabled();
    });

    self.title = ko.observable("New Service");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });
    self.saveTitle = ko.computed(function () {
        return "<b>" + self.title() + "</b>";
    });

    $tabs.on("tabsactivate", function (event, ui) {
        var idx = $tabs.tabs("option", "active");
        self.isTestVisible(idx == 0);
    });

    self.showTab = function (tabIndex) {
        $tabs.tabs("option", "active", tabIndex);
    };

    self.getJsonData = function () {
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
        
        $.post("Service/WebServices/Get" + window.location.search, args, function (result) {
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            // This will be invoked by loadSource 
            self.onLoadSourceCompleted = function() {
                if (result.Method) {
                    self.data.method.Name(result.Method.Name);
                    self.data.method.Parameters(result.Method.Parameters);
                }

                self.data.requestUrl(result.RequestUrl);
                self.data.requestMethod(result.RequestMethod);
                self.data.requestHeaders(result.RequestHeaders);
                self.data.requestBody(result.RequestBody);
                self.data.requestResponse(result.RequestResponse);

                self.pushRecordsets(result);
                self.onLoadSourceCompleted = null;
            };
            
            var found = sourceName && self.selectSourceByName(sourceName);           
            if (!found) {
                found = utils.IsNullOrEmptyGuid(result.Source.ResourceID)
                    ? self.selectSourceByName(result.Source.ResourceName)
                    : self.selectSourceByID(result.Source.ResourceID);
            }
            
            // MUST set these AFTER setting data.source otherwise they will be blanked!
            if (!found) {
                self.onLoadSourceCompleted();
            }
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
            // Need to set this just in case this is the first time!
            self.data.source().Address = result.Address;
            self.data.source().DefaultQuery = result.DefaultQuery;
            self.data.source().AuthenticationType = result.AuthenticationType;
            self.data.source().UserName = result.UserName;
            self.data.source().Password = result.Password;

            self.upsertSources(result);
            self.onSourceChanged(result);
            if (self.onLoadSourceCompleted != null) {
                self.onLoadSourceCompleted();
            }
        });
    };
    
    self.editSource = function () {
        return self.showSource(self.data.source().ResourceName);
    };

    self.newSource = function () {
        return self.showSource("");
    };
    
    self.addResponse = function () {
        $addResponseDialog.dialog("open");
    };
    
    self.testWebService = function (isResponseCleared) {
        self.isTestResultsLoading(true);
        if (isResponseCleared) {
            self.data.requestResponse("");
        }
        
        $.post("Service/WebServices/Test" + window.location.search, self.getJsonData(), function (result) {
            self.isTestResultsLoading(false);
            self.data.requestResponse(result.RequestResponse);
            
            self.pushRecordsets(result);
        });
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.saveViewModel = SaveViewModel.create("Service/WebServices/Save", self, saveContainerID);

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
    

    $("#addResponseDialog").dialog({
        resizable: false,
        autoOpen: false,
        modal: true,
        position: utils.getDialogPosition(),
        width: 600,
        buttons: {
            "Ok": function () {
                $(this).dialog("close");
                webServiceViewModel.testWebService(false);
            },
            "Cancel": function () {
                $(this).dialog("close");
            }
        }
    });
    
    // inject WebSourceDialog
    webServiceViewModel.$webSourceDialogContainer.load("Views/Sources/WebSource.htm", function () {
        // 
        // webSourceViewModel is a global variable instantiated in WebSource.htm
        //
        webSourceViewModel.createDialog(webServiceViewModel.$webSourceDialogContainer);
    });
    return webServiceViewModel;
};