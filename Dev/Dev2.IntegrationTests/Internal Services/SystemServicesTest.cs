
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

        [TestMethod]
        public void DepenendcyViewerReturnsOnlyValidDependenciesExpectTwoDependencies()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "FindDependencyService", "ResourceId=1736ca6e-b870-467f-8d25-262972d8c3e8");

            // The expected graph to be returned 
            const string expected = @"<graph title=""Dependency Graph Of Acceptance Testing Resources\Bug6619""><node id=""Bug6619"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug6619Dep"" /></node><node id=""Bug6619Dep"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug6619Dep2"" /></node><node id=""Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug6619Dep"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug6619"" x="""" y="""" broken=""false""></node></graph>";

            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DepenendcyViewerReturnsOnlyValidDependenciesExpectTwoDependenciesWithTravsCrazyWorkflow()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "FindDependencyService", "ResourceId=e59b7fe3-ad37-4363-8678-74601b9ea3cb");

            // The expected graph to be returned 
            const string expected = @"<graph title=""Dependency Graph Of Acceptance Testing Resources\Bug9245""><node id=""Bug9245"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug9245b"" /><dependency id=""Integration Test Resources\Bug9245a"" /></node><node id=""Bug9245b"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug9245c"" /></node><node id=""Bug9245c"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug6619"" /><dependency id=""Integration Test Resources\Bug8372"" /></node><node id=""Bug6619"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug6619Dep"" /></node><node id=""Bug6619Dep"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug6619Dep2"" /></node><node id=""Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug6619Dep2"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug6619Dep"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug6619"" x="""" y="""" broken=""false""></node><node id=""Bug8372"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug8372Sub"" /></node><node id=""Bug8372Sub"" x="""" y="""" broken=""false""><dependency id=""Integration Test Resources\Bug8372SubSub"" /></node><node id=""Bug8372SubSub"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug8372SubSub"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug8372Sub"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug8372"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug9245c"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug9245b"" x="""" y="""" broken=""false""></node><node id=""Bug9245a"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug9245a"" x="""" y="""" broken=""false""></node><node id=""Acceptance Testing Resources\Bug9245"" x="""" y="""" broken=""false""></node></graph>";

            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DepenendcyViewerReturnsValidDependentsWhenGetDependsOnMeTrueExpectOneDependantWithTravsCrazyWorkflow()
        {
            string postData = string.Format("{0}{1}?{2}&{3}", _webServerURI, "FindDependencyService", "ResourceId=58c69c42-05fc-44ff-84a1-bef6de140937", "GetDependsOnMe=True");

            // The expected graph to be returned 
            const string expected = @"<graph title=""Local Dependants Graph: Acceptance Testing Resources\Bug9245a""><node id=""Bug9245"" x="""" y="""" broken=""false""><dependency id=""Bug9245a"" /></node><node id=""Acceptance Testing Resources\Bug9245a"" x="""" y="""" broken=""false""></node></graph>";

            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expected);
        }

        [TestMethod]
        public void DepenendcyViewerReturnsValidMultipleFirstLevelDependantsWhenGetDependsOnMeTrueExpectTwoDependendant()
        {
            string postData = string.Format("{0}{1}?{2}&{3}", _webServerURI, "FindDependencyService", "ResourceId=b7b0f0ee-add1-437b-bd3b-c7882f64e353", "GetDependsOnMe=True");

            // The expected graph to be returned 
            const string expectedTitle = @"<graph title=""Local Dependants Graph: Acceptance Testing Resources\Bug_9303"">";
            const string expectedNode1 = @"<node id=""DepOn_9303_1"" x="""" y="""" broken=""false""><dependency id=""Bug_9303"" /></node>";
            const string expectedNode2 = @"<node id=""DepOn_9303_2"" x="""" y="""" broken=""false""><dependency id=""Bug_9303"" /></node>";
            const string baseNode = @"<node id=""Acceptance Testing Resources\Bug_9303"" x="""" y="""" broken=""false""></node>";
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
