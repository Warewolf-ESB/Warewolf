
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
            ExplorerUIMap.RightClickRenameResource(OldResourceName, "Unassigned", NewTestResourceWithDashes);
            Assert.IsTrue(ExplorerUIMap.ValidateWorkflowExists(NewTestResourceWithDashes, "Unassigned"));
            ExplorerUIMap.DoubleClickWorkflow(NewTestResourceWithDashes, "Unassigned");
        }
    }
}
