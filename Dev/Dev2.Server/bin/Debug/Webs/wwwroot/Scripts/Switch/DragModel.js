function DragViewModel() {
    var self = this;
    var $mainForm = $("#mainForm");

    // set title
    document.title = "Switch Case";

    self.data = {

        SwitchVariable: ko.observable("")

    };

    self.save = function () {
        //if (!$mainForm.validate().form()) {
        //    return;
        //}

        //var dlID = window.location.search;

        // Bug 8603 - add brackets if they do not exist
        //if (self.data.SwitchVariable().indexOf("[[") < 0) {
        //    var tmp = self.data.SwitchVariable();
        //    self.data.SwitchVariable("[[" + tmp + "]]");
        //}

        var jsonData = ko.toJSON(self.data);

        studio.setValue(jsonData);

        //alert(jsonData);

        //$.post("Service/WebModel/SaveModel" + dlID, jsonData, function (result) {

        //    $("#Dev2Msg").html(result.message);

        //    studio.close();
        //    return true;

        //});
    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.Load = function () {
        //var dlID = window.location.search; //.replace("postdlid", "dlid");

        //// FetchSwitchCase
        //var request = $.ajax({
        //    url: "Service/WebModel/FetchSwitchExpression" + dlID,
        //    type: "post"
        //});

        //request.done(function (response, textStatus, json) {
        //    if (response.SwitchVariable != undefined) {
        //        self.data.SwitchVariable(response.SwitchVariable);
        //    }
        //});

        //request.fail(function (response, textStatus, json) {
        //    alert("An error occured : " + JSON.stringify(response));
        //});
        


        //BUG 8377 Add intellisense
        var dai = studio.getDataAndIntellisense();
        self.intellisenseOptions = dai.intellisenseOptions;
        var response = dai.data;

        $("#SwitchVariable").autocomplete({
            source: self.intellisenseOptions
        });

        if (response.SwitchVariable != undefined) {
            self.data.SwitchVariable(response.SwitchVariable);
        }

    }

}