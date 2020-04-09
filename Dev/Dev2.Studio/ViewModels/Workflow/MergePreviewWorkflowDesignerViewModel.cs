#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.ViewModels.Workflow
{
    public class MergePreviewWorkflowDesignerViewModel : WorkflowDesignerViewModel, IMergePreviewWorkflowDesignerViewModel
    {
        public ModelItemCollection NodesCollection
        {
            get
            {
                var service = _workflowDesignerHelper.GetService<ModelService>(_wd);
                var root = service.Root;
                service = _workflowDesignerHelper.GetService<ModelService>(_wd);
                var chart = service.Find(root, typeof(Flowchart)).FirstOrDefault();

                var nodes = chart?.Properties["Nodes"]?.Collection;
                return nodes;
            }
        }

        public MergePreviewWorkflowDesignerViewModel(IContextualResourceModel resource)
            : base(resource, true)
        {
        }

        public MergePreviewWorkflowDesignerViewModel(IWorkflowDesignerWrapper workflowDesignerHelper, IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, IPopupController popupController, IAsyncWorker asyncWorker, bool createDesigner, bool liteInit)
            : base(eventPublisher, resource, workflowHelper, popupController, asyncWorker, createDesigner, liteInit)
        {
            
        }

        public void AddItem(IToolConflictItem model)
        {
            var service = _workflowDesignerHelper.GetService<ModelService>(_wd);
            var root = service.Root;
            var chart = _workflowDesignerHelper.GetService<ModelService>(_wd).Find(root, typeof(Flowchart)).FirstOrDefault();

            var nodes = chart?.Properties["Nodes"]?.Collection;
            if (nodes == null)
            {
                return;
            }

            var nodeToAdd = model.ModelItem;
            var step = model.FlowNode;

            switch (step)
            {
                case FlowStep normalStep:
                    normalStep.Next = null;
                    if (!nodes.Contains(normalStep))
                    {
                        nodes.Add(normalStep);
                    }
                    break;
                case FlowDecision normalDecision:
                    normalDecision.DisplayName = model.MergeDescription;
                    normalDecision.False = null;
                    normalDecision.True = null;
                    nodes.Add(normalDecision);
                    break;
                case FlowSwitch<string> normalSwitch:
                    var switchAct = new DsfFlowSwitchActivity
                    {
                        ExpressionText = String.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                    "(\"", nodeToAdd.GetProperty<string>("Switch"), "\",",
                                                    GlobalConstants.InjectedDecisionDataListVariable,
                                                    ")"),
                        UniqueID = nodeToAdd.GetProperty<string>("UniqueID")
                    };
                    normalSwitch.DisplayName = model.MergeDescription;
                    normalSwitch.Expression = switchAct;
                    normalSwitch.Cases.Clear();
                    normalSwitch.Default = null;
                    nodes.Add(normalSwitch);
                    break;
                default:
                    break;
            }
            var modelItem = GetItemFromNodeCollection(model.UniqueId);
            if (modelItem == null)
            {
                return;
            }
            SetShapeLocation(modelItem, model.NodeLocation);
            model.IsAddedToWorkflow = true;
        }

        public void RemoveItem(IToolConflictItem model)
        {
            var root = _wd.Context.Services.GetService<ModelService>().Root;
            var chart = _wd.Context.Services.GetService<ModelService>().Find(root, typeof(Flowchart)).FirstOrDefault();

            var nodes = chart?.Properties["Nodes"]?.Collection;
            if (nodes == null)
            {
                return;
            }
            model.IsAddedToWorkflow = false;
            var step = model.FlowNode;
            switch (step)
            {
                case FlowStep normalStep:
                    if (nodes.Contains(normalStep))
                    {
                        normalStep.Next = null;
                        nodes.Remove(normalStep);
                    }

                    break;
                case FlowDecision normalDecision:
                    if (nodes.Contains(normalDecision))
                    {
                        nodes.Remove(normalDecision);
                    }

                    break;
                case FlowSwitch<string> normalSwitch:
                    nodes.Remove(normalSwitch);
                    break;
                default:
                    break;
            }
        }

        public void DeLinkActivities(Guid sourceUniqueId, Guid destinationUniqueId, string key)
        {
            if (SetNextForDecision(sourceUniqueId, destinationUniqueId, key, true))
            {
                return;
            }
            if (SetNextForSwitch(sourceUniqueId, destinationUniqueId, key, true))
            {
                return;
            }
            var step = GetRegularActivityFromNodeCollection(sourceUniqueId);
            if (step != null)
            {
                var next = GetItemFromNodeCollection(destinationUniqueId);
                SetNext(next, step, true);
            }
        }

        public void LinkActivities(Guid sourceUniqueId, Guid destinationUniqueId, string key)
        {
            if (sourceUniqueId == destinationUniqueId)
            {
                return;
            }
            if (SetNextForDecision(sourceUniqueId, destinationUniqueId, key))
            {
                return;
            }
            if (SetNextForSwitch(sourceUniqueId, destinationUniqueId, key))
            {
                return;
            }
            var step = GetRegularActivityFromNodeCollection(sourceUniqueId);
            if (step != null)
            {
                var next = GetItemFromNodeCollection(destinationUniqueId);
                SetNext(next, step);
            }
        }

        void SetShapeLocation(ModelItem modelItem, Point location)
        {
            var service = _workflowDesignerHelper.GetService<ViewStateService>(_wd);
            service.RemoveViewState(modelItem, "ShapeLocation");
            service.StoreViewState(modelItem, "ShapeLocation", location);
        }

        public void RemoveStartNodeConnection()
        {
            var root = _wd.Context.Services.GetService<ModelService>().Root;
            var chart = _wd.Context.Services.GetService<ModelService>().Find(root, typeof(Flowchart)).FirstOrDefault();

            var nodes = chart?.Properties["Nodes"]?.Collection;
            if (nodes == null)
            {
                return;
            }
            var startNode = chart.Properties["StartNode"];
            if (startNode?.ComputedValue != null)
            {
                startNode.SetValue(null);
            }
        }

        public void LinkStartNode(IToolConflictItem model)
        {
            if (model == null)
            {
                return;
            }
            var root = _wd.Context.Services.GetService<ModelService>().Root;
            var chart = _wd.Context.Services.GetService<ModelService>().Find(root, typeof(Flowchart)).FirstOrDefault();

            var nodes = chart?.Properties["Nodes"]?.Collection;
            if (nodes == null)
            {
                return;
            }
            var startNode = chart.Properties["StartNode"];
            if (startNode != null && startNode.ComputedValue == null)
            {
                startNode.SetValue(model.FlowNode);
                Selection.Select(_wd.Context, ModelItemUtils.CreateModelItem(model.FlowNode));
            }
        }

        bool SetNextForDecision(Guid sourceUniqueId, Guid destinationUniqueId, string key, bool delink = false)
        {            
            var decisionItem = GetDecisionFromNodeCollection(sourceUniqueId);
            if (decisionItem != null)
            {
                var next = GetItemFromNodeCollection(destinationUniqueId);
                var parentNodeProperty = decisionItem.Properties[key];
                if (parentNodeProperty != null)
                {
                    SetNextForDecision(next, parentNodeProperty, delink);
                    return true;
                }
            }
            return false;
        }

        void SetNextForDecision(ModelItem next, ModelProperty parentNodeProperty, bool delink = false)
        {
            if (next != null)
            {
                if (delink)
                {
                    parentNodeProperty.SetValue(null);
                }
                else
                {
                    parentNodeProperty.SetValue(next);
                    Selection.Select(_wd.Context, ModelItemUtils.CreateModelItem(next));
                }
            }
        }

        bool SetNextForSwitch(Guid sourceUniqueId, Guid destinationUniqueId, string key, bool delink = false)
        {
            var switchItem = GetSwitchFromNodeCollection(sourceUniqueId);
            if (switchItem != null)
            {
                var next = GetItemFromNodeCollection(destinationUniqueId);
                if (next != null)
                {
                    if (next.GetCurrentValue() is FlowNode nodeItem)
                    {
                        UpdateSwitchArm(key, switchItem, nodeItem, delink);
                    }
                    if (!delink)
                    {
                        Selection.Select(_wd.Context, ModelItemUtils.CreateModelItem(next));
                    }
                    return true;
                }                
            }
            return false;
        }

        void SetNext(ModelItem next, ModelItem source, bool delink = false)
        {
            if (next != null)
            {
                var nextStep = next.GetCurrentValue<FlowNode>();
                if (nextStep != null)
                {
                    SetNextFlowNode(next, source, nextStep, delink);
                }
            }
            else
            {
                var currentlyLinkedItem = GetNextItem(source);
                if (currentlyLinkedItem is null)
                {
                    SetNextProperty(source, null);
                }
            }
        }

        void SetNextFlowNode(ModelItem next, ModelItem source, FlowNode nextStep, bool delink = false)
        {
            if (delink)
            {
                var currentlyLinkedItem = GetNextItem(source);
                if (currentlyLinkedItem != null && currentlyLinkedItem == next)
                {
                    SetNextProperty(source, null);
                }
            }
            else
            {
                SetNextProperty(source, nextStep);
                Selection.Select(_wd.Context, ModelItemUtils.CreateModelItem(next));
            }
        }

        static void UpdateSwitchArm(string key, ModelItem switchItem, FlowNode nodeItem, bool delink = false)
        {
            if (key != "Default")
            {
                var parentNodeProperty = switchItem.Properties["Cases"];
                if (parentNodeProperty != null)
                {
                    if (delink)
                    {
                        var cases = parentNodeProperty.Dictionary;
                        cases.Remove(key);
                        parentNodeProperty.SetValue(cases);

                    }
                    else
                    {
                        var cases = parentNodeProperty.Dictionary;
                        cases.Remove(key);
                        cases.Add(key, nodeItem);
                        parentNodeProperty.SetValue(cases);
                    }
                }
            }
            else
            {
                var defaultProperty = switchItem.Properties["Default"];
                if (defaultProperty != null)
                {
                    if (delink)
                    {
                        defaultProperty.SetValue(null);
                    }
                    else
                    {
                        defaultProperty.SetValue(nodeItem);
                    }
                }
            }
        }

        void SetNextProperty(ModelItem source, FlowNode nextStep)
        {
            var parentNodeProperty = source.Properties["Next"];
            if (parentNodeProperty == null)
            {
                return;
            }
            parentNodeProperty.SetValue(nextStep);
        }

        ModelItem GetNextItem(ModelItem source)
        {
            var parentNodeProperty = source.Properties["Next"];
            if (parentNodeProperty == null)
            {
                return null;
            }
            return parentNodeProperty.Value;
        }

        ModelItem GetRegularActivityFromNodeCollection(Guid uniqueId) => NodesCollection.FirstOrDefault(q =>
        {
            var s = q.GetCurrentValue() as FlowStep;
            var act = s?.Action as IDev2Activity;
            return act?.UniqueID == uniqueId.ToString();
        });
        protected ModelItem GetItemFromNodeCollection(Guid uniqueId) => GetDecisionFromNodeCollection(uniqueId) ?? GetSwitchFromNodeCollection(uniqueId) ?? GetRegularActivityFromNodeCollection(uniqueId);

        ModelItem GetDecisionFromNodeCollection(Guid uniqueId) => NodesCollection.FirstOrDefault(q =>
        {
            var decision = q.GetProperty("Condition") as IDev2Activity;
            if (decision == null)
            {
                return false;
            }
            var hasParent = decision.UniqueID == uniqueId.ToString() && q.GetCurrentValue<FlowNode>() is FlowDecision;
            return hasParent;
        });

        ModelItem GetSwitchFromNodeCollection(Guid uniqueId) => NodesCollection.FirstOrDefault(q =>
        {
            var decision = q.GetProperty("Expression") as IDev2Activity;
            if (decision == null)
            {
                return false;
            }
            var hasParent = decision.UniqueID == uniqueId.ToString();
            return hasParent;
        });
    }
}
