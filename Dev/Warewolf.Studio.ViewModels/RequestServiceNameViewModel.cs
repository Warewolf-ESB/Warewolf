using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Studio.ViewModels;
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
        public MessageBoxResult ViewResult { get; private set; }

        public RequestServiceNameViewModel(IEnvironmentViewModel environmentViewModel,IRequestServiceNameView view)
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
            environmentViewModel.Load();
            _view = view;
            OkCommand = new DelegateCommand(SetServiceName,() => String.IsNullOrEmpty(ErrorMessage));            
            CancelCommand = new DelegateCommand(CloseView);
            SingleEnvironmentExplorerViewModel = new SingleEnvironmentExplorerViewModel(environmentViewModel);
            _view.DataContext = this;
            Name = "";
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
            var parent = SingleEnvironmentExplorerViewModel.SelectedItem.Parent;
            var parentNames = new List<string>();
            while (parent != null)
            {
                parentNames.Add(parent.ResourceName);
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
                if (String.IsNullOrEmpty(Name))
                {
                    ErrorMessage = "'Name' cannot be empty.";
                }
                else if (NameHasInvalidCharacters(Name))
                {
                    ErrorMessage = "'Name' contains invalid characters.";
                }
                else
                {
                    ErrorMessage = "";
                }
            }
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