using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SharepointCopyFileUITest
    {
        [TestMethod]
        public void Sharepoint_Copy_UITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_Sharepoint_CopyFile_Onto_DesignSurface();
            Uimap.Select_NewSharepointSource_FromServer_Lookup_On_SharepointCopyFile_Tool();
            Uimap.Enter_Sharepoint_ServerSource_ServerName();
            Uimap.Click_UserButton_OnSharepointSource();
            Uimap.Enter_Sharepoint_ServerSource_User_Credentials();
            Uimap.Click_Sharepoint_Server_Source_TestConnection();
            Uimap.Click_Close_SharepointSource_Tab_Button();
            Uimap.Click_MessageBox_No();
            Uimap.Open_Sharepoint_Copy_Tool_Large_View();
            Uimap.Enter_Sharepoint_Server_Path_From_OnCopyFile_Tool();
            Uimap.Enter_Sharepoint_Server_Path_To_OnCopyFile_Tool();
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_MessageBox_No();
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
