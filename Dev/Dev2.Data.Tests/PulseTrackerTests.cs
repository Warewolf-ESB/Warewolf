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
            var prObj = new Warewolf.Testing.PrivateObject(pulseTracker);
            Assert.IsNotNull(prObj);
            var start = pulseTracker.Start();
            Assert.IsNotNull(start);
            var timer = prObj.GetField("_timer") as System.Timers.Timer;
            Assert.IsTrue(timer != null && timer.Enabled);
        }
    }
}