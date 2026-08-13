/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.ComponentModel.DataAnnotations;

namespace Warewolf.Execution.QueueProcessor.Configuration
{
    /// <summary>
    /// Settings resolved from environment variables / ACA secrets. Bound with
    /// <c>ValidateDataAnnotations().ValidateOnStart()</c> so a misconfigured replica fails at
    /// startup instead of consuming messages it cannot process.
    ///
    /// Everything queue-shaped (queue name, prefetch, dead-letter, inputs) comes from the
    /// staged trigger <c>.bite</c>, not from here — this type carries only what the container
    /// itself needs to know.
    /// </summary>
    public sealed class QueueProcessorOptions
    {
        public const string SectionName = "QueueProcessor";

        // ── Trigger + source discovery ──────────────────────────────────────────

        /// <summary>
        /// Root of the staged configuration tree, baked into the container image by
        /// <c>Deploy-WwQueueProcessor.ps1</c> (delivery decision: bake, not mount/fetch — a
        /// scale-to-zero worker pays any config round-trip on every 0→1 scale):
        /// <code>
        /// {AppContext.BaseDirectory}/Settings/triggers/{triggerId}.bite
        /// {AppContext.BaseDirectory}/Settings/sources/{sourceId}.bite
        /// </code>
        /// Anchored to <see cref="AppContext.BaseDirectory"/> and never
        /// <c>Environment.CurrentDirectory</c>: the tree is deployed alongside the assembly, and
        /// cwd differs between <c>dotnet run</c>, the container (<c>WORKDIR /app</c>) and a
        /// debugger launch.
        /// </summary>
        public string SettingsPath { get; set; } =
            Path.Combine(AppContext.BaseDirectory, "Settings");

        /// <summary>
        /// Sub-folder of <see cref="SettingsPath"/> holding the queue-trigger definitions, i.e.
        /// <c>Settings/triggers/*.bite</c>. Kept separate from the sources so a trigger file can
        /// never be mistaken for a source (and vice versa) during discovery.
        /// </summary>
        public string TriggersSubPath { get; set; } = "triggers";

        /// <summary>
        /// Sub-folder of <see cref="SettingsPath"/> holding the broker <b>source</b> definitions
        /// referenced by the trigger's <c>QueueSourceId</c> / <c>QueueSinkId</c>, i.e.
        /// <c>Settings/sources/*.bite</c>. Every source a staged trigger points at is deployed
        /// here with it, so a replica can resolve its broker offline.
        /// </summary>
        public string SourcesSubPath { get; set; } = "sources";

        /// <summary>Glob for trigger files inside <see cref="TriggersPath"/>.</summary>
        public string TriggerFilter { get; set; } = "*.bite";

        /// <summary>Resolved absolute path of the trigger folder.</summary>
        public string TriggersPath => ResolveSubPath(TriggersSubPath);

        /// <summary>Resolved absolute path of the source folder.</summary>
        public string SourcesPath => ResolveSubPath(SourcesSubPath);

        /// <summary>
        /// Joins a sub-path onto <see cref="SettingsPath"/> (absolute sub-paths win), then
        /// tolerates a case mismatch. The case tolerance is not cosmetic: the worker runs on
        /// <b>Linux</b>, where <c>Settings/Sources</c> and <c>Settings/sources</c> are different
        /// directories, while the deploy script runs on Windows where they are the same. Without
        /// this, a folder staged with different casing than the configured name fails at cold
        /// start with "source not found" and looks like a missing file rather than a casing bug.
        /// </summary>
        string ResolveSubPath(string subPath)
        {
            var joined = Path.IsPathRooted(subPath)
                ? subPath
                : Path.Combine(SettingsPath, subPath);

            if (Directory.Exists(joined))
            {
                return joined;
            }

            var parent = Path.GetDirectoryName(joined);
            var leaf = Path.GetFileName(joined);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf) || !Directory.Exists(parent))
            {
                return joined;
            }

            foreach (var candidate in Directory.EnumerateDirectories(parent))
            {
                if (string.Equals(Path.GetFileName(candidate), leaf, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return joined;
        }

        /// <summary>
        /// Selects the active trigger when more than one file is staged. Optional when exactly
        /// one matches — the single-trigger-per-app model (decision #10) is the norm.
        /// </summary>
        public string? TriggerId { get; set; }

        // ── Engine ──────────────────────────────────────────────────────────────

        [Required(AllowEmptyStrings = false, ErrorMessage =
            "Engine:BaseUrl is required (e.g. https://wwengine.azurewebsites.net).")]
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Entra resource app id of the engine; the token scope becomes
        /// <c>api://{ResourceAppId}/.default</c>.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage =
            "Engine:ResourceAppId is required to acquire an app-only token for the engine.")]
        public string ResourceAppId { get; set; } = string.Empty;

        public string? TenantId { get; set; }

        /// <summary>
        /// <see cref="TenantId"/> with blank normalised to <c>null</c>, which is what every Azure
        /// credential must be handed.
        ///
        /// An EMPTY tenant id is not the same as "unspecified": passing <c>""</c> into
        /// <c>TokenRequestContext</c> makes every credential in the chain fail with
        /// <i>"Invalid tenant id provided"</i>. Blank is the documented default for a
        /// <b>system-assigned</b> managed identity (the tenant is implied by the platform), so it
        /// must degrade to <c>null</c> rather than propagate — otherwise the failure appears at the
        /// first message as an auth error and looks like a missing app role.
        /// <c>Deploy-WwQueueProcessor.ps1 -EngineTenantId</c> sets it explicitly for every other
        /// credential type, and the worker warns at startup when it is absent.
        /// </summary>
        public string? EffectiveTenantId => ConfigurationValues.NullIfBlank(TenantId);

        /// <summary>Explicit scope override; defaults to <c>api://{ResourceAppId}/.default</c>.</summary>
        public string? Scope { get; set; }

        public string EffectiveScope =>
            string.IsNullOrWhiteSpace(Scope) ? $"api://{ResourceAppId}/.default" : Scope!;

        /// <summary>User-assigned MI client id; unset means the system-assigned identity.</summary>
        public string? ManagedIdentityClientId { get; set; }

        /// <summary>Local-dev only client-secret fallback. Never set in Azure.</summary>
        public bool UseClientSecretFallback { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }

        public int TokenRefreshSkewSeconds { get; set; } = 300;

        /// <summary>
        /// Replaces the on-prem forwarder's <c>Timeout.InfiniteTimeSpan</c>
        /// (<c>WarewolfWebRequestForwarder.cs:97</c>), which could park a worker forever.
        /// Must satisfy <see cref="Validate"/>'s nesting rule.
        ///
        /// <para><b>Why 180 and not the original 45.</b> A timeout that fires while the engine is
        /// still working is not a safe failure: the worker abandons a request the engine goes on to
        /// COMPLETE, so the workflow's side effects happen anyway and the retry runs them a second
        /// time. Under-sizing this is what produced the 2026-08-11 stall - one engine call exceeded
        /// 45s, the delivery was left unacked, and with <c>Prefetch=1</c> the consumer never
        /// received another message.</para>
        ///
        /// <para>Sized from measured engine latency rather than guessed:</para>
        /// <list type="bullet">
        ///   <item>burst at the DEPLOYED concurrency (3+1 replicas, Prefetch=1) - max 11.5s;</item>
        ///   <item>controlled test at concurrency 4 - max 10.0s;</item>
        ///   <item>controlled stress at concurrency 10 - 153s completed successfully, and three
        ///   requests exceeded a 200s client timeout.</item>
        /// </list>
        /// 180s covers the worst SUCCESSFUL observation under 2.5x overload with headroom, and sits
        /// far below the engine's own ceiling.
        ///
        /// <para><b>Hard ceiling: the engine's <c>functionTimeout</c>, 00:10:00 = 600s in
        /// host.json.</b> Setting this at or above that is pointless - the engine kills the
        /// invocation first, so the worker would wait for a response that can never arrive.</para>
        /// </summary>
        [Range(1, 3600)]
        public int EngineTimeoutSeconds { get; set; } = 180;

        // ── Worker behaviour ────────────────────────────────────────────────────

        /// <summary>
        /// In-replica in-flight cap. Default 1 = measured on-prem behaviour (one workflow at a
        /// time per process). Raising it is a deliberate throughput change and requires
        /// <c>Prefetch &gt;= MaxConcurrency</c>.
        /// </summary>
        [Range(1, 64)]
        public int MaxConcurrency { get; set; } = 1;

        /// <summary>
        /// Drain budget on SIGTERM before the connection is closed. Must be >=
        /// <see cref="EngineTimeoutSeconds"/>, or a scale-in kills a replica while an engine call it
        /// would have completed is still outstanding - which loses the ack and forces a redelivery.
        /// Tracks the engine timeout: 180 -> 210.
        /// </summary>
        [Range(1, 3600)]
        public int ShutdownGraceSeconds { get; set; } = 210;

        /// <summary>
        /// How many times a delivery may be attempted before it is dead-lettered instead of retried,
        /// when the engine call fails at TRANSPORT level (timeout, socket error) rather than
        /// returning a non-2xx. Set with <c>WORKER__MAXDELIVERYATTEMPTS</c>.
        ///
        /// <para><b>Only 1 and 2 are meaningful, and values above 2 are clamped to 2.</b> Attempt
        /// counting uses the AMQP <c>redelivered</c> flag, which is a BOOLEAN - the broker records
        /// that a message has been delivered before, not how many times. Counting beyond two would
        /// need the message to be republished with an incremented header rather than requeued,
        /// which changes queue ordering and message identity; that was considered and deliberately
        /// not adopted.</para>
        ///
        /// <para><c>1</c> = dead-letter on the first transport failure, never retry.
        /// <c>2</c> (default) = requeue once, dead-letter if it fails again.</para>
        ///
        /// <para>WHY THIS EXISTS AT ALL: the pump previously left a failed delivery unacked,
        /// expecting the broker to redeliver it "when the channel drops". The channel does not
        /// drop, and with <c>Prefetch=1</c> the broker will not deliver anything else while one
        /// message is unacked - so a single engine timeout stalled the consumer permanently.
        /// Measured 2026-08-11: 34 messages stranded for 20+ minutes behind one unacked message,
        /// with a healthy replica and an attached consumer. KEDA cannot recover it either, because
        /// a non-empty queue keeps the replica alive and nothing forces the restart that would
        /// requeue the message.</para>
        /// </summary>
        public int MaxDeliveryAttempts { get; set; } = 2;

        /// <summary>
        /// <see cref="MaxDeliveryAttempts"/> constrained to what the redelivered flag can express.
        /// Clamping is reported by the pump at startup rather than applied silently.
        /// </summary>
        public int EffectiveMaxDeliveryAttempts => Math.Clamp(MaxDeliveryAttempts, 1, 2);

        /// <summary>
        /// Whether an engine <b>HTTP 500</b> should be retried (treated as transport) instead of
        /// dead-lettered. Set with <c>WORKER__RETRYENGINEINTERNALERRORS</c>. <b>Default false.</b>
        ///
        /// <para>OFF by default deliberately, because 500 is overloaded in this engine. It carries
        /// at least three different meanings:</para>
        /// <list type="bullet">
        ///   <item>a genuine workflow error (e.g. a required input was null) — retrying can never help;</item>
        ///   <item>a WOLF-8418 authorization denial, which surfaces as 500 rather than 403 —
        ///   retrying can never help;</item>
        ///   <item>host exhaustion such as "Insufficient memory to continue the execution of the
        ///   program", observed on the Consumption plan at concurrency 10 (2026-08-12) — retrying
        ///   very likely DOES help, because the message itself is fine.</item>
        /// </list>
        ///
        /// <para>Only the third is retryable, and the status code alone cannot distinguish them.
        /// Turning this on therefore buys resilience to host exhaustion at the cost of spending a
        /// delivery attempt on messages that are genuinely bad or genuinely unauthorized. Prefer
        /// capping concurrency so the host never exhausts; use this when that is not possible.</para>
        ///
        /// <para>Unaffected either way: 408, 429, 502, 503 and 504 are ALWAYS retryable — in every
        /// one of those the workflow provably never ran.</para>
        /// </summary>
        public bool RetryEngineInternalErrors { get; set; }

        /// <summary>TLS override; unset lets the source decide (plan §1.7).</summary>
        public bool? UseSsl { get; set; }

        // ── Key Vault (WFAES:: decryption of staged .bite files) ────────────────

        public string? KeyVaultName { get; set; }
        public string? KeyVaultSecretName { get; set; }

        /// <summary>Local-dev key material, bypassing Key Vault. Never set in production.</summary>
        public string? DebugKeyVaultSecret { get; set; }

        public bool KeyVaultConfigured =>
            !string.IsNullOrWhiteSpace(KeyVaultName) && !string.IsNullOrWhiteSpace(KeyVaultSecretName);

        public bool IsDevelopment { get; set; }

        /// <summary>
        /// Cross-field rules that data annotations cannot express. The timeout nesting is the
        /// one that matters operationally: if the engine call can outlive the drain window, a
        /// scale-in produces a duplicate execution (plan §2.6.1).
        /// </summary>
        public void Validate(int terminationGracePeriodSeconds)
        {
            if (EngineTimeoutSeconds > ShutdownGraceSeconds)
            {
                throw new TriggerConfigurationException(
                    $"Engine:TimeoutSeconds ({EngineTimeoutSeconds}s) must be <= " +
                    $"Worker:ShutdownGraceSeconds ({ShutdownGraceSeconds}s), otherwise an in-flight " +
                    "engine call can outlive the SIGTERM drain window and the message is redelivered " +
                    "after the workflow already ran (duplicate execution).");
            }

            if (terminationGracePeriodSeconds > 0 && ShutdownGraceSeconds >= terminationGracePeriodSeconds)
            {
                throw new TriggerConfigurationException(
                    $"Worker:ShutdownGraceSeconds ({ShutdownGraceSeconds}s) must be < the Container App's " +
                    $"terminationGracePeriodSeconds ({terminationGracePeriodSeconds}s), otherwise SIGKILL " +
                    "arrives mid-drain.");
            }
        }
    }
}
