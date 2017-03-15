using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

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
            Console.WriteLine("WarewolfStudio Process : " + fileName);
            Assert.IsTrue(UIMap.ControlExistsNow(UIMap.MainStudioWindow.SideMenuBar.LockunlockthemenuButton.UnlockMenuText), "Side Menu Bar is Open.");
            UIMap.Close_And_Lock_Side_Menu_Bar();
            var dockWidthBefore = UIMap.MainStudioWindow.DockManager.Width;
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            path = Directory.GetParent(path).ToString();
            Console.WriteLine("Layout file Path: " + path);
            var layOutFile = Environment.ExpandEnvironmentVariables(path + @"\AppData\Local\Warewolf\UserInterfaceLayouts\WorkspaceLayout.xml");
            if(File.Exists(layOutFile))
            {
                Console.WriteLine("Actual Layout file: " + fileName);
                File.Delete(layOutFile);
            }
            ExecuteCommand(fileName);
            UIMap.WaitForControlVisible(UIMap.MainStudioWindow);
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
