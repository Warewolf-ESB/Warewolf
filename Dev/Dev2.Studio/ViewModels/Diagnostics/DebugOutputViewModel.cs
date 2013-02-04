using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    /// <summary>
    /// This is the view-model of the UI.  It provides a data source
    /// for the TreeView (the RootItems property), a bindable
    /// SearchText property, and the SearchCommand to perform a search.
    /// </summary>
    public class DebugOutputViewModel : SimpleBaseViewModel
    {
        #region Fields

        private object _syncContext = new object();
        private readonly List<object> _contentItems;
        private readonly ObservableCollection<DebugTreeViewItemViewModel> _rootItems;
        //private ICommand _searchCommand;
        private ICommand _openItemCommand;
        private ICommand _expandAllCommand;
        private ICommand _showOptionsCommand;

        string _searchText = string.Empty;

        private bool _showVersion = false;
        private bool _showServer = true;
        private bool _showType = true;
        private bool _showTime = false;
        private bool _showDuratrion = false;
        private bool _showInputs = true;
        private bool _showOutputs = true;
        private bool _highlighSimulation = true;
        private bool _highlighError = true;

        private bool _showOptions = false;
        private bool _skipOptionsCommandExecute = false;

        private bool _expandAllMode = false;
        private bool _isRebuildingTree = false;

        private int _depthLimit = 0;

        private DebugOutputTreeGenerationStrategy _debugOutputTreeGenerationStrategy;

        #endregion

        #region Ctor

        public DebugOutputViewModel()
        {
            _debugOutputTreeGenerationStrategy = new DebugOutputTreeGenerationStrategy();

            _rootItems = new ObservableCollection<DebugTreeViewItemViewModel>();
            _contentItems = new List<object>();

            Mediator.RegisterToReceiveMessage(MediatorMessages.DebugWriterWrite, Write);
            Mediator.RegisterToReceiveMessage(MediatorMessages.DebugWriterAppend, Append);

            EnvironmentRepository = ImportService.GetExportValue<IFrameworkRepository<IEnvironmentModel>>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the environment repository, this property is imported via MEF.
        /// </summary>
        /// <value>
        /// The environment repository.
        /// </value>
        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; set; }

        /// <summary>
        /// Gets or sets the depth limit.
        /// </summary>
        /// <value>
        /// The depth limit.
        /// </value>
        public int DepthLimit
        {
            get
            {
                return _depthLimit;
            }
            set
            {
                if (_depthLimit != value)
                {
                    _depthLimit = value;
                    OnPropertyChanged("DepthLimit");
                    RebuildTree();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [expand all mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [expand all mode]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandAllMode
        {
            get
            {
                return _expandAllMode;
            }
            set
            {
                _expandAllMode = value;
                OnPropertyChanged("ExpandAllMode");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the options command show skip executing it's next execution.
        /// </summary>
        /// <value>
        /// <c>true</c> if [skip options command execute]; otherwise, <c>false</c>.
        /// </value>
        public bool SkipOptionsCommandExecute
        {
            get
            {
                return _skipOptionsCommandExecute;
            }
            set
            {
                _skipOptionsCommandExecute = value;
                OnPropertyChanged("SkipOptionsCommandExecute");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show options].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show options]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOptions
        {
            get
            {
                return _showOptions;
            }
            set
            {
                _showOptions = value;
                OnPropertyChanged("ShowOptions");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show version].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show version]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowVersion
        {
            get
            {
                return _showVersion;
            }
            set
            {
                _showVersion = value;
                OnPropertyChanged("ShowVersion");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show server].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show server]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowServer
        {
            get
            {
                return _showServer;
            }
            set
            {
                _showServer = value;
                OnPropertyChanged("ShowServer");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show type].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show type]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowType
        {
            get
            {
                return _showType;
            }
            set
            {
                _showType = value;
                OnPropertyChanged("ShowType");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show time].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show time]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTime
        {
            get
            {
                return _showTime;
            }
            set
            {
                _showTime = value;
                OnPropertyChanged("ShowTime");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show duratrion].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show duratrion]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDuratrion
        {
            get
            {
                return _showDuratrion;
            }
            set
            {
                _showDuratrion = value;
                OnPropertyChanged("ShowDuratrion");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show inputs].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show inputs]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInputs
        {
            get
            {
                return _showInputs;
            }
            set
            {
                _showInputs = value;
                OnPropertyChanged("ShowInputs");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [show outputs].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show outputs]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOutputs
        {
            get
            {
                return _showOutputs;
            }
            set
            {
                _showOutputs = value;
                OnPropertyChanged("ShowOutputs");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [highligh simulation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [highligh simulation]; otherwise, <c>false</c>.
        /// </value>
        public bool HighlighSimulation
        {
            get
            {
                return _highlighSimulation;
            }
            set
            {
                _highlighSimulation = value;
                OnPropertyChanged("HighlighSimulation");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [highligh error].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [highligh error]; otherwise, <c>false</c>.
        /// </value>
        public bool HighlighError
        {
            get
            {
                return _highlighError;
            }
            set
            {
                _highlighError = value;
                OnPropertyChanged("HighlighError");
            }
        }

        /// <summary>
        /// Gets or sets the debug writer.
        /// </summary>
        /// <value>
        /// The debug writer.
        /// </value>
        public IDebugWriter DebugWriter
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a observable collection containing the root level items
        /// in the debug tree, to which the TreeView can bind.
        /// </summary>
        public ObservableCollection<DebugTreeViewItemViewModel> RootItems
        {
            get
            {
                return _rootItems;
            }
        }

        /// <summary>
        /// Gets/sets a fragment of the name to search for.
        /// </summary>
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                if(value == _searchText)
                {
                    return;
                }

                _searchText = value;
                OnPropertyChanged("SearchText");

                RebuildTree();
            }
        }

        #endregion

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

        #endregion

        #region Private Methods

        /// <summary>
        /// Clears all content and the tree.
        /// </summary>
        private void Clear(object payload)
        {
            RootItems.Clear();
            _contentItems.Clear();
        }

        /// <summary>
        /// Expands all nodes.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void ExpandAll(object payload)
        {
            DebugTreeViewItemViewModel node = payload as DebugTreeViewItemViewModel;

            //
            // If no node is passed in then call for all root nodes
            //
            if (node == null)
            {
                foreach (DebugTreeViewItemViewModel rootNode in RootItems)
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
            foreach (DebugTreeViewItemViewModel childNode in node.Children)
            {
                ExpandAll(childNode);
            }
        }

        /// <summary>
        /// Opens an item.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void OpenItem(object payload)
        {
            IDebugState debugState = payload as IDebugState;

            if (debugState == null)
            {
                return;
            }

            if (debugState.ActivityType == ActivityType.Workflow && EnvironmentRepository != null)
            {
                IEnvironmentModel environment = EnvironmentRepository.All().FirstOrDefault(e => 
                {
                    IStudioClientContext studioClientContext = e.DsfChannel as IStudioClientContext;
                        
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

                IResourceModel resource = environment.Resources.FindSingle(r => r.ResourceName == debugState.DisplayName);

                if (resource == null)
            {
                    return;
                }

                Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, resource);
            }
        }

        /// <summary>
        /// Writes the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        private void Write(object content)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<object>(WriteUI), content);
        }

        /// <summary>
        /// Writes the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        private void WriteUI(object content)
        {
            Clear(null);
            AppendUI(content);
        }

        /// <summary>
        /// Appends the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        private void Append(object content)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<object>(AppendUI), content);
        }

        /// <summary>
        /// Appends the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        private void AppendUI(object content)
        {
            _contentItems.Add(content);

            lock (_syncContext)
            {
                if (_isRebuildingTree)
                {
                    return;
                }
            }

            _debugOutputTreeGenerationStrategy.PlaceContentInTree(RootItems, _contentItems, content, SearchText, false, DepthLimit);
        }

        /// <summary>
        /// Rebuilds the tree.
        /// </summary>
        private void RebuildTree()
        {
            lock (_syncContext)
            {
                _isRebuildingTree = true;
            }

            RootItems.Clear();

            foreach (object content in _contentItems)
            {
                _debugOutputTreeGenerationStrategy.PlaceContentInTree(RootItems, _contentItems, content, SearchText, false, DepthLimit);
            }

            lock (_syncContext)
            {
                _isRebuildingTree = false;
            }
        }

        #endregion Private Methods
    }
}
