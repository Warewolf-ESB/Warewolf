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
using System.IO;
using System.Threading;
using System.Timers;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Runtime;
using Dev2.Runtime.Subscription;
using Hangfire.Dashboard.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nest;
using Newtonsoft.Json;
using Warewolf.Usage;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class UsageLoggerTests
    {
        bool _elapsed;

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(UsageLogger))]
        public void UsageLogger_Ctor_CheckValues_ExpectInitialised()
        {
            var usageLogger = new Mock<IUsageLogger>();
            usageLogger.Setup(o => o.Start()).Returns(new UsageLogger(25));
            usageLogger.Setup(o => o.Timer).Returns(new System.Timers.Timer(25));
            usageLogger.Setup(o => o.Interval).Returns(25);
            var usageLoggerObj = usageLogger.Object;
            
            //------------Setup for test--------------------------
            Assert.AreEqual(25, usageLoggerObj.Interval);
            var timer = usageLoggerObj.Timer;
            Assert.AreEqual(false, timer.Enabled);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(UsageLogger))]
        public void UsageLogger_Ctor_Start_ExpectInitialised()
        {
            var t = new System.Timers.Timer(10000);
            var usageLogger = new Mock<IUsageLogger>();
            var mockUsageTracker = new Mock<IUsageTrackerWrapper>();
            usageLogger.Setup(o => o.Start()).Returns(new UsageLogger(10000)).Callback(() => t.Enabled = true);
            usageLogger.Setup(o => o.Timer).Returns(t);
            usageLogger.Setup(o => o.Interval).Returns(10000);
            var usageLoggerObj = usageLogger.Object;

            //------------Setup for test--------------------------
            Assert.AreEqual(10000, usageLoggerObj.Interval);
            var timer = usageLoggerObj.Timer;
            timer.Elapsed += (sender, e) => { _elapsed = true; };
            Assert.AreEqual(false, timer.Enabled);
            //------------Execute Test---------------------------
            usageLoggerObj.Start();
            Thread.Sleep(30000);
            //------------Assert Results-------------------------
            Assert.IsTrue(_elapsed);

        }

        [TestMethod]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(UsageLogger))]
        public void UsageLogger_TrackUsage_Failed()
        {
            //------------Setup for test--------------------------
            var testGuid = Guid.NewGuid();
            ServerStats.SessionId = testGuid;
            var mockUsageTracker = new Mock<IUsageTrackerWrapper>();
            mockUsageTracker.Setup(o => o.TrackEvent(It.IsAny<string>(), It.IsAny<UsageType>(), It.IsAny<string>())).Returns(UsageDataResult.networkConnectionError);

            var usageLogger = new UsageLogger(5000, mockUsageTracker.Object);
            var persistencePath = Path.Combine(Config.UserDataPath, "Persistence");
            var persistenceGuidPath = Path.Combine(persistencePath, testGuid.ToString());

            if(File.Exists(persistencePath))
            {
                File.Delete(persistenceGuidPath);
            }

            var timer = usageLogger._timer;
            timer.Elapsed += (sender, e) => { usageLogger.TrackUsage(UsageType.ServerStart, testGuid); };
            Assert.AreEqual(false, timer.Enabled);
            //------------Execute Test---------------------------
            usageLogger.Start();
            Thread.Sleep(10000);
            //------------Assert Results-------------------------
            Assert.IsTrue(Directory.Exists(persistencePath));
            Assert.IsTrue(File.Exists(persistenceGuidPath));
            File.Delete(persistenceGuidPath);
        }

        [TestMethod]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(UsageLogger))]
        public void UsageLogger_TrackUsage_Success()
        {
            var testGuid = Guid.NewGuid();
            ServerStats.SessionId = testGuid;
            //------------Setup for test--------------------------
            var mockUsageTracker = new Mock<IUsageTrackerWrapper>();
            mockUsageTracker.Setup(o => o.TrackEvent(It.IsAny<string>(), It.IsAny<UsageType>(), It.IsAny<string>())).Returns(UsageDataResult.ok);

            var usageLogger = new UsageLogger(5000, mockUsageTracker.Object);
            var persistencePath = Path.Combine(Config.UserDataPath, "Persistence");
            var persistenceGuidPath = Path.Combine(persistencePath, testGuid.ToString());

            if(File.Exists(persistencePath))
            {
                File.Delete(persistenceGuidPath);
            }

            var timer = usageLogger._timer;
            timer.Elapsed += (sender, e) => { usageLogger.TrackUsage(UsageType.ServerStart, testGuid); };
            Assert.AreEqual(false, timer.Enabled);
            //------------Execute Test---------------------------
            usageLogger.Start();
            Thread.Sleep(10000);
            //------------Assert Results-------------------------
            Assert.IsTrue(!File.Exists(persistenceGuidPath));
        }
    }
}