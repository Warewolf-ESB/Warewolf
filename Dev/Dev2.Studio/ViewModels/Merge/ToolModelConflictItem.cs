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

namespace Dev2.ViewModels.Merge
{
    public class ToolModelConflictItem : ConflictItem, IToolModelConflictItem, ICheckable
    {
        ImageSource _mergeIcon;
        string _mergeDescription;
        bool _isChecked;
        bool _isMergeVisible;
        Guid _uniqueId;
        FlowNode _flowNode;
        string _nodeArmDescription;

        public ToolModelConflictItem()
        {
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

        public bool IsChecked
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

        public event ToolModelChangedHandler NotifyToolModelChanged;

        public event ToggledEventHandler NotifyIsCheckedChanged;
    }
}
