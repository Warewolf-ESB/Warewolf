#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Newtonsoft.Json;

namespace Dev2.Services.Security
{
    public class WindowsGroupPermission : ObservableObject, IWindowsGroupPermission
    {
        public WindowsGroupPermission()
        {
            EnableCellEditing = true;
            CanChangeName = true;
        }

        public const string BuiltInAdministratorsText = "Warewolf Administrators";
        public const string AdministratorsText = "BuiltIn\\Administrators";
        public const string BuiltInGuestsText = "Public";

        bool _isServer;
        Guid _resourceId;
        string _resourceName;
        string _windowsGroup;
        bool _view;
        bool _execute;
        bool _contribute;
        bool _deployTo;
        bool _deployFrom;
        bool _administrator;
        bool _isNew;
        bool _isDeleted;
        RelayCommand _removeRow;
        bool _enableCellEditing;
        bool _canChangeName;

        public bool IsServer
        {
            get
            {
                return _isServer;
            }
            set
            {
                OnPropertyChanged(ref _isServer, value);
                RemoveRow.RaiseCanExecuteChanged();
            }
        }

        
        public Guid ResourceID { get => _resourceId; set => OnPropertyChanged(ref _resourceId, value); }


        public string ResourceName { get => _resourceName; set => OnPropertyChanged(ref _resourceName, value); }

        public string WindowsGroup
        {
            get
            {
                return _windowsGroup;
            }
            set
            {
                OnPropertyChanged(ref _windowsGroup, value);
                OnPropertyChanged("EnableCellEditing");
                OnPropertyChanged("CanRemove");
                OnPropertyChanged("CanChangeName");
                RemoveRow.RaiseCanExecuteChanged();
            }
        }

        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                OnPropertyChanged(ref _isDeleted, value);
                OnPropertyChanged("CanChangeName");
            }
        }

        public bool CanChangeName
        {
            get
            {
                if (IsBuiltInAdministrators || IsBuiltInGuests || IsDeleted)
                {
                    return false;
                }
                return _canChangeName;
            }
            set
            {
                _canChangeName = value;
            }
        }

        public bool EnableCellEditing
        {
            get
            {
                if (IsBuiltInAdministrators)
                {
                    return false;
                }
                return _enableCellEditing;
            }
            set
            {
                
                OnPropertyChanged(ref _enableCellEditing, value);
            }
        }

        public RelayCommand RemoveRow => _removeRow ??
                       (_removeRow =
                       new RelayCommand(o =>
                           {
                               IsDeleted = !IsDeleted;
                               EnableCellEditing = !IsDeleted;
                           }, o => CanRemove));

        public bool CanRemove => !string.IsNullOrEmpty(WindowsGroup) && !IsBuiltInGuests && !IsBuiltInAdministrators;


        public bool View { get => _view; set => OnPropertyChanged(ref _view, value); }

        public bool Execute { get => _execute; set => OnPropertyChanged(ref _execute, value); }

        public bool Contribute { get => _contribute; set => OnPropertyChanged(ref _contribute, value); }

        public bool DeployTo { get => _deployTo; set => OnPropertyChanged(ref _deployTo, value); }

        public bool DeployFrom { get => _deployFrom; set => OnPropertyChanged(ref _deployFrom, value); }

        public bool Administrator { get => _administrator; set => OnPropertyChanged(ref _administrator, value); }

        public bool IsNew { get => _isNew; set => OnPropertyChanged(ref _isNew, value); }

        [JsonIgnore]
        public Permissions Permissions
        {
            get
            {
                var result = Permissions.None;
                if (View)
                {
                    result |= Permissions.View;
                }
                if (Execute)
                {
                    result |= Permissions.Execute;
                }
                if(Contribute)
                {
                    result |= Permissions.Contribute;
                }
                if(DeployTo)
                {
                    result |= Permissions.DeployTo;
                }
                if(DeployFrom)
                {
                    result |= Permissions.DeployFrom;
                }
                if(Administrator)
                {
                    result |= Permissions.Administrator;
                }

                return result;
            }
            set
            {
                View = (value & Permissions.View) == Permissions.View;
                Execute = (value & Permissions.Execute) == Permissions.Execute;
                Contribute = (value & Permissions.Contribute) == Permissions.Contribute;
                DeployTo = (value & Permissions.DeployTo) == Permissions.DeployTo;
                DeployFrom = (value & Permissions.DeployFrom) == Permissions.DeployFrom;
                Administrator = (value & Permissions.Administrator) == Permissions.Administrator;
            }
        }

        [JsonIgnore]
        public bool IsBuiltInAdministrators => WindowsGroup != null && IsServer && WindowsGroup.Equals(BuiltInAdministratorsText, StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        public bool IsBuiltInGuests => IsServer && IsBuiltInGuestsForExecution;

        [JsonIgnore]
        public bool IsBuiltInGuestsForExecution => WindowsGroup != null && WindowsGroup.Equals(BuiltInGuestsText, StringComparison.InvariantCultureIgnoreCase);

        [JsonIgnore]
        public bool IsValid => IsServer
            ? !string.IsNullOrEmpty(WindowsGroup)
            : !string.IsNullOrEmpty(WindowsGroup) && !string.IsNullOrEmpty(ResourceName);

        public static WindowsGroupPermission CreateAdministrators() => new WindowsGroupPermission
        {
            IsServer = true,
            WindowsGroup = BuiltInAdministratorsText,
            View = true,
            Execute = true,
            Contribute = true,
            DeployTo = true,
            DeployFrom = true,
            Administrator = true

        };

        public static WindowsGroupPermission CreateEveryone()
        {
            return new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Everyone",
                View = true,
                Execute = true,
                Contribute = true,
                DeployTo = true,
                DeployFrom = true,
                Administrator = true
            }
;
        }

        public static WindowsGroupPermission CreateGuests() => new WindowsGroupPermission
        {
            IsServer = true,
            WindowsGroup = BuiltInGuestsText,
            View = false,
            Execute = false,
            Contribute = false,
            DeployTo = false,
            DeployFrom = false,
            Administrator = false

        };
    }
}
