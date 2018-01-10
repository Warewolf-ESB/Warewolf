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
using System.Collections.ObjectModel;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System.Activities.Statements;
using System.Activities.Presentation.Model;
using System.Windows;
using Dev2.Studio.Interfaces;

namespace Dev2.ViewModels.Merge
{
    public class MergeToolModel : BindableBase, IMergeToolModel
    {
        ImageSource _mergeIcon;
        string _mergeDescription;
        bool _isMergeChecked;
        bool _isMergeEnabled;
        bool _isMergeVisible;
        ObservableCollection<IMergeToolModel> _children;
        string _parentDescription;
        bool _hasParent;
        Guid _uniqueId;
        FlowNode _flowNode;
        IMergeToolModel _parent;
        string _nodeArmDescription;

        public MergeToolModel()
        {
            Children = new ObservableCollection<IMergeToolModel>();
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel { get; set; }

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
        public bool IsMergeChecked
        {
            get => _isMergeChecked;
            set
            {
                var current = MemberwiseClone();
                _isMergeChecked = value;

                if (_isMergeChecked)
                {
                    RemovePreviousActivity();
                    AddActivity();
                    SomethingModelToolChanged?.Invoke(current, this);
                }
                OnPropertyChanged(() => IsMergeChecked);
            }
        }

        void RemovePreviousActivity()
        {
            if (Container?.CurrentViewModel == this)
            {
                WorkflowDesignerViewModel?.RemoveItem(Container.DiffViewModel);
            }
            if (Container?.DiffViewModel == this)
            {
                WorkflowDesignerViewModel?.RemoveItem(Container.CurrentViewModel);
            }
        }

        void AddActivity()
        {
            if (Container != null && Container.IsStartNode)
            {
                WorkflowDesignerViewModel?.RemoveStartNodeConnection();
            }
            if (ModelItem != null)
            {
                WorkflowDesignerViewModel?.AddItem(this);
                if (WorkflowDesignerViewModel != null)
                {
                    WorkflowDesignerViewModel.SelectedItem = ModelItem;
                }
            }
        }

        public bool IsMergeEnabled
        {
            get => _isMergeEnabled && Container.HasConflict;
            set
            {
                _isMergeEnabled = value;
                OnPropertyChanged(() => IsMergeEnabled);
            }
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

        public IToolConflict Container { get; set; }

        public IMergeToolModel Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                OnPropertyChanged(() => Parent);
            }
        }

        public ObservableCollection<IMergeToolModel> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
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

        public string ParentDescription
        {
            get => _parentDescription;
            set
            {
                _parentDescription = value;
                OnPropertyChanged(() => ParentDescription);
            }
        }
        public bool HasParent
        {
            get => _hasParent;
            set
            {
                _hasParent = value;
                OnPropertyChanged(() => HasParent);
            }
        }

        public ModelItem ModelItem { get; set; }
        public Point NodeLocation { get; set; }

        public bool IsTrueArm { get; set; }
        public string NodeArmDescription
        {
            get => _nodeArmDescription;
            set
            {
                _nodeArmDescription = value;
                OnPropertyChanged(() => NodeArmDescription);
            }
        }

        public event ModelToolChanged SomethingModelToolChanged;

        public void DisableEvents()
        {
            IsMergeEnabled = false;
            IsMergeChecked = false;
        }
    }
}