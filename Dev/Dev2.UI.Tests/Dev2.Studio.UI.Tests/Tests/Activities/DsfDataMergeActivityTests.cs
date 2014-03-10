using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfDataMergeActivity
    /// </summary>
    [CodedUITest]
    // ReSharper disable InconsistentNaming
    public class DsfDataMergeActivityTests : UIMapBase
    {
        #region Fields


        #endregion

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
        }

        #endregion

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ToolDesigners_DataMergeQuickVariableInput")]
        public void ToolDesigners_DataMergeQuickVariableInput_AddVariablesInTwoActivities_QVIContinuesToWork()
        {
            var firstActivity = new DsfDataMergeUiMap();
            firstActivity.ClickOpenQuickVariableInput();
            firstActivity.EnterVariables("aa,bb,cc,dd,ee,ff,gg,hh,ii,jj");
            firstActivity.SelectSplitOn(1);
            firstActivity.EnterSplitCharacter(",");
            firstActivity.ClickAddButton();

            var secondActivity = new DsfDataSplitUiMap(false, false);
            secondActivity.TheTab = firstActivity.TheTab;
            secondActivity.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.DataSplit, firstActivity.TheTab, new Point(firstActivity.Activity.BoundingRectangle.X + 5, firstActivity.Activity.BoundingRectangle.Y + 100));

            secondActivity.ClickOpenQuickVariableInput();
            secondActivity.EnterVariables("aa,bb,cc,dd,ee,ff,gg,hh,ii,jj");
            secondActivity.SelectSplitOn(1);
            secondActivity.EnterSplitCharacter(",");
            secondActivity.ClickAddButton();

            firstActivity.ClickOpenQuickVariableInput();
            firstActivity.EnterVariables("XX,YY,ZZ");
            firstActivity.SelectSplitOn(1);
            firstActivity.EnterSplitCharacter(",");
            firstActivity.SelectReplaceOption();
            firstActivity.ClickAddButton();

            var firstVariable = firstActivity.GetVariable(0);
            var secondVariable = firstActivity.GetVariable(1);
            var thirdVariable = firstActivity.GetVariable(2);

            Assert.AreEqual("[[XX]]", firstVariable);
            Assert.AreEqual("[[YY]]", secondVariable);
            Assert.AreEqual("[[ZZ]]", thirdVariable);
        }
    }
}
