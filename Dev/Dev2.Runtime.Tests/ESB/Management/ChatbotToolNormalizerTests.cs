/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/
using System;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Dev2.Tests.Runtime.ESB.Management
{
    [TestClass]
    public class ChatbotToolNormalizerTests
    {
        const string Owner = "Coverage";
        const string Cat = nameof(ChatbotToolNormalizer);

        // ---------- NormalizeResponse: trivial / null inputs ----------

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_Null_ReturnsNull()
        {
            Assert.IsNull(ChatbotToolNormalizer.NormalizeResponse(null));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_Empty_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, ChatbotToolNormalizer.NormalizeResponse(string.Empty));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_Whitespace_PassesThrough()
        {
            Assert.AreEqual("   ", ChatbotToolNormalizer.NormalizeResponse("   "));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_PlainText_NoChange()
        {
            const string text = "Just some plain text without any json.";
            var result = ChatbotToolNormalizer.NormalizeResponse(text);
            Assert.AreEqual(text, result);
        }

        // ---------- NormalizeResponse: JSON code blocks ----------

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_JsonCodeBlock_SingleObject_RegeneratesId()
        {
            const string original = "abc-123";
            var input = "```json\n{ \"id\": \"" + original + "\", \"shape\": \"HttpGetWebMethodTool\", \"data\": { \"requestUrl\": \"http://example.com\" } }\n```";

            var result = ChatbotToolNormalizer.NormalizeResponse(input);

            // Extract JSON content from result and parse
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal);
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            Assert.IsTrue(startIdx >= 0 && endIdx > startIdx, "Expected json fences in output");
            var json = result.Substring(startIdx + "```json".Length, endIdx - startIdx - "```json".Length).Trim();
            var obj = JObject.Parse(json);

            Assert.IsNotNull(obj["id"]);
            Assert.AreNotEqual(original, obj["id"].ToString(), "id should be regenerated");
            Assert.AreEqual(original, obj["originalId"].ToString());
            Assert.IsNotNull(obj["data"]);
            Assert.AreEqual(obj["id"].ToString(), obj["data"]["UniqueID"].ToString());
            // displayname should be derived from ShapeToDisplayName for HttpGetWebMethodTool
            Assert.AreEqual("HTTP GET Web Method", obj["data"]["displayname"].ToString());
            // type derived from ShapeToDataType
            Assert.AreEqual("webgetactivity", obj["data"]["type"].ToString());
            // querystring derived from requestUrl
            Assert.AreEqual("http://example.com", obj["data"]["querystring"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_JsonCodeBlock_Array_NormalizesEach()
        {
            const string input = "```json\n[ { \"shape\": \"WebPostActivityNew\", \"data\": {} }, { \"shape\": \"PostgreSqlActivity\", \"data\": {} } ]\n```";

            var result = ChatbotToolNormalizer.NormalizeResponse(input);

            var startIdx = result.IndexOf("```json", StringComparison.Ordinal);
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx + "```json".Length, endIdx - startIdx - "```json".Length).Trim();
            var arr = JArray.Parse(json);

            Assert.AreEqual(2, arr.Count);
            Assert.AreEqual("HTTP POST", arr[0]["data"]["displayname"].ToString());
            Assert.AreEqual("webpostactivitynew", arr[0]["data"]["type"].ToString());
            Assert.AreEqual("PostgreSQL", arr[1]["data"]["displayname"].ToString());
            Assert.AreEqual("postgresqlactivity", arr[1]["data"]["type"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_JsonCodeBlock_Invalid_PreservesOriginal()
        {
            const string input = "```json\n{ not valid json }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            // The invalid JSON inside the code block should be preserved verbatim.
            // NDJSON post-pass may rejoin line endings differently, so just verify the marker remains.
            Assert.IsTrue(result.Contains("{ not valid json }"));
            Assert.IsTrue(result.Contains("```json"));
        }

        // ---------- NormalizeResponse: NDJSON lines ----------

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_NDJsonLines_NormalizesEach()
        {
            var input =
                "{\"shape\":\"HttpDeleteWebMethodTool\",\"data\":{}}\n" +
                "{\"shape\":\"DsfDotNetMultiAssignActivity\",\"data\":{}}";

            var result = ChatbotToolNormalizer.NormalizeResponse(input);

            // Each line is parsed independently
            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, lines.Length);
            var line0 = JObject.Parse(lines[0]);
            var line1 = JObject.Parse(lines[1]);
            Assert.AreEqual("dsfwebdeleteactivity", line0["data"]["type"].ToString());
            Assert.AreEqual("HTTP DELETE Web Method", line0["data"]["displayname"].ToString());
            Assert.AreEqual("dsfdotnetmultiassignactivity", line1["data"]["type"].ToString());
            Assert.AreEqual("Assign", line1["data"]["displayname"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_NDJsonLine_InvalidJson_PreservedLineUnchanged()
        {
            var input = "{ broken }\nplain text line";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            Assert.IsTrue(result.Contains("{ broken }"));
            Assert.IsTrue(result.Contains("plain text line"));
        }

        // ---------- Indirect coverage of NormalizeToolPayload via NormalizeResponse ----------

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_LabelUsedAsDisplayNameWhenNoShapeMapping()
        {
            var input = "```json\n{ \"shape\": \"UnknownShape\", \"label\": \"My Label\", \"data\": {} }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var jsonStart = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var jsonEnd = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(jsonStart, jsonEnd - jsonStart).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("My Label", obj["data"]["displayname"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_ToolWrapper_UnwrapsInnerObject()
        {
            var input = "```json\n{ \"tool\": { \"shape\": \"WebPostWebMethod\", \"data\": {} } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            // The wrapped tool's data should be updated.
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            var tool = (JObject)obj["tool"];
            Assert.IsNotNull(tool);
            Assert.AreEqual("webpostactivitynew", tool["data"]["type"].ToString());
            Assert.AreEqual("HTTP POST", tool["data"]["displayname"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_NullData_BecomesEmptyObject()
        {
            var input = "```json\n{ \"shape\": \"WebGetActivity\", \"data\": null, \"label\": \"L\" }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.IsNotNull(obj["data"]);
            Assert.AreEqual(JTokenType.Object, obj["data"].Type);
            Assert.AreEqual("webgetactivity", obj["data"]["type"].ToString());
            Assert.AreEqual("L", obj["data"]["displayname"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_PropertiesObjectGetsUniqueId()
        {
            var input = "```json\n{ \"shape\": \"DsfSqlServerDatabaseActivity\", \"data\": { \"properties\": {}, \"isOutputToObject\": \"true\", \"procedurename\": \"sp_x\", \"executeactionstring\": \"go\" } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            var data = (JObject)obj["data"];
            var props = (JObject)data["properties"];

            Assert.IsNotNull(props["UniqueID"]);
            Assert.AreEqual("True", props["IsObject"].ToString());
            Assert.AreEqual("Result", data["objectname"].ToString());
            Assert.AreEqual("Result", props["ObjectName"].ToString());
            Assert.AreEqual("sp_x", props["ProcedureName"].ToString());
            Assert.AreEqual("go", props["ExecuteActionString"].ToString());
            Assert.AreEqual("SQL Server Database", data["displayname"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_SqlServer_IsOutputToObjectFalse_KeepsFlag()
        {
            var input = "```json\n{ \"shape\": \"DsfSqlServerDatabaseActivity\", \"data\": { \"isOutputToObject\": \"false\" } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("False", obj["data"]["properties"]["IsObject"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_OutputsArray_NormalizesPathObjectToNull()
        {
            var input = "```json\n{ \"shape\": \"HttpGetWebMethodTool\", \"data\": { \"outputs\": [ { \"Path\": {\"x\":1} }, { \"Path\": \"keep\" } ] } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            var outputs = (JArray)obj["data"]["outputs"];
            Assert.AreEqual(JTokenType.Null, outputs[0]["Path"].Type);
            Assert.AreEqual("keep", outputs[1]["Path"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_HttpPost_QueryStringFromRequestUrl()
        {
            var input = "```json\n{ \"shape\": \"HttpPostWebMethodTool\", \"data\": { \"requestUrl\": \"http://a/b\" } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("http://a/b", obj["data"]["querystring"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_HttpPost_QueryStringFromTopRequestUrl()
        {
            var input = "```json\n{ \"shape\": \"HttpPostWebMethodTool\", \"requestUrl\": \"http://top/req\", \"data\": {} }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("http://top/req", obj["data"]["querystring"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_HttpPost_QueryStringFromTopUrl()
        {
            var input = "```json\n{ \"shape\": \"HttpPostWebMethodTool\", \"url\": \"http://top/url\", \"data\": {} }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("http://top/url", obj["data"]["querystring"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_HttpPost_PostdataWithVariable_AddsIsManualCheckedSetting()
        {
            var input = "```json\n{ \"shape\": \"HttpPostWebMethodTool\", \"data\": { \"postdata\": \"name=[[a]]\" } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            var settings = (JArray)obj["data"]["settings"];
            Assert.IsNotNull(settings);
            var found = false;
            foreach (var s in settings)
            {
                if (s["Name"]?.ToString() == "IsManualChecked" && s["Value"]?.ToString() == "True")
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "IsManualChecked=True setting should be added");
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_HttpPost_PostdataAlreadyHasIsManualChecked_NoDuplicate()
        {
            var input = "```json\n{ \"shape\": \"HttpPostWebMethodTool\", \"data\": { \"postdata\": \"x=[[v]]\", \"settings\": [ { \"Name\": \"IsManualChecked\", \"Value\": \"False\" } ] } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            var settings = (JArray)obj["data"]["settings"];
            var count = 0;
            foreach (var s in settings)
            {
                if (s["Name"]?.ToString() == "IsManualChecked") count++;
            }
            Assert.AreEqual(1, count);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_HttpGet_QueryStringPrecedence()
        {
            // requestUrl on data takes precedence
            var input = "```json\n{ \"shape\": \"WebGetActivity\", \"requestUrl\": \"http://top\", \"url\": \"http://url\", \"data\": { \"requestUrl\": \"http://data\" } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("http://data", obj["data"]["querystring"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_TypeWithQuotes_Trimmed()
        {
            var input = "```json\n{ \"shape\": \"Custom\", \"data\": { \"type\": \"  \\\"customtype\\\"  \" } }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("customtype", obj["data"]["type"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void NormalizeResponse_SqlServerNoType_DefaultsToDsfSqlType()
        {
            var input = "```json\n{ \"shape\": \"DsfSqlServerDatabaseActivity\", \"data\": {} }\n```";
            var result = ChatbotToolNormalizer.NormalizeResponse(input);
            var startIdx = result.IndexOf("```json", StringComparison.Ordinal) + "```json".Length;
            var endIdx = result.LastIndexOf("```", StringComparison.Ordinal);
            var json = result.Substring(startIdx, endIdx - startIdx).Trim();
            var obj = JObject.Parse(json);
            Assert.AreEqual("dsfsqlserverdatabaseactivity", obj["data"]["type"].ToString());
        }
    }
}
