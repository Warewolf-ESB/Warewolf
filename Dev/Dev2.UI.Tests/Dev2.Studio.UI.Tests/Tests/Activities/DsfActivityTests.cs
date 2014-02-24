using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class DsfActivityTests : UIMapBase
    {
        #region Fields

        private static DsfActivityUiMap _dsfActivityUiMap;

        #endregion

        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
            _dsfActivityUiMap = new DsfActivityUiMap();
        }

        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            _dsfActivityUiMap.Dispose();
        }

        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DsfActivityTests_CodedUI")]
        public void DsfActivityTests_CodedUI_SetInitialFocusElementIfNoInputs_HelpTextIsNotEmpty()
        {
            string expectedHelpText = @"Only variables go in here.
Insert the variable that you want the output of the workflow to be mapped into. By default similar matches from the variable list are used where possible.
You can use [[Scalar]] as well as [[Recordset().Fields]].
Using recordset () will add a new record and (*) will assign every record.";

            _dsfActivityUiMap.DragWorkflowOntoDesigner("Sql Bulk Insert Test", "TEST");
            _dsfActivityUiMap.ClickHelp();
            string actualHelpText = _dsfActivityUiMap.GetHelpText();
            Assert.AreEqual(expectedHelpText, actualHelpText);
        }
    }
}
