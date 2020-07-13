using Dev2.Core.Tests.Merge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ConflictListStateApplierTests : MergeTestUtils
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void MergePreviewWorkflowStateApplier_Constructor()
        {
            var conflictListStateApplier = CreateConflictListStateApplier();
            Assert.IsNotNull(conflictListStateApplier);
        }
    }
}
