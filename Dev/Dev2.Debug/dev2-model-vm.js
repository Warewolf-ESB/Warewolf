window.onload = function () {
    ko.bindingHandlers.tooltip = {
        init: function (element, valueAccessor) {
            var local = ko.utils.unwrapObservable(valueAccessor()),
                options = {};

            ko.utils.extend(options, local);

            $(element).tooltip(options);

            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).tooltip("destroy");
            });
        }
    };

    var NodeModel = function (data) {
        var self = this;
        self.Name = ko.observable('');
        self.WorkspaceID = ko.observable('');
        self.ID = ko.observable('');
        self.DisconnectedID = ko.observable('');
        self.ParentID = ko.observable('');
        self.SourceResourceID = ko.observable('');
        self.StateType = ko.observable('');
        self.DisplayName = ko.observable('');
        self.ActivityType = ko.observable('');
        self.Version = ko.observable('');
        self.IsSimulation = ko.observable('');
        self.HasError = ko.observable('');
        self.ErrorMessage = ko.observable('');
        self.Server = ko.observable('');
        self.ServerID = ko.observable('');
        self.EnvironmentID = ko.observable('');
        self.ClientID = ko.observable('');
        self.OriginatingResourceID = ko.observable('');
        self.Inputs = ko.observableArray([]);
        self.Outputs = ko.observableArray();
        self.AssertResultList = ko.observableArray([]);
        self.StartTime = ko.observable([]);
        self.EndTime = ko.observable('');
        self.Duration = ko.observable('');
        self.Message = ko.observable('');
        self.OriginalInstanceID = ko.observable('');
        self.NumberOfSteps = ko.observable('');
        self.ExecutionOrigin = ko.observable('');
        self.ExecutionOriginDescription = ko.observable('');
        self.ExecutingUser = ko.observable('');
        self.Origin = ko.observable('');
        self.SessionID = ko.observable('');
        self.WorkSurfaceMappingId = ko.observable('');
        self.IsDurationVisible = ko.observable('');
        self.ActualType = ko.observable('');
        self.Children = ko.observableArray([]);
        ko.mapping.fromJS(data, self.mapOptions, self);
    };

    NodeModel.prototype.mapOptions = {
        Children: {
            create: function (args) {
                return new NodeModel(args.data);
            }
        }
    };

    var PageModel = function () {

        var self = this;
        self.URL = ko.observable();

        self.treeData = ko.observable({
            Childern: []
        });

        self.GetData = function () {
            $.ajax({
                type: "GET",
                dataType: "json",
                url: self.URL(),
                cache: false,
                xhrFields:
                {
                    withCredentials: true
                },
                success: function (data) {
                    self.loadData(data);
                }
            });
        }

        self.loadData = function (data) {
            var children = [];
            data.forEach(function (child) {
                var toAdd = new NodeModel(child);
                children.push(toAdd);
            });
            self.treeData().Children = children;
        };
    };
    var pageModel = new PageModel();
    ////pageModel.URL("http://rsaklfnkosinath:3142/secure/DownloadedFromSharepoint/Hello%20World.debug?Name=&wid=8331c574-296b-44de-ae7d-6e56e6b9f338");
    pageModel.URL("http://rsaklfnkosinath:3142/secure/DownloadedFromSharepoint/Hello%20World.debug?Name=&wid=8331c574-296b-44de-ae7d-6e56e6b9f338");

    //self.debugUrl = ko.observable("http://rsaklfnkosinath:3142/secure/Hello%20World.debug?Name=&wid=540beccb-b4f5-4b34-bc37-aa24b26370e2");

    $.ajax({
        type: "GET",
        dataType: "json",
        url: pageModel.URL(),
        cache: false,
        xhrFields:
        {
            withCredentials: true
        },
        success: function (data) {
            pageModel.loadData(data);
            ko.applyBindings(pageModel);
        }
    });
}