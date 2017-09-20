using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Common;
using Caliburn.Micro;

namespace Dev2.ViewModels.Merge
{
    public class ConflictViewModel : BindableBase, IConflictViewModel
    {
        private IContextualResourceModel _resourceModel;

        public ConflictViewModel(ModelItem modelItem, IContextualResourceModel resourceModel)
        {
            Children = new ObservableCollection<IMergeToolModel>();
            MergeToolModel = AddModelItem(modelItem);
            _resourceModel = resourceModel;
        }

        public IMergeToolModel AddModelItem(ModelItem modelItem)
        {
            var currentValue = modelItem.Properties["Action"]?.ComputedValue ?? modelItem.Properties["Condition"].ComputedValue;
            var activityType = currentValue.GetType();
            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out Type actual);
            if (actual != null)
            {
                var instance = Activator.CreateInstance(actual, modelItem.Properties["Action"]?.Value ?? modelItem.Properties["Condition"].Value) as ActivityDesignerViewModel;
                var dsfActivity = activityType.GetProperty("DisplayName")?.GetValue(currentValue);
                var mergeToolModel = new MergeToolModel
                {
                    ActivityDesignerViewModel = instance,
                    MergeIcon = modelItem.GetImageSourceForTool(),
                    MergeDescription = dsfActivity?.ToString()
                };

                if (modelItem.ItemType == typeof(FlowDecision))
                {
                    var act = modelItem.GetCurrentValue<FlowDecision>();
                    if (act.True != null)
                    {
                        mergeToolModel.Children.Add(AddModelItem(ModelItemUtils.CreateModelItem(act.True)));
                    }
                    if (act.False != null)
                    {
                        mergeToolModel.Children.Add(AddModelItem(ModelItemUtils.CreateModelItem(act.False)));
                    }
                }
                if (modelItem.ItemType == typeof(FlowSwitch<string>))
                {
                    var act = modelItem.GetCurrentValue<FlowDecision>();
                    if (act.True != null)
                    {
                        mergeToolModel.Children.Add(AddModelItem(ModelItemUtils.CreateModelItem(act.True)));
                    }
                    if (act.False != null)
                    {
                        mergeToolModel.Children.Add(AddModelItem(ModelItemUtils.CreateModelItem(act.False)));
                    }
                }
                //var mergeToolModel = new MergeToolModel();
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

        public string WorkflowName { get; set; }
        public IMergeToolModel MergeToolModel { get; set; }
        public DataListViewModel DataListViewModel { get; set; }
        public ObservableCollection<IMergeToolModel> Children { get; set; }
    }
}