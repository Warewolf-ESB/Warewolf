/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Search;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Windows.Input;

namespace Dev2.ViewModels.Search
{
    public class SearchValue : BindableBase, ISearchValue
    {
        Guid _resourceId;
        string _name;
        string _path;
        string _type;
        string _match;

        public SearchValue(Guid resourceId, string name, string path, IEnvironmentViewModel selectedEnvironment)
        {
            ResourceId = resourceId;
            Name = name;
            Path = path;
            OpenResourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => OpenResource(selectedEnvironment));
        }

        public SearchValue(Guid resourceId, string name, string path, string type, string match, IEnvironmentViewModel selectedEnvironment)
        {
            ResourceId = resourceId;
            Name = name;
            Path = path;
            Type = type;
            Match = match;
            OpenResourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => OpenResource(selectedEnvironment));
        }

        void OpenResource(IEnvironmentViewModel selectedEnvironment)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            if (shellViewModel != null)
            {
                switch (Type)
                {
                    case "Workflow":
                    case "Scalar":
                    case "RecordSet":
                    case "Object":
                        shellViewModel.OpenResource(ResourceId, selectedEnvironment.ResourceId, selectedEnvironment.Server);
                        break;
                    case "Test":
                        shellViewModel.OpenSelectedTest(ResourceId, Name);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Dispose()
        {

        }

        public Guid ResourceId
        {
            get => _resourceId;
            set
            {
                _resourceId = value;
                OnPropertyChanged(() => ResourceId);
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(() => Name);
            }
        }
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(() => Type);
            }
        }
        public string Match
        {
            get => _match;
            set
            {
                _match = value;
                OnPropertyChanged(() => Match);
            }
        }
        public ICommand OpenResourceCommand { get; set; }
    }
}