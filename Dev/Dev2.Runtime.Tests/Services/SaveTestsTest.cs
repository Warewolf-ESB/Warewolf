using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveTestsTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveTests_HandlesType")]
        public void SaveTests_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveTests = new SaveTests();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("SaveTests", saveTests.HandlesType());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveTests_Execute")]
        public void SaveTests_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveTests = new SaveTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = saveTests.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveTests_Execute")]
        public void SaveTests_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var saveTests = new SaveTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = saveTests.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveTests_Execute")]
        public void SaveTests_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder("ABCDE") } };
            var saveTests = new SaveTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = saveTests.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveTests_Execute")]
        public void SaveTests_Execute_TestDefinitionsNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) } };
            var saveTests = new SaveTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = saveTests.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveTests_Execute")]
        public void SaveTests_Execute_ItemToDeleteNotListOfServiceTestTO_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "testDefinitions", new StringBuilder("This is not deserializable to ServerExplorerItem") } };
            var saveTests = new SaveTests();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = saveTests.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SaveTests_Execute")]
        public void SaveTests_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveTests = new SaveTests();

            var listOfTests = new List<ServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF"
                }
            };
            var testModels = new List<ServiceTestModelTO>();
            var repo = new Mock<ITestCatalog>();
            var ws = new Mock<IWorkspace>();
            var resID = Guid.Empty;
            repo.Setup(a => a.SaveTests(It.IsAny<Guid>(), It.IsAny<List<ServiceTestModelTO>>())).Callback((Guid id,List<ServiceTestModelTO> testModelTos)=>
            {
                resID = id;
                testModels = testModelTos;
            }).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(listOfTests));
            inputs.Add("testDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));            
            saveTests.TestCatalog = repo.Object;
            //------------Execute Test---------------------------
            saveTests.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.SaveTests(It.IsAny<Guid>(), It.IsAny<List<ServiceTestModelTO>>()));
            Assert.AreEqual(listOfTests.Count,testModels.Count);
            Assert.AreEqual(listOfTests[0].TestName,testModels[0].TestName);
            Assert.AreEqual(resourceID,resID);
        }        
    }
}