using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities;
using System.Linq;
using Dev2.Common.Interfaces.Enums;

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

        [TestMethod]
        public void GetDifferences_WhenSameActivityDifferentUniqueId_ShouldReturnAsConflict()
        {
            var randomActivityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
            var otherRandomActivityUniqueId = Guid.NewGuid().ToString();
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
                    Action = new DsfRandomActivity { UniqueID = otherRandomActivityUniqueId },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var psd = new ParseServiceForDifferences(ModelItemUtils.CreateModelItem(chart), ModelItemUtils.CreateModelItem(otherChart));
            var diffs = psd.GetDifferences();
            Assert.IsNotNull(diffs);
            Assert.AreEqual(3, diffs.Count);
            Assert.AreEqual(1,diffs.Count(d => !d.conflict));
            Assert.AreEqual(2, diffs.Count(d => d.conflict));
            Assert.AreEqual(calculateUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(randomActivityUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(otherRandomActivityUniqueId, diffs[2].uniqueId.ToString());

            Assert.AreEqual(calculateUniqueId, diffs[0].activity.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, diffs[1].activity.UniqueID);
            Assert.AreEqual(otherRandomActivityUniqueId, diffs[2].activity.UniqueID);
        }

        [TestMethod]
        public void GetDifferences_WhenSameUniqueIdDifferentPropertyValues_ShouldReturnAsConflict()
        {
            var randomActivityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfRandomActivity {
                        UniqueID = randomActivityUniqueId,
                        RandomType = enRandomType.Numbers
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
                    Action = new DsfRandomActivity {
                        UniqueID = randomActivityUniqueId,
                        RandomType = enRandomType.Letters
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var psd = new ParseServiceForDifferences(ModelItemUtils.CreateModelItem(chart), ModelItemUtils.CreateModelItem(otherChart));
            var diffs = psd.GetDifferences();
            Assert.IsNotNull(diffs);
            Assert.AreEqual(3, diffs.Count);
            Assert.AreEqual(1, diffs.Count(d => !d.conflict));
            Assert.AreEqual(2, diffs.Count(d => d.conflict));
            Assert.AreEqual(calculateUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(randomActivityUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(randomActivityUniqueId, diffs[2].uniqueId.ToString());

            Assert.AreEqual(calculateUniqueId, diffs[0].activity.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, diffs[1].activity.UniqueID);
            Assert.AreEqual(randomActivityUniqueId, diffs[2].activity.UniqueID);
        }

        [TestMethod]
        public void GetDifferences_WhenSameUniqueIdDifferentActivityTypes_ShouldReturnAsConflict()
        {
            var activityUniqueId = Guid.NewGuid().ToString();
            var calculateUniqueId = Guid.NewGuid().ToString();
            var chart = new Flowchart
            {
                StartNode = new FlowStep
                {
                    Action = new DsfCountRecordsetNullHandlerActivity
                    {
                        UniqueID = activityUniqueId,
                        RecordsetName = "[[rec()]]"
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
                        UniqueID = activityUniqueId,
                        RandomType = enRandomType.Letters
                    },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfCalculateActivity { UniqueID = calculateUniqueId }
                    }
                }
            };

            var psd = new ParseServiceForDifferences(ModelItemUtils.CreateModelItem(chart), ModelItemUtils.CreateModelItem(otherChart));
            var diffs = psd.GetDifferences();
            Assert.IsNotNull(diffs);
            Assert.AreEqual(3, diffs.Count);
            Assert.AreEqual(1, diffs.Count(d => !d.conflict));
            Assert.AreEqual(2, diffs.Count(d => d.conflict));
            Assert.AreEqual(calculateUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(activityUniqueId, diffs[1].uniqueId.ToString());
            Assert.AreEqual(activityUniqueId, diffs[2].uniqueId.ToString());

            Assert.AreEqual(calculateUniqueId, diffs[0].activity.UniqueID);
            Assert.AreEqual(activityUniqueId, diffs[1].activity.UniqueID);
            Assert.AreEqual(activityUniqueId, diffs[2].activity.UniqueID);
        }

        [TestMethod]
        public void GetDifferences_WhenOrderOfSameActivities_ShouldReturnAsConflictItems()
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
                    Action = new DsfCalculateActivity { UniqueID = calculateUniqueId },
                    Next =
                    new FlowStep
                    {
                        Action = new DsfRandomActivity { UniqueID = randomActivityUniqueId }
                    }
                }
            };

            var psd = new ParseServiceForDifferences(ModelItemUtils.CreateModelItem(chart), ModelItemUtils.CreateModelItem(otherChart));
            var diffs = psd.GetDifferences();
            Assert.IsNotNull(diffs);
            Assert.AreEqual(4, diffs.Count);
            Assert.IsTrue(diffs.All(d => d.conflict));
            Assert.AreEqual(randomActivityUniqueId, diffs[0].uniqueId.ToString());
            Assert.AreEqual(calculateUniqueId, diffs[1].uniqueId.ToString());

            Assert.AreEqual(randomActivityUniqueId, diffs[0].activity.UniqueID);
            Assert.AreEqual(calculateUniqueId, diffs[1].activity.UniqueID);
        }
    }
}
