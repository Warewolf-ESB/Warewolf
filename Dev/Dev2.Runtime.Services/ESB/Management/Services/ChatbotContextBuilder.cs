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


			promptBuilder.AppendLine("Core concepts of Warewolf's JSON format are:");
            promptBuilder.AppendLine("Tool / Node / Activity: A unit of logic, like Assign, HTTP GET, HTTP POST, or SQL Server Database Connector");
			//promptBuilder.AppendLine("Link / Connector / Edge: A connection between activities, defining a process.");
			//promptBuilder.AppendLine("Workflow: A collection of tools and connections forming a process.");
			promptBuilder.AppendLine("Variables: Data used and saved to memory in the datalist. Variables must be expressed using Warewolf's language syntax. For example: [[VariableName]]");
			promptBuilder.AppendLine("Each tool (node) has a ToolType, a UniqueID, and Properties.");
			promptBuilder.AppendLine("The Assign tool's purpose is to assign a value to a variable and it's key property is: fields[]");
            promptBuilder.AppendLine("Here is an example of a complete assign tool json: {\"position\": {\"x\": 100,\"y\": 250},\"size\": null,\"visible\": null,\"shape\": \"DsfDotNetMultiAssignActivity\",\"id\": \"efb0e475-8cfd-4e37-9fa7-9a8e3527e5d8\",\"data\": {\"onerrordata\": {\"errorMessage\": null,\"webServiceUrl\": null,\"endWorkflow\": false},\"fields\": [{\"ErrorMessage\": null,\"Path\": null,\"WatermarkTextValue\": null,\"WatermarkTextVariable\": null,\"FieldName\": \"[[MyScalar]]\",\"FieldValue\": \"some value\",\"IndexNumber\": 0,\"Inserted\": false,\"IsFieldNameFocused\": false,\"IsFieldValueFocused\": false,\"Errors\": {},\"OutList\": [],\"HasError\": false,\"Error\": \"\"},{\"ErrorMessage\": null,\"Path\": null,\"WatermarkTextValue\": null,\"WatermarkTextVariable\": null,\"FieldName\": \"[[MyRecordSet().Field1]]\",\"FieldValue\": \"another value\",\"IndexNumber\": 1,\"Inserted\": false,\"IsFieldNameFocused\": false,\"IsFieldValueFocused\": false,\"Errors\": {},\"OutList\": [],\"HasError\": false,\"Error\": \"\"},{\"ErrorMessage\": null,\"Path\": null,\"WatermarkTextValue\": null,\"WatermarkTextVariable\": null,\"FieldName\": \"\",\"FieldValue\": \"\",\"IndexNumber\": 2,\"Inserted\": false,\"IsFieldNameFocused\": false,\"IsFieldValueFocused\": false,\"Errors\": {},\"OutList\": [],\"HasError\": false,\"Error\": \"\"}],\"type\": \"Unlimited.Applications.BusinessDesignStudio.Activities.DsfDotNetMultiAssignActivity, Dev2.Activities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\",    \"displayname\": \"Assign\",\"properties\": {\"displayname\": \"Assign\",\"id\": null,\"Add\": \"False\",\"CreateBookmark\": \"False\",\"DatabindRecursive\": \"False\",\"DisplayName\": \"Assign\",\"IsEndedOnError\": \"False\",\"IsService\": \"False\",\"IsSimulationEnabled\": \"False\",\"IsUIStep\": \"False\",\"IsWorkflow\": \"False\",\"OnResumeClearAmbientDataList\": \"False\",\"OnResumeClearTags\": \"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\",\"SimulationMode\": \"OnDemand\",\"UniqueID\": \"b71ffa82-274c-4e09-af8c-c719f56135ca\",\"UpdateAllOccurrences\": \"False\"}},\"source\": null,\"target\": null,\"label\": \"Assign\"\r\n}");
			promptBuilder.AppendLine("In the examples there are other properties that define a Warewolf tool. They can all be left the same as in the example exept for the key property, the display name to display on the X6 node on the graph, the x and y coordinates to position the X6 node on the X6 graph and the id to distinguish it from other X6 nodes on the X6 graph.");
            promptBuilder.AppendLine("The HTTP GET tool's purpose is to make HTTP GET requests and it's key properties are: headers, outputs, sourceId, querystring, objectResult, isBase64, isOutputToObject and objectname");
			promptBuilder.AppendLine("Property definitions: headers - dictionary of HTTP headers to send; outputs - mapping specifying which parts of the response to save to datalist variables; sourceId - optional reference to a configured HTTP source/connection; querystring - key/value pairs appended to the URL; objectResult - when true, parse the response as a JSON object for structured output; isBase64 - when true, treat the response as Base64 encoded and decode it before storing; isOutputToObject - when true, place the parsed response into an object variable; objectname - the name of the object variable to store the response into.");
            promptBuilder.AppendLine("Here is an example of a complete HTTP GET tool json: {\"position\": {\"x\": 100,\"y\": 400},\"size\": null,\"visible\": null,\"shape\": \"WebGetActivity\",\"id\": \"354afc9c-0d79-46d1-bffe-6efc0264b0c0\",\"data\": {\"onerrordata\": {\"errorMessage\": \"[[ErrorsVariable]]\",\"webServiceUrl\": \"\",\"endWorkflow\": false},\"type\": \"webgetactivity\",\"displayname\": \"HTTP GET Web Method\",\"UniqueID\": \"c1f84e2c-4475-4a4b-96b8-cbaa914f46b5\",\"headers\": [{\"Name\": \"Content-Type\",\"Value\": \"application/json\"},{\"Name\": \"My-Custom-Header\",\"Value\": \"some sort of value\"},{\"Name\": \"\",\"Value\": \"\"}],\"querystring\": \"\",\"sourceId\": \"f39374fe-ce60-5217-701b-dfd24b08879a\",\"outputdescription\": {\"Format\": 1,\"DataSourceShapes\": [{\"Paths\": [{\"ActualPath\": \"method\",\"DisplayPath\": \"method\",\"OutputExpression\": \"\",\"SampleData\": \"GET\"},{\"ActualPath\": \"protocol\",\"DisplayPath\": \"protocol\",\"OutputExpression\": \"\",\"SampleData\": \"https\"},{\"ActualPath\": \"host\",\"DisplayPath\": \"host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},{\"ActualPath\": \"path\",\"DisplayPath\": \"path\",\"OutputExpression\": \"\",\"SampleData\": \"/\"},{\"ActualPath\": \"ip\",\"DisplayPath\": \"ip\",\"OutputExpression\": \"\",\"SampleData\": \"80.41.233.185:53919\"},{\"ActualPath\": \"rawBody\",\"DisplayPath\": \"rawBody\",\"OutputExpression\": \"\",\"SampleData\": \"[object Object]\"},{\"ActualPath\": \"warnings()\",\"DisplayPath\": \"warnings()\",\"OutputExpression\": \"\",\"SampleData\": \"Error in parsing JSON body. Please check the syntax. [\\\"[object Object]\\\" is not valid JSON]\"},{\"ActualPath\": \"headers.Host\",\"DisplayPath\": \"headers.Host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},{\"ActualPath\": \"headers.User-Agent\",\"DisplayPath\": \"headers.User-Agent\",\"OutputExpression\": \"\",\"SampleData\": \"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)\"},{\"ActualPath\": \"headers.Content-Type\",\"DisplayPath\": \"headers.Content-Type\",\"OutputExpression\": \"\",\"SampleData\": \"application/json\"},{\"ActualPath\": \"headers.My-Custom-Header\",\"DisplayPath\": \"headers.My-Custom-Header\",\"OutputExpression\": \"\",\"SampleData\": \"some sort of value\"},{\"ActualPath\": \"headers.Traceparent\",\"DisplayPath\": \"headers.Traceparent\",\"OutputExpression\": \"\",\"SampleData\": \"00-da55ac2dbf5e45bcd60ea033308f89b0-21f79a09094e5dac-00\"},{\"ActualPath\": \"headers.Via\",\"DisplayPath\": \"headers.Via\",\"OutputExpression\": \"\",\"SampleData\": \"1.1 Caddy\"},{\"ActualPath\": \"headers.Accept-Encoding\",\"DisplayPath\": \"headers.Accept-Encoding\",\"OutputExpression\": \"\",\"SampleData\": \"gzip\"}]}]},\"inputs\": null,\"outputs\": [{\"Path\": {\"ActualPath\": \"method\",\"DisplayPath\": \"method\",\"OutputExpression\": \"\",\"SampleData\": \"GET\"},\"MappedFrom\": \"method\",\"MappedTo\": \"[[warnings().method]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"protocol\",\"DisplayPath\": \"protocol\",\"OutputExpression\": \"\",\"SampleData\": \"https\"},\"MappedFrom\": \"protocol\",\"MappedTo\": \"[[warnings().protocol]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"host\",\"DisplayPath\": \"host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},\"MappedFrom\": \"host\",\"MappedTo\": \"[[warnings().host]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"path\",\"DisplayPath\": \"path\",\"OutputExpression\": \"\",\"SampleData\": \"/\"},\"MappedFrom\": \"path\",\"MappedTo\": \"[[warnings().path]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"ip\",\"DisplayPath\": \"ip\",\"OutputExpression\": \"\",\"SampleData\": \"80.41.233.185:53919\"},\"MappedFrom\": \"ip\",\"MappedTo\": \"[[warnings().ip]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"rawBody\",\"DisplayPath\": \"rawBody\",\"OutputExpression\": \"\",\"SampleData\": \"[object Object]\"},\"MappedFrom\": \"rawBody\",\"MappedTo\": \"[[warnings().rawBody]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Host\",\"DisplayPath\": \"headers.Host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},\"MappedFrom\": \"headersHost\",\"MappedTo\": \"[[warnings().headersHost]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.User-Agent\",\"DisplayPath\": \"headers.User-Agent\",\"OutputExpression\": \"\",\"SampleData\": \"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)\"},\"MappedFrom\": \"headersUser-Agent\",\"MappedTo\": \"[[warnings().headersUser-Agent]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Content-Type\",\"DisplayPath\": \"headers.Content-Type\",\"OutputExpression\": \"\",\"SampleData\": \"application/json\"},\"MappedFrom\": \"headersContent-Type\",\"MappedTo\": \"[[warnings().headersContent-Type]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.My-Custom-Header\",\"DisplayPath\": \"headers.My-Custom-Header\",\"OutputExpression\": \"\",\"SampleData\": \"some sort of value\"},\"MappedFrom\": \"headersMy-Custom-Header\",\"MappedTo\": \"[[warnings().headersMy-Custom-Header]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Traceparent\",\"DisplayPath\": \"headers.Traceparent\",\"OutputExpression\": \"\",\"SampleData\": \"00-da55ac2dbf5e45bcd60ea033308f89b0-21f79a09094e5dac-00\"},\"MappedFrom\": \"headersTraceparent\",\"MappedTo\": \"[[warnings().headersTraceparent]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Via\",\"DisplayPath\": \"headers.Via\",\"OutputExpression\": \"\",\"SampleData\": \"1.1 Caddy\"},\"MappedFrom\": \"headersVia\",\"MappedTo\": \"[[warnings().headersVia]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Accept-Encoding\",\"DisplayPath\": \"headers.Accept-Encoding\",\"OutputExpression\": \"\",\"SampleData\": \"gzip\"},\"MappedFrom\": \"headersAccept-Encoding\",\"MappedTo\": \"[[warnings().headersAccept-Encoding]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"warnings()\",\"DisplayPath\": \"warnings()\",\"OutputExpression\": \"\",\"SampleData\": \"Error in parsing JSON body. Please check the syntax. [\\\"[object Object]\\\" is not valid JSON]\"},\"MappedFrom\": \"warnings()\",\"MappedTo\": \"\",\"RecordSetName\": \"warnings\"}],\"isOutputToObject\": false,\"objectname\": null,\"objectresult\": \"{\\n  \\\"method\\\": \\\"GET\\\",\\n  \\\"protocol\\\": \\\"https\\\",\\n  \\\"host\\\": \\\"echo.free.beeceptor.com\\\",\\n  \\\"path\\\": \\\"/\\\",\\n  \\\"ip\\\": \\\"80.41.233.185:53919\\\",\\n  \\\"headers\\\": {\\n    \\\"Host\\\": \\\"echo.free.beeceptor.com\\\",\\n    \\\"User-Agent\\\": \\\"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)\\\",\\n    \\\"Content-Type\\\": \\\"application/json\\\",\\n    \\\"My-Custom-Header\\\": \\\"some sort of value\\\",\\n    \\\"Traceparent\\\": \\\"00-da55ac2dbf5e45bcd60ea033308f89b0-21f79a09094e5dac-00\\\",\\n    \\\"Via\\\": \\\"1.1 Caddy\\\",\\n    \\\"Accept-Encoding\\\": \\\"gzip\\\"\\n  },\\n  \\\"parsedQueryParams\\\": {},\\n  \\\"rawBody\\\": \\\"[object Object]\\\",\\n  \\\"warnings\\\": [\\n    \\\"Error in parsing JSON body. Please check the syntax. [\\\\\\\"[object Object]\\\\\\\" is not valid JSON]\\\"\\n  ]\\n}\",\"isresponsebase64\": false,\"properties\": {\"displayname\": \"HTTP GET Web Method\",\"id\": null,\"OnErrorVariable\": \"[[ErrorsVariable]]\",\"OnErrorWorkflow\": \"\",\"Add\": \"False\",\"DatabindRecursive\": \"False\",\"DeferExecution\": \"False\",\"DisplayName\": \"HTTP GET Web Method\",\"IsEndedOnError\": \"False\",\"IsObject\": \"False\",\"IsResponseBase64\": \"False\",\"IsService\": \"False\",\"IsSimulationEnabled\": \"False\",\"IsUIStep\": \"False\",\"IsWorkflow\": \"False\",\"ObjectResult\": \"{\\n  \\\"method\\\": \\\"GET\\\",\\n  \\\"protocol\\\": \\\"https\\\",\\n  \\\"host\\\": \\\"echo.free.beeceptor.com\\\",\\n  \\\"path\\\": \\\"/\\\",\\n  \\\"ip\\\": \\\"80.41.233.185:53919\\\",\\n  \\\"headers\\\": {\\n    \\\"Host\\\": \\\"echo.free.beeceptor.com\\\",\\n    \\\"User-Agent\\\": \\\"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)\\\",\\n    \\\"Content-Type\\\": \\\"application/json\\\",\\n    \\\"My-Custom-Header\\\": \\\"some sort of value\\\",\\n    \\\"Traceparent\\\": \\\"00-da55ac2dbf5e45bcd60ea033308f89b0-21f79a09094e5dac-00\\\",\\n    \\\"Via\\\": \\\"1.1 Caddy\\\",\\n    \\\"Accept-Encoding\\\": \\\"gzip\\\"\\n  },\\n  \\\"parsedQueryParams\\\": {},\\n  \\\"rawBody\\\": \\\"[object Object]\\\",\\n  \\\"warnings\\\": [\\n    \\\"Error in parsing JSON body. Please check the syntax. [\\\\\\\"[object Object]\\\\\\\" is not valid JSON]\\\"\\n  ]\\n}\",\"OnResumeClearAmbientDataList\": \"False\",\"OnResumeClearTags\": \"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\",\"QueryString\": \"\",\"RemoveInputFromOutput\": \"False\",\"RunWorkflowAsync\": \"False\",\"SimulationMode\": \"OnDemand\",\"UniqueID\": \"c1f84e2c-4475-4a4b-96b8-cbaa914f46b5\"}},\"source\": null,\"target\": null,\"label\": \"HTTP GET Web Method\"}");
			promptBuilder.AppendLine("The HTTP POST tool's purpose is to make HTTP POST requests and it's key properties are: headers, outputs, sourceId, sourceName, requestUrl, queryString, postData, postDataType, contentType, customContentType, formData, isFormDataChecked, isUrlEncodedChecked, isManualChecked, response, objectResult, isBase64, isOutputToObject and objectname");
			promptBuilder.AppendLine("Property definitions: headers - dictionary of HTTP headers to send; outputs - mapping specifying which parts of the response to save to datalist variables; sourceId - optional reference to a configured HTTP source/connection; sourceName - optional human-readable name of the source; requestUrl - the target URL for the POST request; queryString - key/value pairs appended to the URL; postData - the payload to send in the request body (string or structured data); postDataType - indicates how to interpret postData (e.g., JSON, FormData, Raw); contentType - MIME type of the request body (e.g., 'application/json'); customContentType - override for contentType when using non-standard types; formData - key/value pairs used when sending multipart/form-data; isFormDataChecked - when true, treat postData as form data; isUrlEncodedChecked - when true, encode form fields as application/x-www-form-urlencoded; isManualChecked - when true, send the postData exactly as provided without additional encoding; response - optional variable to capture the raw response body; objectResult - when true, parse the response as a JSON object for structured output; isBase64 - when true, treat the response as Base64 encoded and decode it before storing; isOutputToObject - when true, place the parsed response into an object variable; objectname - the name of the object variable to store the response into.");
            promptBuilder.AppendLine("Here is an example of a complete HTTP GET tool json: {\"position\": {\"x\": 100,\"y\": 400},\"size\": null,\"visible\": null,\"shape\": \"WebPostActivityNew\",\"id\": \"0b7ba44e-b9c6-42cc-b40a-0f2386027ce8\",\"data\": {\"onerrordata\": {\"errorMessage\": \"[[err]]\",\"webServiceUrl\": \"\",\"endWorkflow\": false},\"type\": \"webpostactivitynew\",\"displayname\": \"HTTP POST Web Method\",\"UniqueID\": \"\",\"headers\": [{\"Name\": \"Content-Type\",\"Value\": \"application/json\"},{\"Name\": \"My-Custom-Header\",\"Value\": \"some Warewolf custom data\"},{\"Name\": \"\",\"Value\": \"\"}],\"querystring\": \"\",\"settings\": [{\"Name\": \"IsManualChecked\",\"Value\": \"True\"},{\"Name\": \"IsFormDataChecked\",\"Value\": \"false\"},{\"Name\": \"IsUrlEncodedChecked\",\"Value\": \"false\"}],\"conditions\": [],\"timeout\": 600,\"postdata\": \"some body\",\"sourceId\": \"f39374fe-ce60-5217-701b-dfd24b08879a\",\"outputdescription\": {\"Format\": 1,\"DataSourceShapes\": [{\"Paths\": [{\"ActualPath\": \"method\",\"DisplayPath\": \"method\",\"OutputExpression\": \"\",\"SampleData\": \"POST\"},{\"ActualPath\": \"protocol\",\"DisplayPath\": \"protocol\",\"OutputExpression\": \"\",\"SampleData\": \"https\"},{\"ActualPath\": \"host\",\"DisplayPath\": \"host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},{\"ActualPath\": \"path\",\"DisplayPath\": \"path\",\"OutputExpression\": \"\",\"SampleData\": \"/\"},{\"ActualPath\": \"ip\",\"DisplayPath\": \"ip\",\"OutputExpression\": \"\",\"SampleData\": \"80.41.233.185:61591\"},{\"ActualPath\": \"rawBody\",\"DisplayPath\": \"rawBody\",\"OutputExpression\": \"\",\"SampleData\": \"some body\"},{\"ActualPath\": \"warnings()\",\"DisplayPath\": \"warnings()\",\"OutputExpression\": \"\",\"SampleData\": \"Error in parsing JSON body. Please check the syntax. [Unexpected token 's'__COMMA__ \\\"some body\\\" is not valid JSON]\"},{\"ActualPath\": \"headers.Host\",\"DisplayPath\": \"headers.Host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},{\"ActualPath\": \"headers.Content-Length\",\"DisplayPath\": \"headers.Content-Length\",\"OutputExpression\": \"\",\"SampleData\": \"9\"},{\"ActualPath\": \"headers.Authorization\",\"DisplayPath\": \"headers.Authorization\",\"OutputExpression\": \"\",\"SampleData\": \"\"},{\"ActualPath\": \"headers.Content-Type\",\"DisplayPath\": \"headers.Content-Type\",\"OutputExpression\": \"\",\"SampleData\": \"application/json\"},{\"ActualPath\": \"headers.My-Custom-Header\",\"DisplayPath\": \"headers.My-Custom-Header\",\"OutputExpression\": \"\",\"SampleData\": \"some Warewolf custom data\"},{\"ActualPath\": \"headers.Traceparent\",\"DisplayPath\": \"headers.Traceparent\",\"OutputExpression\": \"\",\"SampleData\": \"00-a181bb15026a5195a64e7c4ee7e520c1-bf725aaeecfb629e-00\"},{\"ActualPath\": \"headers.Via\",\"DisplayPath\": \"headers.Via\",\"OutputExpression\": \"\",\"SampleData\": \"1.1 Caddy\"},{\"ActualPath\": \"headers.Accept-Encoding\",\"DisplayPath\": \"headers.Accept-Encoding\",\"OutputExpression\": \"\",\"SampleData\": \"gzip\"}]}]},\"inputs\": [],\"outputs\": [{\"Path\": {\"ActualPath\": \"method\",\"DisplayPath\": \"method\",\"OutputExpression\": \"\",\"SampleData\": \"POST\"},\"MappedFrom\": \"method\",\"MappedTo\": \"[[warnings().method]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"protocol\",\"DisplayPath\": \"protocol\",\"OutputExpression\": \"\",\"SampleData\": \"https\"},\"MappedFrom\": \"protocol\",\"MappedTo\": \"[[warnings().protocol]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"host\",\"DisplayPath\": \"host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},\"MappedFrom\": \"host\",\"MappedTo\": \"[[warnings().host]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"path\",\"DisplayPath\": \"path\",\"OutputExpression\": \"\",\"SampleData\": \"/\"},\"MappedFrom\": \"path\",\"MappedTo\": \"[[warnings().path]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"ip\",\"DisplayPath\": \"ip\",\"OutputExpression\": \"\",\"SampleData\": \"80.41.233.185:61591\"},\"MappedFrom\": \"ip\",\"MappedTo\": \"[[warnings().ip]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"rawBody\",\"DisplayPath\": \"rawBody\",\"OutputExpression\": \"\",\"SampleData\": \"some body\"},\"MappedFrom\": \"rawBody\",\"MappedTo\": \"[[warnings().rawBody]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Host\",\"DisplayPath\": \"headers.Host\",\"OutputExpression\": \"\",\"SampleData\": \"echo.free.beeceptor.com\"},\"MappedFrom\": \"headersHost\",\"MappedTo\": \"[[warnings().headersHost]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Content-Length\",\"DisplayPath\": \"headers.Content-Length\",\"OutputExpression\": \"\",\"SampleData\": \"9\"},\"MappedFrom\": \"headersContent-Length\",\"MappedTo\": \"[[warnings().headersContent-Length]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Authorization\",\"DisplayPath\": \"headers.Authorization\",\"OutputExpression\": \"\",\"SampleData\": \"\"},\"MappedFrom\": \"headersAuthorization\",\"MappedTo\": \"[[warnings().headersAuthorization]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Content-Type\",\"DisplayPath\": \"headers.Content-Type\",\"OutputExpression\": \"\",\"SampleData\": \"application/json\"},\"MappedFrom\": \"headersContent-Type\",\"MappedTo\": \"[[warnings().headersContent-Type]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.My-Custom-Header\",\"DisplayPath\": \"headers.My-Custom-Header\",\"OutputExpression\": \"\",\"SampleData\": \"some Warewolf custom data\"},\"MappedFrom\": \"headersMy-Custom-Header\",\"MappedTo\": \"[[warnings().headersMy-Custom-Header]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Traceparent\",\"DisplayPath\": \"headers.Traceparent\",\"OutputExpression\": \"\",\"SampleData\": \"00-a181bb15026a5195a64e7c4ee7e520c1-bf725aaeecfb629e-00\"},\"MappedFrom\": \"headersTraceparent\",\"MappedTo\": \"[[warnings().headersTraceparent]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Via\",\"DisplayPath\": \"headers.Via\",\"OutputExpression\": \"\",\"SampleData\": \"1.1 Caddy\"},\"MappedFrom\": \"headersVia\",\"MappedTo\": \"[[warnings().headersVia]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"headers.Accept-Encoding\",\"DisplayPath\": \"headers.Accept-Encoding\",\"OutputExpression\": \"\",\"SampleData\": \"gzip\"},\"MappedFrom\": \"headersAccept-Encoding\",\"MappedTo\": \"[[warnings().headersAccept-Encoding]]\",\"RecordSetName\": \"warnings\"},{\"Path\": {\"ActualPath\": \"warnings()\",\"DisplayPath\": \"warnings()\",\"OutputExpression\": \"\",\"SampleData\": \"Error in parsing JSON body. Please check the syntax. [Unexpected token 's'__COMMA__ \\\"some body\\\" is not valid JSON]\"},\"MappedFrom\": \"warnings()\",\"MappedTo\": \"\",\"RecordSetName\": \"warnings\"}],\"isOutputToObject\": false,\"objectname\": \"\",\"objectresult\": \"{\\n  \\\"method\\\": \\\"POST\\\",\\n  \\\"protocol\\\": \\\"https\\\",\\n  \\\"host\\\": \\\"echo.free.beeceptor.com\\\",\\n  \\\"path\\\": \\\"/\\\",\\n  \\\"ip\\\": \\\"80.41.233.185:61591\\\",\\n  \\\"headers\\\": {\\n    \\\"Host\\\": \\\"echo.free.beeceptor.com\\\",\\n    \\\"Content-Length\\\": \\\"9\\\",\\n    \\\"Authorization\\\": \\\"\\\",\\n    \\\"Content-Type\\\": \\\"application/json\\\",\\n    \\\"My-Custom-Header\\\": \\\"some Warewolf custom data\\\",\\n    \\\"Traceparent\\\": \\\"00-a181bb15026a5195a64e7c4ee7e520c1-bf725aaeecfb629e-00\\\",\\n    \\\"Via\\\": \\\"1.1 Caddy\\\",\\n    \\\"Accept-Encoding\\\": \\\"gzip\\\"\\n  },\\n  \\\"parsedQueryParams\\\": {},\\n  \\\"rawBody\\\": \\\"some body\\\",\\n  \\\"warnings\\\": [\\n    \\\"Error in parsing JSON body. Please check the syntax. [Unexpected token 's', \\\\\\\"some body\\\\\\\" is not valid JSON]\\\"\\n  ]\\n}\",\"properties\": {\"displayname\": \"HTTP POST Web Method\",\"id\": null,\"ObjectName\": \"\",\"OnErrorVariable\": \"[[err]]\",\"OnErrorWorkflow\": \"\",\"Add\": \"False\",\"DatabindRecursive\": \"False\",\"DeferExecution\": \"False\",\"DisplayName\": \"HTTP POST Web Method\",\"IsEndedOnError\": \"False\",\"IsObject\": \"False\",\"IsService\": \"False\",\"IsSimulationEnabled\": \"False\",\"IsUIStep\": \"False\",\"IsWorkflow\": \"False\",\"ObjectResult\": \"{\\n  \\\"method\\\": \\\"POST\\\",\\n  \\\"protocol\\\": \\\"https\\\",\\n  \\\"host\\\": \\\"echo.free.beeceptor.com\\\",\\n  \\\"path\\\": \\\"/\\\",\\n  \\\"ip\\\": \\\"80.41.233.185:61591\\\",\\n  \\\"headers\\\": {\\n    \\\"Host\\\": \\\"echo.free.beeceptor.com\\\",\\n    \\\"Content-Length\\\": \\\"9\\\",\\n    \\\"Authorization\\\": \\\"\\\",\\n    \\\"Content-Type\\\": \\\"application/json\\\",\\n    \\\"My-Custom-Header\\\": \\\"some Warewolf custom data\\\",\\n    \\\"Traceparent\\\": \\\"00-a181bb15026a5195a64e7c4ee7e520c1-bf725aaeecfb629e-00\\\",\\n    \\\"Via\\\": \\\"1.1 Caddy\\\",\\n    \\\"Accept-Encoding\\\": \\\"gzip\\\"\\n  },\\n  \\\"parsedQueryParams\\\": {},\\n  \\\"rawBody\\\": \\\"some body\\\",\\n  \\\"warnings\\\": [\\n    \\\"Error in parsing JSON body. Please check the syntax. [Unexpected token 's', \\\\\\\"some body\\\\\\\" is not valid JSON]\\\"\\n  ]\\n}\",\"OnResumeClearAmbientDataList\": \"False\",\"OnResumeClearTags\": \"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\",\"PostData\": \"some body\",\"QueryString\": \"\",\"RemoveInputFromOutput\": \"False\",\"RunWorkflowAsync\": \"False\",\"SimulationMode\": \"OnDemand\",\"UniqueID\": \"\",\"Timeout\": \"600\"}},\"source\": null,\"target\": null,\"label\": \"HTTP POST Web Method\"}");
			promptBuilder.AppendLine("The SQL Server Database Connector tool's purpose is to execute stored procedure on an SQL server and it's key properties are: procedurename, executeactionstring, serviceserver, sourceId, commandtimeout, isOutputToObject, objectname, objectresult, inputs, outputs");
			promptBuilder.AppendLine("Property definitions: procedurename - the name of the stored procedure to execute; executeactionstring - the execution action to perform (e.g., 'ExecuteReader', 'ExecuteNonQuery'); serviceserver - reference or identifier of the configured SQL server/source or connection details; sourceId - optional reference to a saved database source configuration; commandtimeout - timeout in seconds for the command execution; inputs - mapping of stored procedure input parameter names to values or datalist variables; outputs - mapping of stored procedure output parameter names to datalist variables; objectresult - when true, parse result sets into structured objects; isOutputToObject - when true, place parsed results into an object variable; objectname - the name of the object variable to store the results into.");
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
                        if (XamlToX6Json != null)
						{
							var result = ResourceCatalog.Instance.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
							var serviceXaml = new StringBuilder(result.ToString());
                            var Cleaner = new ResourceDefinationCleaner();
							var finalresult = (ExecuteMessage)Cleaner.GetRawResourceDefinition(false, resourceId, result);
							if (serviceXaml != null && serviceXaml.Length > 0)
                            {
								var workflowXaml = new Dev2.Runtime.ServiceModel.Data.Workflow(serviceXaml.ToXElement(), true);
					            var info = new X6RequestInfo { ResourceName = workflowXaml.ResourceName, ActivityXaml = finalresult.Message.ToString(), WorkflowXML = workflowXaml.ToServiceDefinition().ToString() };
					            var x6Json = XamlToX6Json(info);
                                var deserializedObject = JsonSerializer.Deserialize<X6WorkflowLoadModel>(x6Json);
                                deserializedObject.WorkflowXml = null;
								x6Json = JsonSerializer.Serialize(deserializedObject, new JsonSerializerOptions());

								if (!string.IsNullOrEmpty(x6Json))
                                {
                                    definitions.Add($"Resource: {sanitizedName} (Type: {sanitizedType}, Path: {resourcePath}, ID: {resourceId}, JSON: ```json\n{x6Json}\n```)");
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
