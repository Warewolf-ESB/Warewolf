using Dev2.Studio.UI.Tests.Enums;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class ExplorerUITests : UIMapBase
    {
        #region Cleanup

        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }
        #endregion

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("RenameResource_WithDashes")]
        public void RenameResource_WithDashes_ResourceRenamed()
        {
            const string NewTestResourceWithDashes = "New-Test-Resource-With-Dashes";
            const string OldResourceName = "OldResourceName";
            ExplorerUIMap.RightClickRenameResource(OldResourceName, "Unassigned", ServiceType.Workflows, NewTestResourceWithDashes);
            Assert.IsTrue(ExplorerUIMap.ValidateWorkflowExists(NewTestResourceWithDashes, "Unassigned"));
            ExplorerUIMap.DoubleClickWorkflow(NewTestResourceWithDashes, "Unassigned");
        }
    }
}
