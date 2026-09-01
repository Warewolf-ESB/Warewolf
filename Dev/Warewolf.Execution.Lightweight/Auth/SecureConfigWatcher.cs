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
        var path = ResolveExpectedConfigPath();
        if (path is null)
        {
            _logger.LogInformation(
                "SecureConfigWatcher: no secure.config path resolvable — skipping hot-reload.");
            return Task.CompletedTask;
        }

        var dir  = Path.GetDirectoryName(path);
        var file = Path.GetFileName(path);

        if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(file))
        {
            _logger.LogInformation(
                "SecureConfigWatcher: secure.config path {Path} has no directory/filename — skipping hot-reload.", path);
            return Task.CompletedTask;
        }

        // Ensure the parent directory exists so the FileSystemWatcher can attach
        // even when the secure.config file itself hasn't been written yet (CI
        // pipelines often create the directory before tests, then have each
        // test write the file later — we must still detect that Created event
        // and reload).
        try
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "SecureConfigWatcher: could not ensure directory {Dir} exists — skipping hot-reload.", dir);
            return Task.CompletedTask;
        }

        _debounce = new Timer(_ => SafeReload(), null, Timeout.Infinite, Timeout.Infinite);

        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Renamed += OnChanged;

        // If the file already exists at startup, the watcher only fires on future
        // changes — perform an immediate load so we don't run with the AllowAll
        // fallback any longer than necessary. When the file doesn't yet exist,
        // the Created event will trigger the first SafeReload.
        if (File.Exists(path))
        {
            _logger.LogInformation(
                "SecureConfigWatcher: monitoring {Path} for hot-reload (file present).", path);
            SafeReload();
        }
        else
        {
            _logger.LogInformation(
                "SecureConfigWatcher: monitoring {Path} for hot-reload (file not yet present — will load on creation).", path);
        }

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

    private static string? ResolveExpectedConfigPath()
    {
        // WOLF-8516: sourced from SecureConfigLoader's own constant rather than an independent
        // "WAREWOLF_SECURE_CONFIG" literal, so the two can never drift on the variable name.
        var env = Environment.GetEnvironmentVariable(SecureConfigLoader.ConfigPathEnvVar);
        if (!string.IsNullOrWhiteSpace(env))
            return env;

        // Fall back to the bin-side path; this is always returnable so the watcher
        // can attach to the directory even when the file hasn't been written yet.
        return Path.Combine(AppContext.BaseDirectory, "secure.config");
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
