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
using Dev2.Studio.Interfaces;

namespace Dev2.ViewModels.Merge
{
    public class MergeArmConnectorConflict : BindableBase, IMergeArmConnectorConflict
    {
        public string ArmDescription { get; set; }
        public string SourceUniqueId { get; set; }
        public string DestinationUniqueId { get; set; }
        public IArmConnectorConflict Container { get; set; }
        public string Key { get; set; }
        bool _isChecked;
        bool _isArmSelectionAllowed;

        public event Action<IArmConnectorConflict, bool> OnChecked;

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                if (_isChecked)
                {
                    WorkflowDesignerViewModel?.LinkTools(SourceUniqueId, DestinationUniqueId, Key);
                    OnChecked?.Invoke(Container, _isChecked);
                }
                OnPropertyChanged(() => IsChecked);
            }
        }

        public bool IsArmSelectionAllowed
        {
            get => _isArmSelectionAllowed && Container.HasConflict;
            set
            {
                _isArmSelectionAllowed = value;
                OnPropertyChanged(() => IsArmSelectionAllowed);
            }
        }

        public MergeArmConnectorConflict(IArmConnectorConflict container)
        {
            Container = container;
        }
        public MergeArmConnectorConflict(string armDescription, string sourceUniqueId, string destinationUniqueId,string key, IArmConnectorConflict container)
        {
            ArmDescription = armDescription;
            SourceUniqueId = sourceUniqueId;
            DestinationUniqueId = destinationUniqueId;
            Key = key;
            Container = container;
        }

        public bool Equals(IMergeArmConnectorConflict other)
        {
            if (other == null)
            {
                return false;
            }
            var equals = true;
            equals &= other.SourceUniqueId == SourceUniqueId;
            equals &= other.DestinationUniqueId == DestinationUniqueId;
            equals &= other.Key == Key;
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
            return Equals((IMergeArmConnectorConflict)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ SourceUniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (DestinationUniqueId != null ? DestinationUniqueId.GetHashCode() : 0);
            return hashCode;
        }

        public void DisableEvents()
        {
            IsArmSelectionAllowed = false;
            IsChecked = false;
        }
    }
}
