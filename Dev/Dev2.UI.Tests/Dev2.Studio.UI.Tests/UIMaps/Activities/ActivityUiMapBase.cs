using Dev2.Studio.UI.Tests.Enums;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dev2.Studio.UI.Tests.UIMaps.Activities
{
    public abstract class ActivityUiMapBase : UIMapBase, IDisposable
    {
        private UITestControl _activity;
        private UITestControl _theTab;

        protected ActivityUiMapBase(bool createNewtab = true, int waitAmt = 1000)
        {
            if(createNewtab)
            {
                _theTab = RibbonUIMap.CreateNewWorkflow();
                Playback.Wait(waitAmt);
            }
        }

        #region Properties

        public UITestControl Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                _activity = value;
            }
        }

        public UITestControl TheTab
        {
            get
            {
                return _theTab;
            }
            set
            {
                _theTab = value;
            }
        }

        #endregion

        #region Public Methods

        public void DragToolOntoDesigner(ToolType toolType, Point pointToDragTo = new Point())
        {
            if(toolType == ToolType.Workflow || toolType == ToolType.Service)
            {
                ToolboxUIMap.DragControlToWorkflowDesigner(toolType, TheTab, new Point(), false);
                PopupDialogUIMap.WaitForDialog();
            }
            else
            {
                Activity = ToolboxUIMap.DragControlToWorkflowDesigner(toolType, TheTab);
            }
        }

        public void DragWorkflowOntoDesigner(string workflowName, string categoryName, string serverName = "localhost", Point pointToDragTo = new Point())
        {
            ExplorerUIMap.EnterExplorerSearchText(workflowName);
            Activity = ExplorerUIMap.DragResourceOntoWorkflowDesigner(TheTab, workflowName, categoryName, ServiceType.Workflows, serverName, pointToDragTo);
        }

        public void DragServiceOntoDesigner(string serviceName, string categoryName, string serverName = "localhost", Point pointToDragTo = new Point())
        {
            ExplorerUIMap.EnterExplorerSearchText(serviceName);
            Activity = ExplorerUIMap.DragResourceOntoWorkflowDesigner(TheTab, serviceName, categoryName, ServiceType.Services, serverName, pointToDragTo);
        }


        public string GetHelpText()
        {
            UITestControl parentControl = Activity.GetParent();
            List<UITestControl> uiTestControlCollection = parentControl.GetChildren().Where(c => c.ControlType == ControlType.Text).ToList();
            try
            {
                WpfText lbl = (WpfText)uiTestControlCollection[1];
                return lbl.DisplayText;
            }
            catch(Exception)
            {
                throw new Exception("The Help text couldn't be found, probably because of the activity not expanding on drop.");
            }
        }

        public void ClickHelp()
        {
            UITestControl button = AdornersGetButton("Open Help");
            Mouse.Click(button, new Point(5, 5));
        }

        public UITestControl AdornersGetButton(string adornerFriendlyName)
        {
            UITestControlCollection testFlowChildCollection = Activity.GetChildren();
            return testFlowChildCollection.FirstOrDefault(theControl => theControl.FriendlyName == adornerFriendlyName);
        }


        #endregion

        #region Private Methods


        #endregion

        #region Implementation of IDisposable

        bool _isDisposed;

        ~ActivityUiMapBase()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    OnDisposed();
                }

                // Dispose unmanaged resources.
                OnDisposedUnmanaged();

                _isDisposed = true;
            }
        }

        protected virtual void OnDisposed()
        {
        }

        protected virtual void OnDisposedUnmanaged()
        {
        }

        #endregion
    }
}
