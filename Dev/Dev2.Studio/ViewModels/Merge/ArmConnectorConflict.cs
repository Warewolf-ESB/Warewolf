/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.ViewModels.Merge
{
    public class ArmConnectorConflict : BindableBase, IArmConnectorConflict
    {
        bool _hasConflict;
        bool _isMergeExpanderEnabled;
        public IMergeArmConnectorConflict CurrentArmConnector { get; set; }
        public IMergeArmConnectorConflict DifferentArmConnector { get; set; }
        public bool HasConflict
        {
            get => _hasConflict;
            set
            {
                _hasConflict = value;
                OnPropertyChanged(() => HasConflict);
            }
        }
        public bool IsChecked { get; set; }
        public Guid UniqueId { get; set; }
        public string Key { get; set; }
        public bool IsMergeExpanderEnabled
        {
            get => _isMergeExpanderEnabled && HasConflict;
            set
            {
                _isMergeExpanderEnabled = value;
                OnPropertyChanged(() => IsMergeExpanderEnabled);
            }
        }

        public bool IsEmptyItemSelected { get; set; }

        public bool Equals(IArmConnectorConflict other)
        {
            if (other == null)
            {
                return false;
            }
            var equals = true;
            equals &= (other.CurrentArmConnector?.Equals(CurrentArmConnector)).GetValueOrDefault(false);
            equals &= (other.DifferentArmConnector?.Equals(DifferentArmConnector)).GetValueOrDefault(false);
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((IArmConnectorConflict)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ UniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            return hashCode;
        }
    }
}
