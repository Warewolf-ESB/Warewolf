///// <reference path="../../wwwroot/Scripts/_references.js" />
///// <reference path="../../wwwroot/Scripts/Services/DbServiceViewModel.js" />
///// <reference path="../../wwwroot/Scripts/Services/ServiceData.js" />
///// <reference path="../../wwwroot/Scripts/Sources/DbSourceViewModel.js" />
///// <reference path="../../wwwroot/Scripts/Dialogs/SaveViewModel.js" />

//module("DbService Post Timestamped");

//// TODO: When we figure out how to use mockjax
////asyncTest("'loadMethods' invokes postTimestamped", function () {
////    $.mockjaxClear(); // clear any existing mock jax entries (could be in a setup method)
////    $.mockjax({ 
////        url: 'Service/Services/DbMethods',
////        type: 'post',
////        dataType: 'json',
////        contentType: 'text/json',
////        response: function (settings) {
////            var result = [{ FullName: null, Name: "dbo.TestProc", Parameters: [], SourceCode: "---" }];
////            this.responseText = JSON.stringify(result);
////            console.log("this.responseText : " + this.responseText);
////        }
////    });

////    var source = new DbSourceViewModel();
////    var model = new DbServiceViewModel();

////    equal(model.loadMethodsTime, 0, "loadMethodsTime is zero at start");

////    model.loadMethods(source.data, false, function() {
////        notEqual(model.loadMethodsTime, 0, "loadMethodsTime is not zero after load");
////        start(); // this tells QUnit to start, async is done
////    });
////});
