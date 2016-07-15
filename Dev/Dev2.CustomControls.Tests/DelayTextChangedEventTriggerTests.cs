using System;
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
            try
            {
                // ReSharper disable once UnusedVariable
                var changedEventTrigger = new DelayTextChangedEventTrigger();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }
      
    }
}
