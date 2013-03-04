function DragViewModel() {
    var self = this;
    var $mainForm = $("#mainForm");

    // set title
    document.title = "Switch Case";

    self.data = {

        SwitchVariable: ko.observable("")

    };

    self.save = function () {

        var jsonData = ko.toJSON(self.data);

        studio.setValue(jsonData);

    };

    self.cancel = function () {
        studio.cancel();
        return true;
    };

    self.Load = function () {

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