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
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenNodes_ShouldExpectDefaults()
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
        [Owner("Nkosinathi Sangweni")]
        public void Equals_GivenNodesEquals_ShouldReturnTrue()
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
        [Owner("Nkosinathi Sangweni")]
        public void CompareTo_GivenEqualNodes_ShouldOne()
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
        [Owner("Nkosinathi Sangweni")]
        public void GetHashCode_GivenEqualNodes_ShouldNodeshashCode()
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
