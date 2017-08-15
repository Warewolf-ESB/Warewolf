using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchTestsTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchTests = new FetchTests();

            //------------Execute Test---------------------------
            var resId = fetchTests.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchTests = new FetchTests();

            //------------Execute Test---------------------------
            var resId = fetchTests.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchTests_HandlesType")]
        public void FetchTests_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var fetchTests = new FetchTests();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("FetchTests", fetchTests.HandlesType());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var fetchTests = new FetchTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = fetchTests.Execute(null, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var fetchTests = new FetchTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = fetchTests.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder("ABCDE") } };
            var fetchTests = new FetchTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = fetchTests.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }
        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchTests_Execute")]
        public void FetchTests_Execute_ExpectTestList()
        {
            //------------Setup for test--------------------------
            var fetchTests = new FetchTests();

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
    }
}