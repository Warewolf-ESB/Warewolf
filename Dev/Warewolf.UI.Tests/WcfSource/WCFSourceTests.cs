using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WcfSource.WcfSourceUIMapClasses;
using System.ServiceModel;
using System;
using System.Diagnostics;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class WcfSourceTests
    {
        ServiceHost host;
        const string hostName = "http://localhost:3144/UITestWcfEndpointService";

        [TestCleanup]
        public void Close_WCF_Endpoint()
        {
            host.Close();
        }

        [TestMethod]
        [TestCategory("Source Wizards")]
        public void Create_WcfSource_From_ExplorerContextMenu_UITests()
        {
            ExplorerUIMap.Select_NewWcfSource_From_ExplorerContextMenu();
            Assert.IsTrue(WcfSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.Exists, "WCF Source Tab does now exist");
            Assert.IsTrue(WcfSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.WCFEndpointURLEdit.Enabled, "WCF Endpoint URL Textbox is not enabled");
            Assert.IsFalse(WcfSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WCFServiceSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            WcfSourceUIMap.Enter_TextIntoAddress_On_WCFServiceTab(hostName);
            WcfSourceUIMap.Click_WCFServiceSource_TestConnectionButton();
        }
        
        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            host = new ServiceHost(typeof(UITestWcfEndpointService));
            host.Open();
            UIMap.SetPlaybackSettings();
            Debugger.Break();
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

        WcfSourceUIMap WcfSourceUIMap
        {
            get
            {
                if (_WcfSourceUIMap == null)
                {
                    _WcfSourceUIMap = new WcfSourceUIMap();
                }

                return _WcfSourceUIMap;
            }
        }

        private WcfSourceUIMap _WcfSourceUIMap;

        #endregion
    }

    [ServiceContract]
    public interface IUITestWcfEndpoint
    {
        [OperationContract]
        bool Connected();
    }

    public class UITestWcfEndpointService : IUITestWcfEndpoint
    {
        public bool Connected()
        {
            return true;
        }
    }
}