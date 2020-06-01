using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Activities.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Storage;


namespace Dev2.Tests.Activities.ActivityTests.Web
{
    //"This Test class tests WebBaseActivity using a Delete implemantation as an example "    
    [TestClass]

    public class DsfWebBaseActivityTests
    {
        const string userAgent1 = "Mozilla/4.0";
        const string userAgent2 = "(compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfWebActivity_GivenDeleteInstance_ShouldNotBenull()
        {
            //---------------Set up test pack-------------------
            var dsfWebActivity = CreateWebDeleteActivityFromBase();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(dsfWebActivity);
        }

        static TestDsfWebBaseActivity CreateWebDeleteActivityFromBase()
        {
            return
                new TestDsfWebBaseActivity(WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Delete,
                    "Web Post Delete Tool", "Web Post Delete Tool"));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenRequestMethod_ShouldHaveClient()
        {
            //---------------Set up test pack-------------------
            var dsfWebActivity = CreateWebDeleteActivityFromBase();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebActivity);
            //---------------Execute Test ----------------------
            var httpClient = dsfWebActivity.CreateClient(new List<INameValue>(new[] { new NameValue("a", "b") }),
                Tests.TestUtils.ExampleURL, new WebSource());

            //---------------Test Result -----------------------
            Assert.IsNotNull(httpClient);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenWrongUri_ShouldReturnClientWithNoAdress()
        {
            //---------------Set up test pack-------------------
            var dsfWebActivity = CreateWebDeleteActivityFromBase();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebActivity);
            //---------------Execute Test ----------------------
            var httpClient = dsfWebActivity.CreateClient(new List<INameValue>(new[] { new NameValue("a", "b") }), "Wrong.com.", new WebSource() { Address = "Wrong.com." });
            //---------------Test Result -----------------------
            Assert.IsNull(httpClient.BaseAddress);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Constructer_GivenHasInstance_ShouldHaveType()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebDeleteActivityFromBase();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(deleteActivityFromBase);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(deleteActivityFromBase.Type);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void GetFindMissingType_GivenWebPostActivity_ShouldReturnMissingTypeDataGridAcitvity()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebDeleteActivityFromBase();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(deleteActivityFromBase.Type);
            Assert.IsNotNull(deleteActivityFromBase);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, deleteActivityFromBase.GetFindMissingType());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void GetDebugInputs_GivenEnvironmentIsNull_ShouldReturnZeroDebugInputs()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = CreateWebDeleteActivityFromBase();
            dsfWebPostActivity.Headers = new List<INameValue>();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var debugInputs = dsfWebPostActivity.GetDebugInputs(null, 0);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, debugInputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenNoHeaders_ShouldHaveTwoHeaders()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = CreateWebDeleteActivityFromBase();

            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfWebPostActivity);
            //---------------Execute Test ----------------------
            var httpClient = dsfWebPostActivity.CreateClient(null, String.Empty, CreateTestWebSource());
            //---------------Test Result -----------------------
            var actualHeaderCount = httpClient.DefaultRequestHeaders.Count();
            Assert.AreEqual(2, actualHeaderCount);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CleateClient_GivenNoHeaders_ShouldGlobalConstantsUserAgent()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebDeleteActivityFromBase();
            var httpClient = deleteActivityFromBase.CreateClient(null, String.Empty, CreateTestWebSource());
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var userAgentCollection = httpClient.DefaultRequestHeaders.UserAgent;
            var count = userAgentCollection.Count;
            //---------------Test Result -----------------------
            Assert.AreEqual(2, count);
            Assert.AreEqual(userAgent1, userAgentCollection.First().ToString());
            Assert.AreEqual(userAgent2, userAgentCollection.Last().ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenWebSourceAuthenticationTypeIsUser_ShouldSetWebClientPasswordAndUserName()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebDeleteActivityFromBase();
            var webSource = CreateTestWebSource();

            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var httpClient = deleteActivityFromBase.CreateClient(null, String.Empty, webSource);
            //---------------Test Result -----------------------
            Assert.IsNotNull(httpClient);


            var webClientCredentials = httpClient.DefaultRequestHeaders.Authorization;
            Assert.IsNotNull(webClientCredentials);
            //Assert.AreEqual(webClientCredentials.Parameter, networkCredentialFromWebSource.UserName);
            //Assert.AreEqual(webClientCredentials.Password, networkCredentialFromWebSource.Password);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenAuthenticationTypeIsNotUser_ShouldNotSetCredentials()
        {
            //---------------Set up test pack-------------------
            var dsfWebPostActivity = CreateWebDeleteActivityFromBase();

            var httpClient = dsfWebPostActivity.CreateClient(null, String.Empty, CreateWebSourceWithAnonymousAuthentication());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(httpClient);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(httpClient.DefaultRequestHeaders.Authorization);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateClient_GivenHeaders_ShouldHaveHeadersAdded()
        {
            //---------------Set up test pack-------------------
            var deleteActivityFromBase = CreateWebDeleteActivityFromBase();

            var headers = new List<INameValue>
            {
                new NameValue("Content", "text/json")
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(deleteActivityFromBase);
            //---------------Execute Test ----------------------
            var httpClient = deleteActivityFromBase.CreateClient(headers, String.Empty, CreateTestWebSource());
            //---------------Test Result -----------------------
            var actualHeaderCount = httpClient.DefaultRequestHeaders.Count();
            Assert.AreEqual(3, actualHeaderCount);
            var allContentValues = httpClient.DefaultRequestHeaders.Single(pair => pair.Key == "Content").Value;
            Assert.AreEqual("text/json", allContentValues.ToList()[0]);
        }

        [TestMethod]
        [Timeout(60000)]
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
            var dsfWebPostActivity = CreateWebDeleteActivityFromBase();
            dsfWebPostActivity.Headers = new List<INameValue> { new NameValue("Header 1", "[[City]]") };
            dsfWebPostActivity.QueryString = "http://www.testing.com/[[CountryName]]";
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
            Assert.AreEqual(3, debugInputs.Count);
        }
        static WebSource CreateTestWebSource()
        {
            return new WebSource
            {
                Password = "PasJun1",
                UserName = "User1",
                AuthenticationType = AuthenticationType.User,
                Address = Tests.TestUtils.ExampleURL
            };
        }

        static WebSource CreateWebSourceWithAnonymousAuthentication()
        {
            return new WebSource()
            {
                Password = "PasJun1",
                UserName = "User1",
                AuthenticationType = AuthenticationType.Anonymous,
                Address = Tests.TestUtils.ExampleURL
            };
        }
    }

    public class TestDsfWebBaseActivity : DsfWebActivityBase
    {
        #region Overrides of DsfWebPostActivity

        public string ResponseFromWeb { private get; set; }


        public TestDsfWebBaseActivity(WebRequestDataDto webRequestDataDto)
            : base(webRequestDataDto)
        {
        }

        protected override string PerformWebRequest(IEnumerable<INameValue> head, string query, WebSource source,
            string putData)
        {
            return ResponseFromWeb;
        }

        #endregion
    }
}