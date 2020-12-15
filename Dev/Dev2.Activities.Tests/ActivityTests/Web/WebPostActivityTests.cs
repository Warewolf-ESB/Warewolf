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
using System.Net;
using Dev2.Activities;
using Dev2.Common;
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
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.String.Json;
using Warewolf.Core;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class WebPostActivityTests
    {
        const string _userAgent = "user-agent";
        const string _contentType = "Content-Type";
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Constructed_Correctly_ShouldHaveInheritDsfActivity()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var webPostActivity = new WebPostActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(webPostActivity, typeof(DsfActivity));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Constructor_Correctly_ShouldSetTypeDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var webPostActivity = new WebPostActivity();
            //------------Assert Results-------------------------
            Assert.AreEqual("POST Web Method", webPostActivity.DisplayName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Constructed_Correctly_ShouldHaveCorrectProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var attributes = typeof(WebPostActivity).GetCustomAttributes(false);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, attributes.Length, "attributes.Length should be zero after re-instating DsfWebPostActivity");
            /*Assert.AreEqual(1, attributes.Length);
            var toolDescriptor = attributes[0] as ToolDescriptorInfo;
            Assert.IsNotNull(toolDescriptor);
            Assert.AreEqual("POST", toolDescriptor.Name);*/
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Execute_WithNoOutputDescription_ShouldAddError()
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
            var webPostActivity = new TestWebPostActivity();
            webPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            webPostActivity.Inputs = serviceInputs;
            webPostActivity.Outputs = serviceOutputs;
            webPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            webPostActivity.QueryString = "";
            webPostActivity.PostData = "";
            webPostActivity.SourceId = Guid.Empty;
            webPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            webPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNull(webPostActivity.OutputDescription);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Execute_WithValidWebResponse_ShouldSetVariables()
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
            var webPostActivity = new TestWebPostActivity();
            webPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            webPostActivity.Inputs = serviceInputs;
            webPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            webPostActivity.OutputDescription = service.GetOutputDescription();
            webPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            webPostActivity.QueryString = "";
            webPostActivity.PostData = "";
            webPostActivity.SourceId = Guid.Empty;
            webPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            webPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webPostActivity.OutputDescription);
            Assert.AreEqual("greater than 7 mile(s):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
            Assert.AreEqual("Paris", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
            Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
            Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Execute_WithValidTextResponse_ShouldSetVariables()
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
            var webPostActivity = new TestWebPostActivity();
            webPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Response", "[[Response]]", "") };
            webPostActivity.Inputs = serviceInputs;
            webPostActivity.Outputs = serviceOutputs;
         
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            webPostActivity.OutputDescription = service.GetOutputDescription();
            webPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            webPostActivity.QueryString = "";
            webPostActivity.PostData = "";
            webPostActivity.SourceId = Guid.Empty;
            webPostActivity.Headers = new List<INameValue>();
            webPostActivity.OutputDescription = new OutputDescription();
            webPostActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape() { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

            //------------Execute Test---------------------------
            webPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webPostActivity.OutputDescription);
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Execute_WithInValidWebResponse_ShouldError()
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
            const string invalidResponse = "{\"Location\" \"Paris\",\"Time\": \"May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC\"," +
                                    "\"Wind\": \"from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0\"," +
                                    "\"Visibility\": \"greater than 7 mile(s):0\"," +
                                    "\"Temperature\": \"59 F (15 C)\"," +
                                    "\"DewPoint\": \"41 F (5 C)\"," +
                                    "\"RelativeHumidity\": \"51%\"," +
                                    "\"Pressure\": \"29.65 in. Hg (1004 hPa)\"," +
                                    "\"Status\": \"Success\"" +
                                    "";
            var environment = new ExecutionEnvironment();
            environment.Assign("[[City]]", "PMB", 0);
            environment.Assign("[[CountryName]]", "South Africa", 0);
            var webPostActivity = new TestWebPostActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object,
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            webPostActivity.Inputs = serviceInputs;
            webPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            webPostActivity.OutputDescription = service.GetOutputDescription();
            webPostActivity.ResponseFromWeb = invalidResponse;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            webPostActivity.QueryString = "";
            webPostActivity.PostData = "";
            webPostActivity.SourceId = Guid.Empty;
            webPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            webPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webPostActivity.OutputDescription);
            Assert.AreEqual(1, environment.Errors.Count);
            StringAssert.Contains(environment.Errors.ToList()[0], "Invalid character after parsing property name");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Execute_WithValidXmlEscaped_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response ="<CurrentWeather>" +
                                   "<Location>&lt;Paris&gt;</Location>" +
                                   "<Time>May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC</Time>" +
                                   "<Wind>from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0</Wind>" +
                                   "<Visibility>&lt;greater than 7 mile(s):0&gt;</Visibility>" +
                                   "<Temperature> 59 F (15 C)</Temperature>" +
                                   "<DewPoint> 41 F (5 C)</DewPoint>" +
                                   "<RelativeHumidity> 51%</RelativeHumidity>" +
                                   "<Pressure> 29.65 in. Hg (1004 hPa)</Pressure>" +
                                   "<Status>Success</Status>" +
                                   "</CurrentWeather>";
            var environment = new ExecutionEnvironment();
            environment.Assign("[[City]]", "PMB", 0);
            environment.Assign("[[CountryName]]", "South Africa", 0);
            var webPostActivity = new TestWebPostActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object,
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            webPostActivity.Inputs = serviceInputs;
            webPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            webPostActivity.OutputDescription = service.GetOutputDescription();
            webPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            webPostActivity.QueryString = "";
            webPostActivity.PostData = "";
            webPostActivity.SourceId = Guid.Empty;
            webPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            webPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webPostActivity.OutputDescription);
            Assert.AreEqual("<greater than 7 mile(s):0>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
            Assert.AreEqual("<Paris>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
            Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
            Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Execute_WithInputVariables_ShouldEvalVariablesBeforeExecutingWebRequest()
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
            environment.Assign("[[Post]]", "Some data", 0);
            var webPostActivity = new TestWebPostActivity
            {
                Headers = new List<INameValue> {new NameValue("Header 1", "[[City]]")},
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]",
                ResourceCatalog = new Mock<IResourceCatalog>().Object,
            };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            webPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            webPostActivity.OutputDescription = service.GetOutputDescription();
            webPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            //------------Assert Preconditions-------------------
            Assert.AreEqual(1, webPostActivity.Headers.Count);
            Assert.AreEqual("Header 1", webPostActivity.Headers.ToList()[0].Name);
            Assert.AreEqual("[[City]]", webPostActivity.Headers.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/[[CountryName]]", webPostActivity.QueryString);
            Assert.AreEqual("This is post:[[Post]]", webPostActivity.PostData);
            //------------Execute Test---------------------------
            webPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("PMB", webPostActivity.Head.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/South Africa", webPostActivity.QueryRes);
            Assert.AreEqual("This is post:Some data", webPostActivity.PostValue);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Constructor_GivenHasInstance_ShouldHaveType()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(webPostActivity.Type);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_GetFindMissingType_GivenWebPostActivity_ShouldReturnMissingTypeDataGridActivity()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity.Type);
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, webPostActivity.GetFindMissingType());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_GetDebugInputs_GivenEnvironmentIsNull_ShouldReturnZeroDebugInputs()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = webPostActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_GetDebugInputs_GivenMockEnvironment_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------
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
            environment.Assign("[[Post]]", "Some data", 0);
            var webPostActivity = new TestWebPostActivity
            {
                Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            webPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            webPostActivity.OutputDescription = service.GetOutputDescription();
            webPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            var cat = new Mock<IResourceCatalog>();
            var src = new WebSource { Address = "www.example.com" };
            cat.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(src);
            webPostActivity.ResourceCatalog = cat.Object;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(environment);
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var debugInputs = webPostActivity.GetDebugInputs(environment, 0);
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugInputs);
            Assert.AreEqual(4,debugInputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_CreateClient_GivenNoHeaders_ShouldHaveTwoHeaders()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
         
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var webClient = WebPostActivity.CreateClient(null, String.Empty, new WebSource());
            //---------------Test Result -----------------------
            var actualHeaderCount = webClient.Headers.Count;
            Assert.AreEqual(1, actualHeaderCount);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_CreateClient_GivenNoHeaders_ShouldHaveUserAgentHeader()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webClient = WebPostActivity.CreateClient(null, String.Empty, new WebSource());
            //---------------Assert Precondition----------------
            var actualHeaderCount = webClient.Headers.Count;
            Assert.AreEqual(1, actualHeaderCount);
            //---------------Execute Test ----------------------

            var userAgentHeader = webClient.Headers.AllKeys.Single(header => header == _userAgent);
            //---------------Test Result -----------------------
            Assert.IsNotNull(userAgentHeader);
            Assert.AreEqual(_userAgent, userAgentHeader);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_CreateClient_GivenNoHeaders_ShouldGlobalConstantsUserAgent()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webClient = WebPostActivity.CreateClient(null, String.Empty, new WebSource());
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var userAgentValue = webClient.Headers[_userAgent];
            //---------------Test Result -----------------------
            Assert.AreEqual(userAgentValue, GlobalConstants.UserAgentString);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_CreateClient_GivenWebSourceAuthenticationTypeIsUser_ShouldSetWebClientPasswordAndUserName()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webSource = new WebSource { AuthenticationType = AuthenticationType.User, UserName = "John1", Password = "Password1"};

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webClient = WebPostActivity.CreateClient(null, String.Empty, webSource);
            //---------------Test Result -----------------------
            Assert.IsNotNull(webClient);
            var networkCredentialFromWebSource = new NetworkCredential(webSource.UserName, webSource.Password);
            var webClientCredentials = webClient.Credentials as NetworkCredential;
            Assert.IsNotNull(webClientCredentials);
            Assert.AreEqual(webClientCredentials.UserName, networkCredentialFromWebSource.UserName);
            Assert.AreEqual(webClientCredentials.Password, networkCredentialFromWebSource.Password);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_CreateClient_GivenAuthenticationTypeIsNotUser_ShouldNotSetCredentials()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webSource = new WebSource { AuthenticationType = AuthenticationType.Windows, UserName = "John1", Password = "Password1" };
            var webClient = WebPostActivity.CreateClient(null, String.Empty, webSource);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webClient);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(webClient.Credentials);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_CreateClient_GivenHeaders_ShouldHaveHeadersAdded()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new TestWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };

            var headers = new List<INameValue>
            {
                new NameValue("Content","text/json")
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var webClient = WebPostActivity.CreateClient(headers, String.Empty, new WebSource());
            //---------------Test Result -----------------------
            var actualHeaderCount = webClient.Headers.Count;
            Assert.AreEqual(2, actualHeaderCount);
            Assert.AreEqual("text/json", webClient.Headers["Content"]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Execute_ErrorResponse_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response = "{\"Message\":\"Error\"}";
            var environment = new ExecutionEnvironment();

            var webPostActivity = new TestWebPostActivity();
            webPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var webSource = new WebSource();
            webSource.Address = "http://TFSBLD.premier.local:9910/api/";
            webSource.AuthenticationType = AuthenticationType.Anonymous;
            mockResourceCatalog.Setup(resCat => resCat.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(webSource);
            webPostActivity.ResourceCatalog = mockResourceCatalog.Object;

            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Message", "[[Message]]", "") };
            webPostActivity.Outputs = serviceOutputs;

            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            webPostActivity.OutputDescription = service.GetOutputDescription();
            webPostActivity.ResponseFromWeb = response;
            webPostActivity.QueryString = "Error";
            webPostActivity.SourceId = Guid.Empty;
            webPostActivity.Headers = new List<INameValue>();

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            webPostActivity.SourceId = Guid.Empty;
            webPostActivity.Headers = new List<INameValue>();
            webPostActivity.OutputDescription = new OutputDescription();
            webPostActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape() { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

            //------------Execute Test---------------------------
            webPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Message]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_ExecutionImpl_ErrorResultTO_ReturnErrors_ToActivity_Success()
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

                var dsfWebGetActivity = new TestWebPostActivity
                {
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    HasErrorMessage = "Some error"
                };
                //-----------------------Act-----------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
                Assert.AreEqual(response, errorResultTO.FetchErrors()[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_ExecutionImpl_ResponseManager_PushResponseIntoEnvironment_GivenJsonResponse_MappedToRecodSet_ShouldSucess()
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

    public class TestWebPostActivity : WebPostActivity
    {
        public string HasErrorMessage { get; set; }

        public string ResponseFromWeb { private get; set; }

        protected override string PerformWebPostRequest(IEnumerable<INameValue> head, string query, WebSource source, string postData)
        {
            Head = head;
            QueryRes = query;
            PostValue = postData;
            if (!string.IsNullOrWhiteSpace(HasErrorMessage))
            {
                base._errorsTo = new ErrorResultTO();
                base._errorsTo.AddError(ResponseFromWeb);
            }
            return ResponseFromWeb;
        }
        
        public string PostValue { get; private set; }

        public string QueryRes { get; private set; }

        public IEnumerable<INameValue> Head { get; private set; }

        public void TestExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
        }
    }
}
