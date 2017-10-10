﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers.Tests.WebGetTool;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Web_Service_Post;
using Dev2.Activities.Designers2.Web_Service_Put;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Communication;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestingDotnetDllCascading;
using Warewolf.Core;
using Warewolf.Studio.ViewModels;


namespace Dev2.Activities.Designers.Tests.WebPutTool
{
    [TestClass]
    public class TestWebPutViewModel
    {
        #region Test Setup

        private static MyWebModel GetMockModel()
        {
            return new MyWebModel();
        }

        private static DsfWebPutActivity GetPostActivityWithOutPuts(MyWebModel mod)
        {
            return new DsfWebPutActivity()
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

        private static DsfWebPutActivity GetEmptyPostActivity()
        {
            return new DsfWebPutActivity();
        }

        private WebServicePostViewModel GetWebServicePostViewModel()
        {
            return new WebServicePostViewModel(ModelItemUtils.CreateModelItem(GetEmptyPostActivity(), GetMockModel()));
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
            var postViewModel = CreateViewModel(act, mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);
            postViewModel.ValidateTestComplete();
            Assert.IsTrue(postViewModel.OutputsRegion.IsEnabled);
        }

        private static WebServicePutViewModel CreateViewModel(DsfWebPutActivity act, MyWebModel mod)
        {
            return new WebServicePutViewModel(ModelItemUtils.CreateModelItem(act), mod);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        //public void Validate_GivenHasNewInstance_ShouldHaveOneDefaultError()
        public void WebPut_MethodName_ValidateExpectErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var postViewModel = CreateViewModel(act, mod);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(postViewModel);
            //---------------Execute Test ----------------------
            postViewModel.Validate();
            //---------------Test Result -----------------------
            Assert.AreEqual(postViewModel.Errors.Count, 1);
            Assert.AreEqual(postViewModel.DesignValidationErrors.Count, 2);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WebPutDesignerViewModel_Handle")]
        public void WebPutDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var viewModel = CreateViewModel(act, mod);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ClearValidationMemoWithNoFoundError_GivenHasNoErrors_ShouldNullErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            postViewModel.ClearValidationMemoWithNoFoundError();
            //---------------Test Result -----------------------
            Assert.IsNull(postViewModel.Errors);
            Assert.AreEqual(postViewModel.DesignValidationErrors.Count, 1);
            Assert.IsTrue(postViewModel.IsWorstErrorReadOnly);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetHeaderRegion_GivenIsNew_ShouldReturnInputArea()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var postViewModel = CreateViewModel(act, mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsFalse(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsFalse(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);

            //---------------Test Result -----------------------
            Assert.AreSame(postViewModel.InputArea, postViewModel.GetHeaderRegion());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenIsNew_ShouldHaveDefalutValues()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var postViewModel = CreateViewModel(act, mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsFalse(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsFalse(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);

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
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsFalse(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        //public void TesInputCommand_GivenSourceIsSet_ShouldHaveMappings()
        public void WebPut_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
#pragma warning disable 4014
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);
            Assert.IsFalse(postViewModel.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.AreEqual(0, postViewModel.ManageServiceInputViewModel.Errors.Count);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
#pragma warning disable 4014
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_TestActionSetSourceAndTestClickOkHasserialisationIssue()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            mod.IsTextResponse = true;
            var act = GetEmptyPostActivity();
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
#pragma warning disable 4014
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.AreEqual(postViewModel.OutputsRegion.Outputs.First().MappedFrom, "Result");

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
#pragma warning disable 4014
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);
            Assert.AreEqual(1, postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.AreEqual(0, postViewModel.ManageServiceInputViewModel.Errors.Count);
            //---------------Test Result -----------------------
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_BodyIsJSonNoHeaders_ExpectNewHeadersAdded()
        {
            //---------------Set up test pack-------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageWebServiceModel)
            };
            var shellVm = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            var updateProxy = new Mock<IStudioUpdateManager>();
            var updateManager = new Mock<IQueryManager>();
            serverMock.Setup(server => server.UpdateRepository).Returns(updateProxy.Object);
            serverMock.Setup(server => server.QueryProxy).Returns(updateManager.Object);
            shellVm.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellVm.Object);
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            act.Headers = new List<INameValue>();
            var modelItem = ModelItemUtils.CreateModelItem(act);
            var postViewModel = new WebServicePutViewModel(modelItem);
            var oldCount = postViewModel.InputArea.Headers.Count;
            //---------------Assert Precondition----------------
            Assert.AreEqual(1, oldCount);
            //---------------Execute Test ----------------------
            var human = new Human();
            Dev2JsonSerializer h = new Dev2JsonSerializer();
            var humanString = h.Serialize(human);
            postViewModel.InputArea.PutData = humanString;
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.ApplicationJsonHeader, postViewModel.InputArea.Headers.Single(value => value.Value == GlobalConstants.ApplicationJsonHeader).Value);
            Assert.AreEqual(GlobalConstants.ContentType, postViewModel.InputArea.Headers.Single(value => value.Name == GlobalConstants.ContentType).Name);
            Assert.IsTrue(newCount > oldCount);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_BodyIsXmlNoHeaders_ExpectNewHeadersAdded()
        {
            //---------------Set up test pack-------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageWebServiceModel)
            };
            var shellVm = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            var updateProxy = new Mock<IStudioUpdateManager>();
            var updateManager = new Mock<IQueryManager>();
            serverMock.Setup(server => server.UpdateRepository).Returns(updateProxy.Object);
            serverMock.Setup(server => server.QueryProxy).Returns(updateManager.Object);
            shellVm.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellVm.Object);
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            act.Headers = new List<INameValue>();
            var modelItem = ModelItemUtils.CreateModelItem(act);
            var postViewModel = new WebServicePutViewModel(modelItem);
            var oldCount = postViewModel.InputArea.Headers.Count;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var person = "<person sex=\"female\"><firstname>Anna</firstname><lastname>Smith</lastname></person>";
            postViewModel.InputArea.PutData = person;
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.ApplicationXmlHeader, postViewModel.InputArea.Headers.Single(value => value.Value == GlobalConstants.ApplicationXmlHeader).Value);
            Assert.AreEqual(GlobalConstants.ContentType, postViewModel.InputArea.Headers.Single(value => value.Name == GlobalConstants.ContentType).Name);
            Assert.IsTrue(newCount > oldCount);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_BodyIsJSonExistingHeaders_ExpectNoHeadersAdded()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var oldCount = postViewModel.InputArea.Headers.Count;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var human = new Human();
            postViewModel.InputArea.PutData = dev2JsonSerializer.Serialize(human);
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.IsTrue(newCount == oldCount);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_BodyIsXmlExistingHeaders_ExpectNoHeadersAdded()
        {
            //---------------Set up test pack-------------------
            CustomContainer.LoadedTypes = new List<Type>()
            {
                typeof(ManageWebServiceModel)
            };
            var shellVm = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            var updateProxy = new Mock<IStudioUpdateManager>();
            var updateManager = new Mock<IQueryManager>();
            serverMock.Setup(server => server.UpdateRepository).Returns(updateProxy.Object);
            serverMock.Setup(server => server.QueryProxy).Returns(updateManager.Object);
            shellVm.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellVm.Object);
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var modelItem = ModelItemUtils.CreateModelItem(act);
            var postViewModel = new WebServicePutViewModel(modelItem);
            
            var oldCount = postViewModel.InputArea.Headers.Count;
            //---------------Assert Precondition----------------
           
            //---------------Execute Test ----------------------
            var person = "<person sex=\"female\"><firstname>Anna</firstname><lastname>Smith</lastname></person>";
            postViewModel.InputArea.PutData = person;
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.IsTrue(newCount == oldCount);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            postViewModel.InputArea.QueryString = "the [[b]]";
#pragma warning disable 4014
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);
            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b]]");
            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, postViewModel.ManageServiceInputViewModel.Errors.Count);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPut_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            postViewModel.InputArea.QueryString = "the [[b().a]]";
#pragma warning disable 4014
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.SourceRegion.IsEnabled);
            Assert.IsTrue(postViewModel.OutputsRegion.IsEnabled);
            Assert.IsTrue(postViewModel.InputArea.IsEnabled);
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);
            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b().a]]");
            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, postViewModel.ManageServiceInputViewModel.Errors.Count);
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

        public InputViewForTest(IWebServicePutViewModel model, IWebServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
