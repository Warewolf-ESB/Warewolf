function AppViewModel() {
    var self = this;

    // set title
    document.title = "Switch Flow";

    self.data = {
        DisplayText: ko.observable(""),
        SwitchVariable: ko.observable("")
    };

    self.save = function () {

		// remove &
        var tmp;
        if(self.data.SwitchVariable().indexOf("&") >= 0){
            tmp = self.data.SwitchVariable();
            var regex = new RegExp("&", "g");
			tmp = tmp.replace(regex, "");
			self.data.SwitchVariable(tmp);
		}

        // Bug 8603 - add brackets if they do not exist
        if (self.data.SwitchVariable().indexOf("[[") < 0) {
            tmp = self.data.SwitchVariable();
            if(tmp.length > 0){
				self.data.SwitchVariable("[[" + tmp + "]]");
			}
        }
		
        var jsonData = ko.toJSON(self.data);
        if (window.studio) {
            studio.setValue(jsonData);
        }
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

        if (response.SwitchVariable !== undefined) {
            self.data.SwitchVariable(response.SwitchVariable);
        }

        var responseDisplayText = "";
        if (response.DisplayText !== undefined) {
            responseDisplayText = response.DisplayText;
        }

        self.displayTextComputed = ko.computed(function () {
            var result = self.data.SwitchVariable();
            self.data.DisplayText(result);
            return result;
        });

        if (responseDisplayText !== "") {
            self.data.DisplayText(response.DisplayText);
        }
    };
}