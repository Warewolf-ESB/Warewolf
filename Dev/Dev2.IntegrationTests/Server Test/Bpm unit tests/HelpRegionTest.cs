
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
//using Dev2ApplicationServer.Unit.Tests;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.BPM.Unit.Test {
    /// <summary>
    /// Summary description for HelpRegion
    /// </summary>
    [TestClass]
    public class HelpRegion {



        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();

        public HelpRegion() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        string WebserverURI = ServerSettings.WebserverURI;

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

            tempXmlString = string.Empty;

            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.HelpRegion_DataList, TestResource.Xpath_Fragment, "testHelpRegionFragment");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2helpPartName", "testHelpRegionHelpPartName");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2helpText", "testHelpRegionHelpText");
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2helpTitle", "testHelpRegionHelpText");

        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod()]
        public void Helpregion_ExpectedUserInteraction_Helptext_Expected_FunctionGeneratedForRegionSpecified() {
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.HelpRegion, tempXmlString);
            string expected = @"<script>$('#'+'testHelpRegionHelpPartName').mouseover(function(){vartxt='testHelpRegionHelpText';displayHelp(""testHelpRegionHelpText"",txt);});</script>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = TestHelper.CleanUp(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Helpregion_ExpectedUserInteraction_Without_LinkedPartName_Expected_HelpRegionCreatedLinkedToWebpart() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2helpPartName", "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.HelpRegion, tempXmlString);
            string expected = @"<script>$('#'+'').mouseover(function(){vartxt='testHelpRegionHelpText';displayHelp(""testHelpRegionHelpText"",txt);});</script>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = TestHelper.CleanUp(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Helpregion_ExpectedUserInteraction_Without_HelpTitle_Expected_HelpRegionGeneratedWithoutATitle() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2helpTitle", "");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.HelpRegion, tempXmlString);
            string expected = @"<script>$('#'+'testHelpRegionHelpPartName').mouseover(function(){vartxt='testHelpRegionHelpText';displayHelp("""",txt);});</script>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = TestHelper.CleanUp(actual);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Helpregion_ExpectedUserInteraction_Without_HelpText_Expected_HelpRegionGenerateWithoutHelpText() {
            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, "Dev2helpText", "testHelpRegionHelpText");
            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.HelpRegion, tempXmlString);
            string expected = @"<script>$('#'+'testHelpRegionHelpPartName').mouseover(function(){vartxt='testHelpRegionHelpText';displayHelp(""testHelpRegionHelpText"",txt);});</script>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string actual = TestHelper.ReturnFragment(ResponseData);
            actual = TestHelper.CleanUp(actual);

            Assert.AreEqual(expected, actual);
        }
    }
}
