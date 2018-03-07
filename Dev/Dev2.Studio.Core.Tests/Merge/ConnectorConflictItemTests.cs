﻿



using Dev2.Common;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Dev2.ViewModels.Merge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ConnectorConflictItemTests
    {
        readonly Guid Grouping = Guid.Empty;
        const string ArmDescription = "Arm -> Arm";
        readonly Guid SourceUniqueId = Guid.Empty;
        readonly Guid DestinationUniqueId = Guid.Empty;
        const string Key = "Key";

        readonly Guid SourceUniqueIdDiff = Guid.NewGuid();
        readonly Guid DestinationUniqueIdDiff = Guid.NewGuid();
        const string KeyDiff = "KeyDiff";

        ConflictRowList CreateConflictRowList()
        {
            var modelFactoryCurrent = new Mock<IConflictModelFactory>();
            var modelFactoryDifferent = new Mock<IConflictModelFactory>();
            var currentTree = new List<ConflictTreeNode>();
            var diffTree = new List<ConflictTreeNode>();
            return new ConflictRowList(modelFactoryCurrent.Object, modelFactoryDifferent.Object, currentTree, diffTree);
        }

        ConnectorConflictItem CreateConnectorConflictItem()
        {
            var rowList = CreateConflictRowList();
            return new ConnectorConflictItem(rowList, ConflictRowList.Column.Current, Grouping, ArmDescription, SourceUniqueId, DestinationUniqueId, Key);
        }
        private ConnectorConflictItem CreateConnectorConflictItemDiff()
        {
            var rowList = CreateConflictRowList();
            return new ConnectorConflictItem(rowList, ConflictRowList.Column.Different, Grouping, ArmDescription, SourceUniqueIdDiff, DestinationUniqueIdDiff, KeyDiff);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConnectorConflictItem_Constructor()
        {
            //------------Setup for test--------------------------
            var connectorConflictItem = CreateConnectorConflictItem();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(connectorConflictItem);
            Assert.AreEqual(ArmDescription, connectorConflictItem.ArmDescription);
            Assert.AreEqual("Arm", connectorConflictItem.LeftArmDescription);
            Assert.AreEqual("Arm", connectorConflictItem.RightArmDescription);
            Assert.AreEqual(SourceUniqueId, connectorConflictItem.SourceUniqueId);
            Assert.AreEqual(DestinationUniqueId, connectorConflictItem.DestinationUniqueId);
            Assert.AreEqual(Key, connectorConflictItem.Key);
            Assert.AreEqual(Grouping, connectorConflictItem.Grouping);
            Assert.IsTrue(connectorConflictItem.IsArmConnectorVisible);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConnectorConflictItem_Equals_Expected_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var connectorConflictItem = CreateConnectorConflictItem();
            var connectorConflictItemDiff = CreateConnectorConflictItem();
            //------------Assert Results-------------------------
            Assert.AreEqual(connectorConflictItem, connectorConflictItem);
            Assert.AreEqual(connectorConflictItemDiff, connectorConflictItemDiff);
            Assert.AreEqual(connectorConflictItem, connectorConflictItemDiff);
            Assert.AreEqual(connectorConflictItemDiff, connectorConflictItem);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConnectorConflictItem_Equals_Expected_False()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var connectorConflictItem = CreateConnectorConflictItem();
            var connectorConflictItemDiff = CreateConnectorConflictItemDiff();
            //------------Assert Results-------------------------
            Assert.AreEqual(connectorConflictItem, connectorConflictItem);
            Assert.AreEqual(connectorConflictItemDiff, connectorConflictItemDiff);
            Assert.AreNotEqual(connectorConflictItem, connectorConflictItemDiff);
            Assert.AreNotEqual(connectorConflictItemDiff, connectorConflictItem);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConnectorConflictItem_IsChecked_PropertyChanged()
        {
            //------------Setup for test--------------------------
            var connectorConflictItem = CreateConnectorConflictItem();

            //------------Execute Test---------------------------
            var wasCalled = false;
            connectorConflictItem.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsChecked")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsFalse(connectorConflictItem.IsChecked);
            connectorConflictItem.IsChecked = true;
            Assert.IsTrue(wasCalled);
        }
    }
}
