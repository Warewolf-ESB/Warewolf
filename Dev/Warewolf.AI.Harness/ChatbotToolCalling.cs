using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Warewolf.AI.Harness
{
    /// <summary>
    /// A tool the model may call: an MCP tool definition (name, description, JSON Schema input).
    /// </summary>
    public class ChatbotTool
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("inputSchema")]
        public JObject InputSchema { get; set; }
    }

    /// <summary>
    /// A call the model asked for. Signature carries provider state that must be echoed back
    /// with the call (Gemini's thoughtSignature).
    /// </summary>
    public class ChatbotToolCall
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("arguments")]
        public JObject Arguments { get; set; }

        [JsonProperty("signature", NullValueHandling = NullValueHandling.Ignore)]
        public string Signature { get; set; }
    }

    public class ChatbotToolResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("isError")]
        public bool IsError { get; set; }
    }

    /// <summary>
    /// One round of tool use after the current user message: what the model said and asked for,
    /// and what the client got back from running those calls.
    /// </summary>
    public class ChatbotToolTurn
    {
        [JsonProperty("assistantText")]
        public string AssistantText { get; set; }

        [JsonProperty("toolCalls")]
        public List<ChatbotToolCall> ToolCalls { get; set; } = new List<ChatbotToolCall>();

        [JsonProperty("toolResults")]
        public List<ChatbotToolResult> ToolResults { get; set; } = new List<ChatbotToolResult>();
    }

    public class ChatbotReply
    {
        public string Text { get; set; } = string.Empty;
        public List<ChatbotToolCall> ToolCalls { get; set; } = new List<ChatbotToolCall>();
    }

    /// <summary>
    /// Native tool calling formats for each provider: the tool definitions, the tool rounds
    /// appended after the user message, and the parsing of a reply that may ask for tool calls.
    /// Messages arrive in the generic {role, content} shape used by ChatbotApiService.
    /// </summary>
    public static class ChatbotToolFormats
    {
        // ── OpenAI-compatible (OpenAI, OpenRouter, LM Studio, xAI …) ─────────────────

        public static List<object> OpenAITools(IEnumerable<ChatbotTool> tools) =>
            tools.Select(t => (object)new
            {
                type = "function",
                function = new { name = t.Name, description = t.Description ?? string.Empty, parameters = SchemaOrEmpty(t.InputSchema) }
            }).ToList();

        public static List<object> OpenAIMessages(List<object> messages, IEnumerable<ChatbotToolTurn> turns)
        {
            var result = new List<object>(messages);
            foreach (var turn in turns ?? Enumerable.Empty<ChatbotToolTurn>())
            {
                result.Add(new
                {
                    role = "assistant",
                    content = string.IsNullOrEmpty(turn.AssistantText) ? null : turn.AssistantText,
                    tool_calls = turn.ToolCalls.Select(c => new
                    {
                        id = c.Id,
                        type = "function",
                        function = new { name = c.Name, arguments = (c.Arguments ?? new JObject()).ToString(Formatting.None) }
                    }).ToList()
                });
                foreach (var r in turn.ToolResults)
                {
                    result.Add(new { role = "tool", tool_call_id = r.Id, content = r.Content ?? string.Empty });
                }
            }
            return result;
        }

        public static ChatbotReply ParseOpenAIReply(string responseJson)
        {
            var message = JObject.Parse(responseJson).SelectToken("choices[0].message") as JObject
                ?? throw new Exception("Could not parse response content from AI service");
            var reply = new ChatbotReply { Text = message["content"]?.Type == JTokenType.String ? message["content"].ToString() : string.Empty };
            if (message["tool_calls"] is JArray calls)
            {
                foreach (var call in calls.OfType<JObject>())
                {
                    reply.ToolCalls.Add(new ChatbotToolCall
                    {
                        Id = call["id"]?.ToString(),
                        Name = call.SelectToken("function.name")?.ToString(),
                        Arguments = ParseArguments(call.SelectToken("function.arguments")),
                    });
                }
            }
            return reply;
        }

        // ── Anthropic ─────────────────────────────────────────────────────────────

        public static List<object> AnthropicTools(IEnumerable<ChatbotTool> tools) =>
            tools.Select(t => (object)new
            {
                name = t.Name,
                description = t.Description ?? string.Empty,
                input_schema = SchemaOrEmpty(t.InputSchema)
            }).ToList();

        /// <summary>Messages without system ones (Anthropic takes those as a top-level field).</summary>
        public static List<object> AnthropicMessages(List<object> nonSystemMessages, IEnumerable<ChatbotToolTurn> turns)
        {
            var result = new List<object>(nonSystemMessages);
            foreach (var turn in turns ?? Enumerable.Empty<ChatbotToolTurn>())
            {
                var assistant = new List<object>();
                if (!string.IsNullOrEmpty(turn.AssistantText))
                {
                    assistant.Add(new { type = "text", text = turn.AssistantText });
                }
                assistant.AddRange(turn.ToolCalls.Select(c => (object)new { type = "tool_use", id = c.Id, name = c.Name, input = c.Arguments ?? new JObject() }));
                result.Add(new { role = "assistant", content = assistant });
                result.Add(new
                {
                    role = "user",
                    content = turn.ToolResults.Select(r => new { type = "tool_result", tool_use_id = r.Id, content = r.Content ?? string.Empty, is_error = r.IsError }).ToList()
                });
            }
            return result;
        }

        public static ChatbotReply ParseAnthropicReply(string responseJson)
        {
            var content = JObject.Parse(responseJson)["content"] as JArray
                ?? throw new Exception("Could not parse response content from AI service");
            var reply = new ChatbotReply();
            var text = new List<string>();
            foreach (var block in content.OfType<JObject>())
            {
                switch (block["type"]?.ToString())
                {
                    case "text":
                        text.Add(block["text"]?.ToString());
                        break;
                    case "tool_use":
                        reply.ToolCalls.Add(new ChatbotToolCall
                        {
                            Id = block["id"]?.ToString(),
                            Name = block["name"]?.ToString(),
                            Arguments = block["input"] as JObject ?? new JObject(),
                        });
                        break;
                }
            }
            reply.Text = string.Join("\n\n", text.Where(t => !string.IsNullOrEmpty(t)));
            return reply;
        }

        // ── Google Gemini ─────────────────────────────────────────────────────────

        /// <summary>parametersJsonSchema takes full JSON Schema; parameters only takes an OpenAPI subset.</summary>
        public static List<object> GeminiTools(IEnumerable<ChatbotTool> tools) =>
            new List<object>
            {
                new
                {
                    functionDeclarations = tools.Select(t => new
                    {
                        name = t.Name,
                        description = t.Description ?? string.Empty,
                        parametersJsonSchema = SchemaOrEmpty(t.InputSchema)
                    }).ToList()
                }
            };

        public static List<object> GeminiContents(List<object> contents, IEnumerable<ChatbotToolTurn> turns)
        {
            var result = new List<object>(contents);
            foreach (var turn in turns ?? Enumerable.Empty<ChatbotToolTurn>())
            {
                var parts = new List<object>();
                if (!string.IsNullOrEmpty(turn.AssistantText))
                {
                    parts.Add(new { text = turn.AssistantText });
                }
                foreach (var c in turn.ToolCalls)
                {
                    var part = new JObject { ["functionCall"] = new JObject { ["name"] = c.Name, ["args"] = c.Arguments ?? new JObject() } };
                    if (!string.IsNullOrEmpty(c.Signature))
                    {
                        part["thoughtSignature"] = c.Signature;
                    }
                    parts.Add(part);
                }
                result.Add(new { role = "model", parts });
                result.Add(new
                {
                    role = "user",
                    parts = turn.ToolResults.Select(r => new
                    {
                        functionResponse = new
                        {
                            name = r.Name,
                            response = r.IsError ? (object)new { error = r.Content ?? string.Empty } : new { content = r.Content ?? string.Empty }
                        }
                    }).ToList()
                });
            }
            return result;
        }

        public static ChatbotReply ParseGeminiReply(string responseJson)
        {
            var parts = JObject.Parse(responseJson).SelectToken("candidates[0].content.parts") as JArray
                ?? throw new Exception("Could not parse response content from AI service");
            var reply = new ChatbotReply();
            var text = new List<string>();
            foreach (var part in parts.OfType<JObject>())
            {
                if (part["functionCall"] is JObject call)
                {
                    reply.ToolCalls.Add(new ChatbotToolCall
                    {
                        // Gemini doesn't number its calls; results are matched by name and order.
                        Id = call["id"]?.ToString() ?? $"call_{reply.ToolCalls.Count}",
                        Name = call["name"]?.ToString(),
                        Arguments = call["args"] as JObject ?? new JObject(),
                        Signature = part["thoughtSignature"]?.ToString(),
                    });
                }
                else if (part["text"] != null && part["thought"]?.Value<bool>() != true)
                {
                    text.Add(part["text"].ToString());
                }
            }
            reply.Text = string.Join(string.Empty, text);
            return reply;
        }

        // ── Shared ────────────────────────────────────────────────────────────────

        private static JObject SchemaOrEmpty(JObject schema) =>
            schema ?? new JObject { ["type"] = "object", ["properties"] = new JObject() };

        private static JObject ParseArguments(JToken arguments)
        {
            if (arguments is JObject obj)
            {
                return obj;
            }
            var text = arguments?.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return new JObject();
            }
            try
            {
                return JObject.Parse(text);
            }
            catch (JsonException)
            {
                return new JObject();
            }
        }
    }
}
