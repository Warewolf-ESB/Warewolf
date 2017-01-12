using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Workflow
{
    [CodedUITest]
    public class WorkflowLayoutTests
    {
        public string warewolfStudioProcess;
        public string WarewolfStudioProcess
        {
            get
            {
                if(warewolfStudioProcess == null)
                {
                    warewolfStudioProcess = Studio.MainModule.FileName;
                }
                return warewolfStudioProcess;
            }
        }
        public Process _studio;
        public Process Studio
        {
            get
            {
                if(_studio == null)
                {
                    _studio = Process.GetProcesses().FirstOrDefault(process => process.ProcessName == "Warewolf Studio");
                }
                return _studio;
            }
        }

        [TestMethod]
        [TestCategory("Workflow Layout")]
        public void Workflow_Default_Layout()
        {
            Assert.IsTrue(UIMap.ControlExistsNow(UIMap.UIWarewolfDEV2SANELEMTWindow.UIItemCustom.LockunlockthemenuButton.UnlockMenuTextBox), "Side Menu Bar is Open.");
            var dockWidthBefore = UIMap.MainStudioWindow.DockManager.Width;
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            path = Directory.GetParent(path).ToString();
            var layOutFile = Environment.ExpandEnvironmentVariables(path + @"\AppData\Local\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml");
            if (File.Exists(layOutFile))
                File.Delete(layOutFile);
            ExecuteCommand(WarewolfStudioProcess);
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow);
            var dockWidthAfter = UIMap.MainStudioWindow.DockManager.Width;
            Assert.IsTrue(dockWidthBefore > dockWidthAfter);
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            ExecuteCommand(WarewolfStudioProcess);
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow);
            Assert.IsTrue(UIMap.ControlExistsNow(UIMap.UIWarewolfDEV2SANELEMTWindow.UIItemCustom.LockunlockthemenuButton.UnlockMenuTextBox), "Side Menu Bar is Closed.");
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.UIWarewolfDEV2SANELEMTWindow.UIItemCustom.LockunlockthemenuButton.LockMenuTextBox), "Side Menu Bar is Closed.");
        }

        static void ExecuteCommand(string fileName)
        {
            Process.Start(fileName);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            Assert.IsNotNull(WarewolfStudioProcess);
            Assert.IsNotNull(Studio);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            ExecuteCommand(WarewolfStudioProcess);
        }

        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
