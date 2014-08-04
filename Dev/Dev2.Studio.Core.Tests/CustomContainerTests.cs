using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    /// <summary>
    /// Summary description for CustomContainerTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class CustomContainerTests
    {
        [TestInitialize]
        public void InitializeContainer()
        {
            CustomContainer.Clear();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CustomContainer_Register")]
        public void CustomContainer_Register_TypeOnce_OneEntryIsRegistered()
        {
            //------------Setup for test--------------------------
           var o = new SimpleObject();
            //------------Execute Test---------------------------
            CustomContainer.Register<ISimpleObject>(o);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, CustomContainer.EntiresCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CustomContainer_Register")]
        public void CustomContainer_Register_TypeTwice_OneEntryIsRegistered()
        {
            //------------Setup for test--------------------------
            var o = new SimpleObject();
            //------------Execute Test---------------------------
            CustomContainer.Register<ISimpleObject>(o);
            CustomContainer.Register<ISimpleObject>(o);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, CustomContainer.EntiresCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CustomContainer_Get")]
        public void CustomContainer_Get_NoTypeIsRegisted_Null()
        {
            //------------Execute Test---------------------------
            var o = CustomContainer.Get<ISimpleObject>();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, CustomContainer.EntiresCount);
            Assert.AreEqual(null, o);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CustomContainer_Get")]
        public void CustomContainer_Get_ThereIsATypeRegisted_Type()
        {
            var simpleObject = new SimpleObject();
            CustomContainer.Register<ISimpleObject>(simpleObject);
            //------------Execute Test---------------------------
            var o = CustomContainer.Get<ISimpleObject>();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, CustomContainer.EntiresCount);
            Assert.AreEqual(simpleObject, o);
        }
    }

    public interface ISimpleObject
    {
        int SimpleInt { get; set; }
        string SimpleString { get; set; }
    }

    public class SimpleObject : ISimpleObject
    {
        public int SimpleInt { get; set; }
        public string SimpleString { get; set; }
    }
}
