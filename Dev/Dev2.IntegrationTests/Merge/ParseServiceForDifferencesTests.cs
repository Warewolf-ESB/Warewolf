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
            var parser = new ServiceDifferenceParser(null, new ResourceDefinationCleaner());
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Construct_GivenSameWorkflows_Initialize_NullCleaner()
        {
            //---------------Set up test pack-------------------
            var parser = new ServiceDifferenceParser(new ActivityParser(), null);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts()
        {
            //---------------Set up test pack-------------------
            var resourceId = "c4971c6e-0f16-48a1-9043-c77f7cb694db".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(resourceId);
            var resourceModel = new ResourceModel(_server.Source) { ID = resourceId };
            var xElement = XML.XmlResource.Fetch("SameResourceDecision");
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
            var resourceId = "6dcdd72f-c4ba-484d-9806-8134d8eb2447".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(resourceId);
            var resourceModel = new ResourceModel(_server.Source) { ID = resourceId };
            var xElement = XML.XmlResource.Fetch("SameResourceSwitch");
            var element = xElement.Element("Action");
            Assert.IsNotNull(element);
            var xamlDef = element.ToString(SaveOptions.DisableFormatting);
            resourceModel.WorkflowXaml = new StringBuilder(xamlDef);
            var parser = new ServiceDifferenceParser();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel, false);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.All(tuple => !tuple.hasConflict));
            foreach (var item in valueTuples)
            {
                Assert.IsFalse(parser.NodeHasConflict(item.uniqueId.ToString()));
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts_Sequence()
        {
            //---------------Set up test pack-------------------
            var resourceId = "1b0e0881-9869-4b71-b853-e0c752c38678".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(resourceId);
            var resourceModel = new ResourceModel(_server.Source) { ID = resourceId };
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
            foreach (var item in valueTuples)
            {
                Assert.IsFalse(parser.NodeHasConflict(item.uniqueId.ToString()));
            }
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts_ForEach()
        {
            //---------------Set up test pack-------------------
            var resourceId = "a3ad09e1-a058-4dc1-af6a-b4d856dc0e52".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(resourceId);
            var resourceModel = new ResourceModel(_server.Source) { ID = resourceId };
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
            foreach (var item in valueTuples)
            {
                Assert.IsFalse(parser.NodeHasConflict(item.uniqueId.ToString()));
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDifferences_GivenSameWorkflows_ReturnsNoConflicts_SelectAndApply()
        {
            //---------------Set up test pack-------------------
            var resourceId = "b91d16a5-a4db-4392-aab2-284896debbd3".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(resourceId);
            var resourceModel = new ResourceModel(_server.Source) { ID = resourceId };
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
            foreach (var item in valueTuples)
            {
                Assert.IsFalse(parser.NodeHasConflict(item.uniqueId.ToString()));
            }
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

            var resourceId = "e748cfa6-65e1-4882-86e4-5cc42c3356eb".ToGuid();
            var loadContextualResourceModel = _server.Source.ResourceRepository.LoadContextualResourceModel(resourceId);
            var diff = TestHelper.CreateContextualResourceModel("Decision.SimpleNestedDecision");

            var psd = new ServiceDifferenceParser();
            var diffs = psd.GetDifferences(loadContextualResourceModel, diff, false);

            Assert.AreEqual(1, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.hasConflict));

            ////First Node chart
            var valueTuple = diffs[0];
            var dev2Activity = valueTuple.currentNode.CurrentActivity.GetCurrentValue<IDev2Activity>();
            var dev2Activity1 = valueTuple.differenceNode.CurrentActivity.GetCurrentValue<IDev2Activity>();
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(dev2Activity.UniqueID, dev2Activity1.UniqueID);
            Assert.AreEqual(typeof(DsfDecision), valueTuple.currentNode.CurrentActivity.ItemType);
            Assert.AreEqual(typeof(DsfDecision), valueTuple.differenceNode.CurrentActivity.ItemType);
            foreach (var item in psd.GetAllNodes())
            {
                var uniqueId = item.Key;
                var hasConflict = psd.NodeHasConflict(uniqueId);
                if (uniqueId == "0827abf6-795c-456d-9fe7-ca084fe243db" || uniqueId == "d2c2eb0b-3aa7-4ccf-9b74-4212ebcb20ef")//Main decision and one different assign tool on the true arm
                {
                    Assert.IsTrue(hasConflict, uniqueId + " has conflict");
                }
                else
                {
                    Assert.IsFalse(hasConflict, uniqueId + " has conflict");
                }
            }
        }
    }
}
