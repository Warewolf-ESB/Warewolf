
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
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfDataSplitActivityWFTests
    // ReSharper restore InconsistentNaming
    {
        readonly string _webserverUri = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region DataSplit RecordSet Tests

        [TestMethod]
        public void DataSplitWorkflowWithStar()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "Acceptance Testing Resources/DataSplitTestWithRecordsetUsingStar");
            string expected = @"<RecCount>20</RecCount><Contactsindex=""1""><field>Title</field></Contacts><Contactsindex=""2""><field>Fname</field></Contacts><Contactsindex=""3""><field>LName</field></Contacts><Contactsindex=""4""><field>TelNo</field></Contacts><Contactsindex=""5""><field>1.Mr</field></Contacts><Contactsindex=""6""><field>Frank</field></Contacts><Contactsindex=""7""><field>Williams</field></Contacts><Contactsindex=""8""><field>07956284432.Mr</field></Contacts><Contactsindex=""9""><field>Enzo</field></Contacts><Contactsindex=""10""><field>Ferrari</field></Contacts><Contactsindex=""11""><field>08211698533.Mrs</field></Contacts><Contactsindex=""12""><field>Jenny</field></Contacts><Contactsindex=""13""><field>Smith</field></Contacts><Contactsindex=""14""><field>07624589634.Ms</field></Contacts><Contactsindex=""15""><field>Kerrin</field></Contacts><Contactsindex=""16""><field>deSilvia</field></Contacts><Contactsindex=""17""><field>07245873105.Sir</field></Contacts><Contactsindex=""18""><field>Richard</field></Contacts><Contactsindex=""19""><field>Branson</field></Contacts><Contactsindex=""20""><field>0812457896</field></Contacts>";

            string responseData = TestHelper.PostDataToWebserver(postData);
            responseData = TestHelper.CleanUp(responseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(responseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithIndex()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "Acceptance Testing Resources/DataSplitTestWithRecordsetWithIndex");
            string expected = @"<Contacts index=""5""><field>0812457896</field></Contacts>";

            string responseData = TestHelper.PostDataToWebserver(postData);
            const string NotExpected = "0795628443";

            responseData = TestHelper.CleanUp(responseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(responseData, expected);
            Assert.IsFalse(responseData.Contains(NotExpected));
        }

        [TestMethod]
        public void DataSplitWorkflowWithNoIndex()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "Acceptance Testing Resources/DataSplitTestWithRecordsetsWithNoIndexes");
            string expected = @"<Contactsindex=""1""><Title>Title</Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><Contactsindex=""2""><Title>1.Mr</Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><Contactsindex=""3""><Title>2.Mr</Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><Contactsindex=""4""><Title>3.Mrs</Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><Contactsindex=""5""><Title>4.Ms</Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><Contactsindex=""6""><Title>5.Sir</Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            responseData = TestHelper.CleanUp(responseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(responseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithAppendingToDifferentRecordsets()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "Acceptance Testing Resources/DataSplitTestAppendingToDifferentRecordsets");
            string expected = @"<Titlesindex=""1""><Title>Title</Title></Titles><Titlesindex=""2""><Title>Mr</Title></Titles><Titlesindex=""3""><Title>Mr</Title></Titles><Titlesindex=""4""><Title>Mrs</Title></Titles><Titlesindex=""5""><Title>Ms</Title></Titles><Titlesindex=""6""><Title>Sir</Title></Titles><FirstNamesindex=""1""><FirstName>Fname</FirstName></FirstNames><FirstNamesindex=""2""><FirstName>Frank</FirstName></FirstNames><FirstNamesindex=""3""><FirstName>Enzo</FirstName></FirstNames><FirstNamesindex=""4""><FirstName>Jenny</FirstName></FirstNames><FirstNamesindex=""5""><FirstName>Kerrin</FirstName></FirstNames><FirstNamesindex=""6""><FirstName>Richard</FirstName></FirstNames><LastNamesindex=""1""><LastName>LName</LastName></LastNames><LastNamesindex=""2""><LastName>Williams</LastName></LastNames><LastNamesindex=""3""><LastName>Ferrari</LastName></LastNames><LastNamesindex=""4""><LastName>Smith</LastName></LastNames><LastNamesindex=""5""><LastName>deSilvia</LastName></LastNames><LastNamesindex=""6""><LastName>Branson</LastName></LastNames><Telsindex=""1""><Tel>TelNo</Tel></Tels><Telsindex=""2""><Tel>0795628443</Tel></Tels><Telsindex=""3""><Tel>0821169853</Tel></Tels><Telsindex=""4""><Tel>0762458963</Tel></Tels><Telsindex=""5""><Tel>0724587310</Tel></Tels><Telsindex=""6""><Tel>0812457896</Tel></Tels>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            responseData = TestHelper.CleanUp(responseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(responseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithScalar()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "Acceptance Testing Resources/DataSplitTestWithScalar");
            string expected = @"<Contactsindex=""1""><Title></Title><FirstName>Fname</FirstName><LastName>LName</LastName><Tel>TelNo</Tel></Contacts><Contactsindex=""2""><Title></Title><FirstName>Frank</FirstName><LastName>Williams</LastName><Tel>0795628443</Tel></Contacts><Contactsindex=""3""><Title></Title><FirstName>Enzo</FirstName><LastName>Ferrari</LastName><Tel>0821169853</Tel></Contacts><Contactsindex=""4""><Title></Title><FirstName>Jenny</FirstName><LastName>Smith</LastName><Tel>0762458963</Tel></Contacts><Contactsindex=""5""><Title></Title><FirstName>Kerrin</FirstName><LastName>deSilvia</LastName><Tel>0724587310</Tel></Contacts><Contactsindex=""6""><Title></Title><FirstName>Richard</FirstName><LastName>Branson</LastName><Tel>0812457896</Tel></Contacts><TitleScalar>5.Sir</TitleScalar>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            responseData = TestHelper.CleanUp(responseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(responseData, expected);
        }

        #endregion DataSplit RecordSet Tests

        #region Data Split Different Split Types

        [TestMethod]
        public void DataSplitWorkflowWithAllDifferentSplits()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "Acceptance Testing Resources/DataSplitAllDifferentTypesOfSplits");
            string expected = @"<Contacts index=""1""><Title>0.</Title><FirstName>Title</FirstName><LastName>Fname</LastName><Tel>LName|TelNo</Tel></Contacts><Contacts index=""2""><Title>1.</Title><FirstName>Mr</FirstName><LastName>Frank</LastName><Tel>Williams|0795628443</Tel></Contacts><Contacts index=""3""><Title>2.</Title><FirstName>Mr</FirstName><LastName>Enzo</LastName><Tel>Ferrari|0821169853</Tel></Contacts><Contacts index=""4""><Title>3.</Title><FirstName>Mrs</FirstName><LastName>Jenny</LastName><Tel>Smith|07624 58963</Tel></Contacts><Contacts index=""5""><Title>4.</Title><FirstName>Ms</FirstName><LastName>Kerrin</LastName><Tel>deSilvia|0724587310</Tel></Contacts><Contacts index=""6""><Title>5.</Title><FirstName>Sir</FirstName><LastName>Richard</LastName><Tel>Branson|0812457896</Tel></Contacts>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            responseData = TestHelper.CleanUp(responseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(responseData, expected);
        }

        [TestMethod]
        public void DataSplitWorkflowWithVariableInSource()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "Acceptance Testing Resources/DataSplitVariableInSource");
            string expected = @"<SourceStringVar>Title|Fname|LName|TelNo|1.Mr|Frank|Williams|07956284432.Mr|Enzo|Ferrari|08211698533.Mrs|Jenny|Smith|07624589634.Ms|Kerrin|deSilvia|07245873105.Sir|Richard|Branson|0812457896</SourceStringVar><Contactsindex=""1""><field>Title</field></Contacts><Contactsindex=""2""><field>Fname</field></Contacts><Contactsindex=""3""><field>LName</field></Contacts><Contactsindex=""4""><field>TelNo</field></Contacts><Contactsindex=""5""><field>1.Mr</field></Contacts><Contactsindex=""6""><field>Frank</field></Contacts><Contactsindex=""7""><field>Williams</field></Contacts><Contactsindex=""8""><field>07956284432.Mr</field></Contacts><Contactsindex=""9""><field>Enzo</field></Contacts><Contactsindex=""10""><field>Ferrari</field></Contacts><Contactsindex=""11""><field>08211698533.Mrs</field></Contacts><Contactsindex=""12""><field>Jenny</field></Contacts><Contactsindex=""13""><field>Smith</field></Contacts><Contactsindex=""14""><field>07624589634.Ms</field></Contacts><Contactsindex=""15""><field>Kerrin</field></Contacts><Contactsindex=""16""><field>deSilvia</field></Contacts><Contactsindex=""17""><field>07245873105.Sir</field></Contacts><Contactsindex=""18""><field>Richard</field></Contacts><Contactsindex=""19""><field>Branson</field></Contacts><Contactsindex=""20""><field>0812457896</field></Contacts><RecordCount>20</RecordCount><SplitChars>|</SplitChars>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            responseData = TestHelper.CleanUp(responseData);
            expected = TestHelper.CleanUp(expected);

            StringAssert.Contains(responseData, expected);
        }

        #endregion Data Split Different Split Types
    }
}
