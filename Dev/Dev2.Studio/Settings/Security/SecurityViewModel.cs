using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Activities.Designers2.Core;
using Dev2.Data.Settings.Security;
using Dev2.Dialogs;
using Dev2.Help;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : DependencyObject
    {
        readonly IResourcePickerDialog _resourcePicker;
        readonly IDirectoryObjectPickerDialog _directoryObjectPicker;
        readonly IWin32Window _parentWindow;
        bool _isUpdatingHelpText;

        internal SecurityViewModel(IEnumerable<WindowsGroupPermission> permissions, IWin32Window parentWindow)
            : this(permissions, new ResourcePickerDialog(), new DirectoryObjectPickerDialog(), parentWindow)
        {
        }

        public SecurityViewModel(IEnumerable<WindowsGroupPermission> permissions, IResourcePickerDialog resourcePicker, IDirectoryObjectPickerDialog directoryObjectPicker, IWin32Window parentWindow)
        {
            VerifyArgument.IsNotNull("permissions", permissions);
            VerifyArgument.IsNotNull("resourcePicker", resourcePicker);
            VerifyArgument.IsNotNull("directoryObjectPicker", directoryObjectPicker);
            VerifyArgument.IsNotNull("parentWindow", parentWindow);

            _resourcePicker = resourcePicker;
            _directoryObjectPicker = directoryObjectPicker;
            _parentWindow = parentWindow;
            _directoryObjectPicker.AllowedObjectTypes = ObjectTypes.BuiltInGroups | ObjectTypes.Groups;
            _directoryObjectPicker.DefaultObjectTypes = ObjectTypes.Groups;
            _directoryObjectPicker.AllowedLocations = Locations.All;
            _directoryObjectPicker.DefaultLocations = Locations.JoinedDomain;
            _directoryObjectPicker.MultiSelect = false;
            _directoryObjectPicker.TargetComputer = string.Empty;
            _directoryObjectPicker.ShowAdvancedView = false;

            PickWindowsGroupCommand = new RelayCommand(PickWindowsGroup, o => true);
            PickResourceCommand = new RelayCommand(PickResource, o => true);

            InitializeHelp();
            InitializePermissions(permissions);
        }

        public ObservableCollection<WindowsGroupPermission> ServerPermissions { get; private set; }

        public ObservableCollection<WindowsGroupPermission> ResourcePermissions { get; private set; }

        public ActivityDesignerToggle ServerHelpToggle { get; private set; }

        public ActivityDesignerToggle ResourceHelpToggle { get; private set; }

        public ICommand PickWindowsGroupCommand { get; private set; }

        public ICommand PickResourceCommand { get; private set; }

        public ICommand CloseHelpCommand { get; private set; }

        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register("HelpText", typeof(string), typeof(SecurityViewModel), new PropertyMetadata(null));

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register("IsDirty", typeof(bool), typeof(SecurityViewModel), new PropertyMetadata(false));

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

        public virtual void Save(List<WindowsGroupPermission> permissions)
        {
            VerifyArgument.IsNotNull("permissions", permissions);
            permissions.Clear();
            Copy(ServerPermissions, permissions);
            Copy(ResourcePermissions, permissions);
        }

        void Copy(IList<WindowsGroupPermission> source, IList<WindowsGroupPermission> target)
        {
            for(var i = source.Count - 1; i >= 0; i--)
            {
                var permission = source[i];
                if(permission.IsValid)
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
            CloseHelpCommand = new RelayCommand(o => CloseHelp(), o => true);
        }

        void InitializePermissions(IEnumerable<WindowsGroupPermission> permissions)
        {
            ServerPermissions = new ObservableCollection<WindowsGroupPermission>();
            ResourcePermissions = new ObservableCollection<WindowsGroupPermission>();

            foreach(var permission in permissions)
            {
                RegisterPropertyChanged(permission);
                if(permission.IsServer)
                {
                    ServerPermissions.Add(permission);
                }
                else
                {
                    ResourcePermissions.Add(permission);
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

            var resourceModel = PickResource();
            if(resourceModel == null)
            {
                return;
            }

            permission.ResourceID = resourceModel.ID;
            permission.ResourceName = string.Format("{0}\\{1}\\{2}", resourceModel.ResourceType.GetTreeDescription(), resourceModel.Category, resourceModel.ResourceName);
        }

        IResourceModel PickResource()
        {
            var dialogResult = _resourcePicker.ShowDialog();
            return dialogResult != DialogResult.OK ? null : _resourcePicker.SelectedResource;
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
            var dialogResult = _directoryObjectPicker.ShowDialog(_parentWindow);
            if(dialogResult != DialogResult.OK)
            {
                return null;
            }
            var results = _directoryObjectPicker.SelectedObjects;
            if(results == null || results.Length == 0)
            {
                return null;
            }
            return results[0];
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
                    //var isEmpty = permission.IsServer
                    //    ? string.IsNullOrEmpty(permission.WindowsGroup)
                    //    : string.IsNullOrEmpty(permission.WindowsGroup) && string.IsNullOrEmpty(permission.ResourceName);
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

        WindowsGroupPermission CreateNewPermission(bool isServer)
        {
            var permission = new WindowsGroupPermission { IsNew = true, IsServer = isServer };
            RegisterPropertyChanged(permission);
            return permission;
        }

        ActivityDesignerToggle CreateHelpToggle(DependencyProperty targetProperty)
        {
            var toggle = ActivityDesignerToggle.Create(
                collapseImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png",
                collapseToolTip: "Close Help",
                expandImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceHelp-32.png",
                expandToolTip: "Open Help",
                automationID: "HelpToggle",
                target: this,
                dp: targetProperty
                );

            return toggle;
        }

        void CloseHelp()
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
                        HelpText = HelpTextResources.SettingsSecurityHelpServerPermissions;
                        break;

                    case HelpType.Resource:
                        IsServerHelpVisible = false;
                        HelpText = HelpTextResources.SettingsSecurityHelpResourcePermissions;
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
    }

}
