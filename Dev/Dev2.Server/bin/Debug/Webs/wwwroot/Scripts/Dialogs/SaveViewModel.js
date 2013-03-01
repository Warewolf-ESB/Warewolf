function SaveViewModel(saveUri, baseViewModel, saveFormID) {
    var self = this;
    var $saveForm = $("#" + saveFormID);
    var $errDiv = $("#saveFormError");
    var $errDivSpan = $("#saveFormError span");
    var $resourceFoldersScrollBox = $("#resourceFoldersScrollBox");
    var $resourceFoldersScrollBoxHeight = 240;
    var $resourceFolders = $("#resourceFolders");
    var $resourceName = $("#resourceName");
    var $newFolderDialog = $("#newFolderDialog");
    var $newFolderName = $("#newFolderName");

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

    self.save = function () {
        if (!self.validate()) {
            return;
        }

        var jsonData = ko.toJSON(self.data);
        $.post(saveUri + window.location.search, jsonData, function (result) {
            if (!result.IsValid) {
                Dev2Awesomium.Cancel();
                Dev2Awesomium.Save(JSON.stringify(result));
            } else {
            // TODO: ShowError use $errDiv?
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
        var name = self.data.resourceName().toLowerCase();
        var isValid = false;
        if (name !== "" && self.isValidName(name)) {
            var matches = ko.utils.arrayFilter(self.resourceNames(), function (resourceName) {
                return resourceName.toLowerCase() === name;
            });
            isValid = matches.length == 0;
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
        if (self.isResourceNameValid()) {
            // ENTER key pressed
            if (e.keyCode == 13) {
                $saveForm.dialog("close");
            }
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

    self.toggleValidationError = function ($elem, isValid) {
        if (isValid) {
            $elem.removeClass("error");
            $elem.addClass("valid");
        } else {
            $elem.removeClass("valid");
            $elem.addClass("error");
        }
    };

    self.validate = function () {
        var isValidPath = self.data.resourcePath() !== "";
        var isValidName = self.data.resourceName() !== "";

        self.toggleValidationError($resourceFoldersScrollBox, isValidPath);
        self.toggleValidationError($resourceName, isValidName);
        $errDiv.hide();
        if (!(isValidPath && isValidName)) {
            var message = (isValidPath || isValidName)
                   ? 'You missed 1 field. It has been highlighted above.'
                   : 'You missed 2 fields. They have been highlighted above.';

            $errDivSpan.html(message);
            $errDiv.show();
            return false;
        }

        return true;
    };

    $resourceName.blur(function () {
        self.validate();
    });

    $saveForm.validate({
        onkeyup: false
    });

    $newFolderDialog.dialog({
        resizable: false,
        autoOpen: false,
        modal: true,
        position: utils.GetDialogPosition(),
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

    var $saveButton = $(".ui-dialog-buttonpane button:contains('Save')");
    self.enableSaveButton = function (enabled) {
        $saveButton.button("option", "disabled", !enabled);
    };

    utils.registerSelectHandler($resourceFolders, function (selectedItem) {
        self.data.resourcePath(selectedItem);
        self.validate();
    });

    self.showDialog = function () {
        $saveForm.dialog("open");
    };

    $saveForm.dialog({
        resizable: false,
        autoOpen: false,
        height: 453,
        width: 600,
        modal: true,
        position: utils.GetDialogPosition(),
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

    $("#saveFormError").prependTo("div.ui-dialog-buttonpane");

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