/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Json;
using Unlimited.Framework.Converters.Graph.String.Xml;

namespace Dev2.Tests.Runtime.Util
{
    [TestClass]
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
                new StringPath(){ActualPath = "Response",DisplayPath = "Response",OutputExpression = "",SampleData = ""}, 
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
