using System;
using System.Activities.Statements;
using System.IO;
using System.Text;
using System.Xml.Linq;
using ActivityUnitTests.XML;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Execution;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class DsfActivityTests : BaseActivityUnitTest
    {
        public DsfActivityTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

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

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        // ReSharper disable InconsistentNaming
        public void DsfActivity_Get_Debug_Input_Output_With_All_Notation_Expected_Pass()
        // ReSharper restore InconsistentNaming
        {
            DsfActivity act = new DsfActivity { InputMapping = ActivityStrings.DsfActivityInputMapping, OutputMapping = ActivityStrings.DsfActivityOutputMapping };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckPathOperationActivityDebugInputOutput(act, @"<ADL><scalar></scalar><Numeric><num></num></Numeric><CompanyName></CompanyName><Customer><FirstName></FirstName></Customer></ADL>",
                                                                "<ADL><scalar>scalarData</scalar><Numeric><num>1</num></Numeric><Numeric><num>2</num></Numeric><Numeric><num>3</num></Numeric><Numeric><num>4</num></Numeric><CompanyName>Dev2</CompanyName><Customer><FirstName>Wallis</FirstName></Customer></ADL>", out inRes, out outRes);

            Assert.AreEqual(5, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("innerrecset(*).innerrec", inRes[0].FetchResultsList()[0].Value);
            Assert.AreEqual("[[scalar]]", inRes[0].FetchResultsList()[1].Value);
            Assert.AreEqual("=", inRes[0].FetchResultsList()[2].Value);
            Assert.AreEqual("scalarData", inRes[0].FetchResultsList()[3].Value);
            Assert.AreEqual(4, inRes[1].FetchResultsList().Count);
            Assert.AreEqual("innerrecset(*).innerrec2", inRes[1].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(1).num]]", inRes[1].FetchResultsList()[1].Value);
            Assert.AreEqual("=", inRes[1].FetchResultsList()[2].Value);
            Assert.AreEqual("1", inRes[1].FetchResultsList()[3].Value);
            Assert.AreEqual(4, inRes[2].FetchResultsList().Count);
            Assert.AreEqual("innerrecset(*).innerdate", inRes[2].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(2).num]]", inRes[2].FetchResultsList()[1].Value);
            Assert.AreEqual("=", inRes[2].FetchResultsList()[2].Value);
            Assert.AreEqual("2", inRes[2].FetchResultsList()[3].Value);
            Assert.AreEqual(4, inRes[3].FetchResultsList().Count);
            Assert.AreEqual("innertesting(*).innertest", inRes[3].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(3).num]]", inRes[3].FetchResultsList()[1].Value);
            Assert.AreEqual("=", inRes[3].FetchResultsList()[2].Value);
            Assert.AreEqual("3", inRes[3].FetchResultsList()[3].Value);
            Assert.AreEqual(4, inRes[4].FetchResultsList().Count);
            Assert.AreEqual("innerScalar", inRes[4].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(4).num]]", inRes[4].FetchResultsList()[1].Value);
            Assert.AreEqual("=", inRes[4].FetchResultsList()[2].Value);
            Assert.AreEqual("4", inRes[4].FetchResultsList()[3].Value);

            Assert.AreEqual(5, outRes.Count);
            Assert.AreEqual(4, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("innerrec", outRes[0].FetchResultsList()[0].Value);
            Assert.AreEqual("[[CompanyName]]", outRes[0].FetchResultsList()[1].Value);
            Assert.AreEqual("=", outRes[0].FetchResultsList()[2].Value);
            Assert.AreEqual("Dev2", outRes[0].FetchResultsList()[3].Value);
            Assert.AreEqual(4, outRes[1].FetchResultsList().Count);
            Assert.AreEqual("innerrec2", outRes[1].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(2).num]]", outRes[1].FetchResultsList()[1].Value);
            Assert.AreEqual("=", outRes[1].FetchResultsList()[2].Value);
            Assert.AreEqual("2", outRes[1].FetchResultsList()[3].Value);
            Assert.AreEqual(4, outRes[2].FetchResultsList().Count);
            Assert.AreEqual("innerdate", outRes[2].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(1).num]]", outRes[2].FetchResultsList()[1].Value);
            Assert.AreEqual("=", outRes[2].FetchResultsList()[2].Value);
            Assert.AreEqual("1", outRes[2].FetchResultsList()[3].Value);
            Assert.AreEqual(4, outRes[3].FetchResultsList().Count);
            Assert.AreEqual("innertest", outRes[3].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Numeric(3).num]]", outRes[3].FetchResultsList()[1].Value);
            Assert.AreEqual("=", outRes[3].FetchResultsList()[2].Value);
            Assert.AreEqual("3", outRes[3].FetchResultsList()[3].Value);
            Assert.AreEqual(4, outRes[4].FetchResultsList().Count);
            Assert.AreEqual("innerScalar", outRes[4].FetchResultsList()[0].Value);
            Assert.AreEqual("[[Customer(1).FirstName]]", outRes[4].FetchResultsList()[1].Value);
            Assert.AreEqual("=", outRes[4].FetchResultsList()[2].Value);
            Assert.AreEqual("Wallis", outRes[4].FetchResultsList()[3].Value);

        }

        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDatabaseActivity()
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
