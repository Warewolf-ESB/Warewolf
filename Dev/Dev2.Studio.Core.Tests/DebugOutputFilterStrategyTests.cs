/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DebugOutputFilterStrategyTests
    {
        static DebugOutputFilterStrategy _debugOutputFilterStrategy;

        [ClassInitialize]
        public static void MyTestClassInitialize(TestContext testContext)
        {
            _debugOutputFilterStrategy = new DebugOutputFilterStrategy();
           
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]

        public void DebugOutputFilterStrategy_Filter_Where_ContentIsNull_Expected_False()
        {
            var actual = _debugOutputFilterStrategy.Filter(null, "");

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Filter_Where_FilterTextIsNull_Expected_False()
        {
            var actual = _debugOutputFilterStrategy.Filter("", null);

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsString_And_FilterTextContainsMatch_Expected_True()
        {
            var actual = _debugOutputFilterStrategy.Filter("cake", "ak");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsString_And_FilterTextDoesntContainMatch_Expected_false()
        {
            var actual = _debugOutputFilterStrategy.Filter("cake", "123");

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_FilterTextMatchesNothing_Expected_False()
        {
            var debugState = new DebugState();

            var actual = _debugOutputFilterStrategy.Filter(debugState, "cake");

            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_FilterTextMatchesActivityType_Expected_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Workflow };

            var actual = _debugOutputFilterStrategy.Filter(debugState, "work");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_FilterTextMatchesDisplayName_Expected_True()
        {
            var debugState = new DebugState { DisplayName = "Cake" };

            var actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_ActivityTypeIsStep_And_FilterTextMatchesName_Expected_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Step, DisplayName = "Cake" };

            var actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_ActivityTypeIsWorkflow_And_FilterTextMatchesServer_Expected_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Workflow, DisplayName = "Cake" };
            var actual = _debugOutputFilterStrategy.Filter(debugState, "service");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_ActivityTypeIsStep_And_FilterTextMatchesDurration_Expected_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Step, StartTime = new DateTime(2012, 01, 02, 1, 2, 3), EndTime = new DateTime(2012, 01, 02, 2, 2, 3) };

            var actual = _debugOutputFilterStrategy.Filter(debugState, "01:");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_ActivityTypeIsWorkflow_And_FilterTextMatchesStartTime_Expected_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Workflow, StateType = StateType.Before, StartTime = new DateTime(2012, 01, 02, 1, 2, 3) };
            var actual = _debugOutputFilterStrategy.Filter(debugState, "2012");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_ActivityTypeIsWorkflow_And_FilterTextMatchesEndTime_Expected_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Workflow, StateType = StateType.After, EndTime = new DateTime(2012, 01, 02, 2, 2, 3) };

            var actual = _debugOutputFilterStrategy.Filter(debugState, "2012");

            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_FilterTextMatchesInputOnName_Expected_True()
        {
            var debugState = new DebugState();
            var itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = "cake" });
            debugState.Inputs.Add(itemToAdd);

            const bool Expected = true;
            var actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(Expected, actual);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_FilterTextMatchesOuputOnValue_Expected_True()
        {
            var debugState = new DebugState();
            var itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = "cake" });
            debugState.Outputs.Add(itemToAdd);

            const bool Expected = true;
            var actual = _debugOutputFilterStrategy.Filter(debugState, "ak");

            Assert.AreEqual(Expected, actual);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_ActivityType_Step_And_debugState_NameNotnull_AND_debugState_Name_Contains_filterText_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Step, Name = "Cake" };
            var actual = _debugOutputFilterStrategy.Filter(debugState, "Cake");
            Assert.AreEqual(true, actual);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_ActivityType_WokFlow_And_debugState_ServerNotnull_AND_debugState_Name_Contains_filterText_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Workflow, Server = "Cake" };
            var actual = _debugOutputFilterStrategy.Filter(debugState, "Cake");
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_debugState_VersionNotnull_And_debugState_Version_Contains_filterText_True()
        {
            var debugState = new DebugState {  Version = "Version1" };
            var actual = _debugOutputFilterStrategy.Filter(debugState, "Version1");
            Assert.AreEqual(true, actual);
        }

        

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DebugOutputFilterStrategy))]
        public void DebugOutputFilterStrategy_Where_ContentIsDebugState_And_ActivityTypeStep_And_FilterTextMatchesServer_Step_Expected_True()
        {
            var debugState = new DebugState { ActivityType = ActivityType.Step, Name = "Cake" };
            var actual = _debugOutputFilterStrategy.Filter(debugState, "step");

            Assert.AreEqual(true, actual);
        }
    }
}
