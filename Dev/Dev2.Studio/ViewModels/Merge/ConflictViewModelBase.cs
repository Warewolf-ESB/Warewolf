using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        protected ConflictViewModelBase(IApplicationAdaptor applicationAdaptor, IEnumerable<ModelItem> modelItems)
        {
            foreach (var modelItem in modelItems)
            {
                MergeToolModel = AddModelItem(modelItem, applicationAdaptor);
            }
            //var firstOrDefault = mergeToolModel.Children.FirstOrDefault();
            //if (firstOrDefault != null)
            //{
            //    firstOrDefault.IsMergeExpanded = true;
            //    firstOrDefault.IsMergeExpanderEnabled = true;
            //    MergeToolModel = firstOrDefault;
            //}
        }


        public IMergeToolModel AddModelItem(ModelItem modelItem, IApplicationAdaptor applicationAdaptor)
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
                mergeToolModel.MergeIcon = modelItem.GetImageSourceForTool(applicationAdaptor);
                mergeToolModel.MergeDescription = dsfActivity?.ToString();
            }
            return mergeToolModel;
        }

        public string WorkflowName { get; set; }
        public IMergeToolModel MergeToolModel { get; set; }
        public DataListViewModel DataListViewModel { get; set; }
    }
}