/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Interfaces.Tests
{
    [TestClass]
    public class ServerStatsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerStats))]
        public void ServerStats_SessionId_Set_Get_Success()
        {
            var guid = Guid.NewGuid();
            ServerStats.SessionId = guid;
            Assert.AreEqual(guid,ServerStats.SessionId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerStats))]
        public void ServerStats_TotalExecutions_Success()
        {
            ServerStats.ResetTotalExecutions();
            ServerStats.IncrementTotalExecutions();
            Assert.AreEqual(1,ServerStats.TotalExecutions);
            ServerStats.IncrementTotalExecutions();
            Assert.AreEqual(2,ServerStats.TotalExecutions);
            ServerStats.ResetTotalExecutions();
            Assert.AreEqual(0,ServerStats.TotalExecutions);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerStats))]
        public void ServerStats_UsageServerRetry_Success()
        {
            ServerStats.ResetUsageServerRetry();
            ServerStats.IncrementUsageServerRetry();
            Assert.AreEqual(1,ServerStats.UsageServerRetry);
            ServerStats.IncrementUsageServerRetry();
            Assert.AreEqual(2,ServerStats.UsageServerRetry);
            ServerStats.ResetUsageServerRetry();
            Assert.AreEqual(0,ServerStats.UsageServerRetry);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerStats))]
        public void ServerStats_TotalRequests_Success()
        {
            ServerStats.IncrementTotalRequests();
            Assert.AreEqual(1,ServerStats.TotalRequests);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ServerStats))]
        public void ServerStats_TotalTime_Success()
        {
            var time = new Stopwatch();
            time.Start();
            time.Stop();
            var totalTime = time.ElapsedMilliseconds;
            ServerStats.IncrementTotalTime(totalTime);
            Assert.AreEqual(totalTime,ServerStats.TotalTime);

        }
    }
}