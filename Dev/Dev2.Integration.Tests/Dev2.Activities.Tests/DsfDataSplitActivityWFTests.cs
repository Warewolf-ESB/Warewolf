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
            string expected = @"<RecCount>20</RecCount><Contactsindex=""1""><field>Title</field></Contacts><Contactsindex=""2""><field>Fname</field></Contacts><Contactsindex=""3""><field>LName</field></Contacts><Contactsindex=""4""><field>TelNo</field></Contacts><Contactsindex=""5""><field>1.Mr</field></Contacts><Contactsindex=""6""><field>Frank</field></Contacts><Contactsindex=""7""><field>Williams</field></Contacts><Contactsindex=""8""><field>07956284432.Mr</field></Contacts><Contactsindex=""9""><field>Enzo</field></Contacts><Contactsindex=""10""><field>Ferrari</field></Contacts><Contactsindex=""11""><field>08211698533.Mrs</field></Contacts><Contactsindex=""12""><field>Jenny</field></Contacts><Contactsindex=""13""><field>Smith</field></Contacts><Contactsindex=""14""><field>07624589634.Ms</field></Contacts><Contactsindex=""15""><field>Kerrin</field></Contacts><Contactsindex=""16""><field>deSilvia</field></Contacts><Contactsindex=""17""><field>07245873105.Sir</field></Contacts><Contactsindex=""18""><field>Richard</field></Contacts><Contactsindex=""19""><field>Branson</field></Contacts><Contactsindex=""20""><field>0812457896</field></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithRecordsetWithIndex");
            string expected = @"<Contacts index=""5""><field>0812457896</field></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            const string notExpected = "0795628443";

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
            Assert.IsFalse(ResponseData.Contains(notExpected));
        }

        [TestMethod]
        public void DataSplitWorkflowWithNoIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithRecordsetsWithNoIndexes");
            string expected = @"<Contactsindex=""1""><Title>Title</Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><Contactsindex=""2""><Title>1.Mr</Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><Contactsindex=""3""><Title>2.Mr</Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><Contactsindex=""4""><Title>3.Mrs</Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><Contactsindex=""5""><Title>4.Ms</Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><Contactsindex=""6""><Title>5.Sir</Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithAppendingToDifferentRecordsets()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestAppendingToDifferentRecordsets");
            string expected = @"<Titlesindex=""1""><Title>Title</Title></Titles><Titlesindex=""2""><Title>Mr</Title></Titles><Titlesindex=""3""><Title>Mr</Title></Titles><Titlesindex=""4""><Title>Mrs</Title></Titles><Titlesindex=""5""><Title>Ms</Title></Titles><Titlesindex=""6""><Title>Sir</Title></Titles><FirstNamesindex=""1""><FirstName>Fname</FirstName></FirstNames><FirstNamesindex=""2""><FirstName>Frank</FirstName></FirstNames><FirstNamesindex=""3""><FirstName>Enzo</FirstName></FirstNames><FirstNamesindex=""4""><FirstName>Jenny</FirstName></FirstNames><FirstNamesindex=""5""><FirstName>Kerrin</FirstName></FirstNames><FirstNamesindex=""6""><FirstName>Richard</FirstName></FirstNames><LastNamesindex=""1""><LastName>LName</LastName></LastNames><LastNamesindex=""2""><LastName>Williams</LastName></LastNames><LastNamesindex=""3""><LastName>Ferrari</LastName></LastNames><LastNamesindex=""4""><LastName>Smith</LastName></LastNames><LastNamesindex=""5""><LastName>deSilvia</LastName></LastNames><LastNamesindex=""6""><LastName>Branson</LastName></LastNames><Telsindex=""1""><Tel>TelNo</Tel></Tels><Telsindex=""2""><Tel>0795628443</Tel></Tels><Telsindex=""3""><Tel>0821169853</Tel></Tels><Telsindex=""4""><Tel>0762458963</Tel></Tels><Telsindex=""5""><Tel>0724587310</Tel></Tels><Telsindex=""6""><Tel>0812457896</Tel></Tels>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithScalar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithScalar");
            string expected = @"<Contactsindex=""1""><Title></Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><Contactsindex=""2""><Title></Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><Contactsindex=""3""><Title></Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><Contactsindex=""4""><Title></Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><Contactsindex=""5""><Title></Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><Contactsindex=""6""><Title></Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts><TitleScalar>5.Sir</TitleScalar>";

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
            string expected = @"<Contacts index=""1""><Title>0.</Title><FirstName>Title</FirstName><LastName>Fname</LastName><Tel>LName|TelNo</Tel></Contacts><Contacts index=""2""><Title>1.</Title><FirstName>Mr</FirstName><LastName>Frank</LastName><Tel>Williams|0795628443</Tel></Contacts><Contacts index=""3""><Title>2.</Title><FirstName>Mr</FirstName><LastName>Enzo</LastName><Tel>Ferrari|0821169853</Tel></Contacts><Contacts index=""4""><Title>3.</Title><FirstName>Mrs</FirstName><LastName>Jenny</LastName><Tel>Smith|07624 58963</Tel></Contacts><Contacts index=""5""><Title>4.</Title><FirstName>Ms</FirstName><LastName>Kerrin</LastName><Tel>deSilvia|0724587310</Tel></Contacts><Contacts index=""6""><Title>5.</Title><FirstName>Sir</FirstName><LastName>Richard</LastName><Tel>Branson|0812457896</Tel></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithVariableInSource()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitVariableInSource");
            string expected = @"<SourceStringVar>Title|Fname|LName|TelNo|1.Mr|Frank|Williams|07956284432.Mr|Enzo|Ferrari|08211698533.Mrs|Jenny|Smith|07624589634.Ms|Kerrin|deSilvia|07245873105.Sir|Richard|Branson|0812457896</SourceStringVar><Contactsindex=""1""><field>Title</field></Contacts><Contactsindex=""2""><field>Fname</field></Contacts><Contactsindex=""3""><field>LName</field></Contacts><Contactsindex=""4""><field>TelNo</field></Contacts><Contactsindex=""5""><field>1.Mr</field></Contacts><Contactsindex=""6""><field>Frank</field></Contacts><Contactsindex=""7""><field>Williams</field></Contacts><Contactsindex=""8""><field>07956284432.Mr</field></Contacts><Contactsindex=""9""><field>Enzo</field></Contacts><Contactsindex=""10""><field>Ferrari</field></Contacts><Contactsindex=""11""><field>08211698533.Mrs</field></Contacts><Contactsindex=""12""><field>Jenny</field></Contacts><Contactsindex=""13""><field>Smith</field></Contacts><Contactsindex=""14""><field>07624589634.Ms</field></Contacts><Contactsindex=""15""><field>Kerrin</field></Contacts><Contactsindex=""16""><field>deSilvia</field></Contacts><Contactsindex=""17""><field>07245873105.Sir</field></Contacts><Contactsindex=""18""><field>Richard</field></Contacts><Contactsindex=""19""><field>Branson</field></Contacts><Contactsindex=""20""><field>0812457896</field></Contacts><RecordCount>20</RecordCount><SplitChars>|</SplitChars>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Data Split Different Split Types
    }
}
