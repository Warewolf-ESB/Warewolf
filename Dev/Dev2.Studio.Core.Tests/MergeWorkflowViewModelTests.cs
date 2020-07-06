using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Core.Tests.Merge.Utils;
using System.Linq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MergeWorkflowViewModelTests : MergeTestUtils
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void MergeWorkflowViewModel_Constructor()
        {
            var mergeWorkflowViewModel = CreateMergeWorkflowViewModel();

            Assert.IsNotNull(mergeWorkflowViewModel);
            Assert.IsNotNull(mergeWorkflowViewModel.MergePreviewWorkflowDesignerViewModel);
            Assert.IsNotNull(mergeWorkflowViewModel.Conflicts);
            Assert.AreEqual(6, mergeWorkflowViewModel.Conflicts.Count());

            Assert.IsNotNull(mergeWorkflowViewModel.ModelFactoryCurrent.WorkflowName);
            Assert.IsNotNull(mergeWorkflowViewModel.ModelFactoryDifferent.WorkflowName);
        }
    }
}