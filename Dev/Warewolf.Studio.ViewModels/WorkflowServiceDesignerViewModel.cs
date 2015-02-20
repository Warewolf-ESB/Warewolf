using System;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Utilities;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class WorkflowServiceDesignerViewModel : BindableBase, IWorkflowServiceDesignerViewModel, IDockViewModel
    {
        bool _isActive;
        string _header;
        readonly WorkflowDesigner _wd;

        public WorkflowServiceDesignerViewModel(IXamlResource resource)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            Resource = resource;
            _wd = new WorkflowDesigner();
            var hashTable = new Hashtable
                {
                    {WorkflowDesignerColors.FontFamilyKey, Application.Current.Resources["DefaultFontFamily"]},
                    {WorkflowDesignerColors.FontSizeKey, Application.Current.Resources["FontSize-Normal"]},
                    {WorkflowDesignerColors.FontWeightKey, Application.Current.Resources["DefaultFontWeight"]},
                    {WorkflowDesignerColors.RubberBandRectangleColorKey, Application.Current.Resources["DesignerBackground"]},
                    {WorkflowDesignerColors.WorkflowViewElementBackgroundColorKey, Application.Current.Resources["WorkflowBackgroundBrush"]},
                    {WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey, Application.Current.Resources["WorkflowBackgroundBrush"]},
                    {WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey, Application.Current.Resources["WorkflowSelectedBorderBrush"]},
                    {WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.OutlineViewItemSelectedTextColorKey, Application.Current.Resources["SolidWhite"]},
                    {WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey, Application.Current.Resources["DesignerBackground"]},
                    
                };
            _wd.PropertyInspectorFontAndColorData = XamlServices.Save(hashTable);
            var designerConfigService = _wd.Context.Services.GetService<DesignerConfigurationService>();
            if (designerConfigService != null)
            {
                // set the runtime Framework version to 4.5 as new features are in .NET 4.5 and do not exist in .NET 4
                designerConfigService.TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 5));
                designerConfigService.AutoConnectEnabled = true;
                designerConfigService.AutoSplitEnabled = true;
                designerConfigService.PanModeEnabled = true;
                designerConfigService.RubberBandSelectionEnabled = true;
                designerConfigService.BackgroundValidationEnabled = true; // prevent design-time background validation from blocking UI thread
                // Disabled for now
                designerConfigService.AnnotationEnabled = false;
                designerConfigService.AutoSurroundWithSequenceEnabled = false;
            }
            var wdMeta = new System.Activities.Core.Presentation.DesignerMetadata();
            wdMeta.Register();
            DesignerView = _wd.View;
            if (resource.Xaml == null)
            {
                IsNewWorkflow = true;
                var helper = new WorkflowHelper();
                _wd.Load(helper.CreateWorkflow("Untitled 1"));
                Header = "Untitled 1";
            }
            else
            {
                IsNewWorkflow = false;
                _wd.Text = resource.Xaml.ToString();
            }
            _wd.Context.Services.Subscribe<DesignerView>(DesigenrViewSubscribe);
        }

        void DesigenrViewSubscribe(DesignerView instance)
        {
            // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.None;
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom | ShellBarItemVisibility.PanMode | ShellBarItemVisibility.MiniMap;
        }

        /// <summary>
        /// The resource that is being represented
        /// </summary>
        public IXamlResource Resource
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
