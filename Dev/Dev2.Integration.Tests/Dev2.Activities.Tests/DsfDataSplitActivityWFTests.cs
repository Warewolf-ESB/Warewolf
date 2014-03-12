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
            string expected = @"<RecCount>20</RecCount><ContactsrowID=""1""><field>Title</field></Contacts><ContactsrowID=""2""><field>Fname</field></Contacts><ContactsrowID=""3""><field>LName</field></Contacts><ContactsrowID=""4""><field>TelNo</field></Contacts><ContactsrowID=""5""><field>1.Mr</field></Contacts><ContactsrowID=""6""><field>Frank</field></Contacts><ContactsrowID=""7""><field>Williams</field></Contacts><ContactsrowID=""8""><field>07956284432.Mr</field></Contacts><ContactsrowID=""9""><field>Enzo</field></Contacts><ContactsrowID=""10""><field>Ferrari</field></Contacts><ContactsrowID=""11""><field>08211698533.Mrs</field></Contacts><ContactsrowID=""12""><field>Jenny</field></Contacts><ContactsrowID=""13""><field>Smith</field></Contacts><ContactsrowID=""14""><field>07624589634.Ms</field></Contacts><ContactsrowID=""15""><field>Kerrin</field></Contacts><ContactsrowID=""16""><field>deSilvia</field></Contacts><ContactsrowID=""17""><field>07245873105.Sir</field></Contacts><ContactsrowID=""18""><field>Richard</field></Contacts><ContactsrowID=""19""><field>Branson</field></Contacts><ContactsrowID=""20""><field>0812457896</field></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithRecordsetWithIndex");
            string expected = @"<Contacts rowID=""5""><field>0812457896</field></Contacts>";

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
            string expected = @"<ContactsrowID=""1""><Title>Title</Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><ContactsrowID=""2""><Title>1.Mr</Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><ContactsrowID=""3""><Title>2.Mr</Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><ContactsrowID=""4""><Title>3.Mrs</Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><ContactsrowID=""5""><Title>4.Ms</Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><ContactsrowID=""6""><Title>5.Sir</Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithAppendingToDifferentRecordsets()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestAppendingToDifferentRecordsets");
            string expected = @"<TitlesrowID=""1""><Title>Title</Title></Titles><TitlesrowID=""2""><Title>Mr</Title></Titles><TitlesrowID=""3""><Title>Mr</Title></Titles><TitlesrowID=""4""><Title>Mrs</Title></Titles><TitlesrowID=""5""><Title>Ms</Title></Titles><TitlesrowID=""6""><Title>Sir</Title></Titles><FirstNamesrowID=""1""><FirstName>Fname</FirstName></FirstNames><FirstNamesrowID=""2""><FirstName>Frank</FirstName></FirstNames><FirstNamesrowID=""3""><FirstName>Enzo</FirstName></FirstNames><FirstNamesrowID=""4""><FirstName>Jenny</FirstName></FirstNames><FirstNamesrowID=""5""><FirstName>Kerrin</FirstName></FirstNames><FirstNamesrowID=""6""><FirstName>Richard</FirstName></FirstNames><LastNamesrowID=""1""><LastName>LName</LastName></LastNames><LastNamesrowID=""2""><LastName>Williams</LastName></LastNames><LastNamesrowID=""3""><LastName>Ferrari</LastName></LastNames><LastNamesrowID=""4""><LastName>Smith</LastName></LastNames><LastNamesrowID=""5""><LastName>deSilvia</LastName></LastNames><LastNamesrowID=""6""><LastName>Branson</LastName></LastNames><TelsrowID=""1""><Tel>TelNo</Tel></Tels><TelsrowID=""2""><Tel>0795628443</Tel></Tels><TelsrowID=""3""><Tel>0821169853</Tel></Tels><TelsrowID=""4""><Tel>0762458963</Tel></Tels><TelsrowID=""5""><Tel>0724587310</Tel></Tels><TelsrowID=""6""><Tel>0812457896</Tel></Tels>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithScalar()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitTestWithScalar");
            string expected = @"<ContactsrowID=""1""><Title></Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><ContactsrowID=""2""><Title></Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><ContactsrowID=""3""><Title></Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><ContactsrowID=""4""><Title></Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><ContactsrowID=""5""><Title></Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><ContactsrowID=""6""><Title></Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts><TitleScalar>5.Sir</TitleScalar>";

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
            string expected = @"<Contacts rowID=""1""><Title>0.</Title><FirstName>Title</FirstName><LastName>Fname</LastName><Tel>LName|TelNo</Tel></Contacts><Contacts rowID=""2""><Title>1.</Title><FirstName>Mr</FirstName><LastName>Frank</LastName><Tel>Williams|0795628443</Tel></Contacts><Contacts rowID=""3""><Title>2.</Title><FirstName>Mr</FirstName><LastName>Enzo</LastName><Tel>Ferrari|0821169853</Tel></Contacts><Contacts rowID=""4""><Title>3.</Title><FirstName>Mrs</FirstName><LastName>Jenny</LastName><Tel>Smith|07624 58963</Tel></Contacts><Contacts rowID=""5""><Title>4.</Title><FirstName>Ms</FirstName><LastName>Kerrin</LastName><Tel>deSilvia|0724587310</Tel></Contacts><Contacts rowID=""6""><Title>5.</Title><FirstName>Sir</FirstName><LastName>Richard</LastName><Tel>Branson|0812457896</Tel></Contacts>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithVariableInSource()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataSplitVariableInSource");
            string expected = @"<SourceStringVar>Title|Fname|LName|TelNo|1.Mr|Frank|Williams|07956284432.Mr|Enzo|Ferrari|08211698533.Mrs|Jenny|Smith|07624589634.Ms|Kerrin|deSilvia|07245873105.Sir|Richard|Branson|0812457896</SourceStringVar><ContactsrowID=""1""><field>Title</field></Contacts><ContactsrowID=""2""><field>Fname</field></Contacts><ContactsrowID=""3""><field>LName</field></Contacts><ContactsrowID=""4""><field>TelNo</field></Contacts><ContactsrowID=""5""><field>1.Mr</field></Contacts><ContactsrowID=""6""><field>Frank</field></Contacts><ContactsrowID=""7""><field>Williams</field></Contacts><ContactsrowID=""8""><field>07956284432.Mr</field></Contacts><ContactsrowID=""9""><field>Enzo</field></Contacts><ContactsrowID=""10""><field>Ferrari</field></Contacts><ContactsrowID=""11""><field>08211698533.Mrs</field></Contacts><ContactsrowID=""12""><field>Jenny</field></Contacts><ContactsrowID=""13""><field>Smith</field></Contacts><ContactsrowID=""14""><field>07624589634.Ms</field></Contacts><ContactsrowID=""15""><field>Kerrin</field></Contacts><ContactsrowID=""16""><field>deSilvia</field></Contacts><ContactsrowID=""17""><field>07245873105.Sir</field></Contacts><ContactsrowID=""18""><field>Richard</field></Contacts><ContactsrowID=""19""><field>Branson</field></Contacts><ContactsrowID=""20""><field>0812457896</field></Contacts><RecordCount>20</RecordCount><SplitChars>|</SplitChars>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ResponseData = TestHelper.CleanUp(ResponseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Data Split Different Split Types
    }
}
