using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Interfaces;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Dev2.Tests.Runtime.Util
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public partial class ScrubberTests
    {
        #region Scrub Xml

        [TestMethod]
        public void ScrubberScrubXmlWithValidFormatExpectedGeneratesValidOutputDescription()
        {
            var expectedPaths = CreateCurrentWeatherExpectedPaths();
            VerifyScrub(XmlResource.Fetch("Bug9519_1").ToString(), expectedPaths);
        }

        [TestMethod]
        public void ScrubberScrubXmlWithNamespacesExpectedGeneratesValidOutputDescription()
        {
            var expectedPaths = CreateCurrentWeatherExpectedPaths();
            expectedPaths.AddRange(new[]
            {
                new XmlPath("string.CurrentWeather:type", "string.CurrentWeather:type", "", "Weather"),
            });

            VerifyScrub(XmlResource.Fetch("Bug9519_2").ToString(), expectedPaths);
        }

        [TestMethod]
        public void ScrubberScrubXmlWithSoapEnvelopeExpectedGeneratesValidOutputDescription()
        {
            var expectedPaths = new List<IPath>(new[]
            {
                new XmlPath("Envelope.Body.Fault.faultcode", "Envelope.Body.Fault.faultcode", "", "soap:Client"),
                new XmlPath("Envelope.Body.Fault.faultstring", "Envelope.Body.Fault.faultstring", "", "System.Web.Services.Protocols.SoapException: Server was unable to read request."),
                new XmlPath("Envelope.Body.Fault.detail", "Envelope.Body.Fault.detail", "", ""),
            });
            VerifyScrub(XmlResource.Fetch("Bug9519_3").ToString(), expectedPaths);
        }

        [TestMethod]
        public void ScrubberScrubXmlWithSapWsdlExpectedGeneratesValidOutputDescription()
        {
            var expectedPaths = new List<IPath>(new[]
            {
                new XmlPath("definitions:targetNamespace", "definitions:targetNamespace", "", "urn:sap-com:document:sap:rfc:functions"),
                new XmlPath("definitions.types.schema:targetNamespace", "definitions.types.schema:targetNamespace", "", "urn:sap-com:document:sap:rfc:functions"),
                new XmlPath("definitions.types.schema().element:name", "definitions.types.schema().element:name", "", "STFC_CONNECTION,STFC_CONNECTION.Response"),
                new XmlPath("definitions.types.schema().element.complexType.sequence.element:name", "definitions.types.schema().element.complexType.sequence.element:name", "", "REQUTEXT,ECHOTEXT"),
                new XmlPath("definitions.types.schema().element.complexType.sequence.element.simpleType.restriction:base", "definitions.types.schema().element.complexType.sequence.element.simpleType.restriction:base", "", "xsd:string,xsd:string"),
                new XmlPath("definitions.types.schema().element.complexType.sequence.element.simpleType.restriction.length:value", "definitions.types.schema().element.complexType.sequence.element.simpleType.restriction.length:value", "", "255,255"),
                new XmlPath("definitions.types.schema().element.complexType.sequence().element:name", "definitions.types.schema.element.complexType.sequence().element:name", "", "REQUTEXT,ECHOTEXT,RESPTEXT"),
                new XmlPath("definitions.types.schema().element.complexType.sequence().element.simpleType.restriction:base", "definitions.types.schema.element.complexType.sequence().element.simpleType.restriction:base", "", "xsd:string,xsd:string,xsd:string"),
                new XmlPath("definitions.types.schema().element.complexType.sequence().element.simpleType.restriction.length:value", "definitions.types.schema.element.complexType.sequence().element.simpleType.restriction.length:value", "", "255,255,255"),
                new XmlPath("definitions().message:name", "definitions().message:name", "", "STFC_CONNECTIONInput,STFC_CONNECTIONOutput"),
                new XmlPath("definitions().message.part:name", "definitions().message.part:name", "", "parameters,parameters"),
                new XmlPath("definitions().message.part:element", "definitions().message.part:element", "", "s0:STFC_CONNECTION,s0:STFC_CONNECTION.Response"),
                new XmlPath("definitions.portType:name", "definitions.portType:name", "", "STFC_CONNECTIONPortType"),
                new XmlPath("definitions.portType.operation:name", "definitions.portType.operation:name", "", "STFC_CONNECTION"),
                new XmlPath("definitions.portType.operation.input:message", "definitions.portType.operation.input:message", "", "s0:STFC_CONNECTIONInput"),
                new XmlPath("definitions.portType.operation.output:message", "definitions.portType.operation.output:message", "", "s0:STFC_CONNECTIONOutput"),
                new XmlPath("definitions.binding:name", "definitions.binding:name", "", "STFC_CONNECTIONBinding"),
                new XmlPath("definitions.binding:type", "definitions.binding:type", "", "s0:STFC_CONNECTIONPortType"),
                new XmlPath("definitions.binding.binding:style", "definitions.binding.binding:style", "", "document"),
                new XmlPath("definitions.binding.binding:transport", "definitions.binding.binding:transport", "", "http://schemas.xmlsoap.org/soap/http"),
                new XmlPath("definitions.binding.operation:name", "definitions.binding.operation:name", "", "STFC_CONNECTION"),
                new XmlPath("definitions.binding.operation.operation:soapAction", "definitions.binding.operation.operation:soapAction", "", "http://www.sap.com/STFC_CONNECTION"),
                new XmlPath("definitions.binding.operation.input.body:use", "definitions.binding.operation.input.body:use", "", "literal"),
                new XmlPath("definitions.binding.operation.output.body:use", "definitions.binding.operation.output.body:use", "", "literal"),
                new XmlPath("definitions.service:name", "definitions.service:name", "", "STFC_CONNECTIONService"),
                new XmlPath("definitions.service.documentation", "definitions.service.documentation", "", "SAP Service STFC_CONNECTION via SOAP"),
                new XmlPath("definitions.service.port:name", "definitions.service.port:name", "", "STFC_CONNECTIONPortType"),
                new XmlPath("definitions.service.port:binding", "definitions.service.port:binding", "", "s0:STFC_CONNECTIONBinding"),
                new XmlPath("definitions.service.port.address:location", "definitions.service.port.address:location", "", "http://binmain:8080/sap/bc/soap/rfc"),
            });
            VerifyScrub(XmlResource.Fetch("Bug9519_4").ToString(), expectedPaths);
        }

        [TestMethod]
        public void ScrubberScrubXmlWithAttributesExpectedGeneratesValidOutputDescription()
        {
            var expectedPaths = CreateCurrentWeatherExpectedPaths();
            expectedPaths.AddRange(new[]
            {
                new XmlPath("string:id", "string:id", "", "123"),
                new XmlPath("string:name", "string:name", "", "Nice"),
                new XmlPath("string.CurrentWeather:type", "string.CurrentWeather:type", "", "Weather"),
            });
            VerifyScrub(XmlResource.Fetch("Bug9519_5").ToString(), expectedPaths);
        }

        [TestMethod]
        public void ScrubberScrubXmlWithInvalidXmlDeclarationExpectedGeneratesValidOutputDescription()
        {
            const string Response = "<?xml version=\"1.0\" encoding=\"utf-16\"?><string xmlns=\"http://www.webserviceX.NET\"><?xml version=\"1.0\" encoding=\"utf-16\"?><CurrentWeather>Sunny Skies</CurrentWeather></string>";
            var expectedPaths = new List<IPath>(new[]
            {
                new XmlPath("string.CurrentWeather", "string.CurrentWeather", "", "Sunny Skies")
            });
            VerifyScrub(Response, expectedPaths);
        }

        [TestMethod]
        public void ScrubberScrubXmlWithMalformedXmlExpectedGeneratesErrorOutputDescription()
        {
            const string Response = "<string><CurrentWeather>Sunny Skies</string>";
            var expectedPaths = new List<IPath>(new[]
            {
                new XmlPath("Error", "Error", "", "System.Exception: Couldn't create a mapper for '<string><CurrentWeather>Sunny Skies</string>'")
            });
            VerifyScrub(Response, expectedPaths);
        }

        #endregion

        #region CreateCurrentWeatherExpectedPaths

        static List<IPath> CreateCurrentWeatherExpectedPaths()
        {
            var expectedPaths = new List<IPath>(new[]
            {
                new XmlPath("string.CurrentWeather.Location", "string.CurrentWeather.Location", "", "Nice__COMMA__ France"),
                new XmlPath("string.CurrentWeather.Temperature", "string.CurrentWeather.Temperature", "", "16 C"),
                new XmlPath("string.CurrentWeather.RelativeHumidity", "string.CurrentWeather.RelativeHumidity", "", " 55%"),
                new XmlPath("string.CurrentWeather.Status", "string.CurrentWeather.Status", "", "Success"),
            });
            return expectedPaths;
        }

        #endregion

        #region VerifyScrub

        static void VerifyScrub(string requestResponse, IList<IPath> expectedPaths)
        {
            var webService = new WebService { RequestResponse = requestResponse };
            var outputDescription = webService.GetOutputDescription();

            Assert.AreEqual(1, outputDescription.DataSourceShapes.Count);

            foreach(var shape in outputDescription.DataSourceShapes)
            {
                Assert.IsNotNull(shape);
                Assert.AreEqual(expectedPaths.Count, shape.Paths.Count);

                foreach(var actualPath in shape.Paths)
                {
                    var expectedPath = expectedPaths.FirstOrDefault(p => p.ActualPath == actualPath.ActualPath);
                    Assert.IsNotNull(expectedPath);

                    Assert.AreEqual(expectedPath.DisplayPath, actualPath.DisplayPath);
                    Assert.AreEqual(expectedPath.OutputExpression, actualPath.OutputExpression);
                    Assert.IsTrue(actualPath.SampleData.StartsWith(expectedPath.SampleData));
                }
            }
        }

        #endregion



    }
}
