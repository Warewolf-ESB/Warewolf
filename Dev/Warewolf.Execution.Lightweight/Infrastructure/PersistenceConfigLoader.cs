/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.IO;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Serializers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Data;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Cold-start hydration of <see cref="Config.Persistence"/> for the Azure Execution Engine.
///
/// The on-prem Server reads persistence settings from
/// <c>%AppDataPath%\Server Settings\persistencesettings.json</c> with the Hangfire SQL
/// connection embedded as an encrypted <c>DbSource</c> JSON payload. The engine instead
/// ships two deployable files next to the package (read-only safe — nothing is ever
/// written back):
///
/// <list type="bullet">
///   <item><c>Settings/persistencesettings.json</c> — Enable / scheduler / flags
///         (<see cref="Warewolf.Configuration.PersistenceSettingsData"/> shape).</item>
///   <item><c>Settings/persistencesettingsdbsource.bite</c> — a Warewolf <c>DbSource</c>
///         whose <c>ConnectionString</c> is WFAES-encrypted at deploy time, exactly like
///         <c>Settings/ElasticsearchLoggingSource.bite</c>.</item>
/// </list>
///
/// <see cref="Initialize"/> MUST run after Key Vault initialisation: the
/// <see cref="DbSource(XElement)"/> constructor decrypts the <c>ConnectionString</c>
/// through <see cref="Warewolf.Security.Encryption.DpapiWrapper.AesDecryptHook"/>.
///
/// Failure semantics (mirrors the Key Vault fail-fast policy): a missing settings file
/// means "persistence not configured" and the engine starts normally; but
/// <c>Enable=true</c> with a missing/unreadable DbSource is a configuration error and
/// throws, so the host refuses to start half-configured.
/// </summary>
internal static class PersistenceConfigLoader
{
    internal const string SettingsFileName = "persistencesettings.json";
    internal const string DbSourceFileName = "persistencesettingsdbsource.bite";

    /// <summary>
    /// Loads the persistence settings pair from <paramref name="settingsDirectory"/>
    /// (defaults to <c>{AppContext.BaseDirectory}/Settings</c>) and assigns the result to
    /// <see cref="Config.Persistence"/>. Idempotent and side-effect free on disk.
    /// </summary>
    internal static void Initialize(string settingsDirectory = null)
    {
        const string executionId = "StartupOrchestrator-Persistence";

        try
        {
            settingsDirectory ??= Path.Combine(AppContext.BaseDirectory, "Settings");
            var settingsPath = Path.Combine(settingsDirectory, SettingsFileName);

            if (!File.Exists(settingsPath))
            {
                Dev2Logger.Info(
                    $"Startup | Phase=Persistence | Status=NotConfigured | " +
                    $"'{settingsPath}' not found — suspend/resume tools will report persistence as not configured.",
                    executionId);
                return;
            }

            var settings = new LightweightPersistenceSettings(settingsPath, new FileWrapper(), new DirectoryWrapper());

            if (settings.Enable)
            {
                var dbSourcePath = Path.Combine(settingsDirectory, DbSourceFileName);
                if (!File.Exists(dbSourcePath))
                {
                    throw new InvalidOperationException(
                        $"Persistence is enabled in '{settingsPath}' but the Hangfire SQL data source " +
                        $"'{dbSourcePath}' is missing. Deploy a DbSource .bite (ConnectionString may be " +
                        "WFAES-encrypted) or set Enable=false.");
                }

                settings.SetDataSourceFromBiteFile(dbSourcePath);
            }
            else
            {
                // With persistence disabled, the scheduler name must be cleared:
                // SuspendExecutionActivity's ctor news up PersistenceExecution, whose
                // GetScheduler() constructs a HangfireScheduler (and opens SQL storage)
                // whenever PersistenceScheduler == "Hangfire" — even when Enable=false.
                // An unset scheduler makes GetScheduler() return null, so workflows
                // containing suspend/resume tools still PARSE, and executing the tool
                // reports "persistence settings not configured" — Server parity.
                settings.ClearSchedulerInMemory();
            }

            Config.Persistence = settings;

            Dev2Logger.Info(
                $"Startup | Phase=Persistence | Status=Completed | Enable={settings.Enable} | " +
                $"Scheduler={settings.PersistenceScheduler} | DataSource={settings.PersistenceDataSource?.Name ?? "(none)"}",
                executionId);
        }
        catch (Exception ex)
        {
            Dev2Logger.Fatal(
                "Startup | Phase=Persistence | Status=Failed | " +
                "Persistence is enabled but its configuration could not be loaded. " +
                "Fix Settings/persistencesettings.json + Settings/persistencesettingsdbsource.bite " +
                "(and confirm the Key Vault AES key decrypts the ConnectionString) or set Enable=false.",
                ex, executionId);
            throw; // Fail fast: a half-configured persistence layer must not serve traffic.
        }
    }
}

/// <summary>
/// <see cref="PersistenceSettings"/> variant that reads the standard JSON shape from a
/// caller-supplied path and hydrates the Hangfire SQL data source from a <c>DbSource</c>
/// <c>.bite</c> file — entirely in memory. The base-class property setters all call
/// <c>Save()</c>, which would write to the deployed (read-only) package directory, so this
/// class mutates the protected <c>_settings</c> data object directly and never persists.
/// </summary>
internal sealed class LightweightPersistenceSettings : PersistenceSettings
{
    public LightweightPersistenceSettings(string settingsPath, Dev2.Common.Interfaces.Wrappers.IFile file, Dev2.Common.Interfaces.Wrappers.IDirectory directoryWrapper)
        : base(settingsPath, file, directoryWrapper)
    {
    }

    /// <summary>
    /// Parses the <c>DbSource</c> <c>.bite</c> at <paramref name="bitePath"/> and installs
    /// it as the in-memory <c>PersistenceDataSource</c> payload in the JSON shape
    /// <c>HangfireScheduler.ConnectionString</c> deserialises
    /// (<c>Dev2JsonSerializer.Deserialize&lt;DbSource&gt;</c>).
    ///
    /// The <see cref="DbSource(XElement)"/> constructor decrypts a WFAES-encrypted
    /// <c>ConnectionString</c> via the AES hook, so the payload built here always carries
    /// plaintext — <c>EncryptDataSource</c> is forced to <c>false</c> accordingly. The
    /// plaintext exists only in process memory, protected by the platform; at rest the
    /// connection string stays encrypted inside the .bite.
    /// </summary>
    public void SetDataSourceFromBiteFile(string bitePath)
    {
        var xml = XElement.Load(bitePath);

        // Guard on the RAW attribute: DbSource.ConnectionString is a computed property
        // (rebuilt from the parsed Server/Database/Auth fields), so it is never empty —
        // an empty or undecryptable attribute would silently yield a junk data source.
        var rawConnectionString = xml.Attribute("ConnectionString")?.Value;
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException(
                $"'{bitePath}' has an empty ConnectionString attribute. Supply the Hangfire SQL " +
                "connection (plaintext or WFAES-encrypted with the engine's Key Vault AES key — " +
                "see docs/README-Encryption.md).");
        }

        var source = new DbSource(xml);

        _settings.PersistenceDataSource = new NamedGuidWithEncryptedPayload
        {
            Name = source.ResourceName,
            Value = source.ResourceID,
            Payload = new Dev2JsonSerializer().Serialize(source),
        };
        _settings.EncryptDataSource = false;
    }

    /// <summary>
    /// Clears the scheduler name in memory (never persisted). Used when persistence is
    /// disabled so <c>PersistenceExecution.GetScheduler()</c> returns <c>null</c> instead
    /// of eagerly constructing a <c>HangfireScheduler</c> (and its SQL storage) during
    /// activity construction — which would make any workflow containing a
    /// suspend/resume tool fail to parse on an engine without a configured database.
    /// </summary>
    public void ClearSchedulerInMemory() => _settings.PersistenceScheduler = null;
}
