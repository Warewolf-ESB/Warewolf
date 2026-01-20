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
using Dev2.Common;

namespace Dev2.Runtime.ServiceModel
{
    /// <summary>
    /// Factory for creating web execution strategies
    /// Implements feature flag pattern for safe migration from WebClient to HttpClient
    /// </summary>
    public static class WebExecutionStrategyFactory
    {
        private static readonly Lazy<bool> UseHttpClient = new Lazy<bool>(() =>
        {
            try
            {
                // Check environment variable first
                var envVar = Environment.GetEnvironmentVariable("WAREWOLF_USE_HTTPCLIENT");
                if (!string.IsNullOrEmpty(envVar))
                {
                    if (bool.TryParse(envVar, out bool envValue))
                    {
                        Dev2Logger.Info($"WebExecutionStrategyFactory - Using HttpClient from environment variable: {envValue}", GlobalConstants.WarewolfInfo);
                        return envValue;
                    }
                }
                
                // Check config setting (implement this when config infrastructure is available)
                // var configValue = Config.Server.Get("Web.UseHttpClient", "false");
                // if (bool.TryParse(configValue, out bool configResult))
                // {
                //     Dev2Logger.Info($"WebExecutionStrategyFactory - Using HttpClient from config: {configResult}", GlobalConstants.WarewolfInfo);
                //     return configResult;
                // }
                
                // Default to false (use WebClient) for safety
                Dev2Logger.Info("WebExecutionStrategyFactory - Defaulting to WebClient (feature flag disabled)", GlobalConstants.WarewolfInfo);
                return false;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("WebExecutionStrategyFactory - Error reading feature flag, defaulting to WebClient", ex, GlobalConstants.WarewolfError);
                return false;
            }
        });

        /// <summary>
        /// Creates the appropriate execution strategy based on the feature flag
        /// </summary>
        /// <returns>IWebExecutionStrategy instance</returns>
        public static IWebExecutionStrategy CreateStrategy()
        {
            if (UseHttpClient.Value)
            {
                Dev2Logger.Info("WebExecutionStrategyFactory - Creating HttpClientExecutionStrategy", GlobalConstants.WarewolfInfo);
                return new HttpClientExecutionStrategy();
            }
            
            Dev2Logger.Info("WebExecutionStrategyFactory - Creating WebClientExecutionStrategy", GlobalConstants.WarewolfInfo);
            return new WebClientExecutionStrategy();
        }

        /// <summary>
        /// Gets the current strategy type name for logging/debugging
        /// </summary>
        public static string GetCurrentStrategyName()
        {
            return UseHttpClient.Value ? "HttpClient" : "WebClient";
        }

        /// <summary>
        /// Checks if HttpClient is enabled without creating a strategy instance
        /// </summary>
        public static bool IsHttpClientEnabled()
        {
            return UseHttpClient.Value;
        }
    }
}
