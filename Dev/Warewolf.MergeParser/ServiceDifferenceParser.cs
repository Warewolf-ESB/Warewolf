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
using Dev2.Common.Interfaces;
using Dev2.Communication;
using System.Collections.Concurrent;

namespace Warewolf.MergeParser
{
    public class ServiceDifferenceParser : IServiceDifferenceParser
    {
        private readonly IActivityParser _activityParser;
        private readonly IResourceDefinationCleaner _definationCleaner;
        ConcurrentDictionary<string, (ModelItem leftItem, ModelItem rightItem)> _flowNodes = new ConcurrentDictionary<string, (ModelItem leftItem, ModelItem rightItem)>(StringComparer.OrdinalIgnoreCase);

        private IConflictNode GetCurrentModelItemUniqueId(List<(IDev2Activity, IConflictNode)> items, IDev2Activity activity)
        {
            if (activity == null)
            {
                return default;
            }

            foreach (var modelItem in items)
            {
                if (modelItem.Item1.UniqueID.Equals(activity.UniqueID))
                {
                    return modelItem.Item2;
                }
            }

            return default;
        }

        public ServiceDifferenceParser()
            : this(CustomContainer.Get<IActivityParser>(), new ResourceDefinationCleaner())
        {

        }
        public ServiceDifferenceParser(IActivityParser activityParser, IResourceDefinationCleaner definationCleaner)
        {
            VerifyArgument.IsNotNull(nameof(activityParser), activityParser);
            VerifyArgument.IsNotNull(nameof(definationCleaner), definationCleaner);
            _activityParser = activityParser;
            _definationCleaner = definationCleaner;

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

        public ConcurrentDictionary<string, (ModelItem leftItem, ModelItem rightItem)> GetAllNodes()
        {
            return _flowNodes;
        }


        (IDev2Activity, IConflictNode) BuildNode(ModelItem nodeModelItem, WorkflowDesigner workflowDesigner)
        {
            var dev2Activity1 = _activityParser.Parse(new List<IDev2Activity>(), nodeModelItem);
            var shapeLocation = GetShapeLocation(workflowDesigner, nodeModelItem);
            ConflictNode conflictNode = new ConflictNode()
            {
                CurrentActivity = ModelItemUtils.CreateModelItem(dev2Activity1),
                CurrentFlowStep = nodeModelItem,
                NodeLocation = shapeLocation,
            };

            return (dev2Activity1, conflictNode);
        }

        private void BuildDifferenceStore()
        {
            var equalItems = _flatCurrentActivities.Intersect(_flatDifferentActivities, new Dev2ActivityComparer()).ToList();
            var nodesDifferentInMergeHead = _flatCurrentActivities.Except(_flatDifferentActivities, new Dev2ActivityComparer()).ToList();
            var nodesDifferentInHead = _flatDifferentActivities.Except(_flatCurrentActivities, new Dev2ActivityComparer()).ToList();
            var allDifferences = nodesDifferentInMergeHead.Union(nodesDifferentInHead, new Dev2ActivityComparer());
            var dev2Activities = allDifferences.DistinctBy(activity => activity.UniqueID).ToList();
            var sameConflict = equalItems.Select(p => new KeyValuePair<string, bool>(p.UniqueID, false));
            var diffConflict = dev2Activities.Select(p => new KeyValuePair<string, bool>(p.UniqueID, true));
            conflicts.AddRange(sameConflict);
            conflicts.AddRange(diffConflict);

        }

        public bool NodeHasConflict(string uniqueId)
        {
            var hasConflict = conflicts.SingleOrDefault(p => p.Key.Equals(uniqueId));
            return hasConflict.Value;
        }
        private List<KeyValuePair<string, bool>> conflicts;
        private List<IDev2Activity> _flatCurrentActivities;
        private List<IDev2Activity> _flatDifferentActivities;

        public List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference, bool loadworkflowFromServer = true)
        {
            _flowNodes = new ConcurrentDictionary<string, (ModelItem leftItem, ModelItem rightItem)>();
            _flatCurrentActivities = new List<IDev2Activity>();
            _flatDifferentActivities = new List<IDev2Activity>();
            conflicts = new List<KeyValuePair<string, bool>>();

            var currentDifferences = GetNodes(current, true);
            var remotedifferences = GetNodes(difference, loadworkflowFromServer);

            var allCurentItems = new List<(IDev2Activity, IConflictNode)>();
            var allRemoteItems = new List<(IDev2Activity, IConflictNode)>();

            int treeIndex = 1;
            foreach (var node in currentDifferences.orderedNodeList)
            {
                var nodeConflictPair = BuildNode(node, currentDifferences.wd);
                treeIndex++;
                nodeConflictPair.Item2.TreeIndex = treeIndex;
                allCurentItems.Add(nodeConflictPair);
            }

            foreach (var node in currentDifferences.allNodes)
            {
                var dev2Activity1 = _activityParser.Parse(new List<IDev2Activity>(), node);
                _flatCurrentActivities.Add(dev2Activity1);
                _flowNodes.TryAdd(dev2Activity1.UniqueID, (node, default(ModelItem)));
            }

            foreach (var node in remotedifferences.allNodes)
            {
                var dev2Activity1 = _activityParser.Parse(new List<IDev2Activity>(), node);
                _flatDifferentActivities.Add(dev2Activity1);
                if (_flowNodes.ContainsKey(dev2Activity1.UniqueID))
                {
                    var rightItem = _flowNodes[dev2Activity1.UniqueID];
                    rightItem.rightItem = node;
                    _flowNodes[dev2Activity1.UniqueID] = rightItem;
                }
                else
                {
                    _flowNodes.TryAdd(dev2Activity1.UniqueID, (default(ModelItem), node));
                }
            }

            List<IDev2Activity> currentActivities = allCurentItems.Select(p => p.Item1).ToList();
            CleanUpForDecisionAdSwitch(currentActivities);
            foreach (var node in remotedifferences.orderedNodeList)
            {
                var nodeConflictPair = BuildNode(node, remotedifferences.wd);
                allRemoteItems.Add(nodeConflictPair);
            }
            List<IDev2Activity> differenceActivities = allRemoteItems.Select(p => p.Item1).ToList();
            BuildDifferenceStore();
            CleanUpForDecisionAdSwitch(differenceActivities);
            var equalItems = currentActivities.Intersect(differenceActivities, new Dev2ActivityComparer()).ToList();
            var nodesDifferentInMergeHead = currentActivities.Except(differenceActivities, new Dev2ActivityComparer()).ToList();
            var nodesDifferentInHead = differenceActivities.Except(currentActivities, new Dev2ActivityComparer()).ToList();
            var allDifferences = nodesDifferentInMergeHead.Union(nodesDifferentInHead, new Dev2ActivityComparer());
            IOrderedEnumerable<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> orderedNodes
                = BuildOrderedDifferences(allCurentItems, allRemoteItems, equalItems, allDifferences);
            return orderedNodes.ToList();
        }

        private IOrderedEnumerable<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> BuildOrderedDifferences(List<(IDev2Activity, IConflictNode)> allCurentItems,
            List<(IDev2Activity, IConflictNode)> allRemoteItems, List<IDev2Activity> equalItems, IEnumerable<IDev2Activity> allDifferences)
        {
            var conflictList = new List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)>();
            foreach (var item in equalItems)
            {
                if (item is null)
                {
                    continue;
                }
                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(allCurentItems, item);
                var differences = GetCurrentModelItemUniqueId(allRemoteItems, item);
                var equalItem = (Guid.Parse(item.UniqueID), currentModelItemUniqueId, differences, false);
                conflictList.Add(equalItem);

            }
            var dev2Activities = allDifferences.DistinctBy(activity => activity.UniqueID).ToList();
            foreach (var item in dev2Activities)
            {
                var currentModelItemUniqueId = GetCurrentModelItemUniqueId(allCurentItems, item);
                var differences = GetCurrentModelItemUniqueId(allRemoteItems, item);
                var diffItem = (Guid.Parse(item.UniqueID), currentModelItemUniqueId, differences, true);
                conflictList.Add(diffItem);
            }

            var orderedNodes = conflictList.OrderBy(t => t.currentNode?.TreeIndex ?? t.differenceNode?.TreeIndex);
            return orderedNodes;
        }

        private List<ModelItem> BuildNodeList(ModelItem startNode)
        {
            if (startNode == null)
            {
                return new List<ModelItem>();
            }
            List<ModelItem> orderedNodes = new List<ModelItem>();
            var step = startNode.GetCurrentValue<FlowStep>();
            orderedNodes.Add(startNode);
            while (step != null && step.Next != null)
            {
                var next = ModelItemUtils.CreateModelItem(step.Next);
                orderedNodes.Add(next);
                step = step.Next as FlowStep;
            }

            return orderedNodes;
        }
        private (List<ModelItem> allNodes, List<ModelItem> orderedNodeList, Flowchart flowchartDiff, WorkflowDesigner wd) GetNodes(IContextualResourceModel resourceModel, bool loadFromServer)
        {
            var wd = new WorkflowDesigner();
            var xaml = resourceModel.IsVersionResource ? resourceModel.Environment?.ProxyLayer?.GetVersion(resourceModel.VersionInfo, resourceModel.ID) : resourceModel.WorkflowXaml;

            var workspace = GlobalConstants.ServerWorkspaceID;
            if (loadFromServer)
            {
                var msg = resourceModel.Environment?.ResourceRepository.FetchResourceDefinition(resourceModel.Environment, workspace, resourceModel.ID, true);
                if (msg != null)
                {
                    xaml = msg.Message;
                }
            }
            else
            {
                Dev2JsonSerializer se = new Dev2JsonSerializer();
                var a = _definationCleaner.GetResourceDefinition(true, resourceModel.ID, resourceModel.WorkflowXaml);
                var executeMessage = se.Deserialize<ExecuteMessage>(a);
                xaml = executeMessage.Message;
            }

            if (xaml == null || xaml.Length == 0)
            {
                throw new Exception($"Could not find resource definition for {resourceModel.ResourceName}");
            }
            wd.Text = xaml.ToString();
            wd.Load();

            var modelService = wd.Context.Services.GetService<ModelService>();

            var workflowHelper = new WorkflowHelper();
            var flowchartDiff = workflowHelper.EnsureImplementation(modelService).Implementation as Flowchart;
            var allNodes = modelService.Find(modelService.Root, typeof(FlowNode)).ToList();
            var startNode = ModelItemUtils.CreateModelItem(flowchartDiff.StartNode);
            var orderedList = BuildNodeList(startNode);
            //var unconnected = nodeList.Select(p => p.GetCurrentValue()).Except(orderedList.Select(p => p.GetCurrentValue())); After this code add these to the ordered list 
            return (allNodes, orderedList, flowchartDiff, wd);
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
