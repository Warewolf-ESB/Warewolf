using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolboxViewModel : BindableBase, IToolboxViewModel, IDisposable, IUpdatesHelp
    {
        readonly IToolboxModel _localModel;
        readonly IToolboxModel _remoteModel;
        ICollection<IToolDescriptorViewModel> _tools;
        private ICollection<IToolDescriptorViewModel> _filteredTools;
        bool _isDesignerFocused;
        IToolDescriptorViewModel _selectedTool;
        private string _searchTerm;

        public ToolboxViewModel(IToolboxModel localModel, IToolboxModel remoteModel)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "localModel", localModel }, { "remoteModel", remoteModel } });
            _localModel = localModel;
            _remoteModel = remoteModel;
            _localModel.OnserverDisconnected += _localModel_OnserverDisconnected;
            _remoteModel.OnserverDisconnected += _remoteModel_OnserverDisconnected;
            FilteredTools = new List<IToolDescriptorViewModel>();
            BackedUpTools = new ObservableCollection<IToolDescriptorViewModel>(_remoteModel.GetTools().Select(a => new ToolDescriptorViewModel(a, _localModel.GetTools().Contains(a))));
            Tools = BackedUpTools;
            ClearFilterCommand = new DelegateCommand(() => SearchTerm = string.Empty);
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
            private set
            {
                _tools = value;
                OnPropertyChanged("Tools");
            }
        }
        private ICollection<IToolDescriptorViewModel> FilteredTools
        {
            get { return _filteredTools; }
            set
            {
                _filteredTools = value;
            }
        }

        private ObservableCollection<IToolDescriptorViewModel> BackedUpTools { get; set; }
        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        public ICollection<IToolboxCatergoryViewModel> CategorisedTools
        {
            get
            {
                var toolboxCatergoryViewModels = new ObservableCollection<IToolboxCatergoryViewModel>(Tools.GroupBy(a => a.Tool.Category)
                    .Select(b => new ToolBoxCategoryViewModel(b.Key, new ObservableCollection<IToolDescriptorViewModel>(b))));
                return toolboxCatergoryViewModels;
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
                    OnPropertyChanged(() => SearchTerm);
                    Filter(_searchTerm);
                }
            }
        }

        /// <summary>
        /// filters the list of tools available to the user.
        /// </summary>
        /// <param name="searchString"></param>
        public void Filter(string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString))
            {
                ClearFilter();
            }
            else
            {
                var toolboxCatergoryViewModels = BackedUpTools.Where(model => model.Tool.Name.ToLower().Contains(searchString.ToLower()) ||
                                                                            model.Tool.Category.ToLower().Contains(searchString.ToLower()) ||
                                                                            model.Tool.FilterTag.ToLower().Contains(searchString.ToLower()));
                FilteredTools =
                    toolboxCatergoryViewModels.OrderBy(model => model.Tool.Name)
                        .ThenBy(model => model.Tool.Category)
                        .ThenBy(model => model.Tool.FilterTag)
                        .ToList();
                Tools = FilteredTools;
            }
        }

        public void ClearFilter()
        {
            Tools = BackedUpTools;
            SearchTerm = "";
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
