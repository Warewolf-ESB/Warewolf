using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Workflow
{
    [CodedUITest]
    public class Default_LayoutTests
    {
        [TestMethod]
        [TestCategory("Default Layout")]
        public void Studio_Default_Layout_UITest()
        {
            Process studio = Process.GetProcesses().FirstOrDefault(process => process.ProcessName == "Warewolf Studio");
            var fileName = studio?.MainModule.FileName;
            Assert.IsTrue(UIMap.ControlExistsNow(UIMap.MainStudioWindow.SideMenuBar.LockunlockthemenuButton.UnlockMenuText), "Side Menu Bar is Open.");
            var dockWidthBefore = UIMap.MainStudioWindow.DockManager.Width;
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            path = Directory.GetParent(path).ToString();
            var layOutFile = Environment.ExpandEnvironmentVariables(path + @"\AppData\Local\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml");
            if(File.Exists(layOutFile))
                File.Delete(layOutFile);
            ExecuteCommand(fileName);
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow);
            var dockWidthAfter = UIMap.MainStudioWindow.DockManager.Width;
            Assert.IsTrue(dockWidthBefore > dockWidthAfter);
            Assert.IsTrue(UIMap.ControlExistsNow(UIMap.MainStudioWindow.SideMenuBar.LockunlockthemenuButton.UnlockMenuText), "Side Menu Bar is Open.");
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

        #endregion
    }
}
