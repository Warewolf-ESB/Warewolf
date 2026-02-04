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
using System.Text;
using System.Threading.Tasks;
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
        public bool IncludeResourcesXaml { get; set; }
        public bool IncludeResourcesJson { get; set; }
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
        private const int MaxSystemLogLength = 20_000;
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
                    promptBuilder.AppendLine("You are a Warewolf workflow debugging assistant. You are non-agentic and can only answer questions about the Warewolf resources and system logs provided to you.");
                    promptBuilder.AppendLine();
                    promptBuilder.AppendLine("## Your Capabilities:");

                    var capabilities = new List<string>();

                    if (options.IncludeResourcesJson || options.IncludeResourcesXaml)
                    {
                        capabilities.Add("- List and identify available workflow resources");

                        if (options.IncludeResourcesXaml)
                        {
                            capabilities.Add("- Analyze workflow XAML structure, activities, and data flow");
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

                    // Get resources based on settings
                    if (options.IncludeResourcesJson || options.IncludeResourcesXaml)
                    {
                        options.StatusUpdateCallback?.Invoke(options.IncludeResourcesXaml
                            ? "Loading workspace resources with XAML definitions..."
                            : "Loading workspace resources metadata...");

                        var resourceStopwatch = Stopwatch.StartNew();
                        result.ResourcesJson = GetWorkspaceResourcesAsJson(options.Server, options.IncludeResourcesXaml);
                        resourceStopwatch.Stop();

                        Dev2Logger.Info($"ChatbotContext: Resource loading completed in {resourceStopwatch.ElapsedMilliseconds}ms (includeXaml: {options.IncludeResourcesXaml})", "Warewolf Performance");

                        if (!string.IsNullOrEmpty(result.ResourcesJson))
                        {
                            if (options.IncludeResourcesXaml)
                            {
                                promptBuilder.AppendLine("## Workspace Resources (JSON with Workflow XAML):");
                                promptBuilder.AppendLine("Each workflow resource includes its XAML definition showing activities, connections, and data mappings.");
                            }
                            else
                            {
                                promptBuilder.AppendLine("## Workspace Resources (JSON - Metadata Only):");
                                promptBuilder.AppendLine("Resource names, types, and IDs are provided below.");
                            }
                            promptBuilder.AppendLine("```json");
                            promptBuilder.AppendLine(result.ResourcesJson);
                            promptBuilder.AppendLine("```");
                            promptBuilder.AppendLine();

                            // Count resources
                            try
                            {
                                var resources = JsonConvert.DeserializeObject<List<object>>(result.ResourcesJson);
                                result.ResourceCount = resources?.Count ?? 0;
                                Dev2Logger.Info($"ChatbotContext: Loaded {result.ResourceCount} resources, JSON size: {result.ResourcesJson.Length} chars", "Warewolf Info");
                            }
                            catch (Exception ex)
                            {
                                Dev2Logger.Error("Error counting resources from JSON", ex, "Warewolf Error");
                            }
                        }
                    }

                    // Get system log based on settings
                    if (options.IncludeSystemLog)
                    {
                        options.StatusUpdateCallback?.Invoke("Loading recent system logs...");

                        var logStopwatch = Stopwatch.StartNew();
                        result.SystemLog = GetSystemLog(options.Server);
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

        private string GetWorkspaceResourcesAsJson(IServer server, bool includeXaml)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Ensure server connection is established
                if (server?.Connection == null || !server.Connection.IsConnected)
                {
                    Dev2Logger.Warn("Server connection not established, waiting for connection...", "Warewolf Info");

                    var retries = 0;
                    while ((server?.Connection == null || !server.Connection.IsConnected) && retries < MaxRetryAttempts)
                    {
                        System.Threading.Thread.Sleep(RetryDelayMs);
                        retries++;
                    }

                    if (server?.Connection == null || !server.Connection.IsConnected)
                    {
                        stopwatch.Stop();
                        Dev2Logger.Warn($"Server connection not available after waiting ({stopwatch.ElapsedMilliseconds}ms)", "Warewolf Info");
                        return null;
                    }
                }

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
                    return null;
                }

                dynamic explorerItems = JsonConvert.DeserializeObject(explorerItemsJson);

                var resourceList = new List<object>();

                var extractStopwatch = Stopwatch.StartNew();
                ExtractResourcesFromExplorerItem(server, explorerItems, resourceList, includeXaml);
                extractStopwatch.Stop();

                Dev2Logger.Info($"ChatbotContext: Extracted {resourceList.Count} resources in {extractStopwatch.ElapsedMilliseconds}ms (includeXaml: {includeXaml})", "Warewolf Performance");

                if (resourceList.Count == 0)
                {
                    Dev2Logger.Warn("No resources extracted from explorer items", "Warewolf Info");
                    return null;
                }

                Dev2Logger.Info($"ChatbotContext: Loading {resourceList.Count} resources into chatbot context (includeXaml: {includeXaml})", "Warewolf Info");

                var serializeStopwatch = Stopwatch.StartNew();
                var json = JsonConvert.SerializeObject(resourceList, Formatting.Indented);
                serializeStopwatch.Stop();

                Dev2Logger.Info($"ChatbotContext: JSON serialization completed in {serializeStopwatch.ElapsedMilliseconds}ms, size: {json.Length} chars", "Warewolf Performance");

                if (json.Length > MaxResourcesJsonLength)
                {
                    json = json.Substring(0, MaxResourcesJsonLength) + "\n... (truncated for size)";
                    Dev2Logger.Warn($"ChatbotContext: JSON truncated from {json.Length} to {MaxResourcesJsonLength} chars to avoid token limits", "Warewolf Info");
                }

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
                        string xaml = null;
#pragma warning disable CC0021 // Use nameof - these are string values from API, not type names
                        if (includeXaml && (resourceType == "WorkflowService" || resourceType == "Service"))
#pragma warning restore CC0021
                        {
                            xaml = FetchResourceXaml(server, resourceId);
                        }

                        var resourceInfo = new
                        {
                            id = resourceId,
                            name = resourceName,
                            type = resourceType,
                            xaml = xaml
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

        private static string FetchResourceXaml(IServer server, string resourceId)
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

                if (!string.IsNullOrEmpty(xaml) && xaml.Length > MaxResourceXamlLength)
                {
                    Dev2Logger.Debug($"ChatbotContext: XAML for resource {resourceId} too large ({xaml.Length} chars), excluding from context ({stopwatch.ElapsedMilliseconds}ms)", "Warewolf Debug");
                    return null;
                }

                Dev2Logger.Debug($"ChatbotContext: Fetched XAML for resource {resourceId} in {stopwatch.ElapsedMilliseconds}ms, size: {xaml?.Length ?? 0} chars", "Warewolf Debug");
                return xaml;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Dev2Logger.Error($"ChatbotContext: Error fetching XAML for resource {resourceId} after {stopwatch.ElapsedMilliseconds}ms", ex, "Warewolf Error");
                return null;
            }
        }

        private static string GetSystemLog(IServer server)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                Dev2Logger.Debug("ChatbotContext: Sending FetchCurrentServerLogService request", "Warewolf Debug");

                var comsController = new Dev2.Controller.CommunicationController
                {
                    ServiceName = "FetchCurrentServerLogService"
                };

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

                if (logContent.Length > MaxSystemLogLength)
                {
                    logContent = "... (earlier entries truncated)\n" + logContent.Substring(logContent.Length - MaxSystemLogLength);
                    Dev2Logger.Info($"ChatbotContext: Server log fetched and truncated in {stopwatch.ElapsedMilliseconds}ms, size: {MaxSystemLogLength} chars (truncated)", "Warewolf Performance");
                }
                else
                {
                    Dev2Logger.Info($"ChatbotContext: Server log fetched in {stopwatch.ElapsedMilliseconds}ms, size: {logContent.Length} chars", "Warewolf Performance");
                }

                return logContent;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Dev2Logger.Error($"ChatbotContext: Error getting system log after {stopwatch.ElapsedMilliseconds}ms", ex, "Warewolf Error");
                return "Error reading system log: " + ex.Message;
            }
        }
    }
}
