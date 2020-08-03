#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Dialogs;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Enums;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;
using Warewolf;
using Warewolf.Data;
using Warewolf.Studio.Core.Popup;
using Warewolf.Studio.Resources.Languages;
using Warewolf.Studio.ViewModels;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : SettingsItemViewModel, IHelpSource, IUpdatesHelp
    {
        private IResourcePickerDialog _resourcePicker;
        private readonly DirectoryObjectPickerDialog _directoryObjectPicker;
        private readonly IWin32Window _parentWindow;
        private readonly IServer _environment;
        private bool _isUpdatingHelpText;

        [ExcludeFromCodeCoverage]
        public SecurityViewModel()
        {
        }

        [ExcludeFromCodeCoverage]
        internal SecurityViewModel(SecuritySettingsTO securitySettings, IWin32Window parentWindow, IServer environment)
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

        [ExcludeFromCodeCoverage]
        public SecurityViewModel(SecuritySettingsTO securitySettings, DirectoryObjectPickerDialog directoryObjectPicker, IWin32Window parentWindow, IServer environment)
            : this(securitySettings, directoryObjectPicker, parentWindow, environment, null)
        {
        }

        public SecurityViewModel(SecuritySettingsTO securitySettings, DirectoryObjectPickerDialog directoryObjectPicker, IWin32Window parentWindow, IServer environment, Func<IResourcePickerDialog> createfunc)
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

            OverrideResource = InitializeOverrideResource(securitySettings?.AuthenticationOverrideWorkflow);
            SecretKey = InitializeSecretKey(securitySettings?.SecretKey);
            PickWindowsGroupCommand = new DelegateCommand(PickWindowsGroup, o => CanPickWindowsGroup(securitySettings?.WindowsGroupPermissions));
            PickResourceCommand = new DelegateCommand(PickResource);
            PickOverrideResourceCommand = new DelegateCommand(PickOverrideResource);

            InitializeHelp();

            InitializePermissions(securitySettings?.WindowsGroupPermissions);
        }

        private string InitializeSecretKey(string securitySettingsSecretKey)
        {
            if (securitySettingsSecretKey != null)
            {
                RegisterSecretKeyPropertyChanged(securitySettingsSecretKey);
                return securitySettingsSecretKey;
            }

            return "";
        }

        private INamedGuid InitializeOverrideResource(INamedGuid securitySettingsOverrideResource)
        {
            if (securitySettingsOverrideResource != null)
            {
                var resource = new NamedGuid
                {
                    Value = securitySettingsOverrideResource.Value,
                    Name = securitySettingsOverrideResource.Name
                };
                RegisterOverrideResourcePropertyChanged(resource);
                return resource;
            }

            return new NamedGuid
            {
                Name = "",
                Value = Guid.Empty
            };
        }

        private static bool CanPickWindowsGroup(IEnumerable<WindowsGroupPermission> permissions) => permissions != null;

        public ObservableCollection<WindowsGroupPermission> ServerPermissions
        {
            get => _serverPermissions;
            private set
            {
                _serverPermissions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<WindowsGroupPermission> ResourcePermissions
        {
            get => _resourcePermissions;
            private set
            {
                _resourcePermissions = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore] public ActivityDesignerToggle ServerHelpToggle { get; private set; }

        [JsonIgnore] public ActivityDesignerToggle ResourceHelpToggle { get; private set; }

        [JsonIgnore] public ICommand PickWindowsGroupCommand { get; }

        [JsonIgnore] public ICommand PickResourceCommand { get; }
        [JsonIgnore] public ICommand PickOverrideResourceCommand { get; }

        public INamedGuid OverrideResource
        {
            get => _overrideResource;
            set
            {
                _overrideResource = value;
                OnPropertyChanged(nameof(OverrideResource));
            }
        }

        public string SecretKey
        {
            get => _secretKey;
            set
            {
                _secretKey = value;
                OnPropertyChanged(nameof(SecretKey));
            }
        }

        private bool IsValidOverrideWorkflow(IExplorerTreeItem resourceModel)
        {
            var allPermissions = ResourcePermissions.Concat(ServerPermissions);
            var overrideWorkflowPermissions = allPermissions.Where(o => o.ResourceID == Guid.Empty || o.ResourceID == resourceModel.ResourceId);
            var hasPublicViewAndExecute = overrideWorkflowPermissions
                .Where(o => o.WindowsGroup == "Public")
                .Any(o => o.Execute && o.View); //.Select(o => o.ResourceName)

            if (!hasPublicViewAndExecute)
            {
                return false;
            }

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var outputs = shellViewModel.GetOutputsFromWorkflow(resourceModel.ResourceId);
            var hasCorrectOutputs = outputs.Any(sca => sca.Recordset == "UserGroups" && sca.Field == "Name");
            return hasCorrectOutputs;
        }

        private void PickOverrideResource(object obj)
        {
            var resource = obj as NamedGuid;

            var resourceModel = PickOverrideResource(resource);
            if (resourceModel == null)
            {
                return;
            }

            if (IsValidOverrideWorkflow(resourceModel))
            {
                OverrideResource.Value = resourceModel.ResourceId;
                OverrideResource.Name = resourceModel.ResourcePath;
                var hmac = new HMACSHA256();
                SecretKey = Convert.ToBase64String(hmac.Key);
            }
            else
            {
                var popupController = CustomContainer.Get<Common.Interfaces.Studio.Controller.IPopupController>();
                if (popupController != null)
                {
                    var popupMessage = new PopupMessage
                    {
                        Description = Core.Error_Security_Auth_Override_Workflow,
                        Image = MessageBoxImage.Error,
                        Buttons = MessageBoxButton.OK,
                        IsError = true,
                        Header = @"Invalid Authentication Override"
                    };
                    popupController.Show(popupMessage);
                }

                Dev2Logger.Error(@"Invalid security override: ", new Exception(Core.Error_Security_Auth_Override_Workflow), GlobalConstants.WarewolfError);
            }
        }

        private IExplorerTreeItem PickOverrideResource(INamedGuid resource)
        {
            if (resource != null && resource.Value != Guid.Empty)
            {
                var foundResourceModel = _environment.ResourceRepository?.FindSingle(model => model.ID == resource.Value);
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

        [JsonIgnore]
        public bool IsServerHelpVisible
        {
            get => (bool) GetValue(IsServerHelpVisibleProperty);
            set => SetValue(IsServerHelpVisibleProperty, value);
        }

        [JsonIgnore] public static readonly DependencyProperty IsServerHelpVisibleProperty = DependencyProperty.Register(@"IsServerHelpVisible", typeof(bool), typeof(SecurityViewModel), new PropertyMetadata(false, IsServerHelpVisiblePropertyChanged));

        private static void IsServerHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((SecurityViewModel) d).UpdateHelpText(HelpType.Server);
        }

        [JsonIgnore]
        public bool IsResourceHelpVisible
        {
            get => (bool) GetValue(IsResourceHelpVisibleProperty);
            set => SetValue(IsResourceHelpVisibleProperty, value);
        }

        [JsonIgnore] public static readonly DependencyProperty IsResourceHelpVisibleProperty = DependencyProperty.Register(@"IsResourceHelpVisible", typeof(bool), typeof(SecurityViewModel), new PropertyMetadata(false, IsResourceHelpVisiblePropertyChanged));
        private ObservableCollection<WindowsGroupPermission> _serverPermissions;
        private ObservableCollection<WindowsGroupPermission> _resourcePermissions;
        private ObservableCollection<WindowsGroupPermission> _itemResourcePermissions;
        private ObservableCollection<WindowsGroupPermission> _itemServerPermissions;
        private INamedGuid _overrideResource;
        private string _secretKey;
        private INamedGuid _itemOverrideResource;
        private string _itemSecretKey;

        private static void IsResourceHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((SecurityViewModel) d).UpdateHelpText(HelpType.Resource);
        }

        public virtual void Save(SecuritySettingsTO securitySettings)
        {
            VerifyArgument.IsNotNull(@"securitySettings", securitySettings);

            securitySettings.WindowsGroupPermissions.Clear();
            Copy(ServerPermissions, securitySettings.WindowsGroupPermissions);
            Copy(ResourcePermissions, securitySettings.WindowsGroupPermissions);

            if (OverrideResource.Name != "")
            {
                securitySettings.AuthenticationOverrideWorkflow = new NamedGuid
                {
                    Value = OverrideResource.Value,
                    Name = OverrideResource.Name
                };
                securitySettings.SecretKey = SecretKey;
            }
            else
            {
                securitySettings.AuthenticationOverrideWorkflow = new NamedGuid();
                securitySettings.SecretKey = "";
            }

            SetItem(this);
        }

        private void Copy(IList<WindowsGroupPermission> source, IList<WindowsGroupPermission> target)
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

        private void InitializeHelp()
        {
            ServerHelpToggle = CreateHelpToggle(IsServerHelpVisibleProperty);
            ResourceHelpToggle = CreateHelpToggle(IsResourceHelpVisibleProperty);
        }

        private void InitializePermissions(IEnumerable<WindowsGroupPermission> permissions)
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

        private void PickResource(object obj)
        {
            if (!(obj is WindowsGroupPermission permission))
            {
                return;
            }

            var resourceModel = PickResource(permission);
            if (resourceModel == null)
            {
                return;
            }

            // TODO: This will be reintroduced when we allow folder and server selection
            // string resourceName;
            // string resourcePath;
            // Guid resourceId;
            // if (resourceModel is IEnvironmentViewModel environmentViewModel)
            // {
            //     resourceName = environmentViewModel.ResourceName.Replace(" (Connected)", "");
            //     resourceId = environmentViewModel.ResourceId;
            //     resourcePath = environmentViewModel.ResourcePath;
            // }
            // else
            // {
            //     resourceId = resourceModel.ResourceId;
            //     resourceName = resourceModel.ResourcePath;
            //     resourcePath = resourceModel.ResourcePath;
            // }
            // permission.ResourceID = resourceId;
            // permission.ResourceName = resourceName;
            // permission.ResourcePath = resourcePath;
            permission.ResourceID = resourceModel.ResourceId;
            permission.ResourceName = resourceModel.ResourcePath;
            permission.ResourcePath = resourceModel.ResourcePath;
        }

        private IExplorerTreeItem PickResource(IWindowsGroupPermission permission)
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

        private void PickWindowsGroup(object obj)
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

        private DirectoryObject PickWindowsGroup()
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

                Dev2Logger.Error(@"Error opening group picker: ", e, GlobalConstants.WarewolfError);
            }

            return DialogResult.Cancel;
        }

        public virtual DirectoryObject[] GetSelectedObjectsFromDirectoryObjectPickerDialog() => _directoryObjectPicker.SelectedObjects;

        private void OnAuthenticationPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            IsDirty = !Equals(ItemOverrideResource);
        }

        private void RegisterSecretKeyPropertyChanged(string secretKey)
        {
        }

        private void RegisterOverrideResourcePropertyChanged(NamedGuid authResource)
        {
            authResource.PropertyChanged += OnAuthenticationPropertyChanged;
        }

        private void RegisterPropertyChanged(WindowsGroupPermission permission)
        {
            permission.PropertyChanged += OnPermissionPropertyChanged;
        }

        private void OnPermissionPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            IsDirty = !Equals(ItemServerPermissions, ItemResourcePermissions);
            UpdateOverridingPermission(sender as WindowsGroupPermission, args.PropertyName);
            if (args.PropertyName == @"WindowsGroup" || args.PropertyName == @"ResourceName")
            {
                var permission = (WindowsGroupPermission) sender;

                if (permission.IsNew)
                {
                    AddPermission(permission);
                }
                else
                {
                    RemovePermission(permission);
                }
            }
        }

        private void RemovePermission(WindowsGroupPermission permission)
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

        private void AddPermission(IWindowsGroupPermission permission)
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

        private static void UpdateOverridingPermission(WindowsGroupPermission windowsGroupPermission, string propertyName)
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

        private static void SwitchContributePermissionsOff(IWindowsGroupPermission windowsGroupPermission, string propertyName)
        {
            if (!windowsGroupPermission.View && propertyName == @"View"
                || !windowsGroupPermission.Execute && propertyName == @"Execute")
            {
                windowsGroupPermission.Contribute = false;
            }
        }

        private static void SwitchAdminPermissionsOff(IWindowsGroupPermission windowsGroupPermission, string propertyName)
        {
            if (!windowsGroupPermission.DeployTo && propertyName == @"DeployTo"
                || !windowsGroupPermission.DeployFrom && propertyName == @"DeployFrom"
                || !windowsGroupPermission.Contribute && propertyName == @"Contribute")
            {
                windowsGroupPermission.Administrator = false;
            }
        }

        private static void UpdateForContributePermissions(IWindowsGroupPermission windowsGroupPermission)
        {
            windowsGroupPermission.View = true;
            windowsGroupPermission.Execute = true;
        }

        private static void UpdateForAdministratorPermissions(IWindowsGroupPermission windowsGroupPermission)
        {
            windowsGroupPermission.DeployFrom = true;
            windowsGroupPermission.DeployTo = true;
            windowsGroupPermission.Contribute = true;
        }

        private WindowsGroupPermission CreateNewPermission(bool isServer)
        {
            var permission = new WindowsGroupPermission {IsNew = true, IsServer = isServer};
            RegisterPropertyChanged(permission);
            return permission;
        }

        private ActivityDesignerToggle CreateHelpToggle(DependencyProperty targetProperty)
        {
            var toggle = ActivityDesignerToggle.Create(@"ServiceHelp", @"Close Help", @"ServiceHelp", @"Open Help", @"HelpToggle", this, targetProperty);
            return toggle;
        }

        protected override void CloseHelp()
        {
            IsServerHelpVisible = false;
            IsResourceHelpVisible = false;
        }

        private void UpdateHelpText(HelpType helpType)
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
                        HelpText = Warewolf.Studio.Resources.Languages.HelpText.SettingsSecurityServerHelpWindowsGroup;
                        break;

                    case HelpType.Resource:
                        IsServerHelpVisible = false;
                        HelpText = Warewolf.Studio.Resources.Languages.HelpText.SettingsSecurityResourceHelpResource;
                        break;
                    default:
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
                .GroupBy(i => new {i.ResourceID, i.WindowsGroup})
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

        public bool HasInvalidResourcePermission()
        {
            foreach (var item in ResourcePermissions.Where(perm => !perm.IsDeleted))
            {
                if ((!string.IsNullOrEmpty(item.ResourceName)
                     && string.IsNullOrEmpty(item.WindowsGroup))
                    || (string.IsNullOrEmpty(item.ResourceName)
                        && !string.IsNullOrEmpty(item.WindowsGroup)))
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        public void SetItem(SecurityViewModel model)
        {
            ItemServerPermissions = CloneServerPermissions(model.ServerPermissions);
            ItemResourcePermissions = CloneResourcePermissions(model.ResourcePermissions);
            ItemOverrideResource = CloneOverrideResourcePermissions(model.OverrideResource);
            ItemSecretKey = CloneSecretKeyPermissions(model.SecretKey);
        }

        public INamedGuid ItemOverrideResource
        {
            get => _itemOverrideResource;
            set
            {
                _itemOverrideResource = value;
                OnPropertyChanged();
            }
        }

        public string ItemSecretKey
        {
            get => _itemSecretKey;
            set
            {
                _itemSecretKey = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<WindowsGroupPermission> ItemResourcePermissions
        {
            get => _itemResourcePermissions;
            set
            {
                _itemResourcePermissions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<WindowsGroupPermission> ItemServerPermissions
        {
            get => _itemServerPermissions;
            set
            {
                _itemServerPermissions = value;
                OnPropertyChanged();
            }
        }

        private static INamedGuid CloneOverrideResourcePermissions(INamedGuid overrideResource)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(overrideResource, new JsonSerializerSettings {ContractResolver = resolver});
            var clone = JsonConvert.DeserializeObject<NamedGuid>(ser);
            return clone;
        }

        private static string CloneSecretKeyPermissions(string secretKey)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(secretKey, new JsonSerializerSettings {ContractResolver = resolver});
            var clone = JsonConvert.DeserializeObject<string>(ser);
            return clone;
        }

        private static ObservableCollection<WindowsGroupPermission> CloneResourcePermissions(ObservableCollection<WindowsGroupPermission> resourcePermissions)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(resourcePermissions, new JsonSerializerSettings {ContractResolver = resolver});
            var clone = JsonConvert.DeserializeObject<ObservableCollection<WindowsGroupPermission>>(ser);
            return clone;
        }

        private static ObservableCollection<WindowsGroupPermission> CloneServerPermissions(ObservableCollection<WindowsGroupPermission> serverPermissions)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(serverPermissions, new JsonSerializerSettings {ContractResolver = resolver});
            var clone = JsonConvert.DeserializeObject<ObservableCollection<WindowsGroupPermission>>(ser);
            return clone;
        }

        private bool Equals(ObservableCollection<WindowsGroupPermission> serverPermissions, ObservableCollection<WindowsGroupPermission> resourcePermissions)
        {
            if (ReferenceEquals(null, serverPermissions))
            {
                return false;
            }

            if (ReferenceEquals(null, resourcePermissions))
            {
                return false;
            }

            return EqualsSeq(serverPermissions, resourcePermissions);
        }

        private bool EqualsSeq(ObservableCollection<WindowsGroupPermission> serverPermissions, ObservableCollection<WindowsGroupPermission> resourcePermissions)
        {
            var serverPermissionCompare = ServerPermissionsCompare(serverPermissions, true);
            var resourcePermissionCompare = ResourcePermissionsCompare(resourcePermissions, true);
            var @equals = serverPermissionCompare && resourcePermissionCompare;

            return @equals;
        }

        private bool ServerPermissionsCompare(ObservableCollection<WindowsGroupPermission> serverPermissions, bool serverPermissionCompare)
        {
            if (_serverPermissions == null)
            {
                return true;
            }

            if (_serverPermissions.Count != serverPermissions.Count)
            {
                return false;
            }

            for (int i = 0; i < _serverPermissions.Count; i++)
            {
                if (ServerPermissions[i].ResourceName != serverPermissions[i].ResourceName)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].WindowsGroup != serverPermissions[i].WindowsGroup)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].DeployTo != serverPermissions[i].DeployTo)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].DeployFrom != serverPermissions[i].DeployFrom)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].Administrator != serverPermissions[i].Administrator)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].View != serverPermissions[i].View)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].Execute != serverPermissions[i].Execute)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].Contribute != serverPermissions[i].Contribute)
                {
                    serverPermissionCompare = false;
                }

                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerPermissions[i].IsDeleted != serverPermissions[i].IsDeleted)
                {
                    serverPermissionCompare = false;
                }
            }

            return serverPermissionCompare;
        }

        private bool ResourcePermissionsCompare(ObservableCollection<WindowsGroupPermission> resourcePermissions, bool resourcePermissionCompare)
        {
            if (_resourcePermissions == null)
            {
                return true;
            }

            if (_resourcePermissions.Count != resourcePermissions.Count)
            {
                return false;
            }

            for (int i = 0; i < _resourcePermissions.Count; i++)
            {
                if (ResourcePermissions[i].ResourceName != resourcePermissions[i].ResourceName)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].WindowsGroup != resourcePermissions[i].WindowsGroup)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].DeployTo != resourcePermissions[i].DeployTo)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].DeployFrom != resourcePermissions[i].DeployFrom)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].Administrator != resourcePermissions[i].Administrator)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].View != resourcePermissions[i].View)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].Execute != resourcePermissions[i].Execute)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].Contribute != resourcePermissions[i].Contribute)
                {
                    resourcePermissionCompare = false;
                }

                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourcePermissions[i].IsDeleted != resourcePermissions[i].IsDeleted)
                {
                    resourcePermissionCompare = false;
                }
            }

            return resourcePermissionCompare;
        }
    }
}