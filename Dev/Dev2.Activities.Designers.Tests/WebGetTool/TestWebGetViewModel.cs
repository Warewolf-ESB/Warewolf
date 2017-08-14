using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Web_Service_Get;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework.Converters.Graph.String.Xml;
using Warewolf.Core;



namespace Dev2.Activities.Designers.Tests.WebGetTool
{
    [TestClass]
    public class TestWebGetViewModel
    {


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_MethodName_ValidateExpectErrors()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.Validate();
            //------------Execute Test---------------------------
            Assert.AreEqual(webget.Errors.Count, 1);
            Assert.AreEqual(webget.DesignValidationErrors.Count, 2);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_MethodName_ClearErrors()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //------------Execute Test---------------------------
            webget.ClearValidationMemoWithNoFoundError();
            //------------Assert Results-------------------------
            Assert.IsNull(webget.Errors);
            Assert.AreEqual(webget.DesignValidationErrors.Count, 1);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_Ctor_EmptyModelItem()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);

            //------------Execute Test---------------------------
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsFalse(webget.OutputsRegion.IsEnabled);
            Assert.IsFalse(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("Webget_MethodName")]
        public void GetHeaderRegion_GivenIsNew_ShouldReturnInputArea()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);

            //------------Execute Test---------------------------
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsFalse(webget.OutputsRegion.IsEnabled);
            Assert.IsFalse(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);

            //------------Assert Results-------------------------
            Assert.AreSame(webget.InputArea, webget.GetHeaderRegion());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("WebGetDesignerViewModel_Handle")]
        public void WebGetDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();
            var viewModel = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_TestActionSetSource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.ManageServiceInputViewModel = new InputViewForTest(webget, mod);
            webget.SourceRegion.SelectedSource = webget.SourceRegion.Sources.First();
            webget.SourceRegion.SelectedSource.AuthenticationType = AuthenticationType.Public;
            webget.SourceRegion.SelectedSource.UserName = "";
            webget.SourceRegion.SelectedSource.Password = "";
            webget.SourceRegion.SelectedSource.Path = "";
            //------------Execute Test---------------------------
            var hashCode = webget.SourceRegion.SelectedSource.GetHashCode();
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsFalse(webget.OutputsRegion.IsEnabled);
            Assert.IsTrue(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);
            Assert.AreEqual("", webget.SourceRegion.SelectedSource.UserName);
            Assert.AreEqual("", webget.SourceRegion.SelectedSource.Password);
            Assert.AreEqual(AuthenticationType.Public, webget.SourceRegion.SelectedSource.AuthenticationType);
            Assert.AreEqual("", webget.SourceRegion.SelectedSource.Path);
            Assert.AreEqual(hashCode, webget.SourceRegion.SelectedSource.GetHashCode());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_TestActionSetSourceAndTestClickOkHasMappings()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.ManageServiceInputViewModel = new InputViewForTest(webget, mod);
            webget.SourceRegion.SelectedSource = webget.SourceRegion.Sources.First();
#pragma warning disable 4014
            webget.TestInputCommand.Execute();
            webget.ManageServiceInputViewModel.TestCommand.Execute(null);
            webget.ManageServiceInputViewModel.IsEnabled = true;
            webget.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            webget.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //------------Execute Test---------------------------
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsTrue(webget.OutputsRegion.IsEnabled);
            Assert.IsTrue(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);
            Assert.IsFalse(webget.ManageServiceInputViewModel.InputArea.IsEnabled);
            Assert.AreEqual(0, webget.ManageServiceInputViewModel.Errors.Count);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_TestActionSetSourceAndTestClickOkHasMappingsErrorFromServer()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            mod.HasRecError = true;
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.ManageServiceInputViewModel = new InputViewForTest(webget, mod);
            webget.SourceRegion.SelectedSource = webget.SourceRegion.Sources.First();
#pragma warning disable 4014
            webget.TestInputCommand.Execute();
            webget.ManageServiceInputViewModel.TestCommand.Execute(null);
            webget.ManageServiceInputViewModel.IsEnabled = true;
            webget.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            webget.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //------------Execute Test---------------------------

            Assert.IsTrue(webget.ErrorRegion.IsEnabled);

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_TestActionSetSourceAndTestClickOkHasserialisationIssue()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            mod.IsTextResponse = true;
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.ManageServiceInputViewModel = new InputViewForTest(webget, mod);
            webget.SourceRegion.SelectedSource = webget.SourceRegion.Sources.First();
#pragma warning disable 4014
            webget.TestInputCommand.Execute();
            webget.ManageServiceInputViewModel.TestCommand.Execute(null);
            webget.ManageServiceInputViewModel.IsEnabled = true;
            webget.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //------------Execute Test---------------------------

            Assert.AreEqual(webget.OutputsRegion.Outputs.First().MappedFrom, "Result");

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_TestActionSetSourceAndTestClickOkHasHeaders()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.ManageServiceInputViewModel = new InputViewForTest(webget, mod);
            webget.SourceRegion.SelectedSource = webget.SourceRegion.Sources.First();
            webget.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
#pragma warning disable 4014
            webget.TestInputCommand.Execute();
            webget.ManageServiceInputViewModel.TestCommand.Execute(null);
            webget.ManageServiceInputViewModel.IsEnabled = true;
            webget.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            webget.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //------------Execute Test---------------------------
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsTrue(webget.OutputsRegion.IsEnabled);
            Assert.IsTrue(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);
            Assert.AreEqual(1, webget.ManageServiceInputViewModel.InputArea.Inputs.Count);
            Assert.IsTrue(webget.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[a]]");
            Assert.AreEqual(0, webget.ManageServiceInputViewModel.Errors.Count);
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_TestActionSetSourceAndTestClickOkHasQueryStringAndHeaders()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.ManageServiceInputViewModel = new InputViewForTest(webget, mod);
            webget.SourceRegion.SelectedSource = webget.SourceRegion.Sources.First();
            webget.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            webget.InputArea.QueryString = "the [[b]]";
#pragma warning disable 4014
            webget.TestInputCommand.Execute();
            webget.ManageServiceInputViewModel.TestCommand.Execute(null);
            webget.ManageServiceInputViewModel.IsEnabled = true;
            webget.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            webget.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //------------Execute Test---------------------------
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsTrue(webget.OutputsRegion.IsEnabled);
            Assert.IsTrue(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);
            Assert.IsTrue(webget.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(webget.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b]]");
            Assert.IsTrue(webget.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, webget.ManageServiceInputViewModel.Errors.Count);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Webget_MethodName")]
        public void Webget_TestActionSetSourceAndTestClickOkHasQueryStringAndHeadersRecSet()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var mod = new MyWebModel();
            var act = new DsfWebGetActivity();

            var webget = new WebServiceGetViewModel(ModelItemUtils.CreateModelItem(act), mod);
            webget.ManageServiceInputViewModel = new InputViewForTest(webget, mod);
            webget.SourceRegion.SelectedSource = webget.SourceRegion.Sources.First();
            webget.InputArea.Headers.Add(new NameValue("[[a]]", "asa"));
            webget.InputArea.QueryString = "the [[b().a]]";
#pragma warning disable 4014
            webget.TestInputCommand.Execute();
            webget.ManageServiceInputViewModel.TestCommand.Execute(null);
            webget.ManageServiceInputViewModel.IsEnabled = true;
            webget.ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c"), new ServiceOutputMapping("a", "b", "c") };
            webget.ManageServiceInputViewModel.OkCommand.Execute(null);
#pragma warning restore 4014
            //------------Execute Test---------------------------
            Assert.IsTrue(webget.SourceRegion.IsEnabled);
            Assert.IsTrue(webget.OutputsRegion.IsEnabled);
            Assert.IsTrue(webget.InputArea.IsEnabled);
            Assert.IsTrue(webget.ErrorRegion.IsEnabled);
            Assert.IsTrue(webget.ManageServiceInputViewModel.InputArea.Inputs.Count == 2);
            Assert.IsTrue(webget.ManageServiceInputViewModel.InputArea.Inputs.First().Name == "[[b().a]]");
            Assert.IsTrue(webget.ManageServiceInputViewModel.InputArea.Inputs.Last().Name == "[[a]]");
            Assert.AreEqual(0, webget.ManageServiceInputViewModel.Errors.Count);
            //------------Assert Results-------------------------
        }

    }

    public class MyModel : MyWebModel
    {
        public string Response { get; set; }
        public override string TestService(IWebService inputValues)
        {
            if (IsTextResponse)
                return new Dev2JsonSerializer().Serialize("dora");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var svc = new WebService();
            if (!HasRecError)
                svc.Recordsets = new RecordsetList() { new Recordset() { HasErrors = false, Fields = new List<RecordsetField> { new RecordsetField() { Alias = "bob", Name = "the", RecordsetAlias = "dd", Path = new XmlPath() } } } };
            else
            {
                svc.Recordsets = new RecordsetList() { new Recordset() { HasErrors = true, ErrorMessage = "bobthebuilder", Fields = new List<RecordsetField> { new RecordsetField() { Alias = "bob", Name = "the", RecordsetAlias = "dd", Path = new XmlPath() } } } };

            }
            svc.RequestResponse = Response;
            return serializer.Serialize(svc);
        }
    }
    public class MyWebModel : IWebServiceModel
    {
#pragma warning disable 649
        private IStudioUpdateManager _updateRepository;
#pragma warning restore 649
#pragma warning disable 649
        private IQueryManager _queryProxy;
#pragma warning restore 649
        public ObservableCollection<IWebServiceSource> _sources = new ObservableCollection<IWebServiceSource>
        {
            new WebServiceSourceDefinition() { Name = "bob", HostName = "the", DefaultQuery = "Builder" , Id = Guid.NewGuid()},
            new WebServiceSourceDefinition() { Name = "dora", HostName = "the", DefaultQuery = "explorer",Id = Guid.NewGuid() }
        };

        public bool HasRecError { get; set; }
        public bool IsTextResponse { get; set; }
        #region Implementation of IWebServiceModel

        public ICollection<IWebServiceSource> RetrieveSources()
        {
            return Sources;
        }

        public void CreateNewSource()
        {
        }
        public void EditSource(IWebServiceSource selectedSource)
        {
        }

        public virtual string TestService(IWebService inputValues)
        {
            if (IsTextResponse)
                return new Dev2JsonSerializer().Serialize("dora");
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            var svc = new WebService();
            if (!HasRecError)
                svc.Recordsets = new RecordsetList() { new Recordset() { HasErrors = false, Fields = new List<RecordsetField> { new RecordsetField() { Alias = "bob", Name = "the", RecordsetAlias = "dd", Path = new XmlPath() } } } };
            else
            {
                svc.Recordsets = new RecordsetList() { new Recordset() { HasErrors = true, ErrorMessage = "bobthebuilder", Fields = new List<RecordsetField> { new RecordsetField() { Alias = "bob", Name = "the", RecordsetAlias = "dd", Path = new XmlPath() } } } };

            }
            return serializer.Serialize(svc);
        }
        public void SaveService(IWebService toModel)
        {
        }
        public IStudioUpdateManager UpdateRepository
        {
            get
            {
                return _updateRepository;
            }
        }
        public IQueryManager QueryProxy
        {
            get
            {
                return _queryProxy;
            }
        }
        public ObservableCollection<IWebServiceSource> Sources
        {
            get
            {
                return _sources;
            }
        }
        public string HandlePasteResponse(string current)
        {
            return null;
        }

        #endregion
    }
    public class InputViewForTest : ManageWebServiceInputViewModel
    {
        #region Overrides of ManageWebServiceInputViewModel
        public void ShowView()
        {

        }

        #endregion

        public InputViewForTest(IWebServiceGetViewModel model, IWebServiceModel serviceModel)
            : base(model, serviceModel)
        {
        }
    }
}