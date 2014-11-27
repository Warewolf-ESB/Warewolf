
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;
using System.Xml.Linq;

namespace Dev2.Integration.Tests.BPM.Unit.Test {
    /// <summary>
    /// Summary description for ListOfServices
    /// </summary>
    [TestClass]
    public class ListOfServices {
        public ListOfServices() {
            //
            // TODO: Add constructor logic here
            //
        }

        private string PostAndxmlString;
        private TestContext testContextInstance;
        private const string ListOfServicesWf = "FindResourcesService";
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize() {
            
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ListOfServices_WildCardAllInputs_Expected_AllResources() {
            string expected = "<Dev2WorkerService>LiteralControl.wiz</Dev2WorkerService>";
            PostAndxmlString = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, ListOfServicesWf, "ResourceName=*&ResourceType=*&Roles=*");
            string actual = TestHelper.PostDataToWebserver(PostAndxmlString);
            Assert.IsTrue(actual.Contains(expected));
        }

        // This test is a problem, dont know why, dont know how, but its causing the server to fallover when compiled in debug mode and running
        // through visual studio. I think it has something to do with an out of memory exception and the UnlimitedObjects XmlString
        // - Matt (2012/09/28)
        [TestMethod]
        public void ListOfServices_Workflowservices_Expected_AllWorkflowServicesReturned() {
            string expected = "<Dev2WorkerService>Date Picker</Dev2WorkerService>";
            var DataList = new DataListValueInjector();
            string workflowService = "ResourceName=*&ResourceType=WorkflowService&Roles=*";
            PostAndxmlString = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, ListOfServicesWf, workflowService);
            string actual = TestHelper.PostDataToWebserver(PostAndxmlString);
            Assert.IsTrue(actual.Contains(expected));
        }

        [TestMethod]
        public void ListOfServices_Sources_Expected_AllSourcesReturned() {
            string expected = "<Dev2SourceName>Email Plugin</Dev2SourceName>";
            var DataList = new DataListValueInjector();
            string workflowService = "ResourceName=*&ResourceType=Source&Roles=*";
            PostAndxmlString = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, ListOfServicesWf, workflowService);
            string actual = TestHelper.PostDataToWebserver(PostAndxmlString);
            Assert.IsTrue(actual.Contains(expected));
        }

        [TestMethod]
        public void ListOfServices_Services_Expected_AllWorkerServicesReturned() {
            string expected = "AuthorRoles";
            var DataList = new DataListValueInjector();
            string workflowService = "ResourceName=*&ResourceType=Service&Roles=*";
            PostAndxmlString = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, ListOfServicesWf, workflowService);
            string actual = TestHelper.PostDataToWebserver(PostAndxmlString);
            Assert.IsTrue(actual.Contains(expected));
        }

        [TestMethod]
        public void ListOfServices_RoleChange_Expected_AllServicesReturnedForSpecificRole() {
            string expected = "<Dev2WorkerService>Anything To Xml Hook Plugin</Dev2WorkerService>";
            var DataList = new DataListValueInjector();
            string workflowService = "ResourceName=*&ResourceType=*&Roles=Business Design Studio Developers";
            PostAndxmlString = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, ListOfServicesWf, workflowService);
            string actual = TestHelper.PostDataToWebserver(PostAndxmlString);
            Assert.IsTrue(actual.Contains(expected));
        }

        [TestMethod]
        public void ListOfService_SpecificResource_Expected_SingleResourceReturned() {
            string expected = "<Dev2WorkerService>ServiceToBindFrom</Dev2WorkerService>";
            var DataList = new DataListValueInjector();
            string workflowService = "ResourceName=ServiceToBindFrom&ResourceType=*&Roles=*";
            PostAndxmlString = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, ListOfServicesWf, workflowService);
            string actual = TestHelper.PostDataToWebserver(PostAndxmlString);
            Assert.IsTrue(actual.Contains("ServiceToBindFrom"));

        }
    }
}
