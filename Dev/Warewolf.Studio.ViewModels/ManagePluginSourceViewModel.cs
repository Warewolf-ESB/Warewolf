using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Core;
using Warewolf.Studio.Core;
using Warewolf.Studio.Models.Help;

namespace Warewolf.Studio.ViewModels
{
    public class ManagePluginSourceViewModel : SourceBaseImpl<IPluginSource>, IDisposable, IManagePluginSourceViewModel
    {
        IDllListingModel _selectedDll;
        readonly IManagePluginSourceModel _updateManager ;
        readonly IEventAggregator _aggregator;
        IPluginSource _pluginSource;
        string _resourceName;
        readonly string _warewolfserverName;
        string _headerText;
        private bool _isDisposed;
        List<IDllListingModel> _dllListings;
        bool _isLoading;
        string _searchTerm;
        private IDllListingModel _gacItem;

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ManagePluginSourceViewModel(IManagePluginSourceModel updateManager, IEventAggregator aggregator):base(ResourceType.PluginSource)
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;
            _aggregator = aggregator;

            HeaderText = Resources.Languages.Core.PluginSourceNewHeaderLabel;
            Header = Resources.Languages.Core.PluginSourceNewHeaderLabel;
            OkCommand = new DelegateCommand(Save,CanSave);
            CancelCommand = new DelegateCommand(() => CloseAction.Invoke());
            ClearSearchTextCommand = new DelegateCommand(() => SearchTerm = "");
            RefreshCommand = new DelegateCommand(PerformLoadAll);
            // ReSharper disable MaximumChainedReferences
            PerformLoadAll();
            // ReSharper restore MaximumChainedReferences
            _warewolfserverName = updateManager.ServerName;
        }

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

        void PerformLoadAll()
        {
            new Task(() =>
            {
                IsLoading = true;
                var names = _updateManager.GetDllListings(null).Select(input => new DllListingModel(_updateManager, input)).ToList();
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    DllListings = new List<IDllListingModel>(names);
                    IsLoading = false;
                    if (DllListings != null && DllListings.Count > 1)
                    {
                        GacItem = DllListings[1];
                    }
                });
            }).Start();
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
                if(!value.Equals(_searchTerm))
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
        public ManagePluginSourceViewModel(IManagePluginSourceModel updateManager, IRequestServiceNameViewModel requestServiceNameViewModel, IEventAggregator aggregator)
            : this(updateManager, aggregator)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);

            RequestServiceNameViewModel = requestServiceNameViewModel;

        }

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ManagePluginSourceViewModel(IManagePluginSourceModel updateManager, IEventAggregator aggregator, IPluginSource pluginSource)
            : this(updateManager,  aggregator)
        {
            VerifyArgument.IsNotNull("pluginSource", pluginSource);
            _pluginSource = pluginSource;
            SetupHeaderTextFromExisting();
            FromSource(pluginSource);
        }

        void FromSource(IPluginSource pluginSource)
        {
            SelectedDll = new DllListingModel(_updateManager,pluginSource.SelectedDll);
        }

        public IDllListingModel SelectedDll
        {
            get
            {
                return _selectedDll;
            }
            set
            {
                _selectedDll = value;
                OnPropertyChanged(() => SelectedDll);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        void SetupHeaderTextFromExisting()
        {
            HeaderText = Resources.Languages.Core.PluginSourceEditHeaderLabel + _warewolfserverName.Trim() + "\\" + (_pluginSource == null ? ResourceName : _pluginSource.Name).Trim();
            Header = ((_pluginSource == null ? ResourceName : _pluginSource.Name));
        }

        bool CanSave()
        {
            return _selectedDll!=null&& !string.IsNullOrEmpty(_selectedDll.FullName) &&_selectedDll.FullName.EndsWith(".dll");
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var helpDescriptor = new HelpDescriptor("",helpText,null);
            VerifyArgument.IsNotNull("helpDescriptor", helpDescriptor);
            _aggregator.GetEvent<HelpChangedEvent>().Publish(helpDescriptor);

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
                if(!String.IsNullOrEmpty(value))
                {
                    SetupHeaderTextFromExisting();
                }
                OnPropertyChanged(_resourceName);
            }
        }

        void Save()
        {
            if(_pluginSource == null)
            {
                var res = RequestServiceNameViewModel.ShowSaveDialog();
               
                if(res==MessageBoxResult.OK)
                {
                    ResourceName = RequestServiceNameViewModel.ResourceName.Name;
                    var src = ToModel();
                    src.Path = RequestServiceNameViewModel.ResourceName.Path ?? RequestServiceNameViewModel.ResourceName.Name;
                    Save(src);
                    _pluginSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                Save(ToModel());
            }
        }

        void Save(IPluginSource source)
        {
            _updateManager.Save(source);
        }
        
 
        public override IPluginSource ToModel()
        {
            if(_pluginSource == null)
                return new PluginSourceDefinition
                {
                    Name = ResourceName,
                    SelectedDll = _selectedDll,
                    Id =  _pluginSource==null?Guid.NewGuid():_pluginSource.Id
                };
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                _pluginSource.SelectedDll = _selectedDll;
                return _pluginSource;

            }
        }
        IRequestServiceNameViewModel RequestServiceNameViewModel { get; set; }
       
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

        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
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