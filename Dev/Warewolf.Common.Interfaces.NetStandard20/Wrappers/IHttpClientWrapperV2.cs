/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2025 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Warewolf.Common.Interfaces.NetStandard20
{
    /// <summary>
    /// V2 HttpClient wrapper interface for all HTTP methods
    /// Replaces deprecated WebClient with modern HttpClient
    /// </summary>
    public interface IHttpClientWrapperV2 : IDisposable
    {
        /// <summary>
        /// Gets the last HTTP status code from the most recent request
        /// </summary>
        HttpStatusCode LastStatusCode { get; }

        /// <summary>
        /// Gets whether credentials are configured
        /// </summary>
        bool HasCredentials { get; }

        /// <summary>
        /// Performs an HTTP GET request
        /// </summary>
        /// <param name="url">The complete URL including query string</param>
        /// <returns>Base64 encoded response</returns>
        Task<string> GetAsync(string url);

        /// <summary>
        /// Performs an HTTP POST with string content
        /// </summary>
        /// <param name="url">The complete URL including query string</param>
        /// <param name="data">The string data to post</param>
        /// <returns>Base64 encoded response</returns>
        Task<string> PostAsync(string url, string data);

        /// <summary>
        /// Performs an HTTP POST with HttpContent
        /// </summary>
        /// <param name="url">The complete URL including query string</param>
        /// <param name="content">The HTTP content to post</param>
        /// <returns>Base64 encoded response</returns>
        Task<string> PostAsync(string url, HttpContent content);

        /// <summary>
        /// Performs an HTTP POST with multipart form data
        /// </summary>
        /// <param name="url">The complete URL including query string</param>
        /// <param name="formData">The multipart form data content</param>
        /// <returns>Base64 encoded response</returns>
        Task<string> PostFormDataAsync(string url, MultipartFormDataContent formData);

        /// <summary>
        /// Performs an HTTP POST with URL encoded form data
        /// </summary>
        /// <param name="url">The complete URL including query string</param>
        /// <param name="formData">The URL encoded form data</param>
        /// <returns>Base64 encoded response</returns>
        Task<string> PostUrlEncodedAsync(string url, FormUrlEncodedContent formData);

        /// <summary>
        /// Performs an HTTP PUT with string content
        /// </summary>
        /// <param name="url">The complete URL including query string</param>
        /// <param name="data">The string data to put</param>
        /// <returns>Base64 encoded response</returns>
        Task<string> PutAsync(string url, string data);

        /// <summary>
        /// Performs an HTTP DELETE request
        /// </summary>
        /// <param name="url">The complete URL including query string</param>
        /// <returns>Base64 encoded response</returns>
        Task<string> DeleteAsync(string url);

        /// <summary>
        /// Sets a header on the HttpClient
        /// </summary>
        /// <param name="name">Header name</param>
        /// <param name="value">Header value</param>
        void SetHeader(string name, string value);

        /// <summary>
        /// Sets the timeout for HTTP requests
        /// </summary>
        /// <param name="timeout">Timeout duration</param>
        void SetTimeout(TimeSpan timeout);
    }
}

