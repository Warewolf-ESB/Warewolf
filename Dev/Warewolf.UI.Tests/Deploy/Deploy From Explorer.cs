﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Deploy.DeployUIMapClasses;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class Deploy_From_Explorer
    {        
        [TestMethod]
        [TestCategory("Deploy from Explorer")]
        public void Deploying_From_Explorer_Opens_The_Deploy_With_Resource_Already_Checked()
        {
            ExplorerUIMap.Filter_Explorer("Hello World");
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            ExplorerUIMap.Select_Deploy_From_ExplorerContextMenu();
            DeployUIMap.WhenISelectFromTheSourceTab("Hello World");
            DeployUIMap.ThenFilteredResourseIsCheckedForDeploy();
        }

        [TestMethod]
        [TestCategory("Deploy from Explorer")]
        public void Deploying_From_Explorer_Opens_The_Deploy_With_All_Resources_In_Folder_Already_Checked()
        {
            ExplorerUIMap.Filter_Explorer("Unit Tests");
            ExplorerUIMap.RightClick_Explorer_Localhost_FirstItem();
            ExplorerUIMap.Select_Deploy_From_ExplorerContextMenu();
            DeployUIMap.Enter_DeployViewOnly_Into_Deploy_Source_Filter("Unit Tests");
            DeployUIMap.ThenFilteredResourseIsCheckedForDeploy();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        public UIMap UIMap
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

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

        DeployUIMap DeployUIMap
        {
            get
            {
                if (_DeployUIMap == null)
                {
                    _DeployUIMap = new DeployUIMap();
                }

                return _DeployUIMap;
            }
        }

        private DeployUIMap _DeployUIMap;

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        ServerSourceUIMap ServerSourceUIMap
        {
            get
            {
                if (_ServerSourceUIMap == null)
                {
                    _ServerSourceUIMap = new ServerSourceUIMap();
                }

                return _ServerSourceUIMap;
            }
        }

        private ServerSourceUIMap _ServerSourceUIMap;
        #endregion
    }
}
