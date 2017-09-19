using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities;
using System.Linq;
using Dev2;
using Dev2.Utilities;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Interfaces;

namespace Warewolf.MergeParser.Tests
{
    [TestClass]
    public class ParseServiceForDifferencesTests
    {
       

        [TestMethod]
        public void GetDifferences_WhenSame_ShouldReturnNoConflictItems()
        {
            var randomActivityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity { UniqueID = randomActivityUniqueId },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity { UniqueID = randomActivityUniqueId },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var current = CreateContextualResourceModel(chart);
            var diff = CreateContextualResourceModel(otherChart);

            var psd = new ParseServiceForDifferences();
            var diffs = psd.GetDifferences(current,diff);
            Assert.IsNotNull(diffs);
            Assert.AreEqual(2, diffs.Count);
            Assert.IsTrue(diffs.All(d => !d.conflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[1].uniqueId.ToString());
            var dev2Activity = diffs[0].current.GetCurrentValue() as IDev2Activity;
            var dev2Activity1 = diffs[1].current.GetCurrentValue() as IDev2Activity;
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity.UniqueID);
            Assert.AreEqual(calculateUniqueId, dev2Activity1.UniqueID);
        }

        private static IContextualResourceModel CreateContextualResourceModel(Flowchart chart)
        {
            var workflowHelper = new WorkflowHelper();

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(CustomContainer.Get<IShellViewModel>().ActiveServer, @"WorkflowService",
              "bob");
            tempResource.Category = @"Unassigned\" + "bob";
            tempResource.ResourceName = "bob";
            tempResource.DisplayName = "bob";
            tempResource.IsNewWorkflow = true;
            var builder = workflowHelper.CreateWorkflow("bob");
            builder.Implementation = chart;
            tempResource.WorkflowXaml = workflowHelper.GetXamlDefinition(builder);
            return tempResource;
        }

        [TestMethod]
        public void GetDifferences_WhenDifferent_ShouldReturnConflictItems()
        {
            var randomActivityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity
                    {
                        UniqueID = randomActivityUniqueId,
                        DisplayName = "DisplayName"
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var otherChart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity
                    {
                        UniqueID = randomActivityUniqueId
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var current = CreateContextualResourceModel(chart);
            var diff = CreateContextualResourceModel(otherChart);

            var psd = new ParseServiceForDifferences();
            var diffs = psd.GetDifferences(current, diff);
            Assert.IsNotNull(diffs);
            Assert.AreEqual(2, diffs.Count);
            Assert.IsTrue(diffs.Any(d => d.conflict));
            var valueTuple = diffs[0];
            Assert.AreEqual(calculateUniqueId, valueTuple.uniqueId.ToString());
            var tuple = diffs[1];
            Assert.AreEqual(randomActivityUniqueId, tuple.uniqueId.ToString());
            var dev2Activity = valueTuple.current.GetCurrentValue() as IDev2Activity;
            var dev2Activity1 = tuple.current.GetCurrentValue() as IDev2Activity;
            Assert.IsNotNull(dev2Activity);
            Assert.IsNotNull(dev2Activity1);
            Assert.AreEqual(calculateUniqueId, dev2Activity.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, dev2Activity1.UniqueID);
        }
    }
}
