/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Logs
{
    [TestClass]
    public class PulseLoggerTests
    {
        bool _elapsed;
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PulseLogger_Ctor")]
// ReSharper disable InconsistentNaming
        public void PulseLogger_Ctor_CheckValues_ExpectInitialised()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var pulseLogger = new PulseLogger(25);
            Assert.AreEqual(pulseLogger.Interval,25);
            PrivateObject pvt = new PrivateObject(pulseLogger);
            System.Timers.Timer timer = (System.Timers.Timer)pvt.GetField("_timer");
            Assert.AreEqual(false,timer.Enabled);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PulseLogger_Ctor")]
// ReSharper disable InconsistentNaming
        public void PulseLogger_Ctor_Start_ExpectInitialised()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var pulseLogger = new PulseLogger(2000);
            
            Assert.AreEqual(pulseLogger.Interval, 2000);
            PrivateObject pvt = new PrivateObject(pulseLogger);
            System.Timers.Timer timer = (System.Timers.Timer)pvt.GetField("_timer");
            timer.Elapsed += (sender, e) =>
                {
                    _elapsed = true;

                };
            Assert.AreEqual(false, timer.Enabled);
            //------------Execute Test---------------------------
            pulseLogger.Start();
            Thread.Sleep(6000);
            //------------Assert Results-------------------------
            Assert.IsTrue(_elapsed);
        }

    }
}
