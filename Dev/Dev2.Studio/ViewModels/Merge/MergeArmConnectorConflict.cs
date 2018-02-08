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
    public class MergeArmConnectorConflict : ConflictItem, IConnectorConflictItem
    {
        public string ArmDescription { get; set; }
        public string LeftArmDescription { get; set; }
        public string RightArmDescription { get; set; }
        public Guid SourceUniqueId { get; set; }
        public Guid DestinationUniqueId { get; set; }
        public IArmConnectorConflict Container { get; set; }
        public string Key { get; set; }
        bool _isChecked;

        public event Action<IArmConnectorConflict, bool> OnChecked;

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public bool IsArmConnectorVisible => !string.IsNullOrWhiteSpace(ArmDescription);

        public MergeArmConnectorConflict(IArmConnectorConflict container)
        {
            Container = container;
            RegisterEventHandlers();
        }
        public MergeArmConnectorConflict(string armDescription, Guid sourceUniqueId, Guid destinationUniqueId, string key, IArmConnectorConflict container)
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
            Container = container;

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

            NotifyIsCheckedChanged += AddRemoveActivityHandler;
        }

        private void AddRemoveActivityHandler(IConflictItem item, bool isChecked)
        {
            if (isChecked)
            {
                // TODO: Move to StateApplier?
                if (Key == "Start")
                {
                    WorkflowDesignerViewModel?.RemoveStartNodeConnection();
                    // TODO: Pass in IMergeToolModel
                    WorkflowDesignerViewModel?.AddStartNode(null);
                }
                if (string.IsNullOrEmpty(ArmDescription))
                {
                    DeLinkActivities();
                }
                else
                {
                    LinkActivities();
                }

                OnChecked?.Invoke(Container, _isChecked);
            }
        }

        private void DeLinkActivities()
        {
            WorkflowDesignerViewModel?.DeLinkActivities(SourceUniqueId, DestinationUniqueId, Key);
        }

        private void LinkActivities()
        {
            WorkflowDesignerViewModel?.LinkActivities(SourceUniqueId, DestinationUniqueId, Key);
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