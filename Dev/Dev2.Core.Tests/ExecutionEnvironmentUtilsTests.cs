using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests
{
    [TestClass]
    public class ExecutionEnvironmentUtilsTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetSwaggerOutputForService")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_NullServiceName_ExpectedException()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            ExecutionEnvironmentUtils.GetSwaggerOutputForService(null, "", "");
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetSwaggerOutputForService")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_EmptyDataList_ExpectedException()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            ExecutionEnvironmentUtils.GetSwaggerOutputForService(new Mock<IResource>().Object, "", "");
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetSwaggerOutputForService")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_NullDataList_ExpectedException()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            ExecutionEnvironmentUtils.GetSwaggerOutputForService(new Mock<IResource>().Object, null, "");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetSwaggerOutputForService")]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_NoInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo { VersionNumber = "1.0" };
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            const string expectedSwaggerVersion = "\"swagger\":2";
            const string expectedEmptyParameters = "\"parameters\":[]";
            const string expectedEmptyResponse = "\"200\":{\"schema\":{\"$ref\":\"#/definition/Output\"}}";

            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList></DataList>", "http://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService,expectedSwaggerVersion);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
            StringAssert.Contains(swaggerOutputForService, "\"schemes\":[\"http\"]");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetSwaggerOutputForService")]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_ScalarInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo { VersionNumber = "1.0" };
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            const string expectedSwaggerVersion = "\"swagger\":2";
            const string expectedParameters = "\"parameters\":[" +
                                                        "{" +
                                                            "\"name\":\"Name\","+
                                                            "\"in\":\"query\","+
                                                            "\"required\":true,"+
                                                            "\"type\":\"string\""+
                                                   "}]";
            const string expectedEmptyResponse = "\"200\":{\"schema\":{\"$ref\":\"#/definition/Output\"}}";

            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></DataList>", "https://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService,expectedSwaggerVersion);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
            StringAssert.Contains(swaggerOutputForService, "\"schemes\":[\"https\"]");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetSwaggerOutputForService")]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_RecordSetInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo { VersionNumber = "1.0" };
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            const string expectedSwaggerVersion = "\"swagger\":2";
            const string expectedParameters = "\"parameters\":[" +
                                                        "{" +
                                                            "\"name\":\"DataList\","+
                                                            "\"in\":\"query\","+
                                                            "\"required\":true,"+
                                                            "\"schema\":{\"$ref\":\"#/definitions/DataList\"}"+
                                                   "}]";
            const string expectedEmptyResponse = "\"200\":{\"schema\":{\"$ref\":\"#/definition/Output\"}}";
            const string expectedDataListDefinition = "\"DataList\":{\"type\":\"object\",\"properties\":{\"rc\":{\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"string\"}}";
            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList> <rc Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"><test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></rc></DataList>", "http://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService,expectedSwaggerVersion);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
            StringAssert.Contains(swaggerOutputForService, expectedDataListDefinition);
            StringAssert.Contains(swaggerOutputForService, "\"schemes\":[\"http\"]");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetSwaggerOutputForService")]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_RecordSetInputsScalarInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo { VersionNumber = "1.0" };
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            const string expectedSwaggerVersion = "\"swagger\":2";
            const string expectedParameters = "\"parameters\":[" +
                                                        "{" +
                                                            "\"name\":\"DataList\","+
                                                            "\"in\":\"query\","+
                                                            "\"required\":true,"+
                                                            "\"schema\":{\"$ref\":\"#/definitions/DataList\"}"+
                                                   "}]";
            const string expectedEmptyResponse = "\"200\":{\"schema\":{\"$ref\":\"#/definition/Output\"}}";
            const string expectedDataListDefinition = "\"DataList\":{\"type\":\"object\",\"properties\":{\"Name\":{\"type\":\"string\"},\"rc\":{\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"string\"}}";
            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /> <rc Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"><test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></rc></DataList>", "https://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService,expectedSwaggerVersion);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
            StringAssert.Contains(swaggerOutputForService, expectedDataListDefinition);
            StringAssert.Contains(swaggerOutputForService, "\"produces\":[\"application/json\",\"application/xml\"]");
            StringAssert.Contains(swaggerOutputForService, "\"schemes\":[\"https\"]");
        }


    }
}
