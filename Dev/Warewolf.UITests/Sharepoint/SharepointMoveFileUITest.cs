using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SharepointMoveFileUITest
    {
        [TestMethod]
        public void Sharepoint_Move_UITest()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Sharepoint_MoveFile_Onto_DesignSurface();
            UIMap.Select_NewSharepointSource_FromServer_Lookup_On_SharepointMoveFile_Tool();
            UIMap.Enter_Sharepoint_ServerSource_ServerName();
            UIMap.Click_UserButton_OnSharepointSource();
            UIMap.Enter_Sharepoint_ServerSource_User_Credentials();
            UIMap.Click_Sharepoint_Server_Source_TestConnection();
            UIMap.Click_Close_SharepointSource_Tab_Button();
            UIMap.Open_Sharepoint_MoveFile_Tool_Large_View();
            UIMap.Enter_Sharepoint_Server_Path_From_OnMoveFile_Tool();
            UIMap.Enter_Sharepoint_Server_Path_To_OnMoveFile_Tool();
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
        }
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
        }      

        UIMap UIMap
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
