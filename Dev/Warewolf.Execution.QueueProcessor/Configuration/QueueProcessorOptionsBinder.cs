/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.Extensions.Configuration;

namespace Warewolf.Execution.QueueProcessor.Configuration
{
    /// <summary>
    /// Maps the flat <c>QUEUE__* / ENGINE__* / WORKER__* / RABBITMQ__* / KEYVAULT__*</c>
    /// configuration contract onto <see cref="QueueProcessorOptions"/>.
    ///
    /// <para><b>Why this is a type and not a lambda in <c>Program.cs</c>.</b> It used to be the
    /// body of <c>AddOptions&lt;QueueProcessorOptions&gt;().Configure(o =&gt; …)</c>, and because
    /// top-level statements cannot be unit tested, nothing verified that the contract the deploy
    /// script emits is the contract the worker reads. It was not:
    /// <c>Deploy-WwQueueProcessor.ps1</c> emitted <c>WORKER__MAXDELIVERYATTEMPTS</c> and
    /// <c>WORKER__RETRYENGINEINTERNALERRORS</c>, both were set on the live Container App, and
    /// <b>neither was ever assigned</b> — so <c>-MaxDeliveryAttempts 1</c> and
    /// <c>-RetryEngineInternalErrors</c> were silent no-ops. The values happened to equal the
    /// property defaults, which is exactly why it went unnoticed. Extracting the mapping here is
    /// what makes that testable.</para>
    ///
    /// <para><b>Why not <c>IConfiguration.Bind()</c>.</b> Two reasons, both load-bearing. The keys
    /// deliberately do not match the property names (<c>ENGINE:BASEURL</c> →
    /// <see cref="QueueProcessorOptions.BaseUrl"/>, <c>WORKER:MAXCONCURRENCY</c> →
    /// <see cref="QueueProcessorOptions.MaxConcurrency"/>), so binding would need a key-mapping
    /// layer anyway. More importantly, <c>Bind</c> treats an empty string as a value and would
    /// happily set <c>SettingsPath = ""</c> from the <c>appsettings.json</c> placeholder — which is
    /// the precise defect <see cref="ConfigurationValues"/> exists to prevent.</para>
    ///
    /// <para><b>Defaults are read off <paramref name="options"/>, never restated as literals.</b>
    /// A freshly constructed <see cref="QueueProcessorOptions"/> already carries every documented
    /// default in its property initialisers, so passing the current value as the fallback makes
    /// drift between the two structurally impossible. It previously was not: this mapping defaulted
    /// <c>ENGINE:TIMEOUTSECONDS</c> to 45 and <c>WORKER:SHUTDOWNGRACESECONDS</c> to 60 while the
    /// properties documented 180 and 210 — and since the <c>Configure</c> block wins, the detailed
    /// rationale on those properties described behaviour that could not occur.</para>
    /// </summary>
    public static class QueueProcessorOptionsBinder
    {
        /// <summary>
        /// Applies <paramref name="configuration"/> onto <paramref name="options"/>. Any key that is
        /// absent — or present but blank, which ACA and <c>appsettings.json</c> produce as readily —
        /// leaves the option at the default it was constructed with.
        /// </summary>
        /// <param name="isDevelopment">
        /// From <c>LoggingConfiguration.FromEnvironment()</c>, not from <paramref name="configuration"/>:
        /// the engine and the worker share one environment contract for this and it is resolved
        /// before the host is built.
        /// </param>
        public static void Apply(
            IConfiguration configuration, QueueProcessorOptions options, bool isDevelopment)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(options);

            var cfg = configuration;
            var o = options;

            // ── Trigger + source discovery ──────────────────────────────────────
            // AppContext.BaseDirectory (the property default), never Environment.CurrentDirectory:
            // the settings tree is deployed ALONGSIDE the assembly (Settings\<source>.bite +
            // Settings\triggers\<id>.bite), and cwd differs between `dotnet run`, the container
            // (WORKDIR /app) and a debugger launch.
            o.SettingsPath = ConfigurationValues.ReadString(cfg["QUEUE:SETTINGSPATH"], o.SettingsPath);
            o.TriggersSubPath = ConfigurationValues.ReadString(cfg["QUEUE:TRIGGERSSUBPATH"], o.TriggersSubPath);
            o.SourcesSubPath = ConfigurationValues.ReadString(cfg["QUEUE:SOURCESSUBPATH"], o.SourcesSubPath);
            o.TriggerFilter = ConfigurationValues.ReadString(cfg["QUEUE:TRIGGERFILTER"], o.TriggerFilter);
            o.TriggerId = cfg["QUEUE:TRIGGERID"];

            // ── Engine ──────────────────────────────────────────────────────────
            // BaseUrl and ResourceAppId carry [Required], so an absent value must reach validation
            // as empty rather than being defaulted away.
            o.BaseUrl = cfg["ENGINE:BASEURL"] ?? string.Empty;
            o.ResourceAppId = cfg["ENGINE:RESOURCEAPPID"] ?? string.Empty;
            o.TenantId = cfg["ENGINE:TENANTID"];
            o.Scope = cfg["ENGINE:SCOPE"];
            o.ManagedIdentityClientId = cfg["ENGINE:MANAGEDIDENTITYCLIENTID"];
            o.UseClientSecretFallback =
                ConfigurationValues.ReadBool(cfg["ENGINE:USECLIENTSECRETFALLBACK"], o.UseClientSecretFallback);
            o.ClientId = cfg["ENGINE:CLIENTID"];
            o.ClientSecret = cfg["ENGINE:CLIENTSECRET"];
            o.EngineTimeoutSeconds =
                ConfigurationValues.ReadInt(cfg["ENGINE:TIMEOUTSECONDS"], o.EngineTimeoutSeconds);
            o.TokenRefreshSkewSeconds =
                ConfigurationValues.ReadInt(cfg["ENGINE:TOKENREFRESHSKEWSECONDS"], o.TokenRefreshSkewSeconds);

            // ── Worker behaviour ────────────────────────────────────────────────
            o.MaxConcurrency = ConfigurationValues.ReadInt(cfg["WORKER:MAXCONCURRENCY"], o.MaxConcurrency);
            o.ShutdownGraceSeconds =
                ConfigurationValues.ReadInt(cfg["WORKER:SHUTDOWNGRACESECONDS"], o.ShutdownGraceSeconds);

            // The two keys that were emitted by the deploy script and never read. The pump clamps
            // MaxDeliveryAttempts to what the AMQP redelivered flag can express and reports the
            // clamp, so an out-of-range value here is surfaced rather than silently corrected.
            o.MaxDeliveryAttempts =
                ConfigurationValues.ReadInt(cfg["WORKER:MAXDELIVERYATTEMPTS"], o.MaxDeliveryAttempts);
            o.RetryEngineInternalErrors =
                ConfigurationValues.ReadBool(cfg["WORKER:RETRYENGINEINTERNALERRORS"], o.RetryEngineInternalErrors);

            // ── Broker ──────────────────────────────────────────────────────────
            // Tri-state: unset leaves the decision to the source .bite (plan §1.7), so blank must
            // stay null rather than collapsing onto false.
            var useSsl = cfg["RABBITMQ:USESSL"];
            o.UseSsl = string.IsNullOrWhiteSpace(useSsl) ? null : ConfigurationValues.ReadBool(useSsl);

            // ── Key Vault ───────────────────────────────────────────────────────
            o.KeyVaultName = cfg["KEYVAULT:NAME"];
            o.KeyVaultSecretName = cfg["KEYVAULT:SECRETNAME"];
            o.DebugKeyVaultSecret = cfg["DEBUG_AZURE_KEYVAULT_SECRET"];
            o.IsDevelopment = isDevelopment;
        }
    }
}
