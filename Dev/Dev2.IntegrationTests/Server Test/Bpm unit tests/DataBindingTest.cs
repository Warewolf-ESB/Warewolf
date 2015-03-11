
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Integration.Tests.BPM.Unit.Test
{
    /// <summary>
    /// Summary description for DataBindingTest
    /// </summary>
    [TestClass]
    public class DataBindingTest
    {
        public DataBindingTest()
        {
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
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
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
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestCheckBoxWizardAutoCompleteTest()
        {
            string PostData = String.Format("{0}{1}?", WebserverURI, "Checkbox.wiz");
            //string expected = @"bindAutoComplete(""DevCustomDataServiceCB"", ""Dev2ServiceName"", """", ""Dev2Service"", ""ListServices"")";
            string expected = @"bindAutoComplete(";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            //string actual = TestHelper.ReturnFragment(ResponseData);
            string actual = ResponseData;

            Assert.IsTrue(actual.Contains(expected) == true);
            //Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void TestDropDownListWizardAutoCompleteTest()
        {
            string PostData = String.Format("{0}{1}?", WebserverURI, "Drop Down List.wiz");
            //string expected = @"bindAutoComplete(""DevCustomDataServiceDD"", ""Dev2ServiceName"", """", ""Dev2Service"", ""ListServices"")";
            string expected = @"bindAutoComplete(";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            //string actual = TestHelper.ReturnFragment(ResponseData);
            string actual = ResponseData;

            Assert.IsTrue(actual.Contains(expected) == true);
        }

        [TestMethod]
        public void TestRadioButtonWizardAutoCompleteTest()
        {
            string PostData = String.Format("{0}{1}?", WebserverURI, "Radio Button.wiz");
            //string expected = @"bindAutoComplete(""DevCustomDataServiceRB"", ""Dev2ServiceName"", """", ""Dev2Service"", ""ListServices"")";
            string expected = @"bindAutoComplete(";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            //string actual = TestHelper.ReturnFragment(ResponseData);
            string actual = ResponseData;

            Assert.IsTrue(actual.Contains(expected) == true);
            //Assert.AreEqual(expected, actual);
        }
    }
}
