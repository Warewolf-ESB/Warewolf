using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Activities.Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Activities.Utils;
using System.Activities.Presentation;
using Dev2;
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

    public class ConflictTree:IEquatable<ConflictTree>
    {

        public ConflictTree(ConflictTreeNode startNode)
        {
            Start = startNode;
        }

        public ConflictTreeNode Start { get; }

        public bool Equals(ConflictTree other)
        {
            switch (other)
            {
                case null when Start == null:
                    {
                        return true;
                    }
                case null when Start != null:
                    {
                        return false;
                    }
                default:
                    {
                        return Start.Equals(other);
                    }
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ConflictTree)obj);
        }

        public override int GetHashCode()
        {
            return (Start != null ? Start.GetHashCode() : 0);
        }
    }

    public class ConflictTreeNode : IEquatable<ConflictTreeNode>
    {
        public ConflictTreeNode(IDev2Activity act, Point location)
        {
            Activity = act;
            UniqueId = act.UniqueID;
            Location = location;
        }

        public void AddChild(ConflictTreeNode node)
        {
            if (Children == null)
            {
                Children = new List<(string uniqueId, ConflictTreeNode node)>();
            }
            Children.Add((node.UniqueId, node));
        }

        public void AddParent(ConflictTreeNode node,string name)
        {
            if (Parents == null)
            {
                Parents = new List<(string name,string uniqueId, ConflictTreeNode node)>();
            }
            Parents.Add((name,node.UniqueId, node));
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(ConflictTreeNode other)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            IsInConflict = true;
            if (other == null)
            {
                return false;
            }
            other.IsInConflict = true;
            if (other.UniqueId != UniqueId)
            {
                return false;
            }
            if (!other.Activity.Equals(Activity))
            {
                return false;
            }
            if(other.Parents==null && Parents != null)
            {
                return false;
            }
            if (other.Parents != null && Parents == null)
            {
                return false;
            }
            if (!Parents.SequenceEqual(other.Parents))
            {
                return false;
            }
            if (other.Children == null && Children != null)
            {
                return false;
            }
            if (other.Children != null && Children == null)
            {
                return false;
            }
            if (!Children.SequenceEqual(other.Children))
            {
                return false;
            }
            IsInConflict = false;
            other.IsInConflict = false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ConflictTreeNode)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = (397) ^ UniqueId.GetHashCode();
            hashCode = (hashCode * 397) ^ (Parents != null ? Parents.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Children != null ? Children.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Activity != null ? Activity.GetHashCode() : 0);
            return hashCode;
        }

        public List<(string name, string uniqueId,ConflictTreeNode node)> Parents { get; private set; }
        public List<(string uniqueId, ConflictTreeNode node)> Children { get; private set; }
        public string UniqueId { get; set; }
        public IDev2Activity Activity { get; }
        public Point Location { get; }
        public bool IsInConflict { get; set; }

    }
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
        
        public (ConflictTree currentTree, ConflictTree diffTree, bool hasConflict) GetConflictTrees(IContextualResourceModel current, IContextualResourceModel difference, bool loadworkflowFromServer = true)
        {
            var currentTree = BuildTree(current, true);
            var diffTree = BuildTree(difference, loadworkflowFromServer);
            return (currentTree, diffTree,!currentTree.Equals(diffTree));
        }

        ConflictTree BuildTree(IContextualResourceModel resourceModel, bool loadFromServer)
        {
            var wd = new WorkflowDesigner();
            var xaml = resourceModel.WorkflowXaml;

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
                var se = new Dev2JsonSerializer();
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
            var allNodes = modelService.Find(modelService.Root, typeof(IDev2Activity)).ToList();
            var startNode = ModelItemUtils.CreateModelItem(flowchartDiff.StartNode);
            ConflictTree conflictTreeNode = null;
            if (startNode != null)
            {
                var start = _activityParser.Parse(new List<IDev2Activity>(), startNode);
                var shapeLocation = GetShapeLocation(wd, startNode);
                var startConflictNode = new ConflictTreeNode(start, shapeLocation);
                conflictTreeNode = new ConflictTree(startConflictNode);
                BuildNodeRelationship(wd, start, startConflictNode,allNodes, conflictTreeNode);
            }
            return conflictTreeNode;
        }

        static void BuildNodeRelationship(WorkflowDesigner wd, IDev2Activity act, ConflictTreeNode parentNode,List<ModelItem> allNodes, ConflictTree tree)
        {
            var actChildNodes = act.GetChildrenNodes();
            foreach (var childAct in actChildNodes)
            {
                var loc = GetShapeLocation(wd, ModelItemUtils.CreateModelItem(childAct.Value));
                var allChildren = tree.Start.Children.Flatten(x => x.node.Children);
                var foundChild = allChildren.FirstOrDefault(a => a.uniqueId == childAct.Value.UniqueID).node;
                var childNode = foundChild ?? new ConflictTreeNode(childAct.Value, loc);
                childNode.AddParent(parentNode,childAct.Key);
                parentNode.AddChild(childNode);
                BuildNodeRelationship(wd, childAct.Value, childNode, allNodes, tree);
            }
        }

        public ConcurrentDictionary<string, (ModelItem leftItem, ModelItem rightItem)> GetAllNodes()
        {
            return _flowNodes;
        }


        (IDev2Activity, IConflictNode) BuildNode(ModelItem nodeModelItem, WorkflowDesigner workflowDesigner)
        {
            var dev2Activity = _activityParser.Parse(new List<IDev2Activity>(), nodeModelItem);
            var shapeLocation = GetShapeLocation(workflowDesigner, nodeModelItem);
            var conflictNode = new ConflictNode(dev2Activity)
            {
                CurrentFlowStep = nodeModelItem,
                NodeLocation = shapeLocation,
            };

            return (dev2Activity, conflictNode);
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
            _conflicts.AddRange(sameConflict);
            _conflicts.AddRange(diffConflict);

        }

        public bool NodeHasConflict(string uniqueId)
        {
            var hasConflict = _conflicts.SingleOrDefault(p => p.Key.Equals(uniqueId));
            return hasConflict.Value;
        }
        private List<KeyValuePair<string, bool>> _conflicts;
        private List<IDev2Activity> _flatCurrentActivities;
        private List<IDev2Activity> _flatDifferentActivities;

        public List<(Guid uniqueId, IConflictNode currentNode, IConflictNode differenceNode, bool hasConflict)> GetDifferences(IContextualResourceModel current, IContextualResourceModel difference, bool loadworkflowFromServer = true)
        {

            var trees = GetConflictTrees(current, difference, loadworkflowFromServer);

            _flowNodes = new ConcurrentDictionary<string, (ModelItem leftItem, ModelItem rightItem)>();
            _flatCurrentActivities = new List<IDev2Activity>();
            _flatDifferentActivities = new List<IDev2Activity>();
            _conflicts = new List<KeyValuePair<string, bool>>();

            var currentDifferences = GetNodes(current, true);
            var remotedifferences = GetNodes(difference, loadworkflowFromServer);

            var allCurentItems = new List<(IDev2Activity, IConflictNode)>();
            var allRemoteItems = new List<(IDev2Activity, IConflictNode)>();

            var treeIndex = 1;
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

            var currentActivities = allCurentItems.Select(p => p.Item1).ToList();
            foreach (var node in remotedifferences.orderedNodeList)
            {
                var nodeConflictPair = BuildNode(node, remotedifferences.wd);
                allRemoteItems.Add(nodeConflictPair);
            }
            var differenceActivities = allRemoteItems.Select(p => p.Item1).ToList();
            BuildDifferenceStore();
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
            var orderedNodes = new List<ModelItem>();
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
            var xaml = resourceModel.WorkflowXaml;

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
                var se = new Dev2JsonSerializer();
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
            return (allNodes, orderedList, flowchartDiff, wd);
        }



        private static Point GetShapeLocation(WorkflowDesigner wd, ModelItem modelItem)
        {
            var shapeLocation = new Point();
            var viewStateService = wd.Context.Services.GetService<ViewStateService>();
            var viewState = viewStateService?.RetrieveAllViewState(modelItem);
            if (viewState != null && viewState.ContainsKey("ShapeLocation"))
            {
                shapeLocation = (Point)viewState["ShapeLocation"];
            }

            return shapeLocation;
        }


    }
}
