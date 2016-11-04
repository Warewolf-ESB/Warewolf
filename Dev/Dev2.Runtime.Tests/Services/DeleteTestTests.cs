using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteTestTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var clearLog = new ClearLog();

            //------------Execute Test---------------------------
            var resId = clearLog.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var clearLog = new ClearLog();

            //------------Execute Test---------------------------
            var resId = clearLog.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteTestHandlesType")]
        public void DeleteTestHandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var deleteTest = new DeleteTest();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("DeleteTest", deleteTest.HandlesType());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteTestExecute")]
        public void DeleteTestExecute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var deleteTest = new DeleteTest();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteTest.Execute(null, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteTestExecute")]
        public void DeleteTestExecute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var deleteTest = new DeleteTest();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteTest.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteTestExecute")]
        public void DeleteTestExecute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder("ABCDE") } };
            var deleteTest = new DeleteTest();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteTest.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteTestExecute")]
        public void DeleteTestExecute_TestNameNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "somethingElse", new StringBuilder("something") } };
            var deleteTest = new DeleteTest();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteTest.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }
        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteTestExecute")]
        public void DeleteTestExecute__ValidArgs_ExpectDeleteTestCalled()
        {
            //------------Setup for test--------------------------
            var deleteTest = new DeleteTest();
          
            var repo = new Mock<ITestCatalog>();
            var ws = new Mock<IWorkspace>();
            var resID = Guid.Empty;
            var testName = "";
            repo.Setup(a => a.DeleteTest(It.IsAny<Guid>(),It.IsAny<string>())).Callback((Guid id,string name)=>
            {
                resID = id;
                testName = name;
            }).Verifiable();

            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));            
            inputs.Add("testName", new StringBuilder("TestToDelete"));            
            deleteTest.TestCatalog = repo.Object;
            //------------Execute Test---------------------------
            deleteTest.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.DeleteTest(It.IsAny<Guid>(),It.IsAny<string>()));
            Assert.AreEqual(resourceID,resID);
            Assert.AreEqual("TestToDelete",testName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteTestExecute")]
        public void DeleteTestExecute__ValidArgsPermitted_ExpectDeleteTestCalled()
        {
            //------------Setup for test--------------------------
            var deleteTest = new DeleteTest();
          
            var repo = new Mock<ITestCatalog>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.DeleteTest(It.IsAny<Guid>(), It.IsAny<string>()));
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));            
            inputs.Add("testName", new StringBuilder("TestToDelete"));            
            deleteTest.TestCatalog = repo.Object;
            //------------Execute Test---------------------------
            deleteTest.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.DeleteTest(It.IsAny<Guid>(),It.IsAny<string>()), Times.Once);

        }        
    }
}