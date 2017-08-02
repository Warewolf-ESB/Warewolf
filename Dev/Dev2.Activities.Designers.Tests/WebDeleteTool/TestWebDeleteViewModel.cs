using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers.Tests.WebGetTool;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Web_Service_Delete;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
// ReSharper disable InconsistentNaming
// ReSharper disable All

namespace Dev2.Activities.Designers.Tests.WebDeleteTool
{
    [TestClass]
    public class TestWebDeleteViewModel
    {
        #region Test Setup

        private static MyWebModel GetMockModel()
        {
            return new MyWebModel();
        }

        private static DsfWebDeleteActivity GetPostActivityWithOutPuts(MyWebModel mod)
        {
            return new DsfWebDeleteActivity()
            {
                SourceId = mod.Sources[0].Id,
                Outputs =
                    new List<IServiceOutputMapping>
                    {
                        new ServiceOutputMapping("a", "b", "c"),
                        new ServiceOutputMapping("d", "e", "f")
                    },
                Headers = new List<INameValue> { new NameValue("a", "x") },
                QueryString = "QueryString",
                ServiceName = "dsfBob"
            };
        }

        private static DsfWebDeleteActivity GetEmptyPostActivity()
        {
            return new DsfWebDeleteActivity();
        }

        private WebServiceDeleteViewModel GetWebServicedeleteViewModel()
        {
            return new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(GetEmptyPostActivity(), GetMockModel()));
        }

        #endregion

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnLoad_GivenHasModelAndId_ShouldHaveDefaultHeightValues()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);
            deleteViewModel.ValidateTestComplete();
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Webget_MethodName")]
        public void GetHeaderRegion_GivenIsNew_ShouldReturnInputArea()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);
            deleteViewModel.ValidateTestComplete();
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.AreSame(deleteViewModel.InputArea, deleteViewModel.GetHeaderRegion());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WebDeleteDesignerViewModel_Handle")]
        public void WebDeleteDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var viewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        //public void Validate_GivenHasNewInstance_ShouldHaveOneDefaultError()
        public void WebPost_MethodName_ValidateExpectErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(deleteViewModel);
            //---------------Execute Test ----------------------
            deleteViewModel.Validate();
            //---------------Test Result -----------------------
            Assert.AreEqual(deleteViewModel.Errors.Count, 1);
            Assert.AreEqual(deleteViewModel.DesignValidationErrors.Count, 2);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ClearValidationMemoWithNoFoundError_GivenHasNoErrors_ShouldNullErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            deleteViewModel.ClearValidationMemoWithNoFoundError();
            //---------------Test Result -----------------------
            Assert.IsNull(deleteViewModel.Errors);
            Assert.AreEqual(deleteViewModel.DesignValidationErrors.Count, 1);
            Assert.IsTrue(deleteViewModel.IsWorstErrorReadOnly);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenIsNew_ShouldHaveDefalutValues()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsFalse(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsFalse(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ActionSetSource_GivenSelectedSource_ShouldHaveDefaultValues()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            deleteViewModel.ManageServiceInputViewModel = new InputViewForTest(deleteViewModel, mod);
            deleteViewModel.SourceRegion.SelectedSource = deleteViewModel.SourceRegion.Sources.First();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsFalse(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        //public void TesInputCommand_GivenSourceIsSet_ShouldHaveMappings()
        public void WebPost_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            deleteViewModel.ManageServiceInputViewModel = new InputViewForTest(deleteViewModel, mod);
            deleteViewModel.SourceRegion.SelectedSource = deleteViewModel.SourceRegion.Sources.First();
#pragma warning disable 4014
            deleteViewModel.TestInputCommand.Execute();
            deleteViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            deleteViewModel.ManageServiceInputViewModel.IsEnabled = true;
            deleteViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            deleteViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);
            Assert.IsFalse(deleteViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.AreEqual(0, deleteViewModel.ManageServiceInputViewModel.Errors.Count);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPost_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            deleteViewModel.ManageServiceInputViewModel = new InputViewForTest(deleteViewModel, mod);
            deleteViewModel.SourceRegion.SelectedSource = deleteViewModel.SourceRegion.Sources.First();
#pragma warning disable 4014
            deleteViewModel.TestInputCommand.Execute();
            deleteViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            deleteViewModel.ManageServiceInputViewModel.IsEnabled = true;
            deleteViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            deleteViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);
            
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPost_TestActionSetSourceAndTestClickOkHasserialisationIssue()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            mod.IsTextResponse = true;
            var act = GetEmptyPostActivity();
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            deleteViewModel.ManageServiceInputViewModel = new InputViewForTest(deleteViewModel, mod);
            deleteViewModel.SourceRegion.SelectedSource = deleteViewModel.SourceRegion.Sources.First();
#pragma warning disable 4014
            deleteViewModel.TestInputCommand.Execute();
            deleteViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            deleteViewModel.ManageServiceInputViewModel.IsEnabled = true;
            deleteViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.AreEqual(deleteViewModel.OutputsRegion.Outputs.First().MappedFrom, "Result");
            
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPost_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            deleteViewModel.ManageServiceInputViewModel = new InputViewForTest(deleteViewModel, mod);
            deleteViewModel.SourceRegion.SelectedSource = deleteViewModel.SourceRegion.Sources.First();
            deleteViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
#pragma warning disable 4014
            deleteViewModel.TestInputCommand.Execute();
            deleteViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            deleteViewModel.ManageServiceInputViewModel.IsEnabled = true;
            deleteViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            deleteViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);
            Assert.AreEqual(1, deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.AreEqual(0, deleteViewModel.ManageServiceInputViewModel.Errors.Count);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPost_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            deleteViewModel.ManageServiceInputViewModel = new InputViewForTest(deleteViewModel, mod);
            deleteViewModel.SourceRegion.SelectedSource = deleteViewModel.SourceRegion.Sources.First();
            deleteViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            deleteViewModel.InputArea.QueryString = "the [[b]]";
#pragma warning disable 4014
            deleteViewModel.TestInputCommand.Execute();
            deleteViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            deleteViewModel.ManageServiceInputViewModel.IsEnabled = true;
            deleteViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            deleteViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b]]");
            Assert.IsTrue(deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, deleteViewModel.ManageServiceInputViewModel.Errors.Count);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPost_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var deleteViewModel = new WebServiceDeleteViewModel(ModelItemUtils.CreateModelItem(act), mod);
            deleteViewModel.ManageServiceInputViewModel = new InputViewForTest(deleteViewModel, mod);
            deleteViewModel.SourceRegion.SelectedSource = deleteViewModel.SourceRegion.Sources.First();
            deleteViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            deleteViewModel.InputArea.QueryString = "the [[b().a]]";
#pragma warning disable 4014
            deleteViewModel.TestInputCommand.Execute();
            deleteViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            deleteViewModel.ManageServiceInputViewModel.IsEnabled = true;
            deleteViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            deleteViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(deleteViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.InputArea.IsEnabled);
            Assert.IsTrue(deleteViewModel.ErrorRegion.IsEnabled);
            Assert.IsTrue(deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b().a]]");
            Assert.IsTrue(deleteViewModel.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, deleteViewModel.ManageServiceInputViewModel.Errors.Count);
            //---------------Test Result -----------------------
        }

    }

    public class InputViewForTest : ManageWebServiceInputViewModel
    {
        #region Overrides of ManageWebServiceInputViewModel
        public void ShowView()
        {

        }

        #endregion

        public InputViewForTest(IWebServiceDeleteViewModel model, IWebServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
