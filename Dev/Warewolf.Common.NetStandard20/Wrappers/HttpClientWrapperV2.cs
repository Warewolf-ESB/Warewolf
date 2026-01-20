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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Warewolf.Common.Interfaces.NetStandard20;

namespace Warewolf.Common.NetStandard20
{
    /// <summary>
    /// HttpClient wrapper implementation for POST activity
    /// Replaces deprecated WebClient with modern HttpClient
    /// </summary>
    public class HttpClientWrapperV2 : IHttpClientWrapperV2
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientHandler _handler;
        private bool _disposed;
        private HttpStatusCode _lastStatusCode;

        public HttpClientWrapperV2()
        {
            _handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(_handler)
            {
                Timeout = TimeSpan.FromSeconds(100) // Default timeout
            };

            _httpClient.DefaultRequestHeaders.Add("user-agent", GlobalConstants.UserAgentString);

            Dev2Logger.Info("HttpClientWrapperV2 - Created new HttpClient instance", GlobalConstants.WarewolfInfo);
        }

        public HttpClientWrapperV2(string userName, string password) : this()
        {
            if (!string.IsNullOrEmpty(userName))
            {
                _handler.Credentials = new NetworkCredential(userName, password);
                _handler.PreAuthenticate = true;
                HasCredentials = true;

                Dev2Logger.Info($"HttpClientWrapperV2 - Configured with credentials for user: {userName}", GlobalConstants.WarewolfInfo);
            }
        }

        public HttpStatusCode LastStatusCode => _lastStatusCode;

        public bool HasCredentials { get; private set; }

        public async Task<string> PostAsync(string url, string data)
        {
            Dev2Logger.Info($"HttpClientWrapperV2.PostAsync - URL: {url}, Data Length: {data?.Length ?? 0}", GlobalConstants.WarewolfInfo);

            var content = new StringContent(data ?? string.Empty, Encoding.UTF8);

            // Check if Content-Type header was set via SetHeader
            if (_httpClient.DefaultRequestHeaders.Contains("Content-Type"))
            {
                var contentType = string.Join(",", _httpClient.DefaultRequestHeaders.GetValues("Content-Type"));
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                _httpClient.DefaultRequestHeaders.Remove("Content-Type");
                Dev2Logger.Info($"HttpClientWrapperV2.PostAsync - Content-Type set to: {contentType}", GlobalConstants.WarewolfInfo);
            }

            return await PostAsync(url, content);
        }

        public async Task<string> PostAsync(string url, HttpContent content)
        {
            try
            {
                Dev2Logger.Info($"HttpClientWrapperV2.PostAsync(HttpContent) - URL: {url}", GlobalConstants.WarewolfInfo);

                var response = await _httpClient.PostAsync(url, content);
                _lastStatusCode = response.StatusCode;

                Dev2Logger.Info($"HttpClientWrapperV2.PostAsync - Response Status: {response.StatusCode}", GlobalConstants.WarewolfInfo);

                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                var base64Response = Convert.ToBase64String(responseBytes);

                // If not successful, still return the response for error handling
                if (!response.IsSuccessStatusCode)
                {
                    Dev2Logger.Warn($"HttpClientWrapperV2.PostAsync - Non-success status code: {response.StatusCode}", GlobalConstants.WarewolfWarn);
                }

                return base64Response;
            }
            catch (TaskCanceledException ex)
            {
                Dev2Logger.Error("HttpClientWrapperV2.PostAsync - Request timed out", ex, GlobalConstants.WarewolfError);
                throw new TimeoutException($"HTTP request to {url} timed out", ex);
            }
            catch (HttpRequestException ex)
            {
                Dev2Logger.Error($"HttpClientWrapperV2.PostAsync - HTTP request failed for URL: {url}", ex, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public async Task<string> PostFormDataAsync(string url, MultipartFormDataContent formData)
        {
            Dev2Logger.Info($"HttpClientWrapperV2.PostFormDataAsync - URL: {url}", GlobalConstants.WarewolfInfo);
            return await PostAsync(url, formData);
        }

        public async Task<string> PostUrlEncodedAsync(string url, FormUrlEncodedContent formData)
        {
            Dev2Logger.Info($"HttpClientWrapperV2.PostUrlEncodedAsync - URL: {url}", GlobalConstants.WarewolfInfo);
            return await PostAsync(url, formData);
        }

        public void SetHeader(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var headerName = name.Trim();
            var headerValue = value?.Trim() ?? string.Empty;

            Dev2Logger.Info($"HttpClientWrapperV2.SetHeader - Setting header: {headerName} = {(headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ? "[REDACTED]" : headerValue)}", GlobalConstants.WarewolfInfo);

            // Remove existing header if present
            if (_httpClient.DefaultRequestHeaders.Contains(headerName))
            {
                _httpClient.DefaultRequestHeaders.Remove(headerName);
            }

            // Always use TryAddWithoutValidation to avoid exceptions with restricted headers
            // This handles ALL headers including: Host, User-Agent, Content-Type, etc.
            try
            {
                var success = _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(headerName, headerValue);
                if (!success)
                {
                    Dev2Logger.Warn($"HttpClientWrapperV2.SetHeader - Failed to add header: {headerName}", GlobalConstants.WarewolfWarn);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"HttpClientWrapperV2.SetHeader - Error adding header: {headerName}", ex, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public void SetTimeout(TimeSpan timeout)
        {
            _httpClient.Timeout = timeout;
            Dev2Logger.Info($"HttpClientWrapperV2.SetTimeout - Timeout set to: {timeout.TotalSeconds}s", GlobalConstants.WarewolfInfo);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Dev2Logger.Info("HttpClientWrapperV2 - Disposing HttpClient", GlobalConstants.WarewolfInfo);
                    _httpClient?.Dispose();
                    _handler?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
