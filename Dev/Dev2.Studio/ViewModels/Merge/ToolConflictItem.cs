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
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;
using System.Activities.Statements;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Collections.Generic;

namespace Dev2.ViewModels.Merge
{
    public class ToolConflictItem : ConflictItem, IToolConflictItem, ICheckable
    {
        ImageSource _mergeIcon;
        string _mergeDescription;
        bool _isChecked;
        bool _isMergeVisible;
        Guid _uniqueId;
        FlowNode _flowNode;
        string _nodeArmDescription;

        public ToolConflictItem()
        {
            NotifyIsCheckedChanged += PropagateCheckedState;
        }
        private void PropagateCheckedState(IConflictItem item, bool isChecked)
        {
            if (isChecked)
            {
                NotifyToolModelChanged?.Invoke(this);
            }
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

        [JsonIgnore]
        public ImageSource MergeIcon
        {
            get => _mergeIcon;
            set
            {
                _mergeIcon = value;
                OnPropertyChanged(() => MergeIcon);
            }
        }
        public string MergeDescription
        {
            get => _mergeDescription;
            set
            {
                _mergeDescription = value;
                OnPropertyChanged(() => MergeDescription);
            }
        }

        // Add AutoSelect IsChecked

        public override bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public bool IsMergeVisible
        {
            get => _isMergeVisible;
            set
            {
                _isMergeVisible = value;
                OnPropertyChanged(() => IsMergeVisible);
            }
        }

        public Guid UniqueId
        {
            get => _uniqueId;
            set
            {
                _uniqueId = value;
                OnPropertyChanged(() => UniqueId);
            }
        }

        public FlowNode FlowNode
        {
            get => _flowNode;
            set
            {
                _flowNode = value;
                OnPropertyChanged(() => FlowNode);
            }
        }

        public ModelItem ModelItem { get; set; }
        public Point NodeLocation { get; set; }

        public string NodeArmDescription
        {
            get => _nodeArmDescription;
            set
            {
                _nodeArmDescription = value;
                OnPropertyChanged(() => NodeArmDescription);
            }
        }

        public event Action<IToolConflictItem> NotifyToolModelChanged;

        public override bool Equals(object obj)
        {
            var item = obj as ToolConflictItem;
            return item != null &&
                   MergeDescription == item.MergeDescription &&
                   UniqueId.Equals(item.UniqueId) &&
                   EqualityComparer<ModelItem>.Default.Equals(ModelItem, item.ModelItem);
        }

        public override int GetHashCode()
        {
            var hashCode = 1531919647;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MergeDescription);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(UniqueId);
            hashCode = hashCode * -1521134295 + EqualityComparer<ModelItem>.Default.GetHashCode(ModelItem);
            return hashCode;
        }
    }
}
