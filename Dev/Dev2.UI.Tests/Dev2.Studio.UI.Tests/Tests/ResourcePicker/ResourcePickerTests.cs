using System.Windows.Forms;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.Tests.ResourcePicker
{
    /// <summary>
    /// Summary description for DbServiceTests
    /// </summary>
    [CodedUITest]
    public class ResourcePickerTests : UIMapBase
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
            Halt();
        }
        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ResourcePickerTests_CodedUI")]
        public void ResourcePickerTests_CodedUI_DropWorkflowFromToolbox_ExpectResourcePickerToBehaveCorrectly()
        {
            DsfActivityUiMap dsfActivityUiMap = new DsfActivityUiMap();
            dsfActivityUiMap.DragToolOntoDesigner(ToolType.Workflow);

            #region Checking Ok Button enabled property

            Assert.IsFalse(ActivityDropUIMap.IsOkButtonEnabled());

            ActivityDropUIMap.SingleClickResource("Example", "Control Flow - Decision");

            Assert.IsTrue(ActivityDropUIMap.IsOkButtonEnabled());

            ActivityDropUIMap.SingleClickAFolder("Example");

            Assert.IsFalse(ActivityDropUIMap.IsOkButtonEnabled());

            #endregion

            #region Checking the double click of a resource puts it on the design surface

            //Select a resource in the explorer view
            ActivityDropUIMap.SelectAResourceAndClickOk("Example", "Control Flow - Decision");

            // Check if it exists on the designer
            bool doesControlExistOnWorkflowDesigner = WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(dsfActivityUiMap.TheTab, "Control Flow - Decision");
            Assert.IsTrue(doesControlExistOnWorkflowDesigner);
            SendKeys.SendWait("{DELETE}");

            #endregion

            #region Checking the click of the Cacnel button doesnt Adds the resource to the design surface

            // And drag it onto the point
            dsfActivityUiMap.DragToolOntoDesigner(ToolType.Workflow);

            // Single click a folder in the tree
            ActivityDropUIMap.SingleClickResource("Example", "Control Flow - Decision");

            // Click the Ok button on the window
            ActivityDropUIMap.ClickCancelButton();

            // Check if it exists on the designer
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(dsfActivityUiMap.TheTab, "Control Flow - Decision"));

            #endregion
        }
    }
}
