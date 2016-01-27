// Make this available to chrome debugger
//@ sourceURL=SaveViewModel.js  

function SaveViewModel(saveUri, baseViewModel, saveFormID, environment) {
    var self = this;
    var $saveForm = $("#" + saveFormID);
    var $savefileTree = $("#savefileTree");
    var $resourceFoldersScrollBox = $("#resourceFoldersScrollBox");
    var $resourceFoldersScrollBoxHeight = 240;
    var $resourceFolders = $("#resourceFolders");
    var $resourceName = $("#resourceName");
    var $newFolderDialog = $("#newFolderDialog");
    var $newFolderName = $("#newFolderName");
    var $dialogSaveButton = null;
    //2013.06.08: Ashley Lewis for PBI 9458
    self.category = getParameterByName("category");
    self.currentEnvironment = ko.observable(environment);
    self.inTitleEnvironment = false;
    self.filePaths = ko.observableArray();
    self.IsDialogLess = false;
    self.fullPath = "";
    self.selectedNode = null;
    self.onSaveCompleted = null;
    self.isWindowClosedOnSave = true;
    self.viewModel = baseViewModel;
    self.data = baseViewModel.data;
    self.isEditing = ko.observable(baseViewModel.isEditing);

    self.titleSearchString = "Save";

    self.HasKeyPressed = false;

    //2013.06.20: Ashley Lewis for bug 9786 - default folder selection + resource name help text
    //self.defaultFolderName = "Unassigned";
    self.helpDictionaryID = "SaveDialog";
    self.helpDictionary = {};
    self.updateHelpText = function(id) {
        var text = self.helpDictionary[id];
        text = text ? text : "";
        utils.updateSaveValidationSpan(baseViewModel.titleSearchString || self.titleSearchString, text);
    };
    $.post("Service/Help/GetDictionary" + window.location.search, self.helpDictionaryID, function(result) {
        self.helpDictionary = result;
        self.updateHelpText("default");
    });

    //2013.06.21: Ashley Lewis for bug 9786 - filter invalid characters
    self.attemptedResourceName = ko.computed({
        read: function() {
            return self.data.resourceName();
        },
        write: function(value) {
            if (!self.isValidName(value)) {
                self.data.resourceName(self.RemoveInvalidCharacters(value));
                self.data.resourceName.valueHasMutated();
            } else {
                self.data.resourceName(value);
            }
        },
        owner: self
    });

    self.resourceNames = ko.observableArray();
    self.resourceFolders = ko.observableArray();
    self.searchFolderTerm = ko.observable("");
    self.selectedFolderPath = ko.observable("");
    self.searchFolderResults = ko.computed(function() {
        var term = self.searchFolderTerm().toLowerCase();

        if (term == "") {
            return self.resourceFolders();
        }

        return ko.utils.arrayFilter(self.resourceFolders(), function(folder) {
            return folder.toLowerCase().indexOf(term) !== -1;
        });
    });
    
    self.searchFolderTerm.subscribe(function (filterText) {
        $(".dynatree-title").each(function () {
            var startNode = $savefileTree.dynatree("getRoot");
            startNode.visit(function (node) {
                if (filterText) {
                    node.parent.expand(true);
                    if (node.isVisible() && node.data.title) {
                        if (node.data.title.toLowerCase().substring(0, filterText.length) === filterText.toLowerCase()) {// Make sure we and all our parents are visible 
                            node.visitParents(function (parentNode) {
                                $(parentNode.li).show();
                                return (parentNode.parent != null);
                            }, true);
                            return 'skip';
                        }
                        else {
                            // Hide the node. 
                            $(node.li).hide();
                            return "";
                        }
                    }
                } else {
                    $(node.li).show();
                    node.expand(false);
                }
            });
        });
    });
    
    self.selectedFolderPath.subscribe(function (filterText) {
        $(".dynatree-title").each(function () {
            var startNode = $savefileTree.dynatree("getRoot");
            startNode.visit(function (node) {
                if (filterText) {
                    node.parent.expand(true);
                    if (node.isVisible() && node.data.title) {
                        if (node.data.key.toLowerCase() === filterText.toLowerCase()) {
                            self.fullPath = self.resolvePath(node);
                            self.data.resourcePath(self.fullPath);
                            self.selectedNode = node;
                            $(".dynatree-title", node.li.firstChild).addClass("dynatree-selectedtitle"); 
                            node.visitParents(function (parentNode) {
                                $(parentNode.li).show();
                                return (parentNode.parent != null);
                            }, true);
                            return "skip";
                        }
                        else {
                            // Hide the node. 
                            $(node.li).hide();
                            return "";
                        }
                    }
                } else {
                    $(node.li).show();
                    node.expand(false);
                }
            });
        });
    });

    var baseUrl = utils.parseBaseURL(window.location + "");

    self.treePathLoaded = true;
    
    self.initializeDynatree = function () {
        $.ajax({
            type: 'POST',
            url: baseUrl + "/Services/GetDirectoriesRelativeToServerService",
            data: 'Directory=Resources',
            success: function (fullResult) {
                $savefileTree.dynatree({
                    checkbox: false,
                    clickFolderMode: 1,
                    selectMode: 1,
                    children: fullResult,
                    minExpandLevel: 1,
                    onClick: function (node, event) {
                        node.toggleSelect();
                    },
                    onSelect: function (isSelected, node) {
                        self.data.resourcePath(self.resolvePath(node));
                        getNameForSubFolder(self.fullPath);
                        $(".dynatree-title", $savefileTree).removeClass("dynatree-selectedtitle");
                        $(".dynatree-title", node.li.firstChild).addClass("dynatree-selectedtitle");
                    },
                });
                
                if (self.category) {
                    self.selectedFolderPath(self.category);
                }
                else {
                    self.selectedFolderPath("root");
                }
                getNameForSubFolder(self.fullPath);
            },
            dataType: 'json',
            async: false
        });
    };
    
    self.resolvePath = function(node) {
        self.fullPath = node.data.key;
        self.selectedNode = node;
        return self.fullPath;
    };

    self.removeExpander = function(node) {
        var $li = $(node.li);
        var $expander = $li.find(".dynatree-expander");
        $expander.remove();

        $li.css("margin-left", "16px");
    };

    self.loadTreePath = function() {
        self.initializeDynatree();
    };

    self.load = function() {
        self.treePathLoaded = false;
        self.loadTreePath();
    };

    var getNameForSubFolder = function (fullpath) {
        $.post("Service/Resources/PathsAndNames", fullpath, function (result) {
            if (!result.Paths) {
                self.resourceFolders([]);
            } else {
                self.resourceFolders(result.Paths);
                self.resourceFolders.sort(utils.caseInsensitiveSort);
            }
            if (!result.Names) {
                self.resourceNames([]);
            } else {
                self.resourceNames(result.Names);
                self.resourceNames.sort(utils.caseInsensitiveSort);
            }
        });
    };
    
    self.AddUnassignedFolder = function() {
        if (self.resourceFolders().length > 0) {
            var resourcePath = self.data.resourcePath();
            if (resourcePath == "") {
                self.data.resourcePath(self.fullPath);
                self.selectFolder(self.fullPath);
            }
        } else {
            self.resourceFolders.push(self.fullPath);
            //2013.06.20: Ashley Lewis for bug 9786 - default folder selection
            self.data.resourcePath(self.fullPath);
            self.selectFolder(self.fullPath);
        }
    };

    self.clearFilter = function() {
        self.searchFolderTerm("");
    };

    self.hasFilter = ko.computed(function() {
        return self.searchFolderTerm() !== "";
    });

    utils.makeClearFilterButton("clearSaveFilterButton");

    self.isValidName = function(name) {
        var result = /^[a-zA-Z0-9._\s-]+$/.test(name);
        return result;
    };

    self.clearFilter = function() {
        self.searchFolderTerm("");
    };

    self.hasFilter = ko.computed(function() {
        return self.searchFolderTerm() !== "";
    });

    utils.makeClearFilterButton("clearSaveFilterButton");

    self.isValidName = function(name) {
        if ($.trim(name).length == 0) {
            return false;
        } else {
            var result = /^[a-zA-Z0-9._\s-]+$/.test(name);
            return result;
        }
    };

    self.RemoveInvalidCharacters = function(name) {
        var newName = $.trim(name);
        while (!self.isValidName(newName) && newName !== "") {
            newName = newName.replace(/[^a-zA-Z0-9._\s-]/, "");
        }
        if (newName == "") {
            self.updateHelpText("default");
        }
        return newName;
    };

    self.enableSaveButton = function(enabled) {
        if ($dialogSaveButton && $dialogSaveButton.length > 0) {
            $dialogSaveButton.button("option", "disabled", !enabled);
        }
    };

    self.isNewFolderNameValid = function() {
        var name = $newFolderName.val().toLowerCase().trim();
        var isValid = false;
        if (name !== "" && self.isValidName(name)) {
            var matches = ko.utils.arrayFilter(self.resourceFolders(), function(folder) {
                return folder.toLowerCase() === name;
            });
            isValid = matches.length == 0 && name.toLowerCase() !== "root";
        }
        self.enableNewFolderOkButton(isValid);
        return isValid;
    };

    self.isResourceNameValid = function() {

        var name = self.data.resourceName() != null ? self.data.resourceName().toLowerCase() : "";

        // if a blank name is can never be valid
        if (name == "") {
            return false;
        }

        if (self.HasKeyPressed) {
            var isValid = name !== "" && self.isValidName(name);
            if (!self.isEditing() && isValid) {
                // check for duplicates
                var matches = ko.utils.arrayFilter(self.resourceNames(), function(resourceName) {
                    return resourceName.toLowerCase() === name.trim();
                });
                if (matches.length == 0) {
                    isValid = true;
                    // self.data
                } else {
                    isValid = false;
                    self.updateHelpText("DuplicateFound");
                }
            }

            self.enableSaveButton(isValid);
            return isValid;
        }

        return true;
    };

    $newFolderName.keyup(function(e) {
        if (self.isNewFolderNameValid()) {
            // ENTER key pressed
            if (e.keyCode == 13) {
                $newFolderDialog.dialog("close");
                self.addNewFolder();
            }
        }
    }).keydown(function(e) {
        if (e.which == 13) {
            e.preventDefault();
        }
    });

    $resourceName.keyup(function(e) {
        // ENTER key pressedresourceName
        if (e.keyCode == 13) {
            self.save();
        }
    }).keydown(function(e) {
        if (e.which == 13) {
            e.preventDefault();
        }

        if (!self.HasKeyPressed) {
            self.HasKeyPressed = true;
        }
    });

    self.newFolder = function() {
        var folder = $newFolderName;
        if (folder) {
            folder.val("");
            self.enableNewFolderOkButton(false);
            $newFolderDialog.dialog("open");
        }
    };

    self.addNewFolder = function() {
        var folder = $newFolderName;
        if (folder != undefined) {
            var folderName = $newFolderName.val();
            if (folderName != undefined) {

                var param = {
                    DisplayName: folderName,
                    ResourceType: 512,
                    ResourcePath: self.fullPath + '\\' + folderName,
                    Permissions: 0,
                    Parent: null,
                    Children: null
                };

                $.post(baseUrl + "/Services/AddFolderService", "itemToAdd=" + JSON.stringify(param), function(result) {

                    var selectedNode = self.selectedNode ? self.selectedNode :
                        $savefileTree.dynatree("getRoot");

                    if (result.ErrorMessage == null) {
                        var childNode = selectedNode.addChild({
                            title: folderName,
                            isFolder: true,
                            activate: true,
                            expand:true
                        });
                        
                        self.fullPath = param.ResourcePath;
                        self.data.resourcePath(self.fullPath);
                        self.selectedNode = childNode;
                        selectedNode.expand(true);
                        $savefileTree.scrollTop(($(childNode.li).offset().top - 150) - $savefileTree.offset().top + $savefileTree.scrollTop());
                        $(".dynatree-title", $savefileTree).removeClass("dynatree-selectedtitle"); //every list item in the tree
                        $(".dynatree-title", childNode.li.firstChild).addClass("dynatree-selectedtitle"); //just the selected list item
                    } else {
                        console.log(result.ErrorMessage);
                    }
                });
            }
        }
    };

    self.selectFolder = function(folderName) {
        utils.selectAndScrollToListItem(folderName.toUpperCase(), $resourceFoldersScrollBox, $resourceFoldersScrollBoxHeight);
    };

    self.isFormValid = function() {
        var isValidPath = self.data.resourcePath() ? true : false;
        var isValid = isValidPath && self.isResourceNameValid();
        self.enableSaveButton(isValid);
        if (isValid) {
            self.updateHelpText("");
        }
        return isValid;
    };

    self.isSaveFormValid = ko.computed(function() {
        return self.isFormValid();
    });

    self.createNewFolderDialog = function() {
        if ($newFolderDialog.length > 0) {
            $newFolderDialog.dialog({
                resizable: false,
                autoOpen: false,
                modal: true,
                position: utils.getDialogPosition(),
                buttons: {
                    "Add Folder": function () {
                        self.addNewFolder();
                        $(this).dialog("close");
                    },
                    Cancel: function() {
                        $(this).dialog("close");
                    }
                }
            });
        }

        var $newFolderOkButton = $(".ui-dialog-buttonpane button:contains('Add Folder')");
        self.enableNewFolderOkButton = function(enabled) {
            $newFolderOkButton.button("option", "disabled", !enabled);
        };
    };

    utils.registerSelectHandler($resourceFolders, function(selectedItem) {
        self.data.resourcePath(selectedItem);
    }, self.isReadOnly);

    self.save = function() {

        // there is an issue when not showing the save dialog on edit where the darn thing never closes
        // the IsDialogLess property is here to avoid annoying behavior
        if (!self.IsDialogLess && !self.isFormValid()) {
            return;
        }
        self.enableSaveButton(false);
        var jsonData = ko.toJSON(self.data);

        if (saveUri) {
            $.post(saveUri + window.location.search, jsonData, function(result) {
                if (!result.IsValid) {
                    $saveForm.dialog("close");
                    if (self.onSaveCompleted != null) {
                        self.onSaveCompleted(result);
                    }
                    if (self.isWindowClosedOnSave) {
                        studio.saveAndClose(result);
                    } else {
                        studio.save(result);
                    }
                }
            });
        } else {
            //This is done because C# escapes double qoutes
            jsonData = jsonData.replace(/"/g, "'");
            if (self.isWindowClosedOnSave) {
                studio.saveAndClose(jsonData);
            } else {
                studio.save(jsonData);
            }
        }
    };

    self.cancel = function() {
        if (!SaveViewModel.IsStandAlone && saveUri) {
            $(this).dialog("close");
        } else {
            studio.cancel();
        }
        return true;
    };

    self.showEnvironmentInDialogTitle = function() {
        //2013.06.09: Ashley Lewis for PBI 9458 - Show server in dialog title
        if (self.currentEnvironment() && self.inTitleEnvironment == false) {
            utils.appendEnvironmentSpanSave(baseViewModel.titleSearchString || self.titleSearchString, self.currentEnvironment());
            self.inTitleEnvironment = true;
        }
    };

    self.showDialog = function (isWindowClosedOnSave, onSaveCompleted) {
        self.load();
        $savefileTree.dynatree("getTree").reload();
        var searchText = self.category.toLowerCase();
        if (searchText.trim().length == 0) {
            searchText = "Root";
        }
        self.selectedFolderPath(searchText);
        self.isEditing(baseViewModel.isEditing);
        self.isFormValid();
        self.isWindowClosedOnSave = isWindowClosedOnSave;
        self.onSaveCompleted = onSaveCompleted;
        $saveForm.dialog("open");
        //2013.06.09: Ashley Lewis for PBI 9458 - Show server in dialog title
        self.showEnvironmentInDialogTitle();
    };

    self.createDialog = function() {
        if ($saveForm.length == 1) {
            $saveForm.dialog({
                resizable: false,
                autoOpen: false,
                width: 600,
                modal: true,
                position: utils.getDialogPosition(),
                open: function() {
                    self.enableSaveButton(self.data.resourceName());
                    var resourcePath = self.data.resourcePath();
                    if (resourcePath) {
                        self.selectFolder(resourcePath);
                    } else {
                        //2013.06.20: Ashley Lewis for bug 9786 - default folder selection
                        self.data.resourcePath(self.fullPath);
                        self.selectFolder(self.fullPath);
                    }
                },
                buttons: [{
                        text: "Save",
                        tabindex: 3,
                        click: self.save                    
                    }, {
                        text: "Cancel",
                        tabindex: 4,
                        click: self.cancel
                    }]
            });
        }

        // remove title and button bar
        var $titleBar = $("div[id='header']");
        if ($titleBar) {
            $titleBar.hide();
        }
        var $buttonBar = $("div[id='saveDialogButtonBar']");
        if ($buttonBar) {
            $buttonBar.hide();
        }

        try {
            $dialogSaveButton = $("div[aria-describedby=" + saveFormID + "] .ui-dialog-buttonpane button:contains('Save')");
            $dialogSaveButton.attr("tabindex", "105");
            $dialogSaveButton.next().attr("tabindex", "106");
        } catch(e) {
            //for testing without an html front end, jquery throws exception
            if (e.message === "Syntax error, unrecognized expression: div[aria-describedby=test form] .ui-dialog-buttonpane button:contains('Save')") {
                console.log("no html front end");
            } else {
                throw e;
            }
        }
    };

    if (!SaveViewModel.IsStandAlone) {
        self.createDialog();
    }
    
    self.createNewFolderDialog();

    self.load();
};

SaveViewModel.IsStandAlone = true;

SaveViewModel.create = function(saveUri, baseViewModel, containerID) {
    // MUST get dialog content synchronously!
    $.ajax("Views/Dialogs/SaveDialog.htm", {
        async: false,
        complete: function(jqXhr) {

            // MUST set this BEFORE getting html otherwise you will get standalone version!
            SaveViewModel.IsStandAlone = false;
            $("#" + containerID).html(jqXhr.responseText);
        }
    });

    // apply jquery-ui themes
    var $button = $("button");
    if ($button.length > 0) {
        $("button").button();
    }

    // ensure form id is unique
    var saveFormID = containerID + "SaveForm";
    $("#" + containerID + " #saveForm").attr("id", saveFormID);
    var saveForm = document.getElementById(saveFormID);

    var model = new SaveViewModel(saveUri, baseViewModel, saveFormID, baseViewModel.currentEnvironment());
    ko.applyBindings(model, saveForm);

    return model;
};


SaveViewModel.showStandAlone = function() {
    var resourceID = getParameterByName("rid");
    var resourceType = getParameterByName("type");
    var resourcePath = getParameterByName("path");
    var name = getParameterByName("name");
    var title = getParameterByName("title");
    
    var baseViewModel = {
        saveTitle: "<b>" + title + "" + "</b>",
        isEditing: resourceID ? resourceID != "" : false,
        data: {
            resourceID: ko.observable(resourceID),
            resourceType: ko.observable(resourceType),
            resourceName: ko.observable(name),
            resourcePath: ko.observable(resourcePath)
        },
        titleSearchString: "Workflow"
    };

    // apply jquery-ui themes
    $("button").button();

    // ensure form id is unique
    var saveFormID = "saveForm";
    var saveForm = document.getElementById(saveFormID);

    var $saveForm = $("#" + saveFormID);
    //$saveForm.wrap("<div id='SaveContainer' class='ui-widget ui-widget-content' style='width:569px; height: 460px;' />");
    $saveForm.wrap("<div id='SaveContainer' class='ui-widget ui-widget-content' style='width:600px; height: 456px;' />");
    $saveForm.attr("title", "");

    $("#dialog-save").addClass("ui-dialog ui-dialog-content").attr("style", "width: auto; padding: .5em 1em;");

    var $resourceName = $("#resourceName");
    var resourceNameStyle = $resourceName.attr("style");
    $resourceName.attr("style", resourceNameStyle.replace("margin-right:7px;", "margin-right:5px;"));

    var model = new SaveViewModel(null, baseViewModel, saveFormID, utils.decodeFullStops(getParameterByName("envir")));

    ko.applyBindings(model, saveForm);
    model.showEnvironmentInDialogTitle();
};

