/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Messaging;

namespace Warewolf.Execution.QueueProcessor.Hosting
{
    /// <summary>
    /// Owns the replica's lifetime and the graceful drain (plan §2.6.1).
    ///
    /// <para><b>Why the host owns lifetime.</b> On-prem, <c>QueueWorker.exe</c> called
    /// <c>StartConsuming</c> and then simply returned from <c>Run()</c>
    /// (<c>QueueWorker/Program.cs:136-140</c>) — the process stayed alive only because the
    /// RabbitMQ client kept a non-background thread running. That implicit lifetime is fragile
    /// in a container, where SIGTERM must be observed and in-flight work drained.</para>
    ///
    /// <para>ACA sends SIGTERM then SIGKILL after <c>terminationGracePeriodSeconds</c>, and it
    /// does so on <b>every</b> scale-in and every revision rollout — not just on failure. So the
    /// drain here is the difference between routine, silent duplicate executions and none.</para>
    /// </summary>
    public sealed class QueueConsumerService : BackgroundService
    {
        const string ExecutionId = "QueueProcessor-Host";

        readonly IMessagePump _pump;
        readonly ResolvedQueueConfiguration _config;
        readonly QueueProcessorOptions _options;
        readonly IHostApplicationLifetime _lifetime;

        public QueueConsumerService(
            IMessagePump pump,
            ResolvedQueueConfiguration config,
            IOptions<QueueProcessorOptions> options,
            IHostApplicationLifetime lifetime)
        {
            _pump = pump ?? throw new ArgumentNullException(nameof(pump));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _pump.StartAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Dev2Logger.Fatal(
                    $"Failed to start consuming '{_config.QueueName}'. The replica will exit so the " +
                    "platform can restart it.", ex, ExecutionId);

                // Exit non-zero-ish semantics: stopping the application lets ACA restart the
                // replica rather than leaving a live container that consumes nothing.
                _lifetime.StopApplication();
                return;
            }

            // Park until shutdown is requested; the pump does the work on broker callbacks.
            var completion = new TaskCompletionSource();
            using var registration = stoppingToken.Register(() => completion.TrySetResult());
            await completion.Task.ConfigureAwait(false);

            Dev2Logger.Info(
                $"Shutdown signalled for '{_config.QueueName}'. Draining up to " +
                $"{_options.ShutdownGraceSeconds}s ({_pump.InFlight} in flight).", ExecutionId);
        }

        /// <summary>
        /// Runs after <see cref="ExecuteAsync"/> observes cancellation. The drain must complete
        /// here, inside the host's own shutdown, so the process does not exit while messages are
        /// still being processed.
        /// </summary>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            var grace = TimeSpan.FromSeconds(_options.ShutdownGraceSeconds);

            try
            {
                var stranded = await _pump.DrainAsync(grace).ConfigureAwait(false);
                if (stranded == 0)
                {
                    Dev2Logger.Info($"Drained '{_config.QueueName}' with no stranded messages.", ExecutionId);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error draining '{_config.QueueName}'.", ex, ExecutionId);
            }
            finally
            {
                await _pump.DisposeAsync().ConfigureAwait(false);
            }

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
