function ServerViewModel(saveContainerID, environment) {
    var self = this;
    var $testButton = $("#testButton");
    var $address = $("#address");
    var $userName = $("#userName");
    var $password = $("#password");
    
    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("Server"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        address: ko.observable(""),
        authenticationType: ko.observable("User"),
        userName: ko.observable(""),
        password: ko.observable(""),
        webServerPort: ko.observable(3142)
    };
    
    self.currentEnvironment = ko.observable(environment); //2013.06.08: Ashley Lewis for PBI 9458 - Show server

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

    if ($address.length == 1) {
        $address.autocomplete({
            minLength: 0,
            source: [],
            select: function(event, ui) {
                var addr = "http://" + ui.item.value + ":3142/dsf";
                self.data.address(addr);
                $address.removeClass("ui-autocomplete-loading");
                return false;
            }
        });
    }

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

    self.ServerUrlOnKeyDownEvent = function (elem, e) {
        if (e.keyCode == 13 && !self.isReadOnly) {
            self.test();
        }
        return true;
    };

    self.data.authenticationType.subscribe(function (newValue) {
        var isUser = newValue == "User";
        if (isUser) {
            $userName.addClass("required");
            $password.addClass("required");
        }
        else {
            $userName.removeClass("required");
            $password.removeClass("required");
        }
        $userName.removeClass("error");
        $password.removeClass("error");

        self.isUserInputVisible(isUser);
    });

    self.isUserInputVisible = ko.observable(true);
    self.helpDictionaryID = "Server";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    self.isHelpTextVisible = ko.observable(true);
    self.isReadOnly = false;

    self.isTestResultsLoading = ko.observable(false);
    self.showTestResults = ko.observable(false);
    self.testSucceeded = ko.observable(false);
    self.testError = ko.observable("");
    
    self.isFormTestable = ko.computed(function () {
        var valid = self.data.address() ? true : false;
        if (self.isUserInputVisible()) {
            valid = valid && self.data.userName() && self.data.password();
        }
        self.showTestResults(false);
        return valid;
    });
    
    self.isFormValid = ko.computed(function () {
        return  self.isFormTestable() && self.showTestResults() && !self.testError();
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

    self.testTime = 0;

    self.test = function () {
        $testButton.button("option", "disabled", true);
        self.showTestResults(false);
        self.isTestResultsLoading(true);
        self.testSucceeded(false);
        
        var jsonData = ko.toJSON(self.data);
        
        utils.postTimestamped(self, "testTime", "Service/Connections/Test", jsonData, function(result) {
            if (!self.isReadOnly) {
                $testButton.button("option", "disabled", false);
            }
            self.isTestResultsLoading(false);
            self.showTestResults(true);
            self.testSucceeded(result.IsValid);
            self.testError(result.ErrorMessage);
        });
    };

    $.post("Service/Help/GetDictionary" + window.location.search, self.helpDictionaryID, function (result) {
        self.helpDictionary = result;
        self.updateHelpText("default");
    });

    $(":input").focus(function () {
        self.updateHelpText(this.id);
    });
    
    self.saveViewModel = SaveViewModel.create("Service/Connections/Save", self, saveContainerID);

    self.save = function () {
        self.saveViewModel.showDialog(true);
    };
};


ServerViewModel.create = function (serverContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();

    var serverViewModel = new ServerViewModel(saveContainerID, utils.decodeFullStops(getParameterByName("envir")));
    ko.applyBindings(serverViewModel, document.getElementById(serverContainerID));
};