using Dev2.Core.Tests.Merge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ConflictListStateApplierTests : MergeTestUtils
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ConflictListStateApplier_Constructor()
        {
            Dev2.Net6.Compatibility.STAThreadExtensions.RunAsSTA(()=> {
                var conflictListStateApplier = CreateConflictListStateApplier();
                Assert.IsNotNull(conflictListStateApplier);
            });
        }
    }
}
