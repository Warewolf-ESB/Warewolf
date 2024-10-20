#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ManagePluginSourceViewModel : SourceBaseImpl<IPluginSource>, IManagePluginSourceViewModel
    {
        readonly IManagePluginSourceModel _updateManager;
        IPluginSource _pluginSource;
        string _resourceName;
        readonly string _warewolfserverName;
        string _headerText;
        bool _isDisposed;
        Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        IDllListingModel _selectedDll;
        string _gacAssemblyName;
        string _fileSystemAssemblyName;
        string _configFilePath;
        bool _canSelectConfigFiles;

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public ManagePluginSourceViewModel(IManagePluginSourceModel updateManager, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : base("PluginSource")
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;
            DllChooser = new DLLChooser(updateManager);
            HeaderText = Resources.Languages.Core.PluginSourceNewHeaderLabel;
            Header = Resources.Languages.Core.PluginSourceNewHeaderLabel;
            OkCommand = new DelegateCommand(o => Save(), o => CanSave());
            FileSystemAssemblyName = string.Empty;
            ConfigFilePath = string.Empty;
            GACAssemblyName = string.Empty;
            ChooseFileSystemDLLCommand = new DelegateCommand(o =>
            {
                var dll = DllChooser.GetFileSystemDLL();
                if (dll != null)
                {
                    FileSystemAssemblyName = dll.FullName;
                }
            });
            ChooseGACDLLCommand = new DelegateCommand(o =>
            {
                var dll = DllChooser.GetGacDLL();
                if (dll != null)
                {
                    GACAssemblyName = dll.FullName;
                }
            });

            ChooseConfigFileCommand = new DelegateCommand(o =>
            {
                var fileChooser = CustomContainer.GetInstancePerRequestType<IFileChooserView>();
                fileChooser.ShowView(false);
                if (fileChooser.DataContext is FileChooser vm && vm.Result == MessageBoxResult.OK)
                {
                    var selectedFiles = vm.GetAttachments();
                    if (selectedFiles != null && selectedFiles.Count > 0)
                    {
                        ConfigFilePath = selectedFiles[0];
                    }
                }
            });

            CancelCommand = new DelegateCommand(o => CloseAction.Invoke());

            _warewolfserverName = updateManager.ServerName;
        }

        public ICommand ChooseGACDLLCommand { get; set; }
        public ICommand ChooseFileSystemDLLCommand { get; set; }
        public ICommand ChooseConfigFileCommand { get; set; }

        public ICommand CancelCommand { get; set; }
        public Action CloseAction { get; set; }

        public ManagePluginSourceViewModel(IManagePluginSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : this(updateManager, aggregator, asyncWorker)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;
            Item = ToModel();
        }

        public ManagePluginSourceViewModel(IManagePluginSourceModel updateManager, IEventAggregator aggregator, IPluginSource pluginSource, IAsyncWorker asyncWorker)
            : this(updateManager, aggregator, asyncWorker)
        {
            VerifyArgument.IsNotNull("pluginSource", pluginSource);
            asyncWorker.Start(() => updateManager.FetchSource(pluginSource.Id), source =>
            {
                _pluginSource = source;
                _pluginSource.Path = pluginSource.Path;
                ToItem();
                FromModel(_pluginSource);
                SetupHeaderTextFromExisting();
            });
        }

        public ManagePluginSourceViewModel() : base("PluginSource")
        {
        }

        public override void FromModel(IPluginSource source)
        {
            Name = source.Name;
            Path = source.Path;
            FileSystemAssemblyName = source.FileSystemAssemblyName;
            ConfigFilePath = source.ConfigFilePath;
            GACAssemblyName = source.GACAssemblyName;
        }

        public string FileSystemAssemblyName
        {
            get
            {
                return _fileSystemAssemblyName;
            }
            set
            {
                _fileSystemAssemblyName = value;
                CanSelectConfigFiles = !string.IsNullOrWhiteSpace(_fileSystemAssemblyName) && _fileSystemAssemblyName.EndsWith(".dll");
                if (!string.IsNullOrEmpty(_fileSystemAssemblyName))
                {
                    GACAssemblyName = string.Empty;
                }
                OnPropertyChanged(() => FileSystemAssemblyName);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public string ConfigFilePath
        {
            get
            {
                return _configFilePath;
            }
            set
            {
                _configFilePath = value;
                OnPropertyChanged(() => ConfigFilePath);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public bool CanSelectConfigFiles
        {
            get { return _canSelectConfigFiles; }
            set
            {
                _canSelectConfigFiles = value;
                OnPropertyChanged(() => CanSelectConfigFiles);
            }
        }

        public string GACAssemblyName
        {
            get
            {
                return _gacAssemblyName;
            }
            set
            {
                _gacAssemblyName = value;
                if (!string.IsNullOrEmpty(_gacAssemblyName))
                {
                    FileSystemAssemblyName = string.Empty;
                    ConfigFilePath = string.Empty;
                }
                OnPropertyChanged(() => GACAssemblyName);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
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
                _selectedDll = value;
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
            var canSave = HasChanged;
            if (canSave)
            {
                canSave = (!string.IsNullOrEmpty(FileSystemAssemblyName) && FileSystemAssemblyName.EndsWith(".dll")) ||
                          (!string.IsNullOrEmpty(GACAssemblyName) && GACAssemblyName.StartsWith("GAC:"));
            }

            return canSave;
        }

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
                    src.Path = GetRequestServiceNameViewModel().ResourceName.Path ?? GetRequestServiceNameViewModel().ResourceName.Name;
                    Save(src);
                    if (GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel != null)
                    {
                        AfterSave(GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                    }

                    Path = src.Path;
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
            Item = new PluginSourceDefinition
            {
                Id = _pluginSource.Id,
                Name = _pluginSource.Name,
                Path = _pluginSource.Path,
                FileSystemAssemblyName = _pluginSource.FileSystemAssemblyName,
                ConfigFilePath = _pluginSource.ConfigFilePath,
                GACAssemblyName = _pluginSource.GACAssemblyName
            };
        }

        void Save(IPluginSource source)
        {
            source.GACAssemblyName = FileSystemAssemblyName;
            source.ConfigFilePath = ConfigFilePath;
            source.GACAssemblyName = GACAssemblyName;
            _updateManager.Save(source);
        }

        public sealed override IPluginSource ToModel()
        {
            if(Item == null)
            {
                Item = ToSource();
                return Item;
            }
            return new PluginSourceDefinition
            {
                Name = ResourceName,
                Path = Path,
                FileSystemAssemblyName = _fileSystemAssemblyName,
                ConfigFilePath = _configFilePath,
                GACAssemblyName = _gacAssemblyName
            };
        }

        IPluginSource ToSource()
        {
            if (_pluginSource == null)
            {
                return ToNewSource();
            }

            else
            {
                _pluginSource.FileSystemAssemblyName = FileSystemAssemblyName;
                _pluginSource.ConfigFilePath = ConfigFilePath;
                _pluginSource.GACAssemblyName = GACAssemblyName;
                return _pluginSource;
            }
        }

        IPluginSource ToNewSource() => new PluginSourceDefinition
        {
            Name = ResourceName,
            Path = Path,
            FileSystemAssemblyName = _fileSystemAssemblyName,
            ConfigFilePath = _configFilePath,
            GACAssemblyName = _gacAssemblyName,
            Id = _pluginSource?.Id ?? Guid.NewGuid()
        };

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
        public DLLChooser DllChooser { get; }

        protected override void OnDispose()
        {
            GetRequestServiceNameViewModel()?.Dispose();
            DisposeManagePluginSourceViewModel(true);
        }
        
        void DisposeManagePluginSourceViewModel(bool disposing)
        {
            if (!_isDisposed && !disposing)
            {
                _isDisposed = true;
            }
        }
    }
}
