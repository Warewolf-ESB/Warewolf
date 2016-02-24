﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities.Designers.Tests.WebGetTool;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Web_Service_Post;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

namespace Dev2.Activities.Designers.Tests.WebPostTool
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TestWebPostViewModel
    {
        #region Test Setup

        private static MyWebModel GetMockModel()
        {
            return new MyWebModel();
        }

        private static DsfWebPostActivity GetPostActivityWithOutPuts(MyWebModel mod)
        {
            return new DsfWebPostActivity()
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

        private static DsfWebPostActivity GetEmptyPostActivity()
        {
            return new DsfWebPostActivity();
        }

        private WebServicePostViewModel GetWebServicePostViewModel()
        {
            return new WebServicePostViewModel(ModelItemUtils.CreateModelItem(GetEmptyPostActivity(), GetMockModel()));
        }

        #endregion

        //[TestMethod]
        //[Owner("Nkosinathi Sangweni")]
        //public void OnLoad_GivenHasModelAndId_ShouldHaveDefaultHeightValues()
        //{
        //    //---------------Set up test pack-------------------
        //    var id = Guid.NewGuid();
        //    var mod = GetMockModel();
        //    var act = GetPostActivityWithOutPuts(mod);
        //    var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
        //    //---------------Assert Precondition----------------
        //    //---------------Execute Test ----------------------
        //    //---------------Test Result -----------------------
        //    Assert.AreEqual(465, postViewModel.DesignMaxHeight);
        //    Assert.AreEqual(465, postViewModel.DesignMinHeight);
        //    Assert.AreEqual(465, postViewModel.DesignHeight);
        //    Assert.IsTrue(postViewModel.SourceRegion.IsVisible);
        //    Assert.IsTrue(postViewModel.OutputsRegion.IsVisible);
        //    Assert.IsTrue(postViewModel.InputArea.IsVisible);
        //    Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);
        //    postViewModel.ValidateTestComplete();
        //    Assert.IsTrue(postViewModel.OutputsRegion.IsVisible);
        //}

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        //public void Validate_GivenHasNewInstance_ShouldHaveOneDefaultError()
        public void WebPost_MethodName_ValidateExpectErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetEmptyPostActivity();
            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
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
        public void ClearValidationMemoWithNoFoundError_GivenHasNoErrors_ShouldNullErrors()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            postViewModel.ClearValidationMemoWithNoFoundError();
            //---------------Test Result -----------------------
            Assert.IsNull(postViewModel.Errors);
            Assert.AreEqual(postViewModel.DesignValidationErrors.Count, 1);
        }

        //[TestMethod]
        //[Owner("Nkosinathi Sangweni")]
        //public void Construct_GivenIsNew_ShouldHaveDefalutValues()
        //{
        //    //---------------Set up test pack-------------------
        //    var id = Guid.NewGuid();
        //    var mod = GetMockModel();
        //    var act = GetPostActivityWithOutPuts(mod);
        //    var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
        //    //---------------Assert Precondition----------------
        //    //---------------Execute Test ----------------------
        //    Assert.AreEqual(150, postViewModel.DesignMaxHeight);
        //    Assert.AreEqual(150, postViewModel.DesignMinHeight);
        //    Assert.AreEqual(150, postViewModel.DesignHeight);
        //    Assert.IsTrue(postViewModel.SourceRegion.IsVisible);
        //    Assert.IsFalse(postViewModel.OutputsRegion.IsVisible);
        //    Assert.IsFalse(postViewModel.InputArea.IsVisible);
        //    Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);

        //    //---------------Test Result -----------------------
        //}

        //[TestMethod]
        //[Owner("Nkosinathi Sangweni")]
        //public void ActionSetSource_GivenSelectedSource_ShouldHaveDefaultValues()
        //{
        //    //---------------Set up test pack-------------------
        //    var id = Guid.NewGuid();
        //    var mod = GetMockModel();
        //    var act = GetPostActivityWithOutPuts(mod);
        //    var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
        //    postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
        //    postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
        //    //---------------Assert Precondition----------------
        //    //---------------Execute Test ----------------------
        //    Assert.AreEqual(330, postViewModel.DesignMaxHeight);
        //    Assert.AreEqual(330, postViewModel.DesignMinHeight);
        //    Assert.AreEqual(330, postViewModel.DesignHeight);
        //    Assert.IsTrue(postViewModel.SourceRegion.IsVisible);
        //    Assert.IsFalse(postViewModel.OutputsRegion.IsVisible);
        //    Assert.IsTrue(postViewModel.InputArea.IsVisible);
        //    Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);

        //    //---------------Test Result -----------------------
        //}

//        [TestMethod]
//        [Owner("Nkosinathi Sangweni")]
//        //public void TesInputCommand_GivenSourceIsSet_ShouldHaveMappings()
//        public void Webget_TestActionSetSourceAndTestClickOkHasMappings()
//        {
//            //---------------Set up test pack-------------------
//            var id = Guid.NewGuid();
//            var mod = GetMockModel();
//            var act = GetPostActivityWithOutPuts(mod);
//            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
//            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
//            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
//#pragma warning disable 4014
//            postViewModel.TestInputCommand.Execute();
//            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
//            postViewModel.ManageServiceInputViewModel.IsVisible = true;
//            postViewModel.ManageServiceInputViewModel.SetInitialVisibility();
//            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
//            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
//#pragma warning restore 4014
//            //---------------Assert Precondition----------------
//            //---------------Execute Test ----------------------
//            Assert.AreEqual(465, postViewModel.DesignMaxHeight);
//            Assert.AreEqual(440, postViewModel.DesignMinHeight);
//            Assert.AreEqual(440, postViewModel.DesignHeight);
//            Assert.IsTrue(postViewModel.SourceRegion.IsVisible);
//            Assert.IsTrue(postViewModel.OutputsRegion.IsVisible);
//            Assert.IsTrue(postViewModel.InputArea.IsVisible);
//            Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);
//            Assert.IsFalse(postViewModel.ManageServiceInputViewModel.InputArea.IsVisible);
//            //---------------Test Result -----------------------
//        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void WebPost_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //---------------Set up test pack-------------------
            var id = Guid.NewGuid();
            var mod = GetMockModel();
            var act = GetPostActivityWithOutPuts(mod);
            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
#pragma warning disable 4014
            postViewModel.TestInputCommand.Execute();
            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
            postViewModel.ManageServiceInputViewModel.IsVisible = true;
            postViewModel.ManageServiceInputViewModel.SetInitialVisibility();
            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);
            //---------------Test Result -----------------------
        }

//        [TestMethod]
//        [Owner("Nkosinathi Sangweni")]
//        public void WebPost_TestActionSetSourceAndTestClickOkHasserialisationIssue()
//        {
//            //---------------Set up test pack-------------------
//            var id = Guid.NewGuid();
//            var mod = GetMockModel();
//            var act = GetEmptyPostActivity();
//            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
//            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
//            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
//#pragma warning disable 4014
//            postViewModel.TestInputCommand.Execute();
//            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
//            postViewModel.ManageServiceInputViewModel.IsVisible = true;
//            postViewModel.ManageServiceInputViewModel.SetInitialVisibility();
//            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
//#pragma warning restore 4014
//            //---------------Assert Precondition----------------
//            //---------------Execute Test ----------------------
//            Assert.AreEqual(postViewModel.OutputsRegion.Outputs.First().MappedFrom, "Result");

//            //---------------Test Result -----------------------
//        }

//        [TestMethod]
//        [Owner("Nkosinathi Sangweni")]
//        public void Webget_TestActionSetSourceAndTestClickOkHasHeaders()
//        {
//            //---------------Set up test pack-------------------
//            var id = Guid.NewGuid();
//            var mod = GetMockModel();
//            var act = GetPostActivityWithOutPuts(mod);
//            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
//            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
//            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
//            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
//#pragma warning disable 4014
//            postViewModel.TestInputCommand.Execute();
//            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
//            postViewModel.ManageServiceInputViewModel.IsVisible = true;
//            postViewModel.ManageServiceInputViewModel.SetInitialVisibility();
//            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
//            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
//#pragma warning restore 4014
//            //---------------Assert Precondition----------------
//            //---------------Execute Test ----------------------
//            Assert.AreEqual(495, postViewModel.DesignMaxHeight);
//            Assert.AreEqual(470, postViewModel.DesignMinHeight);
//            Assert.AreEqual(470, postViewModel.DesignHeight);
//            Assert.IsTrue(postViewModel.SourceRegion.IsVisible);
//            Assert.IsTrue(postViewModel.OutputsRegion.IsVisible);
//            Assert.IsTrue(postViewModel.InputArea.IsVisible);
//            Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);
//            Assert.AreEqual(1, postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count);
//            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");

//            //---------------Test Result -----------------------
//        }

//        [TestMethod]
//        [Owner("Nkosinathi Sangweni")]
//        public void Webget_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
//        {
//            //---------------Set up test pack-------------------
//            var id = Guid.NewGuid();
//            var mod = GetMockModel();
//            var act = GetPostActivityWithOutPuts(mod);
//            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
//            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
//            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
//            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
//            postViewModel.InputArea.QueryString = "the [[b]]";
//#pragma warning disable 4014
//            postViewModel.TestInputCommand.Execute();
//            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
//            postViewModel.ManageServiceInputViewModel.IsVisible = true;
//            postViewModel.ManageServiceInputViewModel.SetInitialVisibility();
//            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
//            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
//#pragma warning restore 4014
//            //---------------Assert Precondition----------------

//            //---------------Execute Test ----------------------
//            Assert.AreEqual(495, postViewModel.DesignMaxHeight);
//            Assert.AreEqual(470, postViewModel.DesignMinHeight);
//            Assert.AreEqual(470, postViewModel.DesignHeight);
//            Assert.IsTrue(postViewModel.SourceRegion.IsVisible);
//            Assert.IsTrue(postViewModel.OutputsRegion.IsVisible);
//            Assert.IsTrue(postViewModel.InputArea.IsVisible);
//            Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);
//            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
//            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b]]");
//            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
//            //---------------Test Result -----------------------
//        }

//        [TestMethod]
//        [Owner("Nkosinathi Sangweni")]
//        public void Webget_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
//        {
//            //---------------Set up test pack-------------------
//            var id = Guid.NewGuid();
//            var mod = GetMockModel();
//            var act = GetPostActivityWithOutPuts(mod);
//            var postViewModel = new WebServicePostViewModel(ModelItemUtils.CreateModelItem(act), mod);
//            postViewModel.ManageServiceInputViewModel = new InputViewForTest(postViewModel, mod);
//            postViewModel.SourceRegion.SelectedSource = postViewModel.SourceRegion.Sources.First();
//            postViewModel.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
//            postViewModel.InputArea.QueryString = "the [[b().a]]";
//#pragma warning disable 4014
//            postViewModel.TestInputCommand.Execute();
//            postViewModel.ManageServiceInputViewModel.TestCommand.Execute(null);
//            postViewModel.ManageServiceInputViewModel.IsVisible = true;
//            postViewModel.ManageServiceInputViewModel.SetInitialVisibility();
//            postViewModel.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
//            postViewModel.ManageServiceInputViewModel.OkCommand.Execute(null);
//#pragma warning restore 4014
//            //---------------Assert Precondition----------------

//            //---------------Execute Test ----------------------
//            Assert.AreEqual(495, postViewModel.DesignMaxHeight);
//            Assert.AreEqual(470, postViewModel.DesignMinHeight);
//            Assert.AreEqual(470, postViewModel.DesignHeight);
//            Assert.IsTrue(postViewModel.SourceRegion.IsVisible);
//            Assert.IsTrue(postViewModel.OutputsRegion.IsVisible);
//            Assert.IsTrue(postViewModel.InputArea.IsVisible);
//            Assert.IsTrue(postViewModel.ErrorRegion.IsVisible);
//            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
//            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b().a]]");
//            Assert.IsTrue(postViewModel.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
//            //---------------Test Result -----------------------
//        }

    }

    public class InputViewForTest : ManageWebServiceInputViewModel
    {
        #region Overrides of ManageWebServiceInputViewModel
        [ExcludeFromCodeCoverage]
        public override void ShowView()
        {

        }

        #endregion

        public InputViewForTest(IWebServiceGetViewModel model, IWebServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
        public InputViewForTest(IWebServicePostViewModel model, IWebServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}
