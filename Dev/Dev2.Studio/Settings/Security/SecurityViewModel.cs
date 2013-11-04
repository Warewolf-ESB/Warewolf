using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Data.Settings.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Workflow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : ObservableObject
    {
        bool _isDirty;

        public SecurityViewModel()
        {
            PickWindowsGroupCommand = new RelayCommand(PickWindowsGroup, o => true);
            PickResourceCommand = new RelayCommand(PickResource, o => true);

            ServerPermissions = new ObservableCollection<WindowsGroupPermission>();
            ResourcePermissions = new ObservableCollection<WindowsGroupPermission>();
        }

        public ObservableCollection<WindowsGroupPermission> ServerPermissions { get; private set; }
        public ObservableCollection<WindowsGroupPermission> ResourcePermissions { get; private set; }

        public ICommand PickWindowsGroupCommand { get; private set; }
        public ICommand PickResourceCommand { get; private set; }

        public bool IsDirty { get { return _isDirty; } set { OnPropertyChanged(ref _isDirty, value); } }

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
            permission.PropertyChanged += OnPermissionPropertyChanged;
        }

        void OnPermissionPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            IsDirty = true;
            if(args.PropertyName == "WindowsGroup" || args.PropertyName == "ResourceName")
            {
                var permission = (WindowsGroupPermission)sender;

                if(permission.IsNew)
                {
                    var isEmpty = permission.IsServer
                        ? string.IsNullOrEmpty(permission.WindowsGroup)
                        : string.IsNullOrEmpty(permission.WindowsGroup) && string.IsNullOrEmpty(permission.ResourceName);
                    if(!isEmpty)
                    {
                        permission.IsNew = false;
                        var newPermission = CreateNewPermission(permission.IsServer);

                        if(permission.IsServer)
                        {
                            ServerPermissions.Add(newPermission);
                        }
                        else
                        {
                            ResourcePermissions.Add(newPermission);
                        }
                    }
                }
                else
                {
                    var isEmpty = string.IsNullOrEmpty(permission.WindowsGroup);
                    if(isEmpty)
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

            var viewModel = new SecurityViewModel();

            foreach(var permission in serverPermissions)
            {
                viewModel.RegisterPropertyChanged(permission);
                viewModel.ServerPermissions.Add(permission);
            }
            viewModel.ServerPermissions.Add(viewModel.CreateNewPermission(true));

            foreach(var permission in resourcePermissions)
            {
                viewModel.RegisterPropertyChanged(permission);
                viewModel.ResourcePermissions.Add(permission);
            }
            viewModel.ResourcePermissions.Add(viewModel.CreateNewPermission(false));

            return viewModel;
        }

        WindowsGroupPermission CreateNewPermission(bool isServer)
        {
            var permission = new WindowsGroupPermission { IsNew = true, IsServer = isServer };
            RegisterPropertyChanged(permission);
            return permission;
        }
    }

    public class DefaultText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
