// Make this available to chrome debugger
//@ sourceURL=WebServiceViewModel.js  

function WebServiceViewModel(saveContainerID, resourceID, sourceName, environment, resourcePath) {
    var self = this;
    var srcUrl = 0;
    var srcBody = 1;
    var srcHeader = 2;

    var srcBodyPrev = "";
    var srcHeaderPrev = "";
    
    var $tabs = $("#tabs");
    var $sourceAddress = $("#sourceAddress");
    var $requestHeaders = $("#requestHeaders");
    var $requestUrl = $("#requestUrl");
    var $requestBody = $("#requestBody");
    var $addResponseDialog = $("#addResponseDialog");
    var $addPathDialog = $("#addPathDialog");
    
    $("#addResponseButton").length > 0 ? $("#addResponseButton")
      .text("")
      .append('<img height="16px" width="16px" src="images/edit.png" />')
    // ReSharper disable once WrongExpressionStatement
      .button() : null;
    
    $("#addPathButton").length > 0 ? $("#addPathButton")
      .text("")
      .append('<img height="16px" width="16px" src="images/jsonpath.png" />')
      .button() : null;
    
    self.$webSourceDialogContainer = $("#webSourceDialogContainer");

    self.currentEnvironment = ko.observable(environment); //2013.06.08: Ashley Lewis for PBI 9458 - Show server
    self.titleSearchString = "Web Service";
    
    self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    self.onLoadSourceCompleted = null;
    self.inputMappingLink = "Please enter a request url or body first (Step 3 & 4)";
    self.outputMappingLink = "Please run a test first (Step 5) or paste a response first (Step 6)";
    self.isReadOnly = false;
    
    self.data = new ServiceData(self.isEditing ? resourceID : $.Guid.Empty(), "WebService");
    self.data.requestUrl = ko.observable("");
    self.data.requestMethod = ko.observable("");
    self.data.requestHeaders = ko.observable("");
    self.data.requestBody = ko.observable("");
    self.data.requestResponse = ko.observable("");   
    self.data.jsonPath = ko.observable("");
	self.data.jsonPathResult = ko.observable("");
	self.data.requestMessage = ko.observable("");	
	self.data.displayData = ko.observable("");

    self.sourceAddress = ko.observable("");
    
    self.placeHolderRequestBody = ko.computed(function () {
        return self.data.source() ? "" : "e.g. CountryName=[[CountryName]]";
    });
    self.placeHolderRequestUrl = ko.computed(function () {
        return self.data.source() ? "" : "e.g. http://www.webservicex.net/globalweather.asmx/GetCitiesByCountry?CountryName=[[CountryName]]";
    });
    
    self.isPaste = ko.observable(false);
    self.isPaste.subscribe(function (isPasting) {
        if (isPasting) {
            self.clearRequestVariables();
        }
        self.hasSourceSelectionChanged = isPasting;
		self.hasPasteHappened = isPasting;
    });

    self.isCut = ko.observable(false);
    self.isCut.subscribe(function (isCutting) {
        if (isCutting) {
            self.clearRequestVariables();
        }
        self.hasSourceSelectionChanged = isCutting;
    });

    self.RequestUrlOnKeyDownEvent = function (elem, e) {
        self.isBackspacePressed = e.keyCode == 8;
        self.isEqualPressed = e.keyCode == 187;
        self.isCloseBracketPressed = e.keyCode == 221;
        self.isPaste(e.ctrlKey && e.keyCode == 86); // CTRL+V
        self.isCut(e.ctrlKey && e.keyCode == 88); // CTRL+X
        return true;
    };
    self.RequestUrlOnKeyUpEvent = function (elem, e) {
        self.isBackspacePressed = false;
        self.isEqualPressed = false;
        self.isCloseBracketPressed = false;
        self.isPaste(false);
        self.isCut(false);
        return true;
    };

    $requestBody.keydown(function (e) {
        self.isCloseBracketPressed = e.keyCode == 221;
    }).keyup(function (e) {
        self.isCloseBracketPressed = false;
    });

    $requestHeaders.keydown(function (e) {
        self.isCloseBracketPressed = e.keyCode == 221;
    }).keyup(function (e) {
        self.isCloseBracketPressed = false;
    });
    
    self.isUpdatingVariables = false;
    self.isBackspacePressed = false;
    self.isEqualPressed = false;
    self.isCloseBracketPressed = false;
    self.hasSourceSelectionChanged = false;
	self.hasPasteHappened = false;

    self.pushRequestVariable = function (varName, varSrc, varValue) {
        var oldVar = $.grep(self.data.method.Parameters(), function (e) { return e.Name == varName; });
        if (oldVar.length == 0) {
            self.data.method.Parameters.push({ Name: varName, Src: varSrc, Value: varValue, DefaultValue: varValue, IsRequired: false, EmptyToNull: false });
        }
    };

    self.clearRequestVariables = function () {
        self.data.method.Parameters.removeAll();
    };
    
    self.getOutputDisplayName = function (name) {
        return name && name.length > 20 ? ("..." + name.substr(name.length - 17, name.length)) : name;
    };
    
    self.pushRecordsets = function (result) {
        var hasResults = result.Recordsets && result.Recordsets.length > 0;
        self.hasTestResults(hasResults);

        recordsets.pushResult(self.data.recordsets, result.Recordsets);
        utils.toggleUIReadOnlyState(self.isReadOnly);
    };

    self.getParameter = function (text, start) {
        var result = { name: "", value: "", nameStart: 0, nameEnd: 0, valueStart: 0, valueEnd: 0 };

        result.nameEnd = start - 1;
        result.nameStart = text.lastIndexOf("&", result.nameEnd);
        if (result.nameStart == -1) {
            result.nameStart = text.lastIndexOf("?", result.nameEnd);
        }
        if (result.nameStart != -1 && result.nameEnd >= result.nameStart) {
            result.nameStart++;
            result.name = text.substring(result.nameStart, result.nameEnd);
        }
        
        result.valueStart = start;
        result.valueEnd = text.indexOf("&", start);
        if (result.valueEnd == -1) {
            result.valueEnd = text.indexOf("=", start - 1);
            if (result.valueEnd == -1) {
                result.valueEnd = text.length;
            } else {
                result.valueEnd = result.valueStart;
            }
        }
        result.value = text.substring(result.valueStart, result.valueEnd);
        
        return result;
    };

    self.extractAndPushRequestVariable = function (text, start, varSrc) {
        var prev = text.slice(start - 2, start - 1);
		
        if (prev == "]") {
            
            var idx = text.lastIndexOf("[[", start);
            if (idx != -1) {
                var paramName = text.substring(idx + 2, start - 2);
                self.pushRequestVariable(paramName, varSrc, "");
            }
        }
    };

    self.updateVariablesText = function (varSrc, newValue, caretPos) {
        try {
            self.isUpdatingVariables = true;
            if (varSrc == srcUrl) {
                self.data.requestUrl(newValue);
                $requestUrl.caret(caretPos);

            } else if(varSrc == srcBody){
                self.data.requestBody(newValue);
            }else {
                self.data.requestHeaders(newValue);
            }
        } finally {
            self.isUpdatingVariables = false;
        }
    };
    
    self.updateAllVariables = function (varSrc, newValue) {
        if (!newValue) {
            newValue = "";
        }

        var paramNames = [];
        var paramValues = [];

        var paramVars = newValue.match(/\[\[\w*\]\]/g); // match our variables!
        if (!paramVars) {
            paramVars = [];
        }
        if (varSrc == srcUrl) {
            // regex should include a lookbehind to exclude leading char but lookbehind ?<= is not supported!            
            paramNames = newValue.match(/([?&#]).*?(?==)/g); // ideal regex: (?<=[?&#]).*?(?==) 
            paramValues = newValue.match(/=([^&#]*)/g); // ideal regex: (?<==)([^&#]*) 
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
            if (varSrc == srcUrl) {
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

            self.pushRequestVariable(paramName, varSrc, paramValue ? paramValue.slice(1) : ""); // remove = prefix from paramValue

            if (varSrc == srcUrl) {
                var replaceStr = paramName + "=" + varName;
                if (newValue.indexOf(replaceStr) == -1) {
                    newValue = newValue.replace(paramName + paramValue, paramName + "=" + varName);
                }
            }
            self.updateVariablesText(varSrc, newValue, newValue.length);
            return true;
        });
    };

    self.ProperlyHandleVariableInput = function (newValue) {
		// handle more normal use cases ;)
			
		// find ? then substring
		var indexOfQuestionMark = newValue.indexOf("?");
		
        if (indexOfQuestionMark >= 0) {
		
			// create working string 
            var tmpValue = newValue.substring((indexOfQuestionMark + 1));
			// generate pairs
			var pairs = tmpValue.split("&");
			
			// now match pairs ;)
            for (var i = 0; i < pairs.length; i++) {
				
				var tmp = pairs[i];

				var subPairs = tmp.split("=");
				
				if (subPairs.length == 2) {


				    var varName = subPairs[1].replace("[[", "").replace("]]", "");

				    var targetEnd = varName.indexOf("&");

				    if (targetEnd > 0) {
				        // we need to trim it up a bit ;)
				        varName = varName.substring(0, targetEnd);
				    }

				    self.pushRequestVariable(varName, "", "");
				} else {
				    // we may have a silly chicken with just a datalist fragment ;)
				    if (tmp.indexOf("[[") == 0) {
				   
				        varName = tmp.replace("[[", "").replace("]]", "");
				        self.pushRequestVariable(varName, "", "");
				        
				    }
				}
			}
		}
        
	};

    self.updateVariables = function (varSrc, newValue) {

		var param = self.getParameter(newValue, start);
		var start = 0;

        if(varSrc == srcUrl) {
            start = $requestUrl.caret();
        }else if(varSrc == srcBody) {
            start = $requestBody.caret();
        } else if (varSrc == srcHeader) {
            start = $requestHeaders.caret();
        }

        if (self.hasSourceSelectionChanged) {
		
            // when paste do this ;)
            if (self.hasPasteHappened) {
		
				self.hasPasteHappened = false;

                // Scan for [[]] regions prior to the variable request string ;)
				paramVars = newValue.match(/\[\[\w*\]\]/g); // match our variables!

                for (var ii = 0; ii < paramVars.length; ii++) {
                    var value = paramVars[ii].replace("[[","").replace("]]","");
                    self.pushRequestVariable(value, "", "");
                }

            } else {
				self.updateAllVariables(varSrc, newValue);
			}
			
			self.data.method.Parameters.sort(utils.nameCaseInsensitiveSort);
			
            return;
        }
	
        if (varSrc == srcUrl) {
            if (self.isEqualPressed) {
                param = self.getParameter(newValue, start);

				var afterParam = "";
                var offSet = start + (param.name.length);
				
                if (offSet < newValue.length) {
					afterParam = newValue.substring(offSet);
				}
                
				// handle the case of ]]= and &= correctly 
				if (param.name.indexOf("[[") < 0 && param.name.indexOf("=") < 0 && param.name.length > 0 && afterParam.indexOf("[") != 0) {
				    self.pushRequestVariable(param.name, varSrc, param.value);

				    var prefix = newValue.slice(0, param.valueStart);
				    var postfix = newValue.slice(param.valueEnd, newValue.length);
				    var paramValue = "[[" + param.name + "]]";
				    newValue = prefix.concat(paramValue).concat(postfix);					


				    self.updateVariablesText(varSrc, newValue, start + paramValue.length);
				}
                
            } else if (self.isCloseBracketPressed) {
                self.extractAndPushRequestVariable(newValue, start, varSrc);
            } else {
				// handle more normal use case ;)
				self.ProperlyHandleVariableInput(newValue);
			}
        } else { // SRC_BODY
			
            if (self.isCloseBracketPressed) {
                self.extractAndPushRequestVariable(newValue, start, varSrc);
            } else {
                if (srcHeaderPrev == "" || srcBodyPrev == "") {
                    // fake paste due to silly binding issues
                    self.updateAllVariables(varSrc, newValue);
                }
            }
        }


            // Clean up variables
            var paramVars = newValue.match(/\[\[\w*\]\]/g);    // match our variables!
            var forceRemoveItems = new Array();
            if (!paramVars) {
                paramVars = newValue.match(/\[\[\w*\]/g);    // match our variables!
                if (!paramVars) {
                    // all gone, swap what was there for parsing ;)
                    if (varSrc == srcBody) {
                        paramVars = srcBodyPrev.match(/\[\[\w*\]\]/g);    // match our variables!
                    }else if(varSrc == srcHeader) {
                        paramVars = srcHeaderPrev.match(/\[\[\w*\]\]/g);    // match our variables!
                    }
                }

                for (var q = 0; q < paramVars.length; q++) {
                    if (!paramVars[q].endsWith("]]")) {
                        forceRemoveItems[q] = paramVars[q] + "]";
                    }else {
                        forceRemoveItems[q] = paramVars[q];
                    }
                }
            }


        if (paramVars) {
            for (var i = self.data.method.Parameters().length - 1; i >= 0; i--) {
                var requestVar = self.data.method.Parameters()[i];
                if (requestVar != undefined) {
                    var paramVar = $.grep(paramVars, function (e1) { return e1 == "[[" + requestVar.Name + "]]"; });
                }
                if (paramVar.length == 0 && requestVar.Src == varSrc) {
                    self.data.method.Parameters.splice(i, 1);
                } else {
                    for (var z = 0; z < forceRemoveItems.length; z++) {
                        //alert("here");

                        for (var c = 0; c < self.data.method.Parameters().length; c++) {
                            var tmp = self.data.method.Parameters()[c];
                            if("[["+tmp.Name+"]]" == forceRemoveItems[z]) {
                                self.data.method.Parameters.splice(c, 1);
                            }
                        }
                    }
                }
            }
        }
 
		if (varSrc == srcBody) {
			srcBodyPrev = newValue;
		} else if (varSrc == srcHeader) {
			srcHeaderPrev = newValue;
		}
		
        self.data.method.Parameters.sort(utils.nameCaseInsensitiveSort);
    };
    
    self.data.requestHeaders.subscribe(function (newValue) {
        if (!self.isUpdatingVariables) {
            self.updateVariables(srcHeader, newValue);
        }
    });

    self.data.requestUrl.subscribe(function (newValue) {
        if (!self.isUpdatingVariables) {            
            self.updateVariables(srcUrl, newValue);
        }
    });
    
    self.data.requestBody.subscribe(function (newValue) {
        if (!self.isUpdatingVariables) {
            self.updateVariables(srcBody, newValue);
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
        self.hasSourceSelectionChanged = true;
        try {
            self.data.requestBody("");
            self.data.requestResponse("");
            self.data.requestHeaders("");
            self.clearRequestVariables();
            self.data.recordsets.removeAll();
            self.data.requestUrl(newValue ? newValue.DefaultQuery : ""); // triggers a call updateVariables()
            self.hasTestResults(false);
            self.sourceAddress(newValue ? newValue.Address : "");

            var addressWidth = 0;
            if (newValue) {
                $sourceAddress.css("display", "inline-block");
                addressWidth = $sourceAddress.width() + 1;
                $sourceAddress.css("display", "table-cell");
            }
            $sourceAddress.css("width", addressWidth);
        } finally {
            self.hasSourceSelectionChanged = false;
        }
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
    
    self.hasInputs = ko.computed(function () {
        return self.data.method.Parameters().length > 0;
    });
    
    self.isFormValid = ko.computed(function () {
        return self.hasTestResults() && !self.isReadOnly;
    });
    
    self.isTestVisible = ko.observable(true);
    self.isTestEnabled = ko.computed(function () {
        if (self.isReadOnly) {
            return false;
        }
        return self.data.source() ? true : false;
    });
	self.isJsonPathEnabled = ko.computed(function () {
        var firstFlag =  self.data.requestResponse() ? true : false;
		
		if(firstFlag){
			// check that it is JSONData ;)
			try{
				JSON.parse(self.data.requestResponse());
				return true;
			}catch(e){
				return false;
			}
		}
		return false;
    });
    self.isTestResultsLoading = ko.observable(false);
	self.canDisplayJSONPathResult = ko.observable(true);
    self.setTestResultsLoading = function (testResultsLoading) {
		self.canDisplayJSONPathResult(!testResultsLoading);
        return self.isTestResultsLoading(testResultsLoading);
    };
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

    self.setRequestResponse = function (requestResponse) {
        return self.data.requestResponse(requestResponse);
    };
	
	self.setJsonPathData = function (data) {
        return self.data.jsonPathResult(data);
    };
	
	self.setDisplayData = function(data){
		return self.data.displayData(data);
	}

    self.load = function () {
        self.loadSources(
            self.loadService());
        utils.isReadOnly(resourceID, function (isReadOnly) {
            self.isReadOnly = isReadOnly;
        });
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
            self.data.jsonPath(result.JsonPath);
			self.data.requestMessage(result.RequestMessage);
			self.data.displayData("");

            if (!result.ResourcePath && resourcePath) {
                self.data.resourcePath(resourcePath);
            }

            // This will be invoked by loadSource 
            self.onLoadSourceCompleted = function () {
                if (result.Method) {
                    self.data.method.Name(result.Method.Name);
                    self.data.method.Parameters(result.Method.Parameters);
                }

                self.data.requestUrl(result.RequestUrl);
                self.data.requestMethod(result.RequestMethod);
                self.data.requestHeaders(result.RequestHeaders);
                srcHeaderPrev = result.RequestHeaders;
                self.data.requestBody(result.RequestBody);
                srcBodyPrev = result.RequestBody;
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
    
    self.addPath = function () {
        $addPathDialog.dialog("open");
    };
    
    self.testWebService = function (isResponseCleared) {
        self.isTestResultsLoading(true);
        if (isResponseCleared) {
            self.data.requestResponse("");
        }
        
        $.post("Service/WebServices/Test" + window.location.search, self.getJsonData(), function (result) {
            self.isTestResultsLoading(false);
            self.data.requestResponse(result.RequestResponse);
            self.data.requestMessage(result.RequestMessage);
			self.data.jsonPathResult(result.RequestResponse);
			
			// set correct display data ;)
			if(result.RequestMessage != null && result.RequestMessage != ""){
				self.data.displayData(result.RequestMessage + " ");
			}else{
				self.data.displayData(result.RequestResponse+" ");
			}
            
            self.pushRecordsets(result);
        });
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.saveViewModel = SaveViewModel.create("Service/WebServices/Save", self, saveContainerID);

    self.save = function () {

        // if new do old action
        // Guid.IsEmpty does not work!
        if (self.data.resourceID() == "00000000-0000-0000-0000-000000000000") {
            self.saveViewModel.showDialog(true);
        } else {
            // else use new action ;)
            self.saveViewModel.IsDialoglessSave = true;
            self.saveViewModel.save();
        }
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

    var webServiceViewModel = new WebServiceViewModel(saveContainerID, getParameterByName("rid"), getParameterByName("sourceName"), utils.decodeFullStops(getParameterByName("envir")), getParameterByName("path"));

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
    
    $("#addPathDialog").dialog({
        resizable: false,
        autoOpen: false,
        modal: true,
        position: utils.getDialogPosition(),
        width: 650,
        buttons: {
            "Ok": function () {
				webServiceViewModel.setRequestResponse(webServiceViewModel.data.jsonPathResult());
				webServiceViewModel.setDisplayData(webServiceViewModel.data.jsonPathResult());
                $(this).dialog("close");
            },
            "Cancel": function () {
                $(this).dialog("close");
            },
            "Test Path": function () {
                webServiceViewModel.setTestResultsLoading(true);
                $.post("Service/WebServices/ApplyPath" + window.location.search, webServiceViewModel.getJsonData(), function (result) {
                    
					webServiceViewModel.setJsonPathData(result.JsonPathResult);
					
					if(result.jsonPath != null && result.jsonPath != ""){
						webServiceViewModel.setDisplayData(result.JsonPathResult+" ");
					}

					webServiceViewModel.pushRecordsets(result);
					//webServiceViewModel.pushResult(webServiceViewModel.data.recordsets, result.Recordsets);
					webServiceViewModel.setTestResultsLoading(false);
                });
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