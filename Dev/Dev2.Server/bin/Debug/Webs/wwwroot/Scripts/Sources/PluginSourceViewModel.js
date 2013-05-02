function PluginSourceViewModel(saveContainerID) {
    var self = this;

    var $dialogContainerID = null;
    var $dialogSaveButton = null;
    var $fileTree = $("#fileTree");
    var $gacList = $("#GACList");
    var $sourcetabs = $("#sourcetabs");
    var $assemblyFileLocation = $("#pluginAssemblyFileLocation");
    $sourcetabs.tabs();

    self.onSaveCompleted = null;
    self.driveLetter = '';
    
    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("PluginSource"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        assemblyName: ko.observable(""),
        assemblyLocation: ko.observable("")
    };
    
    self.isAssemblyFileValid = ko.observable(false);
    self.isAssemblyInGacList = ko.observable(false);
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
                return self.allGacAssemblies();
            }
            var filteredList = ko.utils.arrayFilter(self.allGacAssemblies(), function (assembly) {
                return assembly.Text.toLowerCase().indexOf(term) !== -1;
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

    //get new intellisense results whenever assembly location ends with a slash
    //and do all validation
    self.data.assemblyLocation.subscribe(function (newvalue) {
        if (newvalue && newvalue != "") {
            if (newvalue.match(".dll") != null) {//is assembly a file?
                self.validateAssemblyFile(self.getFileFromPath(newvalue));
            } else {
                $assemblyFileLocation.removeClass("ui-autocomplete-loading");
                if (newvalue[newvalue.length - 1] == '\\') {
                    $.post("Service/PluginSources/GetDirectoryIntellisense" + window.location.search, self.data.assemblyLocation(), function(result) {
                        $assemblyFileLocation.autocomplete("option", "source", result);
                        $assemblyFileLocation.autocomplete("search");
                    });
                } else {
                    if (newvalue.match("GAC:") != null) {//is assembly a gac entry?
                        self.findInGacList(newvalue);
                    } else {//assembly is neither file nor gac entry
                        self.isAssemblyFileValid(false);
                        self.isAssemblyInGacList(false);
                    }
                }
            }
        } else {//assembly is blank
            self.isAssemblyFileValid(false);
            self.isAssemblyInGacList(false);
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
        var isValid = self.isAssemblyFileValid() || self.isAssemblyInGacList();//$.inArray(self.data.assemblyLocation().substr(4, self.data.assemblyLocation().length-4), self.allGacAssemblies()) > -1);
        if ($dialogContainerID) {
            $dialogSaveButton.button("option", "disabled", !isValid);
        }
        return isValid;
    });

    self.validateAssemblyFile = function (id) {
        $.post("Service/PluginSources/ValidateAssemblyImageFormat" + window.location.search, self.data.assemblyLocation(), function (data) {
            if (data.validationresult == "success") {
                self.isAssemblyFileValid(true);
                self.isAssemblyInGacList(false);
            } else {
                self.isAssemblyFileValid(false);
            }
        })
            .success(function () {
                self.updateHelpText(id);
            });
    };

    self.findInGacList = function (id) {
        if (id.match("GAC:") != null) {
            id = self.removeGacPrefix(id);
        }
        self.isAssemblyInGacList(false);
        self.allGacAssemblies().forEach(function (entry) {
            if (entry.Text == id) {
                self.isAssemblyInGacList(true);
                self.isAssemblyFileValid(false);
                return true;
            }
        });
    };

    self.updateHelpText = function (id) {
        if (id != null) {
            var text = "";
            if (id && id.match(".dll") == null && id.match("GAC:") == null && id.match("gacSearchTerm:") == null) {
                text = self.helpDictionary[id];
                text = text ? text : self.helpDictionary.default;
                text = text ? text : "";
            } else {
                if (id.match("GAC:") != null) {
                    text = "<h4>Global Cache</h4><p>You have selected " + id + "</p>";
                } 
                if (id.match(".dll") != null) {
                    if (self.isAssemblyFileValid()) {
                        text = "<h4>Plugin File</h4><p>You have selected " + id + "</p>";
                    } else {
                        text = "<h4>Plugin File</h4><p>" + id + " is an invalid assembly file</p>";
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
    $gacList.on("change", function() {
        self.updateHelpText($gacList.val());
        self.data.assemblyLocation($gacList.val());
    });
    $gacList.on("click", function() {
        self.updateHelpText($gacList.val());
        self.data.assemblyLocation($gacList.val());
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
    
    //Server drive letter init
    $.post("Service/PluginSources/GetRootDriveLetter" + window.location.search, "", function (driveLetterResult) {
        self.driveLetter = driveLetterResult[0].driveLetter;
    });

    //
    //GAC Assemblies Init
    //
    $.post("Service/PluginSources/GetGacList" + window.location.search, "", function (gacResult) {
        //populate full list
        self.allGacAssemblies(gacResult);
        //view full list
        self.refreshGacList(self.allGacAssemblies());
        //show selected item
        if (self.data.assemblyLocation() != null && self.data.assemblyLocation().match("GAC:") != null) {
            self.gacListScrollIntoView(self.data.assemblyLocation());
        }
    });
    
    //
    // Dynatree Init
    //
    self.treePathLoaded = true;
    $.post("Service/PluginSources/GetServerDirectoryTree" + window.location.search, "", function (fullResult) {
        $fileTree.dynatree({
            onCreate: function (node, nodeSpan) {
                if (!node.data.isFolder) {
                    self.removeExpander(node);//remove expander icon
                }
            },
            children: fullResult,
            onLazyRead: function (node) {
                if (node.data.isFolder) {
                    $.post("Service/PluginSources/GetServerDirectoryTree" + window.location.search, self.resolvePath(node), function (lazyResult) {
                        node.setLazyNodeStatus(DTNodeStatus_Ok);
                        if (lazyResult.ErrorMessage == null) {
                            node.addChild(lazyResult);
                        } else {
                            console.log(lazyResult.ErrorMessage);
                        }
                    })
                        .success(function () {
                            //highjack lazyload to load in assembly location into the tree (this avoids using loadKeyPath)
                            if (!self.treePathLoaded && self.data.assemblyLocation() != null) {
                                node.visit(function (childNode) {
                                    //use childNode.getLevel to find the assembly location part
                                    if (childNode.data.title == self.data.assemblyLocation().split("\\")[childNode.getLevel()]) {
                                        childNode.expand(true);//trigger recursive call
                                        if (!childNode.data.isFolder) {
                                            $fileTree.animate({ // animate the scrolling to the node
                                                scrollTop: ($(childNode.li).offset().top - 150) - $fileTree.offset().top + $fileTree.scrollTop()
                                            }, 'fast');
                                            self.updateHelpText(childNode.data.title);
                                            self.validateAssemblyFile(childNode.data.title);
                                        }
                                        return false;//stop searching
                                    }
                                });
                            }
                            if (node.getChildren() == null) {
                                self.removeExpander(node);//remove expander icon
                            }
                        });
                } else {
                    node.setLazyNodeStatus(DTNodeStatus_Ok);
                    self.treePathLoaded = true;//don't allow lazy load to be hijacked to load assembly paths anymore
                }
            },
            onClick: function (node, event) {
                if (!node.data.isFolder) {
                    self.data.assemblyLocation(self.driveLetter + ":\\" + self.resolvePath(node));
                    $assemblyFileLocation.removeClass("ui-autocomplete-loading");
                    self.updateHelpText(node.data.title);
                    self.validateAssemblyFile(node.data.title);
                }
            }
        });
    });

    self.removeExpander = function (node) {
        node.li.innerHTML = node.li.innerHTML.replace("dynatree-expander", "dynatree-noexpander");//TODO fix (the node shouldnt shift across)
        node.render();
    };

    self.load = function (theResourceID) {
        //
        //Form Init
        //
        $("#gacSearchTerm").value = "";
        $sourcetabs.tabs("option", "active", 0);
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
                if (self.data.assemblyLocation() != null && self.data.assemblyLocation().match("GAC:") != null) {
                    self.gacListScrollIntoView(self.data.assemblyLocation());
                } else {
                    if (self.data.assemblyLocation() != null && self.data.assemblyLocation().match(".dll") != null) {
                        self.loadTreePath();
                    }
                }
            });
    };

    //load self.assemblies into GACList. avoids duplicates
    self.refreshGacList = function (assemblyList) {
        assemblyList.sort(utils.textCaseInsensitiveSort);
        $gacList.empty();
        for (var indx = 0; indx < assemblyList.length; indx++) {
            //avoid duplicates
            if (indx == assemblyList.length - 1) {
                var option = document.createElement("option");
                option.text = assemblyList[indx].Text;
                option.value = "GAC:" + assemblyList[indx].Text;
                $gacList.append(option, null);
            } else {
                if (assemblyList[indx].Text != assemblyList[indx + 1].Text) {
                    //dont add element if its duplicated
                    var option = document.createElement("option");
                    option.text = assemblyList[indx].Text;
                    option.value = "GAC:" + assemblyList[indx].Text;
                    $gacList.append(option, null);
                }
            }
        }
    };

    //GAC list scroll into view
    self.gacListScrollIntoView = function (assembly) {
        $sourcetabs.tabs("option", "active", 1);
        $gacList.focus();
        $gacList.val(assembly);
        self.updateHelpText($gacList.val());
        if ($gacList.val() != null) {
            self.isAssemblyInGacList(true);
            self.isAssemblyFileValid(false);
        }
    };

    //load a path into the tree
    self.loadTreePath = function () {
        self.treePathLoaded = false;
        //childNode.expand(true) triggers lazyload which recursively loads all of assembly location into the dynatree (this is only on load, treePathLoaded is the flag to ensure this)
        var treeRoot = $fileTree.dynatree("getRoot");
        treeRoot.visit(function (childNode) {
            if (self.data.assemblyLocation() != null && childNode.data.title == self.data.assemblyLocation().split("\\")[childNode.getLevel()]) {
                childNode.expand(true);
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
        //trailing slashes trigger autocomplete so they are shaved off
        return fullNodePath.substr(0,fullNodePath.lastIndexOf('\\'));
    };

    self.getFileFromPath = function (path) {
        return path.substr(path.lastIndexOf("\\") + 1, path.length - path.lastIndexOf("\\"));
    };

    self.removeGacPrefix = function(name) {
        return name.substr(4, name.length - 4);
    };

    self.showDialog = function (sourceName, onSaveCompleted) {
        // NOTE: Should only be invoked from PluginService form!
        self.onSaveCompleted = onSaveCompleted;

        self.load(sourceName);

        // remove our title bar
        var $dlgTitle = $("div[id='header']");
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
        $("#pluginSourceContainer").height(400);
        
        //pad search box
        $("#gacSearchTerm").css("margin-left","116px");
        
        //2013.04.16: Ashley Lewis PBI 8721 TODO find out what this is for:
        $dialogContainerID.dialog("open");
    };
    
    self.createDialog = function ($containerID) {
        $dialogContainerID = $containerID;
        $containerID.dialog({
            resizable: false,
            autoOpen: false,
            modal: true,
            width: 'auto',
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
        $dialogSaveButton.next().attr("tabindex", "9");
    };

    if (!$dialogContainerID) {
        self.load(resourceID);
    }

    $(".ui-dialog.ui-widget.ui-widget-content.ui-corner-all.ui-front.ui-dialog-buttons.ui-draggable").removeAttr("width");
};


PluginSourceViewModel.create = function (pluginSourceContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();

    var pluginSourceViewModel = new PluginSourceViewModel(saveContainerID);
    ko.applyBindings(pluginSourceViewModel, document.getElementById(pluginSourceContainerID));
    return pluginSourceViewModel;
};