/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Hangfire;
using Warewolf.Driver.Persistence;

namespace Warewolf.Execution.EngineJobProcessor.Services;

/// <summary>
/// Lazily builds the shared Hangfire <see cref="JobStorage"/> +
/// <see cref="IBackgroundJobClient"/> from <c>Config.Persistence</c> (hydrated at cold
/// start by <c>PersistenceConfigLoader</c> from <c>Settings/persistencesettings.json</c>
/// + <c>Settings/persistencesettingsdbsource.bite</c>).
///
/// Connection resolution is delegated to <see cref="HangfireStorageFactory"/> — the single
/// source shared with the Execution Engine — so processor and engine always target the same
/// Hangfire schema (Dev2JsonSerializer payload → DbSource → ConnectionString).
///
/// No <c>BackgroundJobServer</c> is ever started here — the processor only READS due
/// jobs and WRITES state transitions; execution happens on the Execution Engine.
/// </summary>
public sealed class JobStorageProvider
{
    readonly Lazy<JobStorage> _storage;
    readonly Lazy<IBackgroundJobClient> _client;

    public JobStorageProvider()
    {
        _storage = new Lazy<JobStorage>(HangfireStorageFactory.BuildFromPersistenceConfig, LazyThreadSafetyMode.ExecutionAndPublication);
        _client = new Lazy<IBackgroundJobClient>(() => new BackgroundJobClient(_storage.Value), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>Test seam: inject a pre-built storage + client (unit tests only).</summary>
    internal JobStorageProvider(JobStorage storage, IBackgroundJobClient client)
    {
        _storage = new Lazy<JobStorage>(() => storage);
        _client = new Lazy<IBackgroundJobClient>(() => client);
    }

    public JobStorage Storage => _storage.Value;

    public IBackgroundJobClient Client => _client.Value;
}
