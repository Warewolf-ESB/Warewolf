using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    public class ServiceLocatorTests
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var pCounter = new Mock<IWarewolfPerformanceCounterLocater>();
            CustomContainer.Register(pCounter.Object);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construnct_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            ServiceLocator locator = new ServiceLocator();
            //---------------Test Result -----------------------
            Assert.IsNotNull(locator, "Cannot create new ServiceLocator object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenNullServiceName_ShouldThrowExpection()
        {
            //---------------Set up test pack-------------------
            ServiceLocator locator = new ServiceLocator();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                locator.FindService(null, Guid.NewGuid());
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
                var isValidExc = e is InvalidDataException;
                Assert.IsTrue(isValidExc);
                Assert.AreEqual(ErrorResource.ServiceIsNull, e.Message);
            }
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenEmptyServiceName_ShouldThrowExpection()
        {
            //---------------Set up test pack-------------------
            ServiceLocator locator = new ServiceLocator();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                locator.FindService("", Guid.NewGuid());
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
                var isValidExc = e is InvalidDataException;
                Assert.IsTrue(isValidExc);
                Assert.AreEqual(ErrorResource.ServiceIsNull, e.Message);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenServiceName_ShouldReturnsCorreclty()
        {
            //---------------Set up test pack-------------------
            //GetDynamicObjects<DynamicService>(workspaceID, serviceName).FirstOrDefault();
            var recCat = new Mock<IResourceCatalog>();
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service", false)).Returns(new List<DynamicService>() {new DynamicService()});
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = locator.FindService("service", Guid.Empty);
                recCat.Verify(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service", false));
                Assert.IsNotNull(dynamicService);
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
               Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenNullserviceID_ShouldThrowExpection_serviceID()
        {
            //---------------Set up test pack-------------------
            ServiceLocator locator = new ServiceLocator();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                locator.FindService(Guid.Empty, Guid.NewGuid());
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
                var isValidExc = e is InvalidDataException;
                Assert.IsTrue(isValidExc);
                Assert.AreEqual(ErrorResource.ServiceIsNull, e.Message);
            }
        }
      

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenServiceName_ShouldReturnsCorreclty_serviceID()
        {
            //---------------Set up test pack-------------------
            //GetDynamicObjects<DynamicService>(workspaceID, serviceName).FirstOrDefault();
            var recCat = new Mock<IResourceCatalog>();
            var newGuid = Guid.NewGuid();
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, newGuid)).Returns(new List<DynamicService>() {new DynamicService()});
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = locator.FindService(newGuid, Guid.Empty);
                recCat.Verify(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, newGuid));
                Assert.IsNotNull(dynamicService);
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
               Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindSourceByName_GivenNullServicename_ShouldThrowExpection()
        {
            //---------------Set up test pack-------------------
            ServiceLocator locator = new ServiceLocator();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                locator.FindSourceByName("", Guid.NewGuid());
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
                var isValidExc = e is InvalidDataException;
                Assert.IsTrue(isValidExc);
                Assert.AreEqual(ErrorResource.ServiceIsNull, e.Message);
            }
        }
      

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindSourceByName_GivenServiceName_ShouldReturnsCorreclty_serviceID()
        {
            //---------------Set up test pack-------------------
            //GetDynamicObjects<DynamicService>(workspaceID, serviceName).FirstOrDefault();
            var recCat = new Mock<IResourceCatalog>();
            const string resourceName = "SourceName";
            recCat.Setup(catalog => catalog.GetDynamicObjects<Source>(Guid.Empty, resourceName,false)).Returns(new List<Source>() {new Source()  });
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = locator.FindSourceByName(resourceName, Guid.Empty);
                recCat.Verify(catalog => catalog.GetDynamicObjects<Source>(Guid.Empty, resourceName,false));
                Assert.IsNotNull(dynamicService);
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
               Assert.Fail(e.Message);
            }
        }
    }
}
