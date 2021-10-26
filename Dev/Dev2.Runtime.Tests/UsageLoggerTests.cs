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
using System.IO;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Testing;
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
            //------------Setup for test--------------------------
            using(var usageLogger = new UsageLogger(25))
            {
                Assert.AreEqual(25, usageLogger.Interval);
                var timer = usageLogger._timer;
                Assert.AreEqual(false, timer.Enabled);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(UsageLogger))]
        public void UsageLogger_Ctor_Start_ExpectInitialised()
        {
            //------------Setup for test--------------------------
            using(var usageLogger = new UsageLogger(10000))
            {
                Assert.AreEqual(10000, usageLogger.Interval);
                var timer = usageLogger._timer;
                timer.Elapsed += (sender, e) => { _elapsed = true; };
                Assert.AreEqual(false, timer.Enabled);
                //------------Execute Test---------------------------
                usageLogger.Start();
                Thread.Sleep(30000);
                //------------Assert Results-------------------------
                Assert.IsTrue(_elapsed);
            }
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

            var usageLogger = new UsageLogger(5000, mockUsageTracker.Object, EnvironmentVariablesForTesting.PersistencePathForTests);
            var persistencePath = Path.Combine(Config.UserDataPath, "PersistenceTests");
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

            var usageLogger = new UsageLogger(5000, mockUsageTracker.Object, EnvironmentVariablesForTesting.PersistencePathForTests);
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