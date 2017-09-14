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
    public abstract class CurrentConflictViewModelBase : BindableBase, ICurrentConflictViewModel
    {
        protected CurrentConflictViewModelBase(IApplicationAdaptor applicationAdaptor, IEnumerable<ModelItem> modelItems)
        {
            MergeConflicts = new ObservableCollection<IMergeToolModel>();
            foreach (var modelItem in modelItems)
            {
                var currentValue = modelItem.GetCurrentValue();
                var activityType = currentValue.GetType();
                DesignerAttributeMap.DesignerAttributes.TryGetValue(activityType, out Type actual);
                if (actual != null)
                {
                    var instance = Activator.CreateInstance(actual, modelItem) as ActivityDesignerViewModel;
                    var dsfActivity = activityType.GetProperty("DisplayName")?.GetValue(currentValue);
                    var mergeToolModel = new MergeToolModel()
                    {
                        ActivityDesignerViewModel = instance,
                        MergeIcon = modelItem.GetImageSourceForTool(applicationAdaptor),
                        MergeDescription = dsfActivity?.ToString(),
                    };
                    MergeConflicts.Add(mergeToolModel);
                }
            }
            var firstOrDefault = MergeConflicts.FirstOrDefault();
            if (firstOrDefault != null)
            {
                firstOrDefault.IsMergeExpanded = true;
                firstOrDefault.IsMergeExpanderEnabled = true;
                MergeToolModel = firstOrDefault;
            }
        }

        public string WorkflowName { get; set; }
        public ObservableCollection<IMergeToolModel> MergeConflicts { get; set; }
        public IMergeToolModel MergeToolModel { get; set; }
        public DataListViewModel DataListViewModel { get; set; }
    }
}