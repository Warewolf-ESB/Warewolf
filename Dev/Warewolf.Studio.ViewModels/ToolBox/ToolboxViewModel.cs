#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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
