using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Search;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;

namespace Dev2.ViewModels.Search
{
    public class SearchValue : BindableBase, ISearchValue
    {
        private Guid _resourceId;
        private string _name;
        private string _path;
        private string _type;
        private string _match;

        public SearchValue(Guid resourceId, string name, string path, string type, string match, IEnvironmentViewModel selectedEnvironment)
        {
            ResourceId = resourceId;
            Name = name;
            Path = path;
            Type = type;
            Match = match;
            OpenResourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => OpenResource(selectedEnvironment));
        }

        private void OpenResource(IEnvironmentViewModel selectedEnvironment)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            if (shellViewModel != null)
            {
                switch (Type)
                {
                    case "Workflow":
                        shellViewModel.OpenResource(ResourceId, selectedEnvironment.ResourceId, selectedEnvironment.Server);
                        break;
                    case "Test":
                        shellViewModel.OpenSelectedTest(ResourceId, Name);
                        break;
                }
            }
        }

        public Guid ResourceId
        {
            get { return _resourceId; }
            set
            {
                _resourceId = value;
                OnPropertyChanged(() => ResourceId);
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(() => Name);
            }
        }
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(() => Type);
            }
        }
        public string Match
        {
            get { return _match; }
            set
            {
                _match = value;
                OnPropertyChanged(() => Match);
            }
        }
        public ICommand OpenResourceCommand { get; set; }
    }
}
