function AppViewModel() {
    var self = this;
    var $mainForm = $("#mainForm");

    // set title
    document.title = "Switch Flow";

    self.data = {

        SwitchVariable: ko.observable("")

    };

    self.save = function () {

		// remove &
		if(self.data.SwitchVariable().indexOf("&") >= 0){
			var tmp = self.data.SwitchVariable();
			var regex = new RegExp("&", "g");
			tmp = tmp.replace(regex, "");
			self.data.SwitchVariable(tmp);
		}

	
        // Bug 8603 - add brackets if they do not exist
        if (self.data.SwitchVariable().indexOf("[[") < 0) {
            var tmp = self.data.SwitchVariable();
			if(tmp.length > 0){
				self.data.SwitchVariable("[[" + tmp + "]]");
			}
        }
		
        var jsonData = ko.toJSON(self.data);

        if (window.studio) studio.setValue(jsonData);

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