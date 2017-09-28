using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.MergeParser;

namespace Dev2.Integration.Tests.Merge
{
    [TestClass]
    public class ParseServiceForDifferencesTests
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
            mockServerRepository.Setup(a => a.IsLoaded).Returns(true);
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            CustomContainer.Register(mockApplicationAdapter.Object);
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(_server);
            var mockParseServiceForDifferences = new ServiceDifferenceParser();
            CustomContainer.Register<IServiceDifferenceParser>(mockParseServiceForDifferences);
        }

        [TestCleanup]
        public void CleanUp()
        {
            CustomContainer.DeRegister<IActivityParser>();
            CustomContainer.DeRegister<IShellViewModel>();
            CustomContainer.DeRegister<IServiceDifferenceParser>();
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Construct_GivenSameWorkflows_Initialize()
        {
            //---------------Set up test pack-------------------
            var parser = new ServiceDifferenceParser(null);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts()
        {
            //---------------Set up test pack-------------------
            var helloWorldGuid = "41617daa-509e-40eb-aa76-b0827028721d".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            var resourceModel = new ResourceModel(_server.Source) { ID = helloWorldGuid };
            var xElement = XML.XmlResource.Fetch("SameResource");
            var element = xElement.Element("Action");
            Assert.IsNotNull(element);
            var xamlDef = element.ToString(SaveOptions.DisableFormatting);
            resourceModel.WorkflowXaml = new StringBuilder(xamlDef);
            var parser = new ServiceDifferenceParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.All(tuple => !tuple.hasConflict));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts_Switch()
        {
            //---------------Set up test pack-------------------
            var helloWorldGuid = "41617daa-509e-40eb-aa76-b0827028721d".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            var resourceModel = new ResourceModel(_server.Source) { ID = helloWorldGuid };
            var xElement = XML.XmlResource.Fetch("SameResourceSwitch");
            var element = xElement.Element("Action");
            Assert.IsNotNull(element);
            var xamlDef = element.ToString(SaveOptions.DisableFormatting);
            resourceModel.WorkflowXaml = new StringBuilder(xamlDef);
            var parser = new ServiceDifferenceParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.All(tuple => !tuple.hasConflict));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts_Sequence()
        {
            //---------------Set up test pack-------------------
            var helloWorldGuid = "0bdc3207-ff6b-4c01-a5eb-c7060222f75d".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            var resourceModel = new ResourceModel(_server.Source) { ID = helloWorldGuid };
            var xElement = XML.XmlResource.Fetch("SameResourceSequence");
            var element = xElement.Element("Action");
            Assert.IsNotNull(element);
            var xamlDef = element.ToString(SaveOptions.DisableFormatting);
            resourceModel.WorkflowXaml = new StringBuilder(xamlDef);
            var parser = new ServiceDifferenceParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.All(tuple => !tuple.hasConflict));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts_ForEach()
        {
            //---------------Set up test pack-------------------
            var helloWorldGuid = "8ba79b49-226e-4c67-a732-4657fd0edb6b".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            var resourceModel = new ResourceModel(_server.Source) { ID = helloWorldGuid };
            var xElement = XML.XmlResource.Fetch("SameResourceForEach");
            var element = xElement.Element("Action");
            Assert.IsNotNull(element);
            var xamlDef = element.ToString(SaveOptions.DisableFormatting);
            resourceModel.WorkflowXaml = new StringBuilder(xamlDef);
            var parser = new ServiceDifferenceParser();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.All(tuple => !tuple.hasConflict));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts_SelectAndApply()
        {
            //---------------Set up test pack-------------------
            var helloWorldGuid = "b91d16a5-a4db-4392-aab2-284896debbd3".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            var resourceModel = new ResourceModel(_server.Source) { ID = helloWorldGuid };
            var xElement = XML.XmlResource.Fetch("SelectAndApplyExample");
            var element = xElement.Element("Action");
            Assert.IsNotNull(element);
            var xamlDef = element.ToString(SaveOptions.DisableFormatting);
            resourceModel.WorkflowXaml = new StringBuilder(xamlDef);
            var parser = new ServiceDifferenceParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.All(tuple => !tuple.hasConflict));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_WhenFlatToolAddedOnLocal_ShouldNullOnRemoteChart()
        {
            var activityParser = new ActivityParser();
            var shellView = new Mock<IShellViewModel>();
            var serverMock = new Mock<IServer>();
            shellView.Setup(model => model.ActiveServer).Returns(serverMock.Object);
            CustomContainer.Register(shellView.Object);
            CustomContainer.Register<IActivityParser>(activityParser);

            var randomActivityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
          
            var helloWorldGuid = "49800850-BDF1-4248-93D0-DCD7E5F8B9CA".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(helloWorldGuid);
            var diff = TestHelper.CreateContextualResourceModel("Decision.SimpleNestedDecision");

            var psd = new ServiceDifferenceParser();
            var diffs = psd.GetDifferences(loadContextualResourceModel, diff);

            Assert.AreEqual(1, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.hasConflict));
           
            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.currentTool.modelItem.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.differenceTool.modelItem.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(dev2Activity.UniqueID, dev2Activity1.UniqueID);
            Assert.AreEqual(typeof(DsfDecision), valueTuple.currentTool.modelItem.ItemType);
            Assert.AreEqual(typeof(DsfDecision), valueTuple.differenceTool.modelItem.ItemType);

            //Second chart
            var valueTuple1 = diffs[1];
            var dev2ActivityD = valueTuple1.Item2.modelItem.GetCurrentValue<IDev2Activity>();
            var dev2Activity1D = valueTuple1.Item3.modelItem.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2ActivityD);
            Assert.IsNotNull(dev2Activity1D);
            Assert.AreEqual(calculateUniqueId, dev2ActivityD.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity1D.UniqueID);

            //Third node
            //difference chart
            var valueTuple2 = diffs[2];
            var dev3Activity1D = valueTuple2.Item3.modelItem.GetCurrentValue<IDev2Activity>();
            Assert.IsNull(valueTuple2.Item2.modelItem);
            Assert.IsNotNull(dev3Activity1D);
            //Assert.AreEqual(baseCOnvertId, dev3Activity1D.UniqueID);
        }


    }
}
