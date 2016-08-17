using Dev2.CustomControls.Trigger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.CustomControls.Tests
{
    [TestClass]
    public class DelayTextChangedEventTriggerTests
    {
       

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldCreateInstance()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var changedEventTrigger = new DelayTextChangedEventTrigger();
            //---------------Test Result -----------------------
            Assert.IsNotNull(changedEventTrigger, "Cannot create new changedEventTrigger object.");
        }
      
    }
}
