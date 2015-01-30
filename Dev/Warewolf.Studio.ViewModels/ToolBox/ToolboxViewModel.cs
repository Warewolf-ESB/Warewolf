using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolboxViewModel:BindableBase,IToolboxViewModel,IDisposable
    {
        readonly IToolboxModel _localModel;
        readonly IToolboxModel _remoteModel;
        ICollection<IToolDescriptorViewModel> _tools;
        bool _isDesignerFocused;
        IToolDescriptorViewModel _selectedTool;
        private string _searchTerm;

        public ToolboxViewModel( IToolboxModel localModel,IToolboxModel remoteModel)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"localModel",localModel},{"remoteModel",remoteModel}});
            _localModel = localModel;
            _remoteModel = remoteModel;
            _localModel.OnserverDisconnected += _localModel_OnserverDisconnected;
            _remoteModel.OnserverDisconnected += _remoteModel_OnserverDisconnected;
        }



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
            protected set
            {
                OnPropertyChanged("Tools");
                OnPropertyChanged("CategorisedTools");
                _tools = value;
            }
        }
        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        public ICollection<IToolboxCatergoryViewModel> CategorisedTools
        {
            get
            {
                var toolboxCatergoryViewModels = new ObservableCollection<IToolboxCatergoryViewModel>(Tools.GroupBy(a=>a.Tool.Category)
                    .Select(b=> new ToolBoxCategoryViewModel(b.Key,new ObservableCollection<IToolDescriptorViewModel>(b))));
                return toolboxCatergoryViewModels;
            }
        }
        /// <summary>
        /// the toolbox is only enabled when the active server is connected and the designer is in focus
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return IsDesignerFocused && _localModel.IsEnabled() && _remoteModel.IsEnabled(); 
            }

        }
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
            // not sure about this warning ofr pure methods.
            // ReSharper disable MaximumChainedReferences
            Tools = new ObservableCollection<IToolDescriptorViewModel>( _remoteModel.GetTools()
                .Where(a => a.Name.Contains(searchString))
                .Select(a => new ToolDescriptorViewModel(a, _localModel.GetTools().Contains(a))));
            // ReSharper restore MaximumChainedReferences
        }

        public void ClearFilter()
        {
            // not sure about this warning ofr pure methods.
            // ReSharper disable MaximumChainedReferences
            Tools = new ObservableCollection<IToolDescriptorViewModel>(_remoteModel.GetTools().Select(a => new ToolDescriptorViewModel(a, _localModel.GetTools().Contains(a))));
            // ReSharper restore MaximumChainedReferences
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
    }
}
