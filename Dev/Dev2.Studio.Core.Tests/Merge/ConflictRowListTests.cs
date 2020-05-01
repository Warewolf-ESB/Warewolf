using Dev2.Core.Tests.Merge.Utils;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ConflictRowListTests : MergeTestUtils
    {
        [TestMethod]
        [DoNotParallelize]
        public void ConflictRowList_Constructor()
        {
            var conflictRowList = CreateConflictRowList();

            Assert.AreEqual(6, conflictRowList.Count);
            Assert.AreEqual(6, conflictRowList.Count);

            Assert.IsNotNull(conflictRowList[0].Current);
            Assert.IsNotNull(conflictRowList[0].Different);

            Assert.IsNotNull(conflictRowList[1].Current);
            Assert.IsNotNull(conflictRowList[1].Different);

            Assert.AreEqual(conflictRowList[0].Current, conflictRowList[0].Different);
            Assert.AreEqual(conflictRowList[0].Different, conflictRowList[0].Current);

            Assert.AreNotEqual(conflictRowList[0].Current, conflictRowList[1].Different);
            Assert.AreNotEqual(conflictRowList[0].Different, conflictRowList[1].Current);

            Assert.AreNotEqual(conflictRowList[1].Current, conflictRowList[0].Different);
            Assert.AreNotEqual(conflictRowList[1].Different, conflictRowList[0].Current);

            var toolConflictRow = conflictRowList[0] as ToolConflictRow;
            if (toolConflictRow != null)
            {
                Assert.IsNotNull(toolConflictRow.UniqueId);
                Assert.IsNotNull(toolConflictRow.Connectors);
                Assert.IsFalse(toolConflictRow.HasConflict);
            } else
            {
                Assert.Fail();
            }

            var toolConflictRow2 = conflictRowList[2] as ToolConflictRow;
            if (toolConflictRow2 != null)
            {
                Assert.IsNotNull(toolConflictRow2.UniqueId);
                Assert.IsNotNull(toolConflictRow2.Connectors);
                Assert.IsFalse(toolConflictRow2.HasConflict);
            } else
            {
                Assert.Fail();
            }

            // Validate User Interface properties are Visible
            Assert.IsFalse(toolConflictRow.IsMergeVisible);
            Assert.IsFalse(toolConflictRow2.IsMergeVisible);
        }
    }
}
