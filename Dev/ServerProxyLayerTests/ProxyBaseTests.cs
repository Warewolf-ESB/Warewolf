using System;
using System.Linq;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ServerProxyLayer;

namespace ServerProxyLayerTests
{
    [TestClass]
    public class ProxyBaseTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProxyBas_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void ProxyBaseNullParam_Factory()
        
        {
            // ReSharper disable ObjectCreationAsStatement
            new ProxyImpl(null , new Mock<IEnvironmentConnection>().Object);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProxyBas_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyBaseNullParam_Connection()
        {
            // ReSharper disable ObjectCreationAsStatement
            new ProxyImpl(new Mock<ICommunicationControllerFactory>().Object, null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ProxyBase_Class")]
        public void ProxyBase_Class_AssertNothingIsAddedToAbstract_ExpectknownTypes()
        {
            //------------Setup for test--------------------------
            var methods = typeof(ProxyBase).GetMethods();
            Assert.AreEqual(5, methods.Count());
            Assert.IsNotNull(methods.FirstOrDefault(a => a.Name.Contains("CommunicationControllerFactory")));
            

        }
        // ReSharper restore InconsistentNaming
    }
}