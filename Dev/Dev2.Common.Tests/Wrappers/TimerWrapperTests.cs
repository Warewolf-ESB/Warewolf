using System.Threading.Tasks;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Wrappers
{
    [TestClass]
    public class TimerWrapperTests
    {
        [TestMethod]
        public void TimerWrapper_Construct()
        {
            var callCount = 0;
            var expectedState = new { myValue = true };
            void callback(object state)
            {
                callCount++;
            }
            ITimer timerWrapper;
            using (timerWrapper = new TimerWrapper(callback, expectedState, 100, 100))
            {
                Task.Delay(225).Wait();
            }
            timerWrapper.Dispose();

            Assert.AreEqual(2, callCount);
        }
    }
}
