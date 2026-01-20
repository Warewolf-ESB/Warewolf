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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Common.Interfaces.NetStandard20;
using Warewolf.Common.NetStandard20;
using Warewolf.Data.Options;

namespace Dev2.Runtime.ServiceModel
{
    /// <summary>
    /// HttpClient-based execution strategy
    /// Modern implementation replacing deprecated WebClient
    /// </summary>
    public class HttpClientExecutionStrategy : IWebExecutionStrategy
    {
        public string Execute(IWebPostOptions options, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            
            try
            {
                Dev2Logger.Info("HttpClientExecutionStrategy - Using HttpClient for POST execution", GlobalConstants.WarewolfInfo);
                
                using (var httpClient = CreateHttpClient(options))
                {
                    var result = ExecutePost(httpClient, options).Result; // Blocking for now
                    return result;
                }
            }
            catch (AggregateException aggEx)
            {
                // Unwrap aggregate exceptions from async operations
                var ex = aggEx.InnerException ?? aggEx;
                Dev2Logger.Error("HttpClientExecutionStrategy - Request failed", ex, GlobalConstants.WarewolfError);
                errors.AddError(ex.Message);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("HttpClientExecutionStrategy - Unexpected error", ex, GlobalConstants.WarewolfError);
                errors.AddError(ex.Message);
                return string.Empty;
            }
        }

        private IHttpClientWrapperV2 CreateHttpClient(IWebPostOptions options)
        {
            Dev2Logger.Info($"HttpClientExecutionStrategy - Creating HttpClient for source: {options.Source?.Address}", GlobalConstants.WarewolfInfo);
            
            // Create wrapper with credentials if provided
            var wrapper = string.IsNullOrEmpty(options.Source?.UserName)
                ? new HttpClientWrapperV2()
                : new HttpClientWrapperV2(options.Source.UserName, options.Source.Password);
            
            // Set timeout if specified
            if (options.Timeout > 0)
            {
                wrapper.SetTimeout(TimeSpan.FromSeconds(options.Timeout));
            }
            
            // Add headers
            if (options.Headers != null)
            {
                Dev2Logger.Info($"HttpClientExecutionStrategy - Adding {options.Headers.Count()} header(s)", GlobalConstants.WarewolfInfo);
                
                foreach (var header in options.Headers)
                {
                    if (string.IsNullOrEmpty(header) || header == ":")
                    {
                        continue;
                    }
                    
                    var parts = header.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        var headerName = parts[0].Trim();
                        var headerValue = parts[1].Trim();
                        
                        if (!string.IsNullOrEmpty(headerName))
                        {
                            wrapper.SetHeader(headerName, headerValue);
                        }
                    }
                }
            }
            
            return wrapper;
        }

        private async System.Threading.Tasks.Task<string> ExecutePost(IHttpClientWrapperV2 httpClient, IWebPostOptions options)
        {
            var address = GetAddress(options.Source, options.Query);
            
            Dev2Logger.Info($"HttpClientExecutionStrategy - Executing POST to: {address}", GlobalConstants.WarewolfInfo);
            
            // Determine which POST method to use
            if (options.IsFormDataChecked)
            {
                return await ExecuteFormDataPost(httpClient, address, options);
            }
            else if (options.IsUrlEncodedChecked)
            {
                return await ExecuteUrlEncodedPost(httpClient, address, options);
            }
            else if (options.IsManualChecked)
            {
                return await ExecuteManualPost(httpClient, address, options);
            }
            
            Dev2Logger.Warn("HttpClientExecutionStrategy - No POST mode selected", GlobalConstants.WarewolfWarn);
            return string.Empty;
        }

        private async System.Threading.Tasks.Task<string> ExecuteManualPost(IHttpClientWrapperV2 httpClient, string address, IWebPostOptions options)
        {
            Dev2Logger.Info("HttpClientExecutionStrategy - Executing manual POST", GlobalConstants.WarewolfInfo);
            return await httpClient.PostAsync(address, options.PostData ?? string.Empty);
        }

        private async System.Threading.Tasks.Task<string> ExecuteFormDataPost(IHttpClientWrapperV2 httpClient, string address, IWebPostOptions options)
        {
            Dev2Logger.Info("HttpClientExecutionStrategy - Executing form data POST", GlobalConstants.WarewolfInfo);
            
            var formData = new MultipartFormDataContent();
            
            if (options.Parameters != null)
            {
                foreach (var param in options.Parameters)
                {
                    if (param is FileParameter fileParam)
                    {
                        var fileContent = new ByteArrayContent(fileParam.FileBytes);
                        formData.Add(fileContent, fileParam.Key, fileParam.FileName ?? fileParam.Key);
                        
                        if (!string.IsNullOrEmpty(fileParam.ContentType))
                        {
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileParam.ContentType);
                        }
                    }
                    else if (param is TextParameter textParam)
                    {
                        formData.Add(new StringContent(textParam.Value ?? string.Empty), textParam.Key);
                    }
                }
            }
            
            return await httpClient.PostFormDataAsync(address, formData);
        }

        private async System.Threading.Tasks.Task<string> ExecuteUrlEncodedPost(IHttpClientWrapperV2 httpClient, string address, IWebPostOptions options)
        {
            Dev2Logger.Info("HttpClientExecutionStrategy - Executing URL encoded POST", GlobalConstants.WarewolfInfo);
            
            var formValues = new Dictionary<string, string>();
            
            if (options.Parameters != null)
            {
                foreach (var param in options.Parameters)
                {
                    if (param is TextParameter textParam)
                    {
                        formValues[textParam.Key] = textParam.Value ?? string.Empty;
                    }
                }
            }
            
            var formContent = new FormUrlEncodedContent(formValues);
            return await httpClient.PostUrlEncodedAsync(address, formContent);
        }

        private string GetAddress(WebSource source, string relativeUri)
        {
            if (source == null)
            {
                return relativeUri ?? string.Empty;
            }
            
            if (!string.IsNullOrEmpty(source.Address) && !string.IsNullOrEmpty(relativeUri) && relativeUri.Contains(source.Address))
            {
                return relativeUri;
            }
            
            return $"{source.Address}{relativeUri}";
        }
    }
}
