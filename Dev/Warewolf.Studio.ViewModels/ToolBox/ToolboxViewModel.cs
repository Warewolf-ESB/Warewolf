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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core;
using Dev2.Instrumentation;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolboxViewModel : BindableBase, IToolboxViewModel, IDisposable, IUpdatesHelp
    {
        readonly IToolboxModel _localModel;
        readonly IToolboxModel _remoteModel;
        ICollection<IToolDescriptorViewModel> _tools;
        bool _isDesignerFocused;
        IToolDescriptorViewModel _selectedTool;
        string _searchTerm;
        ObservableCollection<IToolDescriptorViewModel> _backedUpTools;
        bool _isVisible;
        readonly IApplicationTracker _applicationTracker;

        public ToolboxViewModel(IToolboxModel localModel, IToolboxModel remoteModel)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "localModel", localModel }, { "remoteModel", remoteModel } });
            _localModel = localModel;
            _remoteModel = remoteModel;
            _localModel.OnserverDisconnected += _localModel_OnserverDisconnected;
            _remoteModel.OnserverDisconnected += _remoteModel_OnserverDisconnected;
            BuildToolsList();
            ClearFilterCommand = new DelegateCommand(() => SearchTerm = string.Empty);
            _applicationTracker = CustomContainer.Get<IApplicationTracker>();
        }

        public void BuildToolsList()
        {
            BackedUpTools =
                new ObservableCollection<IToolDescriptorViewModel>(
                    _remoteModel.GetTools().Select(a => new ToolDescriptorViewModel(a, _localModel.GetTools().Contains(a))));
            Tools = new AsyncObservableCollection<IToolDescriptorViewModel>(_backedUpTools);
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
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value; 
                OnPropertyChanged("IsVisible");
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
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        FilterItems(value.ToLowerInvariant());
                    }
                    else
                    {
                        Tools = new AsyncObservableCollection<IToolDescriptorViewModel>();
                    }
                }
            }
        }

        void FilterItems(string filterText)
        {
            var searchWords = filterText.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


            if (_applicationTracker != null)
            {
                _applicationTracker.TrackCustomEvent(Resources.Languages.TrackEventToolbox.EventCategory,
                                                Resources.Languages.TrackEventToolbox.ToolBoxSearch, filterText);
            }
            var results = _backedUpTools.Where(i =>
                     searchWords.All(s => i.Tool.Name.ToLower().Contains(s))
                     || searchWords.All(s => i.Tool.Category.ToLower().Contains(s))
                     || i.Tool.FilterTag.ToLower().Equals(filterText)
                     || i.Tool.FilterTag.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Any(s => searchWords.Any(s1 => s1.Equals(s.ToLower()))));

            Tools = new AsyncObservableCollection<IToolDescriptorViewModel>(results);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
                
        void Dispose(bool disposing)        
        {
            if (disposing)
            {
                _localModel.OnserverDisconnected -= _localModel_OnserverDisconnected;
                _remoteModel.OnserverDisconnected -= _remoteModel_OnserverDisconnected;
            }
        }

        #endregion

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackCustomEvent(Resources.Languages.TrackEventHelp.EventCategory,
                                                Resources.Languages.TrackEventHelp.Help, helpText);
            }
        }
    }
}
