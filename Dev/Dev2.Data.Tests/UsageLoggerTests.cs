/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data
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
            using (var usageLogger = new UsageLogger(25))
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
            using (var usageLogger = new UsageLogger(10000))
            {
                Assert.AreEqual(10000, usageLogger.Interval);
                var timer = usageLogger._timer;
                timer.Elapsed += (sender, e) =>
                {
                    _elapsed = true;
                };
                Assert.AreEqual(false, timer.Enabled);
                //------------Execute Test---------------------------
                usageLogger.Start();
                Thread.Sleep(30000);
                //------------Assert Results-------------------------
                Assert.IsTrue(_elapsed);
            }
        }
    }
}