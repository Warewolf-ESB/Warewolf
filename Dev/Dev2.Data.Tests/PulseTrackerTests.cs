using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data
{
    [TestClass]
    public class PulseTrackerTests
    {
        [TestMethod]
        public void PulseTracker_ShouldStartTimerOnStart()
        {
            var pulseTracker =new PulseTracker(50);
            Assert.IsNotNull(pulseTracker);
            var prObj = new PrivateObject(pulseTracker);
            Assert.IsNotNull(prObj);
            var start = pulseTracker.Start();
            Assert.IsTrue(start);
            var timer = prObj.GetField("_timer") as System.Timers.Timer;
            Assert.IsTrue(timer != null && timer.Enabled);
        }

        [TestMethod]
        public void PulseTracker_Should()
        {
            bool elapsed = false;
            var pulseTracker = new PulseTracker(2000);

            Assert.AreEqual(pulseTracker.Interval, 2000);
            PrivateObject pvt = new PrivateObject(pulseTracker);
            System.Timers.Timer timer = (System.Timers.Timer) pvt.GetField("_timer");
            timer.Elapsed += (sender, e) =>
            {
                elapsed = true;
            };
            Assert.AreEqual(false, timer.Enabled);
            pulseTracker.Start();
            Thread.Sleep(6000);
            Assert.IsTrue(elapsed);
        }
    }
}