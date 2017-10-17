using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Activities;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.Switch;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.ExtMethods;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Activities.Statements;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.ViewModels.Merge
{
    public class ConflictModelFactory : BindableBase, IConflictModelFactory
    {
        readonly IActivityParser _activityParser;
        readonly IContextualResourceModel _resourceModel;
        bool _isWorkflowNameChecked;
        bool _isVariablesChecked;

        public IMergeToolModel Model { get; set; }
        public ConflictModelFactory(IConflictNode conflictNode, IContextualResourceModel resourceModel)
            : this(CustomContainer.Get<IActivityParser>())
        {
            Children = new ObservableCollection<IMergeToolModel>();            
            _resourceModel = resourceModel;
            Model = GetModel(conflictNode.CurrentActivity, conflictNode.Activity,null,"");
        }


        public ConflictModelFactory(IActivityParser activityParser)
        {
            _activityParser = activityParser;
        }
        public ConflictModelFactory()
        {
            Children = new ObservableCollection<IMergeToolModel>();
        }

        public void GetDataList()
        {
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(_resourceModel);
            if (DataListViewModel != null)
            {
                DataListViewModel.ViewSortDelete = false;

                if (DataListViewModel.ScalarCollection.Count <= 1)
                {
                    foreach (var scalar in DataListViewModel.ScalarCollection)
                    {
                        scalar.IsVisible = false;
                        scalar.IsExpanded = false;
                        scalar.IsEditable = false;
                    }
                }
                if (DataListViewModel.RecsetCollection.Count <= 1)
                {
                    foreach (var recordset in DataListViewModel.RecsetCollection)
                    {
                        recordset.IsVisible = false;
                        recordset.IsExpanded = false;
                        recordset.IsEditable = false;
                        foreach (var child in recordset.Children)
                        {
                            child.IsVisible = false;
                            child.IsExpanded = false;
                            child.IsEditable = false;
                        }
                    }
                    DataListViewModel.RecsetCollection.Clear();
                }
                if (DataListViewModel.ComplexObjectCollection.Count <= 1)
                {
                    foreach (var complexobject in DataListViewModel.ComplexObjectCollection)
                    {
                        complexobject.IsVisible = false;
                        complexobject.Children.Flatten(a => a.Children).Apply(a => a.IsVisible = false);
                    }
                }
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


        public IMergeToolModel GetModel(ModelItem modelItem, IDev2Activity activity,IMergeToolModel parentItem,string parentLableDescription)
        {
            if (modelItem == null)
            {
                return null;
            }

            var currentValue = modelItem.GetCurrentValue<IDev2Activity>();
            var activityType = currentValue?.GetType();
            if (activityType == null)
            {
                return null;
            }
            if (activityType == typeof(DsfDecision))
            {
                activityType = typeof(DsfFlowDecisionActivity);
            }

            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out var actual);
            if (actual != null)
            {
                ActivityDesignerViewModel instance;
                if (actual == typeof(SwitchDesignerViewModel))
                {
                    var dsfSwitch = currentValue as DsfSwitch;
                    instance = Activator.CreateInstance(actual, modelItem, dsfSwitch?.Switch ?? "") as ActivityDesignerViewModel;
                }
                else if (actual == typeof(ServiceDesignerViewModel))
                {
                    var resourceId = ModelItemUtils.TryGetResourceID(modelItem);
                    var childResourceModel = _resourceModel.Environment.ResourceRepository.LoadContextualResourceModel(resourceId);
                    instance = Activator.CreateInstance(actual, modelItem, childResourceModel) as ActivityDesignerViewModel;
                }
                else
                {
                    instance = Activator.CreateInstance(actual, modelItem) as ActivityDesignerViewModel;
                }

                var dsfActivity = activityType.GetProperty("DisplayName")?.GetValue(currentValue);
                if (currentValue is DsfDecision decision)
                {
                    dsfActivity = decision.Conditions.DisplayText;
                }
                if (currentValue is DsfSwitch switchActivity)
                {
                    dsfActivity = switchActivity.Switch;
                }
                var mergeToolModel = new MergeToolModel
                {
                    ActivityDesignerViewModel = instance,
                    MergeIcon = modelItem.GetImageSourceForTool(),
                    MergeDescription = dsfActivity?.ToString(),
                    UniqueId = currentValue.UniqueID.ToGuid(),
                    IsMergeVisible = true,
                    ActivityType = activity.GetFlowNode(),
                    Parent = parentItem,
                    HasParent = parentItem != null,
                    ParentDescription = parentLableDescription,
                    IsTrueArm = parentLableDescription?.ToLowerInvariant() == "true"                    
                };

                foreach (var act in activity.GetChildrenNodes())
                {
                    if (act.Value == null)
                    {
                        continue;
                    }
                    foreach (var innerAct in act.Value)
                    {
                        var item = GetModel(ModelItemUtils.CreateModelItem(innerAct), innerAct, mergeToolModel, act.Key);
                        mergeToolModel.Children.Add(item);
                    }
                }                
                return mergeToolModel;
            }
            return null;
        }

        public event ConflictModelChanged SomethingConflictModelChanged;
    }
}