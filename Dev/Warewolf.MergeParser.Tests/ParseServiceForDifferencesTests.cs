using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities;
using System.Linq;

namespace Warewolf.MergeParser.Tests
{
    [TestClass]
    public class ParseServiceForDifferencesTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullMergeHead_ShouldThrowException()
        {
            var psd = new ParseServiceForDifferences(null, ModelItemUtils.CreateModelItem());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullHead_ShouldThrowException()
        {
            var psd = new ParseServiceForDifferences(ModelItemUtils.CreateModelItem(), null);
        }
            
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
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId}
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

            var psd = new ParseServiceForDifferences(ModelItemUtils.CreateModelItem(chart), ModelItemUtils.CreateModelItem(otherChart));
            var diffs  = psd.GetDifferences();
            Assert.IsNotNull(diffs);
            Assert.AreEqual(2, diffs.Count);
            Assert.IsTrue(diffs.All(d => !d.conflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[1].uniqueId.ToString());

            Assert.AreEqual(randomActivityUniqueId, diffs[0].activity.UniqueID);
            Assert.AreEqual(calculateUniqueId, diffs[1].activity.UniqueID);
        }
    }
}
