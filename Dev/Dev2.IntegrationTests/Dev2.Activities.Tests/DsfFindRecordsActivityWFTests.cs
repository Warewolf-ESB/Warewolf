
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
using System.Text.RegularExpressions;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfFindRecordsActivityWFTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfFindRecordsActivityWFTests
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


        [TestMethod]
        public void FindRecordsGeneralTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Integration Test Resources/FindRecordsGeneralTest");
            string expected = @"<Person index=""1""><Fname>Barney</Fname><Lname>Buchan</Lname><Tel>0821548996</Tel><DOB>1978/07/23</DOB><Email>barney@WorldClass.co.za</Email></Person><Person index=""2""><Fname>Travis</Fname><Lname>Frisigner</Lname><Tel>0845698712</Tel><DOB>1981/04/29</DOB><Email>trav@Abstact.co.us</Email></Person><Person index=""3""><Fname>Brendon</Fname><Lname>Page</Lname><Tel>0815698235</Tel><DOB>1982/09/30</DOB><Email>Bredon@Troll.co.zn</Email></Person><Person index=""4""><Fname>Mattew</Fname><Lname>van Ryn</Lname><Tel>0824512336</Tel><DOB>1986/12/09</DOB><Email>Mat@Hack4u.co.uk</Email></Person><Results index=""1""><FindRes>2</FindRes></Results>";
            string expected2 = "<Results index=\"1\">      <FindRes>2</FindRes>    </Results>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            expected2 = regex.Replace(expected2, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
            StringAssert.Contains(ResponseData, expected2);
        }


        [TestMethod]
        public void FindRecordsMutiTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Integration Test Resources/FindRecordsMutiTest");
            string expected = "<Person index=\"1\">      <Fname>Barney</Fname>      <Lname>Buchan</Lname>      <Tel>0821548996</Tel>      <DOB>1978/07/23</DOB>      <Email>barney@WorldClass.co.za</Email>    </Person>    <Person index=\"2\">      <Fname>Travis</Fname>      <Lname>Frisigner</Lname>      <Tel>0845698712</Tel>      <DOB>1981/04/29</DOB>      <Email>trav@Abstact.co.us</Email>    </Person>    <Person index=\"3\">      <Fname>Brendon</Fname>      <Lname>Page</Lname>      <Tel>0815698235</Tel>      <DOB>1982/09/30</DOB>      <Email>Bredon@Troll.co.zn</Email>    </Person>    <Person index=\"4\">      <Fname>Mattew</Fname>      <Lname>van Ryn</Lname>      <Tel>0824512336</Tel>      <DOB>1986/12/09</DOB>      <Email>Mat@Hack4u.co.uk</Email>    </Person>";
            string expected2 = @"<Searches index=""1""><field>a</field></Searches><Searches index=""2""><field>B</field></Searches><Searches index=""3""><field>v</field></Searches>";
            string expected3 = @"<Results index=""1""><FindRes>1</FindRes><FindResSec></FindResSec></Results><Results index=""2""><FindRes>4</FindRes><FindResSec></FindResSec></Results><Results index=""3""><FindRes>3</FindRes><FindResSec></FindResSec></Results><Results index=""4""><FindRes></FindRes><FindResSec>1</FindResSec></Results><Results index=""5""><FindRes></FindRes><FindResSec>2</FindResSec></Results><Results index=""6""><FindRes></FindRes><FindResSec>4</FindResSec></Results><Results index=""7""><FindRes></FindRes><FindResSec>1</FindResSec></Results><Results index=""8""><FindRes></FindRes><FindResSec>3</FindResSec></Results><Results index=""9""><FindRes></FindRes><FindResSec>2</FindResSec></Results>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            expected2 = regex.Replace(expected2, "><");
            expected3 = regex.Replace(expected3, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
            StringAssert.Contains(ResponseData, expected2);
            StringAssert.Contains(ResponseData, expected3);
        }
    }
}
