/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*/
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.AI.Harness;

namespace Dev2.Tests.Runtime.ESB.Management
{
    [TestClass]
    public class ChatbotToolFormatsTests
    {
        const string Owner = "Coverage";
        const string Cat = nameof(ChatbotToolFormats);

        static readonly List<ChatbotTool> Tools = new List<ChatbotTool>
        {
            new ChatbotTool
            {
                Name = "azure_list_subscriptions",
                Description = "Lists subscriptions",
                InputSchema = JObject.Parse("{\"type\":\"object\",\"properties\":{\"filter\":{\"type\":\"string\"}}}")
            }
        };

        static List<ChatbotToolTurn> OneTurn() => new List<ChatbotToolTurn>
        {
            new ChatbotToolTurn
            {
                AssistantText = "Checking.",
                ToolCalls = { new ChatbotToolCall { Id = "call_1", Name = "azure_list_subscriptions", Arguments = JObject.Parse("{\"filter\":\"prod\"}"), Signature = "sig" } },
                ToolResults = { new ChatbotToolResult { Id = "call_1", Name = "azure_list_subscriptions", Content = "[\"sub-a\"]" } }
            }
        };

        static List<object> UserMessage() => new List<object> { new { role = "user", content = "which subscriptions?" } };

        static JArray AsJson(object value) => JArray.Parse(JsonConvert.SerializeObject(value));

        // ── OpenAI ───────────────────────────────────────────────────────────

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void OpenAITools_WrapsSchemaAsFunctionParameters()
        {
            var tools = AsJson(ChatbotToolFormats.OpenAITools(Tools));
            Assert.AreEqual("function", tools[0]["type"].ToString());
            Assert.AreEqual("azure_list_subscriptions", tools[0]["function"]["name"].ToString());
            Assert.AreEqual("string", tools[0]["function"]["parameters"]["properties"]["filter"]["type"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void OpenAIMessages_AppendsAssistantToolCallsThenToolResults()
        {
            var messages = AsJson(ChatbotToolFormats.OpenAIMessages(UserMessage(), OneTurn()));
            Assert.AreEqual(3, messages.Count);
            Assert.AreEqual("assistant", messages[1]["role"].ToString());
            Assert.AreEqual("call_1", messages[1]["tool_calls"][0]["id"].ToString());
            Assert.AreEqual("{\"filter\":\"prod\"}", messages[1]["tool_calls"][0]["function"]["arguments"].ToString());
            Assert.AreEqual("tool", messages[2]["role"].ToString());
            Assert.AreEqual("call_1", messages[2]["tool_call_id"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ParseOpenAIReply_ReadsToolCallsWithStringArguments()
        {
            var reply = ChatbotToolFormats.ParseOpenAIReply(
                "{\"choices\":[{\"message\":{\"content\":null,\"tool_calls\":[{\"id\":\"c9\",\"type\":\"function\",\"function\":{\"name\":\"azure_list_subscriptions\",\"arguments\":\"{\\\"filter\\\":\\\"x\\\"}\"}}]}}]}");
            Assert.AreEqual(string.Empty, reply.Text);
            Assert.AreEqual(1, reply.ToolCalls.Count);
            Assert.AreEqual("c9", reply.ToolCalls[0].Id);
            Assert.AreEqual("x", reply.ToolCalls[0].Arguments["filter"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ParseOpenAIReply_TextOnly_HasNoToolCalls()
        {
            var reply = ChatbotToolFormats.ParseOpenAIReply("{\"choices\":[{\"message\":{\"content\":\"hello\"}}]}");
            Assert.AreEqual("hello", reply.Text);
            Assert.AreEqual(0, reply.ToolCalls.Count);
        }

        // ── Anthropic ────────────────────────────────────────────────────────

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void AnthropicMessages_ToolUseThenToolResultBlocks()
        {
            var messages = AsJson(ChatbotToolFormats.AnthropicMessages(UserMessage(), OneTurn()));
            Assert.AreEqual(3, messages.Count);
            Assert.AreEqual("text", messages[1]["content"][0]["type"].ToString());
            Assert.AreEqual("tool_use", messages[1]["content"][1]["type"].ToString());
            Assert.AreEqual("prod", messages[1]["content"][1]["input"]["filter"].ToString());
            Assert.AreEqual("user", messages[2]["role"].ToString());
            Assert.AreEqual("tool_result", messages[2]["content"][0]["type"].ToString());
            Assert.AreEqual("call_1", messages[2]["content"][0]["tool_use_id"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ParseAnthropicReply_JoinsTextAndReadsToolUse()
        {
            var reply = ChatbotToolFormats.ParseAnthropicReply(
                "{\"content\":[{\"type\":\"text\",\"text\":\"Let me look.\"},{\"type\":\"tool_use\",\"id\":\"toolu_1\",\"name\":\"azure_list_subscriptions\",\"input\":{}}]}");
            Assert.AreEqual("Let me look.", reply.Text);
            Assert.AreEqual("toolu_1", reply.ToolCalls[0].Id);
        }

        // ── Gemini ───────────────────────────────────────────────────────────

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GeminiTools_UsesParametersJsonSchema()
        {
            var tools = AsJson(ChatbotToolFormats.GeminiTools(Tools));
            var declaration = tools[0]["functionDeclarations"][0];
            Assert.AreEqual("azure_list_subscriptions", declaration["name"].ToString());
            Assert.IsNotNull(declaration["parametersJsonSchema"]["properties"]["filter"]);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void GeminiContents_EchoesThoughtSignatureAndFunctionResponse()
        {
            var contents = AsJson(ChatbotToolFormats.GeminiContents(UserMessage(), OneTurn()));
            Assert.AreEqual("model", contents[1]["role"].ToString());
            Assert.AreEqual("sig", contents[1]["parts"][1]["thoughtSignature"].ToString());
            Assert.AreEqual("azure_list_subscriptions", contents[2]["parts"][0]["functionResponse"]["name"].ToString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ParseGeminiReply_ReadsFunctionCallAndSkipsThoughts()
        {
            var reply = ChatbotToolFormats.ParseGeminiReply(
                "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"thinking\",\"thought\":true},{\"text\":\"ok\"},{\"functionCall\":{\"name\":\"azure_list_subscriptions\",\"args\":{\"filter\":\"y\"}},\"thoughtSignature\":\"abc\"}]}}]}");
            Assert.AreEqual("ok", reply.Text);
            Assert.AreEqual("call_0", reply.ToolCalls[0].Id);
            Assert.AreEqual("abc", reply.ToolCalls[0].Signature);
            Assert.AreEqual("y", reply.ToolCalls[0].Arguments["filter"].ToString());
        }
    }
}
