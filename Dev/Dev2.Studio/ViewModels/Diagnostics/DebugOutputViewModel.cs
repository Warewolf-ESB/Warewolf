#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.ExtMethods;
using Dev2.Diagnostics;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Diagnostics;

#endregion

namespace Dev2.Studio.ViewModels.Diagnostics
{
    /// <summary>
    ///     This is the view-model of the UI.  It provides a data source
    ///     for the TreeView (the RootItems property), a bindable
    ///     SearchText property, and the SearchCommand to perform a search.
    /// </summary>
    public class DebugOutputViewModel : SimpleBaseViewModel
    {
        #region Fields

        private readonly List<IDebugState> _contentItems;
        private readonly DebugOutputTreeGenerationStrategy _debugOutputTreeGenerationStrategy;
        private readonly object _syncContext = new object();
        private int _depthLimit;
        private ICommand _expandAllCommand;
        private bool _expandAllMode;
        private bool _highlightError = true;
        private bool _highlightSimulation = true;
        private bool _isRebuildingTree;
        private ICommand _openItemCommand;
        private ObservableCollection<DebugTreeViewItemViewModel> _rootItems;
        private string _searchText = string.Empty;
        private bool _showDuratrion;
        private bool _showInputs = true;
        private bool _showOptions;
        private ICommand _showOptionsCommand;
        private bool _showOutputs = true;
        private bool _showServer = true;
        private bool _showTime;
        private bool _showType = true;
        private bool _showVersion;
        private bool _skipOptionsCommandExecute;
        private DebugStatus _debugStatus;
        private bool _showDebugStatus = true;

        #endregion

        #region Ctor

        public DebugOutputViewModel()
            : this(Core.EnvironmentRepository.Instance)
        {
        }

        public DebugOutputViewModel(WorkSurfaceKey workSurfaceKey = null)
            : this(Core.EnvironmentRepository.Instance, workSurfaceKey)
        {
        }

        public DebugOutputViewModel(IEnvironmentRepository environmentRepository, WorkSurfaceKey workSurfaceKey = null)
        {
            if (environmentRepository == null)
            {
                throw new ArgumentNullException("environmentRepository");
            }
            EnvironmentRepository = environmentRepository;
            _debugOutputTreeGenerationStrategy = new DebugOutputTreeGenerationStrategy(EnvironmentRepository);

            _contentItems = new List<IDebugState>();
        }

        #endregion

        #region Properties
        public ProcessController ProcessController { get; set; }

        public DebugStatus DebugStatus
        {
            get { return _debugStatus; }
            set
            {
                _debugStatus = value;

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
        public string ProcessingText
        {
            get { return DebugStatus.GetDescription(); }
        }

        public bool IsProcessing
        {
            get
            {
                return _debugStatus != DebugStatus.Ready && _debugStatus != DebugStatus.Finished;
            }
        }

        public bool IsStopping
        {
            get
            {
                return _debugStatus == DebugStatus.Stopping;
            }
        }

        public string DebugImage
        {
            get
            {
                return IsProcessing ? StringResources.Pack_Uri_Stop_Image : StringResources.Pack_Uri_Debug_Image;
            }
        }

        public string DebugText
        {
            get
            {
                return IsProcessing ? StringResources.Ribbon_StopExecution : StringResources.Ribbon_Debug;
            }
        }

        /// <summary>
        ///     Gets or sets the environment repository, this property is imported via MEF.
        /// </summary>
        /// <value>
        ///     The environment repository.
        /// </value>
        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        /// <summary>
        ///     Gets or sets the depth limit.
        /// </summary>
        /// <value>
        ///     The depth limit.
        /// </value>
        public int DepthLimit
        {
            get { return _depthLimit; }
            set
            {
                if (_depthLimit != value)
                {
                    _depthLimit = value;
                    NotifyOfPropertyChange(() => DepthLimit);
                    RebuildTree();
                }
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
        public bool ShowDuratrion
        {
            get { return _showDuratrion; }
            set
            {
                _showDuratrion = value;
                NotifyOfPropertyChange(() => ShowDuratrion);
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
        ///     Gets a value indicating whether [highligh simulation].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [highligh simulation]; otherwise, <c>false</c>.
        /// </value>
        public bool HighlightSimulation
        {
            get { return _highlightSimulation; }
            set
            {
                _highlightSimulation = value;
                NotifyOfPropertyChange(() => HighlightSimulation);
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
        ///     Gets or sets the debug writer.
        /// </summary>
        /// <value>
        ///     The debug writer.
        /// </value>
        public IDebugWriter DebugWriter { get; set; }

        /// <summary>
        ///     Returns a observable collection containing the root level items
        ///     in the debug tree, to which the TreeView can bind.
        /// </summary>
        public ObservableCollection<DebugTreeViewItemViewModel> RootItems
        {
            get
            {
                if (_rootItems == null)
                {
                    _rootItems = new ObservableCollection<DebugTreeViewItemViewModel>();
                }
                return _rootItems;
            }
        }

        /// <summary>
        ///     Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText)
                {
                    return;
                }

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
                {
                    return;
                }

                _showDebugStatus = value;
                NotifyOfPropertyChange(() => ShowDebugStatus);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Appends the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        public void Append(IDebugState content)
        {
            //Juries - Dont append start and end states, its not for display, just for logging puposes, unless its the first or last step
            if (content.StateType == StateType.Start && !content.IsFirstStep())
            {
                return;
            }

            if (content.StateType == StateType.End && !content.IsFinalStep())
            {
                return;
            }

            //
            //Juries - This is a dirty hack, naughty naughty.
            //Hijacked current functionality to enable erros to be added to an item after its already been added to the tree
            //
            if (content.StateType == StateType.Append)
            {
                _debugOutputTreeGenerationStrategy.AppendErrorToTreeParent(RootItems, _contentItems, content);
                return;
            }

            _contentItems.Add(content);

            lock (_syncContext)
            {
                if (_isRebuildingTree)
                {
                    return;
                }
            }

            _debugOutputTreeGenerationStrategy.PlaceContentInTree(RootItems, _contentItems, content, SearchText,
                                                                    false,
                                                                    DepthLimit);
        }

        public void OpenMoreLink(IDebugLineItem item)
        {
            if (item == null)
                return;

            if (!string.IsNullOrEmpty(item.MoreLink))
            {
                ProcessController = new ProcessController(Process.Start(new ProcessStartInfo(item.MoreLink)));
            }
        }

        public bool CanOpenMoreLink(IDebugLineItem item)
        {
            if (item == null)
                return false;

            return !string.IsNullOrEmpty(item.MoreLink);
        }

        #endregion public methods

        #region Commands

        public ICommand OpenItemCommand
        {
            get
            {
                if (_openItemCommand == null)
                {
                    _openItemCommand = new RelayCommand(OpenItem, c => true);
                }
                return _openItemCommand;
            }
        }

        public ICommand ExpandAllCommand
        {
            get
            {
                if (_expandAllCommand == null)
                {
                    _expandAllCommand = new RelayCommand(ExpandAll, c => true);
                }
                return _expandAllCommand;
            }
        }

        public ICommand ShowOptionsCommand
        {
            get
            {
                if (_showOptionsCommand == null)
                {
                    _showOptionsCommand = new RelayCommand(o =>
                        {
                            if (SkipOptionsCommandExecute)
                            {
                                SkipOptionsCommandExecute = false;
                            }
                            else
                            {
                                ShowOptions = !ShowOptions;
                            }
                        }, c => true);
                }
                return _showOptionsCommand;
            }
        }

        public bool IsConfiguring
        {
            get { return DebugStatus == DebugStatus.Configure; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Clears all content and the tree.
        /// </summary>
        public void Clear()
        {
            RootItems.Clear();
            _contentItems.Clear();
        }

        /// <summary>
        ///     Expands all nodes.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void ExpandAll(object payload)
        {
            var node = payload as DebugTreeViewItemViewModel;

            //
            // If no node is passed in then call for all root nodes
            //
            if (node == null)
            {
                foreach (var rootNode in RootItems)
                {
                    ExpandAll(rootNode);
                }

                //
                // Switch Expand modes
                //
                ExpandAllMode = !ExpandAllMode;

                return;
            }

            //
            // Expand node and call for all children
            //
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

            if (debugState == null)
            {
                return;
            }

            if (debugState.ActivityType == ActivityType.Workflow && EnvironmentRepository != null)
            {
                IEnvironmentModel environment = EnvironmentRepository.All().FirstOrDefault(e =>
                    {
                        var studioClientContext = e.DsfChannel as IStudioClientContext;

                        if (studioClientContext == null)
                        {
                            return false;
                        }

                        return studioClientContext.ServerID == debugState.ServerID;
                    });

                if (environment == null || !environment.IsConnected)
                {
                    return;
                }

                IResourceModel resource =
                    environment.ResourceRepository.FindSingle(r => r.ResourceName == debugState.DisplayName);

                if (resource == null)
                {
                    return;
                }
                EventAggregator.Publish(new AddWorkSurfaceMessage(resource));
            }
        }

        /// <summary>
        ///     Rebuilds the tree.
        /// </summary>
        private void RebuildTree()
        {
            lock(_syncContext)
            {
                _isRebuildingTree = true;
            }

            RootItems.Clear();

            foreach (var content in _contentItems)
            {
                _debugOutputTreeGenerationStrategy.PlaceContentInTree(RootItems, _contentItems, content, SearchText,
                                                                      false, DepthLimit);
            }

            lock(_syncContext)
            {
                _isRebuildingTree = false;
            }
        }

        #endregion Private Methods
    }
}