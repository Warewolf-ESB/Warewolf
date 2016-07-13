using System;
using Dev2.Runtime.ESB.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    public class EsbServicesEndpointTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                // ReSharper disable once UnusedVariable
                var esbServicesEndpoint = new EsbServicesEndpoint();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }
        
    }
}
