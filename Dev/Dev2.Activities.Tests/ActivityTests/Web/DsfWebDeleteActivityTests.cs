using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Toolbox;
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
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DsfWebDeleteActivityTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        
        public void DsfWebDeleteActivity_GivenInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dsfWebDeleteActivity);
        }

        private static TestDsfWebDeleteActivity CreateTestDeleteActivity()
        {
            var testDsfWebDeleteActivity = new TestDsfWebDeleteActivity();
            testDsfWebDeleteActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            return testDsfWebDeleteActivity;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfWebDeleteActivity_GivenIsCreated_ShouldBeDsfActivity()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebDeleteActivity);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsInstanceOfType(dsfWebDeleteActivity, typeof(DsfActivity));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfWebDeleteActivity_GivenNewActivity_ShouldHaveCustomAttribute()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(dsfWebDeleteActivity, typeof(DsfActivity));
            //---------------Execute Test ----------------------
            var toolDescAtribute = dsfWebDeleteActivity.GetType().GetCustomAttributes(true).Single(o => o.GetType() == typeof(ToolDescriptorInfo));
            //---------------Test Result -----------------------
            Assert.IsNotNull(toolDescAtribute);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfWebDeleteActivity_GivenNewActivity_ShouldHaveCorrectAttributeValues()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            var toolDescAtribute = dsfWebDeleteActivity.GetType().GetCustomAttributes(true).Single(o => o.GetType() == typeof(ToolDescriptorInfo)) as ToolDescriptorInfo;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(toolDescAtribute);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(toolDescAtribute);
            Assert.AreEqual("DELETE", toolDescAtribute.Name);
            Assert.AreEqual(ToolType.Native, toolDescAtribute.ToolType);
            Assert.AreEqual(new Guid("6C5F6D7E-4B42-4874-8197-DBE68D4A9F2D"), toolDescAtribute.Id);
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DsfWebDeleteActivity_GivenIsNew_ShouldHaveDisplayNameSet()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebDeleteActivity.DisplayName);
            //---------------Execute Test ---------------------
            //---------------Test Result -----------------------
            Assert.AreEqual("DELETE Web Method", dsfWebDeleteActivity.DisplayName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructer_GivenHasInstance_ShouldHaveType()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebDeleteActivity);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(dsfWebDeleteActivity.Type);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenDeletePostActivity_ShouldReturnMissingTypeDataGridAcitvity()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebDeleteActivity.Type);
            Assert.IsNotNull(dsfWebDeleteActivity);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, dsfWebDeleteActivity.GetFindMissingType());
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldReturnZeroDebugInputs()
        {
            //---------------Set up test pack-------------------
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = dsfWebDeleteActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfWebDeleteActivity_Execute")]
        public void DsfWebDeleteActivity_Delete_WithValidWebResponse_ShouldSetVariables()
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
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebDeleteActivity.Inputs = serviceInputs;
            dsfWebDeleteActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebDeleteActivity.OutputDescription = service.GetOutputDescription();
            dsfWebDeleteActivity.ResponseFromWeb = response;
            dsfWebDeleteActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebDeleteActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebDeleteActivity.QueryString = "";
            //dsfWebPostActivity.PostData = "";
            dsfWebDeleteActivity.SourceId = Guid.Empty;
            dsfWebDeleteActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebDeleteActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebDeleteActivity.OutputDescription);
            Assert.AreEqual("greater than 7 mile(s):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0)));
            Assert.AreEqual("Paris", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0)));
            Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0)));
            Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0)));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfWebDeleteActivity_Execute")]
        public void DsfWebDeleteActivity_Delete_WithTextResponseValidResponseReturned()
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
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Response", "[[Response]]", "") };
            dsfWebDeleteActivity.Inputs = serviceInputs;
            dsfWebDeleteActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            // ReSharper disable once ObjectCreationAsStatement
            new WebService(serviceXml) { RequestResponse = response };
            dsfWebDeleteActivity.OutputDescription = new OutputDescription();
            dsfWebDeleteActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            dsfWebDeleteActivity.OutputDescription.DataSourceShapes.Add(new DataSourceShape { Paths = new List<IPath>() { new StringPath() { ActualPath = "[[Response]]", OutputExpression = "[[Response]]" } } });
            dsfWebDeleteActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebDeleteActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebDeleteActivity.QueryString = "";
            //dsfWebPostActivity.PostData = "";
            dsfWebDeleteActivity.SourceId = Guid.Empty;
            dsfWebDeleteActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebDeleteActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebDeleteActivity.OutputDescription);
            Assert.AreEqual(response, ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Response]]", 0)));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfWebDeleteActivity_Execute")]
        public void DsfWebDeleteActivity_Execute_WithInValidWebResponse_ShouldNotError()
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
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            dsfWebDeleteActivity.Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") };
            dsfWebDeleteActivity.QueryString = "http://www.testing.com/[[CountryName]]";
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };

            dsfWebDeleteActivity.Inputs = serviceInputs;
            dsfWebDeleteActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebDeleteActivity.OutputDescription = service.GetOutputDescription();
            //dsfWebPostActivity.ResponseFromWeb = invalidResponse;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebDeleteActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebDeleteActivity.QueryString = "";
            //dsfWebPostActivity.PostData = "";
            dsfWebDeleteActivity.SourceId = Guid.Empty;
            dsfWebDeleteActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebDeleteActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebDeleteActivity.OutputDescription);
            Assert.AreEqual(0, environment.Errors.Count);
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfWebDeleteActivity_Execute")]
        public void DsfWebDeleteActivity_Execute_WithValidXmlEscaped_ShouldSetVariables()
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
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            var serviceInputs = new List<IServiceInput> { new ServiceInput("CityName", "[[City]]"), new ServiceInput("Country", "[[CountryName]]") };
            var serviceOutputs = new List<IServiceOutputMapping>
            {
                  new ServiceOutputMapping("Location", "[[weather().Location]]", "weather")
                , new ServiceOutputMapping("Time", "[[weather().Time]]", "weather")
                , new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather")
                , new ServiceOutputMapping("Visibility", "[[Visibility]]", "")
            };
            dsfWebDeleteActivity.Inputs = serviceInputs;
            dsfWebDeleteActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebDeleteActivity.OutputDescription = service.GetOutputDescription();
            dsfWebDeleteActivity.ResponseFromWeb = response;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebDeleteActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            dsfWebDeleteActivity.QueryString = "";
            //dsfWebPostActivity.PostData = "";
            dsfWebDeleteActivity.SourceId = Guid.Empty;
            dsfWebDeleteActivity.Headers = new List<INameValue>();
            //------------Execute Test---------------------------
            dsfWebDeleteActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfWebDeleteActivity.OutputDescription);
            var visibility = ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[Visibility]]", 0));
            Assert.AreEqual("<greater than 7 mile(s):0>", visibility);
            var location = ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Location]]", 0));
            Assert.AreEqual("<Paris>", location);
            var time = ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Time]]", 0));
            Assert.AreEqual("May 29, 2013 - 09:00 AM EDT / 2013.05.29 1300 UTC", time);
            var wind = ExecutionEnvironment.WarewolfEvalResultToString(environment.Eval("[[weather().Wind]]", 0));
            Assert.AreEqual("from the NW (320 degrees) at 10 MPH (9 KT) (direction variable):0", wind);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfWebDeleteActivity_Execute")]
        public void DsfWebDeleteActivity_Execute_WithInputVariables_ShouldEvalVariablesBeforeExecutingWebRequest()
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
            var dsfWebDeleteActivity = CreateTestDeleteActivity();
            dsfWebDeleteActivity.Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") };
            dsfWebDeleteActivity.QueryString = "http://www.testing.com/[[CountryName]]";
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };
            dsfWebDeleteActivity.Outputs = serviceOutputs;
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            dsfWebDeleteActivity.OutputDescription = service.GetOutputDescription();
            dsfWebDeleteActivity.ResponseFromWeb = response;
            dsfWebDeleteActivity.ResourceCatalog = new Mock<IResourceCatalog>().Object;
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dsfWebDeleteActivity.ResourceID = InArgument<Guid>.FromValue(Guid.Empty);
            //------------Assert Preconditions-------------------
            Assert.AreEqual(1, dsfWebDeleteActivity.Headers.Count);
            Assert.AreEqual("Header 1", dsfWebDeleteActivity.Headers.ToList()[0].Name);
            Assert.AreEqual("[[City]]", dsfWebDeleteActivity.Headers.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/[[CountryName]]", dsfWebDeleteActivity.QueryString);
            //------------Execute Test---------------------------
            dsfWebDeleteActivity.Execute(dataObjectMock.Object, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual("PMB", dsfWebDeleteActivity.Head.ToList()[0].Value);
            Assert.AreEqual("http://www.testing.com/South Africa", dsfWebDeleteActivity.QueryRes);
        }

    }

    public class TestDsfWebDeleteActivity : DsfWebDeleteActivity
    {
        #region Overrides of DsfWebPostActivity

        public string ResponseFromWeb { private get; set; }

        protected override string PerformWebPostRequest(IEnumerable<NameValue> head, string query, WebSource source, string putData)
        {
            Head = head;
            QueryRes = query;
            return ResponseFromWeb;
        }

        public string QueryRes { get; private set; }

        public IEnumerable<NameValue> Head { get; private set; }

        #endregion


    }
}


