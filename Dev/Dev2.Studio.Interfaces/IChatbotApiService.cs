/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Data.ServiceModel;

namespace Dev2.Studio.Interfaces
{
    /// <summary>
    /// Defines the contract for communicating with external chatbot APIs (OpenAI, Anthropic, etc.).
    /// Handles authentication negotiation, payload formatting, and response parsing.
    /// </summary>
    public interface IChatbotApiService
    {
        /// <summary>
        /// Sends a chat completion request to the configured chatbot API and returns the assistant's response text.
        /// Handles authentication fallback (Bearer to x-api-key) and parameter negotiation
        /// (max_completion_tokens vs max_tokens, temperature support) automatically.
        /// </summary>
        /// <param name="chatMessages">The full conversation history including system prompt, formatted as role/content pairs.</param>
        /// <param name="source">The configured chatbot source containing endpoint URL, API key, and model selection.</param>
        /// <returns>The assistant's response text.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">Thrown when the API returns an error or the response format is unrecognized.</exception>
        Task<string> SendMessageAsync(IList<ChatCompletionMessage> chatMessages, ChatbotSource source);
    }

    /// <summary>
    /// Represents a single message in a chat completion request.
    /// </summary>
    public class ChatCompletionMessage
    {
        /// <summary>The role of the message author (e.g. "system", "user", "assistant").</summary>
        public string Role { get; set; }

        /// <summary>The text content of the message.</summary>
        public string Content { get; set; }
    }
}
