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
        private readonly IActivityParser _activityParser;
        private ModelItem _modelItem;
        private readonly IContextualResourceModel _resourceModel;
        private bool _isWorkflowNameChecked;
        private bool _isVariablesChecked;

        public IMergeToolModel Model { get; set; }
        public ConflictModelFactory(ModelItem modelItem, IContextualResourceModel resourceModel)
            : this(CustomContainer.Get<IActivityParser>())
        {
            Children = new ObservableCollection<IMergeToolModel>();
            _modelItem = modelItem;
            _resourceModel = resourceModel;
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
                    var dsfSwitch = currentValue as DsfSwitch;
                    instance = Activator.CreateInstance(actual, _modelItem, dsfSwitch?.Switch ?? "") as ActivityDesignerViewModel;
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
                if (currentValue is DsfDecision decision)
                {
                    dsfActivity = decision.Conditions.DisplayText;
                }
                var mergeToolModel = new MergeToolModel
                {
                    ActivityDesignerViewModel = instance,
                    MergeIcon = _modelItem.GetImageSourceForTool(),
                    MergeDescription = dsfActivity?.ToString(),
                    UniqueId = currentValue.UniqueID.ToGuid()
                };
                var flowStep = new FlowStep { Action = currentValue as DsfActivity };
                mergeToolModel.ActivityType = flowStep;
                //TODO implement builder pattern
                switch (currentValue)
                {
                    case DsfDecision decisionTool:
                        BuildDecision(decisionTool, mergeToolModel);
                        break;
                    case DsfSwitch switchTool:
                        BuildSwitch(switchTool, mergeToolModel);
                        break;
                    case DsfSequenceActivity sequence:
                        BuildSequence(sequence, mergeToolModel);
                        break;
                    case DsfForEachActivity forEach:
                        BuildForEach(forEach, mergeToolModel);
                        break;
                    case DsfSelectAndApplyActivity selectAndApply:
                        BuildSelectAndApply(selectAndApply, mergeToolModel);
                        break;
                }


                return mergeToolModel;
            }
            return null;
        }

        private void BuildSelectAndApply(DsfSelectAndApplyActivity c, MergeToolModel mergeToolModel)
        {
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

        private void BuildForEach(DsfForEachActivity b, MergeToolModel mergeToolModel)
        {
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

        private void BuildSequence(DsfSequenceActivity sequence, MergeToolModel mergeToolModel)
        {
            var flowSequence = new FlowStep { Action = sequence };
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

        private void BuildSwitch(DsfSwitch switchTool, MergeToolModel mergeToolModel)
        {
            var flowSwitch = new FlowStep { Action = switchTool };
            mergeToolModel.ActivityType = flowSwitch;
            if (switchTool.Switches != null)
            {
                foreach (var group in switchTool.Switches)
                {
                    _modelItem = ModelItemUtils.CreateModelItem(@group.Value);
                    var addModelItem = GetModel(@group.Key);
                    addModelItem.HasParent = true;
                    //addModelItem.ParentDescription = group.Key;
                    addModelItem.MergeDescription = @group.Key;
                    mergeToolModel.Children.Add(addModelItem);
                }
            }
            if (switchTool.Default != null)
            {
                foreach (var dev2Activity in switchTool.Default)
                {
                    _modelItem = ModelItemUtils.CreateModelItem(dev2Activity);
                    var addModelItem = GetModel();
                    addModelItem.HasParent = true;
                    addModelItem.ParentDescription = "Default";
                    addModelItem.MergeDescription = "Default";
                    mergeToolModel.Children.Add(addModelItem);
                }
            }
        }

        private void BuildDecision(DsfDecision de, MergeToolModel mergeToolModel)
        {
            var decisionNode = new FlowDecision(de.GetFlowNode());
            if (de.TrueArm != null)
            {

                var firstOrDefault = de.TrueArm?.FirstOrDefault();
                var activity = _activityParser.ParseToLinkedFlatList(firstOrDefault);
                decisionNode.True = new FlowStep { Action = firstOrDefault as System.Activities.Activity };
                foreach (var dev2Activity in activity)
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
                var firstOrDefault = de.FalseArm?.FirstOrDefault();
                decisionNode.False = new FlowStep { Action = firstOrDefault as System.Activities.Activity };
                var activity = _activityParser.ParseToLinkedFlatList(firstOrDefault);
                foreach (var dev2Activity in activity)
                {
                    _modelItem = ModelItemUtils.CreateModelItem(dev2Activity);
                    var addModelItem = GetModel();
                    addModelItem.HasParent = true;
                    addModelItem.ParentDescription = de.Conditions.FalseArmText;
                    mergeToolModel.Children.Add(addModelItem);
                }

            }
            mergeToolModel.ActivityType = decisionNode;
        }

        public event ConflictModelChanged SomethingConflictModelChanged;
    }
}