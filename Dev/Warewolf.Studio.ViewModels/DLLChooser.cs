#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core;


namespace Warewolf.Studio.ViewModels
{
    public class DLLChooser : BindableBase, IDLLChooser
    {
        readonly IManagePluginSourceModel _updateManager;
        bool _isLoading;
        string _searchTerm;
        List<IDllListingModel> _allDllListingModels;
        IDllListingModel _selectedDll;
        string _assemblyName;
        List<IDllListingModel> _dllListingModels;
        IChooseDLLView _view;
        string _filterTooltip;
        string _filesTooltip;
        string _selectTooltip;

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
            FilterTooltip = Resources.Languages.Tooltips.ManageEmailAttachmentFilterTooltip;
            FilesTooltip = Resources.Languages.Tooltips.ManageEmailAttachmentDriveNameTooltip;
            SelectTooltip = Resources.Languages.Tooltips.ManageEmailAttachmentAttachTooltip;
        }

        public IDllListingModel GetGacDLL()
        {
            SelectedDll = null;
            AssemblyName = string.Empty;
            SearchTerm = string.Empty;
            PerformLoadAll();
            if (AllDllListingModels != null && AllDllListingModels.Count > 1)
            {
                DllListingModels = new List<IDllListingModel> { AllDllListingModels[1] };
            }
            if (DllListingModels != null)
            {
                foreach (var dllListingModel in DllListingModels.Where(model => model.ChildrenCount > 0))
                {
                    dllListingModel.IsExpanded = true;
                }
            }
            FilterTooltip = Resources.Languages.Tooltips.ManagePluginSourceFilterGACTooltip;
            FilesTooltip = Resources.Languages.Tooltips.ManagePluginSourceFilesGACTooltip;
            SelectTooltip = Resources.Languages.Tooltips.ManagePluginSourceSelectGACTooltip;
            _view = CustomContainer.GetInstancePerRequestType<IChooseDLLView>();
            _view.ShowView(this);
            return SelectedDll;
        }

        public IDllListingModel GetFileSystemDLL()
        {
            SelectedDll = null;
            AssemblyName = string.Empty;
            SearchTerm = string.Empty;
            PerformLoadAll();
            if (AllDllListingModels != null && AllDllListingModels.Count > 1)
            {
                DllListingModels = AllDllListingModels[0].Children.ToList();
            }
            FilterTooltip = Resources.Languages.Tooltips.ManagePluginSourceFilterAssemblyTooltip;
            FilesTooltip = Resources.Languages.Tooltips.ManagePluginSourceFilesAssemblyTooltip;
            SelectTooltip = Resources.Languages.Tooltips.ManagePluginSourceSelectAssemblyTooltip;
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
        public string FilterTooltip
        {
            get
            {
                return _filterTooltip;
            }
            set
            {
                if (!value.Equals(_filterTooltip))
                {
                    _filterTooltip = value;
                    OnPropertyChanged(() => FilterTooltip);
                }
            }
        }
        public string FilesTooltip
        {
            get
            {
                return _filesTooltip;
            }
            set
            {
                if (!value.Equals(_filesTooltip))
                {
                    _filesTooltip = value;
                    OnPropertyChanged(() => FilesTooltip);
                }
            }
        }
        public string SelectTooltip
        {
            get
            {
                return _selectTooltip;
            }
            set
            {
                if (!value.Equals(_selectTooltip))
                {
                    _selectTooltip = value;
                    OnPropertyChanged(() => SelectTooltip);
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

        void PerformLoadAll()
        {
            var names = _updateManager.GetDllListings(null).Select(input => new DllListingModel(_updateManager, input)).ToList();
            AllDllListingModels = new List<IDllListingModel>(names);
        }

        void PerformSearch(string searchTerm)
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

        void SelectDllFromUsingAssemblyName()
        {
            if (_selectedDll != null)
            {
                return;
            }

            if (_assemblyName == null)
            {
                return;
            }

            if (!_assemblyName.StartsWith("GAC") && !File.Exists(_assemblyName))
            {
                return;
            }


            var dll = new FileInfo(_assemblyName);
            if (dll.Extension != ".dll")
            {
                return;
            }

            var fileListing = new FileListing { Name = dll.Name, FullName = dll.FullName };
            _selectedDll = new DllListingModel(_updateManager, fileListing);
        }
    }
}
