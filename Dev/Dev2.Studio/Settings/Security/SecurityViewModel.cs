
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Dialogs;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : SettingsItemViewModel, IHelpSource
    {
        readonly IResourcePickerDialog _resourcePicker;
        readonly DirectoryObjectPickerDialog _directoryObjectPicker;
        readonly IWin32Window _parentWindow;
        readonly IEnvironmentModel _environment;
        bool _isUpdatingHelpText;

        internal SecurityViewModel(SecuritySettingsTO securitySettings, IWin32Window parentWindow, IEnvironmentModel environment)
            : this(securitySettings, new ResourcePickerDialog(enDsfActivityType.All, environment), new DirectoryObjectPickerDialog(), parentWindow, environment)
        {
        }

        public SecurityViewModel(SecuritySettingsTO securitySettings, IResourcePickerDialog resourcePicker, DirectoryObjectPickerDialog directoryObjectPicker, IWin32Window parentWindow, IEnvironmentModel environment)
        {
            VerifyArgument.IsNotNull("resourcePicker", resourcePicker);
            VerifyArgument.IsNotNull("directoryObjectPicker", directoryObjectPicker);
            VerifyArgument.IsNotNull("parentWindow", parentWindow);
            VerifyArgument.IsNotNull("environment", environment);

            _resourcePicker = resourcePicker;
            _directoryObjectPicker = directoryObjectPicker;
            _parentWindow = parentWindow;
            _environment = environment;
            _directoryObjectPicker.AllowedObjectTypes = ObjectTypes.BuiltInGroups | ObjectTypes.Groups;
            _directoryObjectPicker.DefaultObjectTypes = ObjectTypes.Groups;
            _directoryObjectPicker.AllowedLocations = Locations.All;
            _directoryObjectPicker.DefaultLocations = Locations.JoinedDomain;
            _directoryObjectPicker.MultiSelect = false;
            _directoryObjectPicker.TargetComputer = string.Empty;
            _directoryObjectPicker.ShowAdvancedView = false;

            PickWindowsGroupCommand = new DelegateCommand(PickWindowsGroup);
            PickResourceCommand = new DelegateCommand(PickResource);

            InitializeHelp();

            InitializePermissions(securitySettings == null ? null : securitySettings.WindowsGroupPermissions);
        }

        public ObservableCollection<IWindowsGroupPermission> ServerPermissions { get; private set; }

        public ObservableCollection<IWindowsGroupPermission> ResourcePermissions { get; private set; }

        public ActivityDesignerToggle ServerHelpToggle { get; private set; }

        public ActivityDesignerToggle ResourceHelpToggle { get; private set; }

        public ICommand PickWindowsGroupCommand { get; private set; }

        public ICommand PickResourceCommand { get; private set; }

        public bool IsServerHelpVisible
        {
            get { return (bool)GetValue(IsServerHelpVisibleProperty); }
            set { SetValue(IsServerHelpVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsServerHelpVisibleProperty = DependencyProperty.Register("IsServerHelpVisible", typeof(bool), typeof(SecurityViewModel), new PropertyMetadata(false, IsServerHelpVisiblePropertyChanged));

        static void IsServerHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((SecurityViewModel)d).UpdateHelpText(HelpType.Server);
        }

        public bool IsResourceHelpVisible
        {
            get { return (bool)GetValue(IsResourceHelpVisibleProperty); }
            set { SetValue(IsResourceHelpVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsResourceHelpVisibleProperty = DependencyProperty.Register("IsResourceHelpVisible", typeof(bool), typeof(SecurityViewModel), new PropertyMetadata(false, IsResourceHelpVisiblePropertyChanged));

        static void IsResourceHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((SecurityViewModel)d).UpdateHelpText(HelpType.Resource);
        }

        public virtual void Save(SecuritySettingsTO securitySettings)
        {
            VerifyArgument.IsNotNull("securitySettings", securitySettings);

            securitySettings.WindowsGroupPermissions.Clear();
            Copy(ServerPermissions, securitySettings.WindowsGroupPermissions);
            Copy(ResourcePermissions, securitySettings.WindowsGroupPermissions);
        }

        void Copy(IList<IWindowsGroupPermission> source, IList<IWindowsGroupPermission> target)
        {
            for(var i = source.Count - 1; i >= 0; i--)
            {
                var permission = source[i];
                if(permission.IsValid && !permission.IsDeleted)
                {
                    target.Insert(0, permission);
                }
                else
                {
                    source.RemoveAt(i);
                    if(permission.IsNew)
                    {
                        // re-add new permission
                        permission = CreateNewPermission(permission.IsServer);
                        source.Insert(i, permission);
                    }
                }
            }
        }

        void InitializeHelp()
        {
            ServerHelpToggle = CreateHelpToggle(IsServerHelpVisibleProperty);
            ResourceHelpToggle = CreateHelpToggle(IsResourceHelpVisibleProperty);
        }

        void InitializePermissions(IEnumerable<IWindowsGroupPermission> permissions)
        {
            ServerPermissions = new ObservableCollection<IWindowsGroupPermission>();
            ResourcePermissions = new ObservableCollection<IWindowsGroupPermission>();

            if(permissions != null)
            {
                foreach(var permission in permissions)
                {
                    RegisterPropertyChanged((WindowsGroupPermission)permission);
                    if(permission.IsServer)
                    {
                        ServerPermissions.Add(permission);
                    }
                    else
                    {
                        ResourcePermissions.Add(permission);
                    }
                }
            }
            ServerPermissions.Add(CreateNewPermission(true));
            ResourcePermissions.Add(CreateNewPermission(false));
        }

        void PickResource(object obj)
        {
            var permission = obj as WindowsGroupPermission;
            if(permission == null)
            {
                return;
            }

            var resourceModel = PickResource(permission);
            if(resourceModel == null)
            {
                return;
            }

            permission.ResourceID = resourceModel.ID;
            permission.ResourceName = string.Format("{0}\\{1}\\{2}", resourceModel.ResourceType.GetTreeDescription(), resourceModel.Category, resourceModel.ResourceName);
        }

        IResourceModel PickResource(WindowsGroupPermission permission)
        {
            if(permission != null && permission.ResourceID != Guid.Empty)
            {
                if(_environment.ResourceRepository != null)
                {
                    var foundResourceModel = _environment.ResourceRepository.FindSingle(model => model.ID == permission.ResourceID);
                    if(foundResourceModel != null)
                    {
                        _resourcePicker.SelectedResource = foundResourceModel;
                    }
                }
            }
            var hasResult = _resourcePicker.ShowDialog(_environment);
            return hasResult ? _resourcePicker.SelectedResource : null;
        }

        void PickWindowsGroup(object obj)
        {
            var permission = obj as WindowsGroupPermission;
            if(permission == null)
            {
                return;
            }

            var directoryObj = PickWindowsGroup();
            if(directoryObj == null)
            {
                return;
            }

            permission.WindowsGroup = directoryObj.Name;
        }

        DirectoryObject PickWindowsGroup()
        {
            var dialogResult = ShowDirectoryObjectPickerDialog(_parentWindow);
            if(dialogResult != DialogResult.OK)
            {
                return null;
            }
            var results = GetSelectedObjectsFromDirectoryObjectPickerDialog();
            if(results == null || results.Length == 0)
            {
                return null;
            }
            return results[0];
        }

        public virtual DialogResult ShowDirectoryObjectPickerDialog(IWin32Window parentWindow)
        {
            return _directoryObjectPicker.ShowDialog(parentWindow);
        }

        public virtual DirectoryObject[] GetSelectedObjectsFromDirectoryObjectPickerDialog()
        {
            return _directoryObjectPicker.SelectedObjects;
        }

        void RegisterPropertyChanged(WindowsGroupPermission permission)
        {
            permission.PropertyChanged += OnPermissionPropertyChanged;
        }

        void OnPermissionPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            IsDirty = true;
            UpdateOverridingPermission(sender as WindowsGroupPermission, args.PropertyName);
            if(args.PropertyName == "WindowsGroup" || args.PropertyName == "ResourceName")
            {
                var permission = (WindowsGroupPermission)sender;

                if(permission.IsNew)
                {
                    if(permission.IsValid)
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

        void UpdateOverridingPermission(WindowsGroupPermission windowsGroupPermission, string propertyName)
        {
            if(windowsGroupPermission == null)
            {
                return;
            }

            SwitchAdminPermissionsOff(windowsGroupPermission, propertyName);
            SwitchContributePermissionsOff(windowsGroupPermission, propertyName);
            if(windowsGroupPermission.Administrator && propertyName == "Administrator")
            {
                UpdateForAdministratorPermissions(windowsGroupPermission);
            }
            if(windowsGroupPermission.Contribute && propertyName == "Contribute")
            {
                UpdateForContributePermissions(windowsGroupPermission);
            }
        }

        static void SwitchContributePermissionsOff(WindowsGroupPermission windowsGroupPermission, string propertyName)
        {
            if((!windowsGroupPermission.View && propertyName == "View")
               || (!windowsGroupPermission.Execute && propertyName == "Execute"))
            {
                windowsGroupPermission.Contribute = false;
            }
        }

        static void SwitchAdminPermissionsOff(WindowsGroupPermission windowsGroupPermission, string propertyName)
        {
            if((!windowsGroupPermission.DeployTo && propertyName == "DeployTo")
               || (!windowsGroupPermission.DeployFrom && propertyName == "DeployFrom")
               || (!windowsGroupPermission.Contribute && propertyName == "Contribute"))
            {
                windowsGroupPermission.Administrator = false;
            }
        }

        static void UpdateForContributePermissions(WindowsGroupPermission windowsGroupPermission)
        {
            windowsGroupPermission.View = true;
            windowsGroupPermission.Execute = true;
        }

        static void UpdateForAdministratorPermissions(WindowsGroupPermission windowsGroupPermission)
        {
            windowsGroupPermission.DeployFrom = true;
            windowsGroupPermission.DeployTo = true;
            windowsGroupPermission.Contribute = true;
        }

        WindowsGroupPermission CreateNewPermission(bool isServer)
        {
            var permission = new WindowsGroupPermission { IsNew = true, IsServer = isServer };
            RegisterPropertyChanged(permission);
            return permission;
        }

        ActivityDesignerToggle CreateHelpToggle(DependencyProperty targetProperty)
        {
            var toggle = ActivityDesignerToggle.Create("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png", "Close Help", "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png", "Open Help", "HelpToggle", this, targetProperty
                );

            return toggle;
        }

        protected override void CloseHelp()
        {
            IsServerHelpVisible = false;
            IsResourceHelpVisible = false;
        }

        void UpdateHelpText(HelpType helpType)
        {
            if(_isUpdatingHelpText)
            {
                return;
            }
            _isUpdatingHelpText = true;
            try
            {
                switch(helpType)
                {
                    case HelpType.Server:
                        IsResourceHelpVisible = false;
                        HelpText = Help.HelpTextResources.SettingsSecurityServerHelpWindowsGroup;
                        break;

                    case HelpType.Resource:
                        IsServerHelpVisible = false;
                        HelpText = Help.HelpTextResources.SettingsSecurityResourceHelpResource;
                        break;
                }
            }
            finally
            {
                _isUpdatingHelpText = false;
            }
        }

        enum HelpType
        {
            Server,
            Resource
        }

        public bool HasDuplicateResourcePermissions()
        {
            var duplicates = ResourcePermissions
                                  .Where(i => !i.IsDeleted)
                                  .GroupBy(i => new { i.ResourceID, i.WindowsGroup })
                                  .Where(g => g.Count() > 1)
                                  .Select(g => g.Key);
            return duplicates.Any();
        }

        public bool HasDuplicateServerPermissions()
        {
            var duplicates = ServerPermissions
                                .Where(i => !i.IsDeleted)
                                .GroupBy(i => i.WindowsGroup)
                                .Where(g => g.Count() > 1)
                                .Select(g => g.Key);
            return duplicates.Any();
        }
    }

}
