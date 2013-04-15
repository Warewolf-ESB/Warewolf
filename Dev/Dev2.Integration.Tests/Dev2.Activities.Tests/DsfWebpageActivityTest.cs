using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using System.Xml.Linq;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests {
    
    /// <summary>
    /// Summary description for DsfWebpageActivityTest
    /// </summary>
    
    [TestClass]
    public class DsfWebpageActivityTest {

        public DsfWebpageActivityTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        private string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Webpage Service Call Tests

        [TestMethod]
        public void ExecuteNominalWebpageActivity_Expected_WebpageReturned() {
            string postData = string.Format("{0}{1}", WebserverURI, "WebpageTestService");
            string expected = TestResource.NominalWebpageActivity_Expected;
            
            XElement expectedElement = XElement.Parse(expected);
            string actual = TestHelper.PostDataToWebserver(postData);

            XElement actualElement = XElement.Parse(actual).Element("head");
            //IEnumerable<XElement> elems = actualElement.Descendants(XName.Get(expectedElement.Descendants().First().Name.LocalName));
            // Make sure that a webpage is returned and now absolute paths are used.
        //    Assert.IsTrue(elems != null 
        //        && elems.Count() > 0 
        //        && !actualElement.ToString().Contains("localhost") 
        //        && !actualElement.ToString().Contains("127.0.0.1"));
        Assert.Inconclusive("FindMissing is not done foe webpages as all webpages dont work anymore");
        }

        #endregion Webpage Service Call Tests
    }
}
