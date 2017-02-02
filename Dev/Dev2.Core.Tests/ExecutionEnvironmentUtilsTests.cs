using System;
using Dev2.Common.Interfaces.Data;
using Dev2.DynamicServices;
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

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment")]
        public void ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment_WhenHasRecordset_ShouldNotError()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[rec().a]]","1",0);
            dataObj.Environment.Assign("[[rec().a]]","2",0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual,"<DataList>");
            StringAssert.Contains(actual,"<rec>");
            StringAssert.Contains(actual,"<a>1</a>");
            StringAssert.Contains(actual,"<a>2</a>");
            StringAssert.Contains(actual,"</rec>");
            StringAssert.Contains(actual,"</DataList>");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment")]
        public void ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment_WhenDataList_ShouldOnlyHaveVariablesMarkedAsOutputInString()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"/>" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"/>" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<b Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<c Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[rec().a]]","1",0);
            dataObj.Environment.Assign("[[rec().b]]","2",0);
            dataObj.Environment.Assign("[[rec().c]]","3",0);
            dataObj.Environment.Assign("[[Name]]","Bob",0);
            dataObj.Environment.Assign("[[Surname]]","Mary",0);
            dataObj.Environment.Assign("[[FullName]]","Bob Mary",0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual,"<DataList>");
            StringAssert.Contains(actual,"<rec>");
            StringAssert.Contains(actual,"<a>1</a>");
            StringAssert.Contains(actual,"<a></a>");
            StringAssert.Contains(actual,"</rec>");
            StringAssert.Contains(actual,"</DataList>");
            StringAssert.Contains(actual,"<FullName>Bob Mary</FullName>");
            Assert.IsFalse(actual.Contains("<Name>Bob</Name>"));
            Assert.IsFalse(actual.Contains("<Surname>Mary</Surname>"));
            Assert.IsFalse(actual.Contains("<b>2</b>"));
            Assert.IsFalse(actual.Contains("<c>3</c>"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment")]
        public void ExecutionEnvironmentUtils_GetJsonOutputFromEnvironment_WhenDataList_ShouldOnlyHaveVariablesMarkedAsOutputInString()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"/>" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"/>" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<b Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<c Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[rec().a]]","1",0);
            dataObj.Environment.Assign("[[rec().b]]","2",0);
            dataObj.Environment.Assign("[[rec().c]]","3",0);
            dataObj.Environment.Assign("[[Name]]","Bob",0);
            dataObj.Environment.Assign("[[Surname]]","Mary",0);
            dataObj.Environment.Assign("[[FullName]]","Bob Mary",0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual,"rec");
            StringAssert.Contains(actual,"\"a\": \"1\"");
            StringAssert.Contains(actual,"\"a\": \"\"");
            StringAssert.Contains(actual,"\"a\": \"\"");
            StringAssert.Contains(actual,"\"FullName\": \"Bob Mary\"");
            Assert.IsFalse(actual.Contains("\"Name\": \"Bob\""));
            Assert.IsFalse(actual.Contains("\"Surname\": \"Mary\""));
            Assert.IsFalse(actual.Contains("\"b\": \"2\""));
            Assert.IsFalse(actual.Contains("\"c\": \"3\""));
        }
    }
}
