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
using System.Activities.Presentation.View;
using System.Windows;

namespace Warewolf.MergeParser
{
    public class ServiceDifferenceParser : IServiceDifferenceParser
    {
        private readonly IActivityParser _activityParser;
        private (List<(ModelItem modelItem, Point point)> nodeList, Flowchart flowchartDiff) _currentDifferences;
        private (List<(ModelItem modelItem, Point point)> nodeList, Flowchart flowchartDiff) _differences;

        private (ModelItem activity, Point point) GetCurrentModelItemUniqueId(IEnumerable<(Point point, IDev2Activity activity)> items, IDev2Activity activity)
        {
            if (activity == null) return default;
            foreach ((Point point, IDev2Activity activity) item in items)
                if (item.activity.UniqueID.Equals(activity.UniqueID))
                    return ( ModelItemUtils.CreateModelItem(item.activity), item.point);
            return default;
        }

        public ServiceDifferenceParser()
            : this(CustomContainer.Get<IActivityParser>())
        {

        }
        public ServiceDifferenceParser(IActivityParser activityParser)
        {
            VerifyArgument.IsNotNull(nameof(activityParser), activityParser);
            _activityParser = activityParser;
        }

        void CleanUpForDecisionAdSwitch(List<IDev2Activity> dev2Activities)
        {
            List<string> children = new List<string>();
            var decisions = dev2Activities.Where(tuple => tuple is DsfDecision).Cast<DsfDecision>();
            foreach (var valueTuple in decisions)
            {
                var trues = valueTuple.TrueArm?.Flatten(activity => activity.NextNodes ?? new List<IDev2Activity>()) ?? new List<IDev2Activity>();
                var falses = valueTuple.FalseArm?.Flatten(activity => activity.NextNodes ?? new List<IDev2Activity>()) ?? new List<IDev2Activity>();
                var list = trues.Select(activity => activity.UniqueID).Union(falses.Select(activity => activity.UniqueID))
                    .ToList();
                children.AddRange(list);
            }
            var switches = dev2Activities.Where(tuple => tuple is DsfSwitch).Cast<DsfSwitch>();

            foreach (var group in switches)
            {
                foreach (var tool in group.Switches)
                {
                    var currentArmTree = _activityParser.FlattenNextNodesInclusive(tool.Value);
                    var enumerable = currentArmTree.Select(activity => activity.UniqueID);
                    children.AddRange(enumerable);
                }
                foreach (var tool in group.Default)
                {
                    var currentArmTree = _activityParser.FlattenNextNodesInclusive(tool);
                    var enumerable = currentArmTree.Select(activity => activity.UniqueID);
                    children.AddRange(enumerable);
                }
            }
            dev2Activities.RemoveAll(activity => children.Any(s => s.Equals(activity.UniqueID, StringComparison.InvariantCultureIgnoreCase)));
        }

        public List<(Guid uniqueId, (ModelItem modelItem, Point point), (ModelItem modelItem, Point point), bool hasConflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference)
        {
            var conflictList = new List<(Guid uniqueId, (ModelItem modelItem, Point point), (ModelItem modelItem, Point point), bool hasConflict)>();
            _currentDifferences = GetNodes(current);
            _differences = GetNodes(difference);
            var allCurentItems = new List<(Point point, IDev2Activity activity)>();
            var allRemoteItems = new List<(Point point, IDev2Activity activity)>();
            foreach (var node in _currentDifferences.nodeList)
            {
                var dev2Activity1 = _activityParser.Parse(new List<IDev2Activity>(), node.modelItem);
                allCurentItems.Add((node.point, dev2Activity1));
            }
            var currentList=allCurentItems.Select(p => p.activity).ToList();
            CleanUpForDecisionAdSwitch(currentList);
            foreach (var node in _differences.nodeList)
            {
                var dev2Activity1 = _activityParser.Parse(new List<IDev2Activity>(), node.modelItem);
                allRemoteItems.Add((node.point, dev2Activity1));
            }
            var remoteList = allRemoteItems.Select(p => p.activity).ToList();
            CleanUpForDecisionAdSwitch(remoteList);
           
            var equalItems = currentList.Intersect(remoteList, new Dev2ActivityComparer()).ToList();
            var nodesDifferentInMergeHead = currentList.Except(remoteList, new Dev2ActivityComparer()).ToList();
            var nodesDifferentInHead = remoteList.Except(currentList, new Dev2ActivityComparer()).ToList();
            var allDifferences = nodesDifferentInMergeHead.Union(nodesDifferentInHead, new Dev2ActivityComparer());

            foreach (var item in equalItems)
            {
                if (item is null)
                {
                    continue;
                }
                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(allCurentItems, item);

                var equalItem = (Guid.Parse(item.UniqueID), currentModelItemUniqueId, currentModelItemUniqueId, false);
                conflictList.Add(equalItem);
            }
            var dev2Activities = allDifferences.DistinctBy(activity => activity.UniqueID).ToList();
            foreach (var item in dev2Activities)
            {
                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(allRemoteItems, item);
                var differences = GetCurrentModelItemUniqueId(allCurentItems, item);
                var diffItem = (Guid.Parse(item.UniqueID), currentModelItemUniqueId, differences, true);
                conflictList.Add(diffItem);
            }
            return conflictList;
        }

        private (List<(ModelItem modelItem, Point point)> nodeList, Flowchart flowchartDiff) GetNodes(IContextualResourceModel resourceModel)
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
            var locationList = nodeList.Select(node => (node, GetShapeLocation(wd, node))).ToList();

            var workflowHelper = new WorkflowHelper();
            var flowchartDiff = workflowHelper.EnsureImplementation(modelService).Implementation as Flowchart;
            // ReSharper disable once RedundantAssignment assuming this is for disposing
            wd = null;
            return (locationList, flowchartDiff);
        }

        private static Point GetShapeLocation(WorkflowDesigner wd, ModelItem modelItem)
        {
            var shapeLocation = new Point();
            var viewStateService = wd.Context.Services.GetService<ViewStateService>();
            var viewState = viewStateService?.RetrieveAllViewState(modelItem);
            if (viewState != null)
            {
                shapeLocation = (Point)viewState["ShapeLocation"];
            }

            return shapeLocation;
        }
    }
}
