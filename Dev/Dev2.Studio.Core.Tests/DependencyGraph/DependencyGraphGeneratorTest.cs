
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Text;
using Dev2.ViewModels.DependencyVisualization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DependencyGraphGeneratorTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DependencyGraphGenerator_BuildGraph")]
        public void DependencyGraphGenerator_BuildGraph_WhenGraphDataValid_ExpectValidGraph()
        {
            //------------Setup for test--------------------------
            const string graphData = @"<graph title=""Local Dependants Graph: MyLocalWF"">
  <node id=""9139Local"" x="""" y="""" broken=""false"">
    <dependency id=""MyLocalWF"" />
  </node>
  <node id=""MyLocalWF"" x="""" y="""" broken=""false""></node>
</graph>";

            var data = new StringBuilder(graphData);
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            const string expected = @"<graph title=""Local Dependants Graph: MyLocalWF""><node id=""9139Local"" x=""-100"" y=""-400"" broken=""False""><dependency id=""MyLocalWF"" /></node><node id=""MyLocalWF"" x=""-352"" y=""-262"" broken=""False""></node></graph>";
            
            //------------Execute Test---------------------------
            // for some silly reason this is what comes through when you debug?
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 0, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, result.ToString());
            
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DependencyGraphGenerator_BuildGraph")]
        public void DependencyGraphGenerator_BuildGraph_WhenGraphContainsMalformedGraphTag_ExpectErrorGraph()
        {
            //------------Setup for test--------------------------
            const string graphData = @"<graphz title=""Local Dependants Graph: MyLocalWF"">
  <node id=""9139Local"" x="""" y="""" broken=""false"">
    <dependency id=""MyLocalWF"" />
  </node>
  <node id=""MyLocalWF"" x="""" y="""" broken=""false""></node>
</graphz>";
            var data = new StringBuilder(graphData);
            var expected = "Dependency information is malformed";
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            //------------Execute Test---------------------------
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 0, 0);

            //------------Assert Results-------------------------
            StringAssert.Contains(result.Title, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DependencyGraphGenerator_BuildGraph")]
        public void DependencyGraphGenerator_BuildGraph_WhenGraphDataEmpty_ExpectErrorGraph()
        {
            //------------Setup for test--------------------------
            var data = new StringBuilder();
            var expected = "Dependency information could not be retrieved";
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            //------------Execute Test---------------------------
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 0, 0);

            //------------Assert Results-------------------------
            StringAssert.Contains(result.Title, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DependencyGraphGenerator_BuildGraph")]
        public void DependencyGraphGenerator_BuildGraph_WhenGraphDataNull_ExpectErrorGraph()
        {
            //------------Setup for test--------------------------
            const string expected = "Dependency information could not be retrieved";
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            //------------Execute Test---------------------------
            var result = dependencyGraphGenerator.BuildGraph(null, "Test Model", 0, 0);

            //------------Assert Results-------------------------
            StringAssert.Contains(result.Title, expected);
        }
       
    }
}
