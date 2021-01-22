/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers.Tests.WebGetTool;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Web_Post;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Serializers;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using TestingDotnetDllCascading;
using Warewolf.Core;
using Warewolf.Data;
using Warewolf.Data.Options;
using Warewolf.Options;
using Warewolf.Studio.ViewModels;

namespace Dev2.Activities.Designers.Tests.WebPostTool
{
    [TestClass]
    public class WebPostActivityViewModelTests
    {
        static MyWebModel GetMockModel()
        {
            return new MyWebModel();
        }

        static WebPostActivity GetPostActivityWithOutPuts(MyWebModel mod)
        {
            return new WebPostActivity()
            {
                DisplayName = "test displayName",
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

        static WebPostActivity GetEmptyPostActivity()
        {
            return new WebPostActivity();
        }

        WebPostActivityViewModel GetWebPostActivityViewModel()
        {
            return new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(GetEmptyPostActivity(), GetMockModel()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPostActivityViewModel_OnLoad_GivenHasModelAndId_ShouldHaveDefaultHeightValues()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
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
        [Owner("Nkosinathi Sangweni")]
        //public void WebPostActivityViewModel_Validate_GivenHasNewInstance_ShouldHaveOneDefaultError()
        public void WebPostActivityViewModel_WebPost_MethodName_ValidateExpectErrors()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(postViewModel);
            //---------------Execute Test ----------------------
            postViewModel.Validate();
            //---------------Test Result -----------------------
            Assert.AreEqual(postViewModel.Errors.Count, 1);
            Assert.AreEqual(postViewModel.DesignValidationErrors.Count, 2);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPostActivityViewModel_WebPut_BodyIsJSonNoHeaders_ExpectNewHeadersAdded()
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
            var postViewModel = new WebPostActivityViewModel(modelItem);
            var oldHeaderCount = postViewModel.InputArea.Headers.Count;
            //---------------Assert Precondition----------------
            Assert.AreEqual(1, oldHeaderCount);
            //---------------Execute Test ----------------------
            var human = new Human();
            var h = new Dev2JsonSerializer();
            var humanString = h.Serialize(human);
            postViewModel.InputArea.PostData = humanString;
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.ApplicationJsonHeader, postViewModel.InputArea.Headers.Single(value => value.Value == GlobalConstants.ApplicationJsonHeader).Value);
            Assert.AreEqual(GlobalConstants.ContentType, postViewModel.InputArea.Headers.Single(value => value.Name == GlobalConstants.ContentType).Name);
            Assert.IsTrue(newCount > oldHeaderCount);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPostActivityViewModel_WebPut_BodyIsXmlNoHeaders_ExpectNewHeadersAdded()
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
            var postViewModel = new WebPostActivityViewModel(modelItem);
            var oldCount = postViewModel.InputArea.Headers.Count;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var person = "<person sex=\"female\"><firstname>Anna</firstname><lastname>Smith</lastname></person>";
            postViewModel.InputArea.PostData = person;
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.ApplicationXmlHeader, postViewModel.InputArea.Headers.Single(value => value.Value == GlobalConstants.ApplicationXmlHeader).Value);
            Assert.AreEqual(GlobalConstants.ContentType, postViewModel.InputArea.Headers.Single(value => value.Name == GlobalConstants.ContentType).Name);
            Assert.IsTrue(newCount > oldCount);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPostActivityViewModel_WebPut_BodyIsXmlExistingHeaders_ExpectNoHeadersAdded()
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
            var postViewModel = new WebPostActivityViewModel(modelItem);

            var oldCount = postViewModel.InputArea.Headers.Count;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var person = "<person sex=\"female\"><firstname>Anna</firstname><lastname>Smith</lastname></person>";
            postViewModel.InputArea.PostData = person;
            var newCount = postViewModel.InputArea.Headers.Count;
            //---------------Test Result -----------------------
            Assert.IsTrue(newCount == oldCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WebPostDesignerViewModel_Handle")]
        public void WebPostActivityViewModel_WebPostDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var viewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPostActivityViewModel_ClearValidationMemoWithNoFoundError_GivenHasNoErrors_ShouldNullErrors()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
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
        public void WebPostActivityViewModel_Construct_GivenIsNew_ShouldHaveDefalutValues()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
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
        public void WebPostActivityViewModel_GetHeaderRegion_GivenIsNew_ShouldReturnInputArea()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
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
        public void WebPostActivityViewModel_ActionSetSource_GivenSelectedSource_ShouldHaveDefaultValues()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForWebPostTest(postViewModel, mod);
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
        //public void WebPostActivityViewModel_TesInputCommand_GivenSourceIsSet_ShouldHaveMappings()
        public void WebPostActivityViewModel_WebPost_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForWebPostTest(postViewModel, mod);
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
        public void WebPostActivityViewModel_WebPost_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForWebPostTest(postViewModel, mod);
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
        public void WebPostActivityViewModel_WebPost_TestActionSetSourceAndTestClickOkHasserialisationIssue()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            mod.IsTextResponse = true;
            var act = GetEmptyPostActivity();
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForWebPostTest(postViewModel, mod);
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
        public void WebPostActivityViewModel_WebPost_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod)
            {

            };
            postViewModel.ManageServiceInputViewModel = new InputViewForWebPostTest(postViewModel, mod);
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
        public void WebPostActivityViewModel_WebPost_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForWebPostTest(postViewModel, mod);
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
        public void WebPostActivityViewModel_WebPost_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
        {
            //---------------Set up test pack-------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForWebPostTest(postViewModel, mod);
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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityViewModel))]
        public void WebPostActivityViewModel_LoadOptions_IsNull_ExpectDefault_FormDataOptions_Not_Null()
        {
            //------------Setup for test--------------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            act.FormDataOptions = null;
            //------------Execute Test---------------------------
            var formDataDesignerViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var options = formDataDesignerViewModel.Options.Options.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, options.Count);
            Assert.IsNotNull(act.FormDataOptions);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityViewModel))]
        public void WebPostActivityViewModel_UpdateOptionsModelItem_ShouldNotBeDefault()
        {
            //------------Setup for test--------------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var model = ModelItemUtils.CreateModelItem(act);

            var formDataOptions = new FormDataOptions();

            act.FormDataOptions = formDataOptions;
            //------------Execute Test---------------------------
            var formDataDesignerViewModel = new WebPostActivityViewModel(model, mod);
            var options = formDataDesignerViewModel.Options.Options.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, options.Count);
            Assert.IsNotNull(act.FormDataOptions);

            formDataOptions.Notify();               

            Assert.AreNotEqual(formDataOptions, model.Properties["FormDataOptions"], "When the FormDataOptions OnChange event is raised the the Model property should be updated to new value");

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityViewModel))]
        public void WebPostActivityViewModel_LoadConditionExpressionOptions_IsNull_ExpectDefault_FormDataOptions_Not_Null()
        {
            //------------Setup for test--------------------------
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            act.FormDataOptions = null;
            //------------Execute Test---------------------------
            var formDataDesignerViewModel = new WebPostActivityViewModel(ModelItemUtils.CreateModelItem(act), mod);
            var options = formDataDesignerViewModel.Options.Options.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, options.Count);
            Assert.IsNotNull(act.FormDataOptions);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityViewModel))]
        public void WebPostActivityViewModel_LoadConditions()
        {
            //------------Setup for test--------------------------
            var mod = GetMockModel();
            var conditionExpressionList = new List<FormDataConditionExpression>
            {
                new FormDataConditionExpression
                {
                    Left = "[[a]]",
                    Cond = new FormDataConditionMatch
                    {
                        MatchType = enFormDataTableType.Text,
                        Right = "this can be any text message"
                    },

                }
            };

            var gateOptionsProperty = CreateModelProperty("FormDataOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList);
            var displayNameProperty = CreateModelProperty("DisplayName", "test display name");

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "FormDataOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProperty.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new WebPostActivityViewModel(mockModelItem.Object, mod);

            var conditions = gateDesignerViewModel.ConditionExpressionOptions.Options.ToList();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, conditions.Count);

            var condition = conditions[0] as FormDataOptionConditionExpression;
            Assert.AreEqual("[[a]]", condition.Left);
            Assert.AreEqual(enFormDataTableType.Text, condition.MatchType);
            Assert.AreEqual("this can be any text message", condition.Right);

            var emptyCondition = conditions[1] as FormDataOptionConditionExpression;
            Assert.IsNull(emptyCondition.Left);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityViewModel))]
        public void WebPostActivityViewModel_DeleteCondition()
        {
            //------------Setup for test--------------------------
            var mod = GetMockModel();
            var conditionExpressionList = new List<FormDataConditionExpression>
            {
                new FormDataConditionExpression
                {
                    Left = "[[a]]",
                    Cond = new FormDataConditionMatch
                    {
                        MatchType = enFormDataTableType.Text,
                        Right = "this can be any text message"
                    }
                }
            };

            var gateOptionsProperty = CreateModelProperty("FormDataOptions", null).Object;
            var conditionsProperty = CreateModelProperty("Conditions", conditionExpressionList);
            var displayNameProperty = CreateModelProperty("DisplayName", "test display name");

            var mockProperties = new Mock<ModelPropertyCollection>();
            mockProperties.Protected().Setup<ModelProperty>("Find", "FormDataOptions", true).Returns(gateOptionsProperty);
            mockProperties.Protected().Setup<ModelProperty>("Find", "Conditions", true).Returns(conditionsProperty.Object);
            mockProperties.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProperty.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(modelItem => modelItem.Properties).Returns(mockProperties.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new WebPostActivityViewModel(mockModelItem.Object, mod);

            var conditions = gateDesignerViewModel.ConditionExpressionOptions.Options.ToList();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, conditions.Count);

            var optionConditionExpression = conditions[0] as FormDataOptionConditionExpression;
            optionConditionExpression.SelectedMatchType = new NamedInt { Name = "Text", Value = 1 };
            optionConditionExpression.DeleteCommand.Execute(optionConditionExpression);

            conditions = gateDesignerViewModel.ConditionExpressionOptions.Options.ToList();

            Assert.AreEqual(1, conditions.Count);

            var emptyCondition = conditions[0] as FormDataOptionConditionExpression;
            Assert.IsNull(emptyCondition.Left);

        }

        private Mock<ModelProperty> CreateModelProperty(string name, object value)
        {
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.Name).Returns(name);
            prop.Setup(p => p.ComputedValue).Returns(value);
            return prop;
        }


    }

    public class InputViewForWebPostTest : ManageWebServiceInputViewModel
    {

        public InputViewForWebPostTest(IWebServiceBaseViewModel model, IWebServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
