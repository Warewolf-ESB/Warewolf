/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using System;
using System.IO;
using System.Linq;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Lightweight IEsbChannel for the Azure Function executor.
    /// Replaces EsbServicesEndpoint: resolves sub-workflows from disk and
    /// executes them inline using the same XAML-parsing/activity-chain approach
    /// as the main WorkflowExecutor — no ResourceCatalog, no WorkspaceRepository.
    /// </summary>
    internal class LightweightEsbChannel : IEsbChannel
    {
        private readonly string _workflowBaseDirectory;

        internal LightweightEsbChannel(string workflowBaseDirectory)
        {
            _workflowBaseDirectory = workflowBaseDirectory ?? string.Empty;

            // Pre-warm the resource cache for the full resource tree so that the first
            // sub-workflow lookup does not incur the directory-scan cost at call time.
            if (!string.IsNullOrEmpty(_workflowBaseDirectory))
                WorkflowResourceCache.Instance.WarmUp(_workflowBaseDirectory);
        }

        /// <summary>
        /// Top-level workflow execution request — not used in the activity-chain path.
        /// </summary>
        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            errors.AddError($"Sub-service execution via ESB is not supported in the lightweight executor (service: '{request?.ServiceName}').");
            return GlobalConstants.NullDataListID;
        }

        /// <summary>
        /// Called by DsfActivity.ExecutionImpl to run a nested workflow.
        /// Mirrors EsbServicesEndpoint.SubExecutionHelper (sync path) but uses
        /// file-based XAML loading instead of ResourceCatalog/EsbServiceInvoker.
        /// </summary>
        public IExecutionEnvironment ExecuteSubRequest(
            IDSFDataObject dataObject,
            Guid workspaceId,
            string inputDefs,
            string outputDefs,
            out ErrorResultTO errors,
            int update,
            bool handleErrors)
        {
            errors = new ErrorResultTO();

            var subWorkflowPath = WorkflowResourceCache.Instance
                .Resolve(_workflowBaseDirectory, dataObject.ResourceID, dataObject.ServiceName);
            if (subWorkflowPath == null)
            {
                errors.AddError($"Sub-workflow '{dataObject.ServiceName}' (ID: {dataObject.ResourceID}) not found in '{_workflowBaseDirectory}'.");
                return dataObject.Environment;
            }

            // Push a child environment with inputs mapped — mirrors CreateNewEnvironmentFromInputMappings
            var childEnv = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs, update);
            dataObject.PushEnvironment(childEnv);

            WorkflowExecutor.PreparedWorkflow prepared = null;
            try
            {
                var fileContents = WorkflowExecutor.ReadWorkflowFile(subWorkflowPath);
                var (xamlDefinition, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
                if (xamlDefinition != null)
                {
                    // Rented for the duration of the sub-workflow, not shared. This path is the
                    // MOST exposed of the three: one sub-workflow is typically called by several
                    // parents, so concurrent callers would otherwise run the same Dsf*Activity
                    // instances simultaneously. See the _workflowPool comment in WorkflowExecutor.
                    prepared = WorkflowExecutor.RentPreparedWorkflow(subWorkflowPath, xamlDefinition);
                    var startActivity = prepared?.StartActivity;
                    if (startActivity != null)
                    {
                        WorkflowExecutor.ExecuteActivityChain(dataObject, startActivity);
                    }
                }
            }
            catch (Exception ex)
            {
                dataObject.Environment.AddError(ex.Message);
                errors.AddError(ex.Message);
            }
            finally
            {
                WorkflowExecutor.ReturnPreparedWorkflow(subWorkflowPath, prepared);
            }

            // Capture child-env results, restore parent env, map outputs back
            var innerEnvironment = dataObject.Environment;
            dataObject.PopEnvironment();
            ApplyOutputMappings(innerEnvironment, dataObject.Environment, outputDefs, update);

            // Propagate child errors to parent when the activity does not self-handle them
            if (innerEnvironment.HasErrors() && !handleErrors)
            {
                foreach (var error in innerEnvironment.AllErrors.Concat(innerEnvironment.Errors))
                {
                    if (!dataObject.Environment.AllErrors.Contains(error))
                    {
                        dataObject.Environment.AllErrors.Add(error);
                        errors.AddError(error);
                    }
                }
            }

            return innerEnvironment;
        }

        /// <summary>
        /// Called by DsfNativeActivity.PerformCustomErrorHandling when OnErrorWorkflow is set.
        /// No-op in the lightweight path.
        /// </summary>
        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors, int update)
        {
            errors = null;
        }

        /// <summary>
        /// Mirrors IdsfDataObjectExtensionMethods.CreateNewEnvironmentFromInputMappings.
        /// </summary>
        public void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update)
        {
            var childEnv = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs, update);
            dataObject.PushEnvironment(childEnv);
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Inline copy of EnvironmentOutputMappingManager.OutputsToEnvironment
        /// (Dev2.Runtime) — avoids adding a heavy project reference.
        /// Handles scalars, record-sets, and complex objects.
        /// </summary>
        private static void ApplyOutputMappings(
            IExecutionEnvironment childEnv,
            IExecutionEnvironment parentEnv,
            string outputDefs,
            int update)
        {
            if (string.IsNullOrEmpty(outputDefs))
                return;

            try
            {
                var factory = DataListFactory.Instance;
                var outputs = factory.CreateOutputParser().Parse(outputDefs);

                // ── Scalars ──────────────────────────────────────────────
                foreach (var def in factory.CreateScalarList(outputs, true)
                             .Where(o => !o.IsRecordSet && !o.IsObject))
                {
                    var result = childEnv.Eval(
                        DataListUtil.AddBracketsToValueIfNotExist(def.Name), update);

                    if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult list
                        && list.Item.Any())
                    {
                        parentEnv.Assign(
                            "[[" + def.Value + "]]",
                            ExecutionEnvironment.WarewolfAtomToString(list.Item.Last()),
                            update);
                    }
                    else if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult atom)
                    {
                        parentEnv.Assign(
                            DataListUtil.AddBracketsToValueIfNotExist(def.Value),
                            ExecutionEnvironment.WarewolfAtomToString(atom.Item),
                            update);
                    }
                }

                // ── Record-sets ───────────────────────────────────────────
                foreach (var recordSet in factory.CreateRecordSetCollection(outputs, true).RecordSets)
                {
                    var outPutRecSet = outputs.FirstOrDefault(
                        d => d.IsRecordSet && d.RecordSetName == recordSet.SetName);
                    if (outPutRecSet == null) continue;

                    foreach (var col in recordSet.Columns)
                    {
                        var result = childEnv.Eval($"[[{col.RecordSetName}(*).{col.Name}]]", 0);
                        if (result is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult rs)
                        {
                            var idx = DataListUtil.GetRecordsetIndexType(col.RawValue);
                            if (idx == enRecordsetIndexType.Star)
                                parentEnv.EvalAssignFromNestedStar(col.RawValue, rs, update);
                            else if (idx == enRecordsetIndexType.Blank)
                                parentEnv.EvalAssignFromNestedLast(col.RawValue, rs, 0);
                            else if (idx == enRecordsetIndexType.Numeric)
                                parentEnv.EvalAssignFromNestedNumeric(col.RawValue, rs, 0);
                        }
                    }
                }

                // ── Complex objects (JSON) ────────────────────────────────
                foreach (var def in factory.CreateObjectList(outputs).Where(o => o.IsObject))
                {
                    var jsonVal = childEnv.EvalJContainer(
                        DataListUtil.AddBracketsToValueIfNotExist(def.Name));
                    if (jsonVal != null)
                        parentEnv.AddToJsonObjects(
                            DataListUtil.AddBracketsToValueIfNotExist(def.Value), jsonVal);
                }

                parentEnv.CommitAssign();
            }
            catch
            {
                // Output mapping is best-effort; execution result is still usable
            }
        }
    }
}
