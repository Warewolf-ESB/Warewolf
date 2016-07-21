using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Resource.Errors;

#pragma warning disable 1998
namespace Warewolf.Studio.ViewModels
{
    public class RequestServiceNameViewModel : BindableBase, IRequestServiceNameViewModel
    {
        private string _name;
        private string _errorMessage;
        private ResourceName _resourceName;
        private IRequestServiceNameView _view;

        string _selectedPath;
        private bool _hasLoaded;
        string _header;
        private IEnvironmentViewModel _environmentViewModel;
        MessageBoxResult ViewResult { get; set; }

        // ReSharper disable once EmptyConstructor
        public RequestServiceNameViewModel()
        {
        }

        /// <exception cref="ArgumentNullException"><paramref name="environmentViewModel"/> is <see langword="null" />.</exception>

#pragma warning disable 1998
#pragma warning disable 1998
        private async Task<IRequestServiceNameViewModel> InitializeAsync(IEnvironmentViewModel environmentViewModel,  string selectedPath, string header)
#pragma warning restore 1998
#pragma warning restore 1998
        {
            _environmentViewModel = environmentViewModel;
            _environmentViewModel.Connect();
            _selectedPath = selectedPath;
            _header = header;
          
            OkCommand = new DelegateCommand(SetServiceName, () => string.IsNullOrEmpty(ErrorMessage) && HasLoaded);
            CancelCommand = new DelegateCommand(CloseView);
            Name = "";
            environmentViewModel.CanShowServerVersion = false;
            _environmentViewModel = environmentViewModel;
            return this;
        }

        void SingleEnvironmentExplorerViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
            {
                ValidateName();
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

        public static Task<IRequestServiceNameViewModel> CreateAsync(IEnvironmentViewModel environmentViewModel, string selectedPath, string header)
        {
            if (environmentViewModel == null)
        {
                throw new ArgumentNullException(nameof(environmentViewModel));
            }
            var ret = new RequestServiceNameViewModel();
            return ret.InitializeAsync(environmentViewModel, selectedPath, header);
        }

        public Guid IdToSave { get; set; }

        private void CloseView()
        {
            _view.RequestClose();
            SingleEnvironmentExplorerViewModel = null;
        }

        private void SetServiceName()
        {
            var path = GetPath();
            if (!string.IsNullOrEmpty(path))
            {
                path = path.TrimStart('\\') + "\\";
            }
            _resourceName = new ResourceName(path, Name);
            ViewResult = MessageBoxResult.OK;
            _view.RequestClose();
        }

        private string GetPath()
        {
            var selectedItem = SingleEnvironmentExplorerViewModel.SelectedItem;
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

        private void RaiseCanExecuteChanged()
        {
            var command = OkCommand as DelegateCommand;
            command?.RaiseCanExecuteChanged();
        }

        public MessageBoxResult ShowSaveDialog()
        {
            _view = CustomContainer.GetInstancePerRequestType<IRequestServiceNameView>();
            _environmentViewModel.LoadDialog(_selectedPath).ContinueWith(a =>
            {
                if (!string.IsNullOrEmpty(_selectedPath))
                {
                    _environmentViewModel.SelectItem(_selectedPath, b =>
                    {
                        _environmentViewModel.SelectAction(b);
                        b.IsSelected = true;
                    });
                }
                HasLoaded = a.Result;
                ValidateName();
            },TaskContinuationOptions.ExecuteSynchronously);
            SingleEnvironmentExplorerViewModel = new SingleEnvironmentExplorerViewModel(_environmentViewModel, Guid.Empty, false);
            SingleEnvironmentExplorerViewModel.PropertyChanged += SingleEnvironmentExplorerViewModelPropertyChanged;
            _view.DataContext = this;
            _view.ShowView();

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

        void ValidateName()
        {
            if (string.IsNullOrEmpty(Name))
            {
                ErrorMessage = string.Format(ErrorResource.CannotBeNull, "'Name'");
            }
            else if (NameHasInvalidCharacters(Name))
            {
                ErrorMessage = string.Format(ErrorResource.ContainsInvalidCharecters, "'Name'");
            }
            else if (Name.Trim() != Name)
            {
                ErrorMessage = string.Format(ErrorResource.ContainsLeadingOrTrailingWhitespace, "'Name'");
            }
            else if (HasDuplicateName(Name))
            {
                ErrorMessage = string.Format(ErrorResource.ItemWithNameAlreadyExists, Name);
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

        public ICommand CancelCommand { get; private set; }

        public IExplorerViewModel SingleEnvironmentExplorerViewModel { get; set; }


        public void Dispose()
        {
            SingleEnvironmentExplorerViewModel?.Dispose();
            _environmentViewModel?.Dispose();
        }
    }
}