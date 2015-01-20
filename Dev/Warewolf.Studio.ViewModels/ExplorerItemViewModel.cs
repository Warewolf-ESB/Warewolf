using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModel : BindableBase,IExplorerItemViewModel
    {
        string _resourceName;
        private bool _isVisible;

        public ExplorerItemViewModel(IShellViewModel shellViewModel)
        {
            if(shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            Children = new ObservableCollection<IExplorerItemViewModel>();
            OpenCommand = new DelegateCommand(() => shellViewModel.AddService(Resource));
            NewCommand = new DelegateCommand<ResourceType?>(shellViewModel.NewResource);
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
                OnPropertyChanged(() => ResourceName);
            }
        }
        public ICollection<IExplorerItemViewModel> Children
        {
            get;
            set;
        }
        public bool Checked { get; set; }
        public Guid ResourceId { get; set; }
        public ResourceType ResourceType { get; set; }
        public ICommand OpenCommand
        {
            get; set;
        }
        public ICommand NewCommand { get; set; }
        public ICommand DeployCommand { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(() => IsVisible);
                }
            }
        }

        public IResource Resource { get; set; }
    }
}