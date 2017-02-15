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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Messages;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Messages;
using DelegateCommand = Dev2.Runtime.Configuration.ViewModels.Base.DelegateCommand;
// ReSharper disable InconsistentNaming
// ReSharper disable NonLocalizedString

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Diagnostics
// ReSharper restore CheckNamespace
{
    /// <summary>
    ///     This is the view-model of the UI.  It provides a data source
    ///     for the TreeView (the RootItems property), a bindable
    ///     SearchText property, and the SearchCommand to perform a search.
    /// </summary>
    public class DebugOutputViewModel : SimpleBaseViewModel, IUpdatesHelp
    {
        readonly List<IDebugState> _pendingItems = new List<IDebugState>();
        readonly List<IDebugState> _contentItems;
        readonly Dictionary<Guid, IDebugTreeViewItemViewModel> _contentItemMap;
        readonly IDebugOutputFilterStrategy _debugOutputFilterStrategy;
        private readonly IContextualResourceModel _contextualResourceModel;
        readonly SubscriptionService<DebugWriterWriteMessage> _debugWriterSubscriptionService;
        readonly IEnvironmentRepository _environmentRepository;
        readonly object _syncContext = new object();
        ObservableCollection<IDebugTreeViewItemViewModel> _rootItems;
        readonly IPopupController _popup;
        private readonly IDebugOutputViewModelUtil _outputViewModelUtil;

        IDebugState _lastStep;
        DebugStatus _debugStatus;
        ICommand _expandAllCommand;
        ICommand _openItemCommand;
        ICommand _selectAllCommand;
        ICommand _showOptionsCommand;

        int _depthMin;
        int _depthMax;
        string _searchText = string.Empty;
        bool _expandAllMode = true;
        bool _highlightError = true;
        bool _showDebugStatus = true;
        bool _showDuration = true;
        bool _showInputs = true;
        bool _showOutputs = true;
        bool _showAssertResult = true;
        bool _showServer = true;
        bool _showTime = true;
        bool _showType = true;
        bool _showOptions;
        bool _showVersion;
        bool _allDebugReceived;
        bool _isRebuildingTree;
        bool _skipOptionsCommandExecute;
        bool _continueDebugDispatch;
        bool _dispatchLastDebugState;
        private string _addNewTestTooltip;

        public DebugOutputViewModel(IEventPublisher serverEventPublisher, IEnvironmentRepository environmentRepository, IDebugOutputFilterStrategy debugOutputFilterStrategy, IContextualResourceModel contextualResourceModel = null)
        {
            VerifyArgument.IsNotNull("serverEventPublisher", serverEventPublisher);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("debugOutputFilterStrategy", debugOutputFilterStrategy);
            _environmentRepository = environmentRepository;
            _debugOutputFilterStrategy = debugOutputFilterStrategy;
            if (contextualResourceModel != null)
            {
                _contextualResourceModel = contextualResourceModel;
                ResourceID = _contextualResourceModel.ID;
            }
            IsTestView = false;
            _contentItems = new List<IDebugState>();
            _contentItemMap = new Dictionary<Guid, IDebugTreeViewItemViewModel>();
            _debugWriterSubscriptionService = new SubscriptionService<DebugWriterWriteMessage>(serverEventPublisher);
            _debugWriterSubscriptionService.Subscribe(msg =>
            {
                Append(msg.DebugState);                
            });

            SessionID = Guid.NewGuid();
            _popup = CustomContainer.Get<IPopupController>();
            ClearSearchTextCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => SearchText = "");
            AddNewTestCommand = new DelegateCommand(o => AddNewTest(EventPublishers.Aggregator), o=> CanAddNewTest());
            _outputViewModelUtil = new DebugOutputViewModelUtil(SessionID);
        }

        public bool IsTestView { get; set; }

        private void AddNewTest(IEventAggregator eventPublisher)
        {
            var newTestFromDebugMessage = new NewTestFromDebugMessage
            {
                ResourceID = ResourceID,
                ResourceModel = _contextualResourceModel,
                RootItems = RootItems.ToList()
            };
            eventPublisher.Publish(newTestFromDebugMessage);
        }

        public Guid ResourceID { get; set; }

        private bool CanAddNewTest()
        {
            var canAddNewTest = RootItems != null && RootItems.Count > 0;

            if (canAddNewTest && !IsTestView)
            {
                if (_contextualResourceModel != null)
                {
                    canAddNewTest = !_contextualResourceModel.IsNewWorkflow && _contextualResourceModel.IsWorkflowSaved;
                }
            }
            AddNewTestTooltip = canAddNewTest ? Warewolf.Studio.Resources.Languages.Tooltips.DebugOutputViewAddNewTestToolTip : Warewolf.Studio.Resources.Languages.Tooltips.DebugOutputViewAddNewTestUnsavedToolTip;

            return canAddNewTest;
        }

        public int PendingItemCount => _pendingItems.Count;
        public int ContentItemCount => _contentItems.Count;

        public DebugStatus DebugStatus
        {
            get { return _debugStatus; }
            set
            {
                _debugStatus = value;

                if (value == DebugStatus.Executing)
                {
                    _allDebugReceived = false;
                    ClearSelection();
                }

                NotifyOfPropertyChange(() => IsStopping);
                NotifyOfPropertyChange(() => IsProcessing);
                NotifyOfPropertyChange(() => IsConfiguring);
                NotifyOfPropertyChange(() => DebugImage);
                NotifyOfPropertyChange(() => DebugText);
                NotifyOfPropertyChange(() => ProcessingText);
            }
        }


        /// <summary>
        ///     Gets or sets the processing text.
        /// </summary>
        /// <value>
        ///     The processing text.
        /// </value>
        /// <author>Massimo.Guerrera</author>
        /// <date>3/4/2013</date>
        public string ProcessingText => DebugStatus.GetDescription();

        public bool IsProcessing => _debugStatus != DebugStatus.Ready && _debugStatus != DebugStatus.Finished &&
                                    _debugStatus != DebugStatus.Stopping;

        public bool IsStopping => _debugStatus == DebugStatus.Stopping;

        public string DebugImage => IsProcessing ? StringResources.Pack_Uri_Stop_Image : StringResources.Pack_Uri_Debug_Image;

        public string DebugText => IsProcessing ? StringResources.Ribbon_StopExecution : StringResources.Ribbon_Debug;

        /// <summary>
        ///     Gets or sets the environment repository, this property is imported via MEF.
        /// </summary>
        /// <value>
        ///     The environment repository.
        /// </value>
        public IEnvironmentRepository EnvironmentRepository => _environmentRepository;

        public int DepthMin
        {
            get { return _depthMin; }
            set
            {
                if (_depthMin != value)
                {
                    _depthMin = value;
                    NotifyOfPropertyChange(() => DepthMin);
                }
            }
        }

        public int DepthMax
        {
            get { return _depthMax; }
            set
            {
                if (_depthMax != value)
                {
                    _depthMax = value;
                    NotifyOfPropertyChange(() => DepthMax);
                }
            }
        }

        public string AddNewTestTooltip
        {
            get { return _addNewTestTooltip; }
            set
            {
                _addNewTestTooltip = value;
                NotifyOfPropertyChange(() => AddNewTestTooltip);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [expand all mode].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [expand all mode]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandAllMode
        {
            get { return _expandAllMode; }
            set
            {
                _expandAllMode = value;
                NotifyOfPropertyChange(() => ExpandAllMode);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the options command show skip executing it's next execution.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [skip options command execute]; otherwise, <c>false</c>.
        /// </value>
        public bool SkipOptionsCommandExecute
        {
            get { return _skipOptionsCommandExecute; }
            set
            {
                _skipOptionsCommandExecute = value;
                NotifyOfPropertyChange(() => SkipOptionsCommandExecute);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [show options].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show options]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOptions
        {
            get { return _showOptions; }
            set
            {
                _showOptions = value;
                NotifyOfPropertyChange(() => ShowOptions);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show version].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show version]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowVersion
        {
            get { return _showVersion; }
            set
            {
                _showVersion = value;
                NotifyOfPropertyChange(() => ShowVersion);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show server].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show server]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowServer
        {
            get { return _showServer; }
            set
            {
                _showServer = value;
                NotifyOfPropertyChange(() => ShowServer);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show type].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show type]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowType
        {
            get { return _showType; }
            set
            {
                _showType = value;
                NotifyOfPropertyChange(() => ShowType);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show time].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show time]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTime
        {
            get { return _showTime; }
            set
            {
                _showTime = value;
                NotifyOfPropertyChange(() => ShowTime);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show duratrion].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show duratrion]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDuration
        {
            get { return _showDuration; }
            set
            {
                _showDuration = value;
                NotifyOfPropertyChange(() => ShowDuration);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show inputs].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show inputs]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInputs
        {
            get { return _showInputs; }
            set
            {
                _showInputs = value;
                NotifyOfPropertyChange(() => ShowInputs);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show outputs].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show outputs]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOutputs
        {
            get { return _showOutputs; }
            set
            {
                _showOutputs = value;
                NotifyOfPropertyChange(() => ShowOutputs);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [show assertResult].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [show assertResult]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAssertResult
        {
            get { return _showAssertResult; }
            set
            {
                _showAssertResult = value;
                NotifyOfPropertyChange(() => ShowAssertResult);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether [highligh error].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [highligh error]; otherwise, <c>false</c>.
        /// </value>
        public bool HighlightError
        {
            get { return _highlightError; }
            set
            {
                _highlightError = value;
                NotifyOfPropertyChange(() => HighlightError);
            }
        }

        /// <summary>
        ///     Returns a observable collection containing the root level items
        ///     in the debug tree, to which the TreeView can bind.
        /// </summary>
        public ObservableCollection<IDebugTreeViewItemViewModel> RootItems => _rootItems ?? (_rootItems = new ObservableCollection<IDebugTreeViewItemViewModel>());

        public ICommand ClearSearchTextCommand { get; private set; }

        /// <summary>
        ///     Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText)
                    return;

                _searchText = value;
                NotifyOfPropertyChange(() => SearchText);

                RebuildTree();
            }
        }

        public bool ShowDebugStatus
        {
            get
            {
                return _showDebugStatus;
            }
            set
            {
                if (_showDebugStatus == value)
                    return;

                _showDebugStatus = value;
                NotifyOfPropertyChange(() => ShowDebugStatus);
            }
        }

        /// <summary>
        ///     Appends the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        public virtual void Append(IDebugState content)
        {
            if (_outputViewModelUtil.ContenIsNotValid(content)) return;

            IsDebugStateLastStep(content);

            _continueDebugDispatch = false;
            if (content.IsFinalStep() && !IsTestView)
            {
                _allDebugReceived = true;
                _continueDebugDispatch = true;
                ViewModelUtils.RaiseCanExecuteChanged(AddNewTestCommand);
            }

            if(content.StateType == StateType.TestAggregate && IsTestView)
            {
                _allDebugReceived = true;
                _continueDebugDispatch = true;
                ViewModelUtils.RaiseCanExecuteChanged(AddNewTestCommand);
            }

            if (_outputViewModelUtil.QueuePending(content, _pendingItems, IsProcessing))
                return;
            AddItemToTree(content);
            
        }

        private void IsDebugStateLastStep(IDebugState content)
        {
            if ((DebugStatus != DebugStatus.Stopping && DebugStatus != DebugStatus.Finished) || content.StateType == StateType.Message) return;
            if (content.StateType != StateType.End && !IsTestView)
            {
                _lastStep = content;
            }
            if (content.StateType != StateType.TestAggregate && IsTestView)
            {
                _lastStep = content;
            }
        }

        //This is used in the debug view to open the more link file. This is called Dynamically so shows as unused.
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [ExcludeFromCodeCoverage]
        public void OpenMoreLink(IDebugLineItem item)
        {
            if (_outputViewModelUtil.IsItemMoreLinkValid(item) && CanOpenMoreLink(item))
                CreatProcessController(item);
        }

        public bool CanOpenMoreLink(IDebugLineItem item)
        {
            return !string.IsNullOrEmpty(item?.MoreLink);
        }
        [ExcludeFromCodeCoverage]
        private void CreatProcessController(IDebugLineItem item)
        {
            try
            {
                var debugItemTempFilePath = FileHelper.GetDebugItemTempFilePath(item.MoreLink);
                Dev2Logger.Debug($"Debug file path is [{debugItemTempFilePath}]");
                Process.Start(new ProcessStartInfo(debugItemTempFilePath));
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
                ProcessControllerHasError(ex);
            }
        }
        [ExcludeFromCodeCoverage]
        private void ProcessControllerHasError(Exception ex)
        {
            if (ex.Message.Contains("The remote name could not be resolved"))
            {
                _popup.Show(
                    string.Format(Warewolf.Studio.Resources.Languages.Core.DebugCouldNotGetRemoteDebugItemsError, Environment.NewLine),
                    Warewolf.Studio.Resources.Languages.Core.DebugCouldNotGetRemoteDebugItemsErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, "",
                    false, true, false, false, false, false);
            }
            else
            {
                _popup.Show(
                    string.Format(Warewolf.Studio.Resources.Languages.Core.DebugCouldNotGetDebugItemsError, Environment.NewLine),
                    Warewolf.Studio.Resources.Languages.Core.DebugCouldNotGetDebugItemsErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error, "",
                    false, true, false, false, false, false);
            }
        }

        public ICommand OpenItemCommand => _openItemCommand ?? (_openItemCommand = new DelegateCommand(OpenItem));

        public ICommand ExpandAllCommand => _expandAllCommand ?? (_expandAllCommand = new DelegateCommand(ExpandAll));

        public ICommand ShowOptionsCommand
        {
            get
            {
                return _showOptionsCommand ?? (_showOptionsCommand = new DelegateCommand(o =>
                {
                    if (SkipOptionsCommandExecute)
                    {
                        SkipOptionsCommandExecute = false;
                    }
                    else
                    {
                        ShowOptions = !ShowOptions;
                    }
                }));
            }
        }

        public bool IsConfiguring => DebugStatus == DebugStatus.Configure;

        public Guid SessionID { get; }

        public ICommand SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new DelegateCommand(SelectAll));
        public ICommand AddNewTestCommand { get; set; }
        public bool AddNewTestMode { get; set; }

        private void SelectAll(object obj)
        {
            ClearSelection();
            IterateItems<DebugStateTreeViewItemViewModel>(RootItems, item =>
            {
                item.SelectionType = ActivitySelectionType.Add;
                item.IsSelected = true;
            });
        }

        /// <summary>
        ///     Clears all content and the tree.
        /// </summary>
        public void Clear()
        {
            RootItems.Clear();
            _allDebugReceived = false;
            _contentItems.Clear();
            _contentItemMap.Clear();
            _pendingItems.Clear();
        }

        protected override void OnDispose()
        {
            Clear();
            _debugWriterSubscriptionService.Unsubscribe();
            _debugWriterSubscriptionService.Dispose();
            base.OnDispose();
        }


        /// <summary>
        ///     Expands all nodes.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public void ExpandAll(object payload)
        {
            var node = payload as IDebugTreeViewItemViewModel;

            if (node == null)
            {
                foreach (var rootNode in RootItems)
                {
                    ExpandAll(rootNode);
                }
                ExpandAllMode = !ExpandAllMode;
                return;
            }

            node.IsExpanded = ExpandAllMode;
            foreach (var childNode in node.Children)
            {
                ExpandAll(childNode);
            }
        }

        /// <summary>
        ///     Opens an item.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void OpenItem(object payload)
        {
            var debugState = payload as IDebugState;

            if (debugState?.ActivityType == ActivityType.Workflow)
            {
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                shellViewModel?.OpenResource(debugState.OriginatingResourceID, debugState.EnvironmentID,shellViewModel.ActiveServer);
            }
        }

        /// <summary>
        ///     Rebuilds the tree.
        /// </summary>
        private void RebuildTree()
        {
            lock (_syncContext)
            {
                _isRebuildingTree = true;
            }

            RootItems.Clear();
            _contentItemMap.Clear();

            foreach (var content in _contentItems)
            {
                AddItemToTreeImpl(content);
            }

            lock (_syncContext)
            {
                _isRebuildingTree = false;
            }
        }


        public void AddItemToTree(IDebugState content)
        {
            if (_contentItems.Any(a => a.DisconnectedID == content.DisconnectedID))
                return;
            if (content.StateType == StateType.Duration)
            {
                var item = _contentItems.FirstOrDefault(a => a.WorkSurfaceMappingId == content.WorkSurfaceMappingId);
                if (item != null)
                    item.EndTime = content.EndTime;
            }
            else
            {
                var environmentId = content.EnvironmentID;
                var isRemote = environmentId != Guid.Empty;
                if (isRemote)
                {
                    var remoteEnvironmentModel = _environmentRepository.FindSingle(model => model.ID == environmentId);
                    if (remoteEnvironmentModel != null)
                    {
                        if (content.Server == "localhost")
                            content.Server = remoteEnvironmentModel.Name;
                        if (!remoteEnvironmentModel.IsConnected)
                        {
                            remoteEnvironmentModel.Connect();
                        }
                        if (content.ParentID != Guid.Empty)
                        {
                            if (remoteEnvironmentModel.AuthorizationService != null)
                            {
                                var remoteResourcePermissions = remoteEnvironmentModel.AuthorizationService.GetResourcePermissions(content.OriginatingResourceID);
                                if (!remoteResourcePermissions.HasFlag(Permissions.View))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                var debugState = _contentItems.FirstOrDefault(state => state.DisconnectedID == content.DisconnectedID);
                if (debugState == null)
                {
                    _contentItems.Add(content);
                }
                else
                {
                    return;
                }
                lock (_syncContext)
                {
                    if (_isRebuildingTree)
                    {
                        return;
                    }
                }

                var application = Application.Current;
                if (application != null)
                {
                    var dispatcher = application.Dispatcher;
                    var contentToDispatch = content;
                    if (dispatcher != null && dispatcher.CheckAccess())
                    {
                        dispatcher.Invoke(() => AddItemToTreeImpl(contentToDispatch));
                    }
                }
                else
                {
                    AddItemToTreeImpl(content);
                }
            }
        }

        private void AddItemToTreeImpl(IDebugState content)
        {
            if ((DebugStatus == DebugStatus.Stopping || DebugStatus == DebugStatus.Finished || _allDebugReceived) && string.IsNullOrEmpty(content.Message) && !_continueDebugDispatch && !_dispatchLastDebugState)
            {
                return;
            }
            Dev2Logger.Debug(string.Format("Debug content to be added ID: {0}" + Environment.NewLine + "Parent ID: {1}" + Environment.NewLine + "Name: {2}", content.ID, content.ParentID, content.DisplayName));
            if (_lastStep != null && DebugStatus == DebugStatus.Finished && content.StateType == StateType.Message)
            {
                var lastDebugStateProcessed = _lastStep;
                _lastStep = null;
                _dispatchLastDebugState = true;
                AddItemToTreeImpl(new DebugState { StateType = StateType.Message, Message = Resources.CompilerMessage_ExecutionInterrupted, ParentID = lastDebugStateProcessed.ParentID });
                AddItemToTreeImpl(lastDebugStateProcessed);
                _dispatchLastDebugState = false;
            }

            if (!string.IsNullOrWhiteSpace(SearchText) && !_debugOutputFilterStrategy.Filter(content, SearchText))
            {
                return;
            }

            if (AddTreeViewItemToRootItems(content)) return;

            if (content.IsFinalStep())
            {
                DebugStatus = DebugStatus.Finished;
            }
        }

        private bool AddTreeViewItemToRootItems(IDebugState content)
        {
            if (content.StateType == StateType.Message && content.ParentID == Guid.Empty)
            {
                RootItems.Add(new DebugStringTreeViewItemViewModel { Content = content.Message, ActivityTypeName = content.ActualType});
            }
            else
            {
                var isRootItem = content.ParentID == Guid.Empty || content.ID == content.ParentID;

                var child = CreateChildTreeViewItem(content);

                if (!_contentItemMap.ContainsKey(content.ID))
                {
                    _contentItemMap.Add(content.ID, child);
                }
                if (isRootItem)
                {
                    RootItems.Add(child);
                }
                else
                {
                    var parent = CreateParentTreeViewItem(content, child);
                    if (AddErrorToParent(content, child, parent)) return true;
                }
            }
            return false;
        }

        private bool AddErrorToParent(IDebugState content, IDebugTreeViewItemViewModel child, IDebugTreeViewItemViewModel parent)
        {
            if (!child.HasError.GetValueOrDefault(false)) return false;
            var theParent = parent as DebugStateTreeViewItemViewModel;
            if (theParent == null)
                return true;

            theParent.AppendError(content.ErrorMessage);
            theParent.HasError = true;
            var childState = child as DebugStateTreeViewItemViewModel;
            if(childState?.AssertResultList != null && childState.AssertResultList.Count > 0 && IsTestView)
            {
                foreach (var listItem in childState.AssertResultList)
                {
                    var lineItem = listItem as DebugLine;
                    if (lineItem?.LineItems != null)
                    {
                        foreach(var lineItemLineItem in lineItem.LineItems)
                        {
                            var line = lineItemLineItem as DebugLineItem;
                            if (line != null && line.TestStepHasError)
                            {
                                theParent.AppendError(line.Value);                                
                            }
                        }
                    }
                }
            }
            return false;
        }

        private IDebugTreeViewItemViewModel CreateParentTreeViewItem(IDebugState content, IDebugTreeViewItemViewModel child)
        {
            IDebugTreeViewItemViewModel parent;
            if (!_contentItemMap.TryGetValue(content.ParentID, out parent))
            {
                parent = new DebugStateTreeViewItemViewModel(EnvironmentRepository)
                {
                    ActivityTypeName = content.ActualType
                };
                _contentItemMap.Add(content.ParentID, parent);
            }
            child.Parent = parent;
            parent.Children.Add(child);
            return parent;
        }

        private IDebugTreeViewItemViewModel CreateChildTreeViewItem(IDebugState content)
        {
            IDebugTreeViewItemViewModel child;
            if (content.StateType == StateType.Message)
            {
                child = new DebugStringTreeViewItemViewModel
                {
                    Content = content.Message,
                    ActivityTypeName = content.ActualType
                };
            }
            else
            {
                child = new DebugStateTreeViewItemViewModel(EnvironmentRepository)
                {
                    Content = content,
                    ActivityTypeName = content.ActualType
                };
            }
            return child;
        }

        void FlushPending()
        {
            while (_pendingItems.Count > 0)
            {
                AddItemToTree(_pendingItems[0]);
                _pendingItems.RemoveAt(0);
            }
        }

        public override void NotifyOfPropertyChange(string propertyName)
        {
            base.NotifyOfPropertyChange(propertyName);

            if (propertyName == "IsProcessing")
            {
                FlushPending();
            }
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        private static void IterateItems<T>(IEnumerable<IDebugTreeViewItemViewModel> items, Action<T> processItem)
            where T : IDebugTreeViewItemViewModel
        {
            foreach (var debugTreeViewItemViewModel in items.Where(i => i is T))
            {
                var item = (T)debugTreeViewItemViewModel;                
                if(item is DebugStateTreeViewItemViewModel)
                {
                    var actual = item as DebugStateTreeViewItemViewModel;
                    if (actual.Content.StateType != StateType.End)
                    {
                        processItem(item);
                        IterateItems(item.Children, processItem);
                    }
                    
                }
                
            }
        }

        private static void ClearSelection()
        {
            EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs
            {
                SelectionType = ActivitySelectionType.None
            });
        }
    }
}
