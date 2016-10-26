/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Dialogs;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Newtonsoft.Json;
using Warewolf.Studio.Core.Popup;
using Warewolf.Studio.Resources.Languages;
using Warewolf.Studio.ViewModels;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : SettingsItemViewModel, IHelpSource, IUpdatesHelp
    {
        IResourcePickerDialog _resourcePicker;
        readonly DirectoryObjectPickerDialog _directoryObjectPicker;
        readonly IWin32Window _parentWindow;
        readonly IEnvironmentModel _environment;
        bool _isUpdatingHelpText;
        private static IDomain _domain;

        public SecurityViewModel()
        {
            
        }

        internal SecurityViewModel(SecuritySettingsTO securitySettings, IWin32Window parentWindow, IEnvironmentModel environment)
            : this(securitySettings, new DirectoryObjectPickerDialog(), parentWindow, environment)
        {
        }

        public IResourcePickerDialog CreateResourcePickerDialog()
        {
            var env = GetEnvironment();
            var res = new ResourcePickerDialog(enDsfActivityType.Workflow, env);
            ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, env).ContinueWith(a => _resourcePicker = a.Result);
            return res;
        }

        static IEnvironmentViewModel GetEnvironment()
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var env = new EnvironmentViewModel(shellViewModel.ActiveServer, shellViewModel, true);
            return env;
        }

        public SecurityViewModel(SecuritySettingsTO securitySettings, DirectoryObjectPickerDialog directoryObjectPicker, IWin32Window parentWindow, IEnvironmentModel environment, Func<IResourcePickerDialog> createfunc = null)
        {

            VerifyArgument.IsNotNull(@"directoryObjectPicker", directoryObjectPicker);
            VerifyArgument.IsNotNull(@"parentWindow", parentWindow);
            VerifyArgument.IsNotNull(@"environment", environment);

            _environment = environment;
            _parentWindow = parentWindow;
            _resourcePicker = (createfunc ?? CreateResourcePickerDialog)();
            _directoryObjectPicker = directoryObjectPicker;
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

            InitializePermissions(securitySettings?.WindowsGroupPermissions);
            _domain = new DomainWrapper();
        }

        public ObservableCollection<WindowsGroupPermission> ServerPermissions
        {
            get { return _serverPermissions; }
            private set
            {
                _serverPermissions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<WindowsGroupPermission> ResourcePermissions
        {
            get { return _resourcePermissions; }
            private set
            {
                _resourcePermissions = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public ActivityDesignerToggle ServerHelpToggle { get; private set; }

        [JsonIgnore]
        public ActivityDesignerToggle ResourceHelpToggle { get; private set; }

        [JsonIgnore]
        public ICommand PickWindowsGroupCommand { get; private set; }

        [JsonIgnore]
        public ICommand PickResourceCommand { get; private set; }

        [JsonIgnore]
        public bool IsServerHelpVisible
        {
            get { return (bool)GetValue(IsServerHelpVisibleProperty); }
            set { SetValue(IsServerHelpVisibleProperty, value); }
        }

        [JsonIgnore]
        public static readonly DependencyProperty IsServerHelpVisibleProperty = DependencyProperty.Register(@"IsServerHelpVisible", typeof(bool), typeof(SecurityViewModel), new PropertyMetadata(false, IsServerHelpVisiblePropertyChanged));

        static void IsServerHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((SecurityViewModel)d).UpdateHelpText(HelpType.Server);
        }

        [JsonIgnore]
        public bool IsResourceHelpVisible
        {
            get { return (bool)GetValue(IsResourceHelpVisibleProperty); }
            set { SetValue(IsResourceHelpVisibleProperty, value); }
        }

        [JsonIgnore]
        public static readonly DependencyProperty IsResourceHelpVisibleProperty = DependencyProperty.Register(@"IsResourceHelpVisible", typeof(bool), typeof(SecurityViewModel), new PropertyMetadata(false, IsResourceHelpVisiblePropertyChanged));
        private SecurityViewModel _item;
        private ObservableCollection<WindowsGroupPermission> _serverPermissions;
        private ObservableCollection<WindowsGroupPermission> _resourcePermissions;

        static void IsResourceHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((SecurityViewModel)d).UpdateHelpText(HelpType.Resource);
        }

        public virtual void Save(SecuritySettingsTO securitySettings)
        {
            VerifyArgument.IsNotNull(@"securitySettings", securitySettings);

            securitySettings.WindowsGroupPermissions.Clear();
            Copy(ServerPermissions, securitySettings.WindowsGroupPermissions);
            Copy(ResourcePermissions, securitySettings.WindowsGroupPermissions);
        }

        void Copy(IList<WindowsGroupPermission> source, IList<WindowsGroupPermission> target)
        {
            for (var i = source.Count - 1; i >= 0; i--)
            {
                var permission = source[i];
                if (permission.IsValid && !permission.IsDeleted)
                {
                    target.Insert(0, permission);
                }
                else
                {
                    source.RemoveAt(i);
                    if (permission.IsNew)
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

        void InitializePermissions(IEnumerable<WindowsGroupPermission> permissions)
        {
            ServerPermissions = new ObservableCollection<WindowsGroupPermission>();
            ResourcePermissions = new ObservableCollection<WindowsGroupPermission>();

            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    RegisterPropertyChanged(permission);
                    if (permission.IsServer)
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
            if (permission == null)
            {
                return;
            }

            var resourceModel = PickResource(permission);
            if (resourceModel == null)
            {
                return;
            }

            permission.ResourceID = resourceModel.ResourceId;
            permission.ResourceName = resourceModel.ResourcePath;
        }

        IExplorerTreeItem PickResource(WindowsGroupPermission permission)
        {
            if (permission != null && permission.ResourceID != Guid.Empty)
            {
                var foundResourceModel = _environment.ResourceRepository?.FindSingle(model => model.ID == permission.ResourceID);
                if (foundResourceModel != null)
                {
                    _resourcePicker.SelectResource(foundResourceModel.ID);
                }
            }
            var hasResult = _resourcePicker.ShowDialog(_environment);

            if (_environment.ResourceRepository != null)
            {
                return hasResult ? _resourcePicker.SelectedResource : null;
            }
            throw new Exception(@"Server does not exist");
        }

        void PickWindowsGroup(object obj)
        {
            var permission = obj as WindowsGroupPermission;
            if (permission == null)
            {
                return;
            }

            var directoryObj = PickWindowsGroup();
            if (directoryObj == null)
            {
                return;
            }

            permission.WindowsGroup = directoryObj.Name;
        }

        DirectoryObject PickWindowsGroup()
        {

            var dialogResult = ShowDirectoryObjectPickerDialog(_parentWindow);
            if (dialogResult != DialogResult.OK)
            {
                return null;
            }
            var results = GetSelectedObjectsFromDirectoryObjectPickerDialog();
            if (results == null || results.Length == 0)
            {
                return null;
            }
            return results[0];
        }

        public virtual DialogResult ShowDirectoryObjectPickerDialog(IWin32Window parentWindow)
        {
            try
            {
                return _directoryObjectPicker.ShowDialog(parentWindow);
            }
            catch (Exception e)
            {
                var popupController = CustomContainer.Get<Common.Interfaces.Studio.Controller.IPopupController>();
                if (popupController != null)
                {
                    var popupMessage = new PopupMessage
                    {
                        Description = Core.Error_Opening_Windows_Group_Picker,
                        Image = MessageBoxImage.Error,
                        Buttons = MessageBoxButton.OK,
                        IsError = true,
                        Header = @"Error"

                    };
                    popupController.Show(popupMessage);
                }
                Dev2Logger.Error(@"Error opening group picker: ", e);
            }
            return DialogResult.Cancel;
        }

        public virtual DirectoryObject[] GetSelectedObjectsFromDirectoryObjectPickerDialog()
        {
            return _directoryObjectPicker.SelectedObjects;
        }

        void RegisterPropertyChanged(WindowsGroupPermission permission)
        {
            permission.PropertyChanged += OnPermissionPropertyChanged;
        }

        [JsonIgnore]
        public SecurityViewModel Item
        {
            private get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public void SetItem(SecurityViewModel model)
        {
            Item = Clone(model);
        }

        public SecurityViewModel Clone(SecurityViewModel model)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = resolver });
            SecurityViewModel clone = JsonConvert.DeserializeObject<SecurityViewModel>(ser);
            return clone;
        }

        private bool Equals(SecurityViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return EqualsSeq(other);
        }

        private bool EqualsSeq(SecurityViewModel other)
        {
            var serverPermissionCompare = ServerPermissionsCompare(other, true);
            var resourcePermissionCompare = ResourcePermissionsCompare(other, true);
            var @equals = serverPermissionCompare && resourcePermissionCompare;

            return @equals;
        }

        private bool ServerPermissionsCompare(SecurityViewModel other, bool serverPermissionCompare)
        {
            if (_serverPermissions == null)
            {
                return true;
            }
            if (_serverPermissions.Count != other._serverPermissions.Count)
            {
                return false;
            }
            for (int i = 0; i < _serverPermissions.Count; i++)
            {
                if (ServerPermissions[i].ResourceName != other.ServerPermissions[i].ResourceName)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerPermissions[i].WindowsGroup != other.ServerPermissions[i].WindowsGroup)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerPermissions[i].DeployTo != other.ServerPermissions[i].DeployTo)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerPermissions[i].DeployFrom != other.ServerPermissions[i].DeployFrom)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerPermissions[i].Administrator != other.ServerPermissions[i].Administrator)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerPermissions[i].View != other.ServerPermissions[i].View)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerPermissions[i].Execute != other.ServerPermissions[i].Execute)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerPermissions[i].Contribute != other.ServerPermissions[i].Contribute)
                {
                    serverPermissionCompare = false;
                }
            }
            return serverPermissionCompare;
        }

        private bool ResourcePermissionsCompare(SecurityViewModel other, bool resourcePermissionCompare)
        {
            if (_resourcePermissions == null)
            {
                return true;
            }
            if (_resourcePermissions.Count != other._resourcePermissions.Count)
            {
                return false;
            }
            for (int i = 0; i < _resourcePermissions.Count; i++)
            {
                if (ResourcePermissions[i].ResourceName != other.ResourcePermissions[i].ResourceName)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourcePermissions[i].WindowsGroup != other.ResourcePermissions[i].WindowsGroup)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourcePermissions[i].DeployTo != other.ResourcePermissions[i].DeployTo)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourcePermissions[i].DeployFrom != other.ResourcePermissions[i].DeployFrom)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourcePermissions[i].Administrator != other.ResourcePermissions[i].Administrator)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourcePermissions[i].View != other.ResourcePermissions[i].View)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourcePermissions[i].Execute != other.ResourcePermissions[i].Execute)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourcePermissions[i].Contribute != other.ResourcePermissions[i].Contribute)
                {
                    resourcePermissionCompare = false;
                }
            }
            return resourcePermissionCompare;
        }

        void OnPermissionPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            IsDirty = !Equals(Item);
            UpdateOverridingPermission(sender as WindowsGroupPermission, args.PropertyName);
            if (args.PropertyName == @"WindowsGroup" || args.PropertyName == @"ResourceName")
            {
                var permission = (WindowsGroupPermission)sender;

                if (permission.IsNew)
                {
                    if (permission.IsValid)
                    {
                        permission.IsNew = false;
                        var newPermission = CreateNewPermission(permission.IsServer);

                        if (permission.IsServer)
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
                    if (isEmpty)
                    {
                        if (permission.IsServer)
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
            if (windowsGroupPermission == null)
            {
                return;
            }

            SwitchAdminPermissionsOff(windowsGroupPermission, propertyName);
            SwitchContributePermissionsOff(windowsGroupPermission, propertyName);
            if (windowsGroupPermission.Administrator && propertyName == @"Administrator")
            {
                UpdateForAdministratorPermissions(windowsGroupPermission);
            }
            if (windowsGroupPermission.Contribute && propertyName == @"Contribute")
            {
                UpdateForContributePermissions(windowsGroupPermission);
            }
        }

        static void SwitchContributePermissionsOff(WindowsGroupPermission windowsGroupPermission, string propertyName)
        {
            if (!windowsGroupPermission.View && propertyName == @"View"
               || !windowsGroupPermission.Execute && propertyName == @"Execute")
            {
                windowsGroupPermission.Contribute = false;
            }
        }

        static void SwitchAdminPermissionsOff(WindowsGroupPermission windowsGroupPermission, string propertyName)
        {
            if (!windowsGroupPermission.DeployTo && propertyName == @"DeployTo"
               || !windowsGroupPermission.DeployFrom && propertyName == @"DeployFrom"
               || !windowsGroupPermission.Contribute && propertyName == @"Contribute")
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
            var toggle = ActivityDesignerToggle.Create(@"ServiceHelp", @"Close Help", @"ServiceHelp", @"Open Help", @"HelpToggle", this, targetProperty
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
            if (_isUpdatingHelpText)
            {
                return;
            }
            _isUpdatingHelpText = true;
            try
            {
                switch (helpType)
                {
                    case HelpType.Server:
                        IsResourceHelpVisible = false;
                        HelpText = Core.SettingsSecurityServerHelpWindowsGroup;
                        break;

                    case HelpType.Resource:
                        IsServerHelpVisible = false;
                        HelpText = Core.SettingsSecurityResourceHelpResource;
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

        #region Implementation of IUpdatesHelp

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        #endregion

        [JsonIgnore]
        public Visibility Visibility => IsInDomain();

        private static Visibility IsInDomain()
        {
            try
            {
                // ReSharper disable once UnusedVariable
                var computerDomain = _domain.GetComputerDomain();
                return Visibility.Visible;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }


        }
    }
}
