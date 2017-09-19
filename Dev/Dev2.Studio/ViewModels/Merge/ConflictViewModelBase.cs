using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Communication;

namespace Dev2.ViewModels.Merge
{
    public abstract class ConflictViewModelBase : BindableBase, IConflictViewModel
    {
        protected ConflictViewModelBase(ModelItem modelItem)
        {
            MergeToolModel = AddModelItem(modelItem);
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
                var mergeToolModel = new MergeToolModel();
                mergeToolModel.ActivityDesignerViewModel = instance;
                mergeToolModel.MergeIcon = modelItem.GetImageSourceForTool();
                mergeToolModel.MergeDescription = dsfActivity?.ToString();

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
                return mergeToolModel;
            }
            return null;
        }

        public string WorkflowName { get; set; }
        public IMergeToolModel MergeToolModel { get; set; }
        public DataListViewModel DataListViewModel { get; set; }
    }
}