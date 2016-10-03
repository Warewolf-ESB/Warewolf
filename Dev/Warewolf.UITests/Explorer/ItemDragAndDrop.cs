using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ItemDragAndDropTest
    {        
        [TestMethod]
        public void ItemDragAndDropUITest()
        {            
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests\Acceptance Testing Resources";
            Assert.IsFalse(Directory.Exists(resourcesFolder));
            Uimap.Move_AcceptanceTestd_To_AcceptanceTestingResopurces();
            Assert.IsTrue(Directory.Exists(resourcesFolder));
        }

        [TestMethod]
        public void DebugUsingPlayIconRemoteServerUITest()
        {
            Uimap.Filter_Explorer("Hello World");
            Mouse.Hover(Uimap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
            Uimap.Debug_Using_Play_Icon();
            Uimap.Click_DebugInput_Debug_Button();
        }

        [TestMethod]
        public void DisconnectedRemoteServerUITest()
        {

        }
    
        [TestMethod]
        public void ShowDependenciesUITest()
        {            
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu("Hello World");
            Uimap.Click_Close_Dependecy_Tab();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu("SharepointPlugin");
            Uimap.Click_Close_Dependecy_Tab();
            Uimap.Select_Show_Dependencies_In_Explorer_Context_Menu("MySQLDATA");
            Uimap.Click_Close_Dependecy_Tab();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }


        [TestCleanup()]
        public void MyTestCleanup()
        {
            var destinationDirectory = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
            var sourceDirectory = destinationDirectory + @"\Acceptance Tests\Acceptance Testing Resources";
            try
            {
                Directory.Move(sourceDirectory, destinationDirectory);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
UIMap Uimap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
