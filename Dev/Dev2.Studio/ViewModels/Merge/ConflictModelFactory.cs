#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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

        public delegate void ModelItemChanged(ModelItem modelItem, IToolConflictItem mergeToolModel);
        public event ModelItemChanged OnModelItemChanged;

        public ConflictModelFactory(IContextualResourceModel resourceModel)
        {
            _resourceModel = resourceModel;
            WorkflowName = _resourceModel.ResourceName;
            ServerName = _resourceModel.Environment.Name;
            GetDataList(_resourceModel);
        }

        public ConflictModelFactory(IToolConflictItem toolConflictItem, IContextualResourceModel resourceModel, IConflictTreeNode conflict)
        {
            _resourceModel = resourceModel;
            CreateModelItem(toolConflictItem, conflict);
        }

        public ConflictModelFactory()
        {
        }

        public void GetDataList(IContextualResourceModel resourceModel)
        {
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(resourceModel);
            if (DataListViewModel == null)
            {
                return;
            }

            DataListViewModel.ViewSortDelete = false;

            if (DataListViewModel.ScalarCollection.Count <= 1)
            {
                UpdateScalarVisibility();
            }
            if (DataListViewModel.RecsetCollection.Count <= 1)
            {
                UpdateRecordSetVisibility();
            }
            if (DataListViewModel.ComplexObjectCollection.Count < 1)
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
        public string Header { get; set; }
        public string HeaderVersion { get; set; }
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

        public IToolConflictItem CreateModelItem(IToolConflictItem toolConflictItem, IConflictTreeNode node)
        {
            var modelItem = ModelItemUtils.CreateModelItem(node.Activity);
            var viewModel = GetViewModel(toolConflictItem, modelItem, node);

            return ConfigureToolConflictItem(toolConflictItem, modelItem, node, viewModel);
        }

        public ActivityDesignerViewModel GetViewModel(IToolConflictItem toolConflictItem, ModelItem modelItem, IConflictTreeNode node)
        {
            if (modelItem == null || node?.Activity == null)
            {
                return null;
            }

            var activityType = node.Activity.GetType();

            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out var currentType);
            DesignerAttributeMap.DeprecatedDesignerAttributes.TryGetValue(activityType, out var deprecatedType);

            var actualType = currentType ?? deprecatedType;
            if (actualType == null)
            {
                return null;
            }
            ActivityDesignerViewModel instance;
            if (actualType == typeof(SwitchDesignerViewModel))
            {
                var dsfSwitch = node.Activity as DsfSwitch;
                var switchInstance = Activator.CreateInstance(actualType, modelItem, dsfSwitch.DisplayName) as SwitchDesignerViewModel;
                switchInstance.SwitchVariable = dsfSwitch.Switch;
                toolConflictItem.MergeDescription = switchInstance.DisplayText;
                instance = switchInstance;
            }
            else if (actualType == typeof(ServiceDesignerViewModel))
            {
                var resourceId = ModelItemUtils.TryGetResourceID(modelItem);
                var childResourceModel = _resourceModel.Environment.ResourceRepository.LoadContextualResourceModel(resourceId);
                if (childResourceModel != null)
                {
                    instance = Activator.CreateInstance(actualType, modelItem, childResourceModel) as ActivityDesignerViewModel;
                }
                else
                {
                    instance = Activator.CreateInstance(actualType, modelItem, _resourceModel) as ActivityDesignerViewModel;
                }
            }
            else if (node.Activity is IAdapterActivity a)
            {
                var inode = ModelItemUtils.CreateModelItem(a.GetInnerNode());
                instance = Activator.CreateInstance(actualType, inode) as ActivityDesignerViewModel;
            }
            else
            {
                instance = Activator.CreateInstance(actualType, modelItem) as ActivityDesignerViewModel;
            }
            instance.IsMerge = true;

            return instance;
        }

        IToolConflictItem ConfigureToolConflictItem(IToolConflictItem toolConflictItem, ModelItem modelItem, IConflictTreeNode node, ActivityDesignerViewModel instance)
        {
            toolConflictItem.Activity = node.Activity;
            toolConflictItem.UniqueId = node.Activity.UniqueID.ToGuid();
            if (string.IsNullOrWhiteSpace(toolConflictItem.MergeDescription))
            {
                toolConflictItem.MergeDescription = node.Activity.GetDisplayName();
            }
            toolConflictItem.FlowNode = node.Activity.GetFlowNode();
            toolConflictItem.ModelItem = modelItem;
            toolConflictItem.NodeLocation = node.Location;

            toolConflictItem.MergeIcon = modelItem.GetImageSourceForTool();
            if (toolConflictItem is ToolConflictItem toolConflictItemObject)
            {
                toolConflictItemObject.ActivityDesignerViewModel = instance;
            }

            modelItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ModelItem))
                {
                    OnModelItemChanged?.Invoke(modelItem, toolConflictItem);
                }
            };
            return toolConflictItem;
        }

        public event ConflictModelChanged SomethingConflictModelChanged;
    }

    
}