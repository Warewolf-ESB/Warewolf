using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.ServiceModel.Data
{

    // PBI 1220 - 2013.05.26 - TWR - Created
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebServiceTests
    {
        #region CTOR

        [TestMethod]
        public void WebServiceConstructorWithXmlWithoutActionElementExpectedDoesNotThrowException()
        {
            //------------Setup for test--------------------------
            const string XmlDataString = @"<Service Name=""Test WebService"" ID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<Actions>
	</Actions>
	<Category>System</Category>
</Service>";
            //------------Execute Test---------------------------
            var testElm = XElement.Parse(XmlDataString);
            var webService = new WebService(testElm);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test WebService", webService.ResourceName);
            Assert.AreEqual(ResourceType.WebService, webService.ResourceType);
            Assert.AreEqual("51a58300-7e9d-4927-a57b-e5d700b11b55", webService.ResourceID.ToString());
            Assert.AreEqual("System", webService.ResourcePath);
            Assert.IsNull(webService.Source);
        }

        [TestMethod]
        public void WebServiceConstructorExpectedCorrectWebService()
        {
            //------------Setup for test--------------------------
            const string XmlDataString = @"<Service Name=""Test WebService"" ID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
	<Actions>
		<Action Name=""Test_WebService"" Type=""WebService"" SourceName=""Test WebService"" SourceMethod=""Get"" JsonPath=""$.apath"">
			<Inputs>
				<Input Name=""Path"" Source=""Path"">
					<Validator Type=""Required"" />
				</Input>
				<Input Name=""Revision"" Source=""Revision"">
					<Validator Type=""Required"" />
				</Input>
				<Input Name=""Username"" Source=""Username"">
					<Validator Type=""Required"" />
				</Input>
				<Input Name=""Password"" Source=""Password"">
					<Validator Type=""Required"" />
				</Input>
			</Inputs>
			<Outputs>
				<Output Name=""error"" MapsTo=""error"" Value=""[[Error]]"" />
				<Output Name=""author"" MapsTo=""author"" Value=""[[SVNLog().Author]]"" Recordset=""result"" />
			</Outputs>
		</Action>
	</Actions>
	<Category>System</Category>
</Service>";
            //------------Execute Test---------------------------
            var testElm = XElement.Parse(XmlDataString);
            var webService = new WebService(testElm);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test WebService", webService.ResourceName);
            Assert.AreEqual(ResourceType.WebService, webService.ResourceType);
            Assert.AreEqual("51a58300-7e9d-4927-a57b-e5d700b11b55", webService.ResourceID.ToString());
            Assert.AreEqual("$.apath", webService.JsonPath);
            Assert.AreEqual("System", webService.ResourcePath);
            Assert.IsNotNull(webService.Source);
        }
        #endregion

        #region CTOR

        [TestMethod]
        public void WebServiceContructorWithDefaultExpectedInitializesProperties()
        {
            var service = new WebService();
            Assert.AreEqual(Guid.Empty, service.ResourceID);
            Assert.AreEqual(ResourceType.WebService, service.ResourceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServiceContructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new WebService(null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void WebServiceContructorWithInvalidXmlExpectedDoesNotThrowExceptionAndInitializesProperties()
        {
            var xml = new XElement("root");
            var service = new WebService(xml);
            Assert.AreNotEqual(Guid.Empty, service.ResourceID);
            Assert.IsTrue(service.IsUpgraded);
            Assert.AreEqual(ResourceType.WebService, service.ResourceType);
        }

        [TestMethod]
        public void WebServiceContructorWithValidXmlExpectedInitializesProperties()
        {
            var xml = XmlResource.Fetch("WebService");

            var service = new WebService(xml);
            VerifyEmbeddedWebService(service);
        }

        #endregion

        #region ToXml

        [TestMethod]
        public void WebServiceToXmlExpectedSerializesProperties()
        {
            var expected = new WebService
            {
                Source = new WebSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "TestWebSource",
                },
                RequestUrl = "pqr",
                RequestMethod = WebRequestMethod.Get,
                RequestHeaders = "Content-Type: text/xml",
                RequestBody = "abc",
                RequestResponse = "xyz",
                JsonPath = "$.somepath"
            };

            #region setup method parameters

            expected.Method.Parameters.AddRange(
                new[]
                {
                    new MethodParameter
                    {
                        Name = "Param1",
                        DefaultValue = "123"
                    },
                    new MethodParameter
                    {
                        Name = "Param2",
                        DefaultValue = "456"
                    }
                });

            #endregion

            #region setup rs1

            var rs1 = new Recordset
            {
                Name = "Recordset1()"
            };
            rs1.Fields.AddRange(new[]
            {
                new RecordsetField
                {
                    Name = "Field1",
                    Alias = "Alias1",
                    RecordsetAlias = "RecAlias1()"
                },
                new RecordsetField
                {
                    Name = "Field2",
                    Alias = "Alias2",
                    RecordsetAlias = "RecAlias1()"
                }
            });
            expected.Recordsets.Add(rs1);

            #endregion

            #region setup rs2

            var rs2 = new Recordset
            {
                Name = "Recordset2()"
            };
            rs2.Fields.AddRange(new[]
            {
                new RecordsetField
                {
                    Name = "Field3",
                    Alias = "Alias3"
                },
                new RecordsetField
                {
                    Name = "Field4",
                    Alias = "Alias4"
                }
            });
            expected.Recordsets.Add(rs2);

            #endregion

            var xml = expected.ToXml();

            var actual = new WebService(xml);

            Assert.AreEqual(expected.Source.ResourceType, actual.Source.ResourceType);
            Assert.AreEqual(expected.Source.ResourceID, actual.Source.ResourceID);
            Assert.AreEqual(expected.Source.ResourceName, actual.Source.ResourceName);
            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.RequestUrl, actual.RequestUrl);
            Assert.AreEqual(expected.RequestMethod, actual.RequestMethod);
            Assert.AreEqual(expected.RequestHeaders, actual.RequestHeaders);
            Assert.AreEqual(expected.RequestBody, actual.RequestBody);
            Assert.AreEqual(expected.JsonPath, actual.JsonPath);
            Assert.IsNull(actual.RequestResponse);

            foreach(var expectedParameter in expected.Method.Parameters)
            {
                MethodParameter parameter = expectedParameter;
                var actualParameter = actual.Method.Parameters.First(p => p.Name == parameter.Name);
                Assert.AreEqual(expectedParameter.DefaultValue, actualParameter.DefaultValue);
            }

            foreach(var expectedRecordset in expected.Recordsets)
            {
                // expect actual to have removed recordset notation ()...
                Recordset recordset = expectedRecordset;
                var actualRecordset = actual.Recordsets.First(rs => rs.Name == recordset.Name.Replace("()", ""));
                foreach(var expectedField in expectedRecordset.Fields)
                {
                    RecordsetField field = expectedField;
                    var actualField = actualRecordset.Fields.First(f => f.Name == field.Name);
                    Assert.AreEqual(expectedField.Alias, actualField.Alias);
                    // expect actual to have removed recordset notation ()...
                    var expectedRecordsetAlias = string.IsNullOrEmpty(expectedField.RecordsetAlias) ? string.Empty : expectedField.RecordsetAlias.Replace("()", "");
                    Assert.AreEqual(expectedRecordsetAlias, actualField.RecordsetAlias);
                }
            }
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebService_ToXml")]
        public void WebService_ToXml_WhenRequestValuesHaveEnter_ShouldBeRespectedWhenReHydrated()
        {
            //------------Setup for test--------------------------
            var expected = new WebService
            {
                Source = new WebSource
                {
                    ResourceID = Guid.NewGuid(),
                    ResourceName = "TestWebSource",
                },
                RequestUrl = "pqr",
                RequestMethod = WebRequestMethod.Get,
                RequestHeaders = "Content-Type: text/xml\nBearer: Trusted",
                RequestBody = "abc\nhas an enter\nin it",
                RequestResponse = "xyz",
                JsonPath = "$.somepath"
            };
            //------------Execute Test---------------------------
            var xml = expected.ToXml();
            var actual = new WebService(xml);
            //------------Assert Results-------------------------
            StringAssert.Contains(actual.RequestHeaders, "\n");
            StringAssert.Contains(actual.RequestBody, "\n");
        }

        #endregion


        #region Dispose

        [TestMethod]
        public void WebServiceDisposeExpectedDisposesAndNullsSource()
        {
            var service = new WebService { Source = new WebSource() };

            Assert.IsNotNull(service.Source);
            service.Dispose();
            Assert.IsNull(service.Source);
        }

        #endregion

        #region VerifyEmbeddedWebService

        public static void VerifyEmbeddedWebService(WebService service)
        {
            Assert.AreEqual(Guid.Parse("ec2df7f9-53aa-4873-a13e-4001cef21508"), service.ResourceID);
            Assert.AreEqual(ResourceType.WebService, service.ResourceType);
            Assert.AreEqual("/GetWeather?CityName=[[CityName]]&CountryName=[[CountryName]]", service.RequestUrl);
            Assert.AreEqual(WebRequestMethod.Get, service.RequestMethod);
            Assert.AreEqual(Guid.Parse("518edc28-e348-4a52-a900-f6aa75cfe92b"), service.Source.ResourceID);

            Assert.AreEqual("Paris-Aeroport Charles De Gaulle", service.Method.Parameters.First(p => p.Name == "CityName").DefaultValue);
            Assert.AreEqual("France", service.Method.Parameters.First(p => p.Name == "CountryName").DefaultValue);

            Assert.AreEqual("Location", service.Recordsets[0].Fields.First(f => f.Name == "Location").Alias);
            Assert.AreEqual("Time", service.Recordsets[0].Fields.First(f => f.Name == "Time").Alias);
            Assert.AreEqual("Wind", service.Recordsets[0].Fields.First(f => f.Name == "Wind").Alias);
            Assert.AreEqual("Visibility", service.Recordsets[0].Fields.First(f => f.Name == "Visibility").Alias);
            Assert.AreEqual("DewPoint", service.Recordsets[0].Fields.First(f => f.Name == "DewPoint").Alias);
            Assert.AreEqual("RelativeHumidity", service.Recordsets[0].Fields.First(f => f.Name == "RelativeHumidity").Alias);
            Assert.AreEqual("Pressure", service.Recordsets[0].Fields.First(f => f.Name == "Pressure").Alias);
            Assert.AreEqual("Status", service.Recordsets[0].Fields.First(f => f.Name == "Status").Alias);
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebService_ApplyPath")]
        public void WebService_ApplyPath_WhenResponseDataNull_NothingHappens()
        {
            //------------Setup for test--------------------------
            var webService = new WebService { RequestResponse = null };
            //------------Execute Test---------------------------
            webService.ApplyPath();
            //------------Assert Results-------------------------
            Assert.IsNull(webService.RequestResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebService_ApplyPath")]
        public void WebService_ApplyPath_WhenResponseDataEmpty_NothingHappens()
        {
            //------------Setup for test--------------------------
            var webService = new WebService { RequestResponse = "" };
            //------------Execute Test---------------------------
            webService.ApplyPath();
            //------------Assert Results-------------------------
            Assert.AreEqual("", webService.RequestResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebService_ApplyPath")]
        public void WebService_ApplyPath_WhenJsonPathNull_NothingHappens()
        {
            //------------Setup for test--------------------------
            var webService = new WebService();
            const string expected = "this is the response";
            webService.RequestResponse = expected;
            webService.JsonPath = null;
            //------------Execute Test---------------------------
            webService.ApplyPath();
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, webService.RequestResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebService_ApplyPath")]
        public void WebService_ApplyPath_WhenJsonPathEmpty_NothingHappens()
        {
            //------------Setup for test--------------------------
            var webService = new WebService();
            const string expected = "this is the response";
            webService.RequestResponse = expected;
            webService.JsonPath = "";
            //------------Execute Test---------------------------
            webService.ApplyPath();
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, webService.RequestResponse);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WebService_ApplyPath")]
        public void WebService_ApplyPath_ResponseDataNotJsonData_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            const string expected = "blah blah";
            var webService = new WebService { RequestResponse = expected, JsonPath = "some path" };
            //------------Execute Test---------------------------
            webService.ApplyPath();
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, webService.RequestResponse);
        }

    }
}