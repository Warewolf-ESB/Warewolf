// Make this available to chrome debugger
//@ sourceURL=WebSourceViewModel.js  

function WebSourceViewModel(saveContainerID, environment, resourceID) {
    var self = this;
    var $testButton = $("#testButton");
    var $address = $("#address");
    var $dialogContainerID = null;
    var $dialogSaveButton = null;
    //var $inspector = document.getElementById("resultInspector");

    //2013.06.08: Ashley Lewis for PBI 9458
    self.titleSearchString = "Web Source";
    self.currentEnvironment = ko.observable(environment);
    self.inTitleEnvironment = false;
    
    self.onSaveCompleted = null;
    
    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("WebSource"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        address: ko.observable(""),
        defaultQuery: ko.observable(""),
        authenticationType: ko.observable("Anonymous"),
        userName: ko.observable(""),
        password: ko.observable(""),
        
        response: ko.observable("")
    };

    self.requestUrl = ko.computed(function () {
        var result = (self.data.address() ? self.data.address() : "")
            + (self.data.defaultQuery() ? self.data.defaultQuery() : "");
        return result;
    });
    
    self.data.authenticationType.subscribe(function (newValue) {
        var isUser = newValue == "User";

        self.updateHelpText("authenticationType");
        self.isUserInputVisible(isUser);
    });
    self.isUserInputVisible = ko.observable(false);

    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    self.data.resourceID(self.isEditing ? resourceID : $.Guid.Empty());

    self.title = ko.observable("");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });

    self.saveTitle = ko.computed(function () {
        return (self.data ? "<b>Web Source:</b><br/> " + self.data.address() : "<b>Web Source</b>");
    });


    self.load = function(theResourceID) {
        $.post("Service/WebSources/Get" + window.location.search, theResourceID, function (result) {
            self.data.resourceID(result.ResourceID);
            self.data.resourceType(result.ResourceType);
            self.data.resourceName(result.ResourceName);
            self.data.resourcePath(result.ResourcePath);

            self.data.address(result.Address);
            self.data.defaultQuery(result.DefaultQuery);
            self.data.authenticationType(result.AuthenticationType);
            self.data.userName(result.UserName);
            self.data.password(result.Password);

            // DO NOT set test uri!!
           
            self.isEditing = result.ResourceName != null;

            if (self.isEditing && !self.isReadOnly) {
                self.test();
            }

            self.title(self.isEditing ? "Edit Web Source - " + result.ResourceName : "New Web Source");
            
            if ($dialogContainerID) {
                $dialogContainerID.dialog("option", "title", self.title());
            }
        });
        utils.isReadOnly(theResourceID, function (isReadOnly) {
            self.isReadOnly = isReadOnly;
        });
    };

    self.helpDictionaryID = "WebSource";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    self.isHelpTextVisible = ko.observable(true);
    self.isReadOnly = false;

    self.isTestResultsLoading = ko.observable(false);
    self.showTestResults = ko.observable(false);
    self.testSucceeded = ko.observable(false);
    self.testError = ko.observable("");
    
    self.isFormTestable = ko.computed(function () {
        
        if (self.isReadOnly) {
            return false;
        }
        
        var valid = self.data.address() ? utils.isValidUrl(self.data.address()) : false;

        if (self.isUserInputVisible()) {
            valid = valid && self.data.userName() ? true : false;
            valid = valid && self.data.password() ? true : false;
        }
        
        // DO NOT check test uri!!
        
        self.testSucceeded(false);
        self.showTestResults(false);
        return valid;
    });
    
    self.isFormValid = ko.computed(function () {
        //2013.06.21: Ashley Lewis for bug 9586 - validating some addresses is actually impossible
        var isValid = self.isFormTestable();// && self.showTestResults() && !self.testError();
        if ($dialogContainerID) {
            $dialogSaveButton.button("option", "disabled", !isValid);
        }
        return isValid && !self.isReadOnly;
    });
    
    self.updateHelpText = function (id) {
        var text = self.helpDictionary[id];
        text = text ? text : self.helpDictionary.default;
        text = text ? text : "";
        self.helpText(text);
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
    
    self.saveViewModel = SaveViewModel.create("Service/WebSources/Save", self, saveContainerID);

    self.save = function () {
        var isWindowClosedOnSave = $dialogContainerID ? false : true;
        if (self.data.resourceID() == "00000000-0000-0000-0000-000000000000") {
            self.saveViewModel.showDialog(isWindowClosedOnSave, function (result) {
                if (!isWindowClosedOnSave) {
                    $dialogContainerID.dialog("close");
                    if (self.onSaveCompleted != null) {
                        self.onSaveCompleted(result);
                    }
                };
            });
        } else {
            self.saveViewModel.IsDialogLess = true;
            self.saveViewModel.save();
        }
    };
     
    self.viewInBrowser = function () {
        return (window.open(self.requestUrl(), "_blank") != 'undefined');
    };

    self.testTime = 0;
    
    self.test = function () {
        $testButton.button("option", "disabled", true);
        self.showTestResults(false);
        self.isTestResultsLoading(true);
        self.testSucceeded(false);

        var jsonData = ko.toJSON(self.data);
        
        utils.postTimestamped(self, "testTime", "Service/WebSources/Test", jsonData, function(result) {
            if (!self.isReadOnly) {
                $testButton.button("option", "disabled", false);
            }
            self.isTestResultsLoading(false);
            self.showTestResults(true);
            self.testSucceeded(result.IsValid);
            self.testError(result.ErrorMessage);
        });
    };
    
    $address.keyup(function (e) {
        if (self.isFormTestable()) {
            // ENTER key pressed
            if (e.keyCode == 13 && !self.isReadOnly) {
                self.test();
            }
        }
    }).keydown(function (e) {
        if (e.which == 13) {
            e.preventDefault();
        }
    });
    
    self.createDialog = function ($containerID) {
        $dialogContainerID = $containerID;
        $containerID.dialog({
            resizable: false,
            autoOpen: false,
            modal: true,
            width: 712,
            position: utils.getDialogPosition(),
            buttons: {
                "Save Web Source": function () {
                    self.save();
                },
                "Cancel": function () {
                    $(this).dialog("close");
                }
            }
        });
        $("button").button();
        $dialogSaveButton = $(".ui-dialog-buttonpane button:contains('Save Web Source')");
        $dialogSaveButton.attr("tabindex", "59");
        $dialogSaveButton.next().attr("tabindex", "60");
        
        // remove title and button bar
        var $titleBar = $("div[id='webSourceHeader']");
        if ($titleBar) {
            $titleBar.hide();
        }
        var $buttonBar = $("div[id='webSourceButtonBar']");
        if ($buttonBar) {
            $buttonBar.hide();
        }
        
        // Adjust height for removed div's and remove annoying look and feel
        $webSourceContainer = $("#webSourceContainer");
        if ($webSourceContainer) {
            $webSourceContainer.height(400);
            $webSourceContainer.removeClass("ui-widget-content");
        }
        $("#webSourceContent").removeClass("source-content");
    };

    self.showDialog = function (sourceName, onSaveCompleted) {
        // NOTE: Should only be invoked from WebService form!
        self.onSaveCompleted = onSaveCompleted;
        self.load(sourceName);
        $dialogContainerID.dialog("open");

        //2013.06.09: Ashley Lewis for PBI 9458 - Show server in dialog title
        if (self.currentEnvironment() && self.inTitleEnvironment == false) {
            utils.appendEnvironmentSpan(self.titleSearchString, self.currentEnvironment());
            self.inTitleEnvironment = true;
        }
    };
    
    if (!$dialogContainerID) {
        self.load(resourceID);
    }
};


WebSourceViewModel.create = function (serverContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();

    var webSourceViewModel = new WebSourceViewModel(saveContainerID, utils.decodeFullStops(getParameterByName("envir")), getParameterByName("rid"));
    ko.applyBindings(webSourceViewModel, document.getElementById(serverContainerID));
    return webSourceViewModel;
};