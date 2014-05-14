using System.Globalization;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class DsfMultiAssignActivityTests : UIMapBase
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
        [TestCategory("ToolDesigners_AssignLargeView")]
        public void ToolDesigners_AssignLargeView_EnteringMultipleRows_IndexingWorksFine()
        {
            using(var dsfActivityUiMap = new DsfMultiAssignUiMap())
            {
                dsfActivityUiMap.ClickOpenLargeView();
                // Add the data!
                // moved from 100 to 10 for time
                for(int j = 0; j < 10; j++)
                {
                    dsfActivityUiMap.EnterTextIntoVariable(j, "[[theVar" + j.ToString(CultureInfo.InvariantCulture) + "]]");
                    dsfActivityUiMap.EnterTextIntoValue(j, j.ToString(CultureInfo.InvariantCulture));
                }

                // Click it
                Assert.AreEqual("[[theVar9]]", dsfActivityUiMap.GetTextFromVariable(9));
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_AssignSmallView")]
        public void ToolDesigners_AssignSmallView_EnteringMultipleRows_IndexingWorksFine()
        {
            using(var dsfActivityUiMap = new DsfMultiAssignUiMap())
            {

                // Add the data!
                // moved from 100 to 10 for time
                for(int j = 0; j < 10; j++)
                {
                    dsfActivityUiMap.EnterTextIntoVariable(j, "[[theVar" + j.ToString(CultureInfo.InvariantCulture) + "]]");
                    dsfActivityUiMap.EnterTextIntoValue(j, j.ToString(CultureInfo.InvariantCulture));
                }

                // Click it
                Assert.AreEqual("[[theVar9]]", dsfActivityUiMap.GetTextFromVariable(9));
            }
        }

    }
}
