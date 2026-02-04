#pragma warning disable
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
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
    /// <summary>
    /// Represents the type of a chat message, used to distinguish between
    /// user input, bot responses, system notifications, and error messages.
    /// </summary>
    public enum ChatMessageType
    {
        /// <summary>A message sent by the user.</summary>
        User,

        /// <summary>A response from the chatbot API.</summary>
        Bot,

        /// <summary>A system notification or greeting message (not sent to the API).</summary>
        System,

        /// <summary>An error message (not sent to the API).</summary>
        Error
    }

    /// <summary>
    /// Represents a single message in the chatbot conversation with typed metadata.
    /// Replaces raw string-based messages that relied on prefix parsing (e.g. "You: ", "Bot: ").
    /// </summary>
    public class ChatMessage
    {
        /// <summary>Unique identifier for this message.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>The type of message (User, Bot, System, or Error).</summary>
        [JsonProperty("type")]
        public ChatMessageType Type { get; set; }

        /// <summary>The text content of the message.</summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>The timestamp when the message was created.</summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets the display text for the message, including a timestamp and prefix based on the message type.
        /// Used for UI binding to maintain the existing display format.
        /// </summary>
        [JsonIgnore]
        public string DisplayText
        {
            get
            {
                var time = Timestamp.ToString("HH:mm");
                switch (Type)
                {
                    case ChatMessageType.User:
                        return $"[{time}] You: {Content}";
                    case ChatMessageType.Bot:
                        return $"[{time}] Bot: {Content}";
                    case ChatMessageType.Error:
                        return $"[{time}] Error: {Content}";
                    case ChatMessageType.System:
                        return $"[{time}] Chatbot: {Content}";
                    default:
                        return Content;
                }
            }
        }

        /// <summary>
        /// Creates a new ChatMessage with a unique ID and the current timestamp.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="content">The message content.</param>
        /// <returns>A new ChatMessage instance.</returns>
        public static ChatMessage Create(ChatMessageType type, string content)
        {
            return new ChatMessage
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                Content = content,
                Timestamp = DateTime.Now
            };
        }
    }

    /// <summary>
    /// Represents a saved chatbot conversation with metadata for the conversation history dropdown.
    /// </summary>
    public class ChatConversation
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("lastMessageAt")]
        public DateTime LastMessageAt { get; set; }

        [JsonProperty("messages")]
        public List<ChatMessage> Messages { get; set; }

        [JsonProperty("conversationSummary")]
        public string ConversationSummary { get; set; }

        public ChatConversation()
        {
            Messages = new List<ChatMessage>();
        }

        public static ChatConversation CreateNew()
        {
            return new ChatConversation
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = "New Chat",
                CreatedAt = DateTime.Now,
                LastMessageAt = DateTime.Now,
                Messages = new List<ChatMessage>()
            };
        }
    }
}
