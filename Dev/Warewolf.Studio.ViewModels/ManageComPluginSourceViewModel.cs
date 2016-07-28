using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
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
        private bool _isDisposed;
        List<IDllListingModel> _dllListings;
        bool _isLoading;
        string _searchTerm;
        private IDllListingModel _gacItem;
        string _assemblyName;
        Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        private string _progID;
        private string _clsId;

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, IEventAggregator aggregator, IAsyncWorker asyncWorker)
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
            if (Application.Current != null)
            {
                if (Application.Current.Dispatcher != null)
                {
                    DispatcherAction = Application.Current.Dispatcher.Invoke;
                }
            }
        }

        public IAsyncWorker AsyncWorker { get; set; }

        public ICommand RefreshCommand { get; set; }

        public IDllListingModel GacItem
        {
            get { return _gacItem; }
            set
            {
                _gacItem = value;
                OnPropertyChanged(() => GacItem);
            }
        }

        public Action<Action> DispatcherAction { get; set; }

        void PerformLoadAll(Action actionToPerform = null)
        {

            AsyncWorker.Start(() =>
            {
                IsLoading = true;
                var names = _updateManager.GetComDllListings(null).Select(input => new DllListingModel(_updateManager, input)).ToList();

                DispatcherAction.Invoke(() =>
                {
                    DllListings = new List<IDllListingModel>(names);
                    IsLoading = false;
                    if (DllListings != null && DllListings.Count > 1)
                    {
                        GacItem = DllListings[1];
                    }
                    actionToPerform?.Invoke();
                });
            }, () =>
            {

            }, exception =>
            {
                //if (exception.InnerException != null)
                //{
                //    exception = exception.InnerException;
                //}
                //TestMessage = exception.Message;
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
                foreach (var dllListingModel in DllListings)
                {
                    dllListingModel.Filter(searchTerm);
                }
                OnPropertyChanged(() => DllListings);
            }
        }

        public ICommand CancelCommand { get; set; }
        public Action CloseAction { get; set; }
        public List<IDllListingModel> DllListings
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

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : this(updateManager, aggregator, asyncWorker)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformLoadAll();
            _requestServiceNameViewModel = requestServiceNameViewModel;
            Item = ToModel();
        }

        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, Action<Action> dispatcherAction)
            : this(updateManager, aggregator, asyncWorker)
        {
            DispatcherAction = dispatcherAction;
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformLoadAll();
            _requestServiceNameViewModel = requestServiceNameViewModel;
            Item = ToModel();
        }

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, IEventAggregator aggregator, IComPluginSource pluginSource, IAsyncWorker asyncWorker)
            : this(updateManager, aggregator, asyncWorker)
        {
            VerifyArgument.IsNotNull("compluginSource", pluginSource);
            _pluginSource = pluginSource;
            SetupHeaderTextFromExisting();
            PerformLoadAll(() => FromModel(_pluginSource));

            ToItem();
        }

        public ManageComPluginSourceViewModel(IManageComPluginSourceModel updateManager, IEventAggregator aggregator, IComPluginSource pluginSource, IAsyncWorker asyncWorker, Action<Action> dispatcherAction)
            : this(updateManager, aggregator, asyncWorker)
        {
            DispatcherAction = dispatcherAction;
            VerifyArgument.IsNotNull("comPluginSource", pluginSource);
            _pluginSource = pluginSource;
            SetupHeaderTextFromExisting();
            PerformLoadAll(() => FromModel(_pluginSource));

            ToItem();
        }

        public ManageComPluginSourceViewModel()
             : base("ComPluginSource")
        {

        }

        public override void FromModel(IComPluginSource pluginSource)
        {
            var selectedDll = pluginSource;
            if (selectedDll != null)
            {
                var dllListingModel = DllListings.Find(model => model.ClsId == pluginSource.ClsId || model.ProgId == pluginSource.ProgId);
                dllListingModel.IsExpanded = true;
                SelectedDll = dllListingModel;
                dllListingModel.IsSelected = true;
            }

            Name = _pluginSource.Name;
            Path = _pluginSource.ProgId;
            ProgId = _pluginSource.ProgId;
            ClsId = _pluginSource.ClsId;
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
                if (value == null) return;
                _selectedDll = value;
                OnPropertyChanged(() => SelectedDll);
                if (SelectedDll != null)
                {
                    ClsId = SelectedDll.ClsId;
                    ProgId = SelectedDll.ProgId;
                    SelectedDll.IsExpanded = true;
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
                HeaderText = (_pluginSource == null ? ResourceName : _pluginSource.Name).Trim();
                Header = (_pluginSource == null ? ResourceName : _pluginSource.Name).Trim();
            }
            else
            {
                HeaderText = (_pluginSource == null ? ResourceName : _pluginSource.Name).Trim();
                Header = (_pluginSource == null ? ResourceName : _pluginSource.Name).Trim();
            }
        }

        public override bool CanSave()
        {
            return _selectedDll != null && !string.IsNullOrEmpty(ProgId) && HasChanged;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
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

        public string ProgId
        {
            get
            {
                return _progID;
            }
            set
            {
                _progID = value;
                OnPropertyChanged(_progID);
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
                OnPropertyChanged(_clsId);
            }
        }

        public override void Save()
        {
            if (_pluginSource == null)
            {
                var res = RequestServiceNameViewModel.ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {

                    ResourceName = RequestServiceNameViewModel.ResourceName.Name;
                    var src = ToModel();
                    src.Id = SelectedGuid;
                    src.ClsId = SelectedDll.ClsId;
                    Save(src);
                    src.ProgId = SelectedDll.ProgId;
                    _pluginSource = src;
                    ToItem();
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToModel();
                Save(src);
                _pluginSource = src;
                ToItem();
            }
            OnPropertyChanged(() => Header);
        }

        public string Path { get; set; }

        void ToItem()
        {
            Item = new ComPluginSourceDefinition
            {
                Id = _pluginSource.Id,
                Name = _pluginSource.Name,
                ProgId = _pluginSource.ProgId,
                ClsId = _pluginSource.ClsId
            };
            Name = _pluginSource.Name;
        }

        void Save(IComPluginSource source)
        {
            source.SelectedDll = new DllListing(_selectedDll);
            _updateManager.Save(source);
        }

        public sealed override IComPluginSource ToModel()
        {
            if (_pluginSource == null)
            {
                return new ComPluginSourceDefinition
                {
                    Name = ResourceName,
                    ClsId = _clsId,
                    ProgId = _progID
                };
            }
            _pluginSource.SelectedDll = _selectedDll;
            return _pluginSource;
        }

        public IRequestServiceNameViewModel RequestServiceNameViewModel
        {
            get
            {
                if (_requestServiceNameViewModel != null)
                {
                    _requestServiceNameViewModel.Wait();
                    if (_requestServiceNameViewModel.Exception == null)
                    {
                        return _requestServiceNameViewModel.Result;
                    }
                    // ReSharper disable once RedundantIfElseBlock
                    else
                    {
                        throw _requestServiceNameViewModel.Exception;
                    }
                }
                return null;
            }
            set { _requestServiceNameViewModel = new Task<IRequestServiceNameViewModel>(() => value); _requestServiceNameViewModel.Start(); }
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
            RequestServiceNameViewModel?.Dispose();
            Dispose(true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                }

                // Dispose unmanaged resources.
                _isDisposed = true;
            }
        }
    }
}