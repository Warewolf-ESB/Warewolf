using Dev2.Core.Tests.Merge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ConflictRowListTests : MergeTestUtils
    {
        [TestMethod]
        public void ConflictRowList_Constructor()
        {
            var conflictRowList = CreateConflictRowList();

            Assert.AreEqual(2, conflictRowList.Count);
            Assert.AreEqual(2, conflictRowList.Count);

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

            Assert.IsNotNull(conflictRowList[0].UniqueId);
            Assert.IsNotNull(conflictRowList[0].Connectors);
            Assert.IsFalse(conflictRowList[0].HasConflict);

            Assert.IsNotNull(conflictRowList[1].UniqueId);
            Assert.IsNotNull(conflictRowList[1].Connectors);
            Assert.IsFalse(conflictRowList[1].HasConflict);

            // Validate User Interface properties are Visible
            Assert.IsFalse(conflictRowList[0].IsMergeVisible);
            Assert.IsFalse(conflictRowList[1].IsMergeVisible);
        }
    }
}
