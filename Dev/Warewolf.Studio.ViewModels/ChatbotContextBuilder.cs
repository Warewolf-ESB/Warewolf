/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    /// <summary>
    /// Configuration options for building chatbot context.
    /// </summary>
    public class ChatbotContextOptions
    {
        public bool IncludeSystemLog { get; set; }
        public bool LoadResourcesAsXaml { get; set; }
        public int NumberOfLogLines { get; set; } = 1000;
        public List<Guid> SelectedResourceIds { get; set; } = new List<Guid>();
        public IServer Server { get; set; }
        public Action<string> StatusUpdateCallback { get; set; }
    }

    /// <summary>
    /// Result of context building operation.
    /// </summary>
    public class ChatbotContextResult
    {
        public string SystemPrompt { get; set; }
        public int ResourceCount { get; set; }
        public bool HasSystemLog { get; set; }
        public string ResourcesJson { get; set; }
        public string SystemLog { get; set; }
    }

    /// <summary>
    /// Interface for building chatbot context from workspace resources and system logs.
    /// </summary>
    public interface IChatbotContextBuilder
    {
        /// <summary>
        /// Builds the system prompt asynchronously with workspace context.
        /// </summary>
        Task<ChatbotContextResult> BuildContextAsync(ChatbotContextOptions options);
    }

    /// <summary>
    /// Builds chatbot context by aggregating workspace resources and system logs.
    /// </summary>
    public class ChatbotContextBuilder : IChatbotContextBuilder
    {
        private const int MaxResourcesJsonLength = 100_000;
        private const int MaxResourceXamlLength = 5_000;
        private const int MaxRetryAttempts = 20;
        private const int RetryDelayMs = 500;

        public async Task<ChatbotContextResult> BuildContextAsync(ChatbotContextOptions options)
        {
            return await Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                var result = new ChatbotContextResult();

                try
                {
                    var promptBuilder = new StringBuilder(4096);
                    BuildSystemPromptHeader(promptBuilder, options);
                    AppendResourcesContext(promptBuilder, options, result);
                    AppendSystemLogContext(promptBuilder, options, result);

                    result.SystemPrompt = promptBuilder.ToString();

                    stopwatch.Stop();
                    Dev2Logger.Info($"ChatbotContext: Total initialization completed in {stopwatch.ElapsedMilliseconds}ms, system prompt size: {result.SystemPrompt.Length} chars", "Warewolf Performance");

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Dev2Logger.Error($"ChatbotContext: Error building context after {stopwatch.ElapsedMilliseconds}ms", ex, "Warewolf Error");

                    result.SystemPrompt = "You are a Warewolf workflow debugging assistant. Note: Workspace context could not be loaded.";
                    return result;
                }
            });
        }

        private static void BuildSystemPromptHeader(StringBuilder promptBuilder, ChatbotContextOptions options)
        {
            promptBuilder.AppendLine("You are a Warewolf workflow debugging assistant. You are non-agentic and can only answer questions about the Warewolf resources and system logs provided to you.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("## Your Capabilities:");

            var capabilities = BuildCapabilitiesList(options);

            if (capabilities.Count > 0)
            {
                promptBuilder.AppendLine(string.Join("\n", capabilities));
            }
            else
            {
                promptBuilder.AppendLine("- Answer general questions about Warewolf workflows");
            }

            promptBuilder.AppendLine();
            promptBuilder.AppendLine("## Important Rules:");
            promptBuilder.AppendLine("- You can ONLY discuss the resources and logs provided below");
            promptBuilder.AppendLine("- Do NOT provide information about resources not in this context");
            promptBuilder.AppendLine("- Do NOT make assumptions about system behavior beyond what's in the logs");
            promptBuilder.AppendLine("- If asked about something not in your context, politely explain you only have access to the provided resources and logs");
            promptBuilder.AppendLine();
        }

        private static List<string> BuildCapabilitiesList(ChatbotContextOptions options)
        {
            var capabilities = new List<string>();

            if (options.SelectedResourceIds != null && options.SelectedResourceIds.Count > 0)
            {
                capabilities.Add("- List and identify available workflow resources");

                if (options.LoadResourcesAsXaml)
                {
                    capabilities.Add("- Analyze workflow structure summaries, activities, and data flow");
                    capabilities.Add("- Explain workflow logic and identify potential issues");
                    capabilities.Add("- Answer questions about workflow structure and dependencies");
                }
            }

            if (options.IncludeSystemLog)
            {
                capabilities.Add("- Help debug issues using the system log");
                capabilities.Add("- Identify errors and warnings in recent activity");
                capabilities.Add("- Trace execution flow from log entries");
            }

            return capabilities;
        }

        private void AppendResourcesContext(StringBuilder promptBuilder, ChatbotContextOptions options, ChatbotContextResult result)
        {
            if (options.SelectedResourceIds == null || options.SelectedResourceIds.Count == 0)
            {
                return;
            }

            options.StatusUpdateCallback?.Invoke(options.LoadResourcesAsXaml
                ? "Loading selected resources with XAML definitions..."
                : "Loading selected resources metadata...");

            var resourceStopwatch = Stopwatch.StartNew();
            result.ResourcesJson = GetSelectedResourcesAsJson(options.Server, options.SelectedResourceIds, options.LoadResourcesAsXaml);
            resourceStopwatch.Stop();

            Dev2Logger.Info($"ChatbotContext: Resource loading completed in {resourceStopwatch.ElapsedMilliseconds}ms (loadAsXaml: {options.LoadResourcesAsXaml})", "Warewolf Performance");

            if (string.IsNullOrEmpty(result.ResourcesJson))
            {
                return;
            }

            if (options.LoadResourcesAsXaml)
            {
                promptBuilder.AppendLine("## Selected Resources (JSON with Workflow Summaries):");
                promptBuilder.AppendLine("Each workflow resource includes a structural summary showing activities, variables, and flow connections.");
            }
            else
            {
                promptBuilder.AppendLine("## Selected Resources (JSON - Metadata Only):");
                promptBuilder.AppendLine("Resource names, types, and IDs are provided below.");
            }
            promptBuilder.AppendLine("```json");
            promptBuilder.AppendLine(result.ResourcesJson);
            promptBuilder.AppendLine("```");
            promptBuilder.AppendLine();

            result.ResourceCount = CountResourcesFromJson(result.ResourcesJson);
        }

        private static int CountResourcesFromJson(string resourcesJson)
        {
            try
            {
                var resources = JsonConvert.DeserializeObject<List<object>>(resourcesJson);
                var count = resources?.Count ?? 0;
                Dev2Logger.Info($"ChatbotContext: Loaded {count} resources, JSON size: {resourcesJson.Length} chars", "Warewolf Info");
                return count;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Error counting resources from JSON", ex, "Warewolf Error");
                return 0;
            }
        }

        private static void AppendSystemLogContext(StringBuilder promptBuilder, ChatbotContextOptions options, ChatbotContextResult result)
        {
            if (!options.IncludeSystemLog)
            {
                return;
            }

            options.StatusUpdateCallback?.Invoke("Loading recent system logs...");

            var logStopwatch = Stopwatch.StartNew();
            result.SystemLog = GetSystemLog(options.Server, options.NumberOfLogLines);
            logStopwatch.Stop();

            Dev2Logger.Info($"ChatbotContext: System log loading completed in {logStopwatch.ElapsedMilliseconds}ms, size: {result.SystemLog?.Length ?? 0} chars", "Warewolf Performance");

            if (!string.IsNullOrEmpty(result.SystemLog))
            {
                result.HasSystemLog = true;
                promptBuilder.AppendLine("## System Log (Recent Entries):");
                promptBuilder.AppendLine("```");
                promptBuilder.AppendLine(result.SystemLog);
                promptBuilder.AppendLine("```");
                promptBuilder.AppendLine();
            }
        }

        private string GetSelectedResourcesAsJson(IServer server, List<Guid> selectedResourceIds, bool loadAsXaml)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!WaitForServerConnection(server))
                {
                    stopwatch.Stop();
                    Dev2Logger.Warn($"Server connection not available after waiting ({stopwatch.ElapsedMilliseconds}ms)", "Warewolf Info");
                    return null;
                }

                if (selectedResourceIds == null || selectedResourceIds.Count == 0)
                {
                    Dev2Logger.Info("ChatbotContext: No resources selected", "Warewolf Info");
                    return null;
                }

                var resourceList = new List<object>();

                foreach (var resourceId in selectedResourceIds)
                {
                    var resourceInfo = FetchResourceInfo(server, resourceId, loadAsXaml);
                    if (resourceInfo != null)
                    {
                        resourceList.Add(resourceInfo);
                    }
                }

                if (resourceList.Count == 0)
                {
                    Dev2Logger.Warn("No resources could be loaded from selected IDs", "Warewolf Info");
                    return null;
                }

                Dev2Logger.Info($"ChatbotContext: Loading {resourceList.Count} selected resources into chatbot context (loadAsXaml: {loadAsXaml})", "Warewolf Info");

                var json = SerializeAndTruncateResources(resourceList);

                stopwatch.Stop();
                Dev2Logger.Info($"ChatbotContext: GetSelectedResourcesAsJson completed in {stopwatch.ElapsedMilliseconds}ms total", "Warewolf Performance");

                return json;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Dev2Logger.Error($"ChatbotContext: Error getting selected resources after {stopwatch.ElapsedMilliseconds}ms", ex, "Warewolf Error");
                return null;
            }
        }

        private object FetchResourceInfo(IServer server, Guid resourceId, bool loadAsXaml)
        {
            try
            {
                // Use FindSingle to get resource information
                var resource = server.ResourceRepository?.FindSingle(r => r.ID == resourceId);
                if (resource == null)
                {
                    Dev2Logger.Warn($"ChatbotContext: Resource {resourceId} not found", "Warewolf Info");
                    return null;
                }

                string workflowSummary = null;
                var resourceType = resource.ResourceType.ToString();

#pragma warning disable CC0021 // Use nameof - these are string values from API, not type names
                if (loadAsXaml && (resourceType == "WorkflowService" || resourceType == "Service"))
#pragma warning restore CC0021
                {
                    workflowSummary = FetchAndSummarizeResourceXaml(server, resourceId.ToString());
                }

                var resourceInfo = new
                {
                    id = resourceId.ToString(),
                    name = SanitizeContentForPrompt(resource.ResourceName),
                    type = SanitizeContentForPrompt(resourceType),
                    summary = workflowSummary
                };

                return resourceInfo;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"ChatbotContext: Error fetching resource info for {resourceId}", ex, "Warewolf Error");
                return null;
            }
        }

        private string GetWorkspaceResourcesAsJson(IServer server, bool includeXaml)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!WaitForServerConnection(server))
                {
                    stopwatch.Stop();
                    Dev2Logger.Warn($"Server connection not available after waiting ({stopwatch.ElapsedMilliseconds}ms)", "Warewolf Info");
                    return null;
                }

                var explorerItemsJson = FetchExplorerItems(server);
                if (string.IsNullOrEmpty(explorerItemsJson))
                {
                    return null;
                }

                var resourceList = ExtractResources(server, explorerItemsJson, includeXaml);
                if (resourceList.Count == 0)
                {
                    Dev2Logger.Warn("No resources extracted from explorer items", "Warewolf Info");
                    return null;
                }

                Dev2Logger.Info($"ChatbotContext: Loading {resourceList.Count} resources into chatbot context (includeXaml: {includeXaml})", "Warewolf Info");

                var json = SerializeAndTruncateResources(resourceList);

                stopwatch.Stop();
                Dev2Logger.Info($"ChatbotContext: GetWorkspaceResourcesAsJson completed in {stopwatch.ElapsedMilliseconds}ms total", "Warewolf Performance");

                return json;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Dev2Logger.Error($"ChatbotContext: Error getting workspace resources after {stopwatch.ElapsedMilliseconds}ms", ex, "Warewolf Error");
                return null;
            }
        }

        private static bool WaitForServerConnection(IServer server)
        {
            if (server?.Connection != null && server.Connection.IsConnected)
            {
                return true;
            }

            Dev2Logger.Warn("Server connection not established, waiting for connection...", "Warewolf Info");

            var retries = 0;
            while ((server?.Connection == null || !server.Connection.IsConnected) && retries < MaxRetryAttempts)
            {
                System.Threading.Thread.Sleep(RetryDelayMs);
                retries++;
            }

            return server?.Connection != null && server.Connection.IsConnected;
        }

        private static string FetchExplorerItems(IServer server)
        {
            var serializer = new Dev2JsonSerializer();
            var servicePayload = new EsbExecuteRequest
            {
                ServiceName = "FetchExplorerItemsService"
            };

            var toSend = serializer.SerializeToBuilder(servicePayload);

            var requestStopwatch = Stopwatch.StartNew();
            Dev2Logger.Debug("ChatbotContext: Sending FetchExplorerItemsService request", "Warewolf Debug");

            var rawPayload = server.Connection.ExecuteCommand(toSend, server.Connection.WorkspaceID);

            requestStopwatch.Stop();
            Dev2Logger.Info($"ChatbotContext: FetchExplorerItemsService response received in {requestStopwatch.ElapsedMilliseconds}ms, payload size: {rawPayload?.Length ?? 0} bytes", "Warewolf Performance");

            if (rawPayload == null || rawPayload.Length == 0)
            {
                Dev2Logger.Warn("No response from FetchExplorerItemsService", "Warewolf Info");
                return null;
            }

            var explorerItemsJson = ExtractExplorerItemsJson(serializer, rawPayload);

            if (string.IsNullOrEmpty(explorerItemsJson))
            {
                Dev2Logger.Warn("No explorer items after decompression", "Warewolf Info");
            }

            return explorerItemsJson;
        }

        private List<object> ExtractResources(IServer server, string explorerItemsJson, bool includeXaml)
        {
            dynamic explorerItems = JsonConvert.DeserializeObject(explorerItemsJson);

            var resourceList = new List<object>();

            var extractStopwatch = Stopwatch.StartNew();
            ExtractResourcesFromExplorerItem(server, explorerItems, resourceList, includeXaml);
            extractStopwatch.Stop();

            Dev2Logger.Info($"ChatbotContext: Extracted {resourceList.Count} resources in {extractStopwatch.ElapsedMilliseconds}ms (includeXaml: {includeXaml})", "Warewolf Performance");

            return resourceList;
        }

        private static string SerializeAndTruncateResources(List<object> resourceList)
        {
            var serializeStopwatch = Stopwatch.StartNew();
            var json = JsonConvert.SerializeObject(resourceList, Formatting.Indented);
            serializeStopwatch.Stop();

            Dev2Logger.Info($"ChatbotContext: JSON serialization completed in {serializeStopwatch.ElapsedMilliseconds}ms, size: {json.Length} chars", "Warewolf Performance");

            if (json.Length > MaxResourcesJsonLength)
            {
                Dev2Logger.Warn($"ChatbotContext: JSON truncated from {json.Length} to {MaxResourcesJsonLength} chars to avoid token limits", "Warewolf Info");
                json = json.Substring(0, MaxResourcesJsonLength) + "\n... (truncated for size)";
            }

            return json;
        }

        private static string ExtractExplorerItemsJson(Dev2JsonSerializer serializer, StringBuilder rawPayload)
        {
            try
            {
                var compressedMessage = serializer.Deserialize<CompressedExecuteMessage>(rawPayload);
                if (compressedMessage != null && compressedMessage.IsCompressed)
                {
                    return compressedMessage.GetDecompressedMessage().ToString();
                }
                else
                {
                    var executeMessage = serializer.Deserialize<ExecuteMessage>(rawPayload);
                    return executeMessage?.Message?.ToString();
                }
            }
            catch
            {
                return rawPayload.ToString();
            }
        }

        private void ExtractResourcesFromExplorerItem(IServer server, dynamic item, List<object> resourceList, bool includeXaml)
        {
            try
            {
                if (item.ResourceType != null && item.ResourceType.ToString() != "Folder")
                {
                    var resourceId = item.ResourceId?.ToString();
                    var resourceName = item.DisplayName?.ToString();
                    var resourceType = item.ResourceType?.ToString();

                    if (!string.IsNullOrEmpty(resourceId) && resourceId != "00000000-0000-0000-0000-000000000000")
                    {
                        string workflowSummary = null;
#pragma warning disable CC0021 // Use nameof - these are string values from API, not type names
                        if (includeXaml && (resourceType == "WorkflowService" || resourceType == "Service"))
#pragma warning restore CC0021
                        {
                            workflowSummary = FetchAndSummarizeResourceXaml(server, resourceId);
                        }

                        // Sanitize resource metadata fields to prevent prompt injection via crafted resource names
                        var resourceInfo = new
                        {
                            id = resourceId,
                            name = SanitizeContentForPrompt(resourceName),
                            type = SanitizeContentForPrompt(resourceType),
                            summary = workflowSummary
                        };

                        resourceList.Add(resourceInfo);
                    }
                }

                if (item.Children != null)
                {
                    foreach (var child in item.Children)
                    {
                        ExtractResourcesFromExplorerItem(server, child, resourceList, includeXaml);
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error extracting resource from explorer item", ex, "Warewolf Error");
            }
        }

        private static string FetchAndSummarizeResourceXaml(IServer server, string resourceId)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!Guid.TryParse(resourceId, out _))
                {
                    return null;
                }

                var comsController = new Dev2.Controller.CommunicationController
                {
                    ServiceName = "FetchResourceDefinitionService"
                };

                comsController.AddPayloadArgument("ResourceID", new StringBuilder(resourceId));

                var result = comsController.ExecuteCommand<ExecuteMessage>(
                    server.Connection,
                    server.Connection.WorkspaceID);

                stopwatch.Stop();

                if (result == null || result.HasError)
                {
                    Dev2Logger.Debug($"ChatbotContext: Failed to fetch XAML for resource {resourceId} in {stopwatch.ElapsedMilliseconds}ms", "Warewolf Debug");
                    return null;
                }

                var xaml = result.Message?.ToString();

                if (string.IsNullOrEmpty(xaml))
                {
                    return null;
                }

                var summary = SummarizeXaml(xaml);

                // Sanitize summary to prevent prompt injection
                summary = SanitizeContentForPrompt(summary);

                Dev2Logger.Debug($"ChatbotContext: Summarized XAML for resource {resourceId} in {stopwatch.ElapsedMilliseconds}ms, original: {xaml.Length} chars, summary: {summary?.Length ?? 0} chars", "Warewolf Debug");
                return summary;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Dev2Logger.Error($"ChatbotContext: Error fetching/summarizing XAML for resource {resourceId} after {stopwatch.ElapsedMilliseconds}ms", ex, "Warewolf Error");
                return null;
            }
        }

        internal static string SummarizeXaml(string xaml)
        {
            try
            {
                var doc = XDocument.Parse(xaml);
                var sb = new StringBuilder();

                // Extract activities (elements with DisplayName attribute)
                var activities = doc.Descendants()
                    .Where(e => e.Attribute("DisplayName") != null)
                    .Select(e => new
                    {
                        DisplayName = e.Attribute("DisplayName")?.Value,
                        TypeName = e.Name.LocalName
                    })
                    .Where(a => !string.IsNullOrEmpty(a.DisplayName))
                    .ToList();

                if (activities.Count > 0)
                {
                    sb.AppendLine("Activities: " + string.Join(", ",
                        activities.Select(a => $"{a.DisplayName} ({a.TypeName})")));
                }

                // Extract variables
                var variables = doc.Descendants()
                    .Where(e => e.Name.LocalName == "Variable")
                    .Select(e => new
                    {
                        Name = e.Attribute("Name")?.Value,
                        TypeName = ExtractVariableTypeName(e.Attribute("Type")?.Value)
                    })
                    .Where(v => !string.IsNullOrEmpty(v.Name))
                    .ToList();

                if (variables.Count > 0)
                {
                    sb.AppendLine("Variables: " + string.Join(", ",
                        variables.Select(v => $"{v.Name} ({v.TypeName})")));
                }

                // Extract FlowStep/FlowDecision/FlowSwitch structure
                var flowSteps = doc.Descendants()
                    .Where(e => e.Name.LocalName == "FlowStep"
                             || e.Name.LocalName == "FlowDecision"
                             || e.Name.LocalName == "FlowSwitch")
                    .Select(e => e.Name.LocalName)
                    .ToList();

                if (flowSteps.Count > 0)
                {
                    sb.AppendLine("Flow structure: " + string.Join(" -> ", flowSteps));
                }

                // Extract DataList fields if present
                var dataListElements = doc.Descendants()
                    .Where(e => e.Name.LocalName == "DataList")
                    .SelectMany(e => e.Elements())
                    .Select(e => e.Name.LocalName)
                    .ToList();

                if (dataListElements.Count > 0)
                {
                    sb.AppendLine("DataList fields: " + string.Join(", ", dataListElements));
                }

                var summary = sb.ToString().Trim();
                return string.IsNullOrEmpty(summary) ? null : summary;
            }
            catch (Exception ex)
            {
                Dev2Logger.Debug($"ChatbotContext: Failed to parse XAML for summarization, falling back to truncated content: {ex.Message}", "Warewolf Debug");

                // Fall back to truncated raw XAML if parsing fails
                if (xaml.Length > MaxResourceXamlLength)
                {
                    return xaml.Substring(0, MaxResourceXamlLength) + "\n... (truncated)";
                }
                return xaml;
            }
        }

        private static string ExtractVariableTypeName(string typeAttribute)
        {
            if (string.IsNullOrEmpty(typeAttribute))
            {
                return "Object";
            }

            // Type attributes look like: "x:String", "scg:List(x:String)", "System.Int32", etc.
            // Extract just the simple type name
            var typeName = typeAttribute;

            var colonIndex = typeName.LastIndexOf(':');
            if (colonIndex >= 0 && colonIndex < typeName.Length - 1)
            {
                typeName = typeName.Substring(colonIndex + 1);
            }

            var dotIndex = typeName.LastIndexOf('.');
            if (dotIndex >= 0 && dotIndex < typeName.Length - 1)
            {
                typeName = typeName.Substring(dotIndex + 1);
            }

            // Remove trailing parentheses/brackets
            var parenIndex = typeName.IndexOf(')');
            if (parenIndex >= 0)
            {
                typeName = typeName.Substring(0, parenIndex);
            }

            return typeName;
        }

        private static string GetSystemLog(IServer server, int numberOfLogLines)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var comsController = new Dev2.Controller.CommunicationController
                {
                    ServiceName = "FetchCurrentServerLogService"
                };
                comsController.AddPayloadArgument("NumberOfLines", new StringBuilder(numberOfLogLines.ToString()));

                var result = comsController.ExecuteCommand<ExecuteMessage>(
                    server.Connection,
                    server.Connection.WorkspaceID);

                stopwatch.Stop();

                if (result == null || result.HasError)
                {
                    var errorMsg = result?.Message?.ToString() ?? "Failed to fetch server log";
                    Dev2Logger.Warn($"ChatbotContext: Failed to fetch server log in {stopwatch.ElapsedMilliseconds}ms: {errorMsg}", "Warewolf Info");
                    return "Unable to fetch server log: " + errorMsg;
                }

                var logContent = result.Message?.ToString();

                if (string.IsNullOrEmpty(logContent))
                {
                    Dev2Logger.Info($"ChatbotContext: No log data available ({stopwatch.ElapsedMilliseconds}ms)", "Warewolf Info");
                    return "No log data available.";
                }

                // Sanitize log content to prevent prompt injection via crafted log entries
                logContent = SanitizeContentForPrompt(logContent);

                var lines = logContent.Split(new[] { '\n' }, StringSplitOptions.None);
                Dev2Logger.Info($"ChatbotContext: Server log fetched in {stopwatch.ElapsedMilliseconds}ms, {lines.Length} lines", "Warewolf Performance");

                return logContent;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Dev2Logger.Error($"ChatbotContext: Error getting system log after {stopwatch.ElapsedMilliseconds}ms", ex, "Warewolf Error");
                return "Error reading system log: " + ex.Message;
            }
        }

        /// <summary>
        /// Sanitizes untrusted content before including it in the system prompt.
        /// Removes control characters, prompt injection patterns, and role-override attempts
        /// that could manipulate the LLM's behavior.
        /// </summary>
        /// <param name="content">The raw content to sanitize (XAML, log text, etc.).</param>
        /// <returns>The sanitized content safe for inclusion in a system prompt.</returns>
        internal static string SanitizeContentForPrompt(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            // Remove control characters (except common whitespace: tab, newline, carriage return)
            var sanitized = Regex.Replace(content, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);

            // Remove zero-width and invisible Unicode characters that could hide injected instructions
            sanitized = Regex.Replace(sanitized, @"[\u200B-\u200F\u2028-\u202F\uFEFF\u00AD]", string.Empty);

            // Neutralize markdown-style heading patterns that could mimic prompt structure
            // e.g., "## New System Instructions:" → "\\## New System Instructions:"
            sanitized = Regex.Replace(sanitized, @"^(#{1,6}\s)", @"\$1", RegexOptions.Multiline);

            // Neutralize triple-backtick fence closers/openers that could break out of code blocks
            sanitized = sanitized.Replace("```", "'''");

            return sanitized;
        }
    }
}
