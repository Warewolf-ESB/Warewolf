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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.TO;
using Dev2.DynamicServices;
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
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class WebPutActivityTests
    {
        const string _userAgent = "user-agent";
        const string _contentType = "Content-Type";
        const string _userAgent1 = "Mozilla/4.0";
        const string _userAgent2 = "(compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Constructed_Correctly_ShouldHaveInheritDsfActivity()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var DsfWebPutActivity = new WebPutActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(DsfWebPutActivity, typeof(DsfActivity));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Constructor_Correctly_ShouldSetTypeDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var DsfWebPutActivity = new WebPutActivity();
            //------------Assert Results-------------------------
            Assert.AreEqual("PUT Web Method", DsfWebPutActivity.DisplayName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Constructed_Correctly_ShouldHaveCorrectProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var attributes = typeof(WebPutActivity).GetCustomAttributes(false);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, attributes.Length);
            var toolDescriptor = attributes[0] as ToolDescriptorInfo;
            Assert.IsNotNull(toolDescriptor);
            Assert.AreEqual("PUT", toolDescriptor.Name);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Execute_WithNoOutputDescription_ShouldAddError()
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
            var DsfWebPutActivity = new TestWebPutActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            DsfWebPutActivity.Inputs = serviceInputs;
            DsfWebPutActivity.Outputs = serviceOutputs;
            DsfWebPutActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            DsfWebPutActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            DsfWebPutActivity.QueryString = "";
            DsfWebPutActivity.PutData = "";
            DsfWebPutActivity.SourceId = Guid.Empty;
            DsfWebPutActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            DsfWebPutActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNull(DsfWebPutActivity.OutputDescription);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Execute_WithValidWebResponse_ShouldSetVariables()
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
            var DsfWebPutActivity = new TestWebPutActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            DsfWebPutActivity.Inputs = serviceInputs;
            DsfWebPutActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            using (var service = new WebService(serviceXml) { RequestResponse = response })
            {
                DsfWebPutActivity.OutputDescription = service.GetOutputDescription();
                DsfWebPutActivity.ResponseFromWeb = response;
                var dataObjectMock = new Mock<IDSFDataObject>();
                dataObjectMock.Setup(o => o.Environment).Returns(environment);
                dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
                DsfWebPutActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
                DsfWebPutActivity.QueryString = "";
                DsfWebPutActivity.PutData = "";
                DsfWebPutActivity.SourceId = Guid.Empty;
                DsfWebPutActivity.Headers = new List<INameValue>();
                //------------Execute Test---------------------------
                DsfWebPutActivity.Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.IsNotNull(DsfWebPutActivity.OutputDescription);
                Assert.AreEqual("greater than 7 mile(s):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
                Assert.AreEqual("Paris", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
                Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
                Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
            }
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Execute_WithValidTextResponse_ShouldSetVariables()
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
            var DsfWebPutActivity = new TestWebPutActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Response", "[[Response]]", "") };
            DsfWebPutActivity.Inputs = serviceInputs;
            DsfWebPutActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            using (var service = new WebService(serviceXml) { RequestResponse = response })
            {
                DsfWebPutActivity.OutputDescription = service.GetOutputDescription();
                DsfWebPutActivity.ResponseFromWeb = response;
                var dataObjectMock = new Mock<IDSFDataObject>();
                dataObjectMock.Setup(o => o.Environment).Returns(environment);
                dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
                DsfWebPutActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
                DsfWebPutActivity.QueryString = "";
                DsfWebPutActivity.PutData = "";
                DsfWebPutActivity.SourceId = Guid.Empty;
                DsfWebPutActivity.Headers = new List<INameValue>();
                DsfWebPutActivity.OutputDescription = new OutputDescription();
                DsfWebPutActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape { Paths = new List<IPath> { new StringPath { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

                //------------Execute Test---------------------------
                DsfWebPutActivity.Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.IsNotNull(DsfWebPutActivity.OutputDescription);
                Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Execute_WithInValidWebResponse_ShouldError()
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
            var DsfWebPutActivity = new TestWebPutActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            DsfWebPutActivity.Inputs = serviceInputs;
            DsfWebPutActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            using (var service = new WebService(serviceXml) { RequestResponse = response })
            {
                DsfWebPutActivity.OutputDescription = service.GetOutputDescription();
                DsfWebPutActivity.ResponseFromWeb = invalidResponse;
                var dataObjectMock = new Mock<IDSFDataObject>();
                dataObjectMock.Setup(o => o.Environment).Returns(environment);
                dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
                DsfWebPutActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
                DsfWebPutActivity.QueryString = "";
                DsfWebPutActivity.PutData = "";
                DsfWebPutActivity.SourceId = Guid.Empty;
                DsfWebPutActivity.Headers = new List<INameValue>();
                //------------Execute Test---------------------------
                DsfWebPutActivity.Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.IsNotNull(DsfWebPutActivity.OutputDescription);
                Assert.AreEqual(1, environment.Errors.Count);
                StringAssert.Contains(environment.Errors.ToList()[0], "Invalid character after parsing property name");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Execute_WithValidXmlEscaped_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response = "<CurrentWeather>" +
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
            var DsfWebPutActivity = new TestWebPutActivity
            {
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            DsfWebPutActivity.Inputs = serviceInputs;
            DsfWebPutActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            using (var service = new WebService(serviceXml) { RequestResponse = response })
            {
                DsfWebPutActivity.OutputDescription = service.GetOutputDescription();
                DsfWebPutActivity.ResponseFromWeb = response;
                var dataObjectMock = new Mock<IDSFDataObject>();
                dataObjectMock.Setup(o => o.Environment).Returns(environment);
                dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
                DsfWebPutActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
                DsfWebPutActivity.QueryString = "";
                DsfWebPutActivity.PutData = "";
                DsfWebPutActivity.SourceId = Guid.Empty;
                DsfWebPutActivity.Headers = new List<INameValue>();
                //------------Execute Test---------------------------
                DsfWebPutActivity.Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.IsNotNull(DsfWebPutActivity.OutputDescription);
                Assert.AreEqual("<greater than 7 mile(s):0>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
                Assert.AreEqual("<Paris>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
                Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
                Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Execute_WithInputVariables_ShouldEvalVariablesBeforeExecutingWebRequest()
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
            var DsfWebPutActivity = new TestWebPutActivity
            {
                Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                QueryString = "http://www.testing.com/[[CountryName]]",
                PutData = "This is post:[[Post]]",
                ResourceCatalog = new Mock<IResourceCatalog>().Object
            };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            DsfWebPutActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            using (var service = new WebService(serviceXml) { RequestResponse = response })
            {
                DsfWebPutActivity.OutputDescription = service.GetOutputDescription();
                DsfWebPutActivity.ResponseFromWeb = response;
                var dataObjectMock = new Mock<IDSFDataObject>();
                dataObjectMock.Setup(o => o.Environment).Returns(environment);
                dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
                DsfWebPutActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
                //------------Assert Preconditions-------------------
                Assert.AreEqual(1, DsfWebPutActivity.Headers.Count);
                Assert.AreEqual("Header 1", DsfWebPutActivity.Headers.ToList()[0].Name);
                Assert.AreEqual("[[City]]", DsfWebPutActivity.Headers.ToList()[0].Value);
                Assert.AreEqual("http://www.testing.com/[[CountryName]]", DsfWebPutActivity.QueryString);
                Assert.AreEqual("This is post:[[Post]]", DsfWebPutActivity.PutData);
                //------------Execute Test---------------------------
                DsfWebPutActivity.Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.AreEqual("PMB", DsfWebPutActivity.Head.ToList()[0].Value);
                Assert.AreEqual("http://www.testing.com/South Africa", DsfWebPutActivity.QueryRes);
                Assert.AreEqual("This is post:Some data", DsfWebPutActivity.PostValue);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Constructer_GivenHasInstance_ShouldHaveType()
        {
            //---------------Set up test pack-------------------
            var DsfWebPutActivity = new TestWebPutActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(DsfWebPutActivity);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(DsfWebPutActivity.Type);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_GetFindMissingType_GivenWebPostActivity_ShouldReturnMissingTypeDataGridAcitvity()
        {
            //---------------Set up test pack-------------------
            var DsfWebPutActivity = new TestWebPutActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(DsfWebPutActivity.Type);
            Assert.IsNotNull(DsfWebPutActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, DsfWebPutActivity.GetFindMissingType());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_GetDebugInputs_GivenEnvironmentIsNull_ShouldReturnZeroDebugInputs()
        {
            //---------------Set up test pack-------------------
            var DsfWebPutActivity = new TestWebPutActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = DsfWebPutActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_GetDebugInputs_GivenMockEnvironment_ShouldAddDebugInputItems()
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
            var DsfWebPutActivity = new TestWebPutActivity
            {
                Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                QueryString = "http://www.testing.com/[[CountryName]]",
                PutData = "This is post:[[Post]]"
            };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            DsfWebPutActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            using (var service = new WebService(serviceXml) { RequestResponse = response })
            {
                DsfWebPutActivity.OutputDescription = service.GetOutputDescription();
                DsfWebPutActivity.ResponseFromWeb = response;
                var dataObjectMock = new Mock<IDSFDataObject>();
                dataObjectMock.Setup(o => o.Environment).Returns(environment);
                dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
                DsfWebPutActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
                var cat = new Mock<IResourceCatalog>();
                var src = new WebSource { Address = "www.example.com" };
                cat.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(src);
                DsfWebPutActivity.ResourceCatalog = cat.Object;
                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(DsfWebPutActivity);
                //---------------Execute Test ----------------------
                var debugInputs = DsfWebPutActivity.GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_CreateClient_GivenNoHeaders_ShouldHaveTwoHeaders()
        {
            //---------------Set up test pack-------------------
            var DsfWebPutActivity = new TestWebPutActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PutData = "This is post:[[Post]]"
            };

            //---------------Assert Precondition----------------
            Assert.IsNotNull(DsfWebPutActivity);
            //---------------Execute Test ----------------------
            var httpClient = DsfWebPutActivity.CreateClient(null, String.Empty, TestUtils.CreateWebSourceWithCredentials());
            //---------------Test Result -----------------------
            var actualHeaderCount = httpClient.DefaultRequestHeaders.Count();
            Assert.AreEqual(1, actualHeaderCount);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_CleateClient_GivenNoHeaders_ShouldGlobalConstantsUserAgent()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebPutActivityFromBase();
            var httpClient = deleteActivityFromBase.CreateClient(null, String.Empty, TestUtils.CreateWebSourceWithCredentials());
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var userAgentCollection = httpClient.DefaultRequestHeaders.UserAgent;
            var count = userAgentCollection.Count;
            //---------------Test Result -----------------------
            Assert.AreEqual(0, count);

        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_CreateClient_GivenWebSourceAuthenticationTypeIsUser_ShouldSetWebClientPasswordAndUserName()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebPutActivityFromBase();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webSource = TestUtils.CreateWebSourceWithCredentials();
            var httpClient = deleteActivityFromBase.CreateClient(null, String.Empty, webSource);
            //---------------Test Result -----------------------
            Assert.IsNotNull(httpClient);

            var webClientCredentials = httpClient.DefaultRequestHeaders.Authorization;
            Assert.IsNotNull(webClientCredentials);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_CreateClient_GivenAuthenticationTypeIsNotUser_ShouldNotSetCredentials()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = CreateWebPutActivityFromBase();

            var httpClient = dsfWebPostActivity.CreateClient(null, String.Empty, TestUtils.CreateWebSourceWithAnonymousAuthentication());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(httpClient);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(httpClient.DefaultRequestHeaders.Authorization);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_CreateClient_GivenHeaders_ShouldHaveHeadersAdded()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebPutActivityFromBase();

            var headers = new List<INameValue>
            {
                new NameValue("Content", "text/json")
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(deleteActivityFromBase);
            //---------------Execute Test ----------------------
            var httpClient = deleteActivityFromBase.CreateClient(headers, String.Empty, TestUtils.CreateWebSourceWithAnonymousAuthentication());
            //---------------Test Result -----------------------
            var actualHeaderCount = httpClient.DefaultRequestHeaders.Count();
            Assert.AreEqual(1, actualHeaderCount);
            var allContentValues = httpClient.DefaultRequestHeaders.Single(pair => pair.Key == "Content").Value;
            Assert.AreEqual("text/json", allContentValues.ToList()[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_Execute_ErrorResponse_ShouldSetVariables()
        {
            //------------Setup for test--------------------------
            const string response = "{\"Message\":\"Error\"}";
            var dsfWebPutActivity = new WebPutActivity
            {
                ResourceID = InArgument<Guid>.FromValue(Guid.Empty)
            };
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var webSource = new WebSource
            {
                Address = $"http://{Depends.TFSBLDIP}:9910/api/",
                AuthenticationType = AuthenticationType.Anonymous
            };
            mockResourceCatalog.Setup(resCat => resCat.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(webSource);
            dsfWebPutActivity.ResourceCatalog = mockResourceCatalog.Object;
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Message", "[[Message]]", "") };
            dsfWebPutActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPutActivity.OutputDescription = service.GetOutputDescription();

            dsfWebPutActivity.QueryString = "Error";

            dsfWebPutActivity.SourceId = Guid.Empty;
            dsfWebPutActivity.Headers = new List<INameValue>();
            var dataObject = new DsfDataObject("", Guid.NewGuid())
            {
                EsbChannel = new MockEsb()
            };
            //------------Execute Test---------------------------
            dsfWebPutActivity.Execute(dataObject, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("Error", ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval("[[Message]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPutActivity))]
        public void WebPutActivity_ExecutionImpl_ErrorResultTO_ReturnErrors_ToActivity_Success()
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

                var dsfWebGetActivity = new TestWebPutActivity
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
                Assert.AreEqual("Some error", errorResultTO.FetchErrors()[0]);
            }
        }

        static TestWebPutActivity CreateWebPutActivityFromBase()
        {
            return
                new TestWebPutActivity();
        }
    }

    public class TestWebPutActivity : WebPutActivity
    {
        public string HasErrorMessage { get; set; }

        public string ResponseFromWeb { private get; set; }

        protected override string PerformWebRequest(IEnumerable<INameValue> head, string query, WebSource source, string putData, bool isPutDataBase64 = false)
        {
            Head = head;
            QueryRes = query;
            PostValue = putData;
            if (!string.IsNullOrWhiteSpace(HasErrorMessage))
            {
                base._errorsTo = new ErrorResultTO();
                base._errorsTo.AddError(HasErrorMessage);
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