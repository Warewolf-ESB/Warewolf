/// <reference path="../../wwwroot/Scripts/_references.js" />
/// <reference path="../../wwwroot/Scripts/Sources/Service.js" />

module("Source Tests");

test("ServerResourceType", function () {

    var model = new ViewModel();
    equals("Dev2Server", model.data.resourceType, "Resource type is Dev2Server");
});
