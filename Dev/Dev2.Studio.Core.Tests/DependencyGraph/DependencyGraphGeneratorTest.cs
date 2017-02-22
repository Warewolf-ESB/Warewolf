/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Dev2.Core.Tests
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DependencyGraphGeneratorTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DependencyGraphGenerator_BuildGraph")]
        public void DependencyGraphGenerator_BuildGraph_WhenGraphDataValid_ExpectValidGraph()
        {
            //------------Setup for test--------------------------
            const string graphData = "<graph title=\"Local Dependants Graph: 45e1fcc5-9f68-4d4a-9e01-20d587dee532\">" +
                                        "<node id=\"45e1fcc5-9f68-4d4a-9e01-20d587dee532\" x=\"\" y=\"\" broken=\"false\">" +
                                            "<dependency id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" />" +
                                        "</node>" +
                                        "<node id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" x=\"\" y=\"\" broken=\"false\">" +
                                            "<dependency id=\"e0e2cb45-aa63-417f-bc41-2fe4e906ba56\" />" +
                                        "</node>" +
                                     "</graph>";

            var data = new StringBuilder(graphData);
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            const string expected = "<graph title=\"Local Dependants Graph: 45e1fcc5-9f68-4d4a-9e01-20d587dee532\"><node id=\"45e1fcc5-9f68-4d4a-9e01-20d587dee532\" x=\"-100\" y=\"-400\" broken=\"False\"><dependency id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" /></node><node id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" x=\"-352\" y=\"-262\" broken=\"False\"></node></graph>";

            //------------Execute Test---------------------------
            // for some silly reason this is what comes through when you debug?
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 0, 0, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, result.ToString());

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DependencyGraphGenerator_BuildGraph")]
        public void DependencyGraphGenerator_BuildGraph_WhenGraphDataValidAndNestingLevel_ExpectValidGraph()
        {
            //------------Setup for test--------------------------
            const string graphData = "<graph title=\"Local Dependants Graph: 45e1fcc5-9f68-4d4a-9e01-20d587dee532\">" +
                                        "<node id=\"45e1fcc5-9f68-4d4a-9e01-20d587dee532\" x=\"\" y=\"\" broken=\"false\">" +
                                            "<dependency id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" />" +
                                        "</node>" +
                                        "<node id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" x=\"\" y=\"\" broken=\"false\">" +
                                            "<dependency id=\"e0e2cb45-aa63-417f-bc41-2fe4e906ba56\" />" +
                                        "</node>" + 
                                        "<node id=\"a839fe54-3f33-482a-b3e4-de74189e9g00\" x=\"\" y=\"\" broken=\"false\">" +
                                            "<dependency id=\"e0e2cb45-aa63-417f-bc41-1fe4e906ba56\" />" +
                                        "</node>" +
                                     "</graph>";

            var data = new StringBuilder(graphData);
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            const string expected = "<graph title=\"Local Dependants Graph: 45e1fcc5-9f68-4d4a-9e01-20d587dee532\"><node id=\"45e1fcc5-9f68-4d4a-9e01-20d587dee532\" x=\"-100\" y=\"-400\" broken=\"False\"><dependency id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" /></node><node id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" x=\"-352\" y=\"-262\" broken=\"False\"></node></graph>";

            //------------Execute Test---------------------------
            // for some silly reason this is what comes through when you debug?
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 0, 0, 1);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, result.ToString());

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DependencyGraphGenerator_BuildGraph")]
        public void DependencyGraphGenerator_BuildGraph_WhenIdEqualsName_ExpectCenterCoordinates()
        {
            //------------Setup for test--------------------------
            const string graphData = "<graph title=\"Local Dependants Graph: 45e1fcc5-9f68-4d4a-9e01-20d587dee532\">" +
                                         "<node id=\"Test Model\" x=\"\" y=\"\" broken=\"false\">" +
                                             "<dependency id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" />" +
                                         "</node>" +
                                         "<node id=\"a839fe54-3f33-482a-b3e4-de74189e9f00\" x=\"\" y=\"\" broken=\"false\">" +
                                             "<dependency id=\"e0e2cb45-aa63-417f-bc41-2fe4e906ba56\" />" +
                                         "</node>" +
                                         "<node id=\"a839fe54-3f33-482a-b3e4-de74189e9g00\" x=\"\" y=\"\" broken=\"false\">" +
                                             "<dependency id=\"e0e2cb45-aa63-417f-bc41-1fe4e906ba56\" />" +
                                         "</node>" +
                                      "</graph>";

            var data = new StringBuilder(graphData);
            var dependencyGraphGenerator = new DependencyGraphGenerator();


            //------------Execute Test---------------------------
            // for some silly reason this is what comes through when you debug?
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 4, 4, -1);

            //------------Assert Results-------------------------
            var x = result.Nodes.Any(node => node.LocationX == -98);
            var y = result.Nodes.Any(node => node.LocationY == -98);
            Assert.IsTrue(x);
            Assert.IsTrue(y);

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
            const string expected = "Dependency information is malformed";
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            //------------Execute Test---------------------------
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 0, 0, 0);

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
            const string expected = "Dependency information could not be retrieved";
            var dependencyGraphGenerator = new DependencyGraphGenerator();

            //------------Execute Test---------------------------
            var result = dependencyGraphGenerator.BuildGraph(data, "Test Model", 0, 0, 0);

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
            var result = dependencyGraphGenerator.BuildGraph(null, "Test Model", 0, 0, 0);

            //------------Assert Results-------------------------
            StringAssert.Contains(result.Title, expected);
        }

    }
}
