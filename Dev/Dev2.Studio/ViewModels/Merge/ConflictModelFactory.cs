#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
            if (modelItem == null || node == null || node.Activity == null)
            {
                return null;
            }

            var activityType = node.Activity.GetType();

            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out Type actualType);
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