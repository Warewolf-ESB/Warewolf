using Dev2.CustomControls.Trigger;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.CustomControls.Tests
{
    [TestClass]
    public class DelayTextChangedEventTriggerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldCreateInstance()
        {
            //---------------Execute Test ----------------------
            var changedEventTrigger = new DelayTextChangedEventTrigger();
            //---------------Test Result -----------------------
            Assert.IsNotNull(changedEventTrigger, "Cannot create new changedEventTrigger object.");
        }
    }
}
