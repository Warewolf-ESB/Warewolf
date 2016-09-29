﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class MenuUITests : UIMapBase
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
            Halt();
        }

        #endregion

        [TestMethod]
        public void DebugAWorkFlow_EnsureSaveIsEnabledAfterCompletion()
        {

            ExplorerUIMap.EnterExplorerSearchText("ServiceExecutionTest");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "TestCategory", "ServiceExecutionTest");

            PopupDialogUIMap.WaitForDialog();
            KeyboardCommands.SendKey("{F5}");
            PopupDialogUIMap.WaitForDialog();
            KeyboardCommands.SendKey("{F6}");
            PopupDialogUIMap.WaitForDialog();

            var uiControl = RibbonUIMap.GetControlByName("Save");

            uiControl.WaitForControlReady();

            Assert.IsTrue(uiControl.Enabled);
        }
    }
}
