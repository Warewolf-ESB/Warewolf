/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            var list = new List<IDependencyVisualizationNode>();

            var circularDependency = new CircularDependency(list);

            Assert.AreNotEqual(list, circularDependency.Nodes);
            Assert.AreEqual(0, circularDependency.Nodes.Count, "circularDependency.Nodes");
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
            Assert.AreEqual(0, circularDependency.Nodes.Count, "circularDependency.Nodes");
            Assert.AreEqual(0, circularDependency1.Nodes.Count, "circularDependency.Nodes");
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
            Assert.AreEqual(1, circularDependency.Nodes.Count, "circularDependency.Nodes");
            Assert.AreEqual(1, circularDependency1.Nodes.Count, "circularDependency.Nodes");
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
            Assert.AreEqual(2, circularDependency.Nodes.Count, "circularDependency.Nodes");
            Assert.AreEqual(0, circularDependency1.Nodes.Count, "circularDependency.Nodes");
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
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            var hashCode = circularDependency.GetHashCode();
            var code = circularDependency.Nodes.GetHashCode();
            Assert.AreEqual(code,hashCode);
        }
    }
}
