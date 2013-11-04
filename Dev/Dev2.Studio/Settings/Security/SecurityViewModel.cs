using System;
using System.Collections.Generic;
using System.Windows;
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
    public class SecurityViewModel : DependencyObject
    {
        public SecurityViewModel()
        {
            ServerPermissions = new List<WindowsGroupPermission>();
            ResourcePermissions = new List<WindowsGroupPermission>();
            PickWindowsGroupCommand = new RelayCommand(PickWindowsGroup, o => true);
            PickResourceCommand = new RelayCommand(PickResource, o => true);
        }

        public List<WindowsGroupPermission> ServerPermissions { get; private set; }
        public List<WindowsGroupPermission> ResourcePermissions { get; private set; }
        public ICommand PickWindowsGroupCommand { get; private set; }
        public ICommand PickResourceCommand { get; private set; }

        public static SecurityViewModel Create()
        {
            var viewModel = new SecurityViewModel();
            viewModel.ServerPermissions.AddRange(new[]
            {
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "BuiltIn\\Administrators", View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true },
                new WindowsGroupPermission { IsServer = true, WindowsGroup = "Deploy Admins", View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false }
            });
            viewModel.ResourcePermissions.AddRange(new[]
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
                },
            });
            return viewModel;
        }

        void PickResource(object obj)
        {
            var permissionGroup = obj as WindowsGroupPermission;
            if(permissionGroup == null)
            {
                return;
            }

            var resourceModel = PickResource();
            if(resourceModel == null)
            {
                return;
            }

            permissionGroup.ResourceID = resourceModel.ID;
            permissionGroup.ResourceName = string.Format("{0}\\{1}", resourceModel.ResourceName, resourceModel.Category);
        }

        void PickWindowsGroup(object obj)
        {
            var permissionGroup = obj as WindowsGroupPermission;
            if(permissionGroup == null)
            {
                return;
            }

            var directoryObj = PickWindowsGroup(ObjectTypes.BuiltInGroups | ObjectTypes.Groups, ObjectTypes.Groups, Locations.All, Locations.JoinedDomain);
            if(directoryObj == null)
            {
                return;
            }

            permissionGroup.WindowsGroup = directoryObj.Name;
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
    }
}
