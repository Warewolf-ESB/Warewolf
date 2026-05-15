/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// (POL-08) Hosted background service that watches the active
/// <c>secure.config</c> file for changes and triggers a thread-safe reload of
/// <see cref="SecureConfigLoader"/> + <see cref="IWorkflowAuthPolicyLoader"/>.
///
/// <para>
/// Cost note: a single <see cref="FileSystemWatcher"/> is shared for the
/// lifetime of the function host and is suspended while no events fire — the
/// runtime overhead is effectively zero outside of actual policy edits, which
/// keeps Azure Functions consumption-plan execution time minimal.
/// </para>
///
/// <para>
/// Reload events are debounced so the typical "save = multiple file events"
/// pattern produces exactly one reload.
/// </para>
/// </summary>
internal sealed class SecureConfigWatcher : IHostedService, IDisposable
{
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(500);

    private readonly IWorkflowAuthPolicyLoader _policyLoader;
    private readonly ILogger<SecureConfigWatcher> _logger;

    private FileSystemWatcher? _watcher;
    private Timer? _debounce;
    private int _disposed;

    public SecureConfigWatcher(
        IWorkflowAuthPolicyLoader policyLoader,
        ILogger<SecureConfigWatcher> logger)
    {
        _policyLoader = policyLoader;
        _logger       = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var path = ResolveConfigPath();
        if (path is null || !File.Exists(path))
        {
            _logger.LogInformation(
                "SecureConfigWatcher: no secure.config to watch — skipping hot-reload.");
            return Task.CompletedTask;
        }

        var dir  = Path.GetDirectoryName(path)!;
        var file = Path.GetFileName(path);

        _debounce = new Timer(_ => SafeReload(), null, Timeout.Infinite, Timeout.Infinite);

        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Renamed += OnChanged;

        _logger.LogInformation(
            "SecureConfigWatcher: monitoring {Path} for hot-reload.", path);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
        => _debounce?.Change(DebounceWindow, Timeout.InfiniteTimeSpan);

    private void SafeReload()
    {
        try
        {
            SecureConfigLoader.Reload();
            _policyLoader.Reload();
            _logger.LogInformation("SecureConfigWatcher: secure.config reloaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SecureConfigWatcher: hot-reload failed — keeping previous policy.");
        }
    }

    private static string? ResolveConfigPath()
    {
        var env = Environment.GetEnvironmentVariable("WAREWOLF_SECURE_CONFIG");
        if (!string.IsNullOrWhiteSpace(env))
            return env;

        var bin = Path.Combine(AppContext.BaseDirectory, "secure.config");
        return File.Exists(bin) ? bin : null;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnChanged;
            _watcher.Renamed -= OnChanged;
            _watcher.Dispose();
        }

        _debounce?.Dispose();
    }
}
