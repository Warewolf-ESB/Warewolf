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
using Dev2.Activities.Designers2.Sequence;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.Switch;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.ExtMethods;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.ViewModels.Merge
{
    public class ConflictViewModel : BindableBase, IConflictViewModel
    {
        private readonly IContextualResourceModel _resourceModel;

        public ConflictViewModel(ModelItem modelItem, IContextualResourceModel resourceModel)
        {
            Children = new ObservableCollection<IMergeToolModel>();
            MergeToolModel = AddModelItem(modelItem);
            _resourceModel = resourceModel;
        }

        public IMergeToolModel AddModelItem(ModelItem modelItem, string item = "")
        {

            var currentValue = modelItem.GetCurrentValue<IDev2Activity>();
            var activityType = currentValue.GetType();
            if (activityType == typeof(DsfDecision)) activityType = typeof(DsfFlowDecisionActivity);
            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out Type actual);
            if (actual != null)
            {
                ActivityDesignerViewModel instance;
                if (actual == typeof(SwitchDesignerViewModel))
                {
                    instance = Activator.CreateInstance(actual, modelItem, item) as ActivityDesignerViewModel;
                }
                else if(actual == typeof(ServiceDesignerViewModel))
                {
                    instance = Activator.CreateInstance(actual, modelItem, _resourceModel) as ActivityDesignerViewModel;
                }
                else
                {
                    instance = Activator.CreateInstance(actual, modelItem) as ActivityDesignerViewModel;
                }

                var dsfActivity = activityType.GetProperty("DisplayName")?.GetValue(currentValue);
                var mergeToolModel = new MergeToolModel
                {
                    ActivityDesignerViewModel = instance,
                    MergeIcon = modelItem.GetImageSourceForTool(),
                    MergeDescription = dsfActivity?.ToString(),
                    UniqueId = currentValue.UniqueID.ToGuid()
                };

                if (currentValue is DsfDecision de)
                {
                    if (de.TrueArm != null)
                    {
                        var deTrueArm = de.TrueArm.Flatten(p => p.NextNodes ?? new List<IDev2Activity>());
                        foreach (var dev2Activity in deTrueArm)
                        {
                            var addModelItem = AddModelItem(ModelItemUtils.CreateModelItem(dev2Activity));
                            addModelItem.HasParent = true;
                            addModelItem.ParentDescription = de.Conditions.TrueArmText;
                            mergeToolModel.Children.Add(addModelItem);
                        }
                    }

                    if (de.FalseArm != null)
                    {
                        var deTrueArm = de.FalseArm.Flatten(p => p.NextNodes ?? new List<IDev2Activity>());
                        foreach (var dev2Activity in deTrueArm)
                        {
                            var addModelItem = AddModelItem(ModelItemUtils.CreateModelItem(dev2Activity));
                            addModelItem.HasParent = true;
                            addModelItem.ParentDescription = de.Conditions.FalseArmText;
                            mergeToolModel.Children.Add(addModelItem);
                        }
                    }
                    //Todo add 'and' and the default arm
                }
                else if (currentValue is DsfSwitch switchTool)
                {
                    if (switchTool.Switches != null)
                    {
                        var vv = switchTool.Switches.ToDictionary(k => k.Key);

                        foreach (var group in vv)
                        {
                            IEnumerable<IDev2Activity> activities = vv.Values.Where(pair => pair.Key == group.Key).Select(k => k.Value);
                            foreach (var dev2Activity in activities)
                            {
                                var addModelItem = AddModelItem(ModelItemUtils.CreateModelItem(dev2Activity));
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
                            var addModelItem = AddModelItem(ModelItemUtils.CreateModelItem(dev2Activity));
                            addModelItem.HasParent = true;
                            addModelItem.ParentDescription = "Default";
                            mergeToolModel.Children.Add(addModelItem);
                        }
                    }
                }
                else if (currentValue is DsfSequenceActivity sequence)
                {
                    if (sequence.Activities != null)
                        foreach (var dev2Activity in sequence.Activities)
                        {
                            var addModelItem = AddModelItem(ModelItemUtils.CreateModelItem(dev2Activity));
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
                            var addModelItem = AddModelItem(nextModelItem, a.Switch);
                            Children.Add(addModelItem);
                        }

                        else
                        {
                            var addModelItem = AddModelItem(nextModelItem);
                            Children.Add(addModelItem);
                        }
                    }
                }
                else if (currentValue is DsfForEachActivity b)
                {
                    var dev2Activity = b.DataFunc.Handler as IDev2Activity;
                    var singleOrDefault = dev2Activity;
                    if (singleOrDefault != null)
                    {
                        var forEachModel = ModelItemUtils.CreateModelItem(singleOrDefault);
                        var addModelItem = AddModelItem(forEachModel);
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
                            var addModelItem = AddModelItem(nextModelItem, a.Switch);
                            Children.Add(addModelItem);
                        }

                        else
                        {
                            var addModelItem = AddModelItem(nextModelItem);
                            Children.Add(addModelItem);
                        }
                    }
                }
                else if (currentValue is DsfSelectAndApplyActivity c)
                {
                    var dev2Activity = c.ApplyActivityFunc.Handler as IDev2Activity;
                    var singleOrDefault = dev2Activity;
                    if (singleOrDefault != null)
                    {
                        var forEachModel = ModelItemUtils.CreateModelItem(singleOrDefault);
                        var addModelItem = AddModelItem(forEachModel);
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
                            var addModelItem = AddModelItem(nextModelItem, a.Switch);
                            Children.Add(addModelItem);
                        }

                        else
                        {
                            var addModelItem = AddModelItem(nextModelItem);
                            Children.Add(addModelItem);
                        }
                    }
                }
                else
                {
                    var nextNode = currentValue.NextNodes?.SingleOrDefault();
                    if (nextNode != null)
                    {
                        var nextModelItem = ModelItemUtils.CreateModelItem(nextNode);
                        if (nextNode is DsfSwitch a)
                        {
                            var addModelItem = AddModelItem(nextModelItem, a.Switch);
                            Children.Add(addModelItem);
                        }
                       
                        else
                        {
                            var addModelItem = AddModelItem(nextModelItem);
                            Children.Add(addModelItem);
                        }
                    }

                }
                mergeToolModel.ActivityDesignerViewModel = instance;
                mergeToolModel.MergeIcon = modelItem.GetImageSourceForTool();
                mergeToolModel.MergeDescription = dsfActivity?.ToString();
                return mergeToolModel;
            }
            return null;
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
        public bool IsVariablesChecked { get; set; }
        public bool IsWorkflowNameChecked { get; set; }
        public IMergeToolModel MergeToolModel { get; set; }
        public DataListViewModel DataListViewModel { get; set; }
        public ObservableCollection<IMergeToolModel> Children { get; set; }
    }
}