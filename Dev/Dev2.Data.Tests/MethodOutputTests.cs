using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class MethodOutputTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MethodOutput_GivenIsNew_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var dev2Definition = new MethodOutput();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(dev2Definition);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MethodOutput_GivenIsNewParameters_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var dev2Definition = new MethodOutput("a", "b", "c", false, "", false, "", false, "", false);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dev2Definition);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("a", dev2Definition.Name);
            Assert.AreEqual("b", dev2Definition.MapsTo);
            Assert.AreEqual("c", dev2Definition.Value);
        }
    }
}