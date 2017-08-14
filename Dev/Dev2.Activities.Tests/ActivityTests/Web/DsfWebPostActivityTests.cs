using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
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


namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class DsfWebPostActivityTests
    {

        const string userAgent = "user-agent";
        const string contentType = "Content-Type";
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Constructed")]
        public void DsfWebPostActivity_Constructed_Correctly_ShouldHaveInheritDsfActivity()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfWebPostActivity = new DsfWebPostActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dsfWebPostActivity, typeof(DsfActivity));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Constructor")]
        public void DsfWebPostActivity_Constructor_Correctly_ShouldSetTypeDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfWebPostActivity = new DsfWebPostActivity();
            //------------Assert Results-------------------------
            Assert.AreEqual("POST Web Method", dsfWebPostActivity.DisplayName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Constructed")]
        public void DsfWebPostActivity_Constructed_Correctly_ShouldHaveCorrectProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var attributes = typeof(DsfWebPostActivity).GetCustomAttributes(false);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, attributes.Length);
            var toolDescriptor = attributes[0] as ToolDescriptorInfo;
            Assert.IsNotNull(toolDescriptor);
            Assert.AreEqual("POST", toolDescriptor.Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithNoOutputDescription_ShouldAddError()
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
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            dsfWebPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Inputs = serviceInputs;
            dsfWebPostActivity.Outputs = serviceOutputs;
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebPostActivity.QueryString = "";
            dsfWebPostActivity.PostData = "";
            dsfWebPostActivity.SourceId = Guid.Empty;
            dsfWebPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNull(dsfWebPostActivity.OutputDescription);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithValidWebResponse_ShouldSetVariables()
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
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            dsfWebPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Inputs = serviceInputs;
            dsfWebPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebPostActivity.QueryString = "";
            dsfWebPostActivity.PostData = "";
            dsfWebPostActivity.SourceId = Guid.Empty;
            dsfWebPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebPostActivity.OutputDescription);
            Assert.AreEqual("greater than 7 mile(s):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
            Assert.AreEqual("Paris", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
            Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
            Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithValidTextResponse_ShouldSetVariables()
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
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            dsfWebPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Response", "[[Response]]", "") };
            dsfWebPostActivity.Inputs = serviceInputs;
            dsfWebPostActivity.Outputs = serviceOutputs;
         
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebPostActivity.QueryString = "";
            dsfWebPostActivity.PostData = "";
            dsfWebPostActivity.SourceId = Guid.Empty;
            dsfWebPostActivity.Headers = new List<INameValue>();
            dsfWebPostActivity.OutputDescription = new OutputDescription();
            dsfWebPostActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape() { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });

            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebPostActivity.OutputDescription);
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));

        }
        


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithInValidWebResponse_ShouldError()
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
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            dsfWebPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Inputs = serviceInputs;
            dsfWebPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = invalidResponse;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebPostActivity.QueryString = "";
            dsfWebPostActivity.PostData = "";
            dsfWebPostActivity.SourceId = Guid.Empty;
            dsfWebPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebPostActivity.OutputDescription);
            Assert.AreEqual(1, environment.Errors.Count);
            StringAssert.Contains(environment.Errors.ToList()[0], "Invalid character after parsing property name");
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithValidXmlEscaped_ShouldSetVariables()
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
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            dsfWebPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Inputs = serviceInputs;
            dsfWebPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebPostActivity.QueryString = "";
            dsfWebPostActivity.PostData = "";
            dsfWebPostActivity.SourceId = Guid.Empty;
            dsfWebPostActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebPostActivity.OutputDescription);
            Assert.AreEqual("<greater than 7 mile(s):0>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
            Assert.AreEqual("<Paris>", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
            Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
            Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
        }
        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfWebPostActivity_Execute")]
        public void DsfWebPostActivity_Execute_WithInputVariables_ShouldEvalVariablesBeforeExecutingWebRequest()
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
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            dsfWebPostActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            //------------Assert Preconditions-------------------
            Assert.AreEqual(1, dsfWebPostActivity.Headers.Count);
            Assert.AreEqual("Header 1", dsfWebPostActivity.Headers.ToList()[0].Name);
            Assert.AreEqual("[[City]]", dsfWebPostActivity.Headers.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/[[CountryName]]", dsfWebPostActivity.QueryString);
            Assert.AreEqual("This is post:[[Post]]", dsfWebPostActivity.PostData);
            //------------Execute Test---------------------------
            dsfWebPostActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("PMB", dsfWebPostActivity.Head.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/South Africa", dsfWebPostActivity.QueryRes);
            Assert.AreEqual("This is post:Some data", dsfWebPostActivity.PostValue);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructer_GivenHasInstance_ShouldHaveType()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebPostActivity);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(dsfWebPostActivity.Type);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenWebPostActivity_ShouldReturnMissingTypeDataGridAcitvity()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebPostActivity.Type);
            Assert.IsNotNull(dsfWebPostActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, dsfWebPostActivity.GetFindMissingType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldReturnZeroDebugInputs()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = dsfWebPostActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenMockEnvironment_ShouldAddDebugInputItems()
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
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") },
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebPostActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebPostActivity.OutputDescription = service.GetOutputDescription();
            dsfWebPostActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebPostActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            var cat = new Mock<IResourceCatalog>();
            var src = new WebSource { Address = "www.example.com" };
            cat.Setup(a => a.GetResource<WebSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(src);
            dsfWebPostActivity.ResourceCatalog = cat.Object;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(environment);
            Assert.IsNotNull(dsfWebPostActivity);
            //---------------Execute Test ----------------------
            var debugInputs = dsfWebPostActivity.GetDebugInputs(environment, 0);
            //---------------Test Result -----------------------
            Assert.IsNotNull(debugInputs);
            Assert.AreEqual(4,debugInputs.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenNoHeaders_ShouldHaveTwoHeaders()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
         
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebPostActivity);
            //---------------Execute Test ----------------------
            var webClient = dsfWebPostActivity.CreateClient(null, String.Empty, new WebSource());
            //---------------Test Result -----------------------
            var actualHeaderCount = webClient.Headers.Count;
            Assert.AreEqual(1, actualHeaderCount);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CleateClient_GivenNoHeaders_ShouldHaveUserAgentHeader()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webClient = dsfWebPostActivity.CreateClient(null, String.Empty, new WebSource());
            //---------------Assert Precondition----------------
            var actualHeaderCount = webClient.Headers.Count;
            Assert.AreEqual(1, actualHeaderCount);
            //---------------Execute Test ----------------------

            var userAgentHeader = webClient.Headers.AllKeys.Single(header => header == userAgent);
            //---------------Test Result -----------------------
            Assert.IsNotNull(userAgentHeader);
            Assert.AreEqual(userAgent, userAgentHeader);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CleateClient_GivenNoHeaders_ShouldGlobalConstantsUserAgent()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webClient = dsfWebPostActivity.CreateClient(null, String.Empty, new WebSource());
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var userAgentValue = webClient.Headers[userAgent];
            //---------------Test Result -----------------------
            Assert.AreEqual(userAgentValue, GlobalConstants.UserAgentString);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenWebSourceAuthenticationTypeIsUser_ShouldSetWebClientPasswordAndUserName()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webSource = new WebSource { AuthenticationType = AuthenticationType.User, UserName = "John1", Password = "Password1"};
            

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webClient = dsfWebPostActivity.CreateClient(null, String.Empty, webSource);
            //---------------Test Result -----------------------
            Assert.IsNotNull(webClient);
            NetworkCredential networkCredentialFromWebSource = new NetworkCredential(webSource.UserName, webSource.Password);
            NetworkCredential webClientCredentials = webClient.Credentials as NetworkCredential;
            Assert.IsNotNull(webClientCredentials);
            Assert.AreEqual(webClientCredentials.UserName, networkCredentialFromWebSource.UserName);
            Assert.AreEqual(webClientCredentials.Password, networkCredentialFromWebSource.Password);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenAuthenticationTypeIsNotUser_ShouldNotSetCredentials()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };
            var webSource = new WebSource { AuthenticationType = AuthenticationType.Windows, UserName = "John1", Password = "Password1" };
            var webClient = dsfWebPostActivity.CreateClient(null, String.Empty, webSource);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webClient);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(webClient.Credentials);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenHeaders_ShouldHaveHeadersAdded()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = new TestDsfWebPostActivity
            {
                QueryString = "http://www.testing.com/[[CountryName]]",
                PostData = "This is post:[[Post]]"
            };

            var headers = new List<NameValue>
            {
                new NameValue("Content","text/json")
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebPostActivity);
            //---------------Execute Test ----------------------
            var webClient = dsfWebPostActivity.CreateClient(headers, String.Empty, new WebSource());
            //---------------Test Result -----------------------
            var actualHeaderCount = webClient.Headers.Count;
            Assert.AreEqual(2, actualHeaderCount);
            Assert.AreEqual("text/json", webClient.Headers["Content"]);
        }

    }

    public class TestDsfWebPostActivity : DsfWebPostActivity
    {
        #region Overrides of DsfWebPostActivity

        public string ResponseFromWeb { private get; set; }

        protected override string PerformWebPostRequest(IEnumerable<NameValue> head, string query, WebSource source, string postData)
        {
            Head = head;
            QueryRes = query;
            PostValue = postData;
            return ResponseFromWeb;
        }


        public string PostValue { get; private set; }

        public string QueryRes { get; private set; }

        public IEnumerable<NameValue> Head { get; private set; }

        #endregion


    }
}
