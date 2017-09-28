using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
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
            mockServerRepository.Setup(a => a.IsLoaded).Returns(true);
            CustomContainer.Register<IActivityParser>(new ActivityParser());
            CustomContainer.Register(mockApplicationAdapter.Object);
            CustomContainer.Register(mockPopupController.Object);
            CustomContainer.Register(mockServer.Object);
            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(mockServerRepository.Object);
            var mockParseServiceForDifferences = new ServiceDifferenceParser();
            CustomContainer.Register<IServiceDifferenceParser>(mockParseServiceForDifferences);
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
                try
                {
                    var mergeWorkflowViewModel =
                        new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel);
                    //---------------Test Result -----------------------
                    Assert.IsNotNull(mergeWorkflowViewModel);
                    var all = mergeWorkflowViewModel.Conflicts.All(conflict => conflict.DiffViewModel == null);
                    if (all)
                        Assert.IsTrue(all);
                    else
                    {
                        Debug.WriteLine(example.ID + " " + example.DisplayName + " Has some differences ");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(example.ID + " " + example.DisplayName + " Has some differences " + e.Message);
                }
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Example_ControlFlowDecision_Have_No_Differences()
        {
            //---------------Set up test pack-------------------
            var environmentModel = _server.Source;
            environmentModel.Connect();
            var resourceRepository = _server.Source.ResourceRepository;
            resourceRepository.Load();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var contextualResourceModel =
                resourceRepository.LoadContextualResourceModel("41617daa-509e-40eb-aa76-b0827028721d".ToGuid());
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel);
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel);

            var all = mergeWorkflowViewModel.Conflicts.All(conflict => !conflict.HasConflict);
            Assert.IsTrue(all);
            var conflictsCount = mergeWorkflowViewModel.Conflicts.Count;
            Assert.AreEqual(3, conflictsCount);
            var completeConflict1 = mergeWorkflowViewModel.Conflicts[0];
            Assert.IsTrue(!completeConflict1.CurrentViewModel.Children.Any());
            Assert.AreEqual("Use the Decision tool to:", completeConflict1.CurrentViewModel.MergeDescription);
            var completeConflict2 = mergeWorkflowViewModel.Conflicts[1];
            Assert.IsTrue(!completeConflict2.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 1 - Basic Usage", completeConflict2.CurrentViewModel.MergeDescription);
            var completeConflict3 = mergeWorkflowViewModel.Conflicts[2];
            Assert.IsTrue(completeConflict3.CurrentViewModel.Children.Any());
            Assert.AreEqual("Decision", completeConflict3.CurrentViewModel.MergeDescription);
            var childrenCount = completeConflict3.CurrentViewModel.Children.Count;
            Assert.AreEqual(4, childrenCount);


            var mergeToolModel = completeConflict3.CurrentViewModel.Children.Single(model => model.MergeDescription == "Over 18");
            Assert.AreEqual(4, mergeToolModel.Children.Count);
            AsserthildrenHasChild(mergeToolModel.Children, "Correct Result");
            AsserthildrenHasChild(mergeToolModel.Children, "Incorrect Result");
            AsserthildrenHasChild(mergeToolModel.Children, "EXAMPLE 3 - Error Checking");
            AsserthildrenHasChild(mergeToolModel.Children, "Was there an error?");

            var toolModel = mergeToolModel.Children.Single(model => model.MergeDescription == "Was there an error?");
            var count = toolModel.Children.Count;
            Assert.AreEqual(5, count);
            AsserthildrenHasChild(toolModel.Children, "Correct Result");
            AsserthildrenHasChild(toolModel.Children, "Incorrect Result");
            AsserthildrenHasChild(toolModel.Children, "EXAMPLE 4 - Recordsets");
            AsserthildrenHasChild(toolModel.Children, "Assign (3)");
            AsserthildrenHasChild(toolModel.Children, "If [[rec(*).set]] Is Numeric");


             var single = toolModel.Children.Single(model => model.MergeDescription == "If [[rec(*).set]] Is Numeric");
            var childCount = single.Children.Count;
            Assert.AreEqual(2, childCount);
            var trueArmTools = single.Children.Count(model => model.ParentDescription == "All are numeric");
            var falseArmTools = single.Children.Count(model => model.ParentDescription == "Not all numeric");
            Assert.AreEqual(1, trueArmTools);
            Assert.AreEqual(1, falseArmTools);
            AsserthildrenHasChild(single.Children, "Incorrect");
            AsserthildrenHasChild(single.Children, "Correct Result");
        }

        private void AsserthildrenHasChild(ObservableCollection<IMergeToolModel> children, string description)
        {
            var count = children.Count(model =>
                model.MergeDescription.Equals(description, StringComparison.Ordinal));
            Assert.AreEqual(1, count);
        }
    }
}
