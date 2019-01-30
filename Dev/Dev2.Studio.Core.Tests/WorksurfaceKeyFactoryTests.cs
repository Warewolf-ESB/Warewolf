using System;
using Dev2.Factory;
using Dev2.Studio.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class WorksurfaceKeyFactoryTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WorksurfaceKeyFactory_Create_SqlServerSource")]

        public void WorksurfaceKeyFactory_Create_SqlServerSource_NewGuid_Expected()
        {
            //------------Setup for test--------------------------
            var worksurfaceKey =  WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SqlServerSource);

            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreNotEqual(worksurfaceKey.ResourceID,Guid.Empty);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WorksurfaceKeyFactory_Create_MySqlSource")]

        public void WorksurfaceKeyFactory_Create_MySqlSource_NewGuid_Expected()
        {
            //------------Setup for test--------------------------
            var worksurfaceKey =  WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MySqlSource);

            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreNotEqual(worksurfaceKey.ResourceID,Guid.Empty);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WorksurfaceKeyFactory_Create_PostgreSqlSource")]

        public void WorksurfaceKeyFactory_Create_PostgreSqlSource_NewGuid_Expected()
        {
            //------------Setup for test--------------------------
            var worksurfaceKey =  WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PostgreSqlSource);

            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreNotEqual(worksurfaceKey.ResourceID,Guid.Empty);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WorksurfaceKeyFactory_Create_OracleSource")]

        public void WorksurfaceKeyFactory_Create_OracleSource_NewGuid_Expected()
        {
            //------------Setup for test--------------------------
            var worksurfaceKey =  WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OracleSource);

            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreNotEqual(worksurfaceKey.ResourceID,Guid.Empty);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WorksurfaceKeyFactory_Create_OdbcSource")]

        public void WorksurfaceKeyFactory_Create_OdbcSource_NewGuid_Expected()
        {
            //------------Setup for test--------------------------
            var worksurfaceKey =  WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OdbcSource);

            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreNotEqual(worksurfaceKey.ResourceID,Guid.Empty);
        }
    }
}
