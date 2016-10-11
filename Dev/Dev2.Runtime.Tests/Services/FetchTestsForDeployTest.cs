using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchTestsForDeployTest
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("FetchTests_HandlesType")]
        public void FetchTests_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var fetchTests = new FetchTestsForDeploy();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("FetchTestsForDeploy", fetchTests.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var fetchTests = new FetchTestsForDeploy();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = fetchTests.Execute(null, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var fetchTests = new FetchTestsForDeploy();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = fetchTests.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder("ABCDE") } };
            var fetchTests = new FetchTestsForDeploy();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = fetchTests.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }
        

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_ExpectTestList()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IAuthorizer>();
            mock.Setup(authorizer => authorizer.RunPermissions(It.IsAny<Guid>()));
            var fetchTests = new FetchTestsForDeploy(mock.Object);

            var listOfTests = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF"
                }
            };
            var repo = new Mock<ITestCatalog>();
            var ws = new Mock<IWorkspace>();
            var resID = Guid.Empty;
            repo.Setup(a => a.Fetch(It.IsAny<Guid>())).Callback((Guid id)=>
            {
                resID = id;
            }).Returns(listOfTests).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));            
            fetchTests.TestCatalog = repo.Object;
            //------------Execute Test---------------------------
            var res = fetchTests.Execute(inputs, ws.Object);
            var msg = serializer.Deserialize<CompressedExecuteMessage>(res);
            var testModels = serializer.Deserialize<List<IServiceTestModelTO>>(msg.GetDecompressedMessage());
            //------------Assert Results-------------------------
            repo.Verify(a => a.Fetch(It.IsAny<Guid>()));
            Assert.AreEqual(listOfTests.Count,testModels.Count);
            Assert.AreEqual(listOfTests[0].TestName,testModels[0].TestName);
            Assert.AreEqual(resourceID,resID);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_ExecuteExpect_ExpectTestListWithEncryptedPassword()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IAuthorizer>();
            mock.Setup(authorizer => authorizer.RunPermissions(It.IsAny<Guid>()));
            var fetchTests = new FetchTestsForDeploy(mock.Object);

            const string password = "nathi";
            var listOfTests = new List<IServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF",
                    Password = password
                }
            };
            var repo = new Mock<ITestCatalog>();
            var ws = new Mock<IWorkspace>();
            var resID = Guid.Empty;
            repo.Setup(a => a.Fetch(It.IsAny<Guid>())).Callback((Guid id)=>
            {
                resID = id;
            }).Returns(listOfTests).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));            
            fetchTests.TestCatalog = repo.Object;
            //------------Execute Test---------------------------
            var res = fetchTests.Execute(inputs, ws.Object);
            var msg = serializer.Deserialize<CompressedExecuteMessage>(res);
            var testModels = serializer.Deserialize<List<IServiceTestModelTO>>(msg.GetDecompressedMessage());
            //------------Assert Results-------------------------
            repo.Verify(a => a.Fetch(It.IsAny<Guid>()));
            Assert.AreEqual(listOfTests.Count,testModels.Count);
            Assert.AreEqual(listOfTests[0].TestName,testModels[0].TestName);
            Assert.AreEqual(listOfTests[0].TestName,testModels[0].TestName);
            var encrypt = SecurityEncryption.Encrypt(password);
            Assert.AreEqual(listOfTests[0].Password, encrypt);
            Assert.AreEqual(resourceID,resID);
        }        
    }
}