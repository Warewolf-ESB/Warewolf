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
        /// <summary>The type of message (User, Bot, System, or Error).</summary>
        public ChatMessageType Type { get; set; }

        /// <summary>The text content of the message.</summary>
        public string Content { get; set; }

        /// <summary>The timestamp when the message was created.</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets the display text for the message, including a prefix based on the message type.
        /// Used for UI binding to maintain the existing display format.
        /// </summary>
        public string DisplayText
        {
            get
            {
                switch (Type)
                {
                    case ChatMessageType.User:
                        return $"You: {Content}";
                    case ChatMessageType.Bot:
                        return $"Bot: {Content}";
                    case ChatMessageType.Error:
                        return $"Error: {Content}";
                    case ChatMessageType.System:
                        return $"Chatbot: {Content}";
                    default:
                        return Content;
                }
            }
        }

        /// <summary>
        /// Creates a new ChatMessage with the current timestamp.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="content">The message content.</param>
        /// <returns>A new ChatMessage instance.</returns>
        public static ChatMessage Create(ChatMessageType type, string content)
        {
            return new ChatMessage
            {
                Type = type,
                Content = content,
                Timestamp = DateTime.Now
            };
        }
    }
}
