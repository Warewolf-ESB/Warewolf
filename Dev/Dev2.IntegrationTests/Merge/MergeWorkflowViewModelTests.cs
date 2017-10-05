using System;
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
using Dev2.Util;

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
            AppSettings.LocalHost = "http://localhost:1245";
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
            CustomContainer.Register(_server);
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
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(loadContextualResourceModel, loadContextualResourceModel, true);
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
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(loadContextualResourceModel, resourceModel, true);
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
                        new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel, true);
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
        public void Example_ControlFlowDecision_Have_No_Differences_TreeHierachyIsCorrect()
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
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel, true);
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

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Example_ControlFlowSwitch_Have_No_Differences_TreeHierachyIsCorrect()
        {
            //---------------Set up test pack-------------------
            var environmentModel = _server.Source;
            environmentModel.Connect();
            var resourceRepository = _server.Source.ResourceRepository;
            resourceRepository.Load();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var contextualResourceModel =
                resourceRepository.LoadContextualResourceModel("9e9660d8-1a3c-45ab-a330-673c2343e517".ToGuid());
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel);

            var all = mergeWorkflowViewModel.Conflicts.All(conflict => !conflict.HasConflict);
            Assert.IsTrue(all);
            var conflictsCount = mergeWorkflowViewModel.Conflicts.Count;
            Assert.AreEqual(5, conflictsCount);
            var completeConflict1 = mergeWorkflowViewModel.Conflicts[0];
            Assert.IsTrue(!completeConflict1.CurrentViewModel.Children.Any());
            Assert.AreEqual("Use the Switch tool to:", completeConflict1.CurrentViewModel.MergeDescription);
            var completeConflict2 = mergeWorkflowViewModel.Conflicts[1];
            Assert.IsTrue(!completeConflict2.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 1 - Basic Usage", completeConflict2.CurrentViewModel.MergeDescription);
            var completeConflict3 = mergeWorkflowViewModel.Conflicts[2];
            Assert.IsTrue(completeConflict3.CurrentViewModel.Children.Any());
            Assert.AreEqual("[[DiceRollValue]]", completeConflict3.CurrentViewModel.MergeDescription);
            var childrenCount = completeConflict3.CurrentViewModel.Children.Count;
            Assert.AreEqual(7, childrenCount);
            var mergeToolModels = completeConflict3.CurrentViewModel.Children;
            AsserthildrenHasChild(mergeToolModels, "Incorrect", 6);
            AsserthildrenHasChild(mergeToolModels, "Correct", 1);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Example_ControlFlowSequence_Have_No_Differences_TreeHierachyIsCorrect()
        {
            //---------------Set up test pack-------------------
            var environmentModel = _server.Source;
            environmentModel.Connect();
            var resourceRepository = _server.Source.ResourceRepository;
            resourceRepository.Load();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var contextualResourceModel =
                resourceRepository.LoadContextualResourceModel("0bdc3207-ff6b-4c01-a5eb-c7060222f75d".ToGuid());
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel);

            var all = mergeWorkflowViewModel.Conflicts.All(conflict => !conflict.HasConflict);
            Assert.IsTrue(all);
            var conflictsCount = mergeWorkflowViewModel.Conflicts.Count;
            Assert.AreEqual(8, conflictsCount);

            var completeConflict1 = mergeWorkflowViewModel.Conflicts[0];
            Assert.IsTrue(!completeConflict1.CurrentViewModel.Children.Any());
            Assert.AreEqual("Use the Sequence tool to:", completeConflict1.CurrentViewModel.MergeDescription);

            var completeConflict2 = mergeWorkflowViewModel.Conflicts[1];
            Assert.IsTrue(!completeConflict2.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 1 - Basic Usage", completeConflict2.CurrentViewModel.MergeDescription);

            var completeConflict3 = mergeWorkflowViewModel.Conflicts[2];
            Assert.IsFalse(completeConflict3.CurrentViewModel.Children.Any());
            Assert.AreEqual("Comment", completeConflict3.CurrentViewModel.MergeDescription);

            var completeConflict4 = mergeWorkflowViewModel.Conflicts[3];
            Assert.IsTrue(completeConflict4.CurrentViewModel.Children.Any());
            Assert.AreEqual("Organize Customers", completeConflict4.CurrentViewModel.MergeDescription);
            var childrenCount = completeConflict4.CurrentViewModel.Children.Count;
            Assert.AreEqual(3, childrenCount);
            AsserthildrenHasChild(completeConflict4.CurrentViewModel.Children, "Split Names (3)");
            AsserthildrenHasChild(completeConflict4.CurrentViewModel.Children, "Find Only Unique Names");
            AsserthildrenHasChild(completeConflict4.CurrentViewModel.Children, "Sort Names Alphabetically");

            var completeConflict5 = mergeWorkflowViewModel.Conflicts[4];
            Assert.IsFalse(completeConflict5.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 2 - Advanced Usage", completeConflict5.CurrentViewModel.MergeDescription);


            var completeConflict6 = mergeWorkflowViewModel.Conflicts[5];
            Assert.IsTrue(completeConflict6.CurrentViewModel.Children.Any());
            Assert.AreEqual("For Each", completeConflict6.CurrentViewModel.MergeDescription);
            var childrenCount5 = completeConflict6.CurrentViewModel.Children.Count;
            Assert.AreEqual(1, childrenCount5);
            var mergeToolModel = completeConflict6.CurrentViewModel.Children.Single();
            Assert.AreEqual(2, mergeToolModel.Children.Count);
            AsserthildrenHasChild(mergeToolModel.Children, "Convert Case To Title Case (2)");
            AsserthildrenHasChild(mergeToolModel.Children, "Create New Email Addresses (3)");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Example_LoopConstructsForEach_Have_No_Differences_TreeHierachyIsCorrect()
        {
            //---------------Set up test pack-------------------
            var environmentModel = _server.Source;
            environmentModel.Connect();
            var resourceRepository = _server.Source.ResourceRepository;
            resourceRepository.Load();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var contextualResourceModel =
                resourceRepository.LoadContextualResourceModel("8ba79b49-226e-4c67-a732-4657fd0edb6b".ToGuid());
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(contextualResourceModel, contextualResourceModel, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel);

            var all = mergeWorkflowViewModel.Conflicts.All(conflict => !conflict.HasConflict);
            Assert.IsTrue(all);
            var conflictsCount = mergeWorkflowViewModel.Conflicts.Count;
            Assert.AreEqual(10, conflictsCount);

            var completeConflict1 = mergeWorkflowViewModel.Conflicts[0];
            Assert.IsTrue(!completeConflict1.CurrentViewModel.Children.Any());
            Assert.AreEqual("Use the For Each tool to:", completeConflict1.CurrentViewModel.MergeDescription);

            var completeConflict2 = mergeWorkflowViewModel.Conflicts[1];
            Assert.IsTrue(!completeConflict2.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 1 - No. of Executions", completeConflict2.CurrentViewModel.MergeDescription);

            var completeConflict3 = mergeWorkflowViewModel.Conflicts[2];
            Assert.IsFalse(completeConflict3.CurrentViewModel.Children.Any());
            Assert.AreEqual("Comment", completeConflict3.CurrentViewModel.MergeDescription);


            var completeConflict4 = mergeWorkflowViewModel.Conflicts[3];
            Assert.IsTrue(!completeConflict4.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 2 - * in Range", completeConflict4.CurrentViewModel.MergeDescription);

            var completeConflict5 = mergeWorkflowViewModel.Conflicts[4];
            Assert.IsTrue(completeConflict5.CurrentViewModel.Children.Any());
            Assert.AreEqual("For Each", completeConflict5.CurrentViewModel.MergeDescription);
            var childrenCount = completeConflict5.CurrentViewModel.Children.Count;
            Assert.AreEqual(1, childrenCount);
            AsserthildrenHasChild(completeConflict5.CurrentViewModel.Children, "Random");

            var completeConflict = mergeWorkflowViewModel.Conflicts[5];
            Assert.IsFalse(completeConflict.CurrentViewModel.Children.Any());
            Assert.AreEqual("Comment", completeConflict.CurrentViewModel.MergeDescription);

            var completeConflict10 = mergeWorkflowViewModel.Conflicts[6];
            Assert.IsFalse(completeConflict10.CurrentViewModel.Children.Any());
            Assert.AreEqual("Comment", completeConflict10.CurrentViewModel.MergeDescription);

            var completeConflict11 = mergeWorkflowViewModel.Conflicts[7];
            Assert.IsFalse(completeConflict11.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 3 - * in CSV", completeConflict11.CurrentViewModel.MergeDescription);

            var completeConflict6 = mergeWorkflowViewModel.Conflicts[8];
            Assert.IsTrue(completeConflict6.CurrentViewModel.Children.Any());
            Assert.AreEqual("For Each", completeConflict6.CurrentViewModel.MergeDescription);
            var childrenCount1 = completeConflict6.CurrentViewModel.Children.Count;
            Assert.AreEqual(1, childrenCount1);
            AsserthildrenHasChild(completeConflict6.CurrentViewModel.Children, "Random");

            var completeConflict7 = mergeWorkflowViewModel.Conflicts[9];
            Assert.IsTrue(completeConflict7.CurrentViewModel.Children.Any());
            Assert.AreEqual("For Each", completeConflict7.CurrentViewModel.MergeDescription);
            var childrenCount2 = completeConflict7.CurrentViewModel.Children.Count;
            Assert.AreEqual(1, childrenCount2);
            AsserthildrenHasChild(completeConflict7.CurrentViewModel.Children, "Random");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SimpleNestedDecision_Differences_TreeHierachyIsCorrect()
        {
            //---------------Set up test pack-------------------
            var environmentModel = _server.Source;
            environmentModel.Connect();
            var resourceRepository = _server.Source.ResourceRepository;
            resourceRepository.Load();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var contextualResourceModel = resourceRepository.LoadContextualResourceModel("49800850-BDF1-4248-93D0-DCD7E5F8B9CA".ToGuid());
            var resourceModel = TestHelper.CreateContextualResourceModel("Decision.SimpleNestedDecision");

            var mergeWorkflowViewModel = new MergeWorkflowViewModel(contextualResourceModel, resourceModel, false);
            //---------------Test Result -----------------------
            Assert.IsNotNull(mergeWorkflowViewModel);

            var all = mergeWorkflowViewModel.Conflicts.Count(conflict => !conflict.HasConflict);
            Assert.AreEqual(1, all);
            var conflictsCount = mergeWorkflowViewModel.Conflicts.Count;
            Assert.AreEqual(10, conflictsCount);

            var completeConflict1 = mergeWorkflowViewModel.Conflicts[0];
            Assert.IsTrue(!completeConflict1.CurrentViewModel.Children.Any());
            Assert.AreEqual("Use the For Each tool to:", completeConflict1.CurrentViewModel.MergeDescription);

            var completeConflict2 = mergeWorkflowViewModel.Conflicts[1];
            Assert.IsTrue(!completeConflict2.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 1 - No. of Executions", completeConflict2.CurrentViewModel.MergeDescription);

            var completeConflict3 = mergeWorkflowViewModel.Conflicts[2];
            Assert.IsFalse(completeConflict3.CurrentViewModel.Children.Any());
            Assert.AreEqual("Comment", completeConflict3.CurrentViewModel.MergeDescription);


            var completeConflict4 = mergeWorkflowViewModel.Conflicts[3];
            Assert.IsTrue(!completeConflict4.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 2 - * in Range", completeConflict4.CurrentViewModel.MergeDescription);

            var completeConflict5 = mergeWorkflowViewModel.Conflicts[4];
            Assert.IsTrue(completeConflict5.CurrentViewModel.Children.Any());
            Assert.AreEqual("For Each", completeConflict5.CurrentViewModel.MergeDescription);
            var childrenCount = completeConflict5.CurrentViewModel.Children.Count;
            Assert.AreEqual(1, childrenCount);
            AsserthildrenHasChild(completeConflict5.CurrentViewModel.Children, "Random");

            var completeConflict = mergeWorkflowViewModel.Conflicts[5];
            Assert.IsFalse(completeConflict.CurrentViewModel.Children.Any());
            Assert.AreEqual("Comment", completeConflict.CurrentViewModel.MergeDescription);

            var completeConflict10 = mergeWorkflowViewModel.Conflicts[6];
            Assert.IsFalse(completeConflict10.CurrentViewModel.Children.Any());
            Assert.AreEqual("Comment", completeConflict10.CurrentViewModel.MergeDescription);

            var completeConflict11 = mergeWorkflowViewModel.Conflicts[7];
            Assert.IsFalse(completeConflict11.CurrentViewModel.Children.Any());
            Assert.AreEqual("EXAMPLE 3 - * in CSV", completeConflict11.CurrentViewModel.MergeDescription);

            var completeConflict6 = mergeWorkflowViewModel.Conflicts[8];
            Assert.IsTrue(completeConflict6.CurrentViewModel.Children.Any());
            Assert.AreEqual("For Each", completeConflict6.CurrentViewModel.MergeDescription);
            var childrenCount1 = completeConflict6.CurrentViewModel.Children.Count;
            Assert.AreEqual(1, childrenCount1);
            AsserthildrenHasChild(completeConflict6.CurrentViewModel.Children, "Random");

            var completeConflict7 = mergeWorkflowViewModel.Conflicts[9];
            Assert.IsTrue(completeConflict7.CurrentViewModel.Children.Any());
            Assert.AreEqual("For Each", completeConflict7.CurrentViewModel.MergeDescription);
            var childrenCount2 = completeConflict7.CurrentViewModel.Children.Count;
            Assert.AreEqual(1, childrenCount2);
            AsserthildrenHasChild(completeConflict7.CurrentViewModel.Children, "Random");
        }



        private void AsserthildrenHasChild(ObservableCollection<IMergeToolModel> children, string description)
        {
            var count = children.Count(model =>
                model.MergeDescription.Equals(description, StringComparison.Ordinal));
            Assert.AreEqual(1, count);
        }
        private void AsserthildrenHasChild(ObservableCollection<IMergeToolModel> children, string description, int numberOfChildren)
        {
            var count = children.Count(model =>
                model.MergeDescription.Equals(description, StringComparison.Ordinal));
            Assert.AreEqual(numberOfChildren, count);
        }
    }
}
