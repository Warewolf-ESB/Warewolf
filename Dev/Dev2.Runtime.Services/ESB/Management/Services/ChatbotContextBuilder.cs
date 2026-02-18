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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Runtime.Hosting;
using Warewolf.Configuration;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Builds the system prompt for the chatbot by aggregating workspace context
    /// (resource definitions, XAML summaries, and system logs) from server-side sources.
    /// </summary>
    internal class ChatbotContextBuilder
    {
        private const int MaxResourceXamlLength = 5_000;

        /// <summary>
        /// Delegate set at server startup (by Dev2.Server) to convert raw workflow XAML to X6 JSON.
        /// Avoids a circular project reference between Dev2.Runtime.Services and Dev2.Activities.
        /// </summary>
        internal static Func<Dev2.Common.X6.X6RequestInfo, string> XamlToX6Json { get; set; }

        /// <summary>
        /// Builds the structured system prompt including workspace context.
        /// Reads settings, resources and logs directly from server-side APIs.
        /// </summary>
        public static string BuildSystemPrompt(ChatbotSettingsData settings)
        {
            var promptBuilder = new StringBuilder(4096);

            // Header
            promptBuilder.AppendLine("You are a Warewolf workflow debugging assistant. You are non-agentic and can only answer questions about the Warewolf resources and system logs provided to you.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("## Your Capabilities:");

            var capabilities = BuildCapabilitiesList(settings);
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

            // Append resource definitions
            if (settings.SelectedResourceIds != null && settings.SelectedResourceIds.Count > 0)
            {
                try
                {
                    var resourceDefinitions = GetResourceDefinitions(settings.SelectedResourceIds, settings.LoadResourcesAsXaml);
                    if (resourceDefinitions.Any())
                    {
                        if (settings.LoadResourcesAsXaml)
                        {
                            promptBuilder.AppendLine("## Selected Resources (with Workflow Summaries):");
                            promptBuilder.AppendLine("Each workflow resource includes a structural summary showing activities, variables, and flow connections.");
                        }
                        else
                        {
                            promptBuilder.AppendLine("## Selected Resources (JSON Definitions):");
                            promptBuilder.AppendLine("Each resource includes its X6 graph JSON definition showing the full workflow structure.");
                        }

                        foreach (var def in resourceDefinitions)
                        {
                            promptBuilder.AppendLine(def);
                        }
                        promptBuilder.AppendLine();
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"Failed to retrieve resource definitions: {ex.Message}", GlobalConstants.WarewolfWarn);
                }
            }

            // Append system log
            if (settings.IncludeSystemLog)
            {
                try
                {
                    var logEntries = ReadRecentLogEntries(settings.NumberOfLogLines);
                    if (logEntries.Any())
                    {
                        promptBuilder.AppendLine("## System Log (Recent Entries):");
                        promptBuilder.AppendLine("```");
                        var logContent = SanitizeContentForPrompt(string.Join("\n", logEntries));
                        promptBuilder.AppendLine(logContent);
                        promptBuilder.AppendLine("```");
                        promptBuilder.AppendLine();
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"Failed to retrieve system log entries: {ex.Message}", GlobalConstants.WarewolfWarn);
                }
            }

            return promptBuilder.ToString();
        }

        private static List<string> BuildCapabilitiesList(ChatbotSettingsData settings)
        {
            var capabilities = new List<string>();

            if (settings.SelectedResourceIds != null && settings.SelectedResourceIds.Count > 0)
            {
                capabilities.Add("- List and identify available workflow resources");

                if (settings.LoadResourcesAsXaml)
                {
                    capabilities.Add("- Analyze workflow structure summaries, activities, and data flow");
                    capabilities.Add("- Explain workflow logic and identify potential issues");
                    capabilities.Add("- Answer questions about workflow structure and dependencies");
                }
                else
                {
                    capabilities.Add("- Analyze workflow JSON definitions showing nodes, edges, and activity configuration");
                    capabilities.Add("- Explain workflow logic and identify potential issues");
                    capabilities.Add("- Answer questions about workflow structure and dependencies");
                }
            }

            if (settings.IncludeSystemLog)
            {
                capabilities.Add("- Help debug issues using the system log");
                capabilities.Add("- Identify errors and warnings in recent activity");
                capabilities.Add("- Trace execution flow from log entries");
            }

            return capabilities;
        }

        private static List<string> GetResourceDefinitions(List<Guid> resourceIds, bool loadAsXaml)
        {
            var definitions = new List<string>();

            foreach (var resourceId in resourceIds)
            {
                try
                {
                    var resource = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
                    if (resource == null)
                    {
                        continue;
                    }

                    var sanitizedName = SanitizeContentForPrompt(resource.ResourceName);
                    var sanitizedType = SanitizeContentForPrompt(resource.ResourceType.ToString());

                    if (loadAsXaml)
                    {
                        var resourceXml = ResourceCatalog.Instance.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
                        if (resourceXml != null && resourceXml.Length > 0)
                        {
                            var xaml = resourceXml.ToString();
                            var summary = SummarizeXaml(xaml);
                            summary = SanitizeContentForPrompt(summary);
                            definitions.Add($"Resource: {sanitizedName} (Type: {sanitizedType}, ID: {resourceId})\n{summary}");
                        }
                        else
                        {
                            definitions.Add($"Resource: {sanitizedName} (Type: {sanitizedType}, ID: {resourceId})");
                        }
                    }
                    else
                    {
                        var resourcePath = SanitizeContentForPrompt(resource.GetResourcePath(GlobalConstants.ServerWorkspaceID));
                        if (XamlToX6Json != null)
                        {
                            var resourceXml = ResourceCatalog.Instance.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
                            if (resourceXml != null && resourceXml.Length > 0)
                            {
                                var xaml = resourceXml.ToString();
                                var x6Json = XamlToX6Json(new Dev2.Common.X6.X6RequestInfo { ActivityXaml = xaml, WorkflowXML = xaml, ResourceName = sanitizedName });
                                if (!string.IsNullOrEmpty(x6Json))
                                {
                                    definitions.Add($"Resource: {sanitizedName} (Type: {sanitizedType}, Path: {resourcePath}, ID: {resourceId})\n```json\n{x6Json}\n```");
                                    continue;
                                }
                            }
                        }
                        definitions.Add($"Resource: {sanitizedName}\nType: {sanitizedType}\nPath: {resourcePath}\nID: {resourceId}");
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"Failed to load resource {resourceId}: {ex.Message}", GlobalConstants.WarewolfWarn);
                }
            }

            return definitions;
        }

        private static List<string> ReadRecentLogEntries(int numberOfLines)
        {
            var logEntries = new List<string>();
            var serverLogPath = EnvironmentVariables.ServerLogFile;

            if (string.IsNullOrWhiteSpace(serverLogPath) || !File.Exists(serverLogPath))
            {
                return logEntries;
            }

            try
            {
                using (var fileStream = new FileStream(serverLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var allLines = new List<string>();
                    while (!streamReader.EndOfStream)
                    {
                        allLines.Add(streamReader.ReadLine());
                    }

                    logEntries = allLines.Count > numberOfLines
                        ? allLines.Skip(allLines.Count - numberOfLines).ToList()
                        : allLines;
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn($"Failed to read log file: {ex.Message}", GlobalConstants.WarewolfWarn);
            }

            return logEntries;
        }

        /// <summary>
        /// Parses XAML and produces a concise human-readable summary of a workflow's structure,
        /// including activities, variables, flow structure, and DataList fields.
        /// </summary>
        internal static string SummarizeXaml(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
            {
                return null;
            }

            try
            {
                var doc = XDocument.Parse(xaml);
                var sb = new StringBuilder();

                // Extract activities (elements with a DisplayName attribute or recognisable activity elements)
                var activityElements = doc.Descendants()
                    .Select(e => new
                    {
                        DisplayName = e.Attribute("DisplayName")?.Value,
                        TypeName = e.Name.LocalName
                    })
                    .Where(a => a.TypeName != null && a.TypeName != "Variable" && a.TypeName != "DataList")
                    .ToList();

                var activities = new List<(string DisplayName, string TypeName)>();
                var unnamedCount = 0;
                foreach (var a in activityElements)
                {
                    var name = a.DisplayName;
                    if (string.IsNullOrEmpty(name))
                    {
                        unnamedCount++;
                        name = $"(unnamed {a.TypeName} #{unnamedCount})";
                    }
                    activities.Add((SanitizeContentForPrompt(name), a.TypeName));
                }

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

                // Extract flow structure
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

                // Extract DataList fields
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
                Dev2Logger.Debug($"Failed to parse XAML for summarization, falling back to truncated content: {ex.Message}", GlobalConstants.WarewolfDebug);

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

            var parenIndex = typeName.IndexOf(')');
            if (parenIndex >= 0)
            {
                typeName = typeName.Substring(0, parenIndex);
            }

            return typeName;
        }

        /// <summary>
        /// Sanitizes untrusted content before including it in the system prompt.
        /// Removes control characters, prompt injection patterns, and role-override attempts.
        /// </summary>
        internal static string SanitizeContentForPrompt(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            // Remove control characters (except common whitespace: tab, newline, carriage return)
            var sanitized = Regex.Replace(content, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);

            // Remove zero-width and invisible Unicode characters
            sanitized = Regex.Replace(sanitized, @"[\u200B-\u200F\u2028-\u202F\uFEFF\u00AD]", string.Empty);

            // Neutralize markdown-style heading patterns that could mimic prompt structure
            sanitized = Regex.Replace(sanitized, @"^(#{1,6}\s)", @"\$1", RegexOptions.Multiline);

            // Neutralize triple-backtick fence closers/openers that could break out of code blocks
            sanitized = sanitized.Replace("```", "'''");

            return sanitized;
        }
    }
}
