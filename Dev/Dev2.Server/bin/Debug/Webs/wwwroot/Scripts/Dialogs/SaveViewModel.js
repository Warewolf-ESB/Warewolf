function SaveViewModel(saveUri, baseViewModel, saveFormID) {
    var self = this;
    var $saveForm = $("#" + saveFormID);
    var $resourceFoldersScrollBox = $("#resourceFoldersScrollBox");
    var $resourceFoldersScrollBoxHeight = 240;
    var $resourceFolders = $("#resourceFolders");
    var $resourceName = $("#resourceName");
    var $newFolderDialog = $("#newFolderDialog");
    var $newFolderName = $("#newFolderName");
    var $dialogSaveButton = null;
    
    self.onSaveCompleted = null;
    self.isWindowClosedOnSave = true;
    self.viewModel = baseViewModel;
    self.data = baseViewModel.data;
    self.isEditing = baseViewModel.isEditing;

    $.post("Service/Resources/PathsAndNames" + window.location.search, self.data.resourceType(), function (result) {
        self.resourceFolders(result.Paths);
        self.resourceFolders.sort(utils.caseInsensitiveSort);
        self.resourceNames(result.Names);
        self.resourceNames.sort(utils.caseInsensitiveSort);
    });

    self.resourceNames = ko.observableArray();
    self.resourceFolders = ko.observableArray();
    self.searchFolderTerm = ko.observable("");
    self.searchFolderResults = ko.computed(function () {
        var term = self.searchFolderTerm().toLowerCase();
        if (term == "") {
            return self.resourceFolders();
        }
        return ko.utils.arrayFilter(self.resourceFolders(), function (folder) {
            return folder.toLowerCase().indexOf(term) !== -1;
        });
    });

    self.isValidName = function (name) {
        var result = /^[a-zA-Z0-9._\s-]+$/.test(name);
        return result;
    };

    self.enableSaveButton = function (enabled) {
        if ($dialogSaveButton) {
            $dialogSaveButton.button("option", "disabled", !enabled);
        }
    };
    
    self.save = function () {
        if (!self.isFormValid()) {
            return;
        }
        var jsonData = ko.toJSON(self.data);
        $.post(saveUri + window.location.search, jsonData, function (result) {
            if (!result.IsValid) {
                $saveForm.dialog("close");
                if (self.onSaveCompleted != null) {
                    self.onSaveCompleted();
                }
                if (self.isWindowClosedOnSave) {
                    studio.saveAndClose(result);
                } else {
                    studio.save(result);
                }
            }
        });
    };

    self.isNewFolderNameValid = function () {
        var name = $newFolderName.val().toLowerCase();
        var isValid = false;
        if (name !== "" && self.isValidName(name)) {
            var matches = ko.utils.arrayFilter(self.resourceFolders(), function (folder) {
                return folder.toLowerCase() === name;
            });
            isValid = matches.length == 0;
        }
        self.enableNewFolderOkButton(isValid);
        return isValid;
    };

    self.isResourceNameValid = function () {
        var name = self.data.resourceName() != null ? self.data.resourceName().toLowerCase() : "";
        var isValid = name !== "" && self.isValidName(name);
        if (!self.isEditing && isValid) {
            // check for duplicates
            var matches = ko.utils.arrayFilter(self.resourceNames(), function (resourceName) {
                return resourceName.toLowerCase() === name;
            });
            isValid = matches.length == 0;
            console.log("    matches : " + isValid);
        }
        self.enableSaveButton(isValid);
        return isValid;
    };

    $newFolderName.keyup(function (e) {
        if (self.isNewFolderNameValid()) {
            // ENTER key pressed
            if (e.keyCode == 13) {
                $newFolderDialog.dialog("close");
                self.addNewFolder();
            }
        }
    });

    $resourceName.keyup(function (e) {
        // ENTER key pressed
        if (e.keyCode == 13) {
            self.save();
        }
    });

    self.newFolder = function () {
        $newFolderName.val("");
        self.enableNewFolderOkButton(false);
        $newFolderDialog.dialog("open");
    };

    self.addNewFolder = function () {
        var folderName = $newFolderName.val();
        if (folderName !== "") {
            self.resourceFolders.push(folderName);
            self.resourceFolders.sort(self.caseInsensitiveSort);
            self.data.resourcePath(folderName);
            self.selectFolder(folderName);
        }
    };

    self.selectFolder = function (folderName) {
        utils.selectAndScrollToListItem(folderName, $resourceFoldersScrollBox, $resourceFoldersScrollBoxHeight);
    };

    self.isFormValid = function () {
        var isValidPath = self.data.resourcePath() ? true : false;
        var isValid = isValidPath && self.isResourceNameValid();
        self.enableSaveButton(isValid);
        return isValid;
    };
    
    self.isSaveFormValid = ko.computed(function () {        
        return self.isFormValid();
    });     

    self.createNewFolderDialog = function() {
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
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });

        var $newFolderOkButton = $(".ui-dialog-buttonpane button:contains('Add Folder')");
        self.enableNewFolderOkButton = function (enabled) {
            $newFolderOkButton.button("option", "disabled", !enabled);
        };
    };

    utils.registerSelectHandler($resourceFolders, function (selectedItem) {
        self.data.resourcePath(selectedItem);
    });

    self.showDialog = function (isWindowClosedOnSave, onSaveCompleted) {        
        self.isFormValid();
        self.isWindowClosedOnSave = isWindowClosedOnSave;
        self.onSaveCompleted = onSaveCompleted;
        $saveForm.dialog("open");
    };
    
    self.createDialog = function() {
        $saveForm.dialog({
            resizable: false,
            autoOpen: false,
            height: 453,
            width: 600,
            modal: true,
            position: utils.getDialogPosition(),
            open: function (event, ui) {
                self.enableSaveButton(self.data.resourceName());
                var resourcePath = self.data.resourcePath();
                if (resourcePath) {
                    self.selectFolder(resourcePath);
                }
            },
            buttons: [{
                text: "Save",
                tabindex: 3,
                click: self.save
            }, {
                text: "Cancel",
                tabindex: 4,
                click: function () {
                    $(this).dialog("close");
                }
            }]
        });

        $dialogSaveButton = $("div[aria-describedby=" + saveFormID + "] .ui-dialog-buttonpane button:contains('Save')");
    };

    self.createDialog();
    self.createNewFolderDialog();
};

SaveViewModel.create = function (saveUri, baseViewModel, containerID) {
    
    // MUST get dialog content synchronously!
    $.ajax("Views/Dialogs/SaveDialog.htm", {
        async: false,        
        complete: function (jqXhr) {
            $("#" + containerID).html(jqXhr.responseText);
        }
    });
    
    // apply jquery-ui themes
    $("button").button();

    // ensure form id is unique
    var saveFormID = containerID + "SaveForm";
    $("#" + containerID + " #saveForm").attr("id", saveFormID);
    var saveForm = document.getElementById(saveFormID);

    var model = new SaveViewModel(saveUri, baseViewModel, saveFormID);
    ko.applyBindings(model, saveForm);
    
    return model;
};