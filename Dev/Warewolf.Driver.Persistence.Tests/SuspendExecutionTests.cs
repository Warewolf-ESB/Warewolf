/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Driver.Persistence.Tests
{
    [TestClass]
    public class SuspendExecutionTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecution))]
        public void SuspendExecution_CreateandScheduleJob_Hangfire_SuspendForDays_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var suspendOption =  enSuspendOption.SuspendForDays;
            var suspendOptionValue = "1";

            var scheduler = new SuspendExecution();
            var jobId = scheduler.CreateAndScheduleJob(suspendOption,suspendOptionValue, values);
            Assert.IsInstanceOfType(int.Parse(jobId),typeof(int));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecution))]
        public void SuspendExecution_CreateandScheduleJob_Hangfire_SuspendForHours_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var suspendOption =  enSuspendOption.SuspendForHours;
            var suspendOptionValue = "1";

            var scheduler = new SuspendExecution();
            var jobId = scheduler.CreateAndScheduleJob(suspendOption,suspendOptionValue, values);
            Assert.IsInstanceOfType(int.Parse(jobId),typeof(int));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecution))]
        public void SuspendExecution_CreateandScheduleJob_Hangfire_SuspendForMonths_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var suspendOption =  enSuspendOption.SuspendForMonths;
            var suspendOptionValue = "1";

            var scheduler = new SuspendExecution();
            var jobId = scheduler.CreateAndScheduleJob(suspendOption,suspendOptionValue, values);
            Assert.IsInstanceOfType(int.Parse(jobId),typeof(int));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecution))]
        public void SuspendExecution_CreateandScheduleJob_Hangfire_SuspendForMinutes_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var suspendOption =  enSuspendOption.SuspendForMinutes;
            var suspendOptionValue = "1";

            var scheduler = new SuspendExecution();
            var jobId = scheduler.CreateAndScheduleJob(suspendOption,suspendOptionValue, values);
            Assert.IsInstanceOfType(int.Parse(jobId),typeof(int));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecution))]
        public void SuspendExecution_CreateandScheduleJob_Hangfire_SuspendForSeconds_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var suspendOption =  enSuspendOption.SuspendForSeconds;
            var suspendOptionValue = "1";

            var scheduler = new SuspendExecution();
            var jobId = scheduler.CreateAndScheduleJob(suspendOption,suspendOptionValue, values);
            Assert.IsInstanceOfType(int.Parse(jobId),typeof(int));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecution))]
        public void SuspendExecution_CreateandScheduleJob_Hangfire_SuspendUntil_Success()
        {
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
                {"versionNumber", new StringBuilder("1")}
            };

            var suspendOption =  enSuspendOption.SuspendUntil;
            var suspendOptionValue = DateTime.Now.AddDays(1).ToString();

            var scheduler = new SuspendExecution();
            var jobId = scheduler.CreateAndScheduleJob(suspendOption,suspendOptionValue, values);
            Assert.IsInstanceOfType(int.Parse(jobId),typeof(int));
        }
    }
}