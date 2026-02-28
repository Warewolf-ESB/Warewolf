using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Warewolf.Execution.AzureFunction.Lightweight.Models;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Lightweight workflow executor that reads a workflow XML file directly,
    /// parses the XAML into a DynamicActivity, and walks the IDev2Activity chain.
    ///
    /// Execution pipeline (no ESB/ResourceCatalog/ServiceInvoker overhead):
    ///   1. Read workflow XML file from disk
    ///   2. Extract XamlDefinition + DataList from the XML
    ///   3. Load XAML ? DynamicActivity via ActivityXamlServices
    ///   4. Parse DynamicActivity ? IDev2Activity chain via ActivityParser
    ///   5. Build DsfDataObject with input parameters mapped into the environment
    ///   6. Walk IDev2Activity.Execute() in a loop
    ///   7. Extract output JSON from the execution environment
    /// </summary>
    public class WorkflowExecutor : IWorkflowExecutor
    {
        /// <summary>
        /// Executes a workflow from a file path with the provided input parameters.
        /// </summary>
        public WorkflowExecutionResult Execute(string workflowFilePath, Dictionary<string, string> inputs = null)
        {
            return Execute(new WorkflowExecutionRequest
            {
                WorkflowFilePath = workflowFilePath,
                InputParameters = inputs ?? new Dictionary<string, string>()
            });
        }

        /// <summary>
        /// Executes a workflow based on a <see cref="WorkflowExecutionRequest"/>.
        /// </summary>
        public WorkflowExecutionResult Execute(WorkflowExecutionRequest request)
        {
            if (request == null)
            {
                return WorkflowExecutionResult.Failure("Execution request cannot be null.");
            }

            if (!request.IsValid)
            {
                return WorkflowExecutionResult.Failure("WorkflowFilePath must be provided.");
            }

            if (!File.Exists(request.WorkflowFilePath))
            {
                return WorkflowExecutionResult.Failure($"Workflow file not found: {request.WorkflowFilePath}");
            }

            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;
            var executionId = Guid.NewGuid();

            try
            {
                // Step 1: Read the workflow XML file
                var fileContents = ReadWorkflowFile(request.WorkflowFilePath);

                // Step 2: Extract XamlDefinition and DataList from the XML
                var (xamlDefinition, dataList, workflowName) = ExtractWorkflowParts(fileContents);

                if (xamlDefinition == null || xamlDefinition.Length == 0)
                {
                    return WorkflowExecutionResult.Failure("No XamlDefinition found in the workflow file.");
                }

                var resolvedName = request.WorkflowName
                    ?? workflowName
                    ?? Path.GetFileNameWithoutExtension(request.WorkflowFilePath);

                // Step 3: Load XAML into a DynamicActivity
                var dynamicActivity = LoadDynamicActivity(xamlDefinition);

                if (dynamicActivity == null)
                {
                    return WorkflowExecutionResult.Failure("Failed to load DynamicActivity from XAML.");
                }

                // Step 4: Parse DynamicActivity into IDev2Activity chain
                var activityParser = new ActivityParser();
                var startActivity = activityParser.Parse(dynamicActivity);

                if (startActivity == null)
                {
                    return WorkflowExecutionResult.Failure(GlobalConstants.NoStartNodeError);
                }

                // Step 5: Build DsfDataObject with inputs
                var dataObject = BuildDataObject(request, executionId, resolvedName, dataList);

                // Step 6: Execute the activity chain
                ExecuteActivityChain(dataObject, startActivity);

                // Step 7: Extract outputs
                stopwatch.Stop();
                var result = new WorkflowExecutionResult
                {
                    ExecutionId = executionId,
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };

                CollectErrors(dataObject, result);
                TryExtractOutputs(dataObject, result);

                result.IsSuccess = result.Errors.Count == 0;

                return result;
            }
            catch (InvalidWorkflowException iwe)
            {
                stopwatch.Stop();
                var msg = iwe.Message;
                var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                var errorMessage = start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message;
                return new WorkflowExecutionResult
                {
                    IsSuccess = false,
                    ExecutionId = executionId,
                    Errors = new List<string> { errorMessage },
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new WorkflowExecutionResult
                {
                    IsSuccess = false,
                    ExecutionId = executionId,
                    Errors = new List<string> { ex.Message },
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Step 1: Read the workflow resource XML file from disk.
        /// </summary>
        static StringBuilder ReadWorkflowFile(string filePath)
        {
            var contents = new StringBuilder();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    contents.Append(line);
                    contents.Append(Environment.NewLine);
                }
            }
            return contents;
        }

        /// <summary>
        /// Step 2: Parse the resource XML to extract the XAML definition and DataList.
        /// Warewolf resource files have structure:
        ///   &lt;Service Name="..."&gt;
        ///     &lt;Actions&gt;
        ///       &lt;Action Type="Workflow"&gt;
        ///         &lt;XamlDefinition&gt;...XAML...&lt;/XamlDefinition&gt;
        ///       &lt;/Action&gt;
        ///     &lt;/Actions&gt;
        ///     &lt;DataList&gt;...&lt;/DataList&gt;
        ///   &lt;/Service&gt;
        /// </summary>
        static (StringBuilder xamlDefinition, string dataList, string workflowName) ExtractWorkflowParts(StringBuilder fileContents)
        {
            var xe = fileContents.ToXElement();

            var workflowName = Dev2.Common.Common.ExtensionMethods.AttributeSafe(xe, "Name");

            var actions = xe.Element("Actions");
            var action = actions != null ? actions.Element("Action") : xe.Element("Action");

            StringBuilder xamlDefinition = null;
            if (action != null)
            {
                var xElement = action.Element("XamlDefinition");
                if (xElement != null)
                {
                    var def = xElement.ToStringBuilder();
                    def = def.Replace("<XamlDefinition>", "").Replace("</XamlDefinition>", "");
                    xamlDefinition = def.Unescape();
                }
            }

            string dataList = null;
            var dataListElement = xe.Element("DataList");
            if (dataListElement != null)
            {
                dataList = dataListElement.ToString();
            }

            return (xamlDefinition, dataList, workflowName);
        }

        /// <summary>
        /// Step 3: Load XAML definition into a DynamicActivity using ActivityXamlServices.
        /// Applies namespace cleaning for cross-platform compatibility.
        /// </summary>
        static DynamicActivity LoadDynamicActivity(StringBuilder xamlDefinition)
        {
            if (GlobalConstants.RuntimeNamespaceClean)
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }

            Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);

            using var xamlStream = xamlDefinition.EncodeForXmlDocument();
            var activity = ActivityXamlServices.Load(xamlStream);
            return activity as DynamicActivity;
        }

        /// <summary>
        /// Step 5: Build DsfDataObject and map input parameters into the execution environment.
        /// </summary>
        static IDSFDataObject BuildDataObject(
            WorkflowExecutionRequest request,
            Guid executionId,
            string workflowName,
            string dataList)
        {
            var rawPayload = BuildJsonPayload(request.InputParameters);

            var dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), rawPayload)
            {
                IsDebug = request.IsDebug,
                ReturnType = EmitionTypes.JSON,
                ServiceName = workflowName,
                ExecutionID = executionId,
                ExecutionToken = new LightweightExecutionToken()
            };

            if (!string.IsNullOrEmpty(dataList)
                && request.InputParameters != null
                && request.InputParameters.Count > 0)
            {
                ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(
                    dataObject,
                    dataObject.RawPayload,
                    dataList);
            }

            return dataObject;
        }

        /// <summary>
        /// Serialize input parameters to a JSON payload string.
        /// </summary>
        static string BuildJsonPayload(Dictionary<string, string> inputs)
        {
            if (inputs == null || inputs.Count == 0)
            {
                return string.Empty;
            }

            var jObject = new JObject();
            foreach (var kvp in inputs)
            {
                jObject[kvp.Key] = kvp.Value;
            }
            return jObject.ToString(Formatting.None);
        }

        /// <summary>
        /// Step 6: Walk the IDev2Activity linked list, calling Execute() on each node.
        /// This is the stripped-down version of WfExecutionContainer.ExecuteNode —
        /// no ExecutionManager, no SubscriptionProvider, no StateNotifier overhead.
        /// </summary>
        static void ExecuteActivityChain(IDSFDataObject dataObject, IDev2Activity startActivity)
        {
            var next = startActivity;
            var environment = dataObject.Environment;

            while (next != null)
            {
                var current = next;
                next = current.Execute(dataObject, 0);
                environment.AllErrors.UnionWith(environment.Errors);

                if (dataObject.StopExecution)
                {
                    var fetchedErrors = environment.FetchErrors();
                    if (!string.IsNullOrEmpty(fetchedErrors))
                    {
                        dataObject.ExecutionException = new Exception(fetchedErrors);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Collect errors from the execution environment into the result.
        /// </summary>
        static void CollectErrors(IDSFDataObject dataObject, WorkflowExecutionResult result)
        {
            var errors = new ErrorResultTO();
            foreach (var err in dataObject.Environment.Errors.ToList())
            {
                errors.AddError(err, true);
            }
            foreach (var err in dataObject.Environment.AllErrors.ToList())
            {
                errors.AddError(err, true);
            }

            if (errors.HasErrors())
            {
                result.Errors = errors.FetchErrors().ToList();
            }

            if (dataObject.ExecutionException != null && result.Errors.Count == 0)
            {
                result.Errors.Add(dataObject.ExecutionException.Message);
            }
        }

        /// <summary>
        /// Step 7: Extract output data from the execution environment.
        /// </summary>
        static void TryExtractOutputs(IDSFDataObject dataObject, WorkflowExecutionResult result)
        {
            try
            {
                var environment = dataObject.Environment;
                if (environment == null)
                {
                    return;
                }

                var jsonOutput = environment.ToJson();
                if (!string.IsNullOrWhiteSpace(jsonOutput))
                {
                    result.OutputJson = jsonOutput;
                    try
                    {
                        var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonOutput);
                        if (parsed != null)
                        {
                            result.Outputs = parsed;
                        }
                    }
                    catch
                    {
                        // Raw OutputJson is still available if dictionary parse fails
                    }
                }
            }
            catch
            {
                // Output extraction is best-effort
            }
        }
    }

    /// <summary>
    /// Minimal IExecutionToken implementation — no server-side dependencies.
    /// </summary>
    internal class LightweightExecutionToken : IExecutionToken
    {
        public bool IsUserCanceled { get; set; }
    }
}
