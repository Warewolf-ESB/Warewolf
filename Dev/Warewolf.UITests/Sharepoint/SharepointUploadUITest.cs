using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SharepointUploadUITest
    {
        [TestMethod]
        public void Sharepoint_Upload_UITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_Sharepoint_UploadFile_Onto_DesignSurface();
            Uimap.Select_NewSharepointSource_FromServer_Lookup_On_SharepointUpload_Tool();
            Uimap.Enter_Sharepoint_ServerSource_ServerName();
            Uimap.Click_UserButton_OnSharepointSource();
            Uimap.Enter_Sharepoint_ServerSource_User_Credentials();
            Uimap.Click_Sharepoint_Server_Source_TestConnection();
            Uimap.Click_Close_SharepointSource_Tab_Button();
            Uimap.Open_Sharepoint_Upload_Tool_Large_View();
            Uimap.Enter_Sharepoint_Server_Path_From_OnUpload_Tool();
            Uimap.Enter_Sharepoint_Server_Path_To_OnUpload_Tool();
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
