// Make this available to chrome debugger
//@ sourceURL=WebSourceViewModel.js  

function WebSourceViewModel(saveContainerID) {
    var self = this;
    var $testButton = $("#testButton");
    var $address = $("#address");
    var $testRelativeUri = $("#testRelativeUri");
    //var $inspector = document.getElementById("resultInspector");
    
    self.data = {
        resourceID: ko.observable(""),
        resourceType: ko.observable("WebSource"),
        resourceName: ko.observable(""),
        resourcePath: ko.observable(""),

        address: ko.observable(""),
        authenticationType: ko.observable("Anonymous"),
        userName: ko.observable(""),
        password: ko.observable(""),
        
        testResult: ko.observable(""),
        testRelativeUri: ko.observable(""),
    };
        
    self.data.authenticationType.subscribe(function (newValue) {
        var isUser = newValue == "User";

        self.updateHelpText("authenticationType");
        self.isUserInputVisible(isUser);
    });
    self.isUserInputVisible = ko.observable(false);

    var resourceID = getParameterByName("rid");
    self.isEditing = $.Guid.IsValid(resourceID) && !$.Guid.IsEmpty(resourceID);
    self.data.resourceID(self.isEditing ? resourceID : $.Guid.Empty());

    self.title = ko.observable("New Web Source");
    self.title.subscribe(function (newValue) {
        document.title = newValue;
    });

    self.saveTitle = ko.computed(function () {
        return (self.data ? "<b>Web Source:</b><br/> " + self.data.address() : "<b>Web Source</b>");
    });

    $.post("Service/WebSources/Get" + window.location.search, self.data.resourceID(), function (result) {
        self.data.resourceID(result.ResourceID);
        self.data.resourceType(result.ResourceType);
        self.data.resourceName(result.ResourceName);
        self.data.resourcePath(result.ResourcePath);

        self.data.address(result.Address);
        self.data.authenticationType(result.AuthenticationType);
        self.data.userName(result.UserName);
        self.data.password(result.Password);

        // DO NOT set test uri!!

        self.title(self.isEditing ? "Edit Web Source - " + result.ResourceName : "New Web Source");
    });

    self.helpDictionaryID = "WebSource";
    self.helpDictionary = {};
    self.helpText = ko.observable("");
    self.isHelpTextVisible = ko.observable(true);

    self.isTestResultsLoading = ko.observable(false);
    self.showTestResults = ko.observable(false);
    self.testSucceeded = ko.observable(false);
    self.testError = ko.observable("");
    
    self.isFormTestable = ko.computed(function () {
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
        return self.isFormTestable() && self.showTestResults() && !self.testError();
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
        self.saveViewModel.showDialog(true);
    };
     
    self.viewInBrowser = function () {
        if (studio.isAvailable()) {
            studio.navigateTo(self.data.address());
        } else {
            window.open(self.data.address(), "_blank");
        }
    };

    self.test = function () {
        $testButton.button("option", "disabled", true);
        self.showTestResults(false);
        self.isTestResultsLoading(true);
        self.testSucceeded(false);

        self.data.testRelativeUri($testRelativeUri.val());

        var jsonData = ko.toJSON(self.data);
        $.post("Service/WebSources/Test" + window.location.search, jsonData, function (result) {
            $testButton.button("option", "disabled", false);
            self.isTestResultsLoading(false);
            self.showTestResults(true);
            self.testSucceeded(result.IsValid);
            self.testError(result.ErrorMessage);
            //$inspector.src = "data:text/html;charset=utf-8," + escape(result.Result);
        });
    };
    
    $address.keyup(function (e) {
        if (self.isFormTestable()) {
            // ENTER key pressed
            if (e.keyCode == 13) {
                self.test();
            }
        }
    }).keydown(function (e) {
        if (e.which == 13) {
            e.preventDefault();
        }
    });
};


WebSourceViewModel.create = function (serverContainerID, saveContainerID) {
    // apply jquery-ui themes
    $("button").button();

    var serverViewModel = new WebSourceViewModel(saveContainerID);
    ko.applyBindings(serverViewModel, document.getElementById(serverContainerID));
};