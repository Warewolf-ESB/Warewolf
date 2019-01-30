using System;
using System.Collections.Generic;
using Dev2.Common.DependencyVisualization;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.DependencyGraph
{
    [TestClass]
    public class CircularDependencyTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CircularDependency))]
        public void CircularDependency_Constructor_GivenNodes_ShouldExpectDefaults()
        {
            //---------------Set up test pack-------------------
            var circularDependency = new CircularDependency(new List<IDependencyVisualizationNode>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(circularDependency);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(circularDependency.Nodes);
            Assert.AreEqual(0, circularDependency.Nodes.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CircularDependency))]
        public void CircularDependency_Equals_GivenNodesEquals_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var circularDependency = new CircularDependency(new List<IDependencyVisualizationNode>());
            var circularDependency1 = new CircularDependency(new List<IDependencyVisualizationNode>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(circularDependency);

            Assert.IsNotNull(circularDependency.Nodes);
            Assert.AreEqual(0, circularDependency.Nodes.Count);
            Assert.IsNotNull(circularDependency1);
            Assert.IsNotNull(circularDependency1.Nodes);
            Assert.AreEqual(0, circularDependency1.Nodes.Count);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            var @equals = circularDependency.Equals(circularDependency1);
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CircularDependency))]
        public void CircularDependency_Equals_GivenNodesEquals_GivenNodesNotZero_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var dependencyVisualizationNode = new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false);

            var virtualizationNode = new List<IDependencyVisualizationNode>();
            virtualizationNode.Add(dependencyVisualizationNode);

            var circularDependency = new CircularDependency(virtualizationNode);
            var circularDependency1 = new CircularDependency(virtualizationNode);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(circularDependency);

            Assert.IsNotNull(circularDependency.Nodes);
            Assert.AreEqual(1, circularDependency.Nodes.Count);
            Assert.IsNotNull(circularDependency1);
            Assert.IsNotNull(circularDependency1.Nodes);
            Assert.AreEqual(1, circularDependency1.Nodes.Count);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            var @equals = circularDependency.Equals(circularDependency1);
            Assert.IsTrue(@equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CircularDependency))]
        public void CircularDependency_Equals_GivenNodesNotEquals_GivenNodesNotZero_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var dependencyVisualizationNode = new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false);
            var dependencyVisualizationNode1 = new DependencyVisualizationNode(Guid.NewGuid().ToString(), 2, 2, true, false);

            var virtualizationNode = new List<IDependencyVisualizationNode>();
            virtualizationNode.Add(dependencyVisualizationNode);

            var virtualizationNode1 = new List<IDependencyVisualizationNode>();
            virtualizationNode.Add(dependencyVisualizationNode1);

            var circularDependency = new CircularDependency(virtualizationNode);
            var circularDependency1 = new CircularDependency(virtualizationNode1);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(circularDependency);

            Assert.IsNotNull(circularDependency.Nodes);
            Assert.AreEqual(2, circularDependency.Nodes.Count);
            Assert.IsNotNull(circularDependency1);
            Assert.IsNotNull(circularDependency1.Nodes);
            Assert.AreEqual(0, circularDependency1.Nodes.Count);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            var @equals = circularDependency.Equals(circularDependency1);
            Assert.IsFalse(@equals);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CircularDependency))]
        public void CircularDependency_CompareTo_GivenEqualNodes_ShouldOne()
        {
            //---------------Set up test pack-------------------
            var circularDependency = new CircularDependency(new List<IDependencyVisualizationNode>());
            var circularDependency1 = new CircularDependency(new List<IDependencyVisualizationNode>());
            //---------------Assert Precondition----------------
            var @equals = circularDependency.Equals(circularDependency1);
            Assert.IsTrue(@equals);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            var compareTo = circularDependency.CompareTo(circularDependency1);
            Assert.AreEqual(0, compareTo);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CircularDependency))]
        public void CircularDependency_GetHashCode_GivenEqualNodes_ShouldNodeshashCode()
        {
            //---------------Set up test pack-------------------
            var circularDependency = new CircularDependency(new List<IDependencyVisualizationNode>());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(circularDependency.Nodes);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            var hashCode = circularDependency.GetHashCode();
            var code = circularDependency.Nodes.GetHashCode();
            Assert.AreEqual(code,hashCode);
        }
    }
}
