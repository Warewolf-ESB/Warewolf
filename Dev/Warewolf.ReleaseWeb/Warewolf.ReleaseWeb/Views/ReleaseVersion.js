// Make this available to chrome debugger
//@ sourceURL=ReleaseVersion.js  

function ReleaseVersionViewModel() {
    var self = this;   
    
    self.data = {
        PreviousVersionDate: ko.observable(""),
        PreviousVersion: ko.observable(""),
        CurrentVersionDate: ko.observable(""),
        CurrentVersion: ko.observable(""),
    };

    self.canCreate = ko.observable(true);
    self.canUpload = ko.observable(false);
    self.canRollback = ko.observable(false);
    self.errorMessage = ko.observable("");
    self.successMessage = ko.observable("");

    self.updatingMessages = false;
    self.errorMessage.subscribe(function (newValue) {
        if (!self.updatingMessages) {
            self.updatingMessages = true;
            self.successMessage("");
            self.updatingMessages = false;
        }
    });
    self.successMessage.subscribe(function (newValue) {
        if (!self.updatingMessages) {
            self.updatingMessages = true;
            self.errorMessage("");
            self.updatingMessages = false;
        }
    });
    self.isValid = ko.computed(function () {
        var result = self.canCreate() && self.data.PreviousVersion() !== self.data.CurrentVersion();        
        return result;
    });

    self.invokeProxy = function (action, args, callback) {
        $.get("Services/Proxy.ashx?" + action + args, callback);
    };

    self.createRelease = function () {
        self.canCreate(false);
        self.canUpload(false);
        
        var createArgs = "&p=" + self.data.PreviousVersion() + "&c=" + self.data.CurrentVersion();

        self.invokeProxy("create", createArgs, function (createXml) {
            var createJson = $.xml2json(createXml);
            if (createJson && createJson.Result.toLowerCase() === "true") {
                self.successMessage("Release created! Please test the installer BEFORE uploading");
                self.canUpload(true);
            } else {
                self.canCreate(true);
                if (createJson.Message) {
                    self.errorMessage("Error creating the release: " + createJson.Message);
                } else {
                    self.errorMessage("Error creating the release - please see the Audit log for details");
                }
            }
        });
        return true;
    };
    
    self.uploadRelease = function () {
        self.canUpload(false);
        self.invokeProxy("upload", "", function (xml) {
            var json = $.xml2json(xml);
            if (json && json.Result.toLowerCase() === "true") {
                self.successMessage("Release uploaded!");
            } else {
                self.canUpload(true);
                if (json.Message) {
                    self.errorMessage("Error uploading the release: " + json.Message);
                } else {
                    self.errorMessage("Error uploading the release - please see the Audit log for details");
                }
            }
        });
        return true;
    };
    

    self.rollbackUpload = function () {
        self.canRollback(false);
        self.invokeProxy("rollback", "", function (xml) {
            var json = $.xml2json(xml);
            if (json && json.Result.toLowerCase() === "true") {
                self.successMessage("Release rolled back!");
            } else {
                self.canRollback(true);
                if (json.Message) {
                    self.errorMessage("Error rolling back the release: " + json.Message);
                } else {
                    self.errorMessage("Error rolling back the release - please see the Audit log for details");
                }
            }
        });
        return true;
    };

    self.load = function () {
        self.invokeProxy("GetLatest", "", function (xml) {
            var json = $.xml2json(xml);
            var d1 = new Date(json.CurrentVersionDate);
            var d2 = new Date();
            self.data.PreviousVersionDate(d1.toLocaleString());
            self.data.PreviousVersion(json.CurrentVersion);
            self.data.CurrentVersionDate(d2.toLocaleString());
            self.data.CurrentVersion(json.CurrentVersion);
        });   
    };
    
    self.load();
}


ReleaseVersionViewModel.create = function (containerID) {
    // apply jquery-ui themes
    $("button").button();

    var releaseVersionViewModel = new ReleaseVersionViewModel();
    ko.applyBindings(releaseVersionViewModel, document.getElementById(containerID));
    return releaseVersionViewModel;
};
