﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

namespace Warewolf.MergeParser
{
    public class ServiceDifferenceParser : IServiceDifferenceParser
    {
        readonly IActivityParser _activityParser;
        readonly IResourceDefinationCleaner _definationCleaner;

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

        public (List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree) GetConflictTrees(IContextualResourceModel current, IContextualResourceModel difference) => GetConflictTrees(current, difference, true);
        public (List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree) GetConflictTrees(IContextualResourceModel current, IContextualResourceModel difference, bool loadworkflowFromServer)
        {
            var currentTree = BuildTree(current, true);
            var diffTree = BuildTree(difference, loadworkflowFromServer);
            var completeList = currentTree.Concat(diffTree);
            var groupedItems = completeList.GroupBy(a => a.UniqueId);
            foreach (var item in groupedItems)
            {
                var itemList = item.ToList();
                var hasConflict = false;
                if (itemList.Count > 1)
                {
                    var item1 = itemList[0];
                    var item2 = itemList[1];
                    hasConflict = !item1.Equals(item2);
                }
            }
            return (currentTree, diffTree);
        }

        List<ConflictTreeNode> BuildTree(IContextualResourceModel resourceModel, bool loadFromServer)
        {
            var wd = new WorkflowDesigner();
            var xaml = resourceModel.WorkflowXaml;

            var workspace = GlobalConstants.ServerWorkspaceID;
            if (loadFromServer)
            {
                var msg = resourceModel.Environment?.ResourceRepository.FetchResourceDefinition(resourceModel.Environment, workspace, resourceModel.ID, true);
                if (msg != null && msg.Message.Length != 0)
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
            if (flowchartDiff.StartNode == null)
            {
                return new List<ConflictTreeNode>();
            }
            var nodes = BuildNoteItems(wd, modelService, flowchartDiff);
            return nodes;
        }

        List<ConflictTreeNode> BuildNoteItems(WorkflowDesigner wd, ModelService modelService, Flowchart flowchartDiff)
        {
            var idsLocations = GetIdLocations(wd, modelService);
            var nodes = new List<ConflictTreeNode>();
            var startNode = ModelItemUtils.CreateModelItem(flowchartDiff.StartNode);
            if (startNode != null)
            {
                var start = _activityParser.Parse(new List<IDev2Activity>(), startNode);
                var shapeLocation = GetShapeLocation(wd, startNode);
                var startConflictNode = new ConflictTreeNode(start, shapeLocation);
                nodes.Add(startConflictNode);
                BuildItems(idsLocations, nodes, start, startConflictNode);
            }

            return nodes;
        }

        List<(string uniqueId, Point location)> GetIdLocations(WorkflowDesigner wd, ModelService modelService)
        {
            var allNodes = modelService.Find(modelService.Root, typeof(FlowNode)).ToList();
            var idsLocations = new List<(string uniqueId, Point location)>();
            foreach (var n in allNodes)
            {
                var loc = GetShapeLocation(wd, n);
                var id = _activityParser.Parse(new List<IDev2Activity>(), n)?.UniqueID;
                idsLocations.Add((id, loc));
            }
            return idsLocations;
        }

        static void BuildItems(List<(string uniqueId, Point location)> idsLocations, List<ConflictTreeNode> nodes, IDev2Activity start, ConflictTreeNode startConflictNode)
        {
            var nextNodes = start.GetNextNodes();
            var children = start.GetChildrenNodes();
            if (children.Any())
            {
                foreach (var child in children)
                {
                    startConflictNode.AddChild(new ConflictTreeNode(child, idsLocations.FirstOrDefault(t => t.uniqueId == child.UniqueID).location), child.GetDisplayName());
                }
            }
            while (nextNodes.Any())
            {
                var newNextNodes = new List<IDev2Activity>();
                foreach (var next in nextNodes)
                {
                    if (nodes.FirstOrDefault(t => t.UniqueId == next.UniqueID) == null)
                    {
                        var nextConflictNode = new ConflictTreeNode(next, idsLocations.FirstOrDefault(t => t.uniqueId == next.UniqueID).location);
                        nodes.Add(nextConflictNode);
                        newNextNodes.AddRange(next.GetNextNodes());
                    }
                }
                nextNodes = newNextNodes;
            }
        }

        public (List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree) GetDifferences(IContextualResourceModel current, IContextualResourceModel difference) => GetDifferences(current, difference, true);
        public (List<ConflictTreeNode> currentTree, List<ConflictTreeNode> diffTree) GetDifferences(IContextualResourceModel current, IContextualResourceModel difference, bool loadDiffFromLocalServer) => GetConflictTrees(current, difference, loadDiffFromLocalServer);

        static Point GetShapeLocation(WorkflowDesigner wd, ModelItem modelItem)
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
