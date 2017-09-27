using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Common;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.Switch;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.ExtMethods;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Activities.Statements;

namespace Dev2.ViewModels.Merge
{
    public class ConflictModelFactory : BindableBase, IConflictModelFactory
    {
        private ModelItem _modelItem;
        private readonly IContextualResourceModel _resourceModel;
        private bool _isWorkflowNameChecked;
        private bool _isVariablesChecked;

        public IMergeToolModel Model { get; set; }
        public ConflictModelFactory(ModelItem modelItem, IContextualResourceModel resourceModel)
        {
            Children = new ObservableCollection<IMergeToolModel>();
            _modelItem = modelItem;
            _resourceModel = resourceModel;
        }

        public ConflictModelFactory()
        {
        }

        public void GetDataList()
        {
            DataListViewModel = DataListViewModelFactory.CreateDataListViewModel(_resourceModel) as DataListViewModel;
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
        public bool IsVariablesChecked
        {
            get
            {
                return _isVariablesChecked;
            }
            set
            {
                _isVariablesChecked = value;
                OnPropertyChanged(() => IsVariablesChecked);
                SomethingConflictModelChanged?.Invoke(this, this);
            }
        }
        public bool IsWorkflowNameChecked
        {
            get
            {
                return _isWorkflowNameChecked;
            }
            set
            {
                _isWorkflowNameChecked = value;
                OnPropertyChanged(() => IsWorkflowNameChecked);
                SomethingConflictModelChanged?.Invoke(this, this);
            }
        }
        public DataListViewModel DataListViewModel { get; set; }
        public ObservableCollection<IMergeToolModel> Children { get; set; }

        public IMergeToolModel GetModel(string item = "")
        {
            if (_modelItem == default(ModelItem)) return null;
            var currentValue = _modelItem.GetCurrentValue<IDev2Activity>();
            var activityType = currentValue.GetType();
            if (activityType == typeof(DsfDecision)) activityType = typeof(DsfFlowDecisionActivity);
            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out Type actual);
            if (actual != null)
            {
                ActivityDesignerViewModel instance;
                if (actual == typeof(SwitchDesignerViewModel))
                {
                    instance = Activator.CreateInstance(actual, _modelItem, item) as ActivityDesignerViewModel;
                }
                else if (actual == typeof(ServiceDesignerViewModel))
                {
                    instance = Activator.CreateInstance(actual, _modelItem, _resourceModel) as ActivityDesignerViewModel;
                }
                else
                {
                    instance = Activator.CreateInstance(actual, _modelItem) as ActivityDesignerViewModel;
                }

                var dsfActivity = activityType.GetProperty("DisplayName")?.GetValue(currentValue);
                var mergeToolModel = new MergeToolModel
                {
                    ActivityDesignerViewModel = instance,
                    MergeIcon = _modelItem.GetImageSourceForTool(),
                    MergeDescription = dsfActivity?.ToString(),
                    UniqueId = currentValue.UniqueID.ToGuid()
                };

                if (currentValue is DsfDecision de)
                {
                    var decisionNode = new FlowDecision(de.GetFlowNode());
                    
                    if (de.TrueArm != null)
                    {
                        decisionNode.True = new FlowStep { Action = de.TrueArm.FirstOrDefault() as System.Activities.Activity };

                        var deTrueArm = de.TrueArm.Flatten(p => p.NextNodes ?? new List<IDev2Activity>());
                        foreach (var dev2Activity in deTrueArm)
                        {
                            _modelItem = ModelItemUtils.CreateModelItem(dev2Activity);
                            var addModelItem = GetModel();
                            addModelItem.HasParent = true;
                            addModelItem.ParentDescription = de.Conditions.TrueArmText;
                            mergeToolModel.Children.Add(addModelItem);
                        }
                    }

                    if (de.FalseArm != null)
                    {
                        decisionNode.False = new FlowStep { Action = de.FalseArm.FirstOrDefault() as System.Activities.Activity };

                        var deTrueArm = de.FalseArm.Flatten(p => p.NextNodes ?? new List<IDev2Activity>());
                        foreach (var dev2Activity in deTrueArm)
                        {
                            _modelItem = ModelItemUtils.CreateModelItem(dev2Activity);
                            var addModelItem = GetModel();
                            addModelItem.HasParent = true;
                            addModelItem.ParentDescription = de.Conditions.FalseArmText;
                            mergeToolModel.Children.Add(addModelItem);
                        }
                    }
                    mergeToolModel.ActivityType = decisionNode;

                    //Todo add 'and' and the default arm
                }
                else if (currentValue is DsfSwitch switchTool)
                {
                    var flowSwitch = new FlowStep { Action = currentValue as DsfSwitch };
                    mergeToolModel.ActivityType = flowSwitch;
                    if (switchTool.Switches != null)
                    {
                        var vv = switchTool.Switches.ToDictionary(k => k.Key);

                        foreach (var group in vv)
                        {
                            IEnumerable<IDev2Activity> activities =
                                vv.Values.Where(pair => pair.Key == group.Key).Select(k => k.Value);
                            foreach (var dev2Activity in activities)
                            {
                                _modelItem = ModelItemUtils.CreateModelItem(dev2Activity);
                                var addModelItem = GetModel();
                                addModelItem.HasParent = true;
                                addModelItem.ParentDescription = group.Key;
                                mergeToolModel.Children.Add(addModelItem);
                            }
                        }
                    }
                    if (switchTool.Default != null)
                    {
                        var deTrueArm = switchTool.Default.Flatten(p => p.NextNodes ?? new List<IDev2Activity>());
                        foreach (var dev2Activity in deTrueArm)
                        {
                            _modelItem = ModelItemUtils.CreateModelItem(dev2Activity);
                            var addModelItem = GetModel();
                            addModelItem.HasParent = true;
                            addModelItem.ParentDescription = "Default";
                            mergeToolModel.Children.Add(addModelItem);
                        }
                    }
                }
                else if (currentValue is DsfSequenceActivity sequence)
                {
                    var flowSequence = new FlowStep { Action = currentValue as DsfSequenceActivity };
                    mergeToolModel.ActivityType = flowSequence;
                    if (sequence.Activities != null)
                        foreach (var dev2Activity in sequence.Activities)
                        {
                            _modelItem = ModelItemUtils.CreateModelItem(dev2Activity);
                            var addModelItem = GetModel();
                            addModelItem.HasParent = true;
                            addModelItem.ParentDescription = sequence.DisplayName;
                            mergeToolModel.Children.Add(addModelItem);
                        }
                    var nextNode = sequence.NextNodes?.SingleOrDefault();
                    if (nextNode != null)
                    {
                        var nextModelItem = ModelItemUtils.CreateModelItem(nextNode);
                        if (nextNode is DsfSwitch a)
                        {
                            _modelItem = nextModelItem;
                            var addModelItem = GetModel(a.Switch);
                            Children.Add(addModelItem);
                        }

                        else
                        {
                            _modelItem = nextModelItem;
                            var addModelItem = GetModel();
                            Children.Add(addModelItem);
                        }
                    }
                }
                else if (currentValue is DsfForEachActivity b)
                {
                    var flowForEach = new FlowStep { Action = currentValue as DsfForEachActivity };
                    mergeToolModel.ActivityType = flowForEach;

                    var dev2Activity = b.DataFunc.Handler as IDev2Activity;
                    var singleOrDefault = dev2Activity;
                    if (singleOrDefault != null)
                    {
                        var forEachModel = ModelItemUtils.CreateModelItem(singleOrDefault);
                        _modelItem = forEachModel;
                        var addModelItem = GetModel();
                        addModelItem.HasParent = true;
                        addModelItem.ParentDescription = b.DisplayName;
                        mergeToolModel.Children.Add(addModelItem);
                    }
                    var nextNode = b.NextNodes?.SingleOrDefault();
                    if (nextNode != null)
                    {
                        var nextModelItem = ModelItemUtils.CreateModelItem(nextNode);
                        if (nextNode is DsfSwitch a)
                        {
                            _modelItem = nextModelItem;
                            var addModelItem = GetModel(a.Switch);
                            Children.Add(addModelItem);
                        }

                        else
                        {
                            _modelItem = nextModelItem;
                            var addModelItem = GetModel();
                            Children.Add(addModelItem);
                        }
                    }
                }
                else if (currentValue is DsfSelectAndApplyActivity c)
                {
                    var flowSelectAndApply = new FlowStep { Action = currentValue as DsfSelectAndApplyActivity };
                    mergeToolModel.ActivityType = flowSelectAndApply;

                    var dev2Activity = c.ApplyActivityFunc.Handler as IDev2Activity;
                    var singleOrDefault = dev2Activity;
                    if (singleOrDefault != null)
                    {
                        var forEachModel = ModelItemUtils.CreateModelItem(singleOrDefault);
                        _modelItem = forEachModel;
                        var addModelItem = GetModel();
                        addModelItem.HasParent = true;
                        addModelItem.ParentDescription = c.DisplayName;
                        mergeToolModel.Children.Add(addModelItem);
                    }
                    var nextNode = c.NextNodes?.SingleOrDefault();
                    if (nextNode != null)
                    {
                        var nextModelItem = ModelItemUtils.CreateModelItem(nextNode);
                        if (nextNode is DsfSwitch a)
                        {
                            _modelItem = nextModelItem;
                            var addModelItem = GetModel(a.Switch);
                            Children.Add(addModelItem);
                        }

                        else
                        {
                            _modelItem = nextModelItem;
                            var addModelItem = GetModel();
                            Children.Add(addModelItem);
                        }
                    }
                }

                var flowStep = new FlowStep { Action = currentValue as DsfActivity };
                mergeToolModel.ActivityType = flowStep;

                return mergeToolModel;
            }
            return null;
        }

        public event ConflictModelChanged SomethingConflictModelChanged;
    }
}