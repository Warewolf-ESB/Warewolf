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

    self.isProcessing = ko.observable(false);
    
    self.isValid = ko.computed(function () {
        var result = !self.isProcessing() && self.data.PreviousVersion() !== self.data.CurrentVersion();        
        return result;
    });
    
    self.createRelease = function () {
        self.isProcessing(true);

        //var data = ko.toJSON(self.data);
        var updateArgs = "&p=" + self.data.PreviousVersion()
            + "&c=" + self.data.CurrentVersion();
        
        $.get("Services/Proxy2.ashx?UpdateVersion" + updateArgs, function (xml1) {
            var json1 = $.xml2json(xml1);
            $.get("Services/Proxy2.ashx?CreateRelease", function (xml2) {
                var json2 = $.xml2json(xml2);
                self.isProcessing(false);
            });
        });
        return true;
    };
    
    self.uploadRelease = function () {
        self.isProcessing(true);
        $.get("Services/Proxy.ashx?UploadRelease", function (xml) {
            var json = $.xml2json(xml);
            self.isProcessing(false);
        });
        return true;
    };

    self.load = function () {
        $.get("Services/Proxy.ashx?GetLatest", function(xml) {
            var json = $.xml2json(xml);
            var d1 = new Date(json.CurrentVersionDate);
            var d2 = new Date();
            self.data.PreviousVersionDate(d1.toDateString());
            self.data.PreviousVersion(json.CurrentVersion);
            self.data.CurrentVersionDate(d2.toDateString());
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
