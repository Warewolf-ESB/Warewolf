/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Linq;
using System.Text;
using Dev2.Activities.WF;
using Dev2.Common.X6;
using Dev2.Web;
using Newtonsoft.Json;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    /// <summary>
    /// The XAML → X6 JSON → XAML round trip, shared by <see cref="RoundTripFidelityTests"/> (which
    /// asks "does this still behave the same?") and <see cref="ConverterGapDiagnostics"/> (which asks
    /// "what exactly changed?"). Kept in one place so both always exercise the identical pipeline —
    /// a diagnostic that reproduced the conversion slightly differently would send you hunting for
    /// bugs that the real gate never sees.
    /// </summary>
    internal static class X6RoundTripBridge
    {
        /// <summary>
        /// Merges an <c>X6WorkflowLoadModel</c> JSON string (separate "nodes"/"edges" arrays, as
        /// emitted by <c>WorkflowToX6Converter.ConvertToX6Json</c>) into an <c>X6WorkflowSaveModel</c>
        /// JSON string (a single "cells" array, as expected by
        /// <c>X6ToWorkflowConverter.X6JsonToWorkflow</c>), tagging every edge with the AntV/X6
        /// library's own <c>shape: "edge"</c> convention so X6JsonToWorkflow's <c>c.shape != "edge"</c>
        /// node/edge split resolves correctly.
        ///
        /// <para>
        /// In production the X6 web client performs this merge when the user saves the graph; there is
        /// no equivalent bridge in the .NET codebase because the web-studio frontend that owns the X6
        /// graph instance lives outside this repo.
        /// </para>
        /// </summary>
        internal static string LoadModelToSaveModel(string loadModelJson)
        {
            var load = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(loadModelJson);
            foreach (var edge in load.Edges)
            {
                edge.shape = "edge";
            }
            var save = new X6WorkflowSaveModel
            {
                WorkflowXml = load.WorkflowXml,
                Cells = load.Nodes.Concat(load.Edges).ToList()
            };
            return JsonConvert.SerializeObject(save);
        }

        /// <summary>
        /// Runs one full XAML → X6 → XAML round trip. Throws whatever the converters throw; callers
        /// decide whether an exception is a finding or a failure.
        /// </summary>
        internal static string RoundTripXaml(StringBuilder xamlDefinition)
        {
            var activityBuilder = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(xamlDefinition);
            var loadJson = new WorkflowToX6Converter().ConvertToX6Json(activityBuilder, xamlDefinition.ToString());
            var saveJson = LoadModelToSaveModel(loadJson);
            return new X6ToWorkflowConverter().X6JsonToWorkflow(saveJson).ToString();
        }
    }
}
