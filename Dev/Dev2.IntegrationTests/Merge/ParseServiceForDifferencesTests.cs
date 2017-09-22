using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.ExtMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.MergeParser;

namespace Dev2.Integration.Tests.Merge
{
    [TestClass]
    public class ParseServiceForDifferencesTests
    {
        readonly IServerRepository _server = ServerRepository.Instance;
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenSameWorkflows_Initialize()
        {
            //---------------Set up test pack-------------------
            var parser = new ParseServiceForDifferences();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var parserCurrentDifferences = parser.CurrentDifferences;
            var parserDifferences = parser.Differences;
            //---------------Test Result -----------------------
            Assert.IsNotNull(parserCurrentDifferences);
            Assert.IsNotNull(parserDifferences);
            //Assert.AreEqual(0, parserDifferences.Count);
            //Assert.AreEqual(0, parserDifferences.Count);
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
            var parser = new ParseServiceForDifferences();
            //---------------Assert Precondition----------------
            var parserCurrentDifferences = parser.CurrentDifferences;
            var parserDifferences = parser.Differences;
            Assert.IsNotNull(parserCurrentDifferences);
            Assert.IsNotNull(parserDifferences);
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.differenceStore.All(tuple => !tuple.Value));
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
            var parser = new ParseServiceForDifferences();
            //---------------Assert Precondition----------------
            var parserCurrentDifferences = parser.CurrentDifferences;
            var parserDifferences = parser.Differences;
            Assert.IsNotNull(parserCurrentDifferences);
            Assert.IsNotNull(parserDifferences);
            //Assert.AreEqual(0, parserDifferences.Count);
            //Assert.AreEqual(0, parserDifferences.Count);
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.differenceStore.All(tuple => !tuple.Value));
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
            var parser = new ParseServiceForDifferences();
            //---------------Assert Precondition----------------
            var parserCurrentDifferences = parser.CurrentDifferences;
            var parserDifferences = parser.Differences;
            Assert.IsNotNull(parserCurrentDifferences);
            Assert.IsNotNull(parserDifferences);
            //Assert.AreEqual(0, parserDifferences.Count);
            //Assert.AreEqual(0, parserDifferences.Count);
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.differenceStore.All(tuple => !tuple.Value));
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
            var parser = new ParseServiceForDifferences();
            //---------------Assert Precondition----------------
            var parserCurrentDifferences = parser.CurrentDifferences;
            var parserDifferences = parser.Differences;
            Assert.IsNotNull(parserCurrentDifferences);
            Assert.IsNotNull(parserDifferences);
            //Assert.AreEqual(0, parserDifferences.Count);
            //Assert.AreEqual(0, parserDifferences.Count);
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.differenceStore.All(tuple => !tuple.Value));
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
            var parser = new ParseServiceForDifferences();
            //---------------Assert Precondition----------------
            var parserCurrentDifferences = parser.CurrentDifferences;
            var parserDifferences = parser.Differences;
            Assert.IsNotNull(parserCurrentDifferences);
            Assert.IsNotNull(parserDifferences);
            //Assert.AreEqual(0, parserDifferences.Count);
            //Assert.AreEqual(0, parserDifferences.Count);
            //---------------Execute Test ----------------------
            var valueTuples = parser.GetDifferences(loadContextualResourceModel, resourceModel);
            //---------------Test Result -----------------------
            Assert.IsTrue(valueTuples.differenceStore.All(tuple => !tuple.Value));
        }

        
    }
}
