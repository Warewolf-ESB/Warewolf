/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
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
using Dev2.Common.Interfaces.ServerProxyLayer;
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
    public class WebPutViewModelTests
    {
        static MyWebModel GetMockModel()
        {
            return new MyWebModel();
        }

        static WebPutActivity GetPostActivityWithOutPuts(MyWebModel mod)
        {
            return new WebPutActivity()
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

        static WebPutActivity GetEmptyPostActivity()
        {
            return new WebPutActivity();
        }

        WebServicePostViewModel GetWebServicePostViewModel()
        {
            return new WebServicePostViewModel(ModelItemUtils.CreateModelItem(GetEmptyPostActivity(), GetMockModel()));
        }
        static WebServicePutViewModel CreateViewModel(WebPutActivity act, MyWebModel mod)
        {
            return new WebServicePutViewModel(ModelItemUtils.CreateModelItem(act), mod);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_OnLoad_GivenHasModelAndId_ShouldHaveDefaultHeightValues()
        {
            //---------------Set up test pack-------------------
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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_OnLoad_GivenHasModelAndId_ThumbVisibility_ExpectedTrue()
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
            IsItemDragged.Instance.IsDragged = true;
            var putViewModel = new WebServicePutViewModel(modelItem);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(putViewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_OnLoad_GivenHasModelAndId_ThumbVisibility_ExpectedFalse()
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
            IsItemDragged.Instance.IsDragged = false;
            var putViewModel = new WebServicePutViewModel(modelItem);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(putViewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_MethodName_ValidateExpectErrors()
        {
            //---------------Set up test pack-------------------
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_ClearValidationMemoWithNoFoundError_GivenHasNoErrors_ShouldNullErrors()
        {
            //---------------Set up test pack-------------------
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_GetHeaderRegion_GivenIsNew_ShouldReturnInputArea()
        {
            //---------------Set up test pack-------------------
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_Construct_GivenIsNew_ShouldHaveDefalutValues()
        {
            //---------------Set up test pack-------------------
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_ActionSetSource_GivenSelectedSource_ShouldHaveDefaultValues()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();

            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);

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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
            
            var webServiceSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.SourceRegion.SelectedSource = webServiceSource;
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.ErrorRegion.IsEnabled);

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_TestActionSetSourceAndTestClickOkHasserialisationIssue()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            mod.IsTextResponse = true;
            var act = GetEmptyPostActivity();
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
            IWebServiceSource webServiceSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.SourceRegion.SelectedSource = webServiceSource;
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            Assert.AreEqual(postViewModel.OutputsRegion.Outputs.First().MappedFrom, "Result");

            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));

            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);

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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_BodyIsJSonNoHeaders_ExpectNewHeadersAdded()
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
            var h = new Dev2JsonSerializer();
            var humanString = h.Serialize(human);
            postViewModel.InputArea.PutData = humanString;
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.ApplicationJsonHeader, postViewModel.InputArea.Headers.Single(value => value.Value == GlobalConstants.ApplicationJsonHeader).Value);
            Assert.AreEqual(GlobalConstants.ContentType, postViewModel.InputArea.Headers.Single(value => value.Name == GlobalConstants.ContentType).Name);
            Assert.IsTrue(newCount > oldCount);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_BodyIsXmlNoHeaders_ExpectNewHeadersAdded()
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_BodyIsJSonExistingHeaders_ExpectNoHeadersAdded()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_BodyIsXmlExistingHeaders_ExpectNoHeadersAdded()
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            postViewModel.InputArea.QueryString = "the [[b]]";

            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);

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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebServicePutViewModel))]
        public void WebServicePutViewModel_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = CreateViewModel(act, mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTests(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            postViewModel.InputArea.QueryString = "the [[b().a]]";

            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsEnabled = true;
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);

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



    public class InputViewForTests : ManageWebServiceInputViewModel
    {
        public void ShowView()
        {

        }

        public InputViewForTests(IWebServicePutViewModel model, IWebServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
