using System;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ServiceActionRepoTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceActionRepo_AddToCache")]
        public void ServiceActionRepo_AddToCache_WhenNotExisting_ShouldAdd()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var ds = new DynamicService { DisplayName = "Ds 1" };
            //------------Execute Test---------------------------
            ServiceActionRepo.Instance.AddToCache(id, ds);
            //------------Assert Results-------------------------
            var readDs = ServiceActionRepo.Instance.ReadCache(id);
            Assert.IsNotNull(readDs);
            Assert.AreEqual("Ds 1",readDs.DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceActionRepo_AddToCache")]
        public void ServiceActionRepo_AddToCache_WhenIdExists_ShouldReplace()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var ds = new DynamicService { DisplayName = "Ds 1" };
            var ds2 = new DynamicService { DisplayName = "Ds 2" };
            //------------Execute Test---------------------------
            ServiceActionRepo.Instance.AddToCache(id, ds);
            ServiceActionRepo.Instance.AddToCache(id, ds2);
            //------------Assert Results-------------------------
            var readDs = ServiceActionRepo.Instance.ReadCache(id);
            Assert.IsNotNull(readDs);
            Assert.AreEqual("Ds 2",readDs.DisplayName);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceActionRepo_AddToCache")]
        public void ServiceActionRepo_ReadFromCache_WhenNotExisting_ShouldReturnNull()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var ds = new DynamicService { DisplayName = "Ds 1" };
            ServiceActionRepo.Instance.AddToCache(id, ds);
            //------------Execute Test---------------------------
            var readDs = ServiceActionRepo.Instance.ReadCache(Guid.NewGuid());
            //------------Assert Results-------------------------
            Assert.IsNull(readDs);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceActionRepo_AddToCache")]
        public void ServiceActionRepo_ReadFromCache_WhenExisting_ShouldReturnDynamicService()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var ds = new DynamicService { DisplayName = "Ds 1" };
            ServiceActionRepo.Instance.AddToCache(id, ds);
            //------------Execute Test---------------------------
            var readDs = ServiceActionRepo.Instance.ReadCache(id);
            //------------Assert Results-------------------------
            Assert.IsNotNull(readDs);
            Assert.AreEqual("Ds 1", readDs.DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceActionRepo_RemoveFromCache")]
        public void ServiceActionRepo_RemoveFromCache_WhenIdExists_ShouldRemove()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var ds = new DynamicService { DisplayName = "Ds 1" };
            var ds2 = new DynamicService { DisplayName = "Ds 2" };
            ServiceActionRepo.Instance.AddToCache(id, ds);
            ServiceActionRepo.Instance.AddToCache(id2, ds2);
            //------------Execute Test---------------------------
            ServiceActionRepo.Instance.RemoveFromCache(id);
            //------------Assert Results-------------------------
            var readDs = ServiceActionRepo.Instance.ReadCache(id);
            Assert.IsNull(readDs);
            var readDs2 = ServiceActionRepo.Instance.ReadCache(id2);
            Assert.IsNull(readDs);
            Assert.AreEqual("Ds 2", readDs2.DisplayName);
        }
    }
}
