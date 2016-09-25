using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Deploy
{
    [CodedUITest]
    public class DeployViewOnlyService
    {
        private const string WorkflowName = "DeployViewOnly";
        private const string GroupName = "Public";

        [TestMethod]
        public void DeployViewOnlyServiceUITest()
        {
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Save_With_Ribbon_Button_And_Dialog(WorkflowName);
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.SetResourcePermissions(WorkflowName, GroupName, true);
            Uimap.Click_Deploy_Ribbon_Button();
            Uimap.Select_RemoteConnectionIntegration_From_Deploy_Tab_Destination_Server_Combobox();
            Uimap.Click_Deploy_Tab_Destination_Server_Connect_Button();
            Uimap.Deploy_Service_From_Deploy_View(WorkflowName);
            Uimap.Select_RemoteConnectionIntegrationConnected_From_Deploy_Tab_Source_Server_Combobox();
            Uimap.Select_LocalhostConnected_From_Deploy_Tab_Destination_Server_Combobox();
            Uimap.Deploy_Service_From_Deploy_View(WorkflowName);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {            
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
            Console.WriteLine("Test \"" + TestContext.TestName + "\" starting on " + Environment.MachineName);
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            Playback.PlaybackError -= Uimap.OnError;
            Uimap.TryCloseDeployTab();
            Uimap.TryCloseSettingsTab();
            Uimap.TryCloseWorkflowTab();
            Uimap.TryRemoveFromExplorer(WorkflowName);
        }

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;

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