// Make this available to chrome debugger
//@ sourceURL=EmailSourceViewModel.js  

function EmailSourceViewModel(saveContainerID, environment) {
    var self = this;
    var $testButton = $("#testButton");
    var $sendTestEmailDialog = $("#sendTestEmailDialog");
    var $testFromAddress = $("#testFromAddress");
    var $testToAddress = $("#testToAddress");
    
    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("EmailSource"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        host: ko.observable(""),
        userName: ko.observable(""),
        password: ko.observable(""),
        port: ko.observable(25),
        enableSsl: ko.observable(false),
        timeout: ko.observable(100000),

        timeoutSeconds: ko.observable(100),
        
        testFromAddress: ko.observable(""),
        testToAddress: ko.observable("")
    };
    
    self.currentEnvironment = ko.observable(environment); //2013.06.08: Ashley Lewis for PBI 9458 - Show server

    self.data.timeoutSeconds.subscribe(function (newValue) {
        self.data.timeout(newValue * 1000);
    });
    
    // Fix for binding true / false to radio buttons in Knockout JS
    self.data.enableSsl.forEditing = ko.computed({
        read: function () {
            return self.data.enableSsl().toString();
        },
        write: function (newValue) {
            self.data.enableSsl(newValue === "true");
        },
        owner: this
    });
    
    self.data.enableSsl.subscribe(function (newValue) {
        self.updateHelpText("enableSsl");
    });
    
    var resourceID = getParameterByName("rid");
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    self.data.resourceID(self.isEditing ? resourceID : $.Guid.Empty());

    self.title = ko.observable("New Email Source");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });

    self.saveTitle = ko.computed(function () {
        return self.data ? "<b>Server:</b> " + self.data.host() : "";
    });

    $.post("Service/EmailSources/Get" + window.location.search, self.data.resourceID(), function (result) {
        self.data.resourceID(result.ResourceID);
        self.data.resourceType(result.ResourceType);
        self.data.resourceName(result.ResourceName);
        self.data.resourcePath(result.ResourcePath);

        self.data.host(result.Host);
        self.data.userName(result.UserName);
        self.data.password(result.Password);
        self.data.enableSsl(result.EnableSsl);
        self.data.port(result.Port); // MUST set after EnableSsl because KO subscription auto-updates port
        self.data.timeoutSeconds(result.Timeout / 1000);

        // DO NOT set test email addresses!!

        self.title(self.isEditing ? "Edit Email Source - " + result.ResourceName : "New Email Source");
    });

    self.helpDictionaryID = "EmailSource";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    self.isHelpTextVisible = ko.observable(true);
    self.isReadOnly = false;

    self.isTestResultsLoading = ko.observable(false);
    self.showTestResults = ko.observable(false);
    self.testSucceeded = ko.observable(false);
    self.testError = ko.observable("");
    
    self.isFormTestable = ko.computed(function () {
        var valid = self.data.host() ? true : false;
        valid = valid && self.data.userName() ? true : false;
        valid = valid && self.data.password() ? true : false;
        valid = valid && self.data.port() ? true : false;
        valid = valid && self.data.timeout() ? true : false;

        // DO NOT check test email addresses!!
        
        self.showTestResults(false);
        return valid;
    });
    
    self.isFormValid = ko.computed(function () {
        return self.isFormTestable() && self.showTestResults() && !self.testError() && !self.isReadOnly;
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
    
    self.saveViewModel = SaveViewModel.create("Service/EmailSources/Save", self, saveContainerID);

    self.save = function () {
        self.saveViewModel.showDialog(true);
    };
 
    self.createTestDialog = function () {
        $sendTestEmailDialog.dialog({
            resizable: false,
            autoOpen: false,
            modal: true,
            position: utils.getDialogPosition(),
            buttons: {
                "Send": function () {
                    self.doTest();
                    $(this).dialog("close");
                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            }
        });

        var $testDialogOkButton = $(".ui-dialog-buttonpane button:contains('Send')");
        self.enableTestDialogOkButton = function (enabled) {
            $testDialogOkButton.button("option", "disabled", !enabled);
        };
        $testDialogOkButton.attr("tabindex", "106");
        $testDialogOkButton.next().attr("tabindex", "107");
        
        $(".ui-dialog-titlebar button:contains('close')").attr("tabindex", "108");
    };

    self.createTestDialog();
    
    self.data.userName.subscribe(function (newValue) {
        $testFromAddress.val(newValue);
    }); 

    self.isTestDialogValid = function () {
        var isValid = utils.isValidEmail($testFromAddress.val().toLowerCase())
            && utils.isValidEmail($testToAddress.val().toLowerCase());

        self.enableTestDialogOkButton(isValid);
        return isValid;
    };

    $testFromAddress.keyup(function (e) {
        if (self.isTestDialogValid()) {
            // ENTER key pressed
            if (e.keyCode == 13) {
                self.doTest();
                $sendTestEmailDialog.dialog("close");
            }
        }
    });
    
    $testToAddress.keyup(function (e) {
        if (self.isTestDialogValid()) {
            // ENTER key pressed
            if (e.keyCode == 13) {
                self.doTest();
                $sendTestEmailDialog.dialog("close");
            }
        }
    });
   
    self.test = function () {
        self.isTestDialogValid();
        $sendTestEmailDialog.dialog("open");
    };

    self.doTest = function () {
        $testButton.button("option", "disabled", true);
        self.showTestResults(false);
        self.isTestResultsLoading(true);
        self.testSucceeded(false);

        self.data.testFromAddress($testFromAddress.val());
        self.data.testToAddress($testToAddress.val());
        
        var jsonData = ko.toJSON(self.data);
        $.post("Service/EmailSources/Test" + window.location.search, jsonData, function (result) {
            $testButton.button("option", "disabled", false);
            self.isTestResultsLoading(false);
            self.showTestResults(true);
            self.testSucceeded(result.IsValid);
            self.testError(result.ErrorMessage);
        });
    };

    utils.isReadOnly(resourceID, function (isReadOnly) {
        self.isReadOnly = isReadOnly;
    });
};


EmailSourceViewModel.create = function (serverContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();

    var serverViewModel = new EmailSourceViewModel(saveContainerID, utils.decodeFullStops(getParameterByName("envir")));
    ko.applyBindings(serverViewModel, document.getElementById(serverContainerID));
};