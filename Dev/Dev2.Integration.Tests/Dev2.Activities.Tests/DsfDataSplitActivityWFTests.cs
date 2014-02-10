using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfDataSplitActivityWFTests
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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

        #region DataSplit RecordSet Tests

        [TestMethod]
        public void DataSplitWorkflowWithStar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithRecordsetUsingStar");
            string expected = @"<RecCount>20</RecCount><Contacts><field>Title</field></Contacts><Contacts><field>Fname</field></Contacts><Contacts><field>LName</field></Contacts><Contacts><field>TelNo</field></Contacts><Contacts><field>
1.Mr</field></Contacts><Contacts><field>Frank</field></Contacts><Contacts><field>Williams</field></Contacts><Contacts><field>0795628443
2.Mr</field></Contacts><Contacts><field>Enzo</field></Contacts><Contacts><field>Ferrari</field></Contacts><Contacts><field>0821169853
3.Mrs</field></Contacts><Contacts><field>Jenny</field></Contacts><Contacts><field>Smith</field></Contacts><Contacts><field>0762458963
4.Ms</field></Contacts><Contacts><field>Kerrin</field></Contacts><Contacts><field>deSilvia</field></Contacts><Contacts><field>0724587310
5.Sir</field></Contacts><Contacts><field>Richard</field></Contacts><Contacts><field>Branson</field></Contacts><Contacts><field>0812457896</field></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithRecordsetWithIndex");
            string expected = @"<Contacts><field>0812457896</field></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            string notExpected = "0795628443";

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
            Assert.IsFalse(ResponseData.Contains(notExpected));
        }

        [TestMethod]
        public void DataSplitWorkflowWithNoIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithRecordsetsWithNoIndexes");
            string expected = @"<Contacts><Title>Title</Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><Contacts><Title>1.Mr</Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><Contacts><Title>2.Mr</Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><Contacts><Title>3.Mrs</Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><Contacts><Title>4.Ms</Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><Contacts><Title>5.Sir</Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithAppendingToDifferentRecordsets()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestAppendingToDifferentRecordsets");
            string expected = @"<Titles><Title>Title</Title></Titles><Titles><Title>Mr</Title></Titles><Titles><Title>Mr</Title></Titles><Titles><Title>Mrs</Title></Titles><Titles><Title>Ms</Title></Titles><Titles><Title>Sir</Title></Titles><FirstNames><FirstName>Fname</FirstName></FirstNames><FirstNames><FirstName>Frank</FirstName></FirstNames><FirstNames><FirstName>Enzo</FirstName></FirstNames><FirstNames><FirstName>Jenny</FirstName></FirstNames><FirstNames><FirstName>Kerrin</FirstName></FirstNames><FirstNames><FirstName>Richard</FirstName></FirstNames><LastNames><LastName>LName</LastName></LastNames><LastNames><LastName>Williams</LastName></LastNames><LastNames><LastName>Ferrari</LastName></LastNames><LastNames><LastName>Smith</LastName></LastNames><LastNames><LastName>deSilvia</LastName></LastNames><LastNames><LastName>Branson</LastName></LastNames><Tels><Tel>TelNo</Tel></Tels><Tels><Tel>0795628443</Tel></Tels><Tels><Tel>0821169853</Tel></Tels><Tels><Tel>0762458963</Tel></Tels><Tels><Tel>0724587310</Tel></Tels><Tels><Tel>0812457896</Tel></Tels>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithScalar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithScalar");
            string expected = @"<Contacts><Title></Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><Contacts><Title></Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><Contacts><Title></Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><Contacts><Title></Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><Contacts><Title></Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><Contacts><Title></Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts><TitleScalar>5.Sir</TitleScalar>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion DataSplit RecordSet Tests

        #region Data Split Different Split Types

        [TestMethod]
        public void DataSplitWorkflowWithAllDifferentSplits()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitAllDifferentTypesOfSplits");
            string expected = @"<Contacts><Title>0.</Title><FirstName>Title</FirstName><LastName>Fname</LastName><Tel>LName|TelNo</Tel></Contacts><Contacts><Title>1.</Title><FirstName>Mr</FirstName><LastName>Frank</LastName><Tel>Williams|0795628443</Tel></Contacts><Contacts><Title>2.</Title><FirstName>Mr</FirstName><LastName>Enzo</LastName><Tel>Ferrari|0821169853</Tel></Contacts><Contacts><Title>3.</Title><FirstName>Mrs</FirstName><LastName>Jenny</LastName><Tel>Smith|07624 58963</Tel></Contacts><Contacts><Title>4.</Title><FirstName>Ms</FirstName><LastName>Kerrin</LastName><Tel>deSilvia|0724587310</Tel></Contacts><Contacts><Title>5.</Title><FirstName>Sir</FirstName><LastName>Richard</LastName><Tel>Branson|0812457896</Tel></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithVariableInSource()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitVariableInSource");
            string expected = @"<Contacts><field>Title</field></Contacts><Contacts><field>Fname</field></Contacts><Contacts><field>LName</field></Contacts><Contacts><field>TelNo</field></Contacts><Contacts><field>
1.Mr</field></Contacts><Contacts><field>Frank</field></Contacts><Contacts><field>Williams</field></Contacts><Contacts><field>0795628443
2.Mr</field></Contacts><Contacts><field>Enzo</field></Contacts><Contacts><field>Ferrari</field></Contacts><Contacts><field>0821169853
3.Mrs</field></Contacts><Contacts><field>Jenny</field></Contacts><Contacts><field>Smith</field></Contacts><Contacts><field>0762458963
4.Ms</field></Contacts><Contacts><field>Kerrin</field></Contacts><Contacts><field>deSilvia</field></Contacts><Contacts><field>0724587310
5.Sir</field></Contacts><Contacts><field>Richard</field></Contacts><Contacts><field>Branson</field></Contacts><Contacts><field>0812457896</field></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Data Split Different Split Types
    }
}
