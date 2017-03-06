/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xaml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Collections;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.CustomControls.Utils;
using Dev2.Data.Interfaces;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Dialogs;
using Dev2.Factories;
using Dev2.Factory;
using Dev2.Interfaces;
using Dev2.Messages;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Enums;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.Utilities;
using Dev2.Utils;
using Dev2.ViewModels.Workflow;
using Dev2.Workspaces;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ViewModels;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedMember.Global
// ReSharper disable LoopCanBeConvertedToQuery


// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Workflow
// ReSharper restore CheckNamespace
{
    public class FromToolBox
    {
    }

    public class WorkflowDesignerViewModel : BaseWorkSurfaceViewModel,
                                             IHandle<AddStringListToDataListMessage>,
                                             IHandle<EditActivityMessage>,
                                             IHandle<SaveUnsavedWorkflowMessage>,
                                             IWorkflowDesignerViewModel
    {
        static readonly Type[] DecisionSwitchTypes = { typeof(FlowSwitch<string>), typeof(FlowDecision) };

        #region Overrides of Screen

        protected readonly IDesignerManagementService DesignerManagementService;
        readonly IWorkflowHelper _workflowHelper;
        DelegateCommand _collapseAllCommand;

        protected dynamic DataObject { get; set; }
        List<ModelItem> _selectedDebugItems = new List<ModelItem>();
        DelegateCommand _expandAllCommand;
        // ReSharper disable once InconsistentNaming
        private ModelService ModelService;
        IContextualResourceModel _resourceModel;
        // ReSharper disable once InconsistentNaming
        protected Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts;
        // ReSharper disable InconsistentNaming
        protected WorkflowDesigner _wd;
        DesignerMetadata _wdMeta;

        VirtualizedContainerService _virtualizedContainerService;
        MethodInfo _virtualizedContainerServicePopulateAllMethod;

        readonly StudioSubscriptionService<DebugSelectionChangedEventArgs> _debugSelectionChangedService = new StudioSubscriptionService<DebugSelectionChangedEventArgs>();

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resource"> Workflow that appears on design surface</param>
        /// <param name="createDesigner"></param>
        public WorkflowDesignerViewModel(IContextualResourceModel resource, bool createDesigner = true)
            : this(resource, new WorkflowHelper(), createDesigner)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resource">Resource that will be opened</param>
        /// <param name="workflowHelper">seriali</param>
        /// <param name="createDesigner">create a new designer flag</param>
        public WorkflowDesignerViewModel(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            : this(EventPublishers.Aggregator, resource, workflowHelper, createDesigner)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventPublisher">Event publisher that is not the singleton</param>
        /// <param name="resource">Resource that will be opened</param>
        /// <param name="workflowHelper">Serialization helper</param>
        /// <param name="createDesigner">create a new designer flag</param>
        // ReSharper disable TooManyDependencies
        private WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            // ReSharper restore TooManyDependencies
            : this(eventPublisher, resource, workflowHelper,
                CustomContainer.Get<IPopupController>(), new AsyncWorker(), new ExternalProcessExecutor(), createDesigner)
        {
        }

        /// <summary>
        /// Unit Testing Constructor
        /// </summary>
        /// <param name="eventPublisher"> Non singleton event publisher</param>
        /// <param name="resource">Resource that will be opened</param>
        /// <param name="workflowHelper">Serialisation Helper</param>
        /// <param name="popupController">Injected popup controller</param>
        /// <param name="asyncWorker"></param>
        /// <param name="executor">Execute external Processes</param>
        /// <param name="createDesigner">Create a new designer flag</param>
        /// <param name="liteInit"> Lite initialise designer. Testing only</param>
        // ReSharper disable TooManyDependencies
        public WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, IPopupController popupController, IAsyncWorker asyncWorker, IExternalProcessExecutor executor, bool createDesigner = true, bool liteInit = false)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("workflowHelper", workflowHelper);
            VerifyArgument.IsNotNull("popupController", popupController);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _executor = executor;
            _workflowHelper = workflowHelper;
            _resourceModel = resource;
            _resourceModel.OnDataListChanged += FireWdChanged;
            _resourceModel.OnResourceSaved += UpdateOriginalDataList;
            _asyncWorker = asyncWorker;

            PopUp = popupController;

            if (_resourceModel.DataList != null)
            {
                SetOriginalDataList(_resourceModel);
            }
            DesignerManagementService = new DesignerManagementService(resource, _resourceModel.Environment.ResourceRepository);
            if (createDesigner)
            {
                ActivityDesignerHelper.AddDesignerAttributes(this, liteInit);
            }
            _workflowInputDataViewModel = WorkflowInputDataViewModel.Create(_resourceModel);
            GetWorkflowLink();
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(_resourceModel);
            DebugOutputViewModel = new DebugOutputViewModel(_resourceModel.Environment.Connection.ServerEvents, EnvironmentRepository.Instance, new DebugOutputFilterStrategy(), ResourceModel);
            _firstWorkflowChange = true;
        }

        public void SetPermission(Permissions permission)
        {
            SetNonePermissions();

            if (permission.HasFlag(Permissions.View))
            {
                SetViewPermissions();
            }
            if (permission.HasFlag(Permissions.Execute))
            {
                SetExecutePermissions();
            }
            if (permission.HasFlag(Permissions.Contribute))
            {
                SetContributePermissions();
            }
            if (permission.HasFlag(Permissions.Administrator))
            {
                SetAdministratorPermissions();
            }
        }

        private void SetExecutePermissions()
        {
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanRunAllTests = !ResourceModel.IsNewWorkflow;
            CanViewSwagger = !ResourceModel.IsNewWorkflow;
            CanCopyUrl = !ResourceModel.IsNewWorkflow;
        }

        private void SetViewPermissions()
        {
            CanViewSwagger = true;
            CanCopyUrl = true;
        }

        private void SetNonePermissions()
        {
            CanDebugInputs = false;
            CanDebugStudio = false;
            CanDebugBrowser = false;
            CanCreateSchedule = false;
            CanCreateTest = false;
            CanRunAllTests = false;
            CanDuplicate = false;
            CanDeploy = false;
            CanShowDependencies = false;
            CanViewSwagger = false;
            CanCopyUrl = false;
        }

        private void SetAdministratorPermissions()
        {
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanCreateSchedule = !ResourceModel.IsNewWorkflow;
            CanCreateTest = !ResourceModel.IsNewWorkflow;
            CanRunAllTests = !ResourceModel.IsNewWorkflow;
            CanDuplicate = !ResourceModel.IsNewWorkflow;
            CanDeploy = !ResourceModel.IsNewWorkflow;
            CanShowDependencies = !ResourceModel.IsNewWorkflow;
            CanViewSwagger = !ResourceModel.IsNewWorkflow;
            CanCopyUrl = !ResourceModel.IsNewWorkflow;
        }

        private void SetContributePermissions()
        {
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanCreateSchedule = !ResourceModel.IsNewWorkflow;
            CanCreateTest = !ResourceModel.IsNewWorkflow;
            CanRunAllTests = !ResourceModel.IsNewWorkflow;
            CanDuplicate = !ResourceModel.IsNewWorkflow;
            CanDeploy = !ResourceModel.IsNewWorkflow;
            CanShowDependencies = !ResourceModel.IsNewWorkflow;
            CanViewSwagger = !ResourceModel.IsNewWorkflow;
            CanCopyUrl = !ResourceModel.IsNewWorkflow;
        }

        public bool CanCopyUrl
        {
            get { return _canCopyUrl; }
            set
            {
                _canCopyUrl = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    CopyUrlTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    CopyUrlTooltip = _canCopyUrl ? Warewolf.Studio.Resources.Languages.Tooltips.CopyUrlToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanCopyUrl");
            }
        }

        public string CopyUrlTooltip
        {
            get { return _copyUrlTooltip; }
            set
            {
                _copyUrlTooltip = value;
                OnPropertyChanged("CopyUrlTooltip");
            }
        }

        public bool CanViewSwagger
        {
            get { return _canViewSwagger; }
            set
            {
                _canViewSwagger = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    ViewSwaggerTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    ViewSwaggerTooltip = _canViewSwagger ? Warewolf.Studio.Resources.Languages.Tooltips.ViewSwaggerToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanViewSwagger");
            }
        }

        public string ViewSwaggerTooltip
        {
            get { return _viewSwaggerTooltip; }
            set
            {
                _viewSwaggerTooltip = value;
                OnPropertyChanged("ViewSwaggerTooltip");
            }
        }

        public bool CanShowDependencies
        {
            get { return _canShowDependencies; }
            set
            {
                _canShowDependencies = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    ShowDependenciesTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    ShowDependenciesTooltip = _canShowDependencies ? Warewolf.Studio.Resources.Languages.Tooltips.DependenciesToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanShowDependencies");
            }
        }

        public string ShowDependenciesTooltip
        {
            get { return _showDependenciesTooltip; }
            set
            {
                _showDependenciesTooltip = value;
                OnPropertyChanged("ShowDependenciesTooltip");
            }
        }

        public bool CanDeploy
        {
            get { return _canDeploy; }
            set
            {
                _canDeploy = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    DeployTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    DeployTooltip = _canDeploy ? Warewolf.Studio.Resources.Languages.Tooltips.DeployToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanDeploy");
            }
        }

        public string DeployTooltip
        {
            get { return _deployTooltip; }
            set
            {
                _deployTooltip = value;
                OnPropertyChanged("DeployTooltip");
            }
        }

        public bool CanDuplicate
        {
            get { return _canDuplicate; }
            set
            {
                _canDuplicate = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    DuplicateTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    DuplicateTooltip = _canDuplicate ? Warewolf.Studio.Resources.Languages.Tooltips.DuplicateToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanDuplicate");
            }
        }

        public string DuplicateTooltip
        {
            get { return _duplicateTooltip; }
            set
            {
                _duplicateTooltip = value;
                OnPropertyChanged("DuplicateTooltip");
            }
        }

        public bool CanRunAllTests
        {
            get { return _canRunAllTests; }
            set
            {
                _canRunAllTests = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    RunAllTestsTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    RunAllTestsTooltip = _canRunAllTests ? Warewolf.Studio.Resources.Languages.Tooltips.RunAllTestsToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanRunAllTests");
            }
        }

        public string RunAllTestsTooltip
        {
            get { return _runAllTestsTooltip; }
            set
            {
                _runAllTestsTooltip = value;
                OnPropertyChanged("RunAllTestsTooltip");
            }
        }

        public bool CanCreateTest
        {
            get { return _canCreateTest; }
            set
            {
                _canCreateTest = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    CreateTestTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    CreateTestTooltip = _canCreateTest ? Warewolf.Studio.Resources.Languages.Tooltips.TestEditorToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanCreateTest");
            }
        }

        public string CreateTestTooltip
        {
            get { return _createTestTooltip; }
            set
            {
                _createTestTooltip = value;
                OnPropertyChanged("CreateTestTooltip");
            }
        }

        public bool CanCreateSchedule
        {
            get { return _canCreateSchedule; }
            set
            {
                _canCreateSchedule = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    ScheduleTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    ScheduleTooltip = _canCreateSchedule ? Warewolf.Studio.Resources.Languages.Tooltips.ScheduleToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanCreateSchedule");
            }
        }

        public string ScheduleTooltip
        {
            get { return _scheduleTooltip; }
            set
            {
                _scheduleTooltip = value;
                OnPropertyChanged("ScheduleTooltip");
            }
        }

        public bool CanDebugBrowser
        {
            get { return _debugBrowser; }
            set
            {
                _debugBrowser = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    DebugBrowserTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    DebugBrowserTooltip = _debugBrowser ? Warewolf.Studio.Resources.Languages.Tooltips.StartNodeDebugBrowserToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanDebugBrowser");
            }
        }

        public string DebugBrowserTooltip
        {
            get { return _debugBrowserTooltip; }
            set
            {
                _debugBrowserTooltip = value;
                OnPropertyChanged("DebugBrowserTooltip");
            }
        }

        public bool CanDebugStudio
        {
            get { return _canDebugStudio; }
            set
            {
                _canDebugStudio = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    DebugStudioTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    DebugStudioTooltip = _canDebugStudio ? Warewolf.Studio.Resources.Languages.Tooltips.StartNodeDebugStudioToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanDebugStudio");
            }
        }

        public string DebugStudioTooltip
        {
            get { return _debugStudioTooltip; }
            set
            {
                _debugStudioTooltip = value;
                OnPropertyChanged("DebugStudioTooltip");
            }
        }

        public bool CanDebugInputs
        {
            get { return _canDebugInputs; }
            set
            {
                _canDebugInputs = value;
                if (ResourceModel.IsNewWorkflow)
                {
                    DebugInputsTooltip = Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip;
                }
                else
                {
                    DebugInputsTooltip = _canDebugInputs ? Warewolf.Studio.Resources.Languages.Tooltips.StartNodeDebugInputsToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
                OnPropertyChanged("CanDebugInputs");
            }
        }

        public string DebugInputsTooltip
        {
            get { return _debugInputsTooltip; }
            set
            {
                _debugInputsTooltip = value;
                OnPropertyChanged("DebugInputsTooltip");
            }
        }

        private void SetOriginalDataList(IContextualResourceModel contextualResourceModel)
        {
            if (!string.IsNullOrEmpty(contextualResourceModel.DataList))
            {
                // ReSharper disable MaximumChainedReferences
                _originalDataList = contextualResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "").Replace(Environment.NewLine, "").Trim();
                // ReSharper restore MaximumChainedReferences
            }
        }

        private void UpdateOriginalDataList(IContextualResourceModel obj)
        {
            if (obj.IsWorkflowSaved)
            {
                SetOriginalDataList(obj);
            }
        }

        #endregion

        public DebugOutputViewModel DebugOutputViewModel
        {
            get
            {
                return _debugOutputViewModel;
            }
            set { _debugOutputViewModel = value; }
        }

        #region Overrides of ViewAware

        #endregion

        public IDataListViewModel DataListViewModel
        {
            get
            {
                return _dataListViewModel;
            }
            set
            {
                if (_dataListViewModel == value)
                {
                    return;
                }

                _dataListViewModel = value;
                NotifyOfPropertyChange(() => DataListViewModel);
            }
        }

        #region Properties

        public override bool HasVariables => true;
        public override bool HasDebugOutput => true;

        public override bool CanSave => ResourceModel.IsAuthorized(AuthorizationContext.Contribute);

        protected virtual bool IsDesignerViewVisible => DesignerView != null && DesignerView.IsVisible;

#pragma warning disable 108,114
        public string DisplayName
#pragma warning restore 108,114
        {
            get
            {
                var displayName = ResourceModel.UserPermissions == Permissions.View ?
                    $"{ResourceHelper.GetDisplayName(ResourceModel)} [READONLY]"
                    : ResourceHelper.GetDisplayName(ResourceModel);
                return displayName;
            }
        }

        public string GetWorkflowLink(bool addWorkflowId = true)
        {
            if (_workflowInputDataViewModel != null)
            {
                if (!string.IsNullOrEmpty(_resourceModel.DataList))
                {

                    _workflowInputDataViewModel.DebugTo.DataList = _resourceModel.DataList;
                }
                _workflowLink = "";
                _workflowInputDataViewModel.LoadWorkflowInputs();
                _workflowInputDataViewModel.SetXmlData(true);
                var buildWebPayLoad = _workflowInputDataViewModel.BuildWebPayLoad();
                var workflowUri = WebServer.GetWorkflowUri(_resourceModel, buildWebPayLoad, UrlType.Json, addWorkflowId);
                if (workflowUri != null)
                {
                    _workflowLink = workflowUri.ToString();
                }
            }
            NotifyOfPropertyChange(() => DisplayWorkflowLink);
            return _workflowLink;
        }

        public string DisplayWorkflowLink
        {
            get
            {
                var workflowLink = _workflowLink;
                if (!string.IsNullOrEmpty(workflowLink))
                {
                    var startIndex = workflowLink.IndexOf("&wid", StringComparison.InvariantCultureIgnoreCase);
                    if (startIndex != -1)
                    {
                        return workflowLink.Remove(startIndex);
                    }
                }
                return workflowLink;
            }
            private set
            {
                _workflowLink = value;
            }
        }

        public Visibility WorkflowLinkVisible => _resourceModel.IsVersionResource ? Visibility.Hidden : Visibility.Visible;

        public IPopupController PopUp { get; set; }

        [ExcludeFromCodeCoverage]
        public virtual object SelectedModelItem => _wd?.Context?.Items.GetValue<Selection>().SelectedObjects.FirstOrDefault();

        public IContextualResourceModel ResourceModel { get { return _resourceModel; } set { _resourceModel = value; } }

        public string WorkflowName => _resourceModel.ResourceName;

        public WorkflowDesigner Designer => _wd;

        public UIElement DesignerView => _wd?.View;

        public void UpdateWorkflowLink(string newLink)
        {
            DisplayWorkflowLink = newLink;
            NotifyOfPropertyChange(nameof(DisplayWorkflowLink));
        }

        public StringBuilder DesignerText => ServiceDefinition;

        public StringBuilder ServiceDefinition { get { return _workflowHelper.SerializeWorkflow(ModelService); } set { } }

        #endregion

        #region Commands

        public ICommand CollapseAllCommand
        {
            get
            {
                return _collapseAllCommand ?? (_collapseAllCommand = new DelegateCommand(param =>
                {
                    bool val = Convert.ToBoolean(param);
                    if (val)
                    {
                        DesignerManagementService.RequestCollapseAll();
                    }
                    else
                    {
                        DesignerManagementService.RequestRestoreAll();
                    }
                }));
            }
        }

        public ICommand ExpandAllCommand
        {
            get
            {
                return _expandAllCommand ?? (_expandAllCommand = new DelegateCommand(param =>
                {
                    bool val = Convert.ToBoolean(param);
                    if (val)
                    {
                        DesignerManagementService.RequestExpandAll();
                    }
                    else
                    {
                        DesignerManagementService.RequestRestoreAll();
                    }
                }));
            }
        }

        public ICommand OpenWorkflowLinkCommand
        {
            get
            {
                return _openWorkflowLinkCommand ?? (_openWorkflowLinkCommand = new DelegateCommand(param =>
                {
                    if (!string.IsNullOrEmpty(_workflowLink))
                    {
                        SaveToWorkspace();
                        if (_workflowInputDataViewModel.WorkflowInputCount == 0)
                        {
                            PopUp.ShowNoInputsSelectedWhenClickLink();

                        }
                        try
                        {
                            _executor.OpenInBrowser(new Uri(_workflowLink));
                        }
                        catch (Exception e)
                        {
                            Dev2Logger.Error("OpenWorkflowLinkCommand", e);
                        }

                    }
                }));
            }
        }

        public ICommand NewServiceCommand
        {
            get
            {
                return _newServiceCommand ?? (_newServiceCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.NewService("");
                        }
                    }
                }));
            }
        }

        public ICommand DebugInputsCommand
        {
            get
            {
                return _debugInputsCommand ?? (_debugInputsCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.DebugCommand.Execute(mvm.ActiveItem);
                        }
                    }
                }));
            }
        }

        public ICommand DebugStudioCommand
        {
            get
            {
                return _debugStudioCommand ?? (_debugStudioCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.QuickDebugCommand.Execute(mvm.ActiveItem);
                        }
                    }
                }));
            }
        }

        public ICommand DebugBrowserCommand
        {
            get
            {
                return _debugBrowserCommand ?? (_debugBrowserCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.QuickViewInBrowserCommand.Execute(mvm.ActiveItem);
                        }
                    }
                }));
            }
        }

        public ICommand ScheduleCommand
        {
            get
            {
                return _scheduleCommand ?? (_scheduleCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.CreateNewSchedule(mvm.ActiveItem.ContextualResourceModel.ID);
                        }
                    }
                }));
            }
        }

        public bool IsCommandEnabled => ResourceModel != null && !ResourceModel.IsNewWorkflow;

        public ICommand TestEditorCommand
        {
            get
            {
                return _testEditorCommand ?? (_testEditorCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.CreateTest(mvm.ActiveItem.ContextualResourceModel.ID);
                        }
                    }
                }));
            }
        }

        public ICommand RunAllTestsCommand
        {
            get
            {
                return _runAllTestsCommand ?? (_runAllTestsCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.RunAllTests(mvm.ActiveItem.ContextualResourceModel.ID);
                        }
                    }
                }));
            }
        }

        public ICommand DuplicateCommand
        {
            get
            {
                return _duplicateCommand ?? (_duplicateCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            IExplorerItemViewModel explorerItem = null;
                            var environmentViewModels = mvm.ExplorerViewModel.Environments.Where(a => a.ResourceId == mvm.ActiveEnvironment.ID);
                            foreach (var environmentViewModel in environmentViewModels)
                            {
                                explorerItem = environmentViewModel.Children.Flatten(model => model.Children).FirstOrDefault(c => c.ResourceId == mvm.ActiveItem.ContextualResourceModel.ID);
                            }

                            if (explorerItem != null)
                                mvm.DuplicateResource(explorerItem);
                        }
                    }
                }));
            }
        }

        public ICommand DeployCommand
        {
            get
            {
                return _deployCommand ?? (_deployCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            var explorerItem = GetSelected(mvm);
                            if (explorerItem != null)
                                mvm.AddDeploySurface(explorerItem.AsList().Union(new[] { explorerItem }));
                        }
                    }
                }));
            }
        }

        private static IExplorerItemViewModel GetSelected(MainViewModel mvm)
        {
            IExplorerItemViewModel explorerItem = null;
            var environmentViewModels = mvm.ExplorerViewModel.Environments.Where(a => a.ResourceId == mvm.ActiveEnvironment.ID);
            foreach (var environmentViewModel in environmentViewModels)
            {
                explorerItem =
                    environmentViewModel.Children.Flatten(model => model.Children)
                        .FirstOrDefault(c => c.ResourceId == mvm.ActiveItem.ContextualResourceModel.ID);
            }
            return explorerItem;
        }

        public ICommand ShowDependenciesCommand
        {
            get
            {
                return _showDependenciesCommand ?? (_showDependenciesCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            var explorerItem = GetSelected(mvm);
                            mvm.ShowDependencies(mvm.ActiveItem.ContextualResourceModel.ID, mvm.ActiveServer, explorerItem.IsSource || explorerItem.IsServer);
                        }
                    }
                }));
            }
        }

        public ICommand ViewSwaggerCommand
        {
            get
            {
                return _viewSwaggerCommand ?? (_viewSwaggerCommand = new DelegateCommand(param =>
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
                    {
                        var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                        if (mvm?.ActiveItem != null)
                        {
                            mvm.ViewSwagger(mvm.ActiveItem.ContextualResourceModel.ID, mvm.ActiveServer);
                        }
                    }
                }));
            }
        }

        public ICommand CopyUrlCommand
        {
            get
            {
                return _copyUrlCommand ?? (_copyUrlCommand = new DelegateCommand(param =>
                {
                    Clipboard.SetText(GetWorkflowLink(false));
                }));
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Fixes up items added. Assigns unique Id. Initialises as flow step
        /// </summary>
        /// <param name="addedItems"></param>
        /// <returns></returns>

        // ReSharper disable MethodTooLong
        // ReSharper disable ExcessiveIndentation
        protected List<ModelItem> PerformAddItems(List<ModelItem> addedItems)
        // ReSharper restore ExcessiveIndentation
        // ReSharper restore MethodTooLong
        {
            for (int i = 0; i < addedItems.Count; i++)
            {
                var mi = addedItems.ToList()[i];

                var computedValue = mi.Content?.ComputedValue;
                if (computedValue is IDev2Activity)
                {
                    (computedValue as IDev2Activity).UniqueID = Guid.NewGuid().ToString();
                }

                if (mi.ItemType == typeof(FlowSwitch<string>))
                {
                    InitializeFlowSwitch(mi);
                }
                else if (mi.ItemType == typeof(FlowDecision))
                {
                    InitializeFlowDecision(mi);
                }
                else if (mi.ItemType == typeof(FlowStep))
                {
                    InitializeFlowStep(mi);
                }
                else
                {
                    AddSwitch(mi);
                }
            }
            return addedItems;
        }

        void AddSwitch(ModelItem mi)
        {
            if (mi.Parent?.Parent?.Parent != null && mi.Parent.Parent.Parent.ItemType == typeof(FlowSwitch<string>))
            {
                ModelProperty activityExpression = mi.Parent.Parent.Parent.Properties["Expression"];
                if (activityExpression != null)
                {
                    var switchExpressionValue = SwitchExpressionValue(activityExpression);
                    ModelProperty modelProperty = mi.Properties["Key"];
                    if (modelProperty?.Value != null && (FlowController.OldSwitchValue == null || string.IsNullOrWhiteSpace(FlowController.OldSwitchValue)))
                    {
                        FlowController.ConfigureSwitchCaseExpression(new ConfigureCaseExpressionMessage { ModelItem = mi, ExpressionText = switchExpressionValue, EnvironmentModel = _resourceModel.Environment });
                    }
                }
            }
        }

        [ExcludeFromCodeCoverage]
        static string SwitchExpressionValue(ModelProperty activityExpression)
        {
            var tmpModelItem = activityExpression.Value;
            var switchExpressionValue = string.Empty;
            var tmpProperty = tmpModelItem?.Properties["ExpressionText"];
            var tmp = tmpProperty?.Value?.ToString();

            if (!string.IsNullOrEmpty(tmp))
            {
                int start = tmp.IndexOf("(", StringComparison.Ordinal);
                int end = tmp.IndexOf(",", StringComparison.Ordinal);

                if (start < end && start >= 0)
                {
                    start += 2;
                    end -= 1;
                    switchExpressionValue = tmp.Substring(start, (end - start));
                }
            }
            return switchExpressionValue;
        }
        private void InitializeFlowStep(ModelItem mi)
        {
            // PBI 9135 - 2013.07.15 - TWR - Changed to "as" check so that database activity also flows through this
            ModelProperty modelProperty1 = mi.Properties["Action"];
            InitialiseWithAction(modelProperty1);
        }

        private void InitialiseWithAction(ModelProperty modelProperty1)
        {
            var droppedActivity = modelProperty1?.ComputedValue as DsfActivity;
            if (droppedActivity != null)
            {
                if (!string.IsNullOrEmpty(droppedActivity.ServiceName))
                {
                    InitialiseWithoutServiceName(modelProperty1, droppedActivity);
                }
                else
                {
                    InitialiseWithDataObject(droppedActivity);
                }
            }
        }

        private void InitialiseWithDataObject(DsfActivity droppedActivity)
        {
            if (DataObject != null)
            {
                var viewModel = DataObject as ExplorerItemViewModel;

                if (viewModel != null)
                {
                    IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == viewModel.Server.EnvironmentID);
                    var theResource = environmentModel?.ResourceRepository.LoadContextualResourceModel(viewModel.ResourceId);

                    if (theResource != null)
                    {
                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(theResource, droppedActivity, true, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());

                        d.DisplayName = theResource.DisplayName;
                        d.ServiceName = theResource.Category;

                        UpdateForRemote(d, theResource);
                    }
                }
                DataObject = null;
            }
        }

        public ResourceType ResourceType
        {
            get
            {
                if (ResourceModel != null)
                {
                    return ResourceModel.ResourceType;
                }
                return ResourceType.Unknown;
            }
        }

        private void InitialiseWithoutServiceName(ModelProperty modelProperty1, DsfActivity droppedActivity)
        {
            DsfActivity activity = droppedActivity;
            IContextualResourceModel resource = _resourceModel.Environment.ResourceRepository.FindSingle(
                c => c.Category == activity.ServiceName) as IContextualResourceModel;
            IEnvironmentRepository environmentRepository = EnvironmentRepository.Instance;
            droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false, environmentRepository, _resourceModel.Environment.IsLocalHostCheck());
            WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(droppedActivity, resource, environmentRepository.ActiveEnvironment);
            modelProperty1.SetValue(droppedActivity);
        }

        private void InitializeFlowSwitch(ModelItem mi)
        {
            // Travis.Frisinger : 28.01.2013 - Switch Amendments
            Dev2Logger.Info("Publish message of type - " + typeof(ConfigureSwitchExpressionMessage));
            _expressionString = FlowController.ConfigureSwitchExpression(new ConfigureSwitchExpressionMessage { ModelItem = mi, EnvironmentModel = _resourceModel.Environment, IsNew = true });
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        private void InitializeFlowDecision(ModelItem mi)
        {
            Dev2Logger.Info("Publish message of type - " + typeof(ConfigureDecisionExpressionMessage));
            ModelProperty modelProperty = mi.Properties["Action"];

            InitialiseWithAction(modelProperty);
            _expressionString = FlowController.ConfigureDecisionExpression(new ConfigureDecisionExpressionMessage { ModelItem = mi, EnvironmentModel = _resourceModel.Environment, IsNew = true });
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        private void EditActivity(ModelItem modelItem, Guid parentEnvironmentID)
        {
            if (Designer == null)
            {
                return;
            }
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root && (modelItem.ItemType == typeof(DsfActivity) || modelItem.ItemType.BaseType == typeof(DsfActivity)))
            {
                var resourceID = ModelItemUtils.TryGetResourceID(modelItem);
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                shellViewModel.OpenResource(resourceID, parentEnvironmentID, shellViewModel.ActiveServer);
            }
        }

        private static ModelItem RecursiveForEachCheck(dynamic activity)
        {
            var innerAct = activity.DataFunc.Handler as ModelItem;
            if (innerAct != null)
            {
                if (innerAct.ItemType == typeof(DsfForEachActivity))
                {
                    innerAct = RecursiveForEachCheck(innerAct);
                }
            }
            return innerAct;
        }

        /// <summary>
        ///     Prevents the delete from being executed if it is a FlowChart.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        [ExcludeFromCodeCoverage]
        private void PreventCommandFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {
            if (Designer?.Context != null)
            {
                var selection = Designer.Context.Items.GetValue<Selection>();

                if (selection?.PrimarySelection == null)
                    return;

                if (selection.PrimarySelection.ItemType != typeof(Flowchart) &&
                   selection.SelectedObjects.All(modelItem => modelItem.ItemType != typeof(Flowchart)))
                    return;
            }

            e.CanExecute = false;
            e.Handled = true;
        }

        /// <summary>
        ///     Sets the last dropped point.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="DragEventArgs" /> instance containing the event data.
        /// </param>
        [ExcludeFromCodeCoverage]
        private void SetLastDroppedPoint(DragEventArgs e)
        {
            var senderAsFrameworkElement = ModelService.Root.View as FrameworkElement;
            UIElement freePormPanel = senderAsFrameworkElement?.FindNameAcrossNamescopes("flowchartPanel");
            if (freePormPanel != null)
            {
                e.GetPosition(freePormPanel);
            }
        }

        #region DataList Workflow Specific Methods

        private IList<IDataListVerifyPart> BuildWorkflowFields()
        {
            var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
            _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
            var modelService = Designer?.Context.Services.GetService<ModelService>();
            if (modelService != null)
            {
                var flowNodes = modelService.Find(modelService.Root, typeof(FlowNode));

                GetWorkflowFieldsFromFlowNodes(flowNodes);
            }
            var flattenedList = _uniqueWorkflowParts.Keys.ToList();
            return flattenedList;
        }

        protected void GetWorkflowFieldsFromFlowNodes(IEnumerable<ModelItem> flowNodes)
        {
            foreach (var flowNode in flowNodes)
            {
                var workflowFields = GetWorkflowFieldsFromModelItem(flowNode);
                foreach (var field in workflowFields)
                {
                    var isJsonObjectSource = field.StartsWith("@");
                    WorkflowDesignerDataPartUtils.BuildDataPart(field, _uniqueWorkflowParts, isJsonObjectSource);
                }
            }
        }

        private IEnumerable<string> GetWorkflowFieldsFromModelItem(ModelItem flowNode)
        {
            var workflowFields = new List<string>();

            var modelProperty = flowNode.Properties["Action"];
            if (modelProperty != null)
            {
                var activity = modelProperty.ComputedValue;
                workflowFields = GetActivityElements(activity);
            }
            else
            {
                string propertyName = string.Empty;
                switch (flowNode.ItemType.Name)
                {
                    case "FlowDecision":
                        propertyName = "Condition";
                        break;
                    case "FlowSwitch`1":
                        propertyName = "Expression";
                        break;
                }
                var property = flowNode.Properties[propertyName];
                if (property != null)
                {
                    if (!string.IsNullOrEmpty(_expressionString))
                    {
                        workflowFields = GetDecisionElements(_expressionString, DataListSingleton.ActiveDataList);
                        var activity = property.ComputedValue;
                        if (activity != null)
                        {
                            workflowFields.AddRange(GetDecisionElements(((dynamic)activity).ExpressionText, DataListSingleton.ActiveDataList));
                        }
                    }
                    else
                    {
                        var activity = property.ComputedValue;
                        if (activity != null)
                        {
                            workflowFields.AddRange(GetDecisionElements(((dynamic)activity).ExpressionText, DataListSingleton.ActiveDataList));
                        }
                    }
                }
                else
                {
                    return workflowFields;
                }
            }
            return workflowFields;
        }

        public List<String> GetDecisionElements(string expression, IDataListViewModel datalistModel)
        {
            var decisionFields = new List<string>();
            if (!string.IsNullOrEmpty(expression))
            {
                int startIndex = expression.IndexOf('"');
                startIndex = startIndex + 1;
                int endindex = expression.IndexOf('"', startIndex);
                string decisionValue = expression.Substring(startIndex, endindex - startIndex);

                try
                {
                    var dds = JsonConvert.DeserializeObject<Dev2DecisionStack>(decisionValue.Replace('!', '\"'));
                    foreach (var decision in dds.TheStack)
                    {
                        var getCols = new[] { decision.Col1, decision.Col2, decision.Col3 };
                        for (var i = 0; i < 3; i++)
                        {
                            var getCol = getCols[i];
                            if (datalistModel != null)
                            {
                                var parsed = GetParsedRegions(getCol, datalistModel);
                                if (!DataListUtil.IsValueRecordset(getCol) && parsed.Any(DataListUtil.IsValueRecordset))
                                {
                                    IList<IIntellisenseResult> parts = DataListFactory.CreateLanguageParser().ParseExpressionIntoParts(decisionValue, new List<IDev2DataLanguageIntellisensePart>());
                                    decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                                }
                                else
                                    decisionFields = decisionFields.Union(GetParsedRegions(getCol, datalistModel)).ToList();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    if (!DataListUtil.IsValueRecordset(decisionValue))
                    {
                        IList<IIntellisenseResult> parts = DataListFactory.CreateLanguageParser().ParseExpressionIntoParts(decisionValue, new List<IDev2DataLanguageIntellisensePart>());
                        decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                    }
                    else
                    {
                        if (DataListSingleton.ActiveDataList != null)
                        {
                            IList<IIntellisenseResult> parts = DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(decisionValue, DataListSingleton.ActiveDataList.WriteToResourceModel(), true);
                            decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                        }
                    }

                }
            }
            return decisionFields;
        }

        private static IEnumerable<string> GetParsedRegions(string getCol, IDataListViewModel datalistModel)
        {
            // Travis.Frisinger - 25.01.2013 
            // We now need to parse this data for regions ;)

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            // NEED - DataList for active workflow
            var parts = parser.ParseDataLanguageForIntellisense(getCol, datalistModel.WriteToResourceModel(), true);

            return (from intellisenseResult in parts
                    select DataListUtil.StripBracketsFromValue(intellisenseResult.Option.DisplayValue)
                    into varWithNoBrackets
                    where !string.IsNullOrEmpty(getCol) && !varWithNoBrackets.Equals(getCol)
                    select getCol
                   ).ToList();
        }

        private static List<String> GetActivityElements(object activity)
        {
            var assign = activity as DsfActivityAbstract<string>;
            var other = activity as DsfActivityAbstract<bool>;
            enFindMissingType findMissingType;

            if (assign != null)
            {
                findMissingType = assign.GetFindMissingType();
            }
            else if (other != null)
            {
                findMissingType = other.GetFindMissingType();
            }
            else
            {
                return new List<String>();
            }

            var activityFields = new List<string>();
            var stratFac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = stratFac.CreateFindMissingStrategy(findMissingType);

            foreach (var activityField in strategy.GetActivityFields(activity))
            {
                if (!string.IsNullOrEmpty(activityField))
                {
                    WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();
                    activityFields.AddRange(wdu.FormatDsfActivityField(activityField).Where(item => !item.Contains("xpath(")));
                }
            }
            return activityFields;
        }

        #endregion

        #endregion

        #region Internal Methods

        private void OnItemSelected(Selection item)
        {
            var primarySelection = item.PrimarySelection;
            NotifyItemSelected(primarySelection);
            primarySelection.SetProperty("IsSelected", true);
            SelectedItem = primarySelection;
        }

        public Action<ModelItem> ItemSelectedAction { get; set; }

        #endregion Internal Methods

        #region Public Methods

        /// <summary>
        ///     Handels the list of strings to be added to the data list without a pop up message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/02/06</date>
        public void Handle(AddStringListToDataListMessage message)
        {
            Dev2Logger.Info(message.GetType().Name);
            IDataListViewModel dlvm = DataListSingleton.ActiveDataList;
            if (dlvm != null)
            {
                var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
                _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
                foreach (var s in message.ListToAdd)
                {
                    WorkflowDesignerDataPartUtils.BuildDataPart(s, _uniqueWorkflowParts);
                }
                IList<IDataListVerifyPart> partsToAdd = _uniqueWorkflowParts.Keys.ToList();
                List<IDataListVerifyPart> uniqueDataListPartsToAdd = dlvm.MissingDataListParts(partsToAdd);
                dlvm.AddMissingDataListItems(uniqueDataListPartsToAdd);
            }
        }

        /// <summary>
        /// Notifies the item selected.
        /// </summary>
        /// <param name="primarySelection">The primary selection.</param>
        /// <returns></returns>
        public bool NotifyItemSelected(object primarySelection)
        {
            var selectedItem = primarySelection as ModelItem;

            if (selectedItem != null)
            {
                if (selectedItem.ItemType == typeof(DsfForEachActivity))
                {
                    dynamic test = selectedItem;
                    ModelItem innerActivity = RecursiveForEachCheck(test);
                    if (innerActivity != null)
                    {
                        //Commenting this out to allow for the Foreach tool to expand to large view.
                        //Do not take out until we have finalized that this is not to be used
                        //selectedItem = innerActivity;
                    }
                }
                //Selection.Union(_wd.Context, selectedItem);
            }
            return false;
        }

        /// <summary>
        /// Saves the new XAML ;)
        /// </summary>
        public void BindToModel()
        {
            _resourceModel.WorkflowXaml = ServiceDefinition;
        }

        /// <summary>
        /// Initializes the designer.
        /// </summary>
        /// <param name="designerAttributes">The designer attributes.</param>
        /// <param name="liteInit">if set to <c>true</c> [lite initialize]. THIS IS FOR TESTING!!!!</param>
        public void InitializeDesigner(IDictionary<Type, Type> designerAttributes, bool liteInit = false)
        {
            _wd = new WorkflowDesigner();

            if (!liteInit)
            {
                SetHashTable();
                SetDesignerConfigService();

                _wdMeta = new DesignerMetadata();
                _wdMeta.Register();
                var builder = new AttributeTableBuilder();
                foreach (var designerAttribute in designerAttributes)
                {
                    builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
                }

                MetadataStore.AddAttributeTable(builder.CreateTable());

                _wd.Context.Items.Subscribe<Selection>(OnItemSelected);
                _wd.Context.Services.Subscribe<ModelService>(ModelServiceSubscribe);
                _wd.Context.Services.Subscribe<DesignerView>(DesigenrViewSubscribe);
                _wd.Context.Services.Publish(DesignerManagementService);

                _wd.View.Measure(new Size(2000, 2000));
                _wd.View.PreviewDrop += ViewPreviewDrop;
                _wd.View.PreviewMouseDown += ViewPreviewMouseDown;
                //_wd..View.MouseEnter += ViewPreviewMouseWheel;
                _wd.View.PreviewKeyDown += ViewOnKeyDown;
                _wd.View.LostFocus += OnViewOnLostFocus;

                //Jurie.Smit 2013/01/03 - Added to disable the deleting of the root flowchart
                CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
                _wd.ModelChanged += WdOnModelChanged;
                _wd.View.Focus();

                int indexOfOpenItem = -1;
                if (_wd.ContextMenu?.Items != null)
                {
                    foreach (var menuItem in _wd.ContextMenu.Items.Cast<object>().OfType<MenuItem>().Where(menuItem => (string)menuItem.Header == "_Open"))
                    {
                        indexOfOpenItem = _wd.ContextMenu.Items.IndexOf(menuItem);
                        break;
                    }
                    if (indexOfOpenItem != -1)
                    {
                        _wd.ContextMenu.Items.RemoveAt(indexOfOpenItem);
                    }
                }

                CommandManager.AddPreviewExecutedHandler(_wd.View, PreviewExecutedRoutedEventHandler);

                Selection.Subscribe(_wd.Context, SelectedItemChanged);

                LoadDesignerXaml();
                _workflowHelper.EnsureImplementation(ModelService);

                //For Changing the icon of the flowchart.
                WorkflowDesignerIcons.Activities.Flowchart = Application.Current.TryFindResource("Explorer-WorkflowService-Icon") as DrawingBrush;
                WorkflowDesignerIcons.Activities.StartNode = Application.Current.TryFindResource("System-StartNode-Icon") as DrawingBrush;
                SubscribeToDebugSelectionChanged();
                SetPermission(ResourceModel.UserPermissions);
                ViewModelUtils.RaiseCanExecuteChanged(_debugOutputViewModel?.AddNewTestCommand);
                UpdateErrorIconWithCorrectMessage();
            }
        }


        private void SetHashTable()
        {
            _wd.PropertyInspectorFontAndColorData = XamlServices.Save(ActivityDesignerHelper.GetDesignerHashTable());
        }

        private void SetDesignerConfigService()
        {
            var designerConfigService = _wd.Context.Services.GetService<DesignerConfigurationService>();
            if (designerConfigService != null)
            {
                // set the runtime Framework version to 4.5 as new features are in .NET 4.5 and do not exist in .NET 4
                designerConfigService.TargetFrameworkName = new FrameworkName(".NETFramework", new Version(4, 5));
                designerConfigService.AutoConnectEnabled = true;
                designerConfigService.AutoSplitEnabled = true;
                designerConfigService.PanModeEnabled = true;
                designerConfigService.RubberBandSelectionEnabled = true;
                designerConfigService.BackgroundValidationEnabled = true;

                // prevent design-time background validation from blocking UI thread
                // Disabled for now
                designerConfigService.AnnotationEnabled = false;
                designerConfigService.AutoSurroundWithSequenceEnabled = false;
            }
        }

        [ExcludeFromCodeCoverage] //This method is used to prevent the drill down on the designer
        private static void ViewOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource != null)
            {
                var origSource = e.OriginalSource.GetType();
                if (origSource.BaseType == typeof(ActivityDesigner))
                {
                    if (e.Key == Key.Return)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        static void DesigenrViewSubscribe(DesignerView instance)
        {
            // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
            instance.WorkflowShellHeaderItemsVisibility = ShellHeaderItemsVisibility.All;
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.None;
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom | ShellBarItemVisibility.PanMode | ShellBarItemVisibility.MiniMap;
        }

        [ExcludeFromCodeCoverage]
        private void OnViewOnLostFocus(object sender, RoutedEventArgs args)
        {
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);

            // If we are opening from server skip this check, it cannot have "real" changes!
            if (!OpeningWorkflowsHelper.IsWorkflowWaitingforDesignerLoad(workSurfaceKey))
            {
                // an additional case we need to account for - Designer has resized and is only visible once focus is lost?! ;)
                if (OpeningWorkflowsHelper.IsWaitingForFistFocusLoss(workSurfaceKey) || WatermarkSential.IsWatermarkBeingApplied)
                {
                    ResourceModel.WorkflowXaml = ServiceDefinition;
                    OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
                }
            }
        }

        private void ModelServiceSubscribe(ModelService instance)
        {
            ModelService = instance;
            ModelService.ModelChanged += ModelServiceModelChanged;
        }

        private void SubscribeToDebugSelectionChanged()
        {
            _virtualizedContainerService = _wd.Context.Services.GetService<VirtualizedContainerService>();
            if (_virtualizedContainerService != null)
            {
                _virtualizedContainerServicePopulateAllMethod = _virtualizedContainerService.GetType().GetMethod("BeginPopulateAll", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            _debugSelectionChangedService.Unsubscribe();
            _debugSelectionChangedService.Subscribe(args =>
            {
                // we only care when the designer is visible
                if (!IsDesignerViewVisible)
                {
                    return;
                }

                if (args.SelectionType == ActivitySelectionType.None)
                {
                    ClearSelection();
                    return;
                }

                var debugState = args.DebugState;
                if (debugState != null)
                {
                    var workSurfaceMappingId = debugState.WorkSurfaceMappingId;
                    var selectedModelItem = GetSelectedModelItem(workSurfaceMappingId, debugState.ParentID);
                    if (selectedModelItem != null)
                    {
                        switch (args.SelectionType)
                        {
                            case ActivitySelectionType.Single:
                                ClearSelection();
                                SelectSingleModelItem(selectedModelItem);

                                BringIntoView(selectedModelItem);
                                break;
                            case ActivitySelectionType.Add:
                                AddModelItemToSelection(selectedModelItem);

                                BringIntoView(selectedModelItem);
                                break;
                            case ActivitySelectionType.Remove:
                                RemoveModelItemFromSelection(selectedModelItem);
                                break;
                        }
                    }
                }
            });
        }

        public bool IsTestView { get; set; }

        protected virtual ModelItem GetSelectedModelItem(Guid itemId, Guid parentId)
        {
            if (ModelService != null)
            {
                var modelItems = ModelService.Find(ModelService.Root, typeof(IDev2Activity));
                // ReSharper disable MaximumChainedReferences
                var selectedModelItem = (from mi in modelItems
                                         let instanceID = ModelItemUtils.GetUniqueID(mi)
                                         where instanceID == itemId || instanceID == parentId
                                         select mi).FirstOrDefault();
                // ReSharper restore MaximumChainedReferences

                if (selectedModelItem == null)
                {
                    // Find the root flow chart
                    selectedModelItem = ModelService.Find(ModelService.Root, typeof(Flowchart)).FirstOrDefault();
                }
                else
                {
                    if (DecisionSwitchTypes.Contains(selectedModelItem.Parent.ItemType))
                    {
                        // Decision/switches activities are represented by their parents in the designer!
                        selectedModelItem = selectedModelItem.Parent;
                    }
                }
                return selectedModelItem;
            }
            return null;
        }


        private void SelectSingleModelItem(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                return;
            }
            Selection.SelectOnly(_wd.Context, selectedModelItem);
            SelectedDebugItems.Add(selectedModelItem);
        }

        private void RemoveModelItemFromSelection(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                SelectedDebugItems.Remove(selectedModelItem);
            }
            Selection.Unsubscribe(_wd.Context, SelectedItemChanged);
        }
        public List<ModelItem> DebugModels => SelectedDebugItems;
        private void AddModelItemToSelection(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                return;
            }
            Selection.Union(_wd.Context, selectedModelItem);

            ModelService modelService = _wd.Context.Services.GetService<ModelService>();
            IEnumerable<ModelItem> activityCollection = modelService.Find(modelService.Root, typeof(Activity));

            var modelItems = activityCollection as ModelItem[] ?? activityCollection.ToArray();
            var index = modelItems.ToList().IndexOf(selectedModelItem);
            if (index != -1)
            {
                Selection.Select(_wd.Context, modelItems.ElementAt(index));
            }
            else
            {
                if (DecisionSwitchTypes.Contains(selectedModelItem.Parent.ItemType))
                {
                    // Decision/switches activities are represented by their parents in the designer!
                    selectedModelItem = selectedModelItem.Parent;
                    index = modelItems.ToList().IndexOf(selectedModelItem);
                    if (index != -1)
                    {
                        Selection.Select(_wd.Context, modelItems.ElementAt(index));
                    }
                }
            }
            SelectedDebugItems.Add(selectedModelItem);
        }

        private void ClearSelection()
        {
            _wd.Context.Items.SetValue(new Selection());
            if (_selectedDebugItems != null)
            {
                foreach (var selectedDebugItem in _selectedDebugItems)
                {
                    selectedDebugItem.SetProperty("IsSelected", false);
                }
            }
            _selectedDebugItems = new List<ModelItem>();
        }

        protected virtual void BringIntoView(ModelItem selectedModelItem)
        {
            var view = selectedModelItem.View as FrameworkElement;
            if (view != null && view.IsVisible)
            {
                BringIntoView(view);
                return;
            }

            var onAfterPopulateAll = new System.Action(() => BringIntoView(selectedModelItem.View as FrameworkElement));
            _virtualizedContainerServicePopulateAllMethod?.Invoke(_virtualizedContainerService, new object[] { onAfterPopulateAll });
        }

        private static void BringIntoView(FrameworkElement view)
        {
            view?.BringIntoView();
        }
        protected void LoadDesignerXaml()
        {

            var xaml = _resourceModel.WorkflowXaml;

            // if null, try fetching. It appears there is more than the two routes identified to populating xaml ;(
            if (xaml == null || xaml.Length == 0)
            {
                // we always want server at this point ;)
                var workspace = GlobalConstants.ServerWorkspaceID;

                // log the trace for fetch ;)
                Dev2Logger.Info($"Null Definition For {_resourceModel.ID} :: {_resourceModel.ResourceName}. Fetching...");

                // In the case of null of empty try fetching again ;)
                var msg = EnvironmentModel.ResourceRepository.FetchResourceDefinition(_resourceModel.Environment, workspace, _resourceModel.ID, false);
                if (msg != null)
                {
                    xaml = msg.Message;
                }
            }

            // if we still cannot find it, create a new one ;)
            if (xaml == null || xaml.Length == 0)
            {
                if (_resourceModel.ResourceType == ResourceType.WorkflowService)
                {
                    // log the trace for fetch ;)
                    Dev2Logger.Info($"Could not find {_resourceModel.ResourceName}. Creating a new workflow");
                    var activityBuilder = _workflowHelper.CreateWorkflow(_resourceModel.ResourceName);
                    _wd.Load(activityBuilder);
                    BindToModel();
                }
                else
                {
                    // we have big issues ;(
                    throw new Exception($"Could not find resource definition for {_resourceModel.ResourceName}");
                }
            }
            else
            {
                SetDesignerText(xaml);
                _wd.Load();
            }
        }

        private void SetDesignerText(StringBuilder xaml)
        {
            var designerText = _workflowHelper.SanitizeXaml(xaml);
            if (designerText != null)
                _wd.Text = designerText.ToString();
        }

        private void SelectedItemChanged(Selection item)
        {
            if (_wd?.Context != null)
            {
                ContextItemManager contextItemManager = _wd.Context.Items;
                var selection = contextItemManager.GetValue<Selection>();
                if (selection.SelectedObjects.Count() > 1)
                {
                    DeselectFlowchart();
                }
            }
        }

        private void DeselectFlowchart()
        {
            if (_wd?.Context != null)
            {
                EditingContext editingContext = _wd.Context;
                var selection = editingContext.Items.GetValue<Selection>();
                foreach (var item in selection.SelectedObjects.Where(item => item.ItemType == typeof(Flowchart)))
                {
                    Selection.Toggle(editingContext, item);
                    break;
                }
            }
        }

        protected void WdOnModelChanged(object sender, EventArgs eventArgs)
        {
            if ((Designer != null && Designer.View.IsKeyboardFocusWithin) || sender != null)
            {
                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);
                UpdateErrorIconWithCorrectMessage();

                // If we are opening from server skip this check, it cannot have "real" changes!
                if (!OpeningWorkflowsHelper.IsWorkflowWaitingforDesignerLoad(workSurfaceKey))
                {
                    // an additional case we need to account for - Designer has resized and is only visible once focus is lost?! ;)
                    if (OpeningWorkflowsHelper.IsWaitingForFistFocusLoss(workSurfaceKey))
                    {
                        ResourceModel.WorkflowXaml = ServiceDefinition;
                        OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
                    }

                    var checkServiceDefinition = CheckServiceDefinition();
                    var checkDataList = CheckDataList();

                    ResourceModel.IsWorkflowSaved = checkServiceDefinition && checkDataList;
                    _workspaceSave = false;
                    WorkflowChanged?.Invoke();
                    NotifyOfPropertyChange(() => DisplayName);
                    ViewModelUtils.RaiseCanExecuteChanged(_debugOutputViewModel?.AddNewTestCommand);
                }
                else
                {
                    // When opening from server, save the hydrated changes for future comparison ;)
                    if (!CheckServiceDefinition())
                    {
                        // process any latent datalist changes ;)
                        ProcessDataListOnLoad();
                        ResourceModel.WorkflowXaml = ServiceDefinition;
                    }
                }

                // THIS MUST NEVER BE DELETED ;)
                WatermarkSential.IsWatermarkBeingApplied = false;
            }
            if (_firstWorkflowChange)
            {
                AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
                _firstWorkflowChange = false;
            }
        }

        private void UpdateErrorIconWithCorrectMessage()
        {
            var validationIcon = DesignerView.FindChild<Border>(border => border.Name.Equals("validationVisuals", StringComparison.CurrentCultureIgnoreCase));
            if (validationIcon != null && validationIcon.Name.Equals("validationVisuals", StringComparison.CurrentCultureIgnoreCase))
            {
                validationIcon.ToolTip = Warewolf.Studio.Resources.Languages.Tooltips.StartNodeNotConnectedToolTip;
            }
        }

        private bool CheckDataList()
        {
            if (_originalDataList == null)
                return true;
            if (ResourceModel.DataList != null)
            {
                string currentDataList = ResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "");
                return currentDataList.SpaceCaseInsenstiveComparision(_originalDataList);
            }
            return true;
        }

        private bool CheckServiceDefinition()
        {
            return ServiceDefinition.IsEqual(ResourceModel.WorkflowXaml);
        }

        /// <summary>
        /// Processes the data list configuration load.
        /// </summary>
        private void ProcessDataListOnLoad()
        {
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(true);
        }

        public void DoWorkspaceSave()
        {
            if (ResourceModel != null && ResourceModel.IsNewWorkflow && !_workspaceSave && ResourceModel.Environment.IsConnected)
            {
                _asyncWorker.Start(SaveToWorkspace);
            }
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        private void SaveToWorkspace()
        {
            BindToModel();
            ResourceModel.Environment.ResourceRepository.Save(ResourceModel);
            _workspaceSave = true;
        }

        /// <summary>
        /// Processes the data list configuration load.
        /// </summary>
        public void UpdateDataList()
        {
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }


        public static bool ValidatResourceModel(string dataList)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataList))
                    // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                    XElement.Parse(dataList);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
            }
            catch (Exception)
            {

                return false;
            }
            return true;

        }

        /// <summary>
        /// Adds the missing with no pop up and find unused data list items.
        /// </summary>
        public void AddMissingWithNoPopUpAndFindUnusedDataListItems()
        {
            //DoWorkspaceSave();
            UpdateDataList();
        }

        public ModelItem GetModelItem(Guid workSurfaceMappingId, Guid parentID)
        {
            var modelItems = ModelService.Find(ModelService.Root, typeof(IDev2Activity));
            ModelItem selectedModelItem = null;
            foreach (var mi in modelItems)
            {
                Guid instanceID = ModelItemUtils.GetUniqueID(mi);
                if (instanceID == workSurfaceMappingId || instanceID == parentID)
                {
                    selectedModelItem = mi;
                    break;
                }
            }
            return selectedModelItem;
        }

        /// <summary>
        /// Adds the missing with no pop up and find unused data list items implementation.
        /// </summary>
        /// <param name="isLoadEvent">if set to <c>true</c> [is load event].</param>
        private void AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(bool isLoadEvent)
        {
            if (DataListViewModel != null)
            {
                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        UpdateDataListWithMissingParts(isLoadEvent);
                    }), DispatcherPriority.Background);
                }
                else
                {
                    UpdateDataListWithMissingParts(isLoadEvent);
                }
            }
        }

        private void UpdateDataListWithMissingParts(bool isLoadEvent)
        {
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);
            if (OpeningWorkflowsHelper.IsWorkflowWaitingforDesignerLoad(workSurfaceKey) && !isLoadEvent)
            {
                OpeningWorkflowsHelper.RemoveWorkflowWaitingForDesignerLoad(workSurfaceKey);
            }

            IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
            DataListViewModel?.UpdateDataListItems(ResourceModel, workflowFields);
        }

        #endregion

        #region Event Handlers
        [ExcludeFromCodeCoverage]
        private void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = HandleMouseClick(e.LeftButton, e.ClickCount, e.OriginalSource as DependencyObject, e.Source as DesignerView);
        }

        /// <summary>
        /// Handles Mouse click events on Designer
        /// </summary>
        /// <param name="leftButtonState">State of left button</param>
        /// <param name="clickCount">Double,Single click</param>
        /// <param name="dp">Item Clicked</param>
        /// <param name="designerView">Designer view</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private bool HandleMouseClick(MouseButtonState leftButtonState, int clickCount, DependencyObject dp, DesignerView designerView)
        {
            if (HandleDoubleClick(leftButtonState, clickCount, dp, designerView))
            {
                return true;
            }

            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.RefreshActiveEnvironment();
                }
            }

            var dp1 = dp as Run;
            if (dp1?.Parent is TextBlock && dp1.DataContext.GetType().Name.Contains("FlowchartDesigner"))
            {
                var selectedModelItem = ModelService.Find(ModelService.Root, typeof(Flowchart)).FirstOrDefault();
                if (selectedModelItem != null)
                {
                    SelectSingleModelItem(selectedModelItem);
                }
                return true;
            }

            var dp2 = dp as TextBlock;
            if (dp2 != null && dp2.DataContext.GetType().Name.Contains("FlowchartDesigner"))
            {
                var selectedModelItem = ModelService.Find(ModelService.Root, typeof(Flowchart)).FirstOrDefault();
                if (selectedModelItem != null)
                {
                    SelectSingleModelItem(selectedModelItem);
                }
                return true;
            }

            return false;
        }



        [ExcludeFromCodeCoverage]
        private bool HandleDoubleClick(MouseButtonState leftButtonState, int clickCount, DependencyObject dp, DesignerView designerView)
        {
            if (leftButtonState == MouseButtonState.Pressed && clickCount == 2)
            {
                if (designerView != null && designerView.FocusedViewElement == null)
                {
                    return true;
                }

                var item = SelectedModelItem as ModelItem;

                if (item != null && item.ItemType == typeof(Flowchart))
                {
                    return true;
                }

                HandleDependencyObject(dp, item);
            }
            return false;
        }

        [ExcludeFromCodeCoverage]
        private void HandleDependencyObject(DependencyObject dp, ModelItem item)
        {
            if (item != null)
            {
                string itemFn = item.ItemType.FullName;

                if (dp != null && string.Equals(dp.ToString(), "Microsoft.Windows.Themes.ScrollChrome", StringComparison.InvariantCulture))
                {
                    WizardEngineAttachedProperties.SetDontOpenWizard(dp, true);
                }

                // Handle Case Edits
                if (itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchCaseLink", StringComparison.Ordinal) &&
                    !itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchDefaultLink", StringComparison.Ordinal))
                {
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp))
                    {
                        FlowController.EditSwitchCaseExpression(new EditCaseExpressionMessage
                        {
                            ModelItem = item,
                            EnvironmentModel = _resourceModel.Environment
                        });
                    }
                }

                // Handle Switch Edits
                if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                    item.ItemType == typeof(FlowSwitch<string>))
                {
                    _expressionString =
                        FlowController.ConfigureSwitchExpression(new ConfigureSwitchExpressionMessage
                        {
                            ModelItem = item,
                            EnvironmentModel = _resourceModel.Environment
                        });
                    AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
                }

                //// Handle Decision Edits
                if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                    item.ItemType == typeof(FlowDecision))
                {
                    _expressionString =
                        FlowController.ConfigureDecisionExpression(new ConfigureDecisionExpressionMessage
                        {
                            ModelItem = item,
                            EnvironmentModel = _resourceModel.Environment
                        });
                    AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
                }
            }
        }

        [ExcludeFromCodeCoverage]
        private IResourcePickerDialog CreateResourcePickerDialog(enDsfActivityType activityType)
        {
            var environment = EnvironmentRepository.Instance.ActiveEnvironment;
            IServer server = new Server(environment);

            if (server.Permissions == null)
            {
                server.Permissions = new List<IWindowsGroupPermission>();
                server.Permissions.AddRange(environment.AuthorizationService.SecurityService.Permissions);
            }
            var env = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);
            var res = new ResourcePickerDialog(activityType, env);
            ResourcePickerDialog.CreateAsync(activityType, env);
            return res;
        }

        /// <summary>
        /// Views the preview drop.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        [ExcludeFromCodeCoverage]
        private void ViewPreviewDrop(object sender, DragEventArgs e)
        {
            SetLastDroppedPoint(e);
            var dataObject = e.Data;
            e.Handled = ApplyForDrop(dataObject);
        }

        protected bool ApplyForDrop(IDataObject dataObject)
        {
            var handled = false;
            if (dataObject != null)
            {
                DataObject = dataObject.GetData(typeof(ExplorerItemViewModel));
                if (DataObject != null)
                {
                    IsItemDragged.Instance.IsDragged = true;
                }

                var isWorkflow = dataObject.GetData("WorkflowItemTypeNameFormat") as string;
                if (isWorkflow != null)
                {
                    handled = WorkflowDropFromResourceToolboxItem(dataObject, isWorkflow, true, false);
                    ApplyIsDraggedInstance(isWorkflow);
                }
                else
                {
                    IsItemDragged.Instance.IsDragged = false;
                }
            }
            return handled;
        }

        private static void ApplyIsDraggedInstance(string isWorkflow)
        {
            if (isWorkflow.Contains("DsfSqlServerDatabaseActivity") || isWorkflow.Contains("DsfMySqlDatabaseActivity")
                || isWorkflow.Contains("DsfODBCDatabaseActivity") || isWorkflow.Contains("DsfOracleDatabaseActivity")
                || isWorkflow.Contains("DsfPostgreSqlActivity") || isWorkflow.Contains("DsfWebDeleteActivity")
                || isWorkflow.Contains("DsfWebGetActivity") || isWorkflow.Contains("DsfWebPostActivity")
                || isWorkflow.Contains("DsfWebPutActivity") || isWorkflow.Contains("DsfComDllActivity")
                || isWorkflow.Contains("DsfEnhancedDotNetDllActivity") || isWorkflow.Contains("DsfWcfEndPointActivity"))
            {
                IsItemDragged.Instance.IsDragged = true;
            }
        }

        [ExcludeFromCodeCoverage]
        private bool WorkflowDropFromResourceToolboxItem(IDataObject dataObject, string isWorkflow, bool dropOccured, bool handled)
        {
            var activityType = ResourcePickerDialog.DetermineDropActivityType(isWorkflow);
            if (IsTestView)
            {
                return true;
            }
            if (activityType != enDsfActivityType.All)
            {
                var dialog = CreateResourcePickerDialog(activityType);
                if (dialog.ShowDialog())
                {
                    var res = dialog.SelectedResource;
                    if (res != null)
                    {
                        dataObject.SetData(res);
                        dataObject.SetData(new FromToolBox());
                        DataObject = res;
                    }
                    if (res == null)
                    {
                        dropOccured = false;
                        handled = true;
                    }
                }
                else
                {
                    handled = true;
                    dropOccured = false;
                }
            }
            if (dropOccured)
            {
                _workspaceSave = false;
                ResourceModel.IsWorkflowSaved = false;
                NotifyOfPropertyChange(() => DisplayName);
            }
            return handled;
        }

        // Activity : Next
        // Decision : True, False
        // Switch   : Default, Key
        public static readonly string[] SelfConnectProperties =
        {
            "Next",
            "True",
            "False",
            "Default",
            "Key"
        };

        private string _originalDataList;
        private bool _workspaceSave;
        private WorkflowInputDataViewModel _workflowInputDataViewModel;
        private string _workflowLink;
        private ICommand _openWorkflowLinkCommand;
        private bool _firstWorkflowChange;
        private readonly IAsyncWorker _asyncWorker;
        private readonly IExternalProcessExecutor _executor;
        private string _expressionString;
        private ICommand _debugInputsCommand;
        private ICommand _debugStudioCommand;
        private ICommand _debugBrowserCommand;
        private ICommand _scheduleCommand;
        private ICommand _testEditorCommand;
        private ICommand _runAllTestsCommand;
        private ICommand _duplicateCommand;
        private ICommand _deployCommand;
        private ICommand _showDependenciesCommand;
        private ICommand _viewSwaggerCommand;
        private ICommand _copyUrlCommand;
        private DebugOutputViewModel _debugOutputViewModel;
        private IDataListViewModel _dataListViewModel;
        private bool _canDebugInputs;
        private bool _canDebugStudio;
        private bool _debugBrowser;
        private bool _canCreateSchedule;
        private bool _canCreateTest;
        private bool _canRunAllTests;
        private bool _canDuplicate;
        private bool _canDeploy;
        private bool _canShowDependencies;
        private bool _canViewSwagger;
        private bool _canCopyUrl;
        private string _copyUrlTooltip;
        private string _viewSwaggerTooltip;
        private string _debugInputsTooltip;
        private string _debugStudioTooltip;
        private string _debugBrowserTooltip;
        private string _scheduleTooltip;
        private string _createTestTooltip;
        private string _runAllTestsTooltip;
        private string _duplicateTooltip;
        private string _deployTooltip;
        private string _showDependenciesTooltip;
        private ICommand _newServiceCommand;
        private ModelItem _selectedItem;

        /// <summary>
        /// Models the service model changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ModelChangedEventArgs"/> instance containing the event data.</param>
        protected void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        {
            if (e.ModelChangeInfo != null &&
                e.ModelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged)
            {
                if (SelfConnectProperties.Contains(e.ModelChangeInfo.PropertyName))
                {
                    if (e.ModelChangeInfo.Subject == e.ModelChangeInfo.Value)
                    {
                        var modelProperty = e.ModelChangeInfo.Value.Properties[e.ModelChangeInfo.PropertyName];
                        modelProperty?.ClearValue();
                    }
                    return;
                }
                if (e.ModelChangeInfo.PropertyName == "StartNode")
                {
                    return;
                }

                if (e.ModelChangeInfo.PropertyName == "Handler")
                {
                    if (DataObject != null)
                    {
                        ModelItemPropertyChanged(e);
                    }
                }
            }

            if (e.ModelChangeInfo != null && e.ModelChangeInfo.ModelChangeType == ModelChangeType.CollectionItemAdded)
            {
                PerformAddItems(new List<ModelItem> { e.ModelChangeInfo.Value });
            }
            WorkflowChanged?.Invoke();
        }

        private void ModelItemPropertyChanged(ModelChangedEventArgs e)
        {
            Guid? envID = null;
            Guid? resourceID = null;
            var explorerItem = DataObject as IExplorerItemViewModel;
            if (explorerItem != null)
            {
                if (explorerItem.Server != null)
                    envID = explorerItem.Server.EnvironmentID;
                resourceID = explorerItem.ResourceId;
            }

            ModelProperty modelProperty = e.ModelChangeInfo.Subject.Content;

            if (envID != null && modelProperty != null)
            {
                IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == envID);
                var resource = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceID.Value);
                if (resource != null)
                {
                    DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());
                    d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.Category;
                    UpdateForRemote(d, resource);
                    modelProperty.SetValue(d);
                }
            }
            DataObject = null;
            WorkflowChanged?.Invoke();
        }

        private void UpdateForRemote(DsfActivity d, IContextualResourceModel resource)
        {
            if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                dynamic mvm = Application.Current.MainWindow.DataContext;
                if (mvm != null && mvm.ActiveItem != null)
                {
                    WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(d, resource, mvm.ActiveItem.Environment);
                }
            }
            else
            {
                if (ActiveEnvironment != null)
                {
                    WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(d, resource, ActiveEnvironment);
                }
            }
        }

        protected IEnvironmentModel ActiveEnvironment { get; set; }

        /// <summary>
        ///     Handler attached to intercept checks for executing the delete command
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        [ExcludeFromCodeCoverage]
        private void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete ||      //triggered from deleting an activity
                e.Command == EditingCommands.Delete ||          //triggered from editing displayname, expressions, etc
                e.Command == System.Activities.Presentation.View.DesignerView.CopyCommand ||
                e.Command == System.Activities.Presentation.View.DesignerView.CutCommand)
            {
                PreventCommandFromBeingExecuted(e);
            }
        }


        /// <summary>
        ///     Handler attached to intercept checks for executing the delete command
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        [ExcludeFromCodeCoverage]
        private void PreviewExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                _wd?.View?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }

            if (e.Command == System.Activities.Presentation.View.DesignerView.PasteCommand)
            {

                var dataObject = Clipboard.GetDataObject();
                if (dataObject != null)
                {
                    var dataPresent = dataObject.GetDataPresent("WorkflowXamlFormat");
                    if (dataPresent)
                    {
                        var data = dataObject.GetData("WorkflowXamlFormat") as string;
                        if (!string.IsNullOrEmpty(data))
                        {
                            var indexOf = data.IndexOf("ResourceID=", StringComparison.InvariantCultureIgnoreCase);
                            var guid = data.Substring(indexOf + 12, 36);
                            if (guid.Equals(ResourceModel.ID.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                e.Handled = true;
                                return;
                            }
                        }
                    }
                }
                new Task(() =>
                {
                    BuildWorkflowFields();
                }).Start();
            }
        }

        #endregion

        #region OnDispose
        protected override void OnDispose()
        {
            if (_wd != null)
            {
                _wd.ModelChanged -= WdOnModelChanged;
                _wd.Context.Services.Unsubscribe<ModelService>(ModelServiceSubscribe);

                _wd.View.PreviewDrop -= ViewPreviewDrop;

                _wd.View.PreviewMouseDown -= ViewPreviewMouseDown;

                _wd.Context.Services.Unsubscribe<DesignerView>(DesigenrViewSubscribe);
                _virtualizedContainerService = null;
                _virtualizedContainerServicePopulateAllMethod = null;
            }

            DesignerManagementService?.Dispose();
            _debugSelectionChangedService?.Unsubscribe();

            if (_resourceModel != null)
            {
                _resourceModel.OnDataListChanged -= FireWdChanged;
                _resourceModel.OnResourceSaved -= UpdateOriginalDataList;
            }

            if (ModelService != null)
            {
                ModelService.ModelChanged -= ModelServiceModelChanged;
            }

            if (_uniqueWorkflowParts != null)
            {
                _uniqueWorkflowParts.Clear();
                _uniqueWorkflowParts = null;
            }

            // remove this value from our helper class's cache ;)
            if (ResourceModel != null)
            {
                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);
                OpeningWorkflowsHelper.PruneWorkflowFromCaches(workSurfaceKey);
            }
            if (_workflowInputDataViewModel != null)
            {
                _workflowInputDataViewModel.Dispose();
                _workflowInputDataViewModel = null;
            }
            try
            {
                CEventHelper.RemoveAllEventHandlers(_wd);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
            _debugSelectionChangedService?.Unsubscribe();
            base.OnDispose();
        }

        #endregion

        /// <summary>
        /// Gets the work surface context.
        /// </summary>
        /// <value>
        /// The work surface context.
        /// </value>
        public override WorkSurfaceContext WorkSurfaceContext => ResourceModel?.ResourceType.ToWorkSurfaceContext() ?? WorkSurfaceContext.Unknown;

        #region Overrides of ViewAware

        #endregion

        /// <summary>
        /// Gets the environment model.
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnvironmentModel EnvironmentModel => ResourceModel.Environment;

        protected List<ModelItem> SelectedDebugItems => _selectedDebugItems;
        public ModelItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
            }
        }
        public bool WorkspaceSave => _workspaceSave;

        #region Implementation of IHandle<EditActivityMessage>

        public void Handle(EditActivityMessage message)
        {
            Dev2Logger.Info(message.GetType().Name);
            EditActivity(message.ModelItem, message.ParentEnvironmentID);
        }

        #endregion

        private void FireWdChanged()
        {
            WdOnModelChanged(new object(), new EventArgs());
            GetWorkflowLink();
        }

        #region Implementation of IHandle<SaveUnsavedWorkflow>

        public void Handle(SaveUnsavedWorkflowMessage message)
        {
            if (message?.ResourceModel == null || string.IsNullOrEmpty(message.ResourceName) || message.ResourceModel.ID != ResourceModel.ID)
            {
                return;
            }
            var resourceModel = message.ResourceModel;
            WorkspaceItemRepository.Instance.Remove(resourceModel);
            var unsavedName = resourceModel.ResourceName;
            UpdateResourceModel(message, resourceModel, unsavedName);
            PublishMessages(resourceModel);
            DisposeDesigner();
            ActivityDesignerHelper.AddDesignerAttributes(this);
            _workflowInputDataViewModel = WorkflowInputDataViewModel.Create(_resourceModel);
            UpdateWorkflowLink(GetWorkflowLink());
            NotifyOfPropertyChange(() => DesignerView);
            RemoveUnsavedWorkflowName(unsavedName);
        }
        internal void RemoveUnsavedWorkflowName(string unsavedName)
        {
            NewWorkflowNames.Instance.Remove(unsavedName);
        }
        internal void RemoveAllWorkflowName(string unsavedName)
        {
            NewWorkflowNames.Instance.RemoveAll(unsavedName);
        }
        private void DisposeDesigner()
        {
            if (_wd != null)
            {
                _wd.ModelChanged -= WdOnModelChanged;
                _wd.Context.Services.Unsubscribe<ModelService>(ModelServiceSubscribe);

                _wd.View.PreviewDrop -= ViewPreviewDrop;
                _wd.View.PreviewMouseDown -= ViewPreviewMouseDown;

                _wd.Context.Services.Unsubscribe<DesignerView>(DesigenrViewSubscribe);
                _virtualizedContainerService = null;
                _virtualizedContainerServicePopulateAllMethod = null;
            }

            DesignerManagementService?.Dispose();
            if (ModelService != null)
            {
                ModelService.ModelChanged -= ModelServiceModelChanged;
            }
            _debugSelectionChangedService?.Unsubscribe();
        }

        private void PublishMessages(IContextualResourceModel resourceModel)
        {
            UpdateResource(resourceModel);
            Dev2Logger.Info("Publish message of type - " + typeof(UpdateResourceMessage));
            EventPublisher.Publish(new UpdateResourceMessage(resourceModel));
        }

        private void UpdateResource(IContextualResourceModel resourceModel)
        {
            if (ContexttualResourceModelEqualityComparer.Current.Equals(resourceModel, _resourceModel))
            {
                IObservableReadOnlyList<IErrorInfo> currentErrors = null;
                if (resourceModel.Errors != null && resourceModel.Errors.Count > 0)
                {
                    currentErrors = resourceModel.Errors;
                }
                _resourceModel.Update(resourceModel);
                if (currentErrors != null && currentErrors.Count > 0)
                {
                    foreach (var currentError in currentErrors)
                    {
                        _resourceModel.AddError(currentError);
                    }
                }
            }
        }
        private void UpdateResourceModel(SaveUnsavedWorkflowMessage message, IContextualResourceModel resourceModel, string unsavedName)
        {
            resourceModel.ResourceName = message.ResourceName;
            resourceModel.DisplayName = message.ResourceName;
            resourceModel.Category = message.ResourceCategory;
            resourceModel.WorkflowXaml = ServiceDefinition?.Replace(unsavedName, message.ResourceName);
            resourceModel.IsNewWorkflow = false;
            resourceModel.Environment.ResourceRepository.SaveToServer(resourceModel);
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            var environmentViewModel = mainViewModel?.ExplorerViewModel?.Environments.FirstOrDefault(model => model.Server.EnvironmentID == resourceModel.Environment.ID);
            if (environmentViewModel != null)
            {
                var item = environmentViewModel.FindByPath(resourceModel.GetSavePath());
                var viewModel = environmentViewModel as EnvironmentViewModel;
                var savedItem = viewModel?.CreateExplorerItemFromResource(environmentViewModel.Server, item, false, false, resourceModel);
                item.AddChild(savedItem);
            }
            resourceModel.IsWorkflowSaved = true;
        }

        #region Implementation of IWorkflowDesignerViewModel

        public System.Action WorkflowChanged { get; set; }

        #endregion

        #endregion
    }
}
