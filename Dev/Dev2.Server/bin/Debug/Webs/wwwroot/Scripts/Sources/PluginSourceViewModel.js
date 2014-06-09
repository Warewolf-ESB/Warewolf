// Make this available to chrome debugger
//@ sourceURL=PluginSourceViewModel.js  

function PluginSourceViewModel(saveContainerID, environment) {
    var self = this;

    var $dialogContainerID = null;
    var $fileTree = $("#fileTree");
    var $gacList = $("#GACList");
    var $sourcetabs = $("#sourcetabs");
    var $assemblyFileLocation = $("#pluginAssemblyFileLocation");
    var $dialogSaveButton = $("#saveButton");
    if ($sourcetabs.tabs != undefined) {
        $sourcetabs.tabs();
    }

    //2013.06.08: Ashley Lewis for PBI 9458
    self.titleSearchString = "Plugin Source";
    self.currentEnvironment = ko.observable(environment);
    self.inTitleEnvironment = false;
    self.isReadOnly = false;
    
    self.onSaveCompleted = null;
    
    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("PluginSource"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        assemblyName: ko.observable(""),
        assemblyLocation: ko.observable("")
    };
    
    self.isAssemblyValid = ko.observable(false);
    self.validationErrorMsg = ko.observable();
    self.allGacAssemblies = ko.observableArray();
    self.gacSearchTerm = ko.observable("");
    self.gacSearchTerm.subscribe(function (newvalue) {
        self.gacSearchResults(newvalue);
    });
    self.gacSearchResults = function (value) {
        if (self.allGacAssemblies() != null && self.allGacAssemblies().length > 0 && $sourcetabs.tabs("option", "active") == 1) {
            var term = value.toLowerCase();
            if (term == "") {
                self.refreshGacList(self.allGacAssemblies());
                self.updateHelpText("gacSearchTerm");
				
				$("#gacSearchTerm").attr('placeholder', 'Search');
				
                return self.allGacAssemblies();
            }
            var filteredList = ko.utils.arrayFilter(self.allGacAssemblies(), function (assembly) {
                return assembly.AssemblyName.toLowerCase().indexOf(term) !== -1;
            });
            self.updateHelpText("gacSearchTerm:" + filteredList.length);
            self.refreshGacList(filteredList);
            return filteredList;
        }
        return null;
    };
    
    var resourceID = getParameterByName("rid");
    //self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    //self.data.resourceID(self.isEditing ? resourceID : $.Guid.Empty());

    // TODO: reinstate this check when all resources use an ID 
    //self.isEditing = !utils.IsNullOrEmptyGuid(resourceID);
    // TODO: remove this check: resourceID is either a GUID or a name to cater for legacy stuff
    self.isEditing = resourceID ? !utils.IsNullOrEmptyGuid(self.data.resourceID()) : false;

    self.title = ko.observable("New Plugin Source");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });

    self.saveTitle = ko.computed(function () {
        return self.data ? "<b>Plugin Source:</b> " + self.data.resourceName() : "";
    });

    if ($assemblyFileLocation.autocomplete != undefined) {
        $assemblyFileLocation.autocomplete({
            minLength: 2,
            source: [],
            select: function (event, ui) {
                self.data.assemblyLocation(ui.item.value.match(".dll") == null ? ui.item.value + "\\" : ui.item.value);
                $assemblyFileLocation.removeClass("ui-autocomplete-loading");
                //self.loadTreePath(); TODO fix
                return false;
            },
            //hoist autocomplete up up above assembly location textbox
            open: function (event, ui) {
                var autocomplete = $(".ui-autocomplete");
                var oldTop = autocomplete.offset().top;
                var newTop = oldTop - autocomplete.height() - $assemblyFileLocation.height() - 10;
                autocomplete.css("top", newTop);
            }
        });
    }

    //get new intellisense results whenever assembly location ends with a slash
    //and do all validation
    self.data.assemblyLocation.subscribe(function (newvalue) {
        if (newvalue && newvalue != "") {
            if (newvalue.match("GAC:") != null) {//is assembly a file?
                $assemblyFileLocation.removeClass("ui-autocomplete-loading");
                if (newvalue[newvalue.length - 1] == '\\') {
                    $.post("Service/PluginSources/GetDirectoryIntellisense" + window.location.search, self.data.assemblyLocation(), function(result) {
                        $assemblyFileLocation.autocomplete("option", "source", result);
                        $assemblyFileLocation.autocomplete("search");
                    });
                } else {
                    if (newvalue.match("GAC:") != null) {//is assembly a gac entry?
                        self.validateAssemblyFile(newvalue);
                    } else {//assembly is neither file nor gac entry
                        self.isAssemblyValid(false);
                        self.enableSaveButton(false);
                    }
                }
            } else {
                self.validateAssemblyFile(self.getFileFromPath(newvalue));
            }
        } else {//assembly is blank
            self.isAssemblyValid(false);
            self.enableSaveButton(false);
        }
    });
    
    //open the intellisense results on focus
    $assemblyFileLocation.bind('focus', function () { $(this).autocomplete("search"); });

    self.helpDictionaryID = "pluginSource";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    self.isHelpTextVisible = ko.observable(true);

    self.pluginFiles = ko.observableArray();
    self.searchFileResults = ko.computed(function () {
        return self.pluginFiles();
    });

    self.isFormValid = ko.computed(function () {
        var isValid = self.isAssemblyValid();
        if ($dialogContainerID) {
            $dialogSaveButton.button("option", "disabled", !isValid);
        }
        return isValid && !self.isReadOnly;
    });

    self.validateAssemblyFile = function (helpID) {
        $.ajax({
            type: 'POST',
            url: "Service/PluginSources/ValidateAssemblyImageFormat" + window.location.search,
            data: self.data.assemblyLocation(),
            success: function (data) {
                if (data.validationresult == "success") {
                    self.isAssemblyValid(true);
                    self.enableSaveButton(true);
                } else {
                    self.isAssemblyValid(false);
                    self.enableSaveButton(false);
                    self.validationErrorMsg(data.validationresult);
                }
                self.updateHelpText(helpID);
            },
            error: function(data, error) {
                self.isAssemblyValid(false);
                self.enableSaveButton(false);
                if (error == "parsererror") {
                    self.validationErrorMsg("Cannot find the file specified");
                    self.helpText("<h4>Plugin File</h4><p>" + self.validationErrorMsg() + "</p>");
                }
            },
            async: false
        });
    };
    
    self.enableSaveButton = function (enabled) {
        $dialogSaveButton.button("option", "disabled", !enabled);
    };

    self.updateHelpText = function (id) {
        if (id != null) {
            var text = "";
            if (id.indexOf(".") == -1 && id.match("GAC:") == null && id.match("gacSearchTerm:") == null) {
                text = self.helpDictionary[id];
                text = text ? text : self.helpDictionary.default;
                text = text ? text : "";
            } else {
                if (id.match("GAC:") != null) {
                    if (self.isAssemblyValid()) {
                        self.enableSaveButton(true);
						text = "<h4>Global Cache</h4><p>You have selected " + id + "</p>";
					}else{
                        // invalid asm ;(
                        self.enableSaveButton(false);
						text = "<h4>Global Cache</h4><p>" + id + " returned error: " + self.validationErrorMsg() + "</p>";
					}
                } 
                if (id.indexOf(".") != -1) {
                    if (self.isAssemblyValid()) {
                        text = "<h4>Plugin File</h4><p>You have selected " + id + "</p>";
                    } else {
                        text = "<h4>Plugin File</h4><p>" + id + " returned error: " + self.validationErrorMsg() + "</p>";
                    }
                }
                if (id.match("gacSearchTerm:")) {
                    var resolveListLength = id.substr(14, id.length - 14);
                    text = "<h4>Global Cache</h4><p>You are viewing " + resolveListLength + " out of " + self.allGacAssemblies().length + " assemblies</p>";
                }
            }
            self.helpText(text);
        }
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };
    
    $.post("Service/Help/GetDictionary" + window.location.search, self.helpDictionaryID, function (result) {
        self.helpDictionary = result;
        self.updateHelpText("default");
    });

    $(":input").focus(function () {
        self.updateHelpText(this.id);
    });
    
    $sourcetabs.on("tabsactivate", function (event, ui) {
        self.updateHelpText("tab " + $sourcetabs.tabs("option", "active"));
        if ($sourcetabs.tabs("option", "active") == 1) {
            gacSearchTerm.hidden = false;
            if (self.data.assemblyLocation() != null && self.data.assemblyLocation().match("GAC:") != null) {
                self.gacListScrollIntoView(self.data.assemblyLocation());
            }
        } else {
            gacSearchTerm.hidden = true;
        }
    });
    
    //manually bind GACList to assemblyLocation (GACList.selectedOption binding is for arrays)
    $gacList.on("change", function (newvalue) {
        self.data.assemblyLocation($gacList.val());
        self.validateAssemblyFile($gacList.val());
    });

    self.saveViewModel = SaveViewModel.create("Service/PluginSources/Save", self, saveContainerID);

    self.save = function () {
        var isWindowClosedOnSave = $dialogContainerID ? false : true;
        self.saveViewModel.showDialog(isWindowClosedOnSave, function (result) {
            if (!isWindowClosedOnSave) {
                $dialogContainerID.dialog("close");
                if (self.onSaveCompleted != null) {
                    self.onSaveCompleted(result);
                }
            };
        });
    };
    
	// set root URL
	var baseURL = utils.parseBaseURL(window.location + "");
	
	// Travis.Frisinger - Refacotred to use Management Services ;)
	$.ajax({
	    url: baseURL + "/Services/RegisteredAssemblyService",
	    data: "",
	    success: function (gacResult) {
	        //populate full list

	        self.allGacAssemblies(gacResult);
	        //view full list
	        self.refreshGacList(self.allGacAssemblies());
	        //show selected item
	        if (self.data.assemblyLocation() != null && self.data.assemblyLocation().match("GAC:") != null) {
	            self.gacListScrollIntoView(self.data.assemblyLocation());
	        }
	    }
	});

    self.treePathLoaded = true;
    self.initializeDynatree = function() {
	    // Travis.Frisinger - Refactord to use Management Services ;)
	    //
        // Dynatree Init
        //
        $.ajax({
            type: 'GET',
            url: baseURL + "/Services/FindDriveService",
            data: '',
            success: function(fullResult) {
                $fileTree.dynatree({
                    onCreate: function(node, nodeSpan) {
                        if (!node.data.isFolder) {
                            self.removeExpander(node); //remove expander icon
                        }
                    },
                    children: fullResult,
                    onLazyRead: function(node) {
                        if (node.data.isFolder) {

                            $.post(baseURL + "/Services/FindDirectoryService?DirectoryPath=" + self.resolvePath(node), "", function(lazyResult) {
                                node.setLazyNodeStatus(DTNodeStatus_Ok);
                                if (lazyResult.ErrorMessage == null) {
                                    node.addChild(lazyResult);
                                } else {
                                    console.log(lazyResult.ErrorMessage);
                                }
                            })
                                .success(function() {
                                    //highjack lazyload to load in assembly location into the tree (this avoids using loadKeyPath)
                                    if (!self.treePathLoaded && self.data.assemblyLocation() != null) {
                                        node.visit(function(childNode) {
                                            //use childNode.getLevel to find the assembly location part
                                            if (childNode.data.title == self.data.assemblyLocation().split("\\")[childNode.getLevel()-1]) {
                                                childNode.expand(true); //trigger recursive call
                                                if (!childNode.data.isFolder) {
                                                    $fileTree.animate({
                                                        // animate the scrolling to the node
                                                        scrollTop: ($(childNode.li).offset().top - 150) - $fileTree.offset().top + $fileTree.scrollTop()
                                                    }, 'fast');
                                                    self.updateHelpText(childNode.data.title);
                                                    self.validateAssemblyFile(childNode.data.title);
                                                    $(".dynatree-title", childNode.li).addClass("dynatree-selectedtitle");
                                                }
                                                return false; //stop searching
                                            }
                                        });
                                    }
                                    if (node.getChildren() == null) {
                                        self.removeExpander(node); //remove expander icon
                                    }
                                });
                        } else {
                            node.setLazyNodeStatus(DTNodeStatus_Ok);
                            self.treePathLoaded = true; //don't allow lazy load to be hijacked to load assembly paths anymore
                        }
                    },
                    onClick: function(node, event) {
                        if (!node.data.isFolder) {
                            self.data.assemblyLocation(self.resolvePath(node));
                            $assemblyFileLocation.removeClass("ui-autocomplete-loading");
                            self.updateHelpText(node.data.title);
                            self.validateAssemblyFile(node.data.title);
                            $(".dynatree-title", $fileTree).removeClass("dynatree-selectedtitle"); //every list item in the tree
                            $(".dynatree-title", node.li).addClass("dynatree-selectedtitle"); //just the selected list item
                        }
                    }
                });
            },
            dataType: 'json',
            async: false
        });
    };
    
    self.removeExpander = function (node) {
        node.li.innerHTML = node.li.innerHTML.replace("dynatree-expander", "dynatree-noexpander");//TODO fix (the node shouldnt shift across)
        $(node.li).css("margin-left", "16px");
        node.render();
    };
    
    self.load = function (theResourceID) {
        //
        //Form Init
        //
        $("#gacSearchTerm").value = "";
        if ($sourcetabs.tabs != undefined) {
            $sourcetabs.tabs("option", "active", 0);
        }
        self.treePathLoaded = false;
        //
        //Data Init
        //
        $.post("Service/PluginSources/Get" + window.location.search, theResourceID, function (result) {
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            self.data.assemblyLocation(result.AssemblyLocation);

            self.isEditing = self.data.resourceID() ? !utils.IsNullOrEmptyGuid(self.data.resourceID()) : false;
            self.title(self.isEditing ? "Edit Plugin Source - " + result.ResourceName : "New Plugin Source");
            
            if ($dialogContainerID) {
                $dialogContainerID.dialog("option", "title", self.title());
            }
        })
            .always(function () {
                self.loadTreePath();
                if (self.data.assemblyLocation() != null && self.data.assemblyLocation().match("GAC:") != null) {
                    self.gacListScrollIntoView(self.data.assemblyLocation());
                } 
            });
    };

    //load self.assemblies into GACList. avoids duplicates
    self.refreshGacList = function (assemblyList) {
        $gacList.empty();
        for (var indx = 0; indx < assemblyList.length; indx++) {
            //avoid duplicates
            if (indx == assemblyList.length - 1) {
                var option = document.createElement("option");
                //2013.05.13: Ashley Lewis Bug 9348 clean up value and name
                option.text = self.CleanAssemblyName(assemblyList[indx].AssemblyName);
                option.value = "GAC:" + assemblyList[indx].AssemblyName;
				option.title = assemblyList[indx].AssemblyName;
                $gacList.append(option, null);
            } else {
                if (assemblyList[indx].AssemblyName != assemblyList[indx + 1].AssemblyName) {
                    //dont add element if its duplicated
                    var option = document.createElement("option");
                    //2013.05.13: Ashley Lewis Bug 9348 clean up value and name
                    option.text = self.CleanAssemblyName(assemblyList[indx].AssemblyName);
                    option.value = "GAC:" + assemblyList[indx].AssemblyName;
					option.title = assemblyList[indx].AssemblyName;
                    $gacList.append(option, null);
                }
            }
        }
    };

    //GAC list scroll into view
    self.gacListScrollIntoView = function (assembly) {
        $sourcetabs.tabs("option", "active", 1);
        $gacList.focus();
        $gacList.val("");
        $gacList.val(assembly);
        self.validateAssemblyFile($gacList.val());
    };

    //load a path into the tree
    self.loadTreePath = function () {
        self.treePathLoaded = false;
        self.initializeDynatree();
        var treeRoot = $fileTree.dynatree("getRoot");
        treeRoot.visit(function (childNode) {
            if (self.data.assemblyLocation() != null && childNode.data.title == self.data.assemblyLocation().split("\\")[childNode.getLevel()-1]) {
                childNode.expand(true);//triggers lazyload which recursively loads all of assembly location into the dynatree (this is only on load, treePathLoaded is the flag to ensure this)
                return false;
            }
        });
    };

    self.resolvePath = function (node) {
        var fullNodePath = "";
        var nodeIterator = node;

        for (var i = 0; i < node.getLevel() ; i++) {
            fullNodePath = nodeIterator.data.title + "\\" + fullNodePath;
            nodeIterator = nodeIterator.getParent();
        }
		
        if (!node.data.isFolder) {
            fullNodePath = fullNodePath.substr(0, fullNodePath.lastIndexOf('\\'));
        }
        return fullNodePath;
    };

    self.getFileFromPath = function (path) {
        return path.substr(path.lastIndexOf("\\") + 1, path.length - path.lastIndexOf("\\"));
    };

    self.removeGacPrefix = function(name) {
        return name.substr(4, name.length - 4);
    };

    self.CleanAssemblyName = function(fullName) {
        var secondComma = fullName.indexOf(',', fullName.indexOf("Version="));
        return fullName.substr(0, secondComma);
    };

    self.showDialog = function (sourceName, onSaveCompleted) {
        // NOTE: Should only be invoked from PluginService form!
        self.onSaveCompleted = onSaveCompleted;

        self.load(sourceName);

        // remove our title bar
        var $dlgTitle = $("div[id='pluginSourceHeader']");
        if ($dlgTitle) {
            $dlgTitle.hide();
        }

        // display showHelp checkbox that's on that title bar
        var $chkShowHelp = $("showHelp");
        if ($chkShowHelp) {
            $chkShowHelp.show();
            $chkShowHelp.css("visibility", "visible");
            $chkShowHelp.css("margin-top", "0px");
        }

        // remove non-dialog buttons
        var $btnSave = $("#saveButton");
        if ($btnSave) {
            $btnSave.hide();
        }
        
        var $btnCancel = $("#nonButtonBarCancelButton");
        if ($btnCancel) {
            $btnCancel.hide();
        }

        // the dialog button bar adds about 50px, take 50px from the div height
        $("#pluginSourceContainer").height(410);
        
        //remove resize
        $fileTree.css("resize", "horizontal");
        
        $dialogContainerID.dialog("open");

        //2013.07.04: Ashley Lewis for bug 9799 - set default focus
        $fileTree.focus();

        //2013.06.09: Ashley Lewis for PBI 9458 - Show server in dialog title
        if (self.currentEnvironment() && self.inTitleEnvironment == false) {
            utils.appendEnvironmentSpan(self.titleSearchString, self.currentEnvironment());
            self.inTitleEnvironment = true;
        }
    };
    
    self.createDialog = function ($containerID) {
        $dialogContainerID = $containerID;
        $containerID.dialog({
            resizable: false,
            autoOpen: false,
            modal: true,
            width: 703,
            position: utils.getDialogPosition(),
            buttons: {
                "Save Plugin": function () {
                    self.save();
                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            }
        });
        
        $("button").button();
        $dialogSaveButton = $(".ui-dialog-buttonpane button:contains('Save Plugin')");
        $dialogSaveButton.attr("tabindex", "8");
        $dialogSaveButton.attr("data-bind", "jEnable: isFormValid");
        $dialogSaveButton.next().attr("tabindex", "9");
        
        // remove annoying look and feel
        $pluginSourceContainer = $("#pluginSourceContainer");
        if ($pluginSourceContainer) {
            $pluginSourceContainer.removeClass("ui-widget-content");
        }
    };

    if (!$dialogContainerID) {
        self.load(resourceID);
    }
    
    utils.isReadOnly(resourceID, function (isReadOnly) {
        self.isReadOnly = isReadOnly;
    });
};

PluginSourceViewModel.create = function (pluginSourceContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();

    var pluginSourceViewModel = new PluginSourceViewModel(saveContainerID, utils.decodeFullStops(getParameterByName("envir")));
    ko.applyBindings(pluginSourceViewModel, document.getElementById(pluginSourceContainerID));
    return pluginSourceViewModel;
};