using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Security;
using Dev2.Controller;
using Dev2.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Resource.Errors;

namespace Warewolf.Studio.ViewModels
{
    public class RequestServiceNameViewModel : BindableBase, IRequestServiceNameViewModel
    {
        private string _name;
        private string _errorMessage;
        private ResourceName _resourceName;
        private IRequestServiceNameView _view;

        private string _selectedPath;
        private bool _hasLoaded;
        string _header;
        private IEnvironmentViewModel _environmentViewModel;
        IExplorerItemViewModel _explorerItemViewModel;
        private bool _isDuplicate;
        private bool _fixReferences;
        MessageBoxResult ViewResult { get; set; }

        // ReSharper disable once EmptyConstructor
        public RequestServiceNameViewModel()
        {
        }
#pragma warning disable 1998
#pragma warning disable 1998
        private async Task<IRequestServiceNameViewModel> InitializeAsync(IEnvironmentViewModel environmentViewModel, string selectedPath, string header, IExplorerItemViewModel explorerItemViewModel = null)
#pragma warning restore 1998
#pragma warning restore 1998
        {
            _environmentViewModel = environmentViewModel;
            _environmentViewModel.Connect();
            _selectedPath = selectedPath;
            _header = header;
            _explorerItemViewModel = explorerItemViewModel;
            OkCommand = new DelegateCommand(SetServiceName, () => string.IsNullOrEmpty(ErrorMessage) && HasLoaded);
            DuplicateCommand = new DelegateCommand(CallDuplicateService, CanDuplicate);
            CancelCommand = new DelegateCommand(CloseView, CanClose);
            Name = header;
            IsDuplicate = explorerItemViewModel != null;
            return this;
        }

        private bool CanDuplicate()
        {
            var b = _explorerItemViewModel != null && string.IsNullOrEmpty(ErrorMessage) && HasLoaded && !IsDuplicating;
            return b;
        }

        private bool CanClose()
        {
            if (IsDuplicate)
            {
                return !IsDuplicating;
            }
            return true;
        }

        readonly IEnvironmentConnection _lazyCon = EnvironmentRepository.Instance.ActiveEnvironment?.Connection;
        ICommunicationController _lazyComs = new CommunicationController { ServiceName = "DuplicateResourceService" };

        private void CallDuplicateService()
        {
            try
            {
                IsDuplicating = true;

                if (_explorerItemViewModel.IsFolder)
                {
                    _lazyComs = new CommunicationController { ServiceName = "DuplicateFolderService" };
                    _lazyComs.AddPayloadArgument("FixRefs", FixReferences.ToString());
                }
                _lazyComs.AddPayloadArgument("NewResourceName", Name);

                if (!_explorerItemViewModel.IsFolder)
                {
                    _lazyComs.AddPayloadArgument("ResourceID", _explorerItemViewModel.ResourceId.ToString());
                }

                _lazyComs.AddPayloadArgument("sourcePath", _explorerItemViewModel.ResourcePath);
                _lazyComs.AddPayloadArgument("destinationPath", Path);

                var executeCommand = _lazyComs.ExecuteCommand<ResourceCatalogDuplicateResult>(_lazyCon ?? EnvironmentRepository.Instance.ActiveEnvironment?.Connection, GlobalConstants.ServerWorkspaceID);
                if (executeCommand == null)
                {
                    var environmentViewModel = SingleEnvironmentExplorerViewModel.Environments.FirstOrDefault();
                    environmentViewModel?.RefreshCommand.Execute(null);
                    CloseView();
                    ViewResult = MessageBoxResult.OK;
                }
                else
                {
                    if (executeCommand.Status == ExecStatus.Success)
                    {
                        var duplicatedItems = executeCommand.DuplicatedItems;
                        var environmentViewModel = SingleEnvironmentExplorerViewModel.Environments.FirstOrDefault();
                        var parentItem = SelectedItem ?? _explorerItemViewModel.Parent;
                        var childItems = environmentViewModel?.CreateExplorerItemModels(duplicatedItems, _explorerItemViewModel.Server, parentItem, false, false);
                        var explorerItemViewModels = parentItem.Children;
                        explorerItemViewModels.AddRange(childItems);
                        parentItem.Children = explorerItemViewModels;
                        CloseView();
                        ViewResult = MessageBoxResult.OK;
                    }
                    else
                    {
                        ErrorMessage = executeCommand.Message;
                    }
                }
            }
            catch (Exception)
            {
                //
            }
            finally
            {
                IsDuplicating = false;
            }
        }

        public bool FixReferences
        {
            get
            {
                return _fixReferences;
            }
            // ReSharper disable once UnusedMember.Global
            set
            {
                _fixReferences = value;
                OnPropertyChanged(() => FixReferences);
            }
        }

        void SingleEnvironmentExplorerViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                ValidateName();

                HasLoaded = false;

                if (SingleEnvironmentExplorerViewModel?.SelectedEnvironment != null)
                {
                    HasLoaded = true;
                }
                else if (SingleEnvironmentExplorerViewModel?.SelectedItem != null && SingleEnvironmentExplorerViewModel.SelectedItem.IsFolder)
                {
                    HasLoaded = true;
                }
                if (SingleEnvironmentExplorerViewModel?.SelectedItem != null && !SingleEnvironmentExplorerViewModel.SelectedItem.IsFolder)
                {
                    HasLoaded = false;
                    ErrorMessage = ErrorResource.SaveToFolderOrRootOnly;
                }
            }
        }

        bool HasLoaded
        {
            get
            {
                return _hasLoaded;
            }
            set
            {
                _hasLoaded = value;
                RaiseCanExecuteChanged();
            }
        }

        public static Task<IRequestServiceNameViewModel> CreateAsync(IEnvironmentViewModel environmentViewModel, string selectedPath, string header, IExplorerItemViewModel explorerItemViewModel = null)
        {
            if (environmentViewModel == null)
            {
                throw new ArgumentNullException(nameof(environmentViewModel));
            }
            var ret = new RequestServiceNameViewModel();
            return ret.InitializeAsync(environmentViewModel, selectedPath, header, explorerItemViewModel);
        }


        private void CloseView()
        {
            _view.RequestClose();
            ViewResult = MessageBoxResult.Cancel;
            SingleEnvironmentExplorerViewModel = null;
        }

        private void SetServiceName()
        {
            var path = Path;
            if (!string.IsNullOrEmpty(path))
            {
                path = path.TrimStart('\\') + "\\";
            }
            _resourceName = new ResourceName(path, Name);
            ViewResult = MessageBoxResult.OK;
            _view.RequestClose();
        }

        private string Path
        {
            get
            {
                var selectedItem = SelectedItem;
                if (selectedItem != null)
                {
                    var parent = selectedItem.Parent;
                    var parentNames = new List<string>();
                    while (parent != null)
                    {
                        if (parent.ResourceType != "ServerSource")
                        {
                            parentNames.Add(parent.ResourceName);
                        }
                        parent = parent.Parent;
                    }
                    var path = "";
                    if (parentNames.Count > 0)
                    {
                        for (var index = parentNames.Count; index > 0; index--)
                        {
                            var parentName = parentNames[index - 1];
                            path = path + "\\" + parentName;
                        }
                    }
                    if (selectedItem.ResourceType == "Folder")
                    {
                        path = path + "\\" + selectedItem.ResourceName;
                    }
                    return path;
                }
                return "";
            }

        }
        private IExplorerTreeItem _treeItem;
        private bool _isDuplicating;
        private IExplorerTreeItem SelectedItem
        {
            get
            {
                _treeItem = SingleEnvironmentExplorerViewModel.SelectedItem;
                return _treeItem;
            }

            // ReSharper disable once UnusedMember.Local
            set
            {
                _treeItem = value;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            var command = OkCommand as DelegateCommand;
            command?.RaiseCanExecuteChanged();
            var dupCommad = DuplicateCommand as DelegateCommand;
            dupCommad?.RaiseCanExecuteChanged();
        }

        public MessageBoxResult ShowSaveDialog()
        {
            _view = CustomContainer.GetInstancePerRequestType<IRequestServiceNameView>();

            SingleEnvironmentExplorerViewModel = new SingleEnvironmentExplorerViewModel(_environmentViewModel,
                Guid.Empty, false);
            SingleEnvironmentExplorerViewModel.PropertyChanged += SingleEnvironmentExplorerViewModelPropertyChanged;
            SingleEnvironmentExplorerViewModel.SearchText = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(_selectedPath))
                {
                    _environmentViewModel.SelectItem(_selectedPath, b =>
                    {
                        _environmentViewModel.SelectAction(b);
                        b.IsSelected = true;
                    });
                }
                _environmentViewModel.IsSaveDialog = true;
                _environmentViewModel.Children?.Flatten(model => model.Children)
                    .Apply(model => model.IsSaveDialog = true);
            }
            catch (Exception)
            {
                //
            }

            HasLoaded = true;
            ValidateName();
            _view.DataContext = this;
            _view.ShowView();

            _environmentViewModel.Filter(string.Empty);
            _environmentViewModel.IsSaveDialog = false;
            _environmentViewModel.Children?.Flatten(model => model.Children)
                .Apply(model => model.IsSaveDialog = false);

            var windowsGroupPermission = _environmentViewModel.Server?.Permissions?[0];
            if (windowsGroupPermission != null)
                _environmentViewModel.SetPropertiesForDialogFromPermissions(windowsGroupPermission);

            var permissions = _environmentViewModel.Server?.GetPermissions(_environmentViewModel.ResourceId);
            if (permissions != null)
            {
                if (_environmentViewModel.Children != null)
                    foreach (var explorerItemViewModel in _environmentViewModel.Children.Flatten(model => model.Children))
                    {
                        explorerItemViewModel.SetPermissions((Permissions) permissions);
                    }
            }

            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel?.ExplorerViewModel != null)
                mainViewModel.ExplorerViewModel.SearchText = string.Empty;
            return ViewResult;
        }

        public ResourceName ResourceName => _resourceName;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(() => Name);
                ValidateName();
            }
        }

        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged(() => Header);
            }
        }
        public bool IsDuplicate
        {
            get
            {
                return _isDuplicate;
            }
            set
            {
                _isDuplicate = value;
                OnPropertyChanged(() => IsDuplicate);
            }
        }

        void ValidateName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                ErrorMessage = ErrorResource.CannotBeNull;
            }
            else if (NameHasInvalidCharacters(Name))
            {
                ErrorMessage = ErrorResource.ContainsInvalidCharecters;
            }
            else if (Name.Trim() != Name)
            {
                ErrorMessage = ErrorResource.ContainsLeadingOrTrailingWhitespace;
            }
            else if (HasDuplicateName(Name))
            {
                ErrorMessage = ErrorResource.ItemWithNameAlreadyExists;
            }
            else
            {
                ErrorMessage = "";
            }
        }

        private bool HasDuplicateName(string requestedServiceName)
        {
            if (SingleEnvironmentExplorerViewModel != null)
            {
                var explorerTreeItem = SingleEnvironmentExplorerViewModel.SelectedItem;
                if (explorerTreeItem != null && explorerTreeItem.ResourceType == "Folder")
                {
                    return explorerTreeItem.Children.Any(model => model.ResourceName.ToLower() == requestedServiceName.ToLower() && model.ResourceType != "Folder");
                }
                if (SingleEnvironmentExplorerViewModel.Environments.FirstOrDefault() != null)
                {
                    var explorerItemViewModels = SingleEnvironmentExplorerViewModel.Environments.First().Children;
                    if (IsDuplicate)
                    {
                        return explorerItemViewModels != null && explorerItemViewModels.Any(model => requestedServiceName != null && model.ResourceName != null && model.ResourceName.ToLower() == requestedServiceName.ToLower());
                    }
                    return explorerItemViewModels != null && explorerItemViewModels.Any(model => requestedServiceName != null && model.ResourceName != null && model.ResourceName.ToLower() == requestedServiceName.ToLower() && model.ResourceType != "Folder");
                }
            }
            return false;
        }

        private bool NameHasInvalidCharacters(string name)
        {
            return Regex.IsMatch(name, @"[^a-zA-Z0-9._\s-]");
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(() => ErrorMessage);
                RaiseCanExecuteChanged();
            }
        }

        public ICommand OkCommand { get; set; }
        public ICommand DuplicateCommand { get; set; }

        public ICommand CancelCommand { get; private set; }

        public IExplorerViewModel SingleEnvironmentExplorerViewModel { get; set; }
        public bool IsDuplicating
        {
            get
            {
                return _isDuplicating;
            }
            set
            {
                _isDuplicating = value;
                OnPropertyChanged(() => IsDuplicating);
                ViewModelUtils.RaiseCanExecuteChanged(DuplicateCommand);
                ViewModelUtils.RaiseCanExecuteChanged(CancelCommand);
            }
        }

        public void Dispose()
        {
            SingleEnvironmentExplorerViewModel?.Dispose();
            _environmentViewModel?.Dispose();
        }
    }
}