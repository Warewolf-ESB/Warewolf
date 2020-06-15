using Dev2.Core.Tests.Merge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ToolConflictRowTests : MergeTestUtils
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [DoNotParallelize]
        public void ToolConflictRow_CreateConflictRow()
        {
            var conflictRow = CreateConflictRow();

            Assert.IsNotNull(conflictRow.Current);
            Assert.IsNotNull(conflictRow.Different);
            Assert.IsNotNull(conflictRow.Connectors);

            Assert.AreNotEqual(conflictRow.Current, conflictRow.Different);
            Assert.AreNotEqual(conflictRow.Different, conflictRow.Current);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictRow_CreateStartRow()
        {
            var conflictRow = CreateStartRow();

            Assert.IsNotNull(conflictRow.Current);
            Assert.IsNotNull(conflictRow.Different);

            Assert.AreEqual(conflictRow.Current, conflictRow.Different);
            Assert.AreEqual(conflictRow.Different, conflictRow.Current);
        }
    }
}
