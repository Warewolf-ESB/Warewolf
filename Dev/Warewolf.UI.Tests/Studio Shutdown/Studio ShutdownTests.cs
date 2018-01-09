using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UI.Tests.Workflow
{
    [CodedUITest]
    public class Studio_ShutdownTests
    {
        [TestMethod]
        [TestCategory("Studio Shutdown")]
        public void Studio_Close_KillsProcess_UITest()
        {
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            UIMap.WaitForControlNotVisible(UIMap.MainStudioWindow.DockManager);
            Process studio = Process.GetProcesses().FirstOrDefault(process => process.ProcessName == "Warewolf Studio");            
            Assert.IsNull(studio, "Warewolf Studio is running in the background.");
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
