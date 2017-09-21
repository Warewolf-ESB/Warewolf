using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.MergeParser;

namespace Dev2.Integration.Tests.Merge
{
    [TestClass]
    public class MergeWorkflowViewModelIntergrationTests
    {
        readonly IServerRepository _server = ServerRepository.Instance;

        [TestInitialize]
        [Owner("Nkosinathi Sangweni")]
        public void Init()
        {
            _server.Source.ResourceRepository.ForceLoad();
            var mockApplicationAdapter = new Mock<IApplicationAdaptor>();
            var mockPopupController = new Mock<IPopupController>();
            var mockServer = new Mock<IServer>();
            var mockShellViewModel = new Mock<IShellViewModel>();
            var mockServerRepository = new Mock<IServerRepository>();
            var mockParseServiceForDifferences = new ParseServiceForDifferences();
            mockServerRepository.Setup(a => a.IsLoaded).Returns(true);
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            CustomContainer.Register(mockApplicationAdapter.Object);
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(mockServerRepository.Object);
            CustomContainer.Register<IParseServiceForDifferences>(mockParseServiceForDifferences);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Initialize_GivenSameResourceModel_ShouldHaveNoDeifferences_desicion()
        {
            //---------------Set up test pack-------------------
            var helloWorldGuid = "41617daa-509e-40eb-aa76-b0827028721d".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(loadContextualResourceModel);
            //---------------Execute Test ----------------------
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(loadContextualResourceModel, loadContextualResourceModel);
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Initialize_GivenSameResourceModel_ShouldHaveNoDeifferences_Switch()
        {
            //---------------Set up test pack-------------------
            var helloWorldGuid = "9e9660d8-1a3c-45ab-a330-673c2343e517".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            var resourceModel = new ResourceModel(_server.Source) { ID = helloWorldGuid };
            var xElement = XML.XmlResource.Fetch("SameResourceSwitch");
            var element = xElement.Element("Action");
            Assert.IsNotNull(element);
            var xamlDef = element.ToString(SaveOptions.DisableFormatting);
            resourceModel.WorkflowXaml = new StringBuilder(xamlDef);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Batch_Compare_All_Examples_Have_No_Differences()
        {
            //---------------Set up test pack-------------------
            var environmentModel = _server.Source;
            environmentModel.Connect();
            var resourceRepository = _server.Source.ResourceRepository;
            
            resourceRepository.Load();
            var resourceModels = resourceRepository.All();
           
            Assert.IsNotNull(resourceModels);
            var examples = resourceModels.Where(model => model.GetSavePath().StartsWith("Examples")).ToList();

            foreach (var example in examples)
            {
                //---------------Assert Precondition----------------
                //---------------Execute Test ----------------------
                var contextualResourceModel = example as IContextualResourceModel;
                var mergeWorkflowViewModel = new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel);
                //---------------Test Result -----------------------
                Assert.IsNotNull(mergeWorkflowViewModel);
                var all = mergeWorkflowViewModel.Conflicts.All(conflict => conflict.DiffViewModel == null);
                Assert.IsTrue(all, example.DisplayName + " Has some differences ");
            }


        }
    }
}
