using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;



namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    [TestCategory("Runtime ESB")]
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
            var locator = new ServiceLocator();
            //---------------Test Result -----------------------
            Assert.IsNotNull(locator, "Cannot create new ServiceLocator object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenNullServiceName_ShouldThrowExpection()
        {
            //---------------Set up test pack-------------------
            var locator = new ServiceLocator();
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
            var locator = new ServiceLocator();
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
        [Owner("Hagashen Naidu")]
        public void FindService_GivenServiceName_ReturnsNull_ShouldUpdatePerfCounter()
        {
            //---------------Set up test pack-------------------
            var mockWarewolfPerformanceCounterLocater = new Mock<IWarewolfPerformanceCounterLocater>();
            var mockPerformanceCounter = new Mock<IPerformanceCounter>();
            mockPerformanceCounter.Setup(counter => counter.Increment()).Verifiable();
            mockWarewolfPerformanceCounterLocater.Setup(locater => locater.GetCounter(It.IsAny<string>())).Returns(mockPerformanceCounter.Object);
            CustomContainer.Register(mockWarewolfPerformanceCounterLocater.Object);
            var recCat = new Mock<IResourceCatalog>();
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service")).Returns(new List<DynamicService> { null });
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            var dynamicService = locator.FindService("service", Guid.Empty);
            recCat.Verify(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service"));
            mockPerformanceCounter.Verify(counter => counter.Increment());
            Assert.IsNull(dynamicService);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenServiceName_ShouldReturnsCorreclty()
        {
            //---------------Set up test pack-------------------
            var recCat = new Mock<IResourceCatalog>();
            var service = new DynamicService();
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service")).Returns(new List<DynamicService> {service});
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = locator.FindService("service", Guid.Empty);
                recCat.Verify(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service"));
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
        [DoNotParallelize]
        public void FindService_GivenServiceName_InCache_ShouldReturnFromCache()
        {
            //---------------Set up test pack-------------------
            var recCat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            res.Setup(resource => resource.ResourceID).Returns(resourceId);
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service")).Returns(new List<DynamicService>() { new DynamicService() });
            recCat.Setup(catalog => catalog.GetResource(Guid.Empty, "service")).Returns(res.Object);
            ServiceActionRepo.Instance.AddToCache(resourceId, new DynamicService());
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            var dynamicService = locator.FindService("service", Guid.Empty);
            recCat.Verify(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, "service"),Times.Never());
            Assert.IsNotNull(dynamicService);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void FindService_GivenServiceID_InCache_ShouldReturnFromCache()
        {
            //---------------Set up test pack-------------------
            var recCat = new Mock<IResourceCatalog>();
            var res = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            res.Setup(resource => resource.ResourceID).Returns(resourceId);
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, resourceId)).Returns(new List<DynamicService>() { new DynamicService() });
            ServiceActionRepo.Instance.AddToCache(resourceId,new DynamicService());
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = locator.FindService(resourceId, Guid.Empty);
                recCat.Verify(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty,resourceId),Times.Never());
                Assert.IsNotNull(dynamicService);
            }
            catch (Exception e)
            {
                //---------------Test Result -----------------------
                Assert.Fail(e.Message);
            }
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void FindService_GivenResourceID_ReturnsNull_ShouldUpdatePerfCounter()
        {
            //---------------Set up test pack-------------------
            var mockWarewolfPerformanceCounterLocater = new Mock<IWarewolfPerformanceCounterLocater>();
            var mockPerformanceCounter = new Mock<IPerformanceCounter>();
            var resourceId = Guid.NewGuid();
            mockPerformanceCounter.Setup(counter => counter.Increment()).Verifiable();
            mockWarewolfPerformanceCounterLocater.Setup(locater => locater.GetCounter(It.IsAny<string>())).Returns(mockPerformanceCounter.Object);
            CustomContainer.Register(mockWarewolfPerformanceCounterLocater.Object);
            var recCat = new Mock<IResourceCatalog>();
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, resourceId)).Returns(new List<DynamicService> { null });
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = locator.FindService(resourceId, Guid.Empty);
                recCat.Verify(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, resourceId));
                mockPerformanceCounter.Verify(counter => counter.Increment());
                Assert.IsNull(dynamicService);
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
            var locator = new ServiceLocator();
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
            var recCat = new Mock<IResourceCatalog>();
            var newGuid = Guid.NewGuid();
            var service = new DynamicService();
            var serviceAction = new ServiceAction();
            service.Actions = new List<ServiceAction> { serviceAction };
            recCat.Setup(catalog => catalog.GetDynamicObjects<DynamicService>(Guid.Empty, newGuid)).Returns(new List<DynamicService> {service});
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
                Assert.AreEqual(newGuid,serviceAction.ServiceID);
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
            var locator = new ServiceLocator();
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
            var recCat = new Mock<IResourceCatalog>();
            const string resourceName = "SourceName";
            recCat.Setup(catalog => catalog.GetDynamicObjects<Source>(Guid.Empty, resourceName)).Returns(new List<Source>() {new Source()  });
            var locator = new ServiceLocator();
            var privateObject = new PrivateObject(locator);
            privateObject.SetField("_resourceCatalog", recCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(locator);
            //---------------Execute Test ----------------------
            try
            {
                var dynamicService = locator.FindSourceByName(resourceName, Guid.Empty);
                recCat.Verify(catalog => catalog.GetDynamicObjects<Source>(Guid.Empty, resourceName));
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
