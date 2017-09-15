using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Merge
{
    [TestClass]
    public class MergeWorkflowViewModelIntergrationTests
    {
        [TestMethod]
        public void Constructor_GivenIsNew_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var mergeWorkflowViewModel = new MergeWorkflowViewModel(null, null, null);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
        }
    }
}
