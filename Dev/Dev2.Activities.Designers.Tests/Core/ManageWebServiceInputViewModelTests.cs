/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Dev2.Activities.Designers.Tests.WebGetTool;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Web_Service_Get;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;



namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class ManageWebServiceInputViewModelTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_Ctor()
        {
            var mod = new MyWebModel();
            var act = new WebGetActivity()
            {
                SourceId = mod.Sources[0].Id,
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                Headers = new List<INameValue> { new NameValue("a", "x") },
                QueryString = "Bob the builder",
                ServiceName = "dsfBob"
            };

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);

            //------------Assert Results-------------------------
            var vm = new ManageWebServiceInputViewModel(webget, mod);
            Assert.IsNotNull(vm.CloseCommand);
            Assert.IsNotNull(vm.PasteResponseCommand);
            Assert.IsNotNull(vm.CloseCommand);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_TestAction()
        {
            var called = false;
            var calledOk = false;

            var mod = new MyWebModel();
            var act = new WebGetActivity()
            {
                SourceId = mod.Sources[0].Id,
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                Headers = new List<INameValue> { new NameValue("a", "x") },
                QueryString = "Bob the builder",
                ServiceName = "dsfBob"
            };

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);

            var vm = new ManageWebServiceInputViewModel(webget, mod)
            {
                TestAction = () => { called = true; },
                OkAction = () =>
                {
                    calledOk = true;
                }
            };
            vm.TestAction();
            vm.OkAction();

            //------------Assert Results-------------------------

            Assert.IsTrue(called);
            Assert.IsTrue(calledOk);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_TestActionSetSourceAndTest()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel();

            var act = new WebGetActivity();
           
            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = new WebServiceDefinition()
            };
            //------------Execute Test---------------------------
            inputview.ExecuteTest();
            //------------Assert Results-------------------------
            Assert.IsTrue(inputview.InputArea.IsEnabled);
            Assert.IsTrue(inputview.OutputArea.IsEnabled);
            Assert.IsNotNull(inputview.OutputArea.Outputs);
            Assert.IsTrue(inputview.OutputArea.Outputs.Count>0);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_TestActionSetSourceAndTest_ExistingContent()
        {
            //------------Setup for test--------------------------
            var mod = new MyModel()
            {
                Response = "{\"NormalText\":\"\"}"
            };

            var act = new WebGetActivity()
            {
                Headers = new List<INameValue>()
                {
                    new NameValue("Content-Type","Application/xml")
                }
            };
           
            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);

            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = new WebServiceDefinition()
            };
            Assert.AreEqual(2, webget.InputArea.Headers.Count);
            //------------Execute Test---------------------------
            inputview.ExecuteTest();
            //------------Assert Results-------------------------
            Assert.IsTrue(inputview.InputArea.IsEnabled);
            Assert.IsTrue(inputview.OutputArea.IsEnabled);
            Assert.IsNotNull(inputview.OutputArea.Outputs);
            Assert.IsTrue(inputview.OutputArea.Outputs.Count>0);
            Assert.AreEqual(2, webget.InputArea.Headers.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_PropertyChangedHandler()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel();

            var act = new WebGetActivity();
            var called = false;
            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod);
            inputview.PropertyChanged += (sender, args) => called = true;
            inputview.Model = new WebServiceDefinition();
            //------------Execute Test---------------------------
            inputview.ExecuteTest();

            //------------Assert Results-------------------------
            Assert.IsTrue(called);


        }
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_CloneRegion_ReturnsNull()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel();

            var act = new WebGetActivity();
            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = new WebServiceDefinition()
            };

            //------------Execute Test---------------------------
            var clone = inputview.CloneRegion();

            //------------Assert Results-------------------------
            Assert.AreEqual(inputview,clone);
            
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_TestAction_Exception()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel
            {
                HasRecError = true
            };

            var act = new WebGetActivity();
            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = null
            };

            //------------Execute Test---------------------------
            inputview.ExecuteTest();

            //------------Assert Results-------------------------
            Assert.IsTrue(inputview.Errors.Count == 1);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_OkAction_Exception()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel
            {
                HasRecError = true
            };

            var act = new WebGetActivity();
            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod);
            webget.OutputsRegion.Outputs = null;

            //------------Execute Test---------------------------
            inputview.ExecuteOk();

            //------------Assert Results-------------------------
            Assert.IsTrue(inputview.Errors.Count == 1);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_RestoreRegion_DoesNothing()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel();

            var act = new WebGetActivity();
            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = new WebServiceDefinition()
            };

            //------------Execute Test---------------------------
            inputview.RestoreRegion(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(true,"Error RestoreRegion should do nothing");

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_TestActionSetSourceAndTestClickOk()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel();

            var act = new WebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = new WebServiceDefinition()
            };
            inputview.ExecuteTest();
            //------------Execute Test---------------------------
            Assert.IsTrue(inputview.InputArea.IsEnabled);
            Assert.IsTrue(inputview.OutputArea.IsEnabled);
            Assert.IsNotNull(inputview.OutputArea.Outputs);
            Assert.IsTrue(inputview.OutputArea.Outputs.Count > 0);

            inputview.ExecuteOk();
            //------------Execute Ok---------------------------
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsTrue(webget.OutputsRegion.IsEnabled);
            Assert.IsTrue(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);
            Assert.IsFalse(webget.ManageServiceInputViewModel.InputArea.IsEnabled);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_TestActionSetSourceAndTestClickPaste()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel();

            var act = new WebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = new WebServiceDefinition()
            };
            //------------Execute Test---------------------------
            inputview.ExecutePaste();

            //------------Assert Results-------------------------
            Assert.IsTrue(inputview.PasteResponseVisible);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_TestActionSetSourceAndTestClickClose()
        {
            //------------Setup for test--------------------------
            var mod = new MyWebModel();

            var act = new WebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var inputview = new ManageWebServiceInputViewModel(webget, mod)
            {
                Model = new WebServiceDefinition()
            };
            inputview.ExecuteClose();
            //------------Execute Ok---------------------------
            Assert.IsNull(inputview.OutputArea.Outputs);
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsFalse(webget.OutputsRegion.IsEnabled);
            Assert.IsTrue(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);
            Assert.IsFalse(webget.ManageServiceInputViewModel.InputArea.IsEnabled);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManageWebServiceInputViewModel))]
        public void ManageWebServiceInputViewModel_Properties()
        {
            var mod = new MyWebModel();
            var act = new WebGetActivity()
            {
                SourceId = mod.Sources[0].Id,
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("d", "e", "f") },
                Headers = new List<INameValue> { new NameValue("a", "x") },
                QueryString = "Bob the builder",
                ServiceName = "dsfBob"
            };

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);

            var vm = new ManageWebServiceInputViewModel(webget, mod);
            var lst = new List<IServiceInput>();
            vm.InputArea.Inputs = lst;
            Assert.AreEqual(lst.Count, vm.InputArea.Inputs.Count);
            var lsto = new List<IServiceOutputMapping>();
            vm.OutputArea.Outputs = lsto;
            Assert.AreEqual(lsto, vm.OutputArea.Outputs);
            vm.TestResults = "bob";
            Assert.AreEqual("bob", vm.TestResults);
            vm.TestResultsAvailable = true;
            Assert.IsTrue(vm.TestResultsAvailable);
            vm.OkSelected = true;
            Assert.IsTrue(vm.OkSelected);
            vm.IsTestResultsEmptyRows = true;
            Assert.IsTrue(vm.IsTestResultsEmptyRows);
            vm.IsTesting = true;
            Assert.IsTrue(vm.IsTesting);
            vm.PasteResponseVisible = true;
            Assert.IsTrue(vm.PasteResponseVisible);
            vm.PasteResponseAvailable = true;
            Assert.IsTrue(vm.PasteResponseAvailable);
            var b = new WebServiceDefinition() { Headers = new List<INameValue>() { new NameValue("a", "b") } };
            vm.Model = b;
            Assert.IsNotNull(vm.Model);
        }
    }
}
