/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
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
using Warewolf.Data.Options;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using Warewolf.UnitTestAttributes;

namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class WebPostActivityNewTests
    {
        const string _userAgent = "user-agent";
        const string _contentType = "Content-Type";

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Constructed_Correctly_ShouldHaveInheritDsfActivity()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var webPostActivityNew = new WebPostActivityNew();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(webPostActivityNew
                , typeof(DsfActivity));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Constructor_Correctly_ShouldSetTypeDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var webPostActivityNew = new WebPostActivityNew();
            //------------Assert Results-------------------------
            Assert.AreEqual("POST Web Method", webPostActivityNew.DisplayName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Constructed_Correctly_ShouldHaveCorrectProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var attributes = typeof(WebPostActivityNew).GetCustomAttributes(false);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, attributes.Length);
            var toolDescriptor = attributes[0] as ToolDescriptorInfo;
            Assert.IsNotNull(toolDescriptor);
            Assert.AreEqual("POST", toolDescriptor.Name);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Execute_WithNoOutputDescription_ShouldAddError()
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

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var webPostActivityNew = new TestWebPostActivityNew
            {
                ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                ResourceCatalog = new Mock<IResourceCatalog>().Object,
                Inputs = new List<IServiceInput>
                {
                    new ServiceInput("CityName", "[[City]]"),
                    new ServiceInput("Country", "[[CountryName]]")
                },
                Outputs = new List<IServiceOutputMapping>
                {
                    new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"),
                    new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"),
                    new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"),
                    new ServiceOutputMapping("Visibility", "[[Visibility]]", "")
                },
                ResponseFromWeb = response,
            };

            //------------Execute Test---------------------------
            webPostActivityNew.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNull(webPostActivityNew.OutputDescription);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Execute_WithValidWebResponse_ShouldSetVariables()
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

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                var settings = new List<INameValue>();
                settings.Add(new NameValue("IsManualChecked", "true"));
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
                    Settings = settings,
                    Inputs = new List<IServiceInput>
                    {
                        new ServiceInput("CityName", "[[City]]"),
                        new ServiceInput("Country", "[[CountryName]]")
                    },
                    Outputs = new List<IServiceOutputMapping>
                    {
                        new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"),
                        new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"),
                        new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"),
                        new ServiceOutputMapping("Visibility", "[[Visibility]]", "")
                    },
                    OutputDescription = service.GetOutputDescription(),
                    ResponseFromWeb = response,
                };

                //------------Execute Test---------------------------
                webPostActivityNew
                    .Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.IsNotNull(webPostActivityNew.OutputDescription);
                Assert.AreEqual("greater than 7 mile(s):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
                Assert.AreEqual("Paris", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
                Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
                Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Execute_WithValidTextResponse_ShouldSetVariables()
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

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            var webPostActivityNew = new TestWebPostActivityNew
            {
                ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                ResourceCatalog = new Mock<IResourceCatalog>().Object,
                Settings = settings,
                ResponseFromWeb = response,
                Outputs = new List<IServiceOutputMapping>
                {
                    new ServiceOutputMapping("Response", "[[Response]]", "")
                },
                OutputDescription = new OutputDescription
                {
                    DataSourceShapes = new List<IDataSourceShape>
                    {
                        new DataSourceShape
                        {
                            Paths = new List<IPath>
                            {
                                new StringPath
                                {
                                    ActualPath = "[[Response]]",
                                    OutputExpression = "[[Response]]"
                                }
                            }
                        }
                    }
                }
            };

            //------------Execute Test---------------------------
            webPostActivityNew.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(webPostActivityNew.OutputDescription);
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Execute_WithInValidWebResponse_ShouldError()
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

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
                    Settings = settings,
                    OutputDescription = service.GetOutputDescription(),
                    PostData = "",
                    QueryString = "",
                    ResponseFromWeb = invalidResponse,
                    Inputs = new List<IServiceInput>
                    {
                        new ServiceInput("CityName", "[[City]]"),
                        new ServiceInput("Country", "[[CountryName]]")
                    },
                    Outputs = new List<IServiceOutputMapping>
                    {
                        new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"),
                        new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"),
                        new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"),
                        new ServiceOutputMapping("Visibility", "[[Visibility]]", "")
                    },
                };

                //------------Execute Test---------------------------
                webPostActivityNew
                    .Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.IsNotNull(webPostActivityNew
                    .OutputDescription);
                Assert.AreEqual(1, environment.Errors.Count);
                StringAssert.Contains(environment.Errors.ToList()[0], "Invalid character after parsing property name");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Execute_WithValidXmlEscaped_ShouldSetVariables()
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

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
                    Settings = settings,
                    PostData = "",
                    OutputDescription = service.GetOutputDescription(),
                    ResponseFromWeb = response,
                    Inputs = new List<IServiceInput>
                    {
                        new ServiceInput("CityName", "[[City]]"),
                        new ServiceInput("Country", "[[CountryName]]")
                    },
                    Outputs = new List<IServiceOutputMapping>
                    {
                        new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"),
                        new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"),
                        new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"),
                        new ServiceOutputMapping("Visibility", "[[Visibility]]", "")
                    },
                };

                //------------Execute Test---------------------------
                webPostActivityNew
                    .Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.IsNotNull(webPostActivityNew
                    .OutputDescription);
                Assert.AreEqual("<greater than 7 mile(s):0>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
                Assert.AreEqual("<Paris>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
                Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
                Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Execute_WithInputVariables_ShouldEvalVariablesBeforeExecutingWebRequest()
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

            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    PostData = "This is post:[[Post]]",
                    OutputDescription = service.GetOutputDescription(),
                    Outputs = new List<IServiceOutputMapping>
                    {
                        new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"),
                        new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"),
                        new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"),
                        new ServiceOutputMapping("Visibility", "[[Visibility]]", "")
                    },
                    ResponseFromWeb = response,
                };

                //------------Assert Preconditions-------------------
                Assert.AreEqual(1, webPostActivityNew
                    .Headers.Count);
                Assert.AreEqual("Header 1", webPostActivityNew
                    .Headers.ToList()[0].Name);
                Assert.AreEqual("[[City]]", webPostActivityNew
                    .Headers.ToList()[0].Value);
                Assert.AreEqual("http://www.testing.com/[[CountryName]]", webPostActivityNew
                    .QueryString);
                Assert.AreEqual("This is post:[[Post]]", webPostActivityNew
                    .PostData);
                //------------Execute Test---------------------------
                webPostActivityNew
                    .Execute(dataObjectMock.Object, 0);
                //------------Assert Results-------------------------
                Assert.AreEqual("PMB", webPostActivityNew
                    .Head.ToList()[0].Value);
                Assert.AreEqual("http://www.testing.com/South Africa", webPostActivityNew
                    .QueryRes);
                Assert.AreEqual("This is post:Some data", webPostActivityNew
                    .PostValue);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Constructor_GivenHasInstance_ShouldHaveType()
        {
            //---------------Set up test pack-------------------
            var webPostActivityNew = new TestWebPostActivityNew();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivityNew
                );
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(webPostActivityNew.Type);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ShouldReturnMissingTypeDataGridActivity()
        {
            //---------------Set up test pack-------------------
            var webPostActivityNew = new TestWebPostActivityNew();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivityNew.Type);
            Assert.IsNotNull(webPostActivityNew
                );
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, webPostActivityNew.GetFindMissingType());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_GivenEnvironmentIsNull_ShouldReturnZeroDebugInputs()
        {
            //---------------Set up test pack-------------------
            var webPostActivityNew = new TestWebPostActivityNew();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = webPostActivityNew.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_IsManualChecked_GivenMockEnvironment_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------
            var environment = new ExecutionEnvironment();
            environment.Assign("[[City]]", "PMB", 0);
            environment.Assign("[[CountryName]]", "South Africa", 0);
            environment.Assign("[[Post]]", "Some data", 0);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new WebSource { Address = "www.example.com" });

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    PostData = "This is post:[[Post]]",
                    ResourceCatalog = mockResourceCatalog.Object,
                };

                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(webPostActivityNew
                    );
                //---------------Execute Test ----------------------
                var debugInputs = webPostActivityNew
                    .GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);

                var item4 = debugInputs.Last().FetchResultsList();
                Assert.IsTrue(item4.Count == 2);
                Assert.AreEqual("Post Data", item4.First().Label);
                Assert.AreEqual("This is post:Some data", item4.Last().Value);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_IsFormDataChecked_GivenNoEnvironmentVariablesToEval_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------
            var testTextKey = "testTextKey";
            var testTextValue = "testTextValue";

            var testFileKey = "testFileKey";
            var testFileContent = "this can be any file type converted to base64 string, if it ware to be pasted into this textbox";
            var testFileName = "testFileName.ext";

            var environment = new ExecutionEnvironment();

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    new WebSource
                    {
                        Address = "www.example.com"
                    });

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    ResourceCatalog = mockResourceCatalog.Object,
                    Conditions = new List<FormDataConditionExpression>
                    {
                        new FormDataConditionExpression
                        {
                            Key = testTextKey,
                            Cond = new FormDataConditionText
                            {
                                Value = testTextValue
                            }
                        },
                        new FormDataConditionExpression
                        {
                            Key = testFileKey,
                            Cond = new FormDataConditionFile
                            {
                                FileBase64 = testFileContent,
                                FileName = testFileName
                            }
                        }
                    }
                };

                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(webPostActivityNew
                    );
                //---------------Execute Test ----------------------
                var debugInputs = webPostActivityNew
                    .GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);

                var item4 = debugInputs.Last().FetchResultsList();
                var item4First = item4.First();
                Assert.AreEqual(1, item4.Count);
                Assert.AreEqual("Parameters", item4First.Label);
                Assert.AreEqual("\nKey: testTextKey Text: testTextValue\nKey: testFileKey File Content: this can be any file type converted to base64 string, if it ware to be pasted into this textbox File Name: testFileName.ext", item4First.Value);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_IsUrlEncodedChecked_GivenNoEnvironmentVariablesToEval_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------
            var testTextKey = "testTextKey";
            var testTextValue = "testTextValue";

            var environment = new ExecutionEnvironment();

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    new WebSource
                    {
                        Address = "www.example.com"
                    });

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsUrlEncodedChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    ResourceCatalog = mockResourceCatalog.Object,
                    Conditions = new List<FormDataConditionExpression>
                    {
                        new FormDataConditionExpression
                        {
                            Key = testTextKey,
                            Cond = new FormDataConditionText
                            {
                                Value = testTextValue
                            }
                        }
                    }
                };

                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(webPostActivityNew
                    );
                //---------------Execute Test ----------------------
                var debugInputs = webPostActivityNew
                    .GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);

                var item4 = debugInputs.Last().FetchResultsList();
                var item4First = item4.First();
                Assert.AreEqual(1, item4.Count);
                Assert.AreEqual("Parameters", item4First.Label);
                Assert.IsTrue(debugInputs[2].ResultsList[1].Value.Contains("application/x-www-form-urlencoded"));
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_IsFormDataChecked_Given_MultipartHeader_WithEnvironmentVariablesToEval_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------
            var multipartFormDataVar = "multipart/form-data";

            var environment = new ExecutionEnvironment();
            environment.Assign("[[multipartFormDataVar]]", multipartFormDataVar, 0);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(environment);
            mockDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    new WebSource
                    {
                        Address = "www.example.com"
                    });

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Headers = new List<INameValue> { new NameValue("Content-Type", "[[multipartFormDataVar]]") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    ResourceCatalog = mockResourceCatalog.Object,
                    Conditions = new List<FormDataConditionExpression> { }
                };

                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(webPostActivityNew
                    );
                //---------------Execute Test ----------------------
                var debugInputs = webPostActivityNew
                    .GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);

                var item2 = debugInputs[2].FetchResultsList();
                var item4First = item2.First();
                Assert.AreEqual(2, item2.Count);
                Assert.AreEqual("Headers", item4First.Label);
                StringAssert.Contains(item2[1].Value, "Content-Type : multipart/form-data; boundary=----------");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_IsUrlEncodedChecked_Given_MultipartHeader_WithEnvironmentVariablesToEval_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------
            var urlEncodedVar = "application/x-www-form-urlencoded";

            var environment = new ExecutionEnvironment();
            environment.Assign("[[urlEncodedVar]]", urlEncodedVar, 0);

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(environment);
            mockDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    new WebSource
                    {
                        Address = "www.example.com"
                    });

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Headers = new List<INameValue> { new NameValue("Content-Type", "[[urlEncodedVar]]") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    ResourceCatalog = mockResourceCatalog.Object,
                    Conditions = new List<FormDataConditionExpression> { }
                };

                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(webPostActivityNew
                    );
                //---------------Execute Test ----------------------
                var debugInputs = webPostActivityNew
                    .GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);

                var item2 = debugInputs[2].FetchResultsList();
                var item4First = item2.First();
                Assert.AreEqual(2, item2.Count);
                Assert.AreEqual("Headers", item4First.Label);
                StringAssert.Contains(item2[1].Value, "Content-Type : application/x-www-form-urlencoded");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_IsFormDataChecked_Given_MultipartHeader_WithNoEnvironmentVariablesToEval_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------

            var environment = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(environment);
            mockDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    new WebSource
                    {
                        Address = "www.example.com"
                    });

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Headers = new List<INameValue> { new NameValue("Content-Type", "multipart/form-data") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    ResourceCatalog = mockResourceCatalog.Object,
                    Conditions = new List<FormDataConditionExpression> { }
                };

                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(webPostActivityNew
                    );
                //---------------Execute Test ----------------------
                var debugInputs = webPostActivityNew
                    .GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);

                var item2 = debugInputs[2].FetchResultsList();
                var item4First = item2.First();
                Assert.AreEqual(2, item2.Count);
                Assert.AreEqual("Headers", item4First.Label);
                StringAssert.Contains(item2[1].Value, "Content-Type : multipart/form-data; boundary=----------");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_GetDebugInputs_IsUrlEncodedChecked_Given_MultipartHeader_WithNoEnvironmentVariablesToEval_ShouldAddDebugInputItems()
        {
            //---------------Set up test pack-------------------

            var environment = new ExecutionEnvironment();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(environment);
            mockDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(
                    new WebSource
                    {
                        Address = "www.example.com"
                    });

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                var webPostActivityNew = new TestWebPostActivityNew
                {
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Headers = new List<INameValue> { new NameValue("Content-Type", "application/x-www-form-urlencoded") },
                    QueryString = "http://www.testing.com/[[CountryName]]",
                    Settings = settings,
                    ResourceCatalog = mockResourceCatalog.Object,
                    Conditions = new List<FormDataConditionExpression> { }
                };

                //---------------Assert Precondition----------------
                Assert.IsNotNull(environment);
                Assert.IsNotNull(webPostActivityNew
                    );
                //---------------Execute Test ----------------------
                var debugInputs = webPostActivityNew
                    .GetDebugInputs(environment, 0);
                //---------------Test Result -----------------------
                Assert.IsNotNull(debugInputs);
                Assert.AreEqual(4, debugInputs.Count);

                var item2 = debugInputs[2].FetchResultsList();
                var item4First = item2.First();
                Assert.AreEqual(2, item2.Count);
                Assert.AreEqual("Headers", item4First.Label);
                StringAssert.Contains(item2[1].Value, "Content-Type : application/x-www-form-urlencoded");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_Execute_ErrorResponse_ShouldSetVariables()
        {
            using (var dependency = new Depends(Depends.ContainerType.HTTPVerbsApi))
            {
                //------------Setup for test--------------------------
                const string response = "{\"Message\":\"Error\"}";
                var environment = new ExecutionEnvironment();

                var mockResourceCatalog = new Mock<IResourceCatalog>();
                mockResourceCatalog.Setup(resCat => resCat.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .Returns(
                        new WebSource
                        {
                            Address = $"http://{dependency.Container.IP}:{dependency.Container.Port}/api/",
                            AuthenticationType = AuthenticationType.Anonymous
                        });

                var dataObjectMock = new Mock<IDSFDataObject>();
                dataObjectMock.Setup(o => o.Environment).Returns(environment);
                dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var settings = new List<INameValue>();
                settings.Add(new NameValue("IsManualChecked", "true"));
                using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
                {
                    var webPostActivityNew
                        = new TestWebPostActivityNew
                        {
                            ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                            ResourceCatalog = mockResourceCatalog.Object,
                            Settings = settings,
                            Outputs = new List<IServiceOutputMapping>
                        {
                            new ServiceOutputMapping("Message", "[[Message]]", "")
                        },

                            OutputDescription = new OutputDescription
                            {
                                DataSourceShapes = new List<IDataSourceShape>
                            {
                                new DataSourceShape
                                {
                                    Paths = new List<IPath>
                                    {
                                        new StringPath
                                        {
                                            ActualPath = "[[Response]]",
                                            OutputExpression = "[[Response]]"
                                        }
                                    }
                                }
                            }
                            },
                            ResponseFromWeb = response,
                            QueryString = "Error",
                            SourceId = Guid.Empty,
                            Headers = new List<INameValue>()
                        };

                    //------------Execute Test---------------------------
                    webPostActivityNew
                        .Execute(dataObjectMock.Object, 0);
                    //------------Assert Results-------------------------
                    Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Message]]", 0)));
                }
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ExecutionImpl_ErrorResultTO_ReturnErrors_ToActivity_Success()
        {
            //-----------------------Arrange-------------------------
            const string response = "{\"Message\":\"TEST Error\"}";
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var errorResultTO = new ErrorResultTO();

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebPostActivityNew
                {
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    Settings = settings,
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
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ExecutionImpl_ResponseManager_PushResponseIntoEnvironment_GivenJsonResponse_MappedToRecodSet_ShouldSucess()
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

                var dsfWebGetActivity = new TestWebPostActivityNew
                {
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ExecutionImpl_ResponseManager_PushResponseIntoEnvironment_GivenXmlResponse_MappedToJsonObject_ShouldSucess()
        {
            //-----------------------Arrange-------------------------
            const string xml = "<Messanger><Message>xml response from the request</Message></Messanger>";
            var response = Convert.ToBase64String(xml.ToBytesArray());
            const string objName = "[[@objName]]";

            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebPostActivityNew
                {
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    IsObject = true,
                    ObjectName = objName,
                    Outputs = new List<IServiceOutputMapping>
                    {
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
                Assert.AreEqual("Messanger.Message", paths.First().ActualPath);
                Assert.AreEqual("Messanger.Message", paths.First().DisplayPath);
                Assert.AreEqual(string.Empty, paths.First().OutputExpression);
                Assert.AreEqual("xml response from the request", paths.First().SampleData);

                //assert execution environment
                var envirVariable = environment.Eval(objName, 0);
                var ress = envirVariable as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                Assert.IsNotNull(envirVariable);
                Assert.IsTrue(ress.Item.IsNothing, "Item should Not contain the recset mapped to the messanger key");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ExecutionImpl_Given_PostData_With_EnvironmentVariable_NotExist_And_ShouldThrow_True_ShouldThrow()
        {
            //-----------------------Arrange-------------------------
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var errorResultTO = new ErrorResultTO();

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsManualChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")))
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var dsfWebGetActivity = new TestWebPostActivityNew
                {
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    Settings = settings,
                    PostData = "{'valueKey':'[[NotExistVariable]]'}"
                };
                //-----------------------Act-----------------------------
                //-----------------------Assert--------------------------
                dsfWebGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                Assert.AreEqual(1, errorResultTO.FetchErrors().Count);
                Assert.AreEqual("variable { NotExistVariable } not found", errorResultTO.FetchErrors()[0]);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ExecutionImpl_PerformFormDataWebPostRequest_ExpectSuccess()
        {
            //-----------------------Arrange-------------------------
            var testWebResponse = "this can be the web response";
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var errorResultTO = new ErrorResultTO();

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var webGetActivity = new TestWebPostActivityNew
                {
                    ResourceCatalog = mockResourceCatalog.Object,
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    Settings = settings,
                    ResponseFromWeb = testWebResponse
                };
                //-----------------------Act-----------------------------
                //-----------------------Assert--------------------------
                webGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);

                //assert first DataSourceShapes
                var resourceManager = webGetActivity.ResponseManager;
                var outputDescription = resourceManager.OutputDescription;
                var dataShapes = outputDescription.DataSourceShapes;
                var paths = dataShapes.First().Paths;
                Assert.IsNotNull(outputDescription);
                Assert.AreEqual("Response", paths.First().ActualPath);
                Assert.AreEqual("Response", paths.First().DisplayPath);
                Assert.AreEqual(string.Empty, paths.First().OutputExpression);
                Assert.AreEqual(string.Empty, paths.First().SampleData);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ExecutionImpl_PerformUrlEncodedWebPostRequest_ExpectSuccess()
        {
            //-----------------------Arrange-------------------------
            var testWebResponse = "this can be the web response";
            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var errorResultTO = new ErrorResultTO();

            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsUrlEncodedChecked", "true"));
            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = string.Empty })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var webGetActivity = new TestWebPostActivityNew
                {
                    ResourceCatalog = mockResourceCatalog.Object,
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    Settings = settings,
                    ResponseFromWeb = testWebResponse
                };
                //-----------------------Act-----------------------------
                //-----------------------Assert--------------------------
                webGetActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);

                //assert first DataSourceShapes
                var resourceManager = webGetActivity.ResponseManager;
                var outputDescription = resourceManager.OutputDescription;
                var dataShapes = outputDescription.DataSourceShapes;
                var paths = dataShapes.First().Paths;
                Assert.IsNotNull(outputDescription);
                Assert.AreEqual("Response", paths.First().ActualPath);
                Assert.AreEqual("Response", paths.First().DisplayPath);
                Assert.AreEqual(string.Empty, paths.First().OutputExpression);
                Assert.AreEqual(string.Empty, paths.First().SampleData);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivityNew_ExecutionImpl_ResponseManager_PushResponseIntoEnvironment_EscapedJsonString_ShouldSucess()
        {
            //-----------------------Arrange-------------------------
            const string json = "\"{\\\"val1\\\":\\\"A\\\", \\\"val2\\\":\\\"B\\\"}\"";
            var response = Convert.ToBase64String(json.ToBytesArray());
            const string varName = "[[retVal]]";

            var environment = new ExecutionEnvironment();

            var mockEsbChannel = new Mock<IEsbChannel>();
            var mockDSFDataObject = new Mock<IDSFDataObject>();
            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var errorResultTO = new ErrorResultTO();

            using (var service = new WebService(XmlResource.Fetch("WebService")) { RequestResponse = response })
            {
                mockDSFDataObject.Setup(o => o.Environment).Returns(environment);
                mockDSFDataObject.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

                var settings = new List<INameValue>();
                settings.Add(new NameValue("IsManualChecked", "true"));
                var dsfWebPostActivity = new TestWebPostActivityNew
                {
                    ResourceCatalog = new Mock<IResourceCatalog>().Object,
                    OutputDescription = service.GetOutputDescription(),
                    ResourceID = InArgument<Guid>.FromValue(Guid.Empty),
                    QueryString = "test Query",
                    Headers = new List<INameValue>(),
                    ResponseFromWeb = response,
                    IsObject = false,
                    ObjectName = varName,
                    Outputs = new List<IServiceOutputMapping>
                    {
                        new ServiceOutputMapping("retVal", varName, "")
                    },
                    Settings = settings
                };
                //-----------------------Act-----------------------------
                dsfWebPostActivity.TestExecutionImpl(mockEsbChannel.Object, mockDSFDataObject.Object, "Test Inputs", "Test Outputs", out errorResultTO, 0);
                //-----------------------Assert--------------------------
                Assert.IsFalse(errorResultTO.HasErrors());


                var retVal = ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[retVal]]", 0));
                Assert.AreEqual("\"{\\\"val1\\\":\\\"A\\\", \\\"val2\\\":\\\"B\\\"}\"", retVal);
            }
        }
    }

    public class TestWebPostActivityNew : WebPostActivityNew
    {
        public string HasErrorMessage { get; set; }

        public string ResponseFromWeb { private get; set; }

        public string PostValue { get; private set; }
        public IEnumerable<IFormDataParameters> FormDataParametersValue { get; private set; }

        public string QueryRes { get; private set; }

        public IEnumerable<INameValue> Head { get; private set; }
        public IEnumerable<string> HeadString { get; private set; }

        public void TestExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
        }

        public override IList<IActionableErrorInfo> PerformValidation()
        {
            return base.PerformValidation();
        }

        protected override string PerformWebPostRequest(IWebPostOptions webPostOptions)
        {
            Head = webPostOptions.Head;
            HeadString = webPostOptions.Headers;
            QueryRes = webPostOptions.Query;
            FormDataParametersValue = webPostOptions.Parameters;
            PostValue = webPostOptions.PostData;

            if (!string.IsNullOrWhiteSpace(HasErrorMessage))
            {
                base._errorsTo = new ErrorResultTO();
                base._errorsTo.AddError(ResponseFromWeb);
            }

            return ResponseFromWeb;
        }
    }
}