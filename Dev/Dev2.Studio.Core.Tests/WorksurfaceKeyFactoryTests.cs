using System;
using Dev2.Factory;
using Dev2.Studio.Core.AppResources.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class WorksurfaceKeyFactoryTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WorksurfaceKeyFactory_Create")]
        // ReSharper disable once InconsistentNaming
        public void WorksurfaceKeyFactory_Create_NewGuid_Expected()
        {
            //------------Setup for test--------------------------
            var worksurfaceKey =  WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DbSource);

            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreNotEqual(worksurfaceKey.ResourceID,Guid.Empty);
        }
    }
}
