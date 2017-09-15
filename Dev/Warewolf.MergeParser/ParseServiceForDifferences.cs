using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Activities.Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Presentation;
using System.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2;

namespace Warewolf.MergeParser
{
    public class ParseServiceForDifferences
    {
        readonly ModelItem _difference;
        readonly ModelItem _currentDifference;

        public ParseServiceForDifferences(ModelItem currentDif, ModelItem differenceHead)
        {
            _currentDifference = currentDif ?? throw new ArgumentNullException(nameof(currentDif));
            _difference = differenceHead ?? throw new ArgumentNullException(nameof(differenceHead));

        }

        public List<ModelItem> CurrentDifferences { get; private set; }
        public List<ModelItem> Differences { get; private set; }

        private ModelItem GetCurrentModelItemUniqueId(IEnumerable<ModelItem> items, string uniqueId)
        {
            foreach (var modelItem in items)
            {
                if (modelItem.GetCurrentValue<FlowStep>().Action is IDev2Activity currentValue &&
                    currentValue.UniqueID.Equals(uniqueId, StringComparison.InvariantCultureIgnoreCase))
                {
                    return ModelItemUtils.CreateModelItem(currentValue);
                }
            }

            return default;
        }



        public List<(Guid uniqueId, ModelItem current, ModelItem difference, bool conflict)> GetDifferences()
        {
            var conflictList = new List<(Guid uniqueId, ModelItem current, ModelItem difference, bool conflict)>();
            CurrentDifferences = GetNodes(_difference);
            Differences = GetNodes(_currentDifference);

            var mergeHeadActivities = CurrentDifferences.Select(i => i.GetProperty<IDev2Activity>("Action")).ToList();
            var headActivities = Differences.Select(i => i.GetProperty<IDev2Activity>("Action")).ToList();
            var equalItems = mergeHeadActivities.Intersect(headActivities);
            var nodesDifferentInMergeHead = mergeHeadActivities.Except(headActivities);
            var nodesDifferentInHead = headActivities.Except(mergeHeadActivities);

            var allDifferences = nodesDifferentInMergeHead.Union(nodesDifferentInHead);

            foreach (var item in equalItems)
            {
                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(CurrentDifferences, item.UniqueID);
                var equalItem = (Guid.Parse(item.UniqueID), currentModelItemUniqueId, currentModelItemUniqueId, false);
                conflictList.Add(equalItem);
            }

            var differenceGroups = allDifferences.GroupBy(activity => activity.UniqueID);
            foreach (var item in differenceGroups)
            {
                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(CurrentDifferences, item.Key);
                var differences = GetCurrentModelItemUniqueId(Differences, item.Key);
                var diffItem = (Guid.Parse(item.Key), currentModelItemUniqueId, differences, true);
                conflictList.Add(diffItem);
            }
            return conflictList;

        }

        private List<ModelItem> GetNodes(ModelItem modelItem)
        {
            var wd = new WorkflowDesigner();
            wd.Load(modelItem);
            var modelService = wd.Context.Services.GetService<ModelService>();
            var nodeList = modelService.Find(modelItem, typeof(FlowNode)).ToList();
            wd = null;
            return nodeList;
        }


    }
}
