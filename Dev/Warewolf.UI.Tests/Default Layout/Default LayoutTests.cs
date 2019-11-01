using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.Workflow
{
    [CodedUITest]
    public class Default_LayoutTests
    {
        [TestMethod]
        [TestCategory("Default Layout")]
        public void StudioLayout_ChangesSaved_UITest()
        {
            Process studio = Process.GetProcesses().FirstOrDefault(process => process.ProcessName == "Warewolf Studio");
            var fileName = studio?.MainModule.FileName;
            Console.WriteLine("WarewolfStudio Process : " + fileName);
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.LockunlockthemenuButton.UnlockMenuText.Exists, "Side Menu Bar is Open.");
            UIMap.Close_And_Lock_Side_Menu_Bar();
            var dockWidthBefore = UIMap.MainStudioWindow.DockManager.Width;
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            Playback.Wait(2000);
            ExecuteCommand(fileName);
            Playback.Wait(2000);
            Assert.IsTrue(UIMap.MainStudioWindow.Exists, "Warewolf studio is not running. You are expected to run \"Dev\\Warewolf.Launcher\\bin\\Debug\\Warewolf.Launcher.exe\" as an administrator and wait for it to complete before running any coded UI tests");
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow.DockManager);
            var dockWidthAfter = UIMap.MainStudioWindow.DockManager.Width;
            Assert.IsTrue(dockWidthBefore > dockWidthAfter, "Then Menu Bar did not Open/Close");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.LockunlockthemenuButton.UnlockMenuText.Exists, "Side Menu Bar is Open.");
        }

        static void ExecuteCommand(string fileName)
        {
            try
            {
                Process.Start(fileName);
            }
            catch(Exception)
            {
                Assert.Fail(fileName + "did not Start Correctly.");
            }
            
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        #endregion
    }
}
