using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class ReloadTestsTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DeleteTestHandlesType")]
        public void ReloadTestsHandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var deleteTest = new ReloadTests();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("ReloadTestsService", deleteTest.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ReloadTestsExecute")]
        public void ReloadTestsExecute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var deleteTest = new ReloadTests();
            var repo = new Mock<ITestCatalog>();
            repo.Setup(catalog => catalog.Load()).Throws(new Exception());
            deleteTest.TestCatalog = repo.Object;
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteTest.Execute(null, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ReloadTestsExecute")]
        public void ReloadTestsExecute__ValidArgs_ExpectDeleteTestCalled()
        {
            //------------Setup for test--------------------------
            var deleteTest = new ReloadTests();
          
            var repo = new Mock<ITestCatalog>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.Load()).Verifiable();

       
            deleteTest.TestCatalog = repo.Object;
            //------------Execute Test---------------------------
            deleteTest.Execute(new Dictionary<string, StringBuilder>(), ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.Load(), Times.Once);
        }        
    }
}