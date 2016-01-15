using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

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
        MessageBoxResult ViewResult { get; set; }

        private RequestServiceNameViewModel()
        {

        }
        /// <exception cref="ArgumentNullException"><paramref name="environmentViewModel"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null" />.</exception>
#pragma warning disable 1998
        private async Task<IRequestServiceNameViewModel> InitializeAsync(IEnvironmentViewModel environmentViewModel, IRequestServiceNameView view, string selectedPath, string header)
#pragma warning restore 1998
        {
            if (environmentViewModel == null)
            {
                throw new ArgumentNullException("environmentViewModel");
            }
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            environmentViewModel.Connect();
            _selectedPath = selectedPath;
            _header = header;
#pragma warning disable 4014
            environmentViewModel.LoadDialog(selectedPath).ContinueWith(a=>HasLoaded=a.Result);
          
#pragma warning restore 4014
            _view = view;

            OkCommand = new DelegateCommand(SetServiceName, () => String.IsNullOrEmpty(ErrorMessage) && HasLoaded);
            CancelCommand = new DelegateCommand(CloseView);
            SingleEnvironmentExplorerViewModel = new SingleEnvironmentExplorerViewModel(environmentViewModel, Guid.Empty, false);
            SingleEnvironmentExplorerViewModel.PropertyChanged += SingleEnvironmentExplorerViewModelPropertyChanged;
            _view.DataContext = this;
            Name = "";
            environmentViewModel.CanShowServerVersion = false;
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
                ((DelegateCommand)OkCommand).RaiseCanExecuteChanged();
            }
        }

        public static Task<IRequestServiceNameViewModel> CreateAsync(IEnvironmentViewModel environmentViewModel, IRequestServiceNameView view, string selectedPath, string header)
        {
            var ret = new RequestServiceNameViewModel();
            return ret.InitializeAsync(environmentViewModel, view, selectedPath, header);
        }

        private void CloseView()
        {
            _view.RequestClose();
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
            if (SingleEnvironmentExplorerViewModel.SelectedItem != null)
            {
                var parent = SingleEnvironmentExplorerViewModel.SelectedItem.Parent;
                var parentNames = new List<string>();
                while (parent != null)
                {
                    if (parent.ResourceType != ResourceType.ServerSource)
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
                if (SingleEnvironmentExplorerViewModel.SelectedItem.ResourceType == ResourceType.Folder)
                {
                    path = path + "\\" + SingleEnvironmentExplorerViewModel.SelectedItem.ResourceName;
                }
                return path;
            }
            return "";
        }

        private void RaiseCanExecuteChanged()
        {
            var command = OkCommand as DelegateCommand;
            if (command != null)
            {
                command.RaiseCanExecuteChanged();
            }
        }

        public MessageBoxResult ShowSaveDialog()
        {
            SingleEnvironmentExplorerViewModel.SelectItem(_selectedPath);
            _view.ShowView();

            return ViewResult;
        }

        public ResourceName ResourceName
        {
            get { return _resourceName; }
        }

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
            if (String.IsNullOrEmpty(Name))
            {
                ErrorMessage = "'Name' cannot be empty.";
            }
            else if (NameHasInvalidCharacters(Name))
            {
                ErrorMessage = "'Name' contains invalid characters.";
            }
            else if (Name.Trim() != Name)
            {
                ErrorMessage = "'Name' contains leading or trailing whitespace characters.";
            }
            else if (HasDuplicateName(Name))
            {
                ErrorMessage = string.Format("An item with name '{0}' already exists in this folder.", Name);
            }
            else
            {
                ErrorMessage = "";
            }
        }

        private bool HasDuplicateName(string requestedServiceName)
        {
            var explorerTreeItem = SingleEnvironmentExplorerViewModel.SelectedItem;
            if (explorerTreeItem != null)
            {
                return explorerTreeItem.Children.Any(model => model.ResourceName.ToLower() == requestedServiceName.ToLower() && model.ResourceType != ResourceType.Folder);
            }
            if (SingleEnvironmentExplorerViewModel.Environments.First() != null)
            {
                var explorerItemViewModels = SingleEnvironmentExplorerViewModel.Environments.First().Children;
                return explorerItemViewModels != null && explorerItemViewModels.Any(model => requestedServiceName != null && (model.ResourceName != null && (model.ResourceName.ToLower() == requestedServiceName.ToLower() && model.ResourceType != ResourceType.Folder)));
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

        public IExplorerViewModel SingleEnvironmentExplorerViewModel { get; private set; }
    }
}