using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Workflow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : ObservableObject
    {
        ObservableCollection<WindowsGroupPermission> _serverPermissions;
        ObservableCollection<WindowsGroupPermission> _resourcePermissions;

        public SecurityViewModel()
        {
            PickWindowsGroupCommand = new RelayCommand(PickWindowsGroup, o => true);
            PickResourceCommand = new RelayCommand(PickResource, o => true);
        }
       

        public ObservableCollection<WindowsGroupPermission> ServerPermissions { get { return _serverPermissions; } set { OnPropertyChanged(ref _serverPermissions, value); } }
        public ObservableCollection<WindowsGroupPermission> ResourcePermissions { get { return _resourcePermissions; } set { OnPropertyChanged(ref _resourcePermissions, value); } }

        public WindowsGroupPermission SelectedServerPermission { get; set; }

        public ICommand PickWindowsGroupCommand { get; private set; }
        public ICommand PickResourceCommand { get; private set; }

        void PickResource(object obj)
        {
            var permission = obj as WindowsGroupPermission;
            if(permission == null)
            {
                return;
            }

            var resourceModel = PickResource();
            if(resourceModel == null)
            {
                return;
            }

            permission.ResourceID = resourceModel.ID;
            permission.ResourceName = string.Format("{0}\\{1}", resourceModel.ResourceName, resourceModel.Category);
        }

        void PickWindowsGroup(object obj)
        {
            var permission = obj as WindowsGroupPermission;
            if(permission == null)
            {
                return;
            }

            var directoryObj = PickWindowsGroup(ObjectTypes.BuiltInGroups | ObjectTypes.Groups, ObjectTypes.Groups, Locations.All, Locations.JoinedDomain);
            if(directoryObj == null)
            {
                return;
            }

            permission.WindowsGroup = directoryObj.Name;
        }

        protected virtual DirectoryObject PickWindowsGroup(ObjectTypes allowedTypes, ObjectTypes defaultTypes, Locations allowedLocations, Locations defaultLocations)
        {
            var picker = new DirectoryObjectPickerDialog
            {
                AllowedObjectTypes = allowedTypes,
                DefaultObjectTypes = defaultTypes,
                AllowedLocations = allowedLocations,
                DefaultLocations = defaultLocations,
                MultiSelect = false,
                TargetComputer = string.Empty
            };

            var dialogResult = picker.ShowDialog((IWin32Window)System.Windows.Application.Current.MainWindow);
            if(dialogResult != DialogResult.OK)
            {
                return null;
            }
            var results = picker.SelectedObjects;
            if(results == null || results.Length == 0)
            {
                return null;
            }
            return results[0];
        }

        protected virtual IResourceModel PickResource()
        {
            var type = typeof(DsfWorkflowActivity);
            DsfActivityDropViewModel dropViewModel;
            return DsfActivityDropUtils.TryPickResource(type.FullName, out dropViewModel) ? dropViewModel.SelectedResourceModel : null;
        }

        void RegisterPropertyChanged(WindowsGroupPermission permission)
        {
            //var windowsGroupProperty = DependencyPropertyDescriptor.FromProperty(WindowsGroupPermission2.WindowsGroupProperty, typeof(WindowsGroupPermission2));
            //windowsGroupProperty.AddValueChanged(permission, OnWindowsGroupPropertyChanged);
        }

        void OnWindowsGroupPropertyChanged(object sender, EventArgs eventArgs)
        {
            var permission = (WindowsGroupPermission)sender;

            if(string.IsNullOrEmpty(permission.WindowsGroup))
            {
                if(permission.IsServer)
                {
                    ServerPermissions.Remove(permission);
                }
                else
                {
                    ResourcePermissions.Remove(permission);
                }
            }
        }

        

        public static SecurityViewModel Create()
        {
            var serverPermissions = new[]
            {
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "BuiltIn\\Administrators", View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true },
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "Deploy Admins", View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false }
            };
            var resourcePermissions = new[]
            {
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 2", View = false, Execute = false, Contribute = true
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow2",
                    WindowsGroup = "Windows Group 1", View = true, Execute = true, Contribute = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 3", View = true, Execute = false, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 4", View = false, Execute = true, Contribute = false
                },


                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow4",
                    WindowsGroup = "Windows Group 3", View = false, Execute = false, Contribute = true
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 4", View = false, Execute = false, Contribute = true
                }
            };

            var viewModel = new SecurityViewModel
            {
                _serverPermissions = new ObservableCollection<WindowsGroupPermission>(), 
                _resourcePermissions = new ObservableCollection<WindowsGroupPermission>()
            };

            foreach(var permission in serverPermissions)
            {
                viewModel.RegisterPropertyChanged(permission);
                viewModel._serverPermissions.Add(permission);
            }

            foreach(var permission in resourcePermissions)
            {
                viewModel.RegisterPropertyChanged(permission);
                viewModel._resourcePermissions.Add(permission);
            }

            // Update bindings
            viewModel.OnPropertyChanged("ServerPermissions");
            viewModel.OnPropertyChanged("ResourcePermissions");

            return viewModel;
        }

    }

    public class IgnoreNewItemPlaceHolderConverter : IValueConverter
    {
        private const string NewItemPlaceholderName = "{NewItemPlaceholder}";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value != null && value.ToString() == NewItemPlaceholderName)
            {
                value = DependencyProperty.UnsetValue;
            }
            return value;
        }
    }
}
