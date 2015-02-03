using System;
using System.Activities.Presentation;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class WorkflowServiceDesignerViewModel : BindableBase, IWorkflowServiceDesignerViewModel, IDockViewModel
    {
        bool _isActive;
        string _header;
        readonly WorkflowDesigner _wd;
        private ResourceType? _image;

        public WorkflowServiceDesignerViewModel(IXamlResource resource)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            Resource = resource;
            _wd = new WorkflowDesigner();
            DesignerView = _wd.View;
            if (resource.Xaml == null)
            {
                IsNewWorkflow = true;
            }
            else
            {
                IsNewWorkflow = false;
                _wd.Text = resource.Xaml.ToString();
            }
        }

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
        public UIElement DesignerView
        {
            get;
            private set;
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
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged(() => IsActive);
            }
        }
        public event EventHandler IsActiveChanged;

        #endregion

        #region Implementation of IDockAware

        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
                OnPropertyChanged(() => Header);
            }
        }

        public ResourceType? Image
        {
            get { return ResourceType.WorkflowService; }
        }

        public bool IsNewWorkflow { get; set; }
        public string DesignerText
        {
            get
            {
                return _wd.Text;
            }
        }

        #endregion
    }
}
