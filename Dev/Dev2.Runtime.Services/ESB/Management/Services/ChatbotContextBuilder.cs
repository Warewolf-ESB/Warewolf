/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.X6;
using Dev2.Communication;
using Dev2.Runtime.Hosting;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Warewolf.Configuration;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Builds the system prompt for the chatbot by aggregating workspace context
    /// (resource definitions, XAML summaries, and system logs) from server-side sources.
    /// </summary>
    public class ChatbotContextBuilder
    {
        private const int MaxResourceXamlLength = 5_000;

        /// <summary>
        /// Delegate set at server startup (by Dev2.Server) to convert raw workflow XAML to X6 JSON.
        /// Avoids a circular project reference between Dev2.Runtime.Services and Dev2.Activities.
        /// </summary>
        public static Func<Dev2.Common.X6.X6RequestInfo, string> XamlToX6Json { get; set; }

        /// <summary>
        /// Builds the structured system prompt including workspace context.
        /// Reads settings, resources and logs directly from server-side APIs.
        /// </summary>
        public static string BuildSystemPrompt(ChatbotSettingsData settings)
        {
            var promptBuilder = new StringBuilder(4096);

            // Header
            promptBuilder.AppendLine("You are a Warewolf workflow assistant capable of answering questions about workflows, the system log and can collaborate with the user in light content creation.");
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

			// Generate tools
			promptBuilder.AppendLine("When collaborating with the user on creating content, always assume this content is a Warewolf workflow.");

			//promptBuilder.AppendLine("Do not generate any content other than Warewolf workflows. All generated Warewolf workflows must be in Warewolf's JSON format.");
			//TODO: remove these and replace it with the commented line above when we are ready to generate whole workflows:
			promptBuilder.AppendLine("Do not generate any content other than Warewolf tools. All generated Warewolf tools must be in Warewolf's JSON format.");
			promptBuilder.AppendLine("Always assume all of your generated tools will be linked together to form a process automatically by the AI harness.");
			promptBuilder.AppendLine("CRITICAL OUTPUT RULES: Each tool must be output as compact single-line JSON (no indentation, no newlines inside the JSON) inside its own ```json code fence. Do NOT pretty-print or add whitespace formatting to the JSON. Do NOT add explanatory text, headers, or comments between or inside the code fences. Output only the code fences with the compact JSON.");


			promptBuilder.AppendLine("Core concepts of Warewolf's JSON format are:");
            promptBuilder.AppendLine("Tool / Node / Activity: A unit of logic, like Assign, HTTP GET, HTTP POST, or SQL Server Database Connector");
			//promptBuilder.AppendLine("Link / Connector / Edge: A connection between activities, defining a process.");
			//promptBuilder.AppendLine("Workflow: A collection of tools and connections forming a process.");
			promptBuilder.AppendLine("Variables: Data used and saved to memory in the datalist. Variables must be expressed using Warewolf's language syntax. For example: [[VariableName]]");
			promptBuilder.AppendLine("Each tool (node) has a ToolType, a UniqueID, and Properties.");
			promptBuilder.AppendLine("The Assign tool's purpose is to assign a value to a variable and it's key property is: fields[]");
            promptBuilder.AppendLine("Here is a canonical single-line example (compact JSON) you must match exactly in structure when producing assign tools: {\"position\": {\"x\": 100,\"y\": 250},\"size\": null,\"visible\": null,\"shape\": \"DsfDotNetMultiAssignActivity\",\"id\": \"efb0e475-8cfd-4e37-9fa7-9a8e3527e5d8\",\"data\": {\"onerrordata\": {\"errorMessage\": null,\"webServiceUrl\": null,\"endWorkflow\": false},\"fields\": [{\"FieldName\": \"[[MyScalar]]\",\"FieldValue\": \"some value\",\"IndexNumber\": 0,\"Inserted\": false},{\"FieldName\": \"[[MyRecordSet().Field1]]\",\"FieldValue\": \"another value\",\"IndexNumber\": 1,\"Inserted\": false},{\"FieldName\": \"\",\"FieldValue\": \"\",\"IndexNumber\": 2,\"Inserted\": false}],\"type\": \"dsfdotnetmultiassignactivity\",\"displayname\": \"Assign\",\"properties\": {\"DisplayName\": \"Assign\",\"UniqueID\": \"b71ffa82-274c-4e09-af8c-c719f56135ca\",\"IsEndedOnError\": \"False\",\"OnErrorVariable\": \"\",\"OnErrorWorkflow\": \"\"}},\"source\": null,\"target\": null,\"label\": \"Assign\"}");
			promptBuilder.AppendLine("In the examples there are other properties that define a Warewolf tool. They can all be left the same as in the example exept for the key property, the display name to display on the X6 node on the graph, the x and y coordinates to position the X6 node on the X6 graph and the id to distinguish it from other X6 nodes on the X6 graph.");
            promptBuilder.AppendLine("The HTTP GET tool's purpose is to make HTTP GET requests and it's key properties are: headers, outputs, sourceId, querystring, objectResult, isBase64, isOutputToObject and objectname");
			promptBuilder.AppendLine("Property definitions: headers - dictionary of HTTP headers to send; outputs - mapping specifying which parts of the response to save to datalist variables; sourceId - optional reference to a configured HTTP source/connection; querystring - key/value pairs appended to the URL; objectResult - when true, parse the response as a JSON object for structured output; isBase64 - when true, treat the response as Base64 encoded and decode it before storing; isOutputToObject - when true, place the parsed response into an object variable; objectname - the name of the object variable to store the response into.");
            promptBuilder.AppendLine("Here is a canonical single-line example (compact JSON) you must match exactly in structure when producing HTTP GET tools: {\"position\": {\"x\": 100,\"y\": 400},\"size\": null,\"visible\": null,\"shape\": \"HttpGetWebMethodTool\",\"id\": \"354afc9c-0d79-46d1-bffe-6efc0264b0c0\",\"data\": {\"onerrordata\": {\"errorMessage\": \"[[ErrorsVariable]]\",\"webServiceUrl\": \"\",\"endWorkflow\": false},\"type\": \"webgetactivity\",\"displayname\": \"HTTP GET Web Method\",\"UniqueID\": \"c1f84e2c-4475-4a4b-96b8-cbaa914f46b5\",\"headers\": [{\"Name\": \"Content-Type\",\"Value\": \"application/json\"},{\"Name\": \"My-Custom-Header\",\"Value\": \"some sort of value\"},{\"Name\": \"\",\"Value\": \"\"}],\"querystring\": \"\",\"sourceId\": \"f39374fe-ce60-5217-701b-dfd24b08879a\",\"inputs\": null,\"outputs\": [{\"Path\": null,\"MappedFrom\": \"method\",\"MappedTo\": \"[[response().method]]\",\"RecordSetName\": \"response\"},{\"Path\": null,\"MappedFrom\": \"rawBody\",\"MappedTo\": \"[[response().rawBody]]\",\"RecordSetName\": \"response\"}],\"isOutputToObject\": false,\"objectname\": null,\"objectresult\": \"\",\"isresponsebase64\": false,\"properties\": {\"DisplayName\": \"HTTP GET Web Method\",\"UniqueID\": \"c1f84e2c-4475-4a4b-96b8-cbaa914f46b5\",\"OnErrorVariable\": \"[[ErrorsVariable]]\",\"OnErrorWorkflow\": \"\",\"IsEndedOnError\": \"False\",\"IsResponseBase64\": \"False\",\"IsObject\": \"False\",\"QueryString\": \"\",\"ObjectResult\": \"\"}},\"source\": null,\"target\": null,\"label\": \"HTTP GET Web Method\"}");
            promptBuilder.AppendLine("The HTTP POST tool's purpose is to make HTTP POST requests and its key properties are: headers, outputs, sourceId, sourceName, requestUrl, queryString, postdata, postDataType, contentType, customContentType, formData, isFormDataChecked, isUrlEncodedChecked, isManualChecked, response, objectResult, isBase64, isOutputToObject and objectname");
            promptBuilder.AppendLine("Property definitions: headers - dictionary of HTTP headers to send; outputs - mapping specifying which parts of the response to save to datalist variables; sourceId - optional reference to a configured HTTP source/connection; sourceName - optional human-readable name of the source; requestUrl - the target URL for the POST request; queryString - key/value pairs appended to the URL; postdata - the payload to send in the request body (string or structured data). When the payload is JSON it must be provided as an escaped string (e.g. \"{\\\"name\\\":\\\"[[Var]]\\\"}\"). postdataType - indicates how to interpret postdata (e.g., JSON, FormData, Raw); contentType - MIME type of the request body (e.g., 'application/json'); customContentType - override for contentType when using non-standard types; formData - key/value pairs used when sending multipart/form-data; isFormDataChecked - when true, treat postdata as form data; isUrlEncodedChecked - when true, encode form fields as application/x-www-form-urlencoded; isManualChecked - when true, send the postdata exactly as provided without additional encoding; response - optional variable to capture the raw response body; objectResult - when true, parse the response as a JSON object for structured output; isBase64 - when true, treat the response as Base64 encoded and decode it before storing; isOutputToObject - when true, place the parsed response into an object variable; objectname - the name of the object variable to store the response into.");
            promptBuilder.AppendLine("When generating an HTTP POST tool JSON, follow the exact structure required by Warewolf. Required top-level fields: position (with numeric x and y), size (null), visible (null), shape (\"HttpPostWebMethodTool\"), id (GUID). The data object must include: onerrordata, type (\"webpostactivitynew\"), displayname, UniqueID (GUID), headers (array of {Name,Value}), requestUrl, postdata (escaped string when JSON), optional settings, outputs (with MappedFrom and MappedTo), isOutputToObject (false if not using object output), objectname (empty string), properties (must include DisplayName and UniqueID). Do not omit id or UniqueID, and ensure postdata inner JSON is properly escaped so the AI harness can embed it as a string.");
            promptBuilder.AppendLine("Here is a canonical single-line example (compact JSON) you must match exactly in structure when producing HTTP POST tools: {\"position\":{\"x\":100,\"y\":400},\"size\":null,\"visible\":null,\"shape\":\"HttpPostWebMethodTool\",\"id\":\"0b7ba44e-b9c6-42cc-b40a-0f2386027ce8\",\"data\":{\"onerrordata\":{\"errorMessage\":\"[[err]]\",\"webServiceUrl\":\"\",\"endWorkflow\":false},\"type\":\"webpostactivitynew\",\"displayname\":\"HTTP POST Web Method\",\"UniqueID\":\"c1f84e2c-4475-4a4b-96b8-cbaa914f46b5\",\"headers\":[{\"Name\":\"Content-Type\",\"Value\":\"application/json\"}],\"requestUrl\":\"https://example.com/api\",\"settings\":[{\"Name\":\"IsManualChecked\",\"Value\":\"True\"}],\"postdata\":\"{\\\"message\\\":\\\"[[Message]]\\\"}\",\"outputs\":[{\"Path\":null,\"MappedFrom\":\"rawBody\",\"MappedTo\":\"[[response().rawBody]]\",\"RecordSetName\":\"response\"}],\"isOutputToObject\":false,\"objectname\":\"\",\"properties\":{\"DisplayName\":\"HTTP POST Web Method\",\"UniqueID\":\"c1f84e2c-4475-4a4b-96b8-cbaa914f46b5\",\"OnErrorVariable\":\"[[err]]\",\"OnErrorWorkflow\":\"\",\"IsEndedOnError\":\"False\"}},\"source\":null,\"target\":null,\"label\":\"HTTP POST Web Method\"}");
			promptBuilder.AppendLine("The SQL Server Database Connector tool's purpose is to execute stored procedure on an SQL server and it's key properties are: procedurename, executeactionstring, serviceserver, sourceId, commandtimeout, isOutputToObject, objectname, objectresult, inputs, outputs");
			promptBuilder.AppendLine("Property definitions: procedurename - the name of the stored procedure to execute; executeactionstring - the execution action to perform (e.g., 'ExecuteReader', 'ExecuteNonQuery'); serviceserver - reference or identifier of the configured SQL server/source or connection details; sourceId - optional reference to a saved database source configuration; commandtimeout - timeout in seconds for the command execution; inputs - mapping of stored procedure input parameter names to values or datalist variables; outputs - mapping of stored procedure output parameter names to datalist variables; objectresult - when true, parse result sets into structured objects; isOutputToObject - when true, place parsed results into an object variable; objectname - the name of the object variable to store the results into.");
            promptBuilder.AppendLine("Here is an example of a complete SQL Server Database Connector tool json: {\"position\": {\"x\": 100,\"y\": 400},\"size\": null,\"visible\": null,\"shape\": \"DsfSqlServerDatabaseActivity\",\"id\": \"0aa8377b-4842-4c7a-975c-431101843e67\",\"data\": {\"onerrordata\": {\"errorMessage\": \"[[Errors().SQLError]]\",\"webServiceUrl\": \"\",\"endWorkflow\": false},\"type\": \"dsfsqlserverdatabaseactivity\",\"displayname\": \"SQL Server Database\",\"UniqueID\": \"fa29aa9f-bf1a-14b9-a088-f07f2224e803\",\"procedurename\": \"dbo.autoadmin_metadata_cleanup\",\"executeactionstring\": \"dbo.autoadmin_metadata_cleanup\",\"serviceserver\": \"00000000-0000-0000-0000-000000000000\",\"sourceId\": \"c5863857-9356-4740-b282-6581fb6cd0b9\",\"commandtimeout\": null,\"isOutputToObject\": false,\"objectname\": \"\",\"objectresult\": \"[{\\\"name\\\":\\\"\\\",\\\"value\\\":\\\"\\\"}]\",\"inputs\": [{\"ActionName\": \"dbo.autoadmin_metadata_cleanup\",\"Path\": null,\"Dev2ReturnType\": null,\"EmptyIsNull\": true,\"IntellisenseFilter\": 0,\"IsObject\": false,\"Name\": \"schema_version\",\"RequiredField\": false,\"ShortTypeName\": null,\"TypeName\": null,\"Value\": \"[[schema_version]]\",\"FullName\": \"schema_version\"},{\"ActionName\": \"dbo.autoadmin_metadata_cleanup\",\"Path\": null,\"Dev2ReturnType\": null,\"EmptyIsNull\": true,\"IntellisenseFilter\": 0,\"IsObject\": false,\"Name\": \"agent_started\",\"RequiredField\": false,\"ShortTypeName\": null,\"TypeName\": null,\"Value\": \"[[agent_started]]\",\"FullName\": \"agent_started\"},{\"ActionName\": \"dbo.autoadmin_metadata_cleanup\",\"Path\": null,\"Dev2ReturnType\": null,\"EmptyIsNull\": true,\"IntellisenseFilter\": 0,\"IsObject\": false,\"Name\": \"instance_configured\",\"RequiredField\": false,\"ShortTypeName\": null,\"TypeName\": null,\"Value\": \"[[instance_configured]]\",\"FullName\": \"instance_configured\"}],\"outputs\": [{\"Path\": null,\"MappedFrom\": \"name\",\"MappedTo\": \"[[dboautoadminfetchsystemflags().name]]\",\"RecordSetName\": \"dboautoadminfetchsystemflags\"},{\"Path\": null,\"MappedFrom\": \"value\",\"MappedTo\": \"[[dboautoadminfetchsystemflags().value]]\",\"RecordSetName\": \"dboautoadminfetchsystemflags\"}],\"properties\": {\"displayname\": \"SQL Server Database\",\"id\": null,\"ProcedureName\": \"dbo.autoadmin_metadata_cleanup\",\"ExecuteActionString\": \"dbo.autoadmin_metadata_cleanup\",\"RunWorkflowAsync\": \"False\",\"DeferExecution\": \"False\",\"RemoveInputFromOutput\": \"False\",\"IsObject\": \"False\",\"ObjectName\": \"\",\"ObjectResult\": \"[{\\\"name\\\":\\\"\\\",\\\"value\\\":\\\"\\\"}]\",\"Add\": \"False\",\"OnResumeClearAmbientDataList\": \"False\",\"OnResumeClearTags\": \"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\",\"IsUIStep\": \"False\",\"DatabindRecursive\": \"False\",\"IsSimulationEnabled\": \"False\",\"IsWorkflow\": \"False\",\"IsService\": \"False\",\"SimulationMode\": \"OnDemand\",\"UniqueID\": \"fa29aa9f-bf1a-14b9-a088-f07f2224e803\",\"OnErrorVariable\": \"[[Errors().SQLError]]\",\"OnErrorWorkflow\": \"\",\"IsEndedOnError\": \"False\",\"DisplayName\": \"SQL Server Database\"}},\"source\": null,\"target\": null,\"label\": \"SQL Server Database\"}");
			promptBuilder.AppendLine("Warewolf's language syntax includes syntax for defining three different types of variables:");
            promptBuilder.AppendLine("Scalar: For storing simple string values. The syntax is the name of the variable wrapped in double square braces (e.g., [[Total]])");
			promptBuilder.AppendLine("Recordset: For storing a list of strings. Recordsets each have a set of fields, each field is a list of strings.");
			promptBuilder.AppendLine("The syntax is the name of the recordset followed the index one of the string values in the list in brackets then a dot then the name of the field wrapped in double square braces (e.g., [[Customer().Name]])");
			promptBuilder.AppendLine("The index can be left blank to get all values and some tools take just the name of a recordset, with the index in brackets, wrapped in double square braces without the field (e.g., [[Customer()]])");

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
			capabilities.Add("- Collaborate with the user on creating tools for Warewolf workflows.");

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
                            var sanitizedXaml = SanitizeContentForPrompt(xaml);
                            definitions.Add($"Resource: {sanitizedName} (Type: {sanitizedType}, ID: {resourceId}, XAML: {sanitizedXaml})");
                        }
                        else
                        {
                            definitions.Add($"Resource: {sanitizedName} (Type: {sanitizedType}, ID: {resourceId})");
                        }
                    }
                    else
                    {
                        var resourcePath = SanitizeContentForPrompt(resource.GetResourcePath(GlobalConstants.ServerWorkspaceID));
                        if (XamlToX6Json != null && !resource.IsServer && !resource.IsSource)
                        {
                            var result = ResourceCatalog.Instance.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
                            if (result != null && result.Length > 0)
                            {
                                var serviceXaml = new StringBuilder(result.ToString());
                                var cleaner = new ResourceDefinationCleaner();
                                var finalresult = (ExecuteMessage)cleaner.GetRawResourceDefinition(false, resourceId, result);

                                if (finalresult != null && !finalresult.HasError && finalresult.Message != null)
                                {
                                    var workflowXaml = new Dev2.Runtime.ServiceModel.Data.Workflow(serviceXaml.ToXElement(), true);
                                    var info = new Dev2.Common.X6.X6RequestInfo
                                    {
                                        ResourceName = sanitizedName,
                                        ActivityXaml = finalresult.Message.ToString(),
                                        WorkflowXML = workflowXaml.ToServiceDefinition().ToString()
                                    };

                                    var x6Json = XamlToX6Json?.Invoke(info);

                                    x6Json = RemoveExtraData(x6Json);

                                    if (!string.IsNullOrEmpty(x6Json))
                                    {
                                        x6Json = FetchResourceDefinition.RemovePasswordsFromJson(x6Json).ToString();
                                        definitions.Add($"Resource: {sanitizedName}\nType: {sanitizedType}\nPath: {resourcePath}\nID: {resourceId}\nJSON: ```\n{x6Json}\n```");
                                        continue;
                                    }
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

        private static string RemoveExtraData(string x6Json)
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(x6Json);
            if (node is System.Text.Json.Nodes.JsonObject rootObj)
            {
                rootObj.Remove("workflowxml");

                // The JSON is a graph: {"nodes":[...], "edges":[...]}
                // Iterate over each node in the "nodes" array and strip excess properties
                if (rootObj.TryGetPropertyValue("nodes", out var nodesNode) && nodesNode is System.Text.Json.Nodes.JsonArray nodesArray)
                {
                    var fieldsToRemove = new[] { "ErrorMessage", "Path", "WatermarkTextValue", "WatermarkTextVariable", "Inserted", "IsFieldNameFocused", "IsFieldValueFocused", "Errors", "OutList", "HasError", "Error" };
                    var propsToRemove = new[] { "Add", "CreateBookmark", "DatabindRecursive", "IsService", "IsSimulationEnabled", "IsUIStep", "IsWorkflow", "OnResumeClearAmbientDataList", "OnResumeClearTags", "SimulationMode", "UpdateAllOccurrences" };

                    foreach (var nodeItem in nodesArray)
                    {
                        if (nodeItem is not System.Text.Json.Nodes.JsonObject nodeObj)
                        {
                            continue;
                        }

                        if (!nodeObj.TryGetPropertyValue("data", out var dataNode) || dataNode is not System.Text.Json.Nodes.JsonObject dataObj)
                        {
                            continue;
                        }

                        // Remove excess properties from each field in the "fields" array
                        if (dataObj.TryGetPropertyValue("fields", out var fieldsNode) && fieldsNode is System.Text.Json.Nodes.JsonArray fieldsArray)
                        {
                            foreach (var fieldItem in fieldsArray)
                            {
                                if (fieldItem is System.Text.Json.Nodes.JsonObject fieldObj)
                                {
                                    foreach (var prop in fieldsToRemove)
                                    {
                                        fieldObj.Remove(prop);
                                    }
                                }
                            }
                        }

                        // Remove excess properties from "properties" object
                        if (dataObj.TryGetPropertyValue("properties", out var propsNode) && propsNode is System.Text.Json.Nodes.JsonObject propsObj)
                        {
                            foreach (var prop in propsToRemove)
                            {
                                propsObj.Remove(prop);
                            }
                        }
                    }
                }

                return rootObj.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
            }

            return x6Json;
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
