using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class Dev2DefinitionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2Definition_GivenIsNew_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var dev2Definition = new Dev2Definition();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(dev2Definition);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2Definition_GivenIsNewParameters_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var dev2Definition = new Dev2Definition("a", "b", "c", false, "", false, "");
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
