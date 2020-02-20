using System;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


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
            StringAssert.Contains(swaggerOutputForService, expectedSwaggerVersion);
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
                                                            "\"name\":\"Name\"," +
                                                            "\"in\":\"query\"," +
                                                            "\"required\":true," +
                                                            "\"type\":\"string\"" +
                                                   "}]";
            const string expectedEmptyResponse = "\"200\":{\"schema\":{\"$ref\":\"#/definition/Output\"}}";

            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></DataList>", "https://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService, expectedSwaggerVersion);
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
                                                            "\"name\":\"DataList\"," +
                                                            "\"in\":\"query\"," +
                                                            "\"required\":true," +
                                                            "\"schema\":{\"$ref\":\"#/definitions/DataList\"}" +
                                                   "}]";
            const string expectedEmptyResponse = "\"200\":{\"schema\":{\"$ref\":\"#/definition/Output\"}}";
            const string expectedDataListDefinition = "\"DataList\":{\"type\":\"object\",\"properties\":{\"rc\":{\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"string\"}}";
            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList> <rc Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"><test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></rc></DataList>", "http://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService, expectedSwaggerVersion);
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
                                                            "\"name\":\"DataList\"," +
                                                            "\"in\":\"query\"," +
                                                            "\"required\":true," +
                                                            "\"schema\":{\"$ref\":\"#/definitions/DataList\"}" +
                                                   "}]";
            const string expectedEmptyResponse = "\"200\":{\"schema\":{\"$ref\":\"#/definition/Output\"}}";
            const string expectedDataListDefinition = "\"DataList\":{\"type\":\"object\",\"properties\":{\"Name\":{\"type\":\"string\"},\"rc\":{\"type\":\"object\",\"properties\":{\"test\":{\"type\":\"string\"}}";
            //------------Execute Test---------------------------
            var swaggerOutputForService = ExecutionEnvironmentUtils.GetSwaggerOutputForService(mockResource.Object, "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /> <rc Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\"><test Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></rc></DataList>", "https://serverName:3142/public/resourceName.api").Replace(Environment.NewLine, "").Replace(" ", "");
            //------------Assert Results-------------------------
            StringAssert.Contains(swaggerOutputForService, expectedSwaggerVersion);
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
            StringAssert.Contains(actual, "<a />"); //When WarewolfAtom Result is null then return null instead of empty string
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
        [TestCategory("ExecutionEnvironmentUtils_GetJsonOutputFromEnvironment")]
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
            StringAssert.Contains(actual, "\"a\": null"); //When WarewolfAtom Result is null then return null instead of empty string
            StringAssert.Contains(actual, "\"a\": null"); //When WarewolfAtom Result is null then return null instead of empty string
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
        [TestCategory("ExecutionEnvironmentUtils_GetJsonOutputFromEnvironment")]
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
            StringAssert.Contains(actual, "\"Name\": \"Bob\"");//String datatype
            StringAssert.Contains(actual, "\"Surname\": \"Mary\"");
            StringAssert.Contains(actual, "\"FullName\": \"Bob Mary\"");
            StringAssert.Contains(actual, "\"Age\": 15"); //Int datatype
            StringAssert.Contains(actual, "\"Salary\": 1550.55"); //Float datatype
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory("ExecutionEnvironmentUtils_GetJsonOutputFromEnvironment")]
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
            StringAssert.Contains(actual, "\"Name\": \"Bob\"");//String datatype
            StringAssert.Contains(actual, "\"Surname\": \"Mary\"");
            StringAssert.Contains(actual, "\"FullName\": \"Bob Mary\"");
            StringAssert.Contains(actual, "\"Age\": 15"); //Int datatype
            StringAssert.Contains(actual, "\"Salary\": 1550.55"); //Float datatype
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ExecutionEnvironmentUtils_GetJsonForEnvironmentWithColumnIoDirection")]
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
        [TestCategory("ExecutionEnvironmentUtils_GetXmlOutputFromEnvironment")]
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
