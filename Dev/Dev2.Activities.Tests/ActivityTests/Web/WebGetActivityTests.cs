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
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.String.Json;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class WebGetActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_Execute_WithValidTextResponse_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response = "{\"Location\": \"Paris\",\"Time\": \"May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC\"," +
                                    "\"Wind\": \"from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0\"," +
                                    "\"Visibility\": \"greater than 7 mile(s):0\"," +
                                    "\"Temperature\": \"59 F (15 C)\"," +
                                    "\"DewPoint\": \"41 F (5 C)\"," +
                                    "\"RelativeHumidity\": \"51%\"," +
                                    "\"Pressure\": \"29.65 in. Hg (1004 hPa)\"," +
                                    "\"Status\": \"Success\"" +
                                    "}";
            var environment = new ExecutionEnvironment();
            environment.Assign("[[City]]", "PMB", 0);
            environment.Assign("[[CountryName]]", "South Africa", 0);
            var dsfWebGetActivity = new TestWebGetActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Response", "[[Response]]", "") };
            dsfWebGetActivity.Inputs = serviceInputs;
            dsfWebGetActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebGetActivity.OutputDescription = service.GetOutputDescription();
            dsfWebGetActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebGetActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebGetActivity.QueryString = "";

            dsfWebGetActivity.SourceId = Guid.Empty;
            dsfWebGetActivity.Headers = new List<INameValue>();
            dsfWebGetActivity.OutputDescription = new OutputDescription();
            dsfWebGetActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape() { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebGetActivity.OutputDescription);
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_Execute_ErrorResponse_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response = "{\"Message\":\"Error\"}";
            var environment = new ExecutionEnvironment();

            var dsfWebGetActivity = new TestWebGetActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };

            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Message", "[[Message]]", "") };
            dsfWebGetActivity.Outputs = serviceOutputs;

            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebGetActivity.OutputDescription = service.GetOutputDescription();
            dsfWebGetActivity.ResponseFromWeb = response;
            dsfWebGetActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebGetActivity.QueryString = "Error";

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
          
            dsfWebGetActivity.SourceId = Guid.Empty;
            dsfWebGetActivity.Headers = new List<INameValue>();
            dsfWebGetActivity.OutputDescription = new OutputDescription();
            dsfWebGetActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape() { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

            //------------Execute Test---------------------------
            dsfWebGetActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Message]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_ExecutionImpl_ErrorResultTO_ReturnErrors_ToActivity_Success()
        {
            //-----------------------Arrange-------------------------
            const string response = "{\"Message\":\"TEST Error\"}";
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebGetActivity{
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    HadErrorMessage = "Some error"
                };
                //-----------------------Act-----------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
                Assert.AreEqual("Some error", errorResultTO.FetchErrors()[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_ExecutionImpl_IsResponseBase64_True_And_IsObject_True_Json_WebGetResponse_ExpectJson()
        {
            //-----------------------Arrange-------------------------
            var jsonData = "{color: \"red\",value: \"#f00\"}";
            var jsonBytes = jsonData.Base64StringToByteArray();
            string response = jsonBytes.ToBase64String();
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebGetActivity{
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    IsResponseBase64 = false,
                    IsObject = true,
                    ObjectName = "[[@response]]"
                };
                //-----------------------Act-----------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                var actualResponseWWJson = environment.EvalForJson("[[@response]]") as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                var actualResponse = (actualResponseWWJson.Item as DataStorage.WarewolfAtom.JsonObject).Item;

                Assert.IsFalse(errorResultTO.HasErrors());
                Assert.AreEqual(JToken.Parse(jsonData).ToString(), actualResponse.ToString());
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_ExecutionImpl_IsResponseBase64_False_And_IsObject_True_Json_WebGetResponse_ExpectJsonBase64()
        {
            //-----------------------Arrange-------------------------
            var jsonData = "{color: \"red\",value: \"#f00\"}";
            var jsonBytes = jsonData.Base64StringToByteArray();
            string response = jsonBytes.ToBase64String();
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebGetActivity{
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    IsResponseBase64 = false,
                    IsObject = true,
                    ObjectName = "[[@response]]"
                };
                //-----------------------Act-----------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                var actualResponseWWJson = environment.EvalForJson("[[@response]]") as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                var actualResponse = (actualResponseWWJson.Item as DataStorage.WarewolfAtom.JsonObject).Item;

                Assert.IsFalse(errorResultTO.HasErrors());
                Assert.AreEqual(JToken.Parse(jsonData).ToString(), actualResponse.ToString());
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_ExecutionImpl_IsResponseBase64_False_And_IsObject_False_Html_WebGetResponse_ExpectHtmlString()
        {
            //-----------------------Arrange-------------------------
            const string htmlData = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n  <head>\r\n   <title>Img Align Attribute<\\/title>\r\n <\\/head>\r\n<body>\r\n  <p>This is an example. <img src=\"image.png\" alt=\"Image\" align=\"middle\"> More text right here\r\n  <img src=\"image.png\" alt=\"Image\" width=\"100\"\\/>\r\n  <\\/body>\r\n<\\/html>";
            var htmlBytes = htmlData.Base64StringToByteArray();
            string response = htmlBytes.ToBase64String();
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebGetActivity{
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    IsResponseBase64 = false,
                    Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping { MappedTo = "[[response]]" } },
                };
                //-----------------------Act-----------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                var actualResponseWWJson = environment.Eval("[[response]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                var actualResponse = (actualResponseWWJson.Item as DataStorage.WarewolfAtom.DataString).Item;

                Assert.IsFalse(errorResultTO.HasErrors());
                Assert.AreEqual(htmlData, actualResponse);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_ExecutionImpl_IsResponseBase64_True_And_IsObject_False_Html_WebGetResponse_ExpectHtmlBase64String()
        {
            //-----------------------Arrange-------------------------
            const string htmlData = @"<!DOCTYPE html>\r\n<html lang=\""en\"">\r\n  <head>\r\n   <title>Img Align Attribute<\\/title>\r\n <\\/head>\r\n<body>\r\n  <p>This is an example. <img src=\""image.png\"" alt=\""Image\"" align=\""middle\""> More text right here\r\n  <img src=\""image.png\"" alt=\""Image\"" width=\""100\""\\/>\r\n  <\\/body>\r\n<\\/html>";
            var htmlBytes = htmlData.Base64StringToByteArray();
            string response = htmlBytes.ToBase64String();
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebGetActivity
                {
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    IsResponseBase64 = true,
                    Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping { MappedTo = "[[response]]" } },
                };
                //-----------------------Act-----------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                var actualResponseWWJson = environment.Eval("[[response]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                var actualResponse = (actualResponseWWJson.Item as DataStorage.WarewolfAtom.DataString).Item;

                Assert.IsFalse(errorResultTO.HasErrors());
                Assert.AreEqual(response, actualResponse);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebGetActivity))]
        public void WebGetActivity_ExecutionImpl_ResponseManager_PushResponseIntoEnvironment_GivenJsonResponse_MappedToRecodSet_ShouldSucess()
        {
            //-----------------------Arrange-------------------------
            const string json = "{\"Messanger\":\"jSon response from the request\"}";
            var response = Convert.ToBase64String(json.ToBytesArray());
            const string mappingFrom = "mapFrom";
            const string recordSet = "recset";
            const string mapTo = "mapTo";
            const string variableNameMappingTo = "[[recset().mapTo]]";

            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebPostActivity
                {
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    IsObject = false,
                    Outputs = new List<IServiceOutputMapping>
                    {
                        {
                            new ServiceOutputMapping
                            {
                                MappedFrom = mappingFrom,
                                MappedTo = mapTo,
                                RecordSetName = recordSet
                            }
                        }
                    }
                };
                //-----------------------Act-----------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                Assert.IsFalse(errorResultTO.HasErrors());

                //assert first DataSourceShapes
                var resourceManager = dsfWebGetActivity.ResponseManager;
                var outputDescription = resourceManager.OutputDescription;
                var dataShapes = outputDescription.DataSourceShapes;
                var paths = dataShapes.First().Paths;
                Assert.IsNotNull(outputDescription);
                Assert.AreEqual("Messanger", paths.First().ActualPath);
                Assert.AreEqual("Messanger", paths.First().DisplayPath);
                Assert.AreEqual(variableNameMappingTo, paths.First().OutputExpression);
                Assert.AreEqual("jSon response from the request", paths.First().SampleData);

                //assert execution environment
                var envirVariable = environment.Eval(recordSet, 0);
                var ress = envirVariable as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                Assert.IsNotNull(envirVariable);
                Assert.IsFalse(ress.Item.IsNothing, "Item should contain the recset mapped to the messanger key");
            }
        }
    }

    public class TestWebGetActivity : WebGetActivity
    {
        public string HadErrorMessage { get; set; }

        public string ResponseFromWeb { private get; set; }

        protected override string PerformWebRequest(IEnumerable<INameValue> head, string query, WebSource url)
        {
            if (!string.IsNullOrWhiteSpace(HadErrorMessage))
            {
                base._errorsTo = new ErrorResultTO();
                base._errorsTo.AddError(HadErrorMessage);
            }
            return ResponseFromWeb;
        }
        public void TestExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
        }
    }
}
