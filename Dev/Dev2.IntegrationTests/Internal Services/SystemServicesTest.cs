
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace Dev2.Integration.Tests.Internal_Services
{
    /// <summary>
    /// Summary description for SystemServices
    /// </summary>
    [TestClass]
    public class SystemServicesTest
    {
        // ReSharper disable InconsistentNaming
        private readonly string _webServerURI = ServerSettings.WebserverURI;

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
        public void DepenendcyViewerReturnsOnlyValidDependenciesExpectTwoDependencies()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "FindDependencyService", "ResourceName=TestCategory\\Bug6619");

            // The expected graph to be returned 
            const string expected = @"<graph title=""Dependency Graph Of TestCategory\Bug6619""><node id=""Bug6619"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug6619Dep"" /></node><node id=""Bug6619Dep"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug6619Dep2"" /></node><node id=""Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug6619Dep"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug6619"" x="""" y="""" broken=""false""></node></graph>";

            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DepenendcyViewerReturnsOnlyValidDependenciesExpectTwoDependenciesWithTravsCrazyWorkflow()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "FindDependencyService", "ResourceName=TestCategory\\Bug9245");

            // The expected graph to be returned 
            const string expected = @"<graph title=""Dependency Graph Of TestCategory\Bug9245""><node id=""Bug9245"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug9245a"" /><dependency id=""TestCategory\Bug9245b"" /></node><node id=""Bug9245a"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug9245a"" x="""" y="""" broken=""false""></node><node id=""Bug9245b"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug9245c"" /></node><node id=""Bug9245c"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug6619"" /><dependency id=""TestCategory\Bug8372"" /></node><node id=""Bug6619"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug6619Dep"" /></node><node id=""Bug6619Dep"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug6619Dep2"" /></node><node id=""Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug6619Dep"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug6619"" x="""" y="""" broken=""false""></node><node id=""Bug8372"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug8372Sub"" /></node><node id=""Bug8372Sub"" x="""" y="""" broken=""false""><dependency id=""TestCategory\Bug8372SubSub"" /></node><node id=""Bug8372SubSub"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug8372SubSub"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug8372Sub"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug8372"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug9245c"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug9245b"" x="""" y="""" broken=""false""></node><node id=""TestCategory\Bug9245"" x="""" y="""" broken=""false""></node></graph>";

            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DepenendcyViewerReturnsValidDependentsWhenGetDependsOnMeTrueExpectOneDependantWithTravsCrazyWorkflow()
        {
            string postData = string.Format("{0}{1}?{2}&{3}", _webServerURI, "FindDependencyService", "ResourceName=TestCategory\\Bug9245a", "GetDependsOnMe=True");

            // The expected graph to be returned 
            const string expected = @"<graph title=""Local Dependants Graph: TestCategory\Bug9245a""><node id=""Bug9245"" x="""" y="""" broken=""false""><dependency id=""Bug9245a"" /></node><node id=""TestCategory\Bug9245a"" x="""" y="""" broken=""false""></node></graph>";

            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DepenendcyViewerReturnsValidMultipleFirstLevelDependantsWhenGetDependsOnMeTrueExpectTwoDependendant()
        {
            string postData = string.Format("{0}{1}?{2}&{3}", _webServerURI, "FindDependencyService", "ResourceName=TestCategory\\Bug_9303", "GetDependsOnMe=True");

            // The expected graph to be returned 
            const string expectedTitle = @"<graph title=""Local Dependants Graph: TestCategory\Bug_9303"">";
            const string expectedNode1 = @"<node id=""DepOn_9303_1"" x="""" y="""" broken=""false""><dependency id=""Bug_9303"" /></node>";
            const string expectedNode2 = @"<node id=""DepOn_9303_2"" x="""" y="""" broken=""false""><dependency id=""Bug_9303"" /></node>";
            const string baseNode = @"<node id=""TestCategory\Bug_9303"" x="""" y="""" broken=""false""></node>";
            const string endNode = @"</graph>";
            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expectedTitle);
            StringAssert.Contains(actual, expectedNode1);
            StringAssert.Contains(actual, expectedNode2);
            StringAssert.Contains(actual, baseNode);
            StringAssert.Contains(actual, endNode);
        }
    }
}
