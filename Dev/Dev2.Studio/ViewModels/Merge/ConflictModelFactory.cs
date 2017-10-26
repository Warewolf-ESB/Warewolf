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
        private bool _isWorkflowNameChecked;
        private bool _isVariablesChecked;

        public IMergeToolModel Model { get; set; }
        public delegate void ModelItemChanged(ModelItem modelItem, MergeToolModel mergeToolModel);
        public event ModelItemChanged OnModelItemChanged;

        public ConflictModelFactory(IContextualResourceModel resourceModel, IConflictTreeNode conflict)
        {
            Children = new ObservableCollection<IMergeToolModel>();
            _resourceModel = resourceModel;
            var modelItem = ModelItemUtils.CreateModelItem(conflict.Activity);
            Model = GetModel(modelItem, conflict,null);           
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
                DataListViewModel.ScalarCollection?.Apply(model =>
                {
                    model.IsVisible = false;
                    model.IsExpanded = false;
                    model.IsEditable = false;
                });
            }
            if (DataListViewModel.RecsetCollection?.Count <= 1)
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
            if (DataListViewModel.ComplexObjectCollection?.Count <= 1)
            {
                DataListViewModel.ComplexObjectCollection?.Apply(model =>
                {
                    model.IsVisible = false;
                    model.Children?.Flatten(a => a.Children).Apply(a => a.IsVisible = false);
                });
            }
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
                   

        public IMergeToolModel GetModel(ModelItem modelItem, IConflictTreeNode node, IMergeToolModel parentItem,string parentLabelDescription = "")
        {
            if (modelItem == null || node == null || node.Activity==null)
            {
                return null;
            }

            var activityType = node.Activity.GetType();            

            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out var actual);
            if (actual != null)
            {
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

                var mergeToolModel = new MergeToolModel
                {
                    ActivityDesignerViewModel = instance,
                    MergeIcon = modelItem.GetImageSourceForTool(),
                    MergeDescription = node.Activity.GetDisplayName(),
                    UniqueId = node.Activity.UniqueID.ToGuid(),
                    ActivityType = node.Activity.GetFlowNode(),
                    IsMergeVisible = node.IsInConflict,
                    FlowNode = modelItem,
                    NodeLocation = node.Location,
                    Parent = parentItem,
                    HasParent = parentItem != null,
                    ParentDescription = parentLabelDescription,
                    IsTrueArm = parentLabelDescription?.ToLowerInvariant() == "true"
            };
                
                modelItem.PropertyChanged += (sender, e) =>
                {
                    OnModelItemChanged?.Invoke(modelItem, mergeToolModel);
                };
                
                return mergeToolModel;
            }
            return null;
        }

        public event ConflictModelChanged SomethingConflictModelChanged;
    }
}