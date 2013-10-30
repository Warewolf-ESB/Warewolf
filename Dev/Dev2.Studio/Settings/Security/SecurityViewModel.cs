using System;
using System.Collections.Generic;
using System.Windows;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : DependencyObject
    {
        public SecurityViewModel()
        {
            ServerPermissions = new List<WindowsGroupPermission>();
            ResourcePermissions = new List<WindowsGroupPermission>();
        }

        public List<WindowsGroupPermission> ServerPermissions { get; private set; }
        public List<WindowsGroupPermission> ResourcePermissions { get; private set; }

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
    }
}
