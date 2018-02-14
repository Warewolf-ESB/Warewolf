using System.Collections.Generic;
using System.Windows;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Core.Tests.Merge.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Dev2.ViewModels.Merge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ConflictRowListTests : MergeTestUtils
    {
        [TestMethod]
        public void ConflictRowList_Constructor()
        {
            var modelFactoryCurrent = CreateCurrentConflictModelFactory();
            var modelFactoryDifferent = CreateDiffConflictModelFactory();

            var (currentTree, diffTree) = CreateConflictTreeNodeList();

            var conflictRowList = new ConflictRowList(modelFactoryCurrent, modelFactoryDifferent, currentTree, diffTree);

            Assert.AreEqual(2, conflictRowList.Count);

            Assert.IsNotNull(conflictRowList[0].Current);
            Assert.IsNotNull(conflictRowList[0].Different);
            Assert.IsNotNull(conflictRowList[0].UniqueId);
            Assert.IsNotNull(conflictRowList[0].Connectors);
            Assert.IsTrue(conflictRowList[0].HasConflict);
            Assert.IsTrue(conflictRowList[0].IsMergeVisible);
            Assert.IsFalse(conflictRowList[0].IsChecked);
            Assert.IsFalse(conflictRowList[0].ContainsStart);

            Assert.IsNotNull(conflictRowList[1].Current);
            Assert.IsNotNull(conflictRowList[1].Different);
            Assert.IsNotNull(conflictRowList[1].UniqueId);
            Assert.IsNotNull(conflictRowList[1].Connectors);
            Assert.IsTrue(conflictRowList[1].HasConflict);
            Assert.IsTrue(conflictRowList[1].IsMergeVisible);
            Assert.IsFalse(conflictRowList[1].IsChecked);
            Assert.IsFalse(conflictRowList[1].ContainsStart);
        }
    }
}
