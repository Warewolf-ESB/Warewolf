using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.Practices.Prism.Mvvm;

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
            var mergeToolModel = new MergeToolModel();
            var currentValue = modelItem.GetCurrentValue();
            var activityType = currentValue.GetType();
            DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out Type actual);
            if (actual != null)
            {
                var instance = Activator.CreateInstance(actual, modelItem) as ActivityDesignerViewModel;
                var dsfActivity = activityType.GetProperty("DisplayName")?.GetValue(currentValue);
                mergeToolModel.ActivityDesignerViewModel = instance;
                mergeToolModel.MergeIcon = modelItem.GetImageSourceForTool();
                mergeToolModel.MergeDescription = dsfActivity?.ToString();
            }
            return mergeToolModel;
        }

        public string WorkflowName { get; set; }
        public IMergeToolModel MergeToolModel { get; set; }
        public DataListViewModel DataListViewModel { get; set; }
    }
}