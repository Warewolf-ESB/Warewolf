using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.VariableList
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class VariableListUITests : UIMapBase
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

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("VariableListTests")]
        public void VariableList_DeleteAColumnOffARecorset_DeleteAllButtonIsEnbaled()
        {
            var multiAssign = new DsfMultiAssignUiMap();

            multiAssign.EnterTextIntoVariable(0, "[[rec().a]]");
            multiAssign.EnterTextIntoVariable(1, "[[rec().b]]");
            multiAssign.EnterTextIntoVariable(2, "[[rec().c]]");
            multiAssign.EnterTextIntoVariable(1, "[[b]]");
            multiAssign.EnterTextIntoVariable(3, "[[c]]");

            Assert.IsTrue(VariablesUIMap.IsDeleteAllEnabled());
        }

        #endregion
    }
}
