#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Collections;
using Dev2.Common.Interfaces.Enums;
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
using Dev2.Instrumentation;
using Dev2.Messages;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Enums;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.Utilities;
using Dev2.Utils;
using Dev2.ViewModels.Workflow;
using Dev2.Workspaces;
using Newtonsoft.Json;
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
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xaml;
using System.Xml.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.ViewModels;
using Dev2.ViewModels.Merge;
using Dev2.Communication;
using System.IO;
using Dev2.Common.Interfaces;
using Dev2.Activities.Designers2.Gate;
using Dev2.Activities;
using Warewolf.Data;
using Warewolf.Data.Options;
using StringExtension = Dev2.Common.ExtMethods.StringExtension;

namespace Dev2.Studio.ViewModels.Workflow
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

        protected readonly IDesignerManagementService _designerManagementService;
        readonly IWorkflowHelper _workflowHelper;
        DelegateCommand _collapseAllCommand;

        protected dynamic DataObject { get; set; }
        List<ModelItem> _selectedDebugItems = new List<ModelItem>();
        DelegateCommand _expandAllCommand;

        protected ModelService _modelService;
        IContextualResourceModel _resourceModel;

        protected Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts;

        protected WorkflowDesigner _wd;
        DesignerMetadata _wdMeta;

        VirtualizedContainerService _virtualizedContainerService;
        MethodInfo _virtualizedContainerServicePopulateAllMethod;

        readonly StudioSubscriptionService<DebugSelectionChangedEventArgs> _debugSelectionChangedService = new StudioSubscriptionService<DebugSelectionChangedEventArgs>();

        readonly IApplicationTracker _applicationTracker;
        public bool IsStartNodeErrorMessageSet { get; set; }

        protected IWorkflowDesignerWrapper _workflowDesignerHelper;

        public WorkflowDesignerViewModel(IContextualResourceModel resource)
            : this(resource, true)
        {
        }

        public WorkflowDesignerViewModel(IContextualResourceModel resource, bool createDesigner)
            : this(resource, new WorkflowHelper(), createDesigner)
        {
        }

        public WorkflowDesignerViewModel(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner)
            : this(EventPublishers.Aggregator, resource, workflowHelper, createDesigner)
        {
        }

        WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner)
            : this(eventPublisher, resource, workflowHelper,
                CustomContainer.Get<IPopupController>(), new AsyncWorker(), createDesigner)
        {
        }

        public WorkflowDesignerViewModel(IWorkflowDesignerWrapper workflowDesignerHelper, IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, IPopupController popupController, IAsyncWorker asyncWorker, bool createDesigner, bool liteInit)
            : this(eventPublisher, resource, workflowHelper, popupController, asyncWorker, createDesigner, liteInit)
        {
            _workflowDesignerHelper = workflowDesignerHelper;
        }

        public WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, IPopupController popupController, IAsyncWorker asyncWorker, bool createDesigner)
            : this(eventPublisher, resource, workflowHelper, popupController, asyncWorker, createDesigner, false)
        {
        }

        public WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, IPopupController popupController, IAsyncWorker asyncWorker, bool createDesigner, bool liteInit)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("workflowHelper", workflowHelper);
            VerifyArgument.IsNotNull("popupController", popupController);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _workflowHelper = workflowHelper;
            _resourceModel = resource;
            _resourceModel.OnDataListChanged += FireWdChanged;
            _resourceModel.OnResourceSaved += UpdateOriginalDataList;
            _asyncWorker = asyncWorker;
            CanViewWorkflowLink = true;

            PopUp = popupController;

            if (_resourceModel.DataList != null)
            {
                SetOriginalDataList(_resourceModel);
            }
            _designerManagementService = new DesignerManagementService(resource, _resourceModel.Environment.ResourceRepository);
            if (createDesigner)
            {
                ActivityDesignerHelper.AddDesignerAttributes(this, liteInit);
            }
            UpdateWorkflowInputDataViewModel(_resourceModel);
            UpdateWorkflowLink();
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(_resourceModel);
            DebugOutputViewModel = new DebugOutputViewModel(_resourceModel.Environment.Connection.ServerEvents, CustomContainer.Get<IServerRepository>(), new DebugOutputFilterStrategy(), ResourceModel);
            _firstWorkflowChange = true;
            _workflowDesignerHelper = new WorkflowDesignerWrapper();
            _applicationTracker = CustomContainer.Get<IApplicationTracker>();
            _shellViewModel = GetShellViewModel();
        }

        private static IShellViewModel GetShellViewModel()
        {
            IShellViewModel shellViewModel;
            try
            {
                if (Application.Current?.MainWindow?.DataContext is IShellViewModel tmpShellViewModel)
                {
                    return tmpShellViewModel;
                }
            }
            catch
            {
            }
            return CustomContainer.Get<IShellViewModel>();
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

        void SetExecutePermissions()
        {
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanRunAllTests = !ResourceModel.IsNewWorkflow;
            CanViewSwagger = !ResourceModel.IsNewWorkflow;
            CanCopyUrl = !ResourceModel.IsNewWorkflow;
        }

        void SetViewPermissions()
        {
            CanViewSwagger = !ResourceModel.IsVersionResource;
            CanCopyUrl = !ResourceModel.IsVersionResource;
        }

        void SetNonePermissions()
        {
            CanDebugInputs = false;
            CanDebugStudio = false;
            CanDebugBrowser = false;
            CanCreateSchedule = false;
            CanCreateQueueEvent = false;
            CanCreateTest = false;
            CanRunAllTests = false;
            CanDuplicate = false;
            CanDeploy = false;
            CanMerge = false;
            CanShowDependencies = false;
            CanViewSwagger = false;
            CanCopyUrl = false;
        }

        void SetAdministratorPermissions()
        {
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanCreateSchedule = !ResourceModel.IsNewWorkflow;
            CanCreateQueueEvent = !ResourceModel.IsNewWorkflow;
            CanCreateTest = !ResourceModel.IsNewWorkflow;
            CanRunAllTests = !ResourceModel.IsNewWorkflow;
            CanDuplicate = !ResourceModel.IsNewWorkflow;
            CanDeploy = !ResourceModel.IsNewWorkflow;
            CanMerge = !ResourceModel.IsNewWorkflow;
            CanShowDependencies = !ResourceModel.IsNewWorkflow;
            CanViewSwagger = !ResourceModel.IsNewWorkflow;
            CanCopyUrl = !ResourceModel.IsNewWorkflow;
        }

        void SetContributePermissions()
        {
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanCreateSchedule = !ResourceModel.IsNewWorkflow;
            CanCreateQueueEvent = !ResourceModel.IsNewWorkflow;
            CanCreateTest = !ResourceModel.IsNewWorkflow;
            CanRunAllTests = !ResourceModel.IsNewWorkflow;
            CanDuplicate = !ResourceModel.IsNewWorkflow;
            CanDeploy = !ResourceModel.IsNewWorkflow;
            CanMerge = !ResourceModel.IsNewWorkflow;
            CanShowDependencies = !ResourceModel.IsNewWorkflow;
            CanViewSwagger = !ResourceModel.IsNewWorkflow;
            CanCopyUrl = !ResourceModel.IsNewWorkflow;
        }

        public bool CanCopyUrl
        {
            get => _canCopyUrl;
            set
            {
                _canCopyUrl = value;
                CopyUrlTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canCopyUrl ? Warewolf.Studio.Resources.Languages.Tooltips.CopyUrlToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanCopyUrl");
            }
        }

        public string CopyUrlTooltip
        {
            get => _copyUrlTooltip;
            set
            {
                _copyUrlTooltip = value;
                OnPropertyChanged("CopyUrlTooltip");
            }
        }

        public bool CanViewSwagger
        {
            get => _canViewSwagger;
            set
            {
                _canViewSwagger = value;
                ViewSwaggerTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canViewSwagger ? Warewolf.Studio.Resources.Languages.Tooltips.ViewSwaggerToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanViewSwagger");
            }
        }

        public string ViewSwaggerTooltip
        {
            get => _viewSwaggerTooltip;
            set
            {
                _viewSwaggerTooltip = value;
                OnPropertyChanged("ViewSwaggerTooltip");
            }
        }

        public bool CanShowDependencies
        {
            get => _canShowDependencies;
            set
            {
                _canShowDependencies = value;
                ShowDependenciesTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canShowDependencies ? Warewolf.Studio.Resources.Languages.Tooltips.DependenciesToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanShowDependencies");
            }
        }

        public string ShowDependenciesTooltip
        {
            get => _showDependenciesTooltip;
            set
            {
                _showDependenciesTooltip = value;
                OnPropertyChanged("ShowDependenciesTooltip");
            }
        }

        public bool CanDeploy
        {
            get => _canDeploy;
            set
            {
                _canDeploy = value;
                DeployTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canDeploy ? Warewolf.Studio.Resources.Languages.Tooltips.DeployToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanDeploy");
            }
        }

        public bool CanMerge
        {
            get
            {
                if (ResourceModel.IsVersionResource || (GetVersionHistory() != null && _canMerge))
                {
                    return true;
                }

                return false;
            }
            set
            {
                _canMerge = value;
                MergeTooltip = Warewolf.Studio.Resources.Languages.Tooltips.ViewMergeTooltip;
                OnPropertyChanged("CanMerge");
            }
        }

        ICollection<IVersionInfo> GetVersionHistory()
        {
            var versionInfos = Server?.ExplorerRepository?.GetVersions(ResourceModel.ID);
            if (versionInfos?.Count <= 0)
            {
                return null;
            }

            return versionInfos;
        }

        public string DeployTooltip
        {
            get => _deployTooltip;
            set
            {
                _deployTooltip = value;
                OnPropertyChanged("DeployTooltip");
            }
        }

        public string MergeTooltip
        {
            get => _mergeTooltip;
            set
            {
                _mergeTooltip = value;
                OnPropertyChanged("MergeTooltip");
            }
        }

        public bool CanDuplicate
        {
            get => _canDuplicate;
            set
            {
                _canDuplicate = value;
                DuplicateTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canDuplicate ? Warewolf.Studio.Resources.Languages.Tooltips.DuplicateToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanDuplicate");
            }
        }

        public string DuplicateTooltip
        {
            get => _duplicateTooltip;
            set
            {
                _duplicateTooltip = value;
                OnPropertyChanged("DuplicateTooltip");
            }
        }

        public bool CanRunAllTests
        {
            get => _canRunAllTests;
            set
            {
                _canRunAllTests = value;
                RunAllTestsTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canRunAllTests ? Warewolf.Studio.Resources.Languages.Tooltips.RunAllTestsToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                RunCoverageTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canRunAllTests ? Warewolf.Studio.Resources.Languages.Tooltips.RunCoverageToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanRunAllTests");
            }
        }

        public string RunAllTestsTooltip
        {
            get => _runAllTestsTooltip;
            set
            {
                _runAllTestsTooltip = value;
                OnPropertyChanged("RunAllTestsTooltip");
            }
        }
        
        public string RunCoverageTooltip
        {
            get => _runCoverageTooltip;
            set
            {
                _runCoverageTooltip = value;
                OnPropertyChanged(nameof(RunCoverageTooltip));
            }
        }

        public bool CanCreateTest
        {
            get => _canCreateTest;
            set
            {
                _canCreateTest = value;
                CreateTestTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canCreateTest ? Warewolf.Studio.Resources.Languages.Tooltips.TestEditorToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanCreateTest");
            }
        }

        public string CreateTestTooltip
        {
            get => _createTestTooltip;
            set
            {
                _createTestTooltip = value;
                OnPropertyChanged("CreateTestTooltip");
            }
        }

        public bool CanCreateSchedule
        {
            get => _canCreateSchedule;
            set
            {
                _canCreateSchedule = value;
                ScheduleTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canCreateSchedule ? Warewolf.Studio.Resources.Languages.Tooltips.ScheduleToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanCreateSchedule");
            }
        }

        public string ScheduleTooltip
        {
            get => _scheduleTooltip;
            set
            {
                _scheduleTooltip = value;
                OnPropertyChanged("ScheduleTooltip");
            }
        }

        public bool CanCreateQueueEvent
        {
            get => _canCreateQueueEvent;
            set
            {
                _canCreateQueueEvent = value;
                QueueEventTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canCreateSchedule ? Warewolf.Studio.Resources.Languages.Tooltips.QueueEventToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanCreateQueueEvent");
            }
        }

        public string QueueEventTooltip
        {
            get => _queueEventTooltip;
            set
            {
                _queueEventTooltip = value;
                OnPropertyChanged("QueueEventTooltip");
            }
        }

        public bool CanDebugBrowser
        {
            get => _debugBrowser;
            set
            {
                _debugBrowser = value;
                DebugBrowserTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _debugBrowser ? Warewolf.Studio.Resources.Languages.Tooltips.StartNodeDebugBrowserToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanDebugBrowser");
            }
        }

        public string DebugBrowserTooltip
        {
            get => _debugBrowserTooltip;
            set
            {
                _debugBrowserTooltip = value;
                OnPropertyChanged("DebugBrowserTooltip");
            }
        }

        public bool CanDebugStudio
        {
            get => _canDebugStudio;
            set
            {
                _canDebugStudio = value;
                DebugStudioTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canDebugStudio ? Warewolf.Studio.Resources.Languages.Tooltips.StartNodeDebugStudioToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanDebugStudio");
            }
        }

        public string DebugStudioTooltip
        {
            get => _debugStudioTooltip;
            set
            {
                _debugStudioTooltip = value;
                OnPropertyChanged("DebugStudioTooltip");
            }
        }

        public bool CanDebugInputs
        {
            get => _canDebugInputs;
            set
            {
                _canDebugInputs = value;
                DebugInputsTooltip = ResourceModel.IsNewWorkflow ? Warewolf.Studio.Resources.Languages.Tooltips.DisabledToolTip : _canDebugInputs ? Warewolf.Studio.Resources.Languages.Tooltips.StartNodeDebugInputsToolTip : Warewolf.Studio.Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged("CanDebugInputs");
            }
        }

        public string DebugInputsTooltip
        {
            get => _debugInputsTooltip;
            set
            {
                _debugInputsTooltip = value;
                OnPropertyChanged("DebugInputsTooltip");
            }
        }

        void SetOriginalDataList(IContextualResourceModel contextualResourceModel)
        {
            if (!string.IsNullOrEmpty(contextualResourceModel.DataList))
            {
                _originalDataList = contextualResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "").Replace(Environment.NewLine, "").Trim();
            }
        }

        void UpdateOriginalDataList(IContextualResourceModel obj)
        {
            if (obj.IsWorkflowSaved)
            {
                SetOriginalDataList(obj);
            }
        }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get => _debugOutputViewModel;
            set => _debugOutputViewModel = value;
        }

        public IDataListViewModel DataListViewModel
        {
            get => _dataListViewModel;
            set
            {
                _dataListViewModel = value;
                NotifyOfPropertyChange(() => DataListViewModel);
            }
        }

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
        
        public string GetAndUpdateWorkflowLinkWithWorkspaceID()
        {
            UpdateWorkflowLink();
            return _workflowLinkWithWid;
        }

        public void UpdateWorkflowLink()
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
                var buildWebPayLoad = _workflowInputDataViewModel.BuildInputDataList();

                var uri = _resourceModel.GetWorkflowUri(buildWebPayLoad, UrlType.Json, true);
                if (uri != null) {
                    _workflowLinkWithWid = uri.ToString();
                    var startIndex = _workflowLinkWithWid.IndexOf("&wid", StringComparison.InvariantCultureIgnoreCase);
                    if (startIndex != -1)
                    {
                        _workflowLink = _workflowLinkWithWid.Remove(startIndex);
                    }
                }
            }
            NotifyOfPropertyChange(() => DisplayWorkflowLink);
        }

        public string GetWorkflowInputs(string field)
        {
            var workflowInputDataViewModel = _workflowInputDataViewModel as WorkflowInputDataViewModel;
            var inputsValue = workflowInputDataViewModel?.WorkflowInputs?.FirstOrDefault(o => o.Field == field);
            return inputsValue?.Value;
        }

        public string DisplayWorkflowLink
        {
            get
            {
                return _workflowLink;
            }
            private set
            {
                _workflowLink = value;
            }
        }

        public Visibility WorkflowLinkVisible => _resourceModel.IsVersionResource ? Visibility.Hidden : Visibility.Visible;
        public bool CanViewWorkflowLink { get; set; }

        public IPopupController PopUp { get; set; }
        
        public virtual object SelectedModelItem => _wd?.Context?.Items.GetValue<Selection>().SelectedObjects.FirstOrDefault();

        public IContextualResourceModel ResourceModel
        {
            get => _resourceModel;
            set => _resourceModel = value;
        }

        public string WorkflowName => _resourceModel.ResourceName;

        public WorkflowDesigner Designer => _wd;

        public UIElement DesignerView => _wd?.View;

        public StringBuilder DesignerText => ServiceDefinition;

        public StringBuilder ServiceDefinition => _workflowHelper.SerializeWorkflow(_modelService);

        public ICommand CollapseAllCommand => _collapseAllCommand ?? (_collapseAllCommand = new DelegateCommand(param =>
        {
            var val = Convert.ToBoolean(param);
            if (val)
            {
                _designerManagementService.RequestCollapseAll();
            }
            else
            {
                _designerManagementService.RequestRestoreAll();
            }
        }));

        public ICommand ExpandAllCommand => _expandAllCommand ?? (_expandAllCommand = new DelegateCommand(param =>
        {
            var val = Convert.ToBoolean(param);
            if (val)
            {
                _designerManagementService.RequestExpandAll();
            }
            else
            {
                _designerManagementService.RequestRestoreAll();
            }
        }));

        public ICommand OpenWorkflowLinkCommand
        {
            get
            {
                return _openWorkflowLinkCommand ?? (_openWorkflowLinkCommand = new DelegateCommand(param =>
                {
                    if (!string.IsNullOrEmpty(_workflowLink))
                    {
                        if (_applicationTracker != null)
                        {
                            _applicationTracker.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventMenu.EventCategory,
                                                                Warewolf.Studio.Resources.Languages.TrackEventMenu.LinkUrl);
                        }
                        SaveToWorkspace();
                        if (_workflowInputDataViewModel.WorkflowInputCount == 0)
                        {
                            PopUp.ShowNoInputsSelectedWhenClickLink();
                        }
                        try
                        {
                            OpenLinkInBrowser();
                        }
                        catch (Exception e)
                        {
                            Dev2Logger.Error("OpenWorkflowLinkCommand", e, GlobalConstants.WarewolfError);
                        }
                    }
                }));
            }
        }

        public ICommand NewServiceCommand => _newServiceCommand ?? (_newServiceCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.NewService("");
                }
            }
        }));

        public ICommand DebugInputsCommand => _debugInputsCommand ?? (_debugInputsCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.DebugCommand.Execute(mvm.ActiveItem);
                }
            }
        }));

        public ICommand DebugStudioCommand => _debugStudioCommand ?? (_debugStudioCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.QuickDebugCommand.Execute(mvm.ActiveItem);
                }
            }
        }));

        public ICommand DebugBrowserCommand => _debugBrowserCommand ?? (_debugBrowserCommand = new DelegateCommand(param =>
        {
            OpenLinkInBrowser();
        }));

        static void OpenLinkInBrowser()
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.QuickViewInBrowserCommand.Execute(mvm.ActiveItem);
                }
            }
        }

        public ICommand ScheduleCommand => _scheduleCommand ?? (_scheduleCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.CreateNewSchedule(mvm.ActiveItem.ContextualResourceModel.ID);
                }
            }
        }));

        public ICommand QueueEventCommand => _queueEventCommand ?? (_queueEventCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.CreateNewQueueEvent(mvm.ActiveItem.ContextualResourceModel.ID);
                }
            }
        }));

        public ICommand TestEditorCommand => _testEditorCommand ?? (_testEditorCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.CreateTest(mvm.ActiveItem.ContextualResourceModel.ID);
                }
            }
        }));

        public ICommand DuplicateCommand => _duplicateCommand ?? (_duplicateCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    IExplorerItemViewModel explorerItem = null;
                    var environmentViewModels = mvm.ExplorerViewModel.Environments.Where(a => a.ResourceId == mvm.ActiveServer.EnvironmentID);
                    foreach (var environmentViewModel in environmentViewModels)
                    {
                        explorerItem = environmentViewModel.Children.Flatten(model => model.Children).FirstOrDefault(c => c.ResourceId == mvm.ActiveItem.ContextualResourceModel.ID);
                    }

                    if (explorerItem != null)
                    {
                        mvm.DuplicateResource(explorerItem);
                    }
                }
            }
        }));

        public ICommand DeployCommand => _deployCommand ?? (_deployCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    var explorerItem = GetSelected(mvm);
                    if (explorerItem != null)
                    {
                        mvm.AddDeploySurface(explorerItem.AsList().Union(new[] { explorerItem }));
                    }
                }
            }
        }));

        public ICommand MergeCommand => _mergeCommand ?? (_mergeCommand = new DelegateCommand(param =>
        {
            if (Application.Current?.Dispatcher == null || !Application.Current.Dispatcher.CheckAccess() || Application.Current?.MainWindow == null)
            {
                return;
            }
            MergeWorkflow();
        }));

        public IShellViewModel ShellViewModel => _shellViewModel ?? CustomContainer.Get<IShellViewModel>();
        
        private static void MergeWorkflow()
        {
            var shellViewModel = Application.Current.MainWindow.DataContext as ShellViewModel;
            if (shellViewModel?.ActiveItem == null)
            {
                return;
            }

            var explorerItem = shellViewModel.ActiveItem.ContextualResourceModel.IsVersionResource
                             ? GetMergeResourceVersion(shellViewModel)
                             : GetMergeCurrentResource(shellViewModel);

            if (explorerItem == null)
            {
                return;
            }
            shellViewModel.OpenMergeDialogView(explorerItem);
        }

        private static IExplorerItemViewModel GetMergeResourceVersion(ShellViewModel shellViewModel)
        {
            var resourceId = shellViewModel.ActiveItem.ContextualResourceModel.OriginalId;
            var environmentViewModel = shellViewModel.ExplorerViewModel.Environments.FirstOrDefault(a => a.ResourceId == shellViewModel.ActiveServer.EnvironmentID);
            return environmentViewModel?.UnfilteredChildren?.Flatten(model => model.UnfilteredChildren).FirstOrDefault(c => c.ResourceId == resourceId);
        }

        private static IExplorerItemViewModel GetMergeCurrentResource(ShellViewModel shellViewModel)
        {
            var resourceId = shellViewModel.ActiveItem.ContextualResourceModel.ID;
            var environmentViewModel = shellViewModel.ExplorerViewModel.Environments.FirstOrDefault(a => a.ResourceId == shellViewModel.ActiveServer.EnvironmentID);
            return environmentViewModel?.UnfilteredChildren?.Flatten(model => model.UnfilteredChildren).Where(a => !a.IsVersion).FirstOrDefault(c => c.ResourceId == resourceId);
        }

        static IExplorerItemViewModel GetSelected(ShellViewModel mvm)
        {
            IExplorerItemViewModel explorerItem = null;
            if (mvm?.ActiveServer != null)
            {
                var environmentViewModels = mvm.ExplorerViewModel.Environments.Where(a => a.ResourceId == mvm.ActiveServer.EnvironmentID);
                foreach (var environmentViewModel in environmentViewModels)
                {
                    explorerItem =
                        environmentViewModel.UnfilteredChildren.Flatten(model => model.UnfilteredChildren)
                            .FirstOrDefault(c => c.ResourceId == mvm.ActiveItem.ContextualResourceModel.ID);
                }
            }
            return explorerItem;
        }

        public ICommand ShowDependenciesCommand => _showDependenciesCommand ?? (_showDependenciesCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                var explorerItem = GetSelected(mvm);
                if (explorerItem != null)
                {
                    mvm.ShowDependencies(mvm.ActiveItem.ContextualResourceModel.ID, mvm.ActiveServer, explorerItem.IsSource || explorerItem.IsServer);
                }
            }
        }));

        public ICommand ViewSwaggerCommand => _viewSwaggerCommand ?? (_viewSwaggerCommand = new DelegateCommand(param =>
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.ViewSwagger(mvm.ActiveItem.ContextualResourceModel.ID, mvm.ActiveServer);
                }
            }
        }));

        public ICommand CopyUrlCommand => _copyUrlCommand ?? (_copyUrlCommand = new DelegateCommand(param =>
        {
            Clipboard.SetText(_workflowLink);
        }));
        
        protected ModelItem PerformAddItems(ModelItem addedItem)


        {
            var mi = addedItem;
            var computedValue = mi.Content?.ComputedValue;

            //Track added items when dragged on design surface
            if (computedValue != null && computedValue.GetType() != typeof(DsfActivity))
            {
                if (_applicationTracker != null)
                {
                    _applicationTracker.TrackCustomEvent(Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.EventCategory,
                                                    Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.ItemDragged, computedValue.ToString());
                }
            }
            if (computedValue == null && (mi.ItemType == typeof(DsfFlowDecisionActivity) ||
                                          mi.ItemType == typeof(DsfFlowSwitchActivity)))
            {
                computedValue = mi.Source?.Value?.Source?.ComputedValue;
            }
            if (computedValue is IDev2Activity act)
            {
                if (_isPaste || string.IsNullOrEmpty(act.UniqueID))
                {
                    act.UniqueID = Guid.NewGuid().ToString();
                }
                _modelItems = _modelService.Find(_modelService.Root, typeof(IDev2Activity));
            }
            if (computedValue is Activity)
            {
                _activityCollection = _modelService.Find(_modelService.Root, typeof(Activity));
            }

            if (mi.ItemType == typeof(FlowSwitch<string>))
            {
                InitializeFlowSwitch(mi);
            }
            else if (mi.ItemType == typeof(FlowDecision))
            {
                InitializeFlowDecision(mi);
                if (_applicationTracker != null)
                {
                    _applicationTracker.TrackCustomEvent(Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.EventCategory,
                                                    Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.ItemDragged, mi.ItemType.Name);
                }
            }
            else if (mi.ItemType == typeof(FlowStep))
            {
                InitializeFlowStep(mi);
            }
            else
            {
                AddSwitch(mi);
            }
            _isPaste = false;
            return addedItem;
        }

        void AddSwitch(ModelItem mi)
        {
            if (mi.Parent?.Parent?.Parent != null && mi.Parent.Parent.Parent.ItemType == typeof(FlowSwitch<string>))
            {
                var activityExpression = mi.Parent.Parent.Parent.Properties["Expression"];
                if (activityExpression != null)
                {
                    var switchExpressionValue = SwitchExpressionValue(activityExpression);
                    var modelProperty = mi.Properties["Key"];
                    if (modelProperty?.Value != null && (FlowController.OldSwitchValue == null || string.IsNullOrWhiteSpace(FlowController.OldSwitchValue)))
                    {
                        FlowController.ConfigureSwitchCaseExpression(new ConfigureCaseExpressionMessage { ModelItem = mi, ExpressionText = switchExpressionValue, Server = _resourceModel.Environment, IsPaste = _isPaste });
                    }
                }
            }
        }
        
        static string SwitchExpressionValue(ModelProperty activityExpression)
        {
            var tmpModelItem = activityExpression.Value;
            var switchExpressionValue = string.Empty;
            var tmpProperty = tmpModelItem?.Properties["ExpressionText"];
            var tmp = tmpProperty?.Value?.ToString();

            if (!string.IsNullOrEmpty(tmp))
            {
                var start = tmp.IndexOf("(", StringComparison.Ordinal);
                var end = tmp.IndexOf(",", StringComparison.Ordinal);

                if (start < end && start >= 0)
                {
                    start += 2;
                    end -= 1;
                    switchExpressionValue = tmp.Substring(start, (end - start));
                }
            }
            return switchExpressionValue;
        }

        void InitializeFlowStep(ModelItem mi)
        {
            // PBI 9135 - 2013.07.15 - TWR - Changed to "as" check so that database activity also flows through this
            var modelProperty1 = mi.Properties["Action"];
            InitialiseWithAction(modelProperty1);
        }

        void InitialiseWithAction(ModelProperty modelProperty1)
        {
            if (modelProperty1?.ComputedValue is DsfActivity droppedActivity)
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

        void InitialiseWithDataObject(DsfActivity droppedActivity)
        {
            if (DataObject != null)
            {
                if (DataObject is ExplorerItemViewModel viewModel)
                {
                    var serverRepository = CustomContainer.Get<IServerRepository>();
                    var server = serverRepository.FindSingle(c => c.EnvironmentID == viewModel.Server.EnvironmentID);
                    serverRepository.ActiveServer = server;
                    var theResource = server?.ResourceRepository.LoadContextualResourceModel(viewModel.ResourceId);

                    if (theResource != null)
                    {
                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(theResource, droppedActivity, true, serverRepository, _resourceModel.Environment.IsLocalHostCheck());
                        TrackAction(theResource);

                        UpdateForRemote(d, theResource);
                    }
                }
                DataObject = null;
            }
        }

        void TrackAction(IContextualResourceModel theResource)
        {
            if (_applicationTracker != null)
            {
                if (theResource.DisplayName == "Hello World")
                {
                    //track hello world dragged
                    _applicationTracker.TrackCustomEvent(Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.EventCategory,
                                       Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.HelloWorld, theResource.DisplayName);
                }
                else if (theResource.Category != null && theResource.Category.StartsWith("Examples"))
                {
                    //track examples actitvity dragged
                    _applicationTracker.TrackCustomEvent(Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.EventCategory,
                                        Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.Examples, theResource.DisplayName);
                }
                else
                {
                    // other than above
                    _applicationTracker.TrackCustomEvent(Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.EventCategory,
                                        Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.ItemDragged, theResource.DisplayName);
                }
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

        void InitialiseWithoutServiceName(ModelProperty modelProperty1, DsfActivity droppedActivity)
        {
            var activity = droppedActivity;
            var serverRepository = CustomContainer.Get<IServerRepository>();
            var server = CustomContainer.Get<IServerRepository>().ActiveServer;
            var resourceId = Guid.Parse(activity.ResourceID.Expression.ToString());
            var resource = server.ResourceRepository.LoadContextualResourceModel(resourceId);
            var displayName = resource != null ? resource.DisplayName : activity.DisplayName;

            droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false, serverRepository, _resourceModel.Environment.IsLocalHostCheck());
            WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(droppedActivity, resource, serverRepository.ActiveServer);
            modelProperty1.SetValue(droppedActivity);
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackCustomEvent(Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.EventCategory, Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.ItemDragged, displayName);
            }
        }

        void InitializeFlowSwitch(ModelItem mi)
        {
            // Travis.Frisinger : 28.01.2013 - Switch Amendments
            Dev2Logger.Info("Publish message of type - " + typeof(ConfigureSwitchExpressionMessage), "Warewolf Info");
            _expressionString = FlowController.ConfigureSwitchExpression(new ConfigureSwitchExpressionMessage { ModelItem = mi, Server = _resourceModel.Environment, IsNew = true, IsPaste = _isPaste });
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        void InitializeFlowDecision(ModelItem mi)
        {
            Dev2Logger.Info("Publish message of type - " + typeof(ConfigureDecisionExpressionMessage), "Warewolf Info");
            var modelProperty = mi.Properties["Action"];

            InitialiseWithAction(modelProperty);
            _expressionString = FlowController.ConfigureDecisionExpression(new ConfigureDecisionExpressionMessage { ModelItem = mi, Server = _resourceModel.Environment, IsNew = true, IsPaste = _isPaste });
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        void EditActivity(ModelItem modelItem, Guid parentEnvironmentID)
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

        static ModelItem RecursiveForEachCheck(dynamic activity)
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
        
        void PreventCommandFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {
            if (Designer?.Context != null)
            {
                var selection = Designer.Context.Items.GetValue<Selection>();

                if (selection?.PrimarySelection == null)
                {
                    return;
                }

                if (selection.PrimarySelection.ItemType != typeof(Flowchart) &&
                   selection.SelectedObjects.All(modelItem => modelItem.ItemType != typeof(Flowchart)))
                {
                    return;
                }
            }

            e.CanExecute = false;
            e.Handled = true;
        }
        
        void SetLastDroppedPoint(DragEventArgs e)
        {
            var senderAsFrameworkElement = _modelService.Root.View as FrameworkElement;
            UIElement freePormPanel = senderAsFrameworkElement?.FindNameAcrossNamescopes("flowchartPanel");
            if (freePormPanel != null)
            {
                e.GetPosition(freePormPanel);
            }
        }

        IList<IDataListVerifyPart> BuildWorkflowFields()
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

        IEnumerable<string> GetWorkflowFieldsFromModelItem(ModelItem flowNode)
        {
            var workflowFields = new List<string>();
            try
            {
                var modelProperty = flowNode.Properties["Action"];
                if (modelProperty != null)
                {
                    var activity = modelProperty.ComputedValue;
                    workflowFields = GetActivityElements(activity);
                }
                else
                {
                    var propertyName = string.Empty;
                    if (flowNode.ItemType.Name == "FlowDecision")
                    {
                        propertyName = "Condition";
                    }
                    else
                    {
                        if (flowNode.ItemType.Name == "FlowSwitch`1")
                        {
                            propertyName = "Expression";
                        }
                    }

                    var property = flowNode.Properties[propertyName];
                    if (property != null)
                    {
                        workflowFields = GetWorkflowFieldsFromProperty(workflowFields, property);
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, GlobalConstants.WarewolfError);
            }
            return workflowFields;
        }

        List<string> GetWorkflowFieldsFromProperty(List<string> workflowFields, ModelProperty property)
        {
            if (!string.IsNullOrEmpty(_expressionString))
            {
                workflowFields = TryGetDecisionElements(_expressionString, DataListSingleton.ActiveDataList);
                var activity = property.ComputedValue;
                if (activity != null)
                {
                    workflowFields.AddRange(TryGetDecisionElements(((dynamic)activity).ExpressionText, DataListSingleton.ActiveDataList));
                }
            }
            else
            {
                var activity = property.ComputedValue;
                if (activity != null)
                {
                    workflowFields.AddRange(TryGetDecisionElements(((dynamic)activity).ExpressionText, DataListSingleton.ActiveDataList));
                }
            }

            return workflowFields;
        }

        public static List<String> TryGetDecisionElements(string expression, IDataListViewModel datalistModel)
        {
            var decisionFields = new List<string>();
            if (!string.IsNullOrEmpty(expression))
            {
                var startIndex = expression.IndexOf('"');
                startIndex = startIndex + 1;
                var endindex = expression.IndexOf('"', startIndex);
                var decisionValue = expression.Substring(startIndex, endindex - startIndex);
                try
                {
                    decisionFields = GetDecisionElements(datalistModel, decisionFields, decisionValue);
                }
                catch (Exception)
                {
                    if (!DataListUtil.IsValueRecordset(decisionValue))
                    {
                        var parts = DataListFactory.CreateLanguageParser().ParseExpressionIntoParts(decisionValue, new List<IDev2DataLanguageIntellisensePart>());
                        decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                        return decisionFields;
                    }
                    if (DataListSingleton.ActiveDataList != null)
                    {
                        var parts = DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(decisionValue, DataListSingleton.ActiveDataList.WriteToResourceModel(), true);
                        decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                    }
                }
            }
            return decisionFields;
        }

        private static List<string> GetDecisionElements(IDataListViewModel datalistModel, List<string> decisionFields, string decisionValue)
        {
            var dds = JsonConvert.DeserializeObject<Dev2DecisionStack>(decisionValue.Replace('!', '\"'));
            foreach (var decision in dds.TheStack)
            {
                var getCols = new[] { decision.Col1, decision.Col2, decision.Col3 };
                for (var i = 0; i < 3; i++)
                {
                    decisionFields = GetDecisionFields(datalistModel, decisionFields, decisionValue, getCols, i);
                }
            }
            return decisionFields;
        }

        private static List<string> GetDecisionFields(IDataListViewModel datalistModel, List<string> decisionFields, string decisionValue, string[] getCols, int i)
        {
            var getCol = getCols[i];
            if (datalistModel != null)
            {
                var parsed = GetParsedRegions(getCol, datalistModel);
                if (!DataListUtil.IsValueRecordset(getCol) && parsed.Any(DataListUtil.IsValueRecordset))
                {
                    var parts = DataListFactory.CreateLanguageParser().ParseExpressionIntoParts(decisionValue, new List<IDev2DataLanguageIntellisensePart>());
                    decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                }
                else
                {
                    decisionFields = decisionFields.Union(GetParsedRegions(getCol, datalistModel)).ToList();
                }
            }

            return decisionFields;
        }

        static IEnumerable<string> GetParsedRegions(string getCol, IDataListViewModel datalistModel)
        {
            // Travis.Frisinger - 25.01.2013
            // We now need to parse this data for regions ;)

            var parser = DataListFactory.CreateLanguageParser();
            // NEED - DataList for active workflow
            var parts = parser.ParseDataLanguageForIntellisense(getCol, datalistModel.WriteToResourceModel(), true);

            return (from intellisenseResult in parts
                    select DataListUtil.StripBracketsFromValue(intellisenseResult.Option.DisplayValue)
                    into varWithNoBrackets
                    where !string.IsNullOrEmpty(getCol) && !varWithNoBrackets.Equals(getCol)
                    select getCol
                   ).ToList();
        }

        static List<String> GetActivityElements(object activity)
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
            var strategy = stratFac.CreateFindMissingStrategy(findMissingType);

            foreach (var activityField in strategy.GetActivityFields(activity))
            {
                if (!string.IsNullOrEmpty(activityField))
                {
                    var wdu = new WorkflowDesignerUtils();
                    activityFields.AddRange(wdu.FormatDsfActivityField(activityField).Where(item => !item.Contains("xpath(")));
                }
            }
            return activityFields;
        }

        void OnItemSelected(Selection item)
        {
            var primarySelection = item.PrimarySelection;
            NotifyItemSelected(primarySelection);
            primarySelection.SetProperty("IsSelected", true);
            SelectedItem = primarySelection;
        }

        public Action<ModelItem> ItemSelectedAction { get; set; }

        public void Handle(AddStringListToDataListMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            var dlvm = DataListSingleton.ActiveDataList;
            if (dlvm != null)
            {
                var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
                _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
                foreach (var s in message.ListToAdd)
                {
                    WorkflowDesignerDataPartUtils.BuildDataPart(s, _uniqueWorkflowParts);
                }
                var partsToAdd = _uniqueWorkflowParts.Keys.ToList();
                var uniqueDataListPartsToAdd = dlvm.MissingDataListParts(partsToAdd);
                dlvm.AddMissingDataListItems(uniqueDataListPartsToAdd);
            }
        }

        public bool NotifyItemSelected(object primarySelection) => false;

        public void BindToModel() => _resourceModel.WorkflowXaml = ServiceDefinition;

        public void InitializeDesigner(IDictionary<Type, Type> designerAttributes) => InitializeDesigner(designerAttributes, false);

        public void InitializeDesigner(IDictionary<Type, Type> designerAttributes, bool liteInit)
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
                _wd.Context.Services.Publish(_designerManagementService);

                _wd.View.Measure(new Size(2000, 2000));
                _wd.View.PreviewDrop += ViewPreviewDrop;
                _wd.View.PreviewMouseDown += ViewPreviewMouseDown;
                _wd.View.PreviewKeyDown += ViewOnKeyDown;
                _wd.View.LostFocus += OnViewOnLostFocus;

                //Jurie.Smit 2013/01/03 - Added to disable the deleting of the root flowchart
                CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
                _wd.ModelChanged += WdOnModelChanged;
                _wd.View.Focus();

                var indexOfOpenItem = -1;
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
                _workflowHelper.EnsureImplementation(_modelService);

                WorkflowDesignerIcons.Activities.Flowchart = Application.Current.TryFindResource("Explorer-WorkflowService-Icon") as DrawingBrush;
                WorkflowDesignerIcons.Activities.StartNode = Application.Current.TryFindResource("System-StartNode-Icon") as DrawingBrush;
                SubscribeToDebugSelectionChanged();
                SetPermission(ResourceModel.UserPermissions);
                ViewModelUtils.RaiseCanExecuteChanged(_debugOutputViewModel?.AddNewTestCommand);
                UpdateErrorIconWithCorrectMessage();
            }
        }

        public void CreateDesigner() => CreateDesigner(false);

        public void CreateDesigner(bool liteInit)
        {
            _wd = new WorkflowDesigner();

            if (!liteInit)
            {
                SetHashTable();
                SetDesignerConfigService();

                _wdMeta = new DesignerMetadata();
                _wdMeta.Register();
                var builder = new AttributeTableBuilder();
                foreach (var designerAttribute in ActivityDesignerHelper.DesignerAttributes)
                {
                    builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
                }

                MetadataStore.AddAttributeTable(builder.CreateTable());

                _wd.Context.Items.Subscribe<Selection>(OnItemSelected);
                _wd.Context.Services.Subscribe<ModelService>(ModelServiceSubscribe);
                _wd.Context.Services.Subscribe<DesignerView>(DesigenrViewSubscribe);
                _wd.Context.Services.Publish(_designerManagementService);

                _wd.View.Measure(new Size(2000, 2000));
                _wd.View.PreviewDrop += ViewPreviewDrop;
                _wd.View.PreviewMouseDown += ViewPreviewMouseDown;
                _wd.View.PreviewKeyDown += ViewOnKeyDown;
                _wd.View.LostFocus += OnViewOnLostFocus;

                //Jurie.Smit 2013/01/03 - Added to disable the deleting of the root flowchart
                CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
                _wd.ModelChanged += WdOnModelChanged;
                _wd.View.Focus();

                var indexOfOpenItem = -1;
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
                WorkflowDesignerIcons.Activities.Flowchart = Application.Current.TryFindResource("Explorer-WorkflowService-Icon") as DrawingBrush;
                WorkflowDesignerIcons.Activities.StartNode = Application.Current.TryFindResource("System-StartNode-Icon") as DrawingBrush;
                SubscribeToDebugSelectionChanged();
                SetPermission(ResourceModel.UserPermissions);
                ViewModelUtils.RaiseCanExecuteChanged(_debugOutputViewModel?.AddNewTestCommand);
                UpdateErrorIconWithCorrectMessage();
            }
        }

        void SetHashTable() => _wd.PropertyInspectorFontAndColorData = XamlServices.Save(ActivityDesignerHelper.GetDesignerHashTable());

        void SetDesignerConfigService()
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
        
        static void ViewOnKeyDown(object sender, KeyEventArgs e)
        {
            var grid = sender as Grid;
            if (e.OriginalSource != null)
            {
                var origSource = e.OriginalSource.GetType();
                if (origSource.BaseType == typeof(ActivityDesigner) && e.Key == Key.Return)
                {
                    e.Handled = true;
                }

                var type = grid?.DataContext.GetType();
                if (type == typeof(ServiceTestViewModel) && e.Key == Key.Delete)
                {
                    e.Handled = true;
                }

                if (type == typeof(MergeWorkflowViewModel))
                {
                    if (origSource == typeof(TextBox))
                    {
                        return;
                    }
                    if (e.Key == Key.Delete)
                    {
                        e.Handled = true;
                        return;
                    }
                    var isControlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                    var isExpectedKey = e.Key == Key.C || e.Key == Key.V;
                    isExpectedKey |= e.Key == Key.X || e.Key == Key.Y || e.Key == Key.Z;
                    if (isControlPressed && isExpectedKey)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        static void DesigenrViewSubscribe(DesignerView instance)
        {
            // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
            instance.WorkflowShellHeaderItemsVisibility = ShellHeaderItemsVisibility.ExpandAll;
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.None;
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom | ShellBarItemVisibility.PanMode | ShellBarItemVisibility.MiniMap;
        }
        
        void OnViewOnLostFocus(object sender, RoutedEventArgs args)
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

        protected void ModelServiceSubscribe(ModelService instance)
        {
            _modelService = instance;
            _modelService.ModelChanged += ModelServiceModelChanged;
            if (_activityCollection == null)
            {
                _activityCollection = _modelService.Find(_modelService.Root, typeof(Activity));
            }
            if (_modelItems == null)
            {
                _modelItems = _modelService.Find(_modelService.Root, typeof(IDev2Activity));
            }
        }

        void SubscribeToDebugSelectionChanged()
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
                    var selectedModelItem = GetSelectedModelItem(workSurfaceMappingId, debugState.ParentID.GetValueOrDefault());
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

                            case ActivitySelectionType.None:
                                break;

                            default:
                                break;
                        }
                    }
                }
            });
        }

        public bool IsTestView { get; set; }

        protected virtual ModelItem GetSelectedModelItem(Guid itemId, Guid parentId)
        {
            if (_modelService != null)
            {
                var selectedModelItem = (from mi in _modelItems
                                         let instanceID = ModelItemUtils.GetUniqueID(mi)
                                         where instanceID == itemId || instanceID == parentId
                                         select mi).FirstOrDefault();

                if (selectedModelItem == null)
                {
                    // Find the root flow chart
                    selectedModelItem = _modelService.Find(_modelService.Root, typeof(Flowchart)).FirstOrDefault();
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

        void SelectSingleModelItem(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                return;
            }
            Selection.SelectOnly(_wd.Context, selectedModelItem);
            SelectedDebugItems.Add(selectedModelItem);
        }

        void RemoveModelItemFromSelection(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                SelectedDebugItems.Remove(selectedModelItem);
            }
            Selection.Unsubscribe(_wd.Context, SelectedItemChanged);
        }

        public List<ModelItem> DebugModels => SelectedDebugItems;

        void AddModelItemToSelection(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                return;
            }
            Selection.Union(_wd.Context, selectedModelItem);

            var modelItems = _activityCollection as ModelItem[] ?? _activityCollection.ToArray();
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

        void ClearSelection()
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
            if (selectedModelItem.View is FrameworkElement view && view.IsVisible)
            {
                BringIntoView(view);
                return;
            }

            var onAfterPopulateAll = new System.Action(() => BringIntoView(selectedModelItem.View as FrameworkElement));
            _virtualizedContainerServicePopulateAllMethod?.Invoke(_virtualizedContainerService, new object[] { onAfterPopulateAll });
        }

        public void BringMergeToView(DataTemplate selectedDataTemplate)
        {
            var dependencyObject = selectedDataTemplate.LoadContent();
            var frameworkElement = dependencyObject as FrameworkElement;
            BringIntoView(frameworkElement);
        }

        static void BringIntoView(FrameworkElement view) => Application.Current?.Dispatcher?.InvokeAsync(() => view?.BringIntoView(), DispatcherPriority.Background);

        protected void LoadDesignerXaml()
        {
            var xaml = _resourceModel.WorkflowXaml;

            // if null, try fetching. It appears there is more than the two routes identified to populating xaml ;(
            if (xaml == null || xaml.Length == 0)
            {
                // we always want server at this point ;)
                var workspace = GlobalConstants.ServerWorkspaceID;

                // log the trace for fetch ;)
                Dev2Logger.Info($"Null Definition For {_resourceModel.ID} :: {_resourceModel.ResourceName}. Fetching...", "Warewolf Info");

                // In the case of null of empty try fetching again ;)
                var msg = Server.ResourceRepository.FetchResourceDefinition(_resourceModel.Environment, workspace, _resourceModel.ID, false);
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
                    CreateBlankWorkflow();
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

        public void CreateBlankWorkflow()
        {
            CreateDesigner();
            var activityBuilder = _workflowHelper.CreateWorkflow(_resourceModel.ResourceName);
            _wd.Load(activityBuilder);
            BindToModel();
            _workflowHelper.EnsureImplementation(_modelService);
        }

        void SetDesignerText(StringBuilder xaml)
        {
            var designerText = _workflowHelper.SanitizeXaml(xaml);
            if (designerText != null)
            {
                _wd.Text = designerText.ToString();
            }
        }

        void SelectedItemChanged(Selection item)
        {
            if (_wd?.Context != null)
            {
                var contextItemManager = _wd.Context.Items;
                var selection = contextItemManager.GetValue<Selection>();
                if (selection.SelectedObjects.Count() > 1)
                {
                    DeselectFlowchart();
                }
            }
        }

        void DeselectFlowchart()
        {
            if (_wd?.Context != null)
            {
                var editingContext = _wd.Context;
                var selection = editingContext.Items.GetValue<Selection>();
                foreach (var item in selection.SelectedObjects.Where(item => item.ItemType == typeof(Flowchart)))
                {
                    Selection.Toggle(editingContext, item);
                    break;
                }
            }
        }

        public List<NameValue> GetSelectableGates(string uniqueId)
        {
            var serviceDifferenceParser = CustomContainer.Get<IServiceDifferenceParser>();
            var treeNodes = serviceDifferenceParser.BuildWorkflow(ServiceDefinition);

            var list = new List<NameValue> { new NameValue { Name = "End", Value = Guid.Empty.ToString() } };
            try
            {
                IEnumerable<IDev2Activity> connectedList(IDev2Activity activity)
                {
                    var ret = new List<IDev2Activity>();
                    ret.Add(activity);
                    if (activity.NextNodes is null)
                    {
                        return ret;
                    }

                    foreach (var nextActivity in activity.NextNodes)
                    {
                        ret.AddRange(connectedList(nextActivity));
                    }
                    return ret.Where(o => (o is GateActivity));
                }

                bool found = false;
                var allGates = connectedList(treeNodes[0].Activity)
                    .Cast<GateActivity>()
                    .Where(gate => gate?.GateOptions != null && gate.GateOptions.GateOpts is Continue);

                var selectableGates = allGates
                    .TakeWhile(gate => !(found = (gate.UniqueID == uniqueId)));

                foreach (var gate in selectableGates)
                {
                    var id = gate.UniqueID;
                    var activityName = gate.GetDisplayName();
                    var nameValue = new NameValue { Name = activityName, Value = id };
                    list.Add(nameValue);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Error loading selectable gates. Exception: " + ex.Message, GlobalConstants.ServerWorkspaceID.ToString());
            }
            return list;
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
                    if (ResourceModel.WorkflowXaml is null)
                    {
                        ResourceModel.WorkflowXaml = ServiceDefinition;
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

        void UpdateErrorIconWithCorrectMessage()
        {
            var validationIcon = DesignerView?.FindChild<Border>(border => border.Name.Equals("validationVisuals", StringComparison.CurrentCultureIgnoreCase));
            if (validationIcon != null && validationIcon.Name.Equals("validationVisuals", StringComparison.CurrentCultureIgnoreCase))
            {
                validationIcon.ToolTip = Warewolf.Studio.Resources.Languages.Tooltips.StartNodeNotConnectedToolTip;

                //It should be called once when there is first tool dragged or start node link get deleted
                if (!IsStartNodeErrorMessageSet)
                {
                    IsStartNodeErrorMessageSet = true;
                    if (_applicationTracker != null)
                    {
                        _applicationTracker.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.EventCategory,
                                                        Warewolf.Studio.Resources.Languages.TrackEventWorkflowTabs.StartNodeNotConnected);
                    }
                }
            }
        }

        bool CheckDataList()
        {
            if (_originalDataList == null)
            {
                return true;
            }

            if (ResourceModel.DataList != null)
            {
                var currentDataList = ResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "");
                return StringExtension.SpaceCaseInsenstiveComparision(currentDataList, _originalDataList);
            }
            return true;
        }
        string _serviceDefinitionXamlCache = "";
        string _resourceDefinitionXamlCache = "";
        bool _serviceAndResourceDefinitionXamlSameCache;
        bool CheckServiceDefinition()
        {
            if (ServiceDefinition is null || ResourceModel.WorkflowXaml is null)
            {
                return ServiceDefinition == ResourceModel.WorkflowXaml;
            }
            var serviceDefinitionXaml = ServiceDefinition.ToString();
            var resourceDefinitionXaml = ResourceModel.WorkflowXaml.ToString();
            if (serviceDefinitionXaml == _serviceDefinitionXamlCache && resourceDefinitionXaml == _resourceDefinitionXamlCache)
            {
                return _serviceAndResourceDefinitionXamlSameCache;
            }

            _serviceDefinitionXamlCache = serviceDefinitionXaml;
            _resourceDefinitionXamlCache = resourceDefinitionXaml;

            var eq = WorkflowHelper.AreWorkflowsEqual(ServiceDefinition.ToString(), ResourceModel.WorkflowXaml.ToString());
            _serviceAndResourceDefinitionXamlSameCache = eq;
            return eq;
        }
        
        void ProcessDataListOnLoad()
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

        void SaveToWorkspace()
        {
            BindToModel();
            ResourceModel.Environment.ResourceRepository.Save(ResourceModel);
            _workspaceSave = true;
        }
        
        public void UpdateDataList()
        {
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        public static bool ValidatResourceModel(string dataList)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataList))
                {
                    XElement.Parse(dataList);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void AddMissingWithNoPopUpAndFindUnusedDataListItems()
        {
            UpdateDataList();
        }

        public ModelItem GetModelItem(Guid workSurfaceMappingId, Guid parentID)
        {
            var modelItems = _modelService.Find(_modelService.Root, typeof(IDev2Activity));
            ModelItem selectedModelItem = null;
            foreach (var mi in modelItems)
            {
                var instanceID = ModelItemUtils.GetUniqueID(mi);
                if (instanceID == workSurfaceMappingId || instanceID == parentID)
                {
                    selectedModelItem = mi;
                    break;
                }
            }
            return selectedModelItem;
        }

        void AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(bool isLoadEvent)
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

        void UpdateDataListWithMissingParts(bool isLoadEvent)
        {
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);
            if (OpeningWorkflowsHelper.IsWorkflowWaitingforDesignerLoad(workSurfaceKey) && !isLoadEvent)
            {
                OpeningWorkflowsHelper.RemoveWorkflowWaitingForDesignerLoad(workSurfaceKey);
            }

            var workflowFields = BuildWorkflowFields();
            DataListViewModel?.UpdateDataListItems(ResourceModel, workflowFields);
        }

        void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e) => e.Handled = HandleMouseClick(e.LeftButton, e.ClickCount, e.OriginalSource as DependencyObject, e.Source as DesignerView);
        
        bool HandleMouseClick(MouseButtonState leftButtonState, int clickCount, DependencyObject dp, DesignerView designerView)
        {
            if (HandleDoubleClick(leftButtonState, clickCount, dp, designerView))
            {
                return true;
            }

            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as ShellViewModel;
                if (mvm?.ActiveItem != null)
                {
                    mvm.RefreshActiveServer();
                }
            }

            if (dp is Border border && border.DataContext is GateDesignerViewModel gateDesignerViewModel)
            {
                gateDesignerViewModel.ClearGates();
                string uniqueId = gateDesignerViewModel.ModelItem.Properties["UniqueID"].ComputedValue.ToString();
                var gates = GetSelectableGates(uniqueId);
                gateDesignerViewModel.Gates = gates;
            }

            var dp1 = dp as Run;
            if (dp1?.Parent is TextBlock && dp1.DataContext.GetType().Name.Contains("FlowchartDesigner"))
            {
                var selectedModelItem = _modelService.Find(_modelService.Root, typeof(Flowchart)).FirstOrDefault();
                if (selectedModelItem != null)
                {
                    SelectSingleModelItem(selectedModelItem);
                }
                return true;
            }

            if (dp is TextBlock dp2 && dp2.DataContext.GetType().Name.Contains("FlowchartDesigner"))
            {
                var selectedModelItem = _modelService.Find(_modelService.Root, typeof(Flowchart)).FirstOrDefault();
                if (selectedModelItem != null)
                {
                    SelectSingleModelItem(selectedModelItem);
                }
                return true;
            }

            return false;
        }
        
        bool HandleDoubleClick(MouseButtonState leftButtonState, int clickCount, DependencyObject dp, DesignerView designerView)
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
        
        void HandleDependencyObject(DependencyObject dp, ModelItem item)
        {
            if (item != null)
            {
                var itemFn = item.ItemType.FullName;

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
                        FlowController.TryEditSwitchCaseExpression(new EditCaseExpressionMessage
                        {
                            ModelItem = item,
                            Server = _resourceModel.Environment
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
                            Server = _resourceModel.Environment
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
                            Server = _resourceModel.Environment
                        });
                    AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
                }
            }
        }
        
        static IResourcePickerDialog CreateResourcePickerDialog(enDsfActivityType activityType)
        {
            var server = CustomContainer.Get<IServerRepository>().ActiveServer;

            if (server.Permissions == null)
            {
                server.Permissions = new List<IWindowsGroupPermission>();
                server.Permissions.AddRange(server.AuthorizationService.SecurityService.Permissions);
            }
            var env = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);
            var res = new ResourcePickerDialog(activityType, env);
            ResourcePickerDialog.CreateAsync(activityType, env);
            return res;
        }
        
        void ViewPreviewDrop(object sender, DragEventArgs e)
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

                if (dataObject.GetData("WorkflowItemTypeNameFormat") is string isWorkflow)
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

        static void ApplyIsDraggedInstance(string isWorkflow)
        {
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("DsfSqlServerDatabaseActivity") || isWorkflow.Contains("DsfMySqlDatabaseActivity");
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("DsfODBCDatabaseActivity") || isWorkflow.Contains("DsfOracleDatabaseActivity");
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("DsfPostgreSqlActivity") || isWorkflow.Contains("DsfWebDeleteActivity");
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("DsfWebGetActivity") || isWorkflow.Contains("WebPostActivity");
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("DsfWebPutActivity") || isWorkflow.Contains("DsfComDllActivity");
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("DsfEnhancedDotNetDllActivity") || isWorkflow.Contains("DsfWcfEndPointActivity");
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("AdvancedRecordsetActivity");
            IsItemDragged.Instance.IsDragged |= isWorkflow.Contains("GateActivity");
        }
        
        bool WorkflowDropFromResourceToolboxItem(IDataObject dataObject, string isWorkflow, bool dropOccured, bool handled)
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

        string _originalDataList;
        bool _workspaceSave;
        WorkflowInputDataViewModel _workflowInputDataViewModel;
        string _workflowLink;
        string _workflowLinkWithWid;
        ICommand _openWorkflowLinkCommand;
        bool _firstWorkflowChange;
        readonly IAsyncWorker _asyncWorker;
        string _expressionString;
        ICommand _debugInputsCommand;
        ICommand _debugStudioCommand;
        ICommand _debugBrowserCommand;
        ICommand _scheduleCommand;
        ICommand _queueEventCommand;
        ICommand _testEditorCommand;
        ICommand _runAllTestsCommand;
        ICommand _duplicateCommand;
        ICommand _deployCommand;
        ICommand _showDependenciesCommand;
        ICommand _viewSwaggerCommand;
        ICommand _copyUrlCommand;
        DebugOutputViewModel _debugOutputViewModel;
        IDataListViewModel _dataListViewModel;
        bool _canDebugInputs;
        bool _canDebugStudio;
        bool _debugBrowser;
        bool _canCreateSchedule;
        bool _canCreateQueueEvent;
        bool _canCreateTest;
        bool _canRunAllTests;
        bool _canDuplicate;
        bool _canDeploy;
        bool _canShowDependencies;
        bool _canViewSwagger;
        bool _canCopyUrl;
        string _copyUrlTooltip;
        string _viewSwaggerTooltip;
        string _debugInputsTooltip;
        string _debugStudioTooltip;
        string _debugBrowserTooltip;
        string _scheduleTooltip;
        string _queueEventTooltip;
        string _createTestTooltip;
        string _runAllTestsTooltip;
        string _runCoverageTooltip;
        string _duplicateTooltip;
        string _deployTooltip;
        string _showDependenciesTooltip;
        ICommand _newServiceCommand;
        ModelItem _selectedItem;
        IEnumerable<ModelItem> _modelItems;
        IEnumerable<ModelItem> _activityCollection;
        ICommand _mergeCommand;
        bool _canMerge;
        string _mergeTooltip;
       
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
                    if (e.ModelChangeInfo.OldValue != null)
                    {
                        // incase of delete it will have old value then log
                        IsStartNodeErrorMessageSet = false;
                    }
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
                PerformAddItems(e.ModelChangeInfo.Value);
            }

            if (e.ModelChangeInfo != null && e.ModelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged
                && (e.ModelChangeInfo.Value?.Source?.ComputedValue?.GetType() == typeof(DsfFlowDecisionActivity)
                || e.ModelChangeInfo.Value?.Source?.ComputedValue?.GetType() == typeof(DsfFlowSwitchActivity)))
            {
                PerformAddItems(e.ModelChangeInfo.Value);
            }
            WorkflowChanged?.Invoke();
        }

        void ModelItemPropertyChanged(ModelChangedEventArgs e)
        {
            Guid? envID = null;
            Guid? resourceID = null;
            if (DataObject is IExplorerItemViewModel explorerItem)
            {
                if (explorerItem.Server != null)
                {
                    envID = explorerItem.Server.EnvironmentID;
                }

                resourceID = explorerItem.ResourceId;
            }

            var modelProperty = e.ModelChangeInfo.Subject.Content;

            if (envID != null && modelProperty != null)
            {
                var server = CustomContainer.Get<IServerRepository>().FindSingle(c => c.EnvironmentID == envID);
                var resource = server?.ResourceRepository.LoadContextualResourceModel(resourceID.Value);
                if (resource != null)
                {
                    var d = DsfActivityFactory.CreateDsfActivity(resource, null, true, CustomContainer.Get<IServerRepository>(), _resourceModel.Environment.IsLocalHostCheck());
                    d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.Category;
                    UpdateForRemote(d, resource);
                    modelProperty.SetValue(d);
                }
            }
            DataObject = null;
            WorkflowChanged?.Invoke();
        }

        void UpdateForRemote(DsfActivity d, IContextualResourceModel resource)
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

        protected IServer ActiveEnvironment { get; set; }

        void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command.Equals(ApplicationCommands.Delete) ||      //triggered from deleting an activity
                e.Command.Equals(EditingCommands.Delete) ||          //triggered from editing displayname, expressions, etc
                e.Command.Equals(System.Activities.Presentation.View.DesignerView.CopyCommand) ||
                e.Command.Equals(System.Activities.Presentation.View.DesignerView.CutCommand))
            {
                PreventCommandFromBeingExecuted(e);
            }
        }

        void PreviewExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command.Equals(ApplicationCommands.Delete))
            {
                _wd?.View?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
            
            if (!Handle(e))
            {
                BuildWorkflowFields();
            }
        }

        bool Handle(ExecutedRoutedEventArgs e)
        {
            var Handled = false;
            if (e.Command.Equals(System.Activities.Presentation.View.DesignerView.PasteCommand))
            {
                _isPaste = true;
                var dataObject = Clipboard.GetDataObject();
                if (dataObject != null)
                {
                    var dataPresent = dataObject.GetDataPresent("WorkflowXamlFormat");
                    if (dataPresent)
                    {
                        Handled = Handle(e, dataObject);
                    }
                }
            }
            return Handled;
        }

        bool Handle(ExecutedRoutedEventArgs e, IDataObject dataObject)
        {
            var data = dataObject.GetData("WorkflowXamlFormat") as string;
            if (!string.IsNullOrEmpty(data))
            {
                var indexOf = data.IndexOf("ResourceID=", StringComparison.InvariantCultureIgnoreCase);
                var guid = data.Substring(indexOf + 12, 36);
                if (guid.Equals(ResourceModel.ID.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    e.Handled = true;
                }
            }
            return e.Handled;
        }

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

            _designerManagementService?.Dispose();
            _debugSelectionChangedService?.Unsubscribe();

            if (_resourceModel != null)
            {
                _resourceModel.OnDataListChanged -= FireWdChanged;
                _resourceModel.OnResourceSaved -= UpdateOriginalDataList;
            }

            if (_modelService != null)
            {
                _modelService.ModelChanged -= ModelServiceModelChanged;
            }

            if (_uniqueWorkflowParts != null)
            {
                _uniqueWorkflowParts.Clear();
                _uniqueWorkflowParts = null;
            }
            
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
            catch (Exception e)
            {
                Dev2Logger.Warn("Error disposing Workflow Designer View Model: " + e.Message, GlobalConstants.WarewolfWarn);
            }

            _debugSelectionChangedService?.Unsubscribe();
            base.OnDispose();
        }

        public override WorkSurfaceContext WorkSurfaceContext => ResourceModel?.ResourceType.ToWorkSurfaceContext() ?? WorkSurfaceContext.Unknown;

        public IServer Server => ResourceModel.Environment;

        protected List<ModelItem> SelectedDebugItems => _selectedDebugItems;

        public ModelItem SelectedItem
        {
            get => _selectedItem;
            set => _selectedItem = value;
        }

        public bool WorkspaceSave => _workspaceSave;

        public void Handle(EditActivityMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            EditActivity(message.ModelItem, message.ParentEnvironmentID);
        }

        void FireWdChanged()
        {
            WdOnModelChanged(new object(), new EventArgs());
            UpdateWorkflowLink();
        }

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

            if (message.KeepTabOpen)
            {
                ActivityDesignerHelper.AddDesignerAttributes(this);
                UpdateWorkflowInputDataViewModel(_resourceModel);

                DisplayWorkflowLink = GetAndUpdateWorkflowLinkWithWorkspaceID();
                NotifyOfPropertyChange(nameof(DisplayWorkflowLink));
                NotifyOfPropertyChange(nameof(DesignerView));
            }
            RemoveUnsavedWorkflowName(unsavedName);
        }

        public void UpdateWorkflowInputDataViewModel(IContextualResourceModel resourceModel)
        {
            _workflowInputDataViewModel = WorkflowInputDataViewModel.Create(_resourceModel);
            _workflowInputDataViewModel.LoadWorkflowInputs();
        }

        internal void RemoveUnsavedWorkflowName(string unsavedName) => NewWorkflowNames.Instance.Remove(unsavedName);
        internal void RemoveAllWorkflowName(string unsavedName) => NewWorkflowNames.Instance.RemoveAll(unsavedName);

        void DisposeDesigner()
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

            _designerManagementService?.Dispose();
            if (_modelService != null)
            {
                _modelService.ModelChanged -= ModelServiceModelChanged;
            }
            _debugSelectionChangedService?.Unsubscribe();
        }

        void PublishMessages(IContextualResourceModel resourceModel)
        {
            UpdateResource(resourceModel);
            Dev2Logger.Info("Publish message of type - " + typeof(UpdateResourceMessage), "Warewolf Info");
            EventPublisher.Publish(new UpdateResourceMessage(resourceModel));
        }

        void UpdateResource(IContextualResourceModel resourceModel)
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

        void UpdateResourceModel(SaveUnsavedWorkflowMessage message, IContextualResourceModel resourceModel, string unsavedName)
        {
            resourceModel.ResourceName = message.ResourceName;
            resourceModel.DisplayName = message.ResourceName;
            resourceModel.Category = message.ResourceCategory;
            resourceModel.WorkflowXaml = ServiceDefinition?.Replace(unsavedName, message.ResourceName);
            resourceModel.IsNewWorkflow = false;
            var saveResult = resourceModel.Environment.ResourceRepository.SaveToServer(resourceModel);
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            var environmentViewModel = mainViewModel?.ExplorerViewModel?.Environments.FirstOrDefault(model => model.Server.EnvironmentID == resourceModel.Environment.EnvironmentID);
            if (environmentViewModel != null)
            {
                var item = environmentViewModel.FindByPath(resourceModel.GetSavePath());
                var savedItem = environmentViewModel?.CreateExplorerItemFromResource(environmentViewModel.Server, item, false, false, resourceModel);
                item.AddChild(savedItem);
            }
            resourceModel.IsWorkflowSaved = true;
            DeleteOldResourceAfterSucessfulSave(message, saveResult);
        }

        public void DeleteOldResourceAfterSucessfulSave(SaveUnsavedWorkflowMessage message, ExecuteMessage saveResult)
        {
            if (!saveResult.HasError
                && saveResult.Message.Contains("Added")
                && !message.ResourceLoadingFromServer
                && !string.IsNullOrEmpty(message.OriginalPath))
            {
                try
                {
                    File.Delete(message.OriginalPath);
                }
                catch (Exception)
                {
                    Dev2Logger.Error("Resource from " + message.OriginalPath + " could not be Deleted", "Warewolf Error");
                }
            }
        }

        protected bool _isPaste;
        private IShellViewModel _shellViewModel;

        public System.Action WorkflowChanged { get; set; }
    }
}