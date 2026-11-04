/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Flow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Resource.Errors;

namespace Warewolf.Bite.Flow
{
    /// <summary>
    /// Links a <see cref="FlowchartModel"/> into the executable <see cref="IDev2Activity"/> graph exactly as
    /// <see cref="ActivityParser"/> does, quirks included, because the parity harness (T1.1) is the acceptance test:
    /// <list type="bullet">
    /// <item>a step reached twice is the same instance (the seen-list);</item>
    /// <item>a decision or switch reached twice is wrapped again each time (the seen check looks for the inner
    /// activity but the wrapper is what gets added);</item>
    /// <item>a missing successor is <c>null</c>, not an empty list;</item>
    /// <item>a step whose action is not an <see cref="IDev2Activity"/> ends the chain.</item>
    /// </list>
    /// One deliberate difference: a cycle made only of decisions/switches, which recurses until the stack overflows
    /// in <see cref="ActivityParser"/>, fails with a <see cref="BiteFormatException"/>.
    /// </summary>
    public static class FlowGraphBuilder
    {
        public static IDev2Activity? Build(FlowchartModel? chart)
        {
            if (chart?.StartNode == null)
            {
                return null;
            }
            return ParseTools(chart.StartNode, new List<IDev2Activity>(), ImmutableHashSet<FlowNodeModel>.Empty)?.FirstOrDefault();
        }

        // `branches` holds the decisions/switches entered since the last step; re-entering one means a cycle with no
        // step in it, which ActivityParser can never leave.
        static IEnumerable<IDev2Activity>? ParseTools(FlowNodeModel? node, List<IDev2Activity> seen, ImmutableHashSet<FlowNodeModel> branches)
        {
            switch (node)
            {
                case null:
                    return null;
                case FlowStepModel step:
                    return ParseFlowStep(step, seen);
                case FlowDecisionModel decision:
                    return ParseDecision(decision, seen, Enter(decision, branches));
                case FlowSwitchModel flowSwitch:
                    return ParseSwitch(flowSwitch, seen, Enter(flowSwitch, branches));
                default:
                    return null;
            }
        }

        static ImmutableHashSet<FlowNodeModel> Enter(FlowNodeModel branch, ImmutableHashSet<FlowNodeModel> branches) =>
            branches.Contains(branch)
                ? throw new BiteFormatException($"Flowchart cycle through decisions/switches with no step in it, at '{branch.Name}'")
                : branches.Add(branch);

        static IEnumerable<IDev2Activity>? ParseFlowStep(FlowStepModel step, List<IDev2Activity> seen)
        {
            if (!(step.Action is IDev2Activity action))
            {
                return null;
            }
            if (seen.Contains(action))
            {
                return new List<IDev2Activity> { action };
            }
            seen.Add(action);
            action.NextNodes = ParseTools(step.Next, seen, ImmutableHashSet<FlowNodeModel>.Empty);
            return new List<IDev2Activity> { action };
        }

        static IEnumerable<IDev2Activity> ParseDecision(FlowDecisionModel decision, List<IDev2Activity> seen, ImmutableHashSet<FlowNodeModel> branches)
        {
            if (!(decision.Condition is DsfFlowDecisionActivity activity))
            {
                throw new Exception(ErrorResource.InvalidActivity);
            }
            if (seen.Contains(activity))
            {
                return new List<IDev2Activity> { activity };
            }
            var dec = FlowNodeFactory.CreateDecision(activity);
            if (!seen.Contains(activity))
            {
                seen.Add(dec);
            }
            dec.TrueArm = ParseTools(decision.True, seen, branches);
            dec.FalseArm = ParseTools(decision.False, seen, branches);
            return new List<IDev2Activity> { dec };
        }

        static IEnumerable<IDev2Activity> ParseSwitch(FlowSwitchModel flowSwitch, List<IDev2Activity> seen, ImmutableHashSet<FlowNodeModel> branches)
        {
            if (!(flowSwitch.Expression is DsfFlowSwitchActivity activity))
            {
                throw new Exception(ErrorResource.InvalidActivity);
            }
            if (seen.Contains(activity))
            {
                return new List<IDev2Activity> { activity };
            }
            var swi = FlowNodeFactory.CreateSwitch(activity, flowSwitch.DisplayName);
            if (!seen.Contains(activity))
            {
                seen.Add(swi);
            }
            swi.Switches = flowSwitch.Cases
                .Select(c => new Tuple<string, IDev2Activity>(c.Key, ParseTools(c.Value, seen, branches).FirstOrDefault()))
                .ToDictionary(c => c.Item1, c => c.Item2);
            swi.Default = ParseTools(flowSwitch.Default, seen, branches);
            return new List<IDev2Activity> { swi };
        }
    }
}
