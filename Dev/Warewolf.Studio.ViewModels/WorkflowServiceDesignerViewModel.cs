using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;

namespace Warewolf.Studio.ViewModels
{
    public class WorkflowServiceDesignerViewModel : BindableBase, IWorkflowServiceDesignerViewModel, IActiveAware, IDockAware
    {
        /// <summary>
        /// The resource that is being represented
        /// </summary>
        public IResource Resource
        {
            get;
            set;
        }
        /// <summary>
        /// Should the hyperlink to execute the service in browser
        /// </summary>
        public bool IsServiceLinkVisible
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Command to execute when the hyperlink is clicked
        /// </summary>
        public ICommand OpenWorkflowLinkCommand
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// The hyperlink text shown
        /// </summary>
        public string DisplayWorkflowLink
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// The designer for the resource
        /// </summary>
        public IDesignerView DesignerView
        {
            get
            {
                return null;
            }
        }

        #region Implementation of IActiveAware

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the object is active; otherwise <see langword="false"/>.
        /// </value>
        public bool IsActive
        {
            get
            {
                return false;
            }
            set
            {
            }
        }
        public event EventHandler IsActiveChanged;

        #endregion

        #region Implementation of IDockAware

        public string Header
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        #endregion
    }
}
