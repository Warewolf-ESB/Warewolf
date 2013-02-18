function ServerViewModel(loadedCallback) {
    var self = this;
    var $testButton = $("#testButton");
    var $address = $("#address");
    
    self.saveUri = "Service/Connections/Save";

    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("Server"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        address: ko.observable(""),
        authenticationType: ko.observable("User"),
        userName: ko.observable(""),
        password: ko.observable(""),
        webServerPort: ko.observable(1234)
    };

    var resourceID = getParameterByName("rid");
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    self.data.resourceID(self.isEditing ? resourceID : $.Guid.Empty());

    self.title = ko.observable("New Server");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });

    self.saveTitle = ko.computed(function () {
        return self.data ? "<b>Server:</b> " + self.data.address() : "";
    });

    $address.autocomplete({      
        minLength: 0,        
        select: function (event, ui) {
            var addr = "http://" + ui.item.value + ":77/dsf";
            self.data.address(addr);
            $address.removeClass("ui-autocomplete-loading");
            return false;
        }
    });
    
    $.post("Service/Connections/Search" + window.location.search, "", function (result) {
        $address.autocomplete( "option", "source", result);
    });

    $.post("Service/Connections/Get" + window.location.search, self.data.resourceID(), function (result) {
        self.data.resourceID(result.ResourceID);
        self.data.resourceType(result.ResourceType);
        self.data.resourceName(result.ResourceName);
        self.data.resourcePath(result.ResourcePath);

        self.data.address(result.Address);
        self.data.authenticationType(result.AuthenticationType);
        self.data.userName(result.UserName);
        self.data.password(result.Password);
        self.data.webServerPort(result.WebServerPort);

        self.title(self.isEditing ? "Edit Server - " + result.ResourceName : "New Server");
    });

    self.data.authenticationType.subscribe(function (newValue) {
        var isUser = newValue == "User";
        if (isUser) {
            $("#userName").addClass("required");
            $("#password").addClass("required");
        }
        else {
            $("#userName").removeClass("required");
            $("#password").removeClass("required");
        }
        $("#userName").removeClass("error");
        $("#password").removeClass("error");

        self.testSucceeded = ko.observable(false);
        self.isUserInputVisible(isUser);
    });

    self.isUserInputVisible = ko.observable(true);
    self.helpDictionaryID = "Server";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    self.isHelpTextVisible = ko.observable(true);

    self.isTestResultsLoading = ko.observable(false);
    self.showTestResults = ko.observable(false);
    self.testSucceeded = ko.observable(false);
    self.testError = ko.observable("");
    
    self.isFormTestable = ko.computed(function() {
        self.testSucceeded(false);
        self.showTestResults(false);
        var valid = self.data.address() ? true : false;
        if (self.isUserInputVisible()) {
            valid = valid && self.data.userName() && self.data.password();
        }
        return valid;
    });
    
    self.updateHelpText = function (id) {
        var text = self.helpDictionary[id];
        text = text ? text : self.helpDictionary.default;
        text = text ? text : "";
        self.helpText(text);
    };
    
    self.cancel = function () {
        Dev2Awesomium.Cancel();
        return true;
    };
    
    self.test = function () {
        $testButton.button("option", "disabled", true);
        self.showTestResults(false);
        self.isTestResultsLoading(true);

        var data = $.extend(self.data, self.saveData);
        var jsonData = ko.toJSON(data);
        $.post("Service/Connections/Test" + window.location.search, jsonData, function (result) {
            $testButton.button("option", "disabled", false);
            self.isTestResultsLoading(false);
            self.showTestResults(true);
            self.testError(result.ErrorMessage);
            self.testSucceeded(result.IsValid);
        });
    };
    self.save = function () {
        $("#saveForm").dialog("open");
    };

    $.post("Service/Help/GetDictionary" + window.location.search, self.helpDictionaryID, function (result) {
        self.helpDictionary = result;
        self.updateHelpText("default");
    });

    $(":input").focus(function () {
        self.updateHelpText(this.id);
    });


    // inject SaveDialog
    $("#saveDialogContainer").load("Views/Dialogs/SaveDialog.htm", function () {
        if (loadedCallback) {
            loadedCallback();
        }
    });
};
