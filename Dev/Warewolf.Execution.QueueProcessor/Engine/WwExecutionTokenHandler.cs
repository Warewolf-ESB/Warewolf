/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Net.Http.Headers;
using Azure.Core;
using Microsoft.Extensions.Options;
using Warewolf.Execution.QueueProcessor.Configuration;
using Dev2.Common;

namespace Warewolf.Execution.QueueProcessor.Engine
{
    /// <summary>
    /// Authenticates every outbound engine call with an app-only (client-credentials) token.
    /// Ported from the shipped reference client
    /// (<c>ClientExamples/AzureServiceBus/Auth/WwExecutionTokenHandler.cs</c>) rather than
    /// written afresh — decision #10/#24 — so token caching, the single-flight refresh guard,
    /// and the expiry-skew policy stay identical across every Warewolf engine caller.
    ///
    /// Replaces the on-prem forwarder's Windows/basic <c>NetworkCredential</c>
    /// (<c>HttpClientFactory.cs:33-53</c>): the trigger's stored username/password are not
    /// carried to Azure at all (decision #4).
    /// </summary>
    public sealed class WwExecutionTokenHandler : DelegatingHandler
    {
        const string ExecutionId = "QueueProcessor-Token";

        readonly TokenCredential _credential;
        readonly QueueProcessorOptions _options;
        readonly SemaphoreSlim _refreshLock = new(1, 1);
        readonly string[] _scopes;

        AccessToken _cachedToken;

        public WwExecutionTokenHandler(TokenCredential credential, IOptions<QueueProcessorOptions> options)
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            _scopes = new[] { _options.EffectiveScope };
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            if (!IsExpired(_cachedToken))
            {
                return _cachedToken.Token;
            }

            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!IsExpired(_cachedToken))
                {
                    return _cachedToken.Token;
                }

                Dev2Logger.Debug(
                    $"Acquiring app-only token for scope {_options.EffectiveScope}", ExecutionId);

                // EffectiveTenantId, never TenantId: an empty tenant id is not "unspecified", and
                // passing "" makes every credential in the chain fail with "Invalid tenant id
                // provided". See QueueProcessorOptions.EffectiveTenantId for why blank is legal.
                var context = new TokenRequestContext(_scopes, tenantId: _options.EffectiveTenantId);
                _cachedToken = await _credential.GetTokenAsync(context, cancellationToken)
                                                .ConfigureAwait(false);

                Dev2Logger.Debug(
                    $"Token acquired; expires {_cachedToken.ExpiresOn:u} " +
                    $"(skew {_options.TokenRefreshSkewSeconds}s)", ExecutionId);

                return _cachedToken.Token;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(
                    "Failed to acquire an app-only token for the Execution Engine. Verify this " +
                    "Container App's managed identity holds the engine app role " +
                    "'Warewolf_QueueProcessor' (a roleless caller is rejected) and that the " +
                    "tenant/scope are correct.", ex, ExecutionId);
                throw;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        bool IsExpired(AccessToken token)
        {
            if (string.IsNullOrEmpty(token.Token)) return true;
            var skew = TimeSpan.FromSeconds(Math.Max(0, _options.TokenRefreshSkewSeconds));
            return DateTimeOffset.UtcNow >= token.ExpiresOn - skew;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _refreshLock.Dispose();
            base.Dispose(disposing);
        }
    }
}
