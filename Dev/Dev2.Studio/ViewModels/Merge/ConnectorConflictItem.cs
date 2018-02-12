/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;

namespace Dev2.ViewModels.Merge
{
    public class ConnectorConflictItem : ConflictItem, IConnectorConflictItem
    {
        public string ArmDescription { get; set; }
        public string LeftArmDescription { get; set; }
        public string RightArmDescription { get; set; }
        public Guid SourceUniqueId { get; set; }
        public Guid DestinationUniqueId { get; set; }
        public string Key { get; set; }
        bool _isChecked;

        // Add AutoSelect IsChecked
        public void SetAutoChecked()
        {
            // TODO: call this from state applier
            IsChecked = true;
            AutoChecked = true;
        }
        public bool AutoChecked { get; set; }

        public string Grouping => SourceUniqueId + Key ?? "";

        public event Action<IConnectorConflictRow, bool> OnChecked;

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set {
                SetProperty(ref _isChecked, value);
                AutoChecked = false;
            }
        }

        public bool IsArmConnectorVisible => !string.IsNullOrWhiteSpace(ArmDescription);

        public ConnectorConflictItem()
        {
            RegisterEventHandlers();
        }
        public ConnectorConflictItem(string armDescription, Guid sourceUniqueId, Guid destinationUniqueId, string key)
        {
            ArmDescription = armDescription;
            if (!string.IsNullOrWhiteSpace(armDescription))
            {
                var description = armDescription.Split(new[] { "->" }, StringSplitOptions.None);
                LeftArmDescription = description[0];
                RightArmDescription = description[1];
            }
            SourceUniqueId = sourceUniqueId;
            DestinationUniqueId = destinationUniqueId;
            Key = key;

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            PropertyChanged += (sender, eventArg) => {
                if (eventArg.PropertyName == nameof(IsChecked))
                {
                    NotifyIsCheckedChanged?.Invoke(this, IsChecked);
                }
            };
        }

        public bool Equals(IConnectorConflictItem other)
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
            if (obj is null)
            {
                return false;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((IConnectorConflictItem)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ SourceUniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (DestinationUniqueId.GetHashCode());
            return hashCode;
        }

        public event ToggledEventHandler NotifyIsCheckedChanged;
    }
}