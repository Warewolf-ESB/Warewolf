/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;

namespace Dev2.Tests
{
    [TestClass]
    public class ExecutionEnvironmentUtilsTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
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
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
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
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
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
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_NoInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IWarewolfResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo {VersionNumber = "1.0"};
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            const string expectedOpenapi = "\"openapi\":\"3.0.1\"";
            const string expectedInfo = "\"info\":{\"title\":\"resourceName\",\"description\":\"resourceName\",\"version\":\"1.0\"}";
            const string expectedServers = "\"servers\":[{\"url\":\"http://servername\"}]";

            const string expectedEmptyParameters = "\"parameters\":[]";
            const string expectedEmptyResponse = "\"responses\":{\"200\":{\"description\":\"Success\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"object\",\"properties\":{}}}}}}}}}}";

            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList></DataList>", "http://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService, expectedOpenapi);
            StringAssert.Contains(swaggerOutputForService, expectedInfo);
            StringAssert.Contains(swaggerOutputForService, expectedServers);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_ScalarInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IWarewolfResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo {VersionNumber = "1.0"};
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            const string expectedOpenapi = "\"openapi\":\"3.0.1\"";
            const string expectedInfo = "\"info\":{\"title\":\"resourceName\",\"description\":\"resourceName\",\"version\":\"1.0\"}";
            const string expectedServers = "\"servers\":[{\"url\":\"https://servername\"}]";
            const string expectedParameters = "\"parameters\":[{\"name\":\"Name\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"string\"}}]";
            const string expectedEmptyResponse = "\"responses\":{\"200\":{\"description\":\"Success\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"object\",\"properties\":{}}}}}}}}}}";

            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></DataList>", "https://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService, expectedOpenapi);
            StringAssert.Contains(swaggerOutputForService, expectedInfo);
            StringAssert.Contains(swaggerOutputForService, expectedServers);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_RecordSetInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IWarewolfResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo {VersionNumber = "1.0"};
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);
            const string expectedOpenapi = "\"openapi\":\"3.0.1\"";
            const string expectedInfo = "\"info\":{\"title\":\"resourceName\",\"description\":\"resourceName\",\"version\":\"1.0\"}";
            const string expectedServers = "\"servers\":[{\"url\":\"http://servername\"}]";
            const string expectedParameters = "\"parameters\":[{\"name\":\"rc\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"string\"}}}}]";
            const string expectedEmptyResponse = "\"responses\":{\"200\":{\"description\":\"Success\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"object\",\"properties\":{}}}}}}}}}}";

            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList> <rc Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"><test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></rc></DataList>", "http://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService, expectedOpenapi);
            StringAssert.Contains(swaggerOutputForService, expectedInfo);
            StringAssert.Contains(swaggerOutputForService, expectedServers);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetSwaggerOutputForService_RecordSetInputsScalarInputsNoOutputs_ValidSwaggerDefinition()
        {
            //------------Setup for test--------------------------
            var mockResource = new Mock<IWarewolfResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo {VersionNumber = "1.0"};
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);

            const string expectedOpenapi = "\"openapi\":\"3.0.1\"";
            const string expectedInfo = "\"info\":{\"title\":\"resourceName\",\"description\":\"resourceName\",\"version\":\"1.0\"}";
            const string expectedServers = "\"servers\":[{\"url\":\"https://servername\"}]";
            const string expectedParameters = "\"parameters\":[{\"name\":\"Name\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"string\"}},{\"name\":\"rc\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"string\"}}}}]";
            const string expectedEmptyResponse = "\"responses\":{\"200\":{\"description\":\"Success\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"object\",\"properties\":{}}}}}}}}}}";
            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /> <rc Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"><test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></rc></DataList>", "https://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService, expectedOpenapi);
            StringAssert.Contains(swaggerOutputForService, expectedInfo);
            StringAssert.Contains(swaggerOutputForService, expectedServers);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedEmptyResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment_WhenHasRecordset_ShouldNotError()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[rec().a]]", "1", 0);
            dataObj.Environment.Assign("[[rec().a]]", "2", 0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual, "<DataList>");
            StringAssert.Contains(actual, "<rec>");
            StringAssert.Contains(actual, "<a>1</a>");
            StringAssert.Contains(actual, "<a>2</a>");
            StringAssert.Contains(actual, "</rec>");
            StringAssert.Contains(actual, "</DataList>");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
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
            dataObj.Environment.Assign("[[rec().a]]", "1", 0);
            dataObj.Environment.Assign("[[rec().b]]", "2", 0);
            dataObj.Environment.Assign("[[rec().c]]", "3", 0);
            dataObj.Environment.Assign("[[Name]]", "Bob", 0);
            dataObj.Environment.Assign("[[Surname]]", "Mary", 0);
            dataObj.Environment.Assign("[[FullName]]", "Bob Mary", 0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual, "<DataList>");
            StringAssert.Contains(actual, "<rec>");
            StringAssert.Contains(actual, "<a>1</a>");
            StringAssert.Contains(actual, "<a></a>");
            StringAssert.Contains(actual, "</rec>");
            StringAssert.Contains(actual, "</DataList>");
            StringAssert.Contains(actual, "<FullName>Bob Mary</FullName>");
            Assert.IsFalse(actual.Contains("<Name>Bob</Name>"));
            Assert.IsFalse(actual.Contains("<Surname>Mary</Surname>"));
            Assert.IsFalse(actual.Contains("<b>2</b>"));
            Assert.IsFalse(actual.Contains("<c>3</c>"));
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetJsonOutputFromEnvironment_WhenDataList_ShouldOnlyHaveVariablesMarkedAsOutputInString()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"/>" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"/>" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Age Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Salary Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<rec Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<b Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                                    "<c Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                                    "</rec>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[rec().a]]", "1", 0);
            dataObj.Environment.Assign("[[rec().b]]", "2", 0);
            dataObj.Environment.Assign("[[rec().c]]", "3", 0);
            dataObj.Environment.Assign("[[Name]]", "Bob", 0);
            dataObj.Environment.Assign("[[Surname]]", "Mary", 0);
            dataObj.Environment.Assign("[[FullName]]", "Bob Mary", 0);
            dataObj.Environment.Assign("[[Age]]", "15", 0);
            dataObj.Environment.Assign("[[Salary]]", "1550.55", 0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual, "rec");
            StringAssert.Contains(actual, "\"a\": 1");
            StringAssert.Contains(actual, "\"a\": \"\"");
            StringAssert.Contains(actual, "\"a\": \"\"");
            StringAssert.Contains(actual, "\"FullName\": \"Bob Mary\"");
            StringAssert.Contains(actual, "\"Age\": 15");
            StringAssert.Contains(actual, "\"Salary\": 1550.55");
            Assert.IsFalse(actual.Contains("\"Name\": \"Bob\""));
            Assert.IsFalse(actual.Contains("\"Surname\": \"Mary\""));
            Assert.IsFalse(actual.Contains("\"b\": \"2\""));
            Assert.IsFalse(actual.Contains("\"c\": \"3\""));
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetJsonOutputFromEnvironment_WhenDataList_WithScalarOutputVariable_ShouldReturnStringWithCorrectDatatype()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Age Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "<Salary Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"/>" +
                                    "</DataList>";

            dataObj.Environment.Assign("[[Name]]", "Bob", 0);
            dataObj.Environment.Assign("[[Surname]]", "Mary", 0);
            dataObj.Environment.Assign("[[FullName]]", "Bob Mary", 0);
            dataObj.Environment.Assign("[[Age]]", "15", 0);
            dataObj.Environment.Assign("[[Salary]]", "1550.55", 0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual, "\"Name\": \"Bob\""); //String datatype
            StringAssert.Contains(actual, "\"Surname\": \"Mary\"");
            StringAssert.Contains(actual, "\"FullName\": \"Bob Mary\"");
            StringAssert.Contains(actual, "\"Age\": 15"); //Int datatype
            StringAssert.Contains(actual, "\"Salary\": 1550.55"); //Float datatype
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetJsonOutputFromEnvironment_WhenDataList_WithRecordsetOutputVariable_ShouldReturnStringWithCorrectDatatype()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<User Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                                    "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<FullName Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<Age Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "<Salary Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                                    "</User>" +
                                    "</DataList>";
            dataObj.Environment.Assign("[[User().Name]]", "Bob", 0);
            dataObj.Environment.Assign("[[User().Surname]]", "Mary", 0);
            dataObj.Environment.Assign("[[User().FullName]]", "Bob Mary", 0);
            dataObj.Environment.Assign("[[User().Age]]", "15", 0);
            dataObj.Environment.Assign("[[User().Salary]]", "1550.55", 0);
            //------------Execute Test---------------------------
            var actual = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual, "\"Name\": \"Bob\""); //String datatype
            StringAssert.Contains(actual, "\"Surname\": \"Mary\"");
            StringAssert.Contains(actual, "\"FullName\": \"Bob Mary\"");
            StringAssert.Contains(actual, "\"Age\": 15"); //Int datatype
            StringAssert.Contains(actual, "\"Salary\": 1550.55"); //Float datatype
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetJsonForEnvironmentWithColumnIoDirection_WhenEmptyDataList_ShouldReturnEmptyJson()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList></DataList>";
            //------------Execute Test---------------------------
            var outPutJson = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("{}", outPutJson);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment_WhenEmptyDataList_ShouldReturnEmptyXml()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList></DataList>";
            //------------Execute Test---------------------------
            var outPutJson = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObj, dataList, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("<DataList />", outPutJson);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetRecordsetOutputFromEnvironment_ScalarInput_ShouldReturnRecordsetOutput()
        {
            var mockResource = new Mock<IWarewolfResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo {VersionNumber = "1.0"};
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);

            const string expectedOpenapi = "\"openapi\":\"3.0.1\"";
            const string expectedInfo = "\"info\":{\"title\":\"resourceName\",\"description\":\"resourceName\",\"version\":\"1.0\"}";
            const string expectedServers = "\"servers\":[{\"url\":\"http://servername\"}]";
            const string expectedParameters = "\"parameters\":[{\"name\":\"Name\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"string\"}}]";
            const string expectedRecordsetResponse = "\"responses\":{\"200\":{\"description\":\"Success\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"object\",\"properties\":{\"rc\":{\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"string\"}}}}}}}}}}}}}";

            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList> <Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /> <rc Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"><test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></rc></DataList>", "http://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            StringAssert.Contains(swaggerOutputForService, expectedOpenapi);
            StringAssert.Contains(swaggerOutputForService, expectedInfo);
            StringAssert.Contains(swaggerOutputForService, expectedServers);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedRecordsetResponse);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ExecutionEnvironmentUtils))]
        public void ExecutionEnvironmentUtils_GetScalarOutputFromEnvironment_ScalarInput_ShouldReturnScalarOutput()
        {
            var mockResource = new Mock<IWarewolfResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns("resourceName");
            var versionInfo = new VersionInfo {VersionNumber = "1.0"};
            mockResource.Setup(resource => resource.VersionInfo).Returns(versionInfo);

            const string expectedOpenapi = "\"openapi\":\"3.0.1\"";
            const string expectedInfo = "\"info\":{\"title\":\"resourceName\",\"description\":\"resourceName\",\"version\":\"1.0\"}";
            const string expectedServers = "\"servers\":[{\"url\":\"http://servername\"}]";
            const string expectedParameters = "\"parameters\":[{\"name\":\"Name\",\"in\":\"query\",\"required\":true,\"schema\":{\"type\":\"string\"}}]";
            const string expectedRecordsetResponse = "\"responses\":{\"200\":{\"description\":\"Success\",\"content\":{\"application/json\":{\"schema\":{\"type\":\"object\",\"properties\":{\"Surname\":{\"type\":\"string\"}}}}}}}}}}}";

            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList> <Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /> <Surname Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></DataList>", "http://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            StringAssert.Contains(swaggerOutputForService, expectedOpenapi);
            StringAssert.Contains(swaggerOutputForService, expectedInfo);
            StringAssert.Contains(swaggerOutputForService, expectedServers);
            StringAssert.Contains(swaggerOutputForService, expectedParameters);
            StringAssert.Contains(swaggerOutputForService, expectedRecordsetResponse);
        }

        [TestMethod]
        public void ExecutionEnvironmentUtils_UpdateEnvironmentFromInputPayload_WithXmlChar_ShouldStillMapWhenInputAsJson()
        {
            var dataObj = new DsfDataObject(string.Empty, Guid.NewGuid());
            const string dataList = "<DataList>" +
                                    "<input Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"/>" +
                                    "</DataList>";
            var inputPayload = new StringBuilder("{\"input\":\"123<1234\"}");
            ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(dataObj, inputPayload, dataList);
            Assert.IsNotNull(dataObj.Environment);
            var values = dataObj.Environment.EvalAsListOfStrings("[[input]]", 0);
            Assert.IsNotNull(values);
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("123<1234", values[0]);
        }
    }
}