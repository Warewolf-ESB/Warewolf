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
using Dev2.Data.SystemTemplates.Models;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    /// <summary>
    /// Builds the executable decision and switch nodes from their persisted flow-node activities. Shared by
    /// <see cref="ActivityParser"/> (the WF path) and the WF-free .bite reader so both interpret the persisted
    /// expression text identically. Arms, cases and defaults are wired by the caller.
    /// </summary>
    public static class FlowNodeFactory
    {
        /// <summary>
        /// A <see cref="DsfDecision"/> for <paramref name="activity"/>, with its conditions parsed from the VB-style
        /// <c>ExecuteDecisionStack("{...}",AmbientDataList)</c> expression text. The VB is never evaluated: the JSON
        /// model is cut out of the string.
        /// </summary>
        public static DsfDecision CreateDecision(DsfFlowDecisionActivity activity)
        {
            var rawText = activity.ExpressionText;
            var activityTextJson = rawText.Substring(rawText.IndexOf("{", StringComparison.Ordinal)).Replace(@""",AmbientDataList)", "").Replace("\"", "!");
            var decisionStack = JsonConvert.DeserializeObject<Dev2DecisionStack>(Dev2DecisionStack.FromVBPersitableModelToJSON(activityTextJson));
            return new DsfDecision(activity)
            {
                Conditions = decisionStack,
                And = decisionStack.Mode == Dev2DecisionMode.AND,
            };
        }

        /// <summary>
        /// A <see cref="DsfSwitch"/> for <paramref name="activity"/>, with its switch variable extracted from the
        /// <c>FetchSwitchData("[[x]]",AmbientDataList)</c> expression text and its display name taken from the flow node.
        /// </summary>
        public static DsfSwitch CreateSwitch(DsfFlowSwitchActivity activity, string displayName)
        {
            var switchVariable = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activity.ExpressionText);
            var dsfSwitch = new DsfSwitch(activity)
            {
                Switch = switchVariable,
                DisplayName = displayName,
                OnErrorVariable = activity.OnErrorVariable,
                OnErrorWorkflow = activity.OnErrorWorkflow,
                IsEndedOnError = activity.IsEndedOnError,
            };
            dsfSwitch.Inner.DisplayName = displayName;
            return dsfSwitch;
        }
    }
}
