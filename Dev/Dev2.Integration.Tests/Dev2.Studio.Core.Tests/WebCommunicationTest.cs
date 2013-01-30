using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Dev2.Integration.Tests;
using System.Xml.Linq;

namespace Dev2.Studio.Core.Tests {
    [TestClass]
    public class WebCommunicationTest {

        private string webserverURI = ServerSettings.WebserverURI;

        [Export(typeof(CompositionContainer))]
        public CompositionContainer Container { get; set; }

        [Export(typeof(IWebCommunicationResponse))]
        public IWebCommunicationResponse WebCommunicationResponse {
            get {
                return _webCommResp;
            }
            set {
                _webCommResp = (WebCommunicationResponse)value;
            }
        }
        WebCommunication _webRequest;
        WebCommunicationResponse _webCommResp;
        //WebCommunicationResponseFactory _webResponseFactory;

        [TestInitialize]
        public void TestInitialize() {
            _webRequest = new WebCommunication();
            _webCommResp = new WebCommunicationResponse();
            //_webResponseFactory = new WebCommunicationResponseFactory();
            AssemblyCatalog catalog = new AssemblyCatalog(Assembly.GetAssembly(typeof(IWebCommunication)));
            Container = new CompositionContainer(catalog);

            try {
                Container.ComposeParts(this);
                Container.ComposeExportedValue<IWebCommunicationResponse>("IWebCommunicationResponse", _webCommResp);
            }
            catch (Exception ex) {
                Assert.Fail("Composition Error");
            }

            //_webResponseFactory.Container = Container;
            //_webRequest.WebCommunicationResponseFactory = _webResponseFactory;
        }

        [TestMethod]
        public void GetTest_ExpectedReturnedServiceInformation() {
            _webCommResp.Content = "tabIndex";
            var uri = String.Format("{0}{1}", webserverURI, "TabIndexInject"); 
            IWebCommunicationResponse actual = _webRequest.Get(uri);
            XElement serializedActual = XElement.Parse(actual.Content);
            System.Collections.ICollection actualFinding = serializedActual.Descendants().Where(c => c.Name == "tabIndex").ToList();
            CollectionAssert.AllItemsAreNotNull(actualFinding);
        }

        [TestMethod]
        public void PostTest_ExpectedReturnedDataWithPostData() {
            _webCommResp.Content = "tabIndex";
            var uri = String.Format("{0}{1}", webserverURI, "TabIndexInject"); 
            var data = "Dev2tabIndex=1";
            IWebCommunicationResponse actual = _webRequest.Post(uri, data);
            IWebCommunicationResponse expected = _webCommResp;
            XElement serializedActual = XElement.Parse(actual.Content);
            XElement actualFinding = serializedActual.Descendants().First(c => c.Name == "tabIndex");

            Assert.AreNotEqual(string.Empty, actualFinding.Value);
        }
    }
}
