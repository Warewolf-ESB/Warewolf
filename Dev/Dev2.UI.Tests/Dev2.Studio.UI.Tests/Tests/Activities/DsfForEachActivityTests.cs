
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Drawing;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class DsfForEachActivityTests : UIMapBase
    {

        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            RestartStudioOnFailure();
        }

        #endregion


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_ForEach")]
        public void ToolDesigners_ForEach_DraggingDecision_NotAllowed()
        {
            using(var dsfActivityUiMap = new DsfForEachUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Decision);

                var forEachActivity = dsfActivityUiMap.GetActivity();
                Assert.IsNull(forEachActivity);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_ForEach")]
        public void ToolDesigners_ForEach_DraggingSwitch_NotAllowed()
        {
            using(var dsfActivityUiMap = new DsfForEachUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Switch);

                var forEachActivity = dsfActivityUiMap.GetActivity();
                Assert.IsNull(forEachActivity);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_ForEach")]
        public void ToolDesigners_ForEach_DraggingNonDecision_Allowed()
        {
            using(var dsfActivityUiMap = new DsfForEachUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Assign);

                var forEachActivity = dsfActivityUiMap.GetActivity();
                Assert.IsNotNull(forEachActivity);
            }
        }

    }
}
