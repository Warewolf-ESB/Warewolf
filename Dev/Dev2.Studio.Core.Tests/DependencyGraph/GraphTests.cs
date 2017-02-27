using System.Collections.Generic;
using Dev2.Common.DependencyVisualization;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.DependencyGraph
{
    [TestClass]
    public class GraphTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenTitle_ShouldSetTitleAndDefaults()
        {
            //---------------Set up test pack-------------------
            var graph = new Graph("a");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(graph.Title);
            Assert.IsNotNull(graph.Nodes);
            Assert.IsNotNull(graph.CircularDependencies);
            Assert.AreEqual(0, graph.GridColumn);
            Assert.AreEqual(0, graph.ColSpan);
            Assert.AreEqual(false, graph.HasCircularDependency);
            //---------------Execute Test ----------------------
            var title = graph.Title;
            //---------------Test Result -----------------------
            Assert.AreEqual("a", title);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ToString_GivenNodes_ShouldBuildGraphXml()
        {
            //---------------Set up test pack-------------------
            var graph = new Graph("a");
            graph.Nodes.AddRange(new List<IDependencyVisualizationNode>()
            {
                new DependencyVisualizationNode("a",2,2,true,false)
            });
            //---------------Assert Precondition----------------
            Assert.AreEqual("a", graph.Title);
            Assert.AreEqual(1, graph.Nodes.Count);
            //---------------Execute Test ----------------------
            var graphXml = graph.ToString();
            var isXml = graphXml.IsXml();
            //---------------Test Result -----------------------
            Assert.IsTrue(isXml);
            var containsGraph = graphXml.Contains("</graph>");
            var containsTitle = graphXml.Contains("<graph title=");
            Assert.IsTrue(containsTitle);
            Assert.IsTrue(containsGraph);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CheckForCircularDependencies_GivenNodes_ShouldAddNewCircularDependencies()
        {
            //---------------Set up test pack-------------------
            var graph = new Graph("a");
            IDependencyVisualizationNode dependencyVisualizationNode = new DependencyVisualizationNode("z", 2, 8, true, false);
            dependencyVisualizationNode.NodeDependencies.Add(dependencyVisualizationNode);
            graph.Nodes.AddRange(new List<IDependencyVisualizationNode>()
            {
                dependencyVisualizationNode,
                new DependencyVisualizationNode("b",2,2,true,false),
                new DependencyVisualizationNode("g",2,2,true,false),

            });
            //---------------Assert Precondition----------------
            var id0 = graph.Nodes[0].ID;
            var id1 = graph.Nodes[1].ID;
            var id2 = graph.Nodes[2].ID;
            Assert.AreEqual("z", id0);
            Assert.AreEqual("b", id1);
            Assert.AreEqual("g", id2);
            //---------------Execute Test ----------------------
            graph.CheckForCircularDependencies();
            //---------------Test Result -----------------------
            id0 = graph.Nodes[0].ID;
            id1 = graph.Nodes[1].ID;
            id2 = graph.Nodes[2].ID;
            Assert.AreEqual("z", id0);
            Assert.AreEqual("b", id1);
            Assert.AreEqual("g", id2);
            var circularDependencies = graph.CircularDependencies;
            Assert.AreEqual(1, circularDependencies.Count);
        }
    }
}
