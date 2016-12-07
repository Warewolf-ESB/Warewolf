using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Activities;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class WebResponseManagerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenOutputDescriptionANdMapping_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var newResponseManager = new ResponseManager()
            {
                OutputDescription = It.IsAny<IOutputDescription>()
                ,
                Outputs = It.IsAny<ICollection<IServiceOutputMapping>>()
            };
            //---------------Test Result -----------------------
            Assert.IsNotNull(newResponseManager, "Cannot create new ResponseManager object.");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UnescapeRawXml_GivenEscapedXml_ShouldUnescapeXml()
        {
            //---------------Set up test pack-------------------
            var responseManager = new ResponseManager()
            {
                OutputDescription = It.IsAny<IOutputDescription>()
                    ,
                Outputs = It.IsAny<ICollection<IServiceOutputMapping>>()
            };
            StringBuilder value = new StringBuilder("&lt;x&gt;this &quot; is&apos; &amp; neat&lt;/x&gt;");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(value.ToString());
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            var result = responseManager.UnescapeRawXml(value.ToString());
            //---------------Test Result -----------------------
            Assert.AreEqual("<x>this \" is' & neat</x>", result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UnescapeRawXml_GivenUnEscapedXml_ShouldUnescapeXml()
        {
            //---------------Set up test pack-------------------
            var responseManager = new ResponseManager()
            {
                OutputDescription = It.IsAny<IOutputDescription>()
                    ,
                Outputs = It.IsAny<ICollection<IServiceOutputMapping>>()
            };
            StringBuilder value = new StringBuilder("x&gt;this &quot; is&apos; &amp; neat&lt;/x&gt;");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(value.ToString());
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            var result = responseManager.UnescapeRawXml(value.ToString());
            //---------------Test Result -----------------------
            Assert.AreEqual(value.ToString(), result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PushResponseIntoEnvironment_GivenNoDataObject_ShouldCatchObjectNullException()
        {
            //---------------Set up test pack-------------------
            var responseManager = new ResponseManager()
            {
                OutputDescription = It.IsAny<IOutputDescription>()
                    ,
                Outputs = It.IsAny<ICollection<IServiceOutputMapping>>()
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            responseManager.PushResponseIntoEnvironment(string.Empty, 1, null);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushResponseIntoEnvironment_GivenResponse_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------

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

            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };

            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = response };
            var outPutDesc = service.GetOutputDescription();
            var responseManager = new ResponseManager()
            {
                OutputDescription = outPutDesc,
                Outputs = serviceOutputs
            };
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            try
            {
                responseManager.PushResponseIntoEnvironment(response, 0, dataObjectMock.Object);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushResponseIntoEnvironment_GivenResponseAndIsJosn_ShouldAssignJsonObjects()
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

            var responseManager = new ResponseManager()
            {
                IsObject = true,
                ObjectName = "[[@weather]]"
            };
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            try
            {
                responseManager.PushResponseIntoEnvironment(response, 0, dataObjectMock.Object);
                var evalRes = environment.Eval("[[@weather]]", 0);
                Assert.IsNotNull(evalRes);
                var stringResult = CommonFunctions.evalResultToString(evalRes);
                Assert.AreEqual(response.Replace(" ", ""), stringResult.Replace(Environment.NewLine, "").Replace(" ", ""));
                var propRes = environment.Eval("[[@weather.RelativeHumidity]]", 0);
                Assert.IsNotNull(propRes);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushXmlIntoEnvironment_GivenNull_ShouldLoggError()
        {
            //---------------Set up test pack-------------------

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
            var serviceOutputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Location", "[[weather().Location]]", "weather"), new ServiceOutputMapping("Time", "[[weather().Time]]", "weather"), new ServiceOutputMapping("Wind", "[[weather().Wind]]", "weather"), new ServiceOutputMapping("Visibility", "[[Visibility]]", "") };

            const string errorWasThrown = "Error was Thrown";
            var mockoutPutDesc = new Mock<IOutputDescription>();
            mockoutPutDesc.SetupGet(description => description.DataSourceShapes).Throws(new Exception(errorWasThrown));
            var responseManager = new ResponseManager()
            {
                OutputDescription = mockoutPutDesc.Object
                ,
                Outputs = serviceOutputs
            };
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            dataObjectMock.Setup(o => o.Environment.AddError(errorWasThrown));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            responseManager.PushResponseIntoEnvironment(response, 0, dataObjectMock.Object);
            dataObjectMock.Verify(o => o.Environment.AddError(errorWasThrown));


            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushResponseIntoEnvironment_GivenNoResponse_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            var mockoutPutDesc = new Mock<IOutputDescription>();
            var service = new Mock<WebService>();
            var outPutDesc = service.Object.GetOutputDescription();
            mockoutPutDesc.SetupGet(description => description.DataSourceShapes);
            var responseManager = new ResponseManager()
            {
                OutputDescription = outPutDesc,
                Outputs = new List<IServiceOutputMapping>()
            };
            var environment = new ExecutionEnvironment();
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            responseManager.PushResponseIntoEnvironment(string.Empty, 1, dataObjectMock.Object);
            //---------------Test Result -----------------------
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushResponseIntoEnvironment_GivenNoResponse_ShouldNotAssignOutputs()
        {
            //---------------Set up test pack-------------------
            var mockoutPutDesc = new Mock<IOutputDescription>();
            var serviceXml = XmlResource.Fetch("WebService");
            var service = new WebService(serviceXml) { RequestResponse = string.Empty };
            var outPutDesc = service.GetOutputDescription();
            mockoutPutDesc.SetupGet(description => description.DataSourceShapes);
            var responseManager = new ResponseManager()
            {
                OutputDescription = outPutDesc,
                Outputs = new List<IServiceOutputMapping>()
            };
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            env.Setup(environment => environment.AddError(It.IsAny<string>()));
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(env.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            responseManager.PushResponseIntoEnvironment(string.Empty, 1, dataObjectMock.Object);
            //---------------Test Result -----------------------
            env.Verify(environment => environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            env.Verify(environment => environment.AddError(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushResponseIntoEnvironment_GivenNoResponse_ShouldNotAaddError()
        {
            //---------------Set up test pack-------------------
            var mockoutPutDesc = new Mock<IOutputDescription>();
            mockoutPutDesc.SetupGet(description => description.DataSourceShapes).Returns(new List<IDataSourceShape>());
            var responseManager = new ResponseManager()
            {
                OutputDescription = mockoutPutDesc.Object,
                Outputs = new List<IServiceOutputMapping> {  }
            };
            var env = new Mock<IExecutionEnvironment>();
            env.Setup(environment => environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            env.Setup(environment => environment.AddError(It.IsAny<string>()));
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(env.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(responseManager);
            //---------------Execute Test ----------------------
            responseManager.PushResponseIntoEnvironment(string.Empty, 1, dataObjectMock.Object);
            //---------------Test Result -----------------------
            env.Verify(environment => environment.Assign(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            env.Verify(environment => environment.AddError(It.IsAny<string>()), Times.Never);
        }
    }
}
