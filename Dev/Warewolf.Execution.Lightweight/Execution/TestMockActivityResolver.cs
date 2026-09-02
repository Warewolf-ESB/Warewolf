/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Ports the mock-substitution half of <c>Dev2.Runtime.ESB.Execution.Evaluator</c>'s
    /// <c>MockActivityIfNecessary</c>/<c>RecursivelyMockRecursiveActivities</c>
    /// (<c>Evaluator.cs:336-423</c>) — the ONE part of the server's Service-Test engine that is not
    /// already baked into shared <c>Dev2.Activities</c> code (per-step <b>assertion</b> IS already
    /// there, gated on <c>IDSFDataObject.IsServiceTestExecution</c> — see
    /// <see cref="WorkflowExecutor.ExecuteTestActivityChain"/>'s remarks).
    ///
    /// <para>
    /// <b>Why this is safe to port verbatim here, unlike on a pooled instance.</b>
    /// <c>RecursivelyMockRecursiveActivities</c>'s <c>ForEach</c>/<c>SelectAndApply</c> branches
    /// mutate the target activity's own <c>DataFunc.Handler</c>/<c>ApplyActivityFunc.Handler</c>
    /// property directly (`Evaluator.cs:397-398/404-405`) — on a pooled, cross-execution-shared
    /// <c>WorkflowExecutor.PreparedWorkflow</c>, that mutation would corrupt every future rental of
    /// that pool key exactly the way the six database activities' <c>ServiceExecution</c> field did
    /// before pooling was introduced (see <c>WorkflowExecutor.cs:51-85</c>). <c>execute_test</c>
    /// never pools its <c>PreparedWorkflow</c> (<see cref="WorkflowExecutor.BuildExclusivePreparedWorkflow"/>)
    /// — every test run gets its own, thrown-away-after-use activity graph — so this mutation is
    /// scoped to exactly one execution and cannot leak.
    /// </para>
    /// </summary>
    internal static class TestMockActivityResolver
    {
        /// <summary>
        /// Returns <paramref name="activity"/> unchanged, or a <c>TestMock*Step</c> wrapper in its
        /// place, per the matching <paramref name="testSteps"/> entry's <c>Type</c>/<c>ActivityType</c>.
        /// Container activity types (<c>Sequence</c>/<c>ForEach</c>/<c>SelectAndApply</c>) are mocked
        /// by recursing into their children rather than being replaced wholesale, since their own
        /// non-mocked children must still execute for real.
        /// </summary>
        internal static IDev2Activity MockActivityIfNecessary(IDev2Activity activity, List<IServiceTestStep> testSteps)
        {
            IDev2Activity overriddenActivity = null;
            var foundTestStep = testSteps?.FirstOrDefault(step => activity != null && step.ActivityID.ToString() == activity.UniqueID);
            if (foundTestStep != null)
            {
                var shouldMock = foundTestStep.Type == StepType.Mock;
                var shouldRecursivelyMock = foundTestStep.ActivityType == typeof(DsfSequenceActivity).Name
                                            || foundTestStep.ActivityType == typeof(DsfForEachActivity).Name
                                            || foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name;

                if (shouldMock && !shouldRecursivelyMock)
                {
                    overriddenActivity = ReplaceActivityWithMock(activity, foundTestStep);
                }
                else
                {
                    RecursivelyMockRecursiveActivities(activity, foundTestStep);
                }
            }

            return overriddenActivity ?? activity;
        }

        static IDev2Activity ReplaceActivityWithMock(IDev2Activity resource, IServiceTestStep foundTestStep)
        {
            IDev2Activity overriddenActivity = null;
            if (foundTestStep.ActivityType == typeof(DsfDecision).Name)
            {
                var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == GlobalConstants.ArmResultText);
                if (serviceTestOutput != null)
                {
                    overriddenActivity = new TestMockDecisionStep(resource.As<DsfDecision>()) { NameOfArmToReturn = serviceTestOutput.Value };
                }
            }
            else if (foundTestStep.ActivityType == typeof(DsfSwitch).Name)
            {
                var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == GlobalConstants.ArmResultText);
                if (serviceTestOutput != null)
                {
                    overriddenActivity = new TestMockSwitchStep(resource.As<DsfSwitch>()) { ConditionToUse = serviceTestOutput.Value };
                }
            }
            else
            {
                overriddenActivity = new TestMockStep(resource, foundTestStep.StepOutputs.ToList());
            }

            return overriddenActivity;
        }

        static void RecursivelyMockRecursiveActivities(IDev2Activity activity, IServiceTestStep foundTestStep)
        {
            if (foundTestStep.ActivityType == typeof(DsfSequenceActivity).Name)
            {
                if (activity is DsfSequenceActivity sequenceActivity)
                {
                    RecursivelyMockChildrenOfASequence(foundTestStep, sequenceActivity);
                }
            }
            else if (foundTestStep.ActivityType == typeof(DsfForEachActivity).Name && activity is DsfForEachActivity forEach && foundTestStep.Children != null)
            {
                var replacement = MockActivityIfNecessary(forEach.DataFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                forEach.DataFunc.Handler = replacement;
            }
            else
            {
                if (foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name && activity is DsfSelectAndApplyActivity selectAndApplyActivity && foundTestStep.Children != null)
                {
                    var replacement = MockActivityIfNecessary(selectAndApplyActivity.ApplyActivityFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                    selectAndApplyActivity.ApplyActivityFunc.Handler = replacement;
                }
            }
        }

        static void RecursivelyMockChildrenOfASequence(IServiceTestStep foundTestStep, DsfSequenceActivity sequenceActivity)
        {
            var acts = sequenceActivity.Activities;

            for (var index = 0; index < acts.Count; index++)
            {
                var activity = acts[index];
                if (foundTestStep.Children != null)
                {
                    var replacement = MockActivityIfNecessary(activity as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                    acts[index] = replacement;
                }
            }
        }
    }
}
