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
using System.Drawing;
using System.Windows;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests
{
    [TestClass]
    public class ConflictTreeNodeTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_ObjectEquals_IsNot_ConflictTreeNode_ExpectFalse()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();

            var obj = new object();

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(obj);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_ObjectEquals_Is_ConflictTreeNode_ExpectTrue()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();

            var obj = new object();
            obj = new ConflictTreeNode(mockDev2Activity.Object, point);

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(obj);
            //----------------------Assert-------------------------
            Assert.IsTrue(treeNode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_Is_ConflictTreeNode_ExpectTrue()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity.Object, point);

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsTrue(treeNode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_IsNot_ConflictTreeNode_ExpectFalse()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();
            var mockDev2Activity1 = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();
            var point1 = new System.Windows.Point();

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity1.Object, point1);

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_ConflictTreeNode_IsNull_ExpectFalse()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();
            var mockDev2Activity1 = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();
            var point1 = new System.Windows.Point();

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity1.Object, point1);
            conflictTreeNode1 = null;

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_ConflictTreeNode_IsNull_ExpectFalse1()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();
            var mockDev2Activity1 = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();
            var point1 = new System.Windows.Point();

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity1.Object, point1);
            conflictTreeNode1 = null;

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_Is_ConflictTreeNode_ExpectTrue1()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();
            var mockConflictTreeNode = new Mock<IConflictTreeNode>();

            var point = new System.Windows.Point();

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode1.AddChild(mockConflictTreeNode.Object, "test1");

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode.AddChild(mockConflictTreeNode.Object, "test");

            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_ChildrenSequenceEqual_ExpectTrue()
        {
            //----------------------Arrange------------------------
            var activityId = Guid.NewGuid().ToString();
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(activityId);

            var childActivityId = Guid.NewGuid().ToString();
            var mockChildDev2Activity = new Mock<IDev2Activity>();
            mockChildDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(childActivityId);

            var mockConflictTreeNode = new Mock<IConflictTreeNode>();
            var mockConflictTreeNode1 = new Mock<IConflictTreeNode>();

            var point = new System.Windows.Point();

            var child = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };
            var child1 = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };

            mockConflictTreeNode.Setup(o => o.UniqueId).Returns("TestUniqueId");
            mockConflictTreeNode1.Setup(o => o.UniqueId).Returns("TestUniqueId");

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode.AddChild(child, "test");

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode1.AddChild(child1, "test");
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsTrue(treeNode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_ChildrenSequenceEqual_IdDifference_ExpectFalse()
        {
            //----------------------Arrange------------------------
            var activityId = Guid.NewGuid().ToString();
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(activityId);

            var childActivityId = Guid.NewGuid().ToString();
            var mockChildDev2Activity = new Mock<IDev2Activity>();
            mockChildDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(childActivityId);

            var mockConflictTreeNode = new Mock<IConflictTreeNode>();
            var mockConflictTreeNode1 = new Mock<IConflictTreeNode>();

            var point = new System.Windows.Point();

            var child = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };
            var child1 = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId1"
            };

            mockConflictTreeNode.Setup(o => o.UniqueId).Returns("TestUniqueId");
            mockConflictTreeNode1.Setup(o => o.UniqueId).Returns("TestUniqueId");

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode.AddChild(child, "test");

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode1.AddChild(child1, "test");
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_ChildrenSequenceEqual_NameDifference_ExpectFalse()
        {
            //----------------------Arrange------------------------
            var activityId = Guid.NewGuid().ToString();
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(activityId);

            var childActivityId = Guid.NewGuid().ToString();
            var mockChildDev2Activity = new Mock<IDev2Activity>();
            mockChildDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(childActivityId);

            var mockConflictTreeNode = new Mock<IConflictTreeNode>();
            var mockConflictTreeNode1 = new Mock<IConflictTreeNode>();

            var point = new System.Windows.Point();

            var child = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };
            var child1 = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };

            mockConflictTreeNode.Setup(o => o.UniqueId).Returns("TestUniqueId");
            mockConflictTreeNode1.Setup(o => o.UniqueId).Returns("TestUniqueId");

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode.AddChild(child, "test");

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode1.AddChild(child1, "test1");
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_Equals_ChildrenSequenceEqual_ChildCountDifference_ExpectFalse()
        {
            //----------------------Arrange------------------------
            var activityId = Guid.NewGuid().ToString();
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(activityId);

            var childActivityId = Guid.NewGuid().ToString();
            var mockChildDev2Activity = new Mock<IDev2Activity>();
            mockChildDev2Activity.Setup(dev2Activity => dev2Activity.UniqueID).Returns(childActivityId);

            var mockConflictTreeNode = new Mock<IConflictTreeNode>();
            var mockConflictTreeNode1 = new Mock<IConflictTreeNode>();

            var point = new System.Windows.Point();

            var child = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };
            var child2 = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };
            var child1 = new ConflictTreeNode(mockChildDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };

            mockConflictTreeNode.Setup(o => o.UniqueId).Returns("TestUniqueId");
            mockConflictTreeNode1.Setup(o => o.UniqueId).Returns("TestUniqueId");

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode.AddChild(child, "test");
            conflictTreeNode.AddChild(child2, "test");

            var conflictTreeNode1 = new ConflictTreeNode(mockDev2Activity.Object, point);
            conflictTreeNode1.AddChild(child1, "test");
            //----------------------Act----------------------------
            var treeNode = conflictTreeNode.Equals(conflictTreeNode1);
            //----------------------Assert-------------------------
            Assert.IsFalse(treeNode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_GetHashCode_Children_IsNotNull_ExpectIsNotNull()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();
            var mockConflictTreeNode = new Mock<IConflictTreeNode>();

            var point = new System.Windows.Point();

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };
            conflictTreeNode.AddChild(mockConflictTreeNode.Object, "test");
            //----------------------Act----------------------------
            var hashCode = conflictTreeNode.GetHashCode();
            //----------------------Assert-------------------------
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_GetHashCode_Children_IsNull_ExpectIsNotNull()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point)
            {
                UniqueId = "testUniqueId"
            };
            //----------------------Act----------------------------
            var hashCode = conflictTreeNode.GetHashCode();
            //----------------------Assert-------------------------
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ConflictTreeNode))]
        public void ConflictTreeNode_SetProperty_Location_and_IsInConflict_ExpectSetValue()
        {
            //----------------------Arrange------------------------
            var mockDev2Activity = new Mock<IDev2Activity>();

            var point = new System.Windows.Point();

            var conflictTreeNode = new ConflictTreeNode(mockDev2Activity.Object, point)
            {
                IsInConflict = true
            };
            //----------------------Act----------------------------

            //----------------------Assert-------------------------
            Assert.AreEqual(point, conflictTreeNode.Location);
            Assert.IsTrue(conflictTreeNode.IsInConflict);
        }
    }
}
