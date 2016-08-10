using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolboxViewModel : BindableBase, IToolboxViewModel, IDisposable, IUpdatesHelp
    {
        readonly IToolboxModel _localModel;
        readonly IToolboxModel _remoteModel;
        ICollection<IToolDescriptorViewModel> _tools;
        bool _isDesignerFocused;
        IToolDescriptorViewModel _selectedTool;
        private string _searchTerm;
        private ObservableCollection<IToolDescriptorViewModel> _backedUpTools;
        private readonly BackgroundWorker _worker;

        public ToolboxViewModel(IToolboxModel localModel, IToolboxModel remoteModel)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "localModel", localModel }, { "remoteModel", remoteModel } });
            _localModel = localModel;
            _remoteModel = remoteModel;
            _localModel.OnserverDisconnected += _localModel_OnserverDisconnected;
            _remoteModel.OnserverDisconnected += _remoteModel_OnserverDisconnected;
            BackedUpTools = new ObservableCollection<IToolDescriptorViewModel>(_remoteModel.GetTools().Select(a => new ToolDescriptorViewModel(a, _localModel.GetTools().Contains(a))));
            Tools = BackedUpTools;
            ClearFilterCommand = new DelegateCommand(() => SearchTerm = string.Empty);
            _worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += Filter;
            _worker.RunWorkerCompleted += FilterComplete;
        }

        public ICommand ClearFilterCommand { get; set; }

        #region Implementation of IToolboxViewModel

        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        public ICollection<IToolDescriptorViewModel> Tools
        {
            get
            {

                return _tools;
            }
            set
            {
                _tools = value;
                OnPropertyChanged("Tools");
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public ObservableCollection<IToolDescriptorViewModel> BackedUpTools
        {
            get { return _backedUpTools; }
            set
            {
                _backedUpTools = value;
                OnPropertyChanged("BackedUpTools");
            }
        }

        /// <summary>
        /// the toolbox is only enabled when the active server is connected and the designer is in focus
        /// </summary>
        public bool IsEnabled => IsDesignerFocused && _localModel.IsEnabled() && _remoteModel.IsEnabled();

        public bool IsDesignerFocused
        {
            get
            {
                return _isDesignerFocused;
            }
            set
            {
                _isDesignerFocused = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        public IToolDescriptorViewModel SelectedTool
        {
            get
            {
                return _selectedTool;
            }
            set
            {

                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (value != _selectedTool)
                {
                    _selectedTool = value;
                    OnPropertyChanged("SelectedTool");
                }
            }
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged("SearchTerm");
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Tools = new AsyncObservableCollection<IToolDescriptorViewModel>(_backedUpTools.ToList());
                    }
                    else
                    {
                        BeginFilter(value.ToLowerInvariant());
                    }
                }
            }
        }

        private void Filter(object sender, DoWorkEventArgs e)
        {
            var query = (string)e.Argument;

            var searchWords = query.ToLower().Split(' ');

            var results = _backedUpTools.Where(i => i.Tool.Name.ToLower().Contains(searchWords[0])
                  || i.Tool.Category.ToLower().Contains(searchWords[0])
                  || i.Tool.FilterTag.ToLower().Contains(searchWords[0]));

            if (searchWords.Length > 1)
            {
                for (int x = 1; x < searchWords.Length; x++)
                {
                    var x1 = x;
                    results = results.Where(i => i.Tool.Name.ToLower().Contains(searchWords[x1])
                                                 || i.Tool.Category.ToLower().Contains(searchWords[x1])
                                                 || i.Tool.FilterTag.ToLower().Contains(searchWords[x1]));
                }
            }

            e.Result = results;
        }

        private void FilterComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            var filtered = (IEnumerable<IToolDescriptorViewModel>)e.Result;
            Tools = new AsyncObservableCollection<IToolDescriptorViewModel>(filtered.ToList());
        }

        private void RefilterOnCompletion(object sender, RunWorkerCompletedEventArgs e)
        {
            _worker.RunWorkerCompleted -= RefilterOnCompletion;
            _worker.RunWorkerAsync(_searchTerm.ToLowerInvariant());
        }

        private void BeginFilter(string filterText)
        {
            if (_worker.IsBusy)
            {
                if (!_worker.CancellationPending)
                {
                    _worker.RunWorkerCompleted += RefilterOnCompletion;
                    _worker.CancelAsync();
                }
            }
            else
            {
                _worker.RunWorkerAsync(filterText.ToLowerInvariant());
            }
        }

        void _remoteModel_OnserverDisconnected(object sender)
        {
            OnPropertyChanged("IsEnabled");
        }

        void _localModel_OnserverDisconnected(object sender)
        {
            OnPropertyChanged("IsEnabled");
        }
        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable UnusedParameter.Local
        void Dispose(bool disposing)
        // ReSharper restore UnusedParameter.Local
        {
            _localModel.OnserverDisconnected -= _localModel_OnserverDisconnected;
            _remoteModel.OnserverDisconnected -= _remoteModel_OnserverDisconnected;
        }

        #endregion

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
