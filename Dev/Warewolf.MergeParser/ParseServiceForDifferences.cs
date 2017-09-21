using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Activities.Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Presentation;
using Dev2;
using Dev2.Activities;
using Dev2.Studio.Interfaces;
using Dev2.Common;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.MergeParser
{
    public class ParseServiceForDifferences : IParseServiceForDifferences
    {
        public ParseServiceForDifferences()
        {

        }
        public (List<ModelItem> nodeList, Flowchart flowchartDiff) CurrentDifferences { get; private set; }
        public (List<ModelItem> nodeList, Flowchart flowchartDiff) Differences { get; private set; }

        private ModelItem GetCurrentModelItemUniqueId(IEnumerable<IDev2Activity> items, IDev2Activity activity)
        {
            if (activity == null) return default;
            foreach (var modelItem in items)
                if (modelItem.UniqueID.Equals(activity.UniqueID))
                    return ModelItemUtils.CreateModelItem(activity);
            return default;
        }

        private List<IDev2Activity> DiscoverActivities(List<ModelItem> modelItems)
        {
            var discoverActivities = new List<IDev2Activity>();
            foreach (var modelItem in modelItems)
            {
                if (modelItem.ItemType == typeof(FlowDecision))
                {
                    var dev2Activity = modelItem.GetProperty<IDev2Activity>("Condition");
                    discoverActivities.Add(dev2Activity);
                }
                else if (modelItem.ItemType == typeof(FlowSwitch<string>))
                {
                    var condition = modelItem.GetProperty("Expression");
                    var activity = (DsfFlowNodeActivity<string>)condition;
                    discoverActivities.Add(activity);
                }
                else
                {
                    var currentValue = modelItem.GetProperty<IDev2Activity>("Action");
                    discoverActivities.Add(currentValue);
                }

            }
            return discoverActivities;
        }

        public List<(Guid uniqueId, ModelItem current, ModelItem difference, bool conflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference)
        {
            var conflictList = new List<(Guid uniqueId, ModelItem current, ModelItem difference, bool conflict)>();
            CurrentDifferences = GetNodes(current);
            Differences = GetNodes(difference);
            var parsedCurrent = GetActivity(CurrentDifferences.flowchartDiff).ToList();
            var parsedDifference = GetActivity(Differences.flowchartDiff).ToList();
            List<IDev2Activity> equalItems = new List<IDev2Activity>();
            foreach (var mergeHeadActivity in parsedCurrent)
            {
                var singleOrDefault = parsedDifference.SingleOrDefault(activity => activity.Equals(mergeHeadActivity));
                if (singleOrDefault != null)
                    equalItems.Add(singleOrDefault);
            }

            List<IDev2Activity> nodesDifferentInMergeHead = parsedCurrent.Except(parsedDifference, new Dev2ActivityComparer()).ToList();
            List<IDev2Activity> toRemove = new List<IDev2Activity>();
            foreach (var differentInMergeHead in nodesDifferentInMergeHead)
            {
                if (equalItems.Contains(differentInMergeHead, new Dev2UniqueActivityComparer()))
                {
                    toRemove.Add(differentInMergeHead);
                }
            }

            nodesDifferentInMergeHead.RemoveAll(activity => toRemove.Exists(dev2Activity => dev2Activity.Equals(activity)));

            var nodesDifferentInHead = parsedDifference.Except(parsedCurrent, new Dev2ActivityComparer()).ToList();
            List<IDev2Activity> toRemove1 = new List<IDev2Activity>();
            foreach (var differentInMergeHead in nodesDifferentInHead)
            {
                if (equalItems.Contains(differentInMergeHead))
                {
                    toRemove1.Add(differentInMergeHead);
                }
            }
            nodesDifferentInHead.RemoveAll(activity => toRemove1.Exists(dev2Activity => dev2Activity.Equals(activity)));

            var allDifferences = nodesDifferentInMergeHead.Union(nodesDifferentInHead, new Dev2ActivityComparer());

            foreach (var item in equalItems)
            {
                if (item is null)
                {
                    continue;
                }
                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(parsedCurrent, item);
                var equalItem = (Guid.Parse(item.UniqueID), currentModelItemUniqueId, currentModelItemUniqueId, false);
                conflictList.Add(equalItem);
            }

            var differenceGroups = allDifferences.GroupBy(activity => activity.UniqueID);
            foreach (var item in differenceGroups)
            {
                IDev2Activity currentActivity = null;
                using (var enumerator = item.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (item.Key.Equals(enumerator?.Current?.UniqueID))
                        {
                            currentActivity = enumerator.Current;
                        }
                    }
                }


                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(parsedCurrent, currentActivity);
                var differences = GetCurrentModelItemUniqueId(parsedDifference, currentActivity);
                var diffItem = (Guid.Parse(item.Key), currentModelItemUniqueId, differences, true);
                conflictList.Add(diffItem);
            }
            return conflictList;
        }

        private IEnumerable<IDev2Activity> GetActivity(Flowchart modelItem)
        {
            var activityParser = CustomContainer.Get<IActivityParser>() ?? new ActivityParser();
            var act = activityParser?.Parse(new List<IDev2Activity>(), modelItem);
            var dev2Activities = act.NextNodes.Flatten(activity =>
            {
                if (activity.NextNodes != null) return activity.NextNodes;
                IEnumerable<IDev2Activity> activities = new List<IDev2Activity>();
                if (activity is DsfDecision)
                {
                    activities = activity.NextNodes ?? ((DsfDecision)activity).FalseArm.Union(((DsfDecision)activity).TrueArm);
                }
                return activities;
            });
            return dev2Activities;
        }

        private (List<ModelItem> nodeList, Flowchart flowchartDiff) GetNodes(IContextualResourceModel resourceModel)
        {
            var wd = new WorkflowDesigner();
            var xaml = resourceModel.WorkflowXaml;

            var workspace = GlobalConstants.ServerWorkspaceID;
            var msg = resourceModel.Environment.ResourceRepository.FetchResourceDefinition(resourceModel.Environment, workspace, resourceModel.ID, false);
            if (msg != null)
            {
                xaml = msg.Message;
            }

            if (xaml == null || xaml.Length == 0)
            {
                throw new Exception($"Could not find resource definition for {resourceModel.ResourceName}");
            }
            wd.Text = xaml.ToString();
            wd.Load();

            var modelService = wd.Context.Services.GetService<ModelService>();
            var nodeList = modelService.Find(modelService.Root, typeof(FlowNode)).ToList();
            var workflowHelper = new WorkflowHelper();
            var flowchartDiff = workflowHelper.EnsureImplementation(modelService).Implementation as Flowchart;
            // ReSharper disable once RedundantAssignment assuming this is for disposing
            wd = null;
            return (nodeList, flowchartDiff);
        }

    }
}
