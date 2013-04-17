using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Dev2ApplicationServer.Unit.Tests;

namespace Dev2.Integration.Tests.Application.Server.Tests {
    /// <summary>
    /// Summary description for Ghost_Service_DataList
    /// </summary>
    [TestClass]
    public class Ghost_Service_DataList {

        private readonly static string uri = ServerSettings.WebserverURI;

        public Ghost_Service_DataList() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

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

        //[TestMethod]
        //public void Mirrored_DataList_Populates_Parent() {

        //    string testURI = String.Format("{0}{1}", uri,TestResource.Ghost_Mirror_URI_Fragment);

        //    string result = TestHelper.PostDataToWebserver(testURI).Replace(" ","");
        //    string expected = TestResource.Ghost_Mirror_Expected.Replace(" ", "");

        //    Assert.IsTrue(result.Contains(expected));
        //}

        //[TestMethod]
        //public void Mirrored_DataList_MisMatched_No_Populate() {

        //    string testURI = String.Format("{0}{1}", uri, TestResource.Ghost_MisMatched_URI_Fragment);

        //    string result = TestHelper.PostDataToWebserver(testURI).Replace(" ", "");
        //    string expected = TestResource.Ghost_MisMatched_Expected.Replace(" ", "");

        //    Assert.IsTrue(result.Contains(expected));
        //}

        //[TestMethod]
        //public void Mirrored_DataList_No_DL() {

        //    string testURI = String.Format("{0}{1}", uri, TestResource.Ghost_No_DL_URI_Fragment);

        //    string result = TestHelper.PostDataToWebserver(testURI).Replace(" ", "");
        //    string expected = TestResource.Ghost_No_DL_Expected.Replace(" ", "");

        //    Assert.IsTrue(result.Contains(expected));
        //}
    }
}
