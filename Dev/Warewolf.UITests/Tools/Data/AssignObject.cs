using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Data
{
    /// <summary>
    /// Summary description for WorkflowDesignSurface
    /// </summary>
    [CodedUITest]
    public class AssignObject
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void AssignObjectUITest()
        {            
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_AssignObject_Onto_DesignSurface();
            Uimap.Open_AssignObject_Large_Tool();
            Uimap.Enter_Person_Name_On_Assign_Object_tool();
            Uimap.Enter_Person_Age_On_Assign_Object_tool();
        }

        #region Additional test attributes
        
        [TestInitialize]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_MessageBox_No();
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
