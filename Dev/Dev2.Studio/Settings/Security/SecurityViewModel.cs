using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Data.Settings.Security;
using Dev2.Dialogs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Settings.Security
{
    public class SecurityViewModel : ObservableObject
    {
        readonly IResourcePickerDialog _resourcePicker;
        readonly IDirectoryObjectPickerDialog _directoryObjectPicker;
        readonly IWin32Window _parentWindow;
        bool _isDirty;

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
            permission.ResourceName = string.Format("{0}\\{1}", resourceModel.Category, resourceModel.ResourceName);
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

        WindowsGroupPermission CreateNewPermission(bool isServer)
        {
            var permission = new WindowsGroupPermission { IsNew = true, IsServer = isServer };
            RegisterPropertyChanged(permission);
            return permission;
        }
    }
}
