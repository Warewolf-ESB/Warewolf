using System.Collections.Generic;
using Dev2.Common;
using Dev2.Core.Tests.Merge.Utils;
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

            var conflictNodeCurrent = CreateConflictTreeNode(modelFactoryCurrent.Model);
            var conflictNodeDiff = CreateConflictTreeNode(modelFactoryDifferent.Model);

            var toolConlfictItem = ToolConflictItem.NewFromActivity(multiAssign, modelItem, location);

            var currentTree = new List<ConflictTreeNode> { conflictNodeCurrent };
            var diffTree = new List<ConflictTreeNode> { conflictNodeDiff };

            var conflictRowList = new ConflictRowList(modelFactoryCurrent, modelFactoryDifferent, currentTree, diffTree);

            Assert.AreEqual(1, conflictRowList.Count);

        }
    }
}
