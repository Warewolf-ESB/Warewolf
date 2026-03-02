using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Runtime.ESB.Management.Services
{
    public static class ChatbotToolNormalizer
    {
        private static readonly Dictionary<string, string> ShapeToDataType = new Dictionary<string, string>
        {
            { "HttpPostWebMethodTool", "webpostactivitynew" },
            { "HttpGetWebMethodTool", "webgetactivity" },
            { "WebGetActivity", "webgetactivity" },
            { "WebPostActivityNew", "webpostactivitynew" },
            { "WebPostWebMethod", "webpostactivitynew" },
            { "HttpPostWebMethod", "webpostactivitynew" },
            { "SqlServerDatabaseActivity", "dsfsqlserverdatabaseactivity" },
            { "HttpWebPutTool", "webputactivity" },
            { "HttpDeleteWebMethodTool", "dsfwebdeleteactivity" },
            { "DsfSqlServerDatabaseActivity", "dsfsqlserverdatabaseactivity" },
            { "DsfDotNetMultiAssignActivity", "dsfdotnetmultiassignactivity" },
            { "DsfMySqlDatabaseActivity", "dsfmysqldatabaseactivity" },
            { "PostgreSqlActivity", "postgresqlactivity" }
        };

        private static readonly Dictionary<string, string> ShapeToDisplayName = new Dictionary<string, string>
        {
            { "HttpPostWebMethodTool", "HTTP POST Web Method" },
            { "HttpGetWebMethodTool", "HTTP GET Web Method" },
            { "WebGetActivity", "HTTP GET Web Method" },
            { "WebPostActivityNew", "HTTP POST" },
            { "WebPostWebMethod", "HTTP POST" },
            { "HttpPostWebMethod", "HTTP POST" },
            { "SqlServerDatabaseActivity", "SQL Server Database" },
            { "HttpWebPutTool", "HTTP PUT Web Method" },
            { "HttpDeleteWebMethodTool", "HTTP DELETE Web Method" },
            { "DsfSqlServerDatabaseActivity", "SQL Server Database" },
            { "DsfDotNetMultiAssignActivity", "Assign" },
            { "DsfMySqlDatabaseActivity", "MySQL Database" },
            { "PostgreSqlActivity", "PostgreSQL" }
        };

        public static string NormalizeResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return response;

            // Handle json code blocks
            var regex = new Regex(@"```json\s*([\s\S]*?)\s*```");
            var result = regex.Replace(response, match =>
            {
                var jsonStr = match.Groups[1].Value;
                try
                {
                    // Try array of objects
                    if (jsonStr.TrimStart().StartsWith("["))
                    {
                        var jArray = JArray.Parse(jsonStr);
                        foreach (var token in jArray)
                        {
                            if (token is JObject obj)
                            {
                                NormalizeToolPayload(obj);
                            }
                        }
                        return $@"```json
{jArray.ToString(Formatting.Indented)}
```";
                    }
                    else
                    {
                        var jObj = JObject.Parse(jsonStr);
                        NormalizeToolPayload(jObj);
                        return $@"```json
{jObj.ToString(Formatting.Indented)}
```";
                    }
                }
                catch
                {
                    return match.Value;
                }
            });

            // Handle NDJSON lines
            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var modifiedLines = new List<string>();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("{") && trimmed.EndsWith("}") && !result.Contains("```json"))
                {
                    try
                    {
                        var jObj = JObject.Parse(trimmed);
                        NormalizeToolPayload(jObj);
                        modifiedLines.Add(jObj.ToString(Formatting.None));
                        continue;
                    }
                    catch
                    {
                        // ignore
                    }
                }
                modifiedLines.Add(line);
            }

            return string.Join(Environment.NewLine, modifiedLines);
        }

        private static void NormalizeToolPayload(JObject payload)
        {
            if (payload == null) return;
            
            // if payload contains a "tool" wrapper, normalize the inner object instead
            if (payload["tool"] is JObject toolObj)
            {
                payload = toolObj;
            }

            if (payload["data"] == null || payload["data"].Type == JTokenType.Null)
            {
                payload["data"] = new JObject();
            }

            var data = payload["data"] as JObject;
            if (data == null) return;

            // ID regeneration
            var originalId = payload["id"]?.ToString();
            if (!string.IsNullOrEmpty(originalId))
            {
                payload[nameof(originalId)] = originalId;
                data[nameof(originalId)] = originalId;
            }

            var freshId = Guid.NewGuid().ToString();
            payload["id"] = freshId;
            data["UniqueID"] = freshId;

            if (data["properties"] is JObject props)
            {
                props["UniqueID"] = freshId;
            }

            // Derive data.type from shape
            var shape = payload["shape"]?.ToString();
            var dataType = data["type"]?.ToString();

            if (!string.IsNullOrEmpty(shape) && string.IsNullOrEmpty(dataType) && ShapeToDataType.TryGetValue(shape, out var derivedType))
            {
                data["type"] = derivedType;
                dataType = derivedType;
            }

            if (!string.IsNullOrEmpty(dataType))
            {
                data["type"] = dataType.Trim().Trim('"', '\'');
            }

            if (shape == "DsfSqlServerDatabaseActivity" && string.IsNullOrEmpty(data["type"]?.ToString()))
            {
                data["type"] = "dsfsqlserverdatabaseactivity";
            }

            // Derive data.displayname
            var displayName = data["displayname"]?.ToString();
            var label = payload["label"]?.ToString();

            if (string.IsNullOrEmpty(displayName))
            {
                if (!string.IsNullOrEmpty(label))
                {
                    data["displayname"] = label;
                }
                else if (!string.IsNullOrEmpty(shape) && ShapeToDisplayName.TryGetValue(shape, out var derivedName))
                {
                    data["displayname"] = derivedName;
                }
            }

            // Normalize Outputs Path
            if (data["outputs"] is JArray outputsArray)
            {
                foreach (var outputItem in outputsArray)
                {
                    if (outputItem is JObject outputObj && outputObj["Path"] is JObject)
                    {
                        outputObj["Path"] = null;
                    }
                }
            }

            // HTTP POST
            if (shape == "HttpPostWebMethodTool")
            {
                var reqUrl = data["requestUrl"]?.ToString();
                var qs = data["querystring"]?.ToString();
                
                if (!string.IsNullOrEmpty(reqUrl) && string.IsNullOrEmpty(qs))
                {
                    data["querystring"] = reqUrl;
                }
                
                var topRequestUrl = payload["requestUrl"]?.ToString();
                if (!string.IsNullOrEmpty(topRequestUrl) && string.IsNullOrEmpty(data["querystring"]?.ToString()))
                {
                    data["querystring"] = topRequestUrl;
                }
                
                var topUrl = payload["url"]?.ToString();
                if (!string.IsNullOrEmpty(topUrl) && string.IsNullOrEmpty(data["querystring"]?.ToString()))
                {
                    data["querystring"] = topUrl;
                }

                var postdata = data["postdata"]?.ToString() ?? "";
                if (postdata.Contains("[["))
                {
                    if (data["settings"] == null || data["settings"].Type != JTokenType.Array)
                    {
                        data["settings"] = new JArray();
                    }
                    var settings = data["settings"] as JArray;
                    bool alreadySet = false;
                    foreach (var s in settings)
                    {
                        if (s is JObject sObj && sObj["Name"]?.ToString() == "IsManualChecked")
                        {
                            alreadySet = true;
                            break;
                        }
                    }
                    if (!alreadySet)
                    {
                        var newSetting = new JObject();
                        newSetting["Name"] = "IsManualChecked";
                        newSetting["Value"] = "True";
                        settings.Add(newSetting);
                    }
                }
            }

            // HTTP GET
            if (shape == "HttpGetWebMethodTool" || shape == "WebGetActivity")
            {
                var reqUrl = data["requestUrl"]?.ToString();
                if (!string.IsNullOrEmpty(reqUrl) && string.IsNullOrEmpty(data["querystring"]?.ToString()))
                {
                    data["querystring"] = reqUrl;
                }
                
                var topRequestUrl = payload["requestUrl"]?.ToString();
                if (!string.IsNullOrEmpty(topRequestUrl) && string.IsNullOrEmpty(data["querystring"]?.ToString()))
                {
                    data["querystring"] = topRequestUrl;
                }
                
                var topUrl = payload["url"]?.ToString();
                if (!string.IsNullOrEmpty(topUrl) && string.IsNullOrEmpty(data["querystring"]?.ToString()))
                {
                    data["querystring"] = topUrl;
                }
            }

            // SQL Server
            if (shape == "DsfSqlServerDatabaseActivity")
            {
                if (data["properties"] == null || data["properties"].Type != JTokenType.Object)
                {
                    data["properties"] = new JObject();
                }
                var properties = data["properties"] as JObject;

                var isOutputToObjectToken = data["isOutputToObject"];
                if (isOutputToObjectToken != null && isOutputToObjectToken.Type != JTokenType.Null)
                {
                    bool isOutputToObject = false;
                    if (bool.TryParse(isOutputToObjectToken.ToString(), out isOutputToObject))
                    {
                        if (isOutputToObject)
                        {
                            properties["IsObject"] = "True";
                            var objName = data["objectname"]?.ToString();
                            if (string.IsNullOrEmpty(objName))
                            {
                                data["objectname"] = "Result";
                            }
                            properties["ObjectName"] = data["objectname"];
                        }
                        else
                        {
                            properties["IsObject"] = "False";
                        }
                    }
                }

                var procedureName = data["procedurename"]?.ToString();
                if (!string.IsNullOrEmpty(procedureName) && string.IsNullOrEmpty(properties["ProcedureName"]?.ToString()))
                {
                    properties["ProcedureName"] = procedureName;
                }

                var executeActionString = data["executeactionstring"]?.ToString();
                if (!string.IsNullOrEmpty(executeActionString) && string.IsNullOrEmpty(properties["ExecuteActionString"]?.ToString()))
                {
                    properties["ExecuteActionString"] = executeActionString;
                }
            }
        }
    }
}