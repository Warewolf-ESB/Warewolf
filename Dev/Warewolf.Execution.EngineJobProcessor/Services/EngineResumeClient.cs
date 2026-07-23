/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Core;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;

namespace Warewolf.Execution.EngineJobProcessor.Services;

/// <summary>Outcome of a resume dispatch — used for structured logging and tests.</summary>
public enum ResumeDispatchOutcome
{
    /// <summary>Engine claimed and (synchronously) executed the job — HTTP 200 (202 also tolerated for forward compatibility).</summary>
    Dispatched,

    /// <summary>Engine rejected the claim (HTTP 409) — another dispatch already won. Not an error.</summary>
    AlreadyClaimed,

    /// <summary>
    /// The claim acknowledgment did not arrive within the short ack timeout. The engine
    /// executes resume requests synchronously, so a long-running continuation lands here —
    /// the job is most likely claimed and executing. NOT an error: the state machine
    /// reconciles on the next tick (still Scheduled → re-dispatched; claimed → 409/absent).
    /// </summary>
    AckTimeout,

    /// <summary>Dispatch failed (unexpected status or network error). Job stays Scheduled and is retried next tick.</summary>
    Failed,
}

/// <summary>
/// Fire-and-forget dispatcher for the Execution Engine's secured resume route.
///
/// Contract (see HangeFire-Azure-Architecture.md §4.2):
/// <list type="bullet">
///   <item>POST <c>{ENGINE_RESUME_BASEURL}/secure/resume/{jobId}</c> with a bearer token
///         acquired by the processor's managed identity for <c>%ENGINE_RESUME_SCOPE%</c>
///         (role <c>Warewolf_JobProcessor</c> on the engine's app registration). The route
///         rides the engine's <c>/secure</c> prefix so the full auth pipeline applies.</item>
///   <item>Awaits ONLY the engine's short ack window — the engine responds 200 on
///         completion (it executes SYNCHRONOUSLY), so a long continuation elapses the window
///         and lands on <see cref="ResumeDispatchOutcome.AckTimeout"/>; the processor never
///         blocks on workflow completion. The engine owns execution + final status recording.</item>
///   <item>A failed dispatch leaves the job in <c>Scheduled</c>; the atomic
///         Scheduled→Processing claim on the engine makes duplicate dispatches harmless
///         (the loser receives 409).</item>
/// </list>
/// </summary>
public sealed class EngineResumeClient : IEngineResumeClient
{
    readonly HttpClient _httpClient;
    readonly TokenCredential? _credential;
    readonly ProcessorSettings _settings;
    readonly ILogger<EngineResumeClient> _logger;

    public EngineResumeClient(
        HttpClient httpClient,
        TokenCredential? credential,
        ProcessorSettings settings,
        ILogger<EngineResumeClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _credential = credential;
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Fail fast on missing dispatch config — but only when persistence is enabled;
        // on a disabled processor this client is injected yet never invoked.
        if (Dev2.Common.Config.Persistence.Enable)
        {
            if (string.IsNullOrWhiteSpace(_settings.EngineResumeBaseUrl))
            {
                throw new InvalidOperationException(
                    "ENGINE_RESUME_BASEURL is not configured — the JobProcessor cannot dispatch resume requests.");
            }

            if (!_settings.AuthDisabled && (_credential is null || string.IsNullOrWhiteSpace(_settings.EngineResumeScope)))
            {
                throw new InvalidOperationException(
                    "ENGINE_RESUME_SCOPE (and a token credential) are required unless ENGINE_RESUME_AUTH_DISABLED=true (development only).");
            }
        }
    }

    /// <summary>
    /// Dispatches the resume for <paramref name="jobId"/> and awaits only the claim ack.
    /// Never throws — every failure maps to <see cref="ResumeDispatchOutcome.Failed"/>
    /// so one bad job cannot abort the poll loop.
    /// </summary>
    public async Task<ResumeDispatchOutcome> TryResumeAsync(string jobId, CancellationToken cancellationToken)
    {
        // The engine's resume route lives under /secure so the full auth middleware
        // pipeline (EasyAuth → claims → policy) applies to it.
        var url = $"{_settings.EngineResumeBaseUrl}/secure/resume/{Uri.EscapeDataString(jobId)}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (!_settings.AuthDisabled)
            {
                // TokenCredential implementations (ManagedIdentityCredential / DefaultAzureCredential)
                // cache tokens internally — this is an in-memory read on warm calls.
                var token = await _credential!.GetTokenAsync(
                    new TokenRequestContext(new[] { _settings.EngineResumeScope! }), cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_settings.ResumeTimeoutSeconds));

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);

            switch (response.StatusCode)
            {
                // 200 = the engine claimed AND executed the job synchronously within the
                // ack window; 202 kept for forward compatibility with async engines.
                case HttpStatusCode.OK:
                case HttpStatusCode.Accepted:
                    _logger.LogInformation("JobDispatch | JobId={JobId} | Outcome=Dispatched | Engine accepted the claim (HTTP {StatusCode}).", jobId, (int)response.StatusCode);
                    return ResumeDispatchOutcome.Dispatched;

                case HttpStatusCode.Conflict:
                    _logger.LogInformation("JobDispatch | JobId={JobId} | Outcome=AlreadyClaimed | Another dispatch won the claim — no action.", jobId);
                    return ResumeDispatchOutcome.AlreadyClaimed;

                default:
                    _logger.LogWarning(
                        "JobDispatch | JobId={JobId} | Outcome=Failed | Status={StatusCode} | Job stays Scheduled and will be retried next tick.",
                        jobId, (int)response.StatusCode);
                    return ResumeDispatchOutcome.Failed;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Ack window elapsed — the engine executes synchronously, so a long
            // continuation is still running server-side (client disconnect does not
            // abort it). The job-state machine reconciles on the next tick.
            _logger.LogInformation(
                "JobDispatch | JobId={JobId} | Outcome=AckTimeout | No ack within {TimeoutSeconds}s — engine likely still executing; state reconciles next tick.",
                jobId, _settings.ResumeTimeoutSeconds);
            return ResumeDispatchOutcome.AckTimeout;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _logger.LogWarning(ex,
                "JobDispatch | JobId={JobId} | Outcome=Failed | {ExceptionType} — job stays Scheduled and will be retried next tick.",
                jobId, ex.GetType().Name);
            return ResumeDispatchOutcome.Failed;
        }
    }
}
