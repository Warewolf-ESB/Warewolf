using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core;
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels
{
    public class DLLChooser : BindableBase, IDLLChooser
    {
        private readonly IManagePluginSourceModel _updateManager;
        private bool _isLoading;
        private string _searchTerm;
        private List<IDllListingModel> _allDllListingModels;
        private IDllListingModel _selectedDll;
        private string _assemblyName;
        private List<IDllListingModel> _dllListingModels;
        private IChooseDLLView _view;

        public DLLChooser(IManagePluginSourceModel updateManager)
        {
            _updateManager = updateManager;
            SearchTerm = string.Empty;
            ClearSearchTextCommand = new DelegateCommand(o => SearchTerm = "");
            CancelCommand = new DelegateCommand(o =>
            {
                SelectedDll = null;
                SearchTerm = string.Empty;
                _view?.RequestClose();
            });
            SelectCommand = new DelegateCommand(o => _view?.RequestClose());
        }

        public IDllListingModel GetGacDLL()
        {
            SelectedDll = null;
            AssemblyName = string.Empty;
            PerformLoadAll();
            if (AllDllListingModels != null && AllDllListingModels.Count > 1)
            {
                DllListingModels = new List<IDllListingModel> { AllDllListingModels[1] };
            }
            _view = CustomContainer.GetInstancePerRequestType<IChooseDLLView>();
            _view.ShowView(this);
            return SelectedDll;
        }

        public IDllListingModel GetFileSystemDLL()
        {
            SelectedDll = null;
            AssemblyName = string.Empty;
            PerformLoadAll();
            if (AllDllListingModels != null && AllDllListingModels.Count > 1)
            {
                DllListingModels = AllDllListingModels[0].Children.ToList();
            }
            _view = CustomContainer.GetInstancePerRequestType<IChooseDLLView>();
            _view.ShowView(this);
            return SelectedDll;
        }

        public List<IDllListingModel> DllListingModels
        {
            get
            {
                return _dllListingModels;
            }
            set
            {
                _dllListingModels = value;
                OnPropertyChanged(() => DllListingModels);
            }
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        IDllListingModel ExpandChild(string dir, ObservableCollection<IDllListingModel> children)
        {
            var dllListingModel = children.FirstOrDefault(model => model.Name.StartsWith(dir));
            if (dllListingModel != null)
            {
                dllListingModel.IsExpanded = true;
            }
            return dllListingModel;
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
        public List<IDllListingModel> AllDllListingModels
        {
            get
            {
                return _allDllListingModels;
            }
            set
            {
                _allDllListingModels = value;
                OnPropertyChanged(() => AllDllListingModels);
            }
        }
        public ICommand CancelCommand { get; set; }
        public ICommand SelectCommand { get; set; }
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
                if (SelectedDll != null)
                {
                    SelectedDll.IsExpanded = true;
                    AssemblyName = SelectedDll.FullName;
                }
                ViewModelUtils.RaiseCanExecuteChanged(SelectCommand);
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
                if (_assemblyName != value)
                {
                    _assemblyName = value;
                    if (!string.IsNullOrEmpty(_assemblyName))
                    {
                        if (!_assemblyName.StartsWith("GAC"))
                        {
                            SelectDllFromUsingAssemblyName();
                        }
                    }
                    else
                    {
                        SelectedDll = null;
                    }
                    OnPropertyChanged(() => AssemblyName);
                    ViewModelUtils.RaiseCanExecuteChanged(SelectCommand);
                }
            }
        }

        private void PerformLoadAll()
        {
            var names = _updateManager.GetDllListings(null).Select(input => new DllListingModel(_updateManager, input)).ToList();
            AllDllListingModels = new List<IDllListingModel>(names);
        }

        private void PerformSearch(string searchTerm)
        {
            if (AllDllListingModels != null)
            {
                foreach (var dllListingModel in AllDllListingModels)
                {
                    dllListingModel.Filter(searchTerm);
                }
                OnPropertyChanged(() => AllDllListingModels);
            }
        }

        private void SelectDllFromUsingAssemblyName()
        {
            if (_selectedDll != null) return;
            if (_assemblyName == null) return;
            if (!_assemblyName.StartsWith("GAC"))
                if (!File.Exists(_assemblyName)) return;
            var dll = new FileInfo(_assemblyName);
            if (dll.Extension != ".dll") return;
            var fileListing = new FileListing { Name = dll.Name, FullName = dll.FullName };
            _selectedDll = new DllListingModel(_updateManager, fileListing);
        }
    }
}
