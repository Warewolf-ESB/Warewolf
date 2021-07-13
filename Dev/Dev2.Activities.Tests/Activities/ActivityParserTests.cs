/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.PathOperations;

namespace Dev2.Tests.Activities.Activities
{
    [TestClass]
    public class ActivityParserTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ActivityParser))]
        public void ActivityParser_ParseToLinkedFlatList()
        {
            var topLevelActivity = new DsfDotNetMultiAssignActivity
            {
                DisplayName = "Assign (1)",
                UniqueID = "d006a409-333a-49d3-8e1c-7c908f6ba461",
            };

            var suspendExecutionActivityOne = new SuspendExecutionActivity
            {
                DisplayName = "Suspend Execution",
                UniqueID = "66b7c885-9ea4-4d75-b822-13ff5ef28128",
            };

            topLevelActivity.NextNodes = new List<IDev2Activity>{suspendExecutionActivityOne};

            var dev2DecisionStackOne = new Dev2DecisionStack { DisplayText = "If [[a]] Is = 2" };
            var flowDecisionActivityOne = new DsfDecision
            {
                DisplayName = "If [[a]] Is = 2",
                UniqueID = "1764efe9-4e0e-423a-8510-b35dc9b053f4",
                NextNodes = null,
                Conditions = dev2DecisionStackOne
            };

            var fileWriteActivityOne = new FileWriteActivity
            {
                DisplayName = "Write File",
                UniqueID = "8e35adbf-d0c4-443b-ab04-2a83ca1aaa62",
            };

            flowDecisionActivityOne.TrueArm =  new []{ fileWriteActivityOne };

            var flowDecisionActivityOneFalseArmAssign = new DsfDotNetMultiAssignActivity
            {
                DisplayName = "Assign (1)",
                UniqueID = "23b599b7-49f4-40a8-8304-f034d227d3dd",
            };

            var flowDecisionActivityOneFalseArmFileWrite = new FileWriteActivity
            {
                DisplayName = "Write File",
                UniqueID = "baad2ce3-371b-4fec-81f4-0da4112078c8",
            };

            flowDecisionActivityOneFalseArmAssign.NextNodes = new List<IDev2Activity>{flowDecisionActivityOneFalseArmFileWrite};
            flowDecisionActivityOneFalseArmFileWrite.NextNodes = new List<IDev2Activity>{suspendExecutionActivityOne};

            flowDecisionActivityOne.FalseArm = new []{ flowDecisionActivityOneFalseArmAssign };

            suspendExecutionActivityOne.NextNodes = new List<IDev2Activity>{flowDecisionActivityOne};

            var suspendExecutionActivityTwo = new SuspendExecutionActivity
            {
                DisplayName = "Suspend Execution",
                UniqueID = "f72ab5fe-efc9-46c5-8944-f2032f0613eb",
            };

            fileWriteActivityOne.NextNodes = new List<IDev2Activity>{suspendExecutionActivityTwo};

            var dev2DecisionStackTwo = new Dev2DecisionStack { DisplayText = "If [[a]] Is = 4" };
            var flowDecisionActivityTwo = new DsfDecision
            {
                DisplayName = "If [[a]] Is = 4",
                UniqueID = "9ad7861e-6fe8-449f-8640-92147259f919",
                NextNodes = null,
                Conditions = dev2DecisionStackTwo
            };

            var fileWriteActivityTwo = new FileWriteActivity
            {
                DisplayName = "Write File",
                UniqueID = "88deb70e-ad45-4735-8a87-a77f7eb54d83",
            };

            var flowDecisionActivityTwoFalseArmAssign = new DsfDotNetMultiAssignActivity
            {
                DisplayName = "Assign (1)",
                UniqueID = "c511b3e4-819f-4c38-81f6-6579ae3f52df",
            };

            var flowDecisionActivityTwoFalseArmFileWrite = new FileWriteActivity
            {
                DisplayName = "Write File",
                UniqueID = "3c477009-7b12-432f-908c-b0ad613e8c57",
            };

            flowDecisionActivityTwoFalseArmAssign.NextNodes = new List<IDev2Activity>{flowDecisionActivityTwoFalseArmFileWrite};
            flowDecisionActivityTwoFalseArmFileWrite.NextNodes = new List<IDev2Activity>{suspendExecutionActivityTwo};

            flowDecisionActivityTwo.NextNodes = null;
            flowDecisionActivityTwo.TrueArm = new []{ fileWriteActivityTwo };
            flowDecisionActivityTwo.FalseArm = new []{ flowDecisionActivityTwoFalseArmAssign };

            suspendExecutionActivityTwo.NextNodes = new List<IDev2Activity>{flowDecisionActivityTwo};

            var activityParser = new ActivityParser();
            var activities = activityParser.ParseToLinkedFlatList(topLevelActivity);

            Assert.IsNotNull(activities);

            var dev2Activities = activities.ToList();
            Assert.AreEqual(13, dev2Activities.Count);
        }
    }
}
