using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.JSON;
using Dev2.Tests.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.ServiceModel
{
    // PBI 1220 - 2013.05.27 - TWR - Created
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebServicesTest
    {
        string _requestResponse;

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Webservice_Test")]
        public void Webservice_Test_WhenRequestShouldTimeout_ExpectTimeoutMessage()
        {
            //------------Setup for test--------------------------
            var serviceXml = XmlResource.Fetch("WebService");
            var sourceXml = XmlResource.Fetch("WebSource");
            var response = JsonResource.Fetch("cryptsy_all_markets");

            var service = new WebService(serviceXml) { Source = new WebSource(sourceXml) };

            foreach(var parameter in service.Method.Parameters)
            {
                parameter.Value = parameter.DefaultValue;
            }

            var webExecuteHitCount = 0;
            var resourceCatalog = new Mock<IResourceCatalog>();
            var services = new WebServicesMock(resourceCatalog.Object,
                (WebSource source, WebRequestMethod method, string uri, string data, bool error, out ErrorResultTO errors, string[] headers) =>
                {
                    webExecuteHitCount++;
                    errors = new ErrorResultTO();
                    return response;
                });

            //------------Execute Test---------------------------
            var result = services.Test(service.ToString(), Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, webExecuteHitCount);
            Assert.AreEqual(GlobalConstants.WebServiceTimeoutMessage, result.RequestMessage);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Webservice_Test")]
        public void Webservice_Test_WhenRequestShouldNotTimeout_ExpectNoMessage()
        {
            //------------Setup for test--------------------------
            var serviceXml = XmlResource.Fetch("WebService");
            var sourceXml = XmlResource.Fetch("WebSource");
            var response = JsonResource.Fetch("empty");

            var service = new WebService(serviceXml) { Source = new WebSource(sourceXml) };

            foreach(var parameter in service.Method.Parameters)
            {
                parameter.Value = parameter.DefaultValue;
            }

            var webExecuteHitCount = 0;
            var resourceCatalog = new Mock<IResourceCatalog>();
            var services = new WebServicesMock(resourceCatalog.Object,
                (WebSource source, WebRequestMethod method, string uri, string data, bool error, out ErrorResultTO errors, string[] headers) =>
                {
                    webExecuteHitCount++;
                    errors = new ErrorResultTO();
                    return response;
                });

            //------------Execute Test---------------------------
            var result = services.Test(service.ToString(), Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, webExecuteHitCount);
            Assert.AreEqual(string.Empty, result.RequestMessage);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Webservice_ApplyPath")]
        public void Webservice_Test_WhenJsonPathSet_ExpectShapedData()
        {
            //------------Setup for test--------------------------
            var serviceXml = XmlResource.Fetch("WebService");
            var sourceXml = XmlResource.Fetch("WebSource");
            var response = JsonResource.Fetch("cryptsy_all_markets");
            var expected = JsonResource.Fetch("cryptsy_all_markets_shaped_response");

            var service = new WebService(serviceXml) { Source = new WebSource(sourceXml) };

            var services = new WebServicesMock();

            service.RequestResponse = response;
            service.JsonPath = "$.return.markets[*]";

            //------------Execute Test---------------------------
            var result = services.ApplyPath(service.ToString(), Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, result.JsonPathResult);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Webservice_ApplyPath")]
        public void Webservice_Test_WhenJsonPathNotSet_ExpectNoShapedData()
        {
            //------------Setup for test--------------------------
            var serviceXml = XmlResource.Fetch("WebService");
            var sourceXml = XmlResource.Fetch("WebSource");
            var response = JsonResource.Fetch("cryptsy_all_markets");

            var service = new WebService(serviceXml) { Source = new WebSource(sourceXml) };

            var services = new WebServicesMock();

            service.RequestResponse = response;

            //------------Execute Test---------------------------
            var result = services.ApplyPath(service.ToString(), Guid.Empty, Guid.Empty);

            //------------Assert Results-------------------------
            Assert.AreEqual(null, result.JsonPathResult);
        }


        [TestMethod]
        public void WebServicesTestWithValidArgsAndEmptyResponseExpectedExecutesRequestAndFetchesRecordset()
        {
            var serviceXml = XmlResource.Fetch("WebService");
            var sourceXml = XmlResource.Fetch("WebSource");
            var responseXml = XmlResource.Fetch("WebServiceResponse");

            var service = new WebService(serviceXml) { Source = new WebSource(sourceXml) };

            foreach(var parameter in service.Method.Parameters)
            {
                parameter.Value = parameter.DefaultValue;
            }

            var webExecuteHitCount = 0;
            var resourceCatalog = new Mock<IResourceCatalog>();
            var services = new WebServicesMock(resourceCatalog.Object,
                (WebSource source, WebRequestMethod method, string uri, string data, bool error, out ErrorResultTO errors, string[] headers) =>
                {
                    webExecuteHitCount++;
                    errors = new ErrorResultTO();
                    return responseXml.ToString();
                });
            var result = services.Test(service.ToString(), Guid.Empty, Guid.Empty);

            Assert.AreEqual(1, webExecuteHitCount);

            // BUG 9626 - 2013.06.11 - TWR: RecordsetListHelper.ToRecordsetList returns correct number of recordsets now
            Assert.AreEqual(1, result.Recordsets.Count);
            Assert.AreEqual("", result.Recordsets[0].Name);
        }

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServicesContructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            new WebServices(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServicesContructorWithNullWebExectueExpectedThrowsArgumentNullException()
        {
            new WebServices(new Mock<IResourceCatalog>().Object, null);
        }

        #endregion

        #region DeserializeService

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebServicesDeserializeServiceWithNullJsonExpectedThrowsArgumentNullException()
        {
            var services = new WebServicesMock();
            services.DeserializeService(null);
        }

        [TestMethod]
        public void WebServicesDeserializeServiceWithInvalidJsonExpectedReturnsNewWebService()
        {
            var services = new WebServicesMock();
            var result = services.DeserializeService("{'root' : 'hello' }");
            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        public void WebServicesDeserializeServiceWithValidJsonExpectedReturnsWebService()
        {
            var xml = XmlResource.Fetch("WebService");
            var service = new WebService(xml);

            var services = new WebServicesMock();
            var result = services.DeserializeService(service.ToString());

            WebServiceTests.VerifyEmbeddedWebService(result as WebService);
        }

        [TestMethod]
        public void WebServicesDeserializeServiceWithNullXmlExpectedReturnsNewWebService()
        {
            var services = new WebServicesMock();
            var result = services.DeserializeService(null, ResourceType.WebService);

            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        public void WebServicesDeserializeServiceWithValidXmlExpectedReturnsWebService()
        {
            var xml = XmlResource.Fetch("WebService");

            var services = new WebServicesMock();
            var result = services.DeserializeService(xml, ResourceType.WebService);

            WebServiceTests.VerifyEmbeddedWebService(result as WebService);
        }

        #endregion

        #region Test

        [TestMethod]
        public void WebServicesTestWithValidArgsAndNonEmptyResponseExpectedFetchesRecordsetOnly()
        {
            var serviceXml = XmlResource.Fetch("WebService");
            var sourceXml = XmlResource.Fetch("WebSource");
            var responseXml = XmlResource.Fetch("WebServiceResponse");

            var service = new WebService(serviceXml) { Source = new WebSource(sourceXml), RequestResponse = responseXml.ToString() };

            foreach(var parameter in service.Method.Parameters)
            {
                parameter.Value = parameter.DefaultValue;
            }

            var webExecuteHitCount = 0;
            var resourceCatalog = new Mock<IResourceCatalog>();
            var services = new WebServicesMock(resourceCatalog.Object,
                (WebSource source, WebRequestMethod method, string uri, string data, bool error, out ErrorResultTO errors, string[] headers) =>
                {
                    webExecuteHitCount++;
                    errors = new ErrorResultTO();
                    return string.Empty;
                });

            var result = services.Test(service.ToString(), Guid.Empty, Guid.Empty);

            Assert.AreEqual(0, webExecuteHitCount);

            // BUG 9626 - 2013.06.11 - TWR: RecordsetListHelper.ToRecordsetList returns correct number of recordsets now
            Assert.AreEqual(1, result.Recordsets.Count);
            Assert.AreEqual("", result.Recordsets[0].Name);
        }

        [TestMethod]
        public void WebServicesTestWithInValidArgsExpectedReturnsResponseWithErrorMessage()
        {
            var serviceXml = XmlResource.Fetch("WebService");

            var service = new WebService(serviceXml);

            foreach(var parameter in service.Method.Parameters)
            {
                parameter.Value = parameter.DefaultValue;
            }

            var services = new WebServicesMock();
            var result = services.Test(service.ToString(), Guid.Empty, Guid.Empty);

            Assert.AreEqual("Illegal characters in path.", result.RequestResponse);
            Assert.AreEqual(1, result.Recordsets.Count);
            Assert.IsTrue(result.Recordsets[0].HasErrors);
            Assert.AreEqual("Illegal characters in path.", result.Recordsets[0].ErrorMessage);
        }

        #endregion

        [TestMethod]
        public void WebServicesTestWithValidArgsAndRecordsetFieldsExpectedDoesNotAddRecordsetFields()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            //------------Execute Test---------------------------
            var services = new WebServicesMock();
            services.FetchRecordset(service, false);
            //------------Assert Results-------------------------
            Assert.IsFalse(services.FetchRecordsetAddFields);
        }

        [TestMethod]
        public void WebServicesTestWithValidArgsAndNoRecordsetFieldsExpectedAddsRecordsetFields()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            //service.Recordset.Fields.Clear();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var services = new WebServicesMock();
            services.FetchRecordset(service, true);
            Assert.IsTrue(services.FetchRecordsetAddFields);
        }

        [TestMethod]
        public void WebServicesTestWithValidArgsExpectedFetchesRecordset()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            //------------Execute Test---------------------------
            var services = new WebServicesMock();
            services.FetchRecordset(service, true);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, services.FetchRecordsetHitCount);
        }

        [TestMethod]
        public void WebServicesTestWithValidArgsExpectedClearsRecordsFirst()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            //------------Execute Test---------------------------
            var services = new WebServicesMock();
            services.FetchRecordset(service, true);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, services.FetchRecordsetHitCount);
        }

        [TestMethod]
        public void OuputDescriptionWhereRequestResponseJSONExpectValidOutputDescription()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            service.RequestResponse = "{\"created_at\":\"Mon Jul 16 21:09:33 +0000 2012\",\"id\":224974074361806848,\"id_str\":\"224974074361806848\",\"text\":\"It works! Many thanks @NeoCat\",\"source\":\"web\",\"truncated\":false," +
                "\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":634794199,\"id_str\":\"634794199\"},\"geo\":null,\"coordinates\":null," +
                "\"place\":null,\"contributors\":null,\"retweet_count\":0,\"favorite_count\":0,\"favorited\":false,\"retweeted\":false,\"lang\":\"en\"}";
            //------------Execute Test---------------------------
            var outputDescription = service.GetOutputDescription();
            //------------Assert Results-------------------------
            Assert.IsNotNull(outputDescription);
            Assert.AreEqual(22, outputDescription.DataSourceShapes[0].Paths.Count);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Services_FetchRecordsetList")]
        public void Services_FetchRecordsetList_WhenWebserviceWithJsonDataAndPrimitiveArrayType_ShouldNotIncludeInFieldsList()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            service.RequestResponse = "{\"results\" : [{\"address_components\": [{\"long_name\" :\"Address:\",\"short_name\" :\"Address:\",\"types\" : [\"point_of_interest\",\"establishment\" ]}]}],\"status\" : \"OK\"}";
            //------------Execute Test---------------------------
            WebServices services = new WebServices();
            var result = services.FetchRecordset(service, true);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Services_Execute")]
        public void Services_Execute_WhenHasJsonPath_ShouldReturnValid()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            service.JsonPath = "$.results[*]";
            _requestResponse = "{\"results\" : [{\"address_components\": [{\"long_name\" :\"Address:\",\"short_name\" :\"Address:\",\"types\" : [\"point_of_interest\",\"establishment\" ]}]}],\"status\" : \"OK\"}";
            service.RequestResponse = _requestResponse;
            //------------Execute Test---------------------------
            ErrorResultTO errors;
            WebServices.ExecuteRequest(service, false, out errors, DummyWebExecute);
            //------------Assert Results-------------------------
            Assert.AreEqual("[{\"address_components\":[{\"long_name\":\"Address:\",\"short_name\":\"Address:\",\"types\":[\"point_of_interest\",\"establishment\"]}]}]", service.RequestResponse);
        }

        string DummyWebExecute(WebSource source, WebRequestMethod method, string relativeUri, string data, bool throwError, out ErrorResultTO errors, string[] headers)
        {
            errors = new ErrorResultTO();
            return _requestResponse;
        }

        [TestMethod]
        public void FetchRecordsetWhereRequestResponseJSONExpectValidOutputDescription()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            service.RequestResponse = "{\"created_at\":\"Mon Jul 16 21:09:33 +0000 2012\",\"id\":224974074361806848,\"id_str\":\"224974074361806848\",\"text\":\"It works! Many thanks @NeoCat\",\"source\":\"web\",\"truncated\":false," +
                "\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":634794199,\"id_str\":\"634794199\"},\"geo\":null,\"coordinates\":null," +
                "\"place\":null,\"contributors\":null,\"retweet_count\":0,\"favorite_count\":0,\"favorited\":false,\"retweeted\":false,\"lang\":\"en\"}";
            //------------Execute Test---------------------------
            WebServices services = new WebServices();
            var result = services.FetchRecordset(service, true);
            //------------Assert Results-------------------------
            // BUG 9626 - 2013.06.11 - TWR: RecordsetListHelper.ToRecordsetList returns correct number of recordsets now
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void FetchRecordsetWhereRequestResponseXml2ExpectValidOutputDescription()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            service.RequestResponse = "<?xml version=\"1.0\" encoding=\"utf-8\"?><string xmlns=\"http://www.webserviceX.NET\"><NewDataSet><Table><Country>France</Country><City>Le Touquet</City></Table><Table><Country>France</Country><City>Agen</City></Table><Table><Country>France</Country><City>Cazaux</City></Table><Table><Country>France</Country><City>Bordeaux / Merignac</City></Table><Table><Country>France</Country><City>Bergerac</City></Table><Table><Country>France</Country><City>Toulouse / Francazal</City></Table><Table><Country>France</Country><City>Cognac</City></Table><Table><Country>France</Country><City>La Rochelle</City></Table><Table><Country>France</Country><City>Poitiers</City></Table><Table><Country>France</Country><City>Montlucon / Gueret</City></Table><Table><Country>France</Country><City>Limoges</City></Table><Table><Country>France</Country><City>Mont-De-Marsan</City></Table><Table><Country>France</Country><City>Niort</City></Table><Table><Country>France</Country><City>Toulouse / Blagnac</City></Table><Table><Country>France</Country><City>Pau</City></Table><Table><Country>France</Country><City>Biscarosse</City></Table><Table><Country>France</Country><City>Tarbes / Ossun</City></Table><Table><Country>France</Country><City>Brive</City></Table><Table><Country>France</Country><City>Perigueux</City></Table><Table><Country>France</Country><City>Dax</City></Table><Table><Country>France</Country><City>Biarritz</City></Table><Table><Country>France</Country><City>St-Girons</City></Table><Table><Country>France</Country><City>Albi</City></Table><Table><Country>France</Country><City>Rodez</City></Table><Table><Country>France</Country><City>Auch</City></Table><Table><Country>France</Country><City>Suippes Range Met</City></Table><Table><Country>France</Country><City>Le Puy</City></Table><Table><Country>France</Country><City>Cassagnes-Begonhes</City></Table><Table><Country>France</Country><City>Metz-Nancy-Lorraine</City></Table><Table><Country>France</Country><City>Bastia</City></Table><Table><Country>France</Country><City>Calvi</City></Table><Table><Country>France</Country><City>Figari</City></Table><Table><Country>France</Country><City>Ajaccio</City></Table><Table><Country>France</Country><City>Solenzara</City></Table><Table><Country>France</Country><City>Auxerre</City></Table><Table><Country>France</Country><City>Chambery / Aix-Les-Bains</City></Table><Table><Country>France</Country><City>Clermont-Ferrand</City></Table><Table><Country>France</Country><City>Bourges</City></Table><Table><Country>France</Country><City>Lyon / Satolas</City></Table><Table><Country>France</Country><City>Macon</City></Table><Table><Country>France</Country><City>Saint-Yan</City></Table><Table><Country>France</Country><City>Montelimar</City></Table><Table><Country>France</Country><City>Grenoble / St. Geoirs</City></Table><Table><Country>France</Country><City>Vichy</City></Table><Table><Country>France</Country><City>Aurillac</City></Table><Table><Country>France</Country><City>Chateauroux</City></Table><Table><Country>France</Country><City>Lyon / Bron</City></Table><Table><Country>France</Country><City>Aix Les Milles</City></Table><Table><Country>France</Country><City>Le Luc</City></Table><Table><Country>France</Country><City>Cannes</City></Table><Table><Country>France</Country><City>Nimes / Courbessac</City></Table><Table><Country>France</Country><City>St-Etienne Boutheon</City></Table><Table><Country>France</Country><City>Istres</City></Table><Table><Country>France</Country><City>Carcassonne</City></Table><Table><Country>France</Country><City>Marseille / Marignane</City></Table><Table><Country>France</Country><City>Nice</City></Table><Table><Country>France</Country><City>Orange</City></Table><Table><Country>France</Country><City>Perpignan</City></Table><Table><Country>France</Country><City>Montpellier</City></Table><Table><Country>France</Country><City>Beziers / Vias</City></Table><Table><Country>France</Country><City>St-Auban-Sur-Durance</City></Table><Table><Country>France</Country><City>Salon</City></Table><Table><Country>France</Country><City>Mende / Brenoux</City></Table><Table><Country>France</Country><City>Beauvais</City></Table><Table><Country>France</Country><City>Chateaudun</City></Table><Table><Country>France</Country><City>Evreux</City></Table><Table><Country>France</Country><City>Alencon</City></Table><Table><Country>France</Country><City>La Heve</City></Table><Table><Country>France</Country><City>Abbeville</City></Table><Table><Country>France</Country><City>Orleans</City></Table><Table><Country>France</Country><City>Rouen</City></Table><Table><Country>France</Country><City>Chartres</City></Table><Table><Country>France</Country><City>Vittefleur / St. Vale</City></Table><Table><Country>France</Country><City>Tours</City></Table><Table><Country>France</Country><City>Saint-Quentin</City></Table><Table><Country>France</Country><City>Paris / Le Bourget</City></Table><Table><Country>France</Country><City>Creil Fafb</City></Table><Table><Country>France</Country><City>Paris-Aeroport Charles De Gaulle</City></Table><Table><Country>France</Country><City>Melun</City></Table><Table><Country>France</Country><City>Toussus Le Noble</City></Table><Table><Country>France</Country><City>Paris-Orly</City></Table><Table><Country>France</Country><City>Villacoublay</City></Table><Table><Country>France</Country><City>Paris Met Center</City></Table><Table><Country>France</Country><City>Troyes</City></Table><Table><Country>France</Country><City>Nevers</City></Table><Table><Country>France</Country><City>Chatillon-Sur-Seine</City></Table><Table><Country>France</Country><City>Cambrai</City></Table><Table><Country>France</Country><City>Lille</City></Table><Table><Country>France</Country><City>Charleville</City></Table><Table><Country>France</Country><City>Angers</City></Table><Table><Country>France</Country><City>Brest</City></Table><Table><Country>France</Country><City>Cherbourg / Maupertus</City></Table><Table><Country>France</Country><City>Dinard</City></Table><Table><Country>France</Country><City>Lann Bihoue</City></Table><Table><Country>France</Country><City>La Roche-Sur-Yon</City></Table><Table><Country>France</Country><City>Landivisiau</City></Table><Table><Country>France</Country><City>Caen</City></Table><Table><Country>France</Country><City>Lanveoc Poulmic</City></Table><Table><Country>France</Country><City>Le Mans</City></Table><Table><Country>France</Country><City>Rennes</City></Table><Table><Country>France</Country><City>Lannion / Servel</City></Table><Table><Country>France</Country><City>Quimper</City></Table><Table><Country>France</Country><City>Nantes</City></Table><Table><Country>France</Country><City>Saint-Brieuc</City></Table><Table><Country>France</Country><City>Morlaix / Ploujean</City></Table><Table><Country>France</Country><City>St-Nazaire</City></Table><Table><Country>France</Country><City>Besancon</City></Table><Table><Country>France</Country><City>Bale-Mulhouse</City></Table><Table><Country>France</Country><City>Colmar</City></Table><Table><Country>France</Country><City>Dijon</City></Table><Table><Country>France</Country><City>Metz / Frescaty</City></Table><Table><Country>France</Country><City>St-Dizier</City></Table><Table><Country>France</Country><City>Toul / Rosieres</City></Table><Table><Country>France</Country><City>Nancy / Essey</City></Table><Table><Country>France</Country><City>Nancy / Ochey</City></Table><Table><Country>France</Country><City>Belfort</City></Table><Table><Country>France</Country><City>Reims</City></Table><Table><Country>France</Country><City>Strasbourg</City></Table><Table><Country>France</Country><City>Luxeuil</City></Table><Table><Country>France</Country><City>Hyeres</City></Table><Table><Country>France</Country><City>St-Raphael</City></Table><Table><Country>France</Country><City>Nimes / Garons</City></Table><Table><Country>France</Country><City>Amberieu</City></Table><Table><Country>France</Country><City>Apt / Saint Christol</City></Table><Table><Country>France</Country><City>Romorantin</City></Table><Table><Country>France</Country><City>Maopoopo Ile Futuna</City></Table><Table><Country>France</Country><City>Hihifo Ile Wallis</City></Table></NewDataSet></string>";
            //------------Execute Test---------------------------
            WebServices services = new WebServices();
            var result = services.FetchRecordset(service, true);
            //------------Assert Results-------------------------
            // BUG 9626 - 2013.06.11 - TWR: RecordsetListHelper.ToRecordsetList returns correct number of recordsets now
            Assert.AreEqual(1, result.Count);
        }


        [TestMethod]
        public void FetchRecordsetWhereRequestResponseXMLExpectValidOutputDescription()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            service.RequestResponse = "<statuses type=\"array\">" +
                "<status>" +
                "<created_at>Mon Jul 16 21:09:33 +0000 2012</created_at>" +
                "<id>224974074361806848</id>" +
                "<text>It works! Many thanks @NeoCat</text>" +
                "<source>web</source>" +
                "<truncated>false</truncated>" +
                "<in_reply_to_status_id/>" +
                "<in_reply_to_user_id/>" +
                "<in_reply_to_screen_name/>" +
                "<user>" +
                "<id>634794199</id>" +
                "</user>" +
                "<geo/>" +
                "<coordinates/>" +
                "<place/>" +
                "<contributors/>" +
                "<retweet_count>0</retweet_count>" +
                "<favorite_count>0</favorite_count>" +
                "<favorited>false</favorited>" +
                "<retweeted>false</retweeted>" +
                "</status>" +
                "</statuses>";
            //------------Execute Test---------------------------
            WebServices services = new WebServices();
            var result = services.FetchRecordset(service, true);
            //------------Assert Results-------------------------
            // BUG 9626 - 2013.06.11 - TWR: RecordsetListHelper.ToRecordsetList returns correct number of recordsets now
            Assert.AreEqual(1, result.Count);
        }


        [TestMethod]
        public void OuputDescriptionWhereRequestResponseXMLExpectValidOutputDescription()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyWebService();
            service.RequestResponse = "<statuses type=\"array\">" +
                "<status>" +
                "<created_at>Mon Jul 16 21:09:33 +0000 2012</created_at>" +
                "<id>224974074361806848</id>" +
                "<text>It works! Many thanks @NeoCat</text>" +
                "<source>web</source>" +
                "<truncated>false</truncated>" +
                "<in_reply_to_status_id/>" +
                "<in_reply_to_user_id/>" +
                "<in_reply_to_screen_name/>" +
                "<user>" +
                "<id>634794199</id>" +
                "</user>" +
                "<geo/>" +
                "<coordinates/>" +
                "<place/>" +
                "<contributors/>" +
                "<retweet_count>0</retweet_count>" +
                "<favorite_count>0</favorite_count>" +
                "<favorited>false</favorited>" +
                "<retweeted>false</retweeted>" +
                "</status>" +
                "</statuses>";
            //------------Execute Test---------------------------
            var outputDescription = service.GetOutputDescription();
            //------------Assert Results-------------------------
            Assert.IsNotNull(outputDescription);
            Assert.AreEqual(18, outputDescription.DataSourceShapes[0].Paths.Count);

        }

        WebService CreateDummyWebService()
        {
            return new WebService
            {
                ResourceID = Guid.NewGuid(),

                ResourceName = "DummyWebService",
                ResourceType = ResourceType.WebService,
                ResourcePath = "Tests"

            };
        }

    }
}
