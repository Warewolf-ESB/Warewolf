using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Activities;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
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
    [ExcludeFromCodeCoverage]
    public class WebWebXmlConvertTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenOutputDescriptionANdMapping_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new WebXmlConvert(It.IsAny<IOutputDescription>(), It.IsAny<ICollection<IServiceOutputMapping>>());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UnescapeRawXml_GivenEscapedXml_ShouldUnescapeXml()
        {
            //---------------Set up test pack-------------------
            var webXmlConvert = new WebXmlConvert(It.IsAny<IOutputDescription>(), It.IsAny<ICollection<IServiceOutputMapping>>());
            StringBuilder value = new StringBuilder("&lt;x&gt;this &quot; is&apos; &amp; neat&lt;/x&gt;");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(value.ToString());
            Assert.IsNotNull(webXmlConvert);
            //---------------Execute Test ----------------------
            var result = webXmlConvert.UnescapeRawXml(value.ToString());
            //---------------Test Result -----------------------
            Assert.AreEqual("<x>this \" is' & neat</x>", result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UnescapeRawXml_GivenUnEscapedXml_ShouldUnescapeXml()
        {
            //---------------Set up test pack-------------------
            var webXmlConvert = new WebXmlConvert(It.IsAny<IOutputDescription>(), It.IsAny<ICollection<IServiceOutputMapping>>());
            StringBuilder value = new StringBuilder("x&gt;this &quot; is&apos; &amp; neat&lt;/x&gt;");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(value.ToString());
            Assert.IsNotNull(webXmlConvert);
            //---------------Execute Test ----------------------
            var result = webXmlConvert.UnescapeRawXml(value.ToString());
            //---------------Test Result -----------------------
            Assert.AreEqual(value.ToString(), result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PushXmlIntoEnvironment_GivenNoDataObject_ShouldCatchObjectNullException()
        {
            //---------------Set up test pack-------------------
            var webXmlConvert = new WebXmlConvert(It.IsAny<IOutputDescription>(), It.IsAny<ICollection<IServiceOutputMapping>>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webXmlConvert);
            //---------------Execute Test ----------------------
            webXmlConvert.PushXmlIntoEnvironment(string.Empty, 1, null);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushXmlIntoEnvironment_GivenResponse_ShouldNotThrowException()
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
            var webXmlConvert = new WebXmlConvert(outPutDesc, serviceOutputs);
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(webXmlConvert);
            //---------------Execute Test ----------------------
            try
            {
                webXmlConvert.PushXmlIntoEnvironment(response, 0, dataObjectMock.Object);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PushXmlIntoEnvironment_GivenNoResponse_ShouldAddEnvironmentError()
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
            var webXmlConvert = new WebXmlConvert(outPutDesc, serviceOutputs);
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);
            dataObjectMock.Setup(o => o.Environment.AddError("No Web Response received"));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webXmlConvert);
            //---------------Execute Test ----------------------
            try
            {
                webXmlConvert.PushXmlIntoEnvironment(string.Empty, 0, dataObjectMock.Object);
                dataObjectMock.Verify(o => o.Environment.AddError("No Web Response received"));
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
            var webXmlConvert = new WebXmlConvert(mockoutPutDesc.Object, serviceOutputs);
            var dataObjectMock = new Mock<IDSFDataObject>();
            dataObjectMock.Setup(o => o.Environment).Returns(environment);
            dataObjectMock.Setup(o => o.EsbChannel).Returns(new Mock<IEsbChannel>().Object);

            dataObjectMock.Setup(o => o.Environment.AddError(errorWasThrown));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webXmlConvert);
            //---------------Execute Test ----------------------
            webXmlConvert.PushXmlIntoEnvironment(response, 0, dataObjectMock.Object);
            dataObjectMock.Verify(o => o.Environment.AddError(errorWasThrown));


            //---------------Test Result -----------------------
        }
    }
}
