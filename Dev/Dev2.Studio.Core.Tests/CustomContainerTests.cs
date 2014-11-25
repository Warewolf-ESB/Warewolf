
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomContainer_Deregister")]
        public void CustomContainer_Deregister_TypeExists_TypeRemoved()
        {
            //------------Setup for test--------------------------
            var o = new SimpleObject();
            CustomContainer.Register<ISimpleObject>(o);
            var simpleObject = CustomContainer.Get<ISimpleObject>();
            //------------Preconditions--------------------------
            Assert.IsNotNull(simpleObject);
            //------------Execute Test---------------------------
            CustomContainer.DeRegister<ISimpleObject>();
            //------------Assert Results-------------------------
            var objectAfterDeregister = CustomContainer.Get<ISimpleObject>();
            Assert.IsNull(objectAfterDeregister);
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
