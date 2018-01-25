﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Activities;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.Switch;
using Dev2.Common.ExtMethods;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.ViewModels.Merge
{
    public class ConflictModelFactory : BindableBase, IConflictModelFactory
    {
        readonly IContextualResourceModel _resourceModel;
        bool _isWorkflowNameChecked;
        bool _isVariablesChecked;

        public IMergeToolModel Model { get; set; }
        public delegate void ModelItemChanged(ModelItem modelItem, MergeToolModel mergeToolModel);
        public event ModelItemChanged OnModelItemChanged;

        public ConflictModelFactory(IContextualResourceModel resourceModel, IConflictTreeNode conflict, IWorkflowDesignerViewModel workflowDesignerViewModel)
        {
            Children = new ObservableCollection<IMergeToolModel>();
            _resourceModel = resourceModel;
            var modelItem = ModelItemUtils.CreateModelItem(conflict.Activity);
            Model = GetModel(modelItem, conflict, null, workflowDesignerViewModel);
        }

        public ConflictModelFactory()
        {
            Children = new ObservableCollection<IMergeToolModel>();
        }

        public void GetDataList(IContextualResourceModel resourceModel)
        {
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(resourceModel);
            if (DataListViewModel == null)
            {
                return;
            }

            DataListViewModel.ViewSortDelete = false;

            if (DataListViewModel.ScalarCollection?.Count <= 1)
            {
                UpdateScalarVisibility();
            }
            if (DataListViewModel.RecsetCollection?.Count <= 1)
            {
                UpdateRecordSetVisibility();
            }
            if (DataListViewModel.ComplexObjectCollection?.Count < 1)
            {
                UpdateComplexObjectVisibility();
            }
        }

        void UpdateScalarVisibility()
        {
            DataListViewModel.ScalarCollection?.Apply(model =>
            {
                model.IsVisible = false;
                model.IsExpanded = false;
                model.IsEditable = false;
            });
        }

        void UpdateRecordSetVisibility()
        {
            DataListViewModel.RecsetCollection?.Apply(model =>
            {
                model.IsVisible = false;
                model.IsExpanded = false;
                model.IsEditable = false;
                model.Children?.Apply(child =>
                {
                    child.IsVisible = false;
                    child.IsExpanded = false;
                    child.IsEditable = false;
                });
            });
        }

        void UpdateComplexObjectVisibility()
        {
            DataListViewModel.ComplexObjectCollection?.Apply(model =>
            {
                model.IsVisible = false;
                model.Children?.Flatten(a => a.Children).Apply(a => a.IsVisible = false);
            });
        }

        public string WorkflowName { get; set; }
        public string ServerName { get; set; }
        public bool IsVariablesChecked
        {
            get => _isVariablesChecked;
            set
            {
                _isVariablesChecked = value;
                OnPropertyChanged(() => IsVariablesChecked);
                SomethingConflictModelChanged?.Invoke(this, this);
            }
        }
        public bool IsWorkflowNameChecked
        {
            get => _isWorkflowNameChecked;
            set
            {
                _isWorkflowNameChecked = value;
                OnPropertyChanged(() => IsWorkflowNameChecked);
                SomethingConflictModelChanged?.Invoke(this, this);
            }
        }
        public IDataListViewModel DataListViewModel { get; set; }
        public ObservableCollection<IMergeToolModel> Children { get; set; }

        public IMergeToolModel GetModel(ModelItem modelItem, IConflictTreeNode node, IMergeToolModel parentItem, IWorkflowDesignerViewModel workflowDesignerViewModel, string parentLabelDescription = "")
        {
            if (modelItem == null || node == null || node.Activity == null)
            {
                return null;
            }

            var activityType = node.Activity.GetType();

            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out var actual);
            if (actual == null)
            {
                return null;
            }
            ActivityDesignerViewModel instance;
            if (actual == typeof(SwitchDesignerViewModel))
            {
                var dsfSwitch = node as DsfSwitch;
                instance = Activator.CreateInstance(actual, modelItem, dsfSwitch?.Switch ?? "") as ActivityDesignerViewModel;
            }
            else if (actual == typeof(ServiceDesignerViewModel))
            {
                var resourceId = ModelItemUtils.TryGetResourceID(modelItem);
                var childResourceModel = _resourceModel.Environment.ResourceRepository.LoadContextualResourceModel(resourceId);
                instance = Activator.CreateInstance(actual, modelItem, childResourceModel) as ActivityDesignerViewModel;
            }
            else if (node.Activity is IAdapterActivity a)
            {
                var inode = ModelItemUtils.CreateModelItem(a.GetInnerNode());
                instance = Activator.CreateInstance(actual, inode) as ActivityDesignerViewModel;
            }
            else
            {
                instance = Activator.CreateInstance(actual, modelItem) as ActivityDesignerViewModel;
            }
            instance.IsMerge = true;

            var mergeToolModel = CreateNewMergeToolModel(modelItem, node, parentItem, parentLabelDescription, instance, workflowDesignerViewModel);
            return mergeToolModel;
        }

        MergeToolModel CreateNewMergeToolModel(ModelItem modelItem, IConflictTreeNode node, IMergeToolModel parentItem, string parentLabelDescription, ActivityDesignerViewModel instance, IWorkflowDesignerViewModel workflowDesignerViewModel)
        {
            var mergeToolModel = new MergeToolModel
            {
                ActivityDesignerViewModel = instance,
                WorkflowDesignerViewModel = workflowDesignerViewModel,
                MergeIcon = modelItem.GetImageSourceForTool(),
                MergeDescription = node.Activity.GetDisplayName(),
                UniqueId = node.Activity.UniqueID.ToGuid(),
                FlowNode = node.Activity.GetFlowNode(),
                IsMergeVisible = node.IsInConflict,
                ModelItem = modelItem,
                NodeLocation = node.Location,
                Parent = parentItem,
                HasParent = parentItem != null,
                ParentDescription = parentLabelDescription,
                IsTrueArm = parentLabelDescription?.ToLowerInvariant() == "true",
                NodeArmDescription = node.Activity.GetDisplayName() + " -> " + " Assign",
            };

            modelItem.PropertyChanged += (sender, e) =>
            {
                OnModelItemChanged?.Invoke(modelItem, mergeToolModel);
            };
            return mergeToolModel;
        }

        public event ConflictModelChanged SomethingConflictModelChanged;
    }
}