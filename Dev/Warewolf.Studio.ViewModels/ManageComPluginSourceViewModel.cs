#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageComPluginSourceViewModel : SourceBaseImpl<IComPluginSource>, IManageComPluginSourceViewModel
    {
        IDllListingModel _selectedDll;
        readonly IManageComPluginSourceModel _updateManager;
        IComPluginSource _pluginSource;
        string _resourceName;
        readonly string _warewolfserverName;
        string _headerText;
        bool _isDisposed;
        ObservableCollection<IDllListingModel> _dllListings;
        bool _isLoading;
        string _searchTerm;
        string _assemblyName;
        Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        bool _is32Bit;
        string _clsId;
        AsyncObservableCollection<IDllListingModel> _originalDllListings;

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base("ComPluginSource")
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;
            AsyncWorker = asyncWorker;
            HeaderText = Resources.Languages.Core.ComPluginSourceNewHeaderLabel;
            Header = Resources.Languages.Core.ComPluginSourceNewHeaderLabel;
            OkCommand = new DelegateCommand(Save, CanSave);
            CancelCommand = new DelegateCommand(() => CloseAction.Invoke());
            ClearSearchTextCommand = new DelegateCommand(() => SearchTerm = "");
            RefreshCommand = new DelegateCommand(() => PerformLoadAll());

            _warewolfserverName = updateManager.ServerName;
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                DispatcherAction = Application.Current.Dispatcher.Invoke;
            }

        }

        public IAsyncWorker AsyncWorker { get; set; }

        public ICommand RefreshCommand { get; set; }

        public Action<System.Action> DispatcherAction { get; set; }

        void PerformLoadAll(Action actionToPerform = null)
        {
            AsyncWorker.Start(() =>
            {
                IsLoading = true;
                var names = _updateManager.GetComDllListings(null).Select(input => new DllListingModel(_updateManager, input)).ToList();

                DispatcherAction.Invoke(() =>
                {
                    _originalDllListings = new AsyncObservableCollection<IDllListingModel>(names);
                    DllListings = _originalDllListings;
                    IsLoading = false;
                    actionToPerform?.Invoke();
                });
            }, () =>
            {

            }, exception =>
            {
            });
        }

        public ICommand ClearSearchTextCommand { get; set; }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged(() => IsLoading);
            }
        }

        public string SearchTerm
        {
            get
            {
                return _searchTerm;
            }
            set
            {
                if (!value.Equals(_searchTerm))
                {
                    _searchTerm = value;
                    PerformSearch(_searchTerm);
                    OnPropertyChanged(() => SearchTerm);
                }
            }
        }

        void PerformSearch(string searchTerm)
        {

            if (DllListings != null)
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    DllListings = _originalDllListings;
                }
                else
                {
                    var filteredList = _originalDllListings.Where(model => model.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));
                    DllListings = new ObservableCollection<IDllListingModel>(filteredList);
                }

            }
        }

        public ICommand CancelCommand { get; set; }
        public System.Action CloseAction { get; set; }
        public ObservableCollection<IDllListingModel> DllListings
        {
            get
            {
                return _dllListings;
            }
            set
            {
                _dllListings = value;
                OnPropertyChanged(() => DllListings);
            }
        }
        
        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : this(updateManager, aggregator, asyncWorker)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformLoadAll();
            _requestServiceNameViewModel = requestServiceNameViewModel;
            Item = ToModel();
        }

        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IAsyncWorker asyncWorker, Action<Action> dispatcherAction)
            : this(updateManager, aggregator, asyncWorker)
        {
            DispatcherAction = dispatcherAction;
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformLoadAll();
            _requestServiceNameViewModel = requestServiceNameViewModel;
            Item = ToModel();
        }
        
        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IComPluginSource pluginSource, IAsyncWorker asyncWorker)
            : this(updateManager, aggregator, asyncWorker)
        {
            VerifyArgument.IsNotNull("compluginSource", pluginSource);
            asyncWorker.Start(() =>
            {
                IsLoading = true;
                var comPluginSource = updateManager.FetchSource(pluginSource.Id);
                var names = updateManager.GetComDllListings(null)?.Select(input => new DllListingModel(updateManager, input)).ToList();
                return new Tuple<IComPluginSource, List<DllListingModel>>(comPluginSource, names);
            }, tuple =>
            {
                if (tuple.Item2 != null)
                {
                    _originalDllListings = new AsyncObservableCollection<IDllListingModel>(tuple.Item2);
                    DllListings = _originalDllListings;
                }
                _pluginSource = tuple.Item1;
                _pluginSource.ResourcePath = pluginSource.ResourcePath;
                SetupHeaderTextFromExisting();
                FromModel(_pluginSource);
                Item = ToModel();
                IsLoading = false;
            });
        }

        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IComPluginSource pluginSource, IAsyncWorker asyncWorker, Action<System.Action> dispatcherAction)
            : this(updateManager, aggregator, asyncWorker)
        {
            VerifyArgument.IsNotNull("comPluginSource", pluginSource);
            DispatcherAction = dispatcherAction;
            _pluginSource = pluginSource;
            SetupHeaderTextFromExisting();
            PerformLoadAll(() => FromModel(_pluginSource));
            Item = ToModel();
        }

        public ManageComPluginSourceViewModel()
             : base("ComPluginSource")
        {

        }

        public override void FromModel(IComPluginSource source)
        {
            var selectedDll = source.SelectedDll;
            if (selectedDll != null)
            {
                var dllListingModel = DllListings?.FirstOrDefault(model => model.Name == selectedDll.Name);
                if (dllListingModel != null)
                {
                    dllListingModel.IsExpanded = true;
                    SelectedDll = dllListingModel;
                    SelectedDll.IsSelected = true;
                }
            }

            Name = source.ResourceName;
            Path = source.ResourcePath;
            Is32Bit = source.Is32Bit;
            ClsId = source.ClsId;

        }

        public override string Name
        {
            get
            {
                return ResourceName;
            }
            set
            {
                ResourceName = value;
            }
        }

        public IDllListingModel SelectedDll
        {
            get
            {
                return _selectedDll;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _selectedDll = value;
                OnPropertyChanged(() => SelectedDll);
                if (SelectedDll != null)
                {
                    ClsId = SelectedDll.ClsId;
                    Is32Bit = SelectedDll.Is32Bit;
                    AssemblyName = SelectedDll.Name;
                }
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public string AssemblyName
        {
            get
            {
                return _assemblyName;
            }
            set
            {
                _assemblyName = value;
                if (string.IsNullOrEmpty(_assemblyName))
                {
                    SelectedDll = null;
                }
                OnPropertyChanged(() => Header);
                OnPropertyChanged(() => AssemblyName);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        void SetupHeaderTextFromExisting()
        {
            var serverName = _warewolfserverName;
            if (serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                HeaderText = (_pluginSource == null ? ResourceName : _pluginSource.ResourceName).Trim();
                Header = (_pluginSource == null ? ResourceName : _pluginSource.ResourceName).Trim();
            }
            else
            {
                HeaderText = (_pluginSource == null ? ResourceName : _pluginSource.ResourceName).Trim();
                Header = (_pluginSource == null ? ResourceName : _pluginSource.ResourceName).Trim();
            }
        }

        public override bool CanSave() => _selectedDll != null && !string.IsNullOrEmpty(AssemblyName) && !string.IsNullOrEmpty(ClsId) && HasChanged;

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    SetupHeaderTextFromExisting();
                }
                OnPropertyChanged(_resourceName);
            }
        }

        public bool Is32Bit
        {
            get
            {
                return _is32Bit;
            }
            set
            {
                _is32Bit = value;
                OnPropertyChanged(() => Is32Bit);
            }
        }

        public string ClsId
        {
            get
            {
                return _clsId;
            }
            set
            {
                _clsId = value;
                OnPropertyChanged(() => ClsId);
            }
        }

        public override void Save()
        {
            if (_pluginSource == null)
            {
                var res = GetRequestServiceNameViewModel().ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    ResourceName = GetRequestServiceNameViewModel().ResourceName.Name;
                    var src = ToModel();
                    src.Id = SelectedGuid;
                    src.ResourcePath = GetRequestServiceNameViewModel().ResourceName.Path ?? GetRequestServiceNameViewModel().ResourceName.Name;
                    src.ClsId = SelectedDll.ClsId;
                    src.Is32Bit = SelectedDll.Is32Bit;
                    Save(src);
                    if (GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel != null)
                    {
                        AfterSave(GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                    }

                    Path = src.ResourcePath;
                    src.Is32Bit = SelectedDll.Is32Bit;
                    _pluginSource = src;
                    Item = ToModel();
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToModel();
                src.ClsId = SelectedDll.ClsId;
                src.Is32Bit = SelectedDll.Is32Bit;
                Save(src);
                _pluginSource = src;
                Item = ToModel();
            }
            OnPropertyChanged(() => Header);
        }

        public string Path { get; set; }



        void Save(IComPluginSource source)
        {
            source.SelectedDll = new DllListing(_selectedDll);
            _updateManager.Save(source);
        }

        public sealed override IComPluginSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }
            return new ComPluginSourceDefinition
            {
                ResourceName = ResourceName,
                ClsId = ClsId,
                Is32Bit = Is32Bit,
                SelectedDll = SelectedDll,
                ResourcePath = Path
            };
        }

        IComPluginSource ToSource()
        {
            if (_pluginSource == null)
            {
                return new ComPluginSourceDefinition
                {
                    ResourceName = ResourceName,
                    ClsId = ClsId,
                    Is32Bit = Is32Bit,
                    SelectedDll = SelectedDll,
                    ResourcePath = Path
                };
            }
            if (_selectedDll != null)
            {
                _pluginSource.SelectedDll = _selectedDll;
                _pluginSource.ClsId = ClsId;
                _pluginSource.Is32Bit = Is32Bit;
            }
            return _pluginSource;
        }

        public IRequestServiceNameViewModel GetRequestServiceNameViewModel()
        {
            if (_requestServiceNameViewModel != null)
            {
                _requestServiceNameViewModel.Wait();
                if (_requestServiceNameViewModel.Exception == null)
                {
                    return _requestServiceNameViewModel.Result;
                }

                else
                {
                    throw _requestServiceNameViewModel.Exception;
                }
            }
            return null;
        }

        public void SetRequestServiceNameViewModel(IRequestServiceNameViewModel value)
        {
            _requestServiceNameViewModel = new Task<IRequestServiceNameViewModel>(() => value);
            _requestServiceNameViewModel.Start();
        }

        public ICommand OkCommand { get; set; }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }

        protected override void OnDispose()
        {
            GetRequestServiceNameViewModel()?.Dispose();
            DisposeManageComPluginSourceViewModel(true);
        }
        
        void DisposeManageComPluginSourceViewModel(bool disposing)
        {
            if (!_isDisposed && !disposing)
            {
                _isDisposed = true;
            }
        }
    }
}