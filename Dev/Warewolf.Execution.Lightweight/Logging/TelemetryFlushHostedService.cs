using System;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Flushes buffered Application Insights telemetry when the host begins stopping.
    ///
    /// The AI SDK's default channel batches telemetry and transmits it on its own periodic
    /// schedule (tens of seconds). On a Consumption-plan Functions app under bursty load, an
    /// instance can scale in and be recycled well inside that window, discarding every
    /// buffered trace/exception/dependency with no error and no warning — this was confirmed
    /// live during the 2026-08-30/31 ShovelBridge load tests (WOLF-8512), which produced zero
    /// telemetry for the entire ~30-minute run window despite correct AI configuration.
    /// <see cref="TelemetryClient.Flush"/> only queues a send; it is not guaranteed to complete
    /// before the method returns, so a short bounded wait after calling it is required to give
    /// the transmission an actual chance to leave the process before shutdown continues.
    /// </summary>
    public sealed class TelemetryFlushHostedService : IHostedService
    {
        static readonly TimeSpan DefaultFlushWait = TimeSpan.FromSeconds(5);

        readonly TelemetryClient _telemetryClient;
        readonly TimeSpan _flushWait;

        public TelemetryFlushHostedService(TelemetryClient telemetryClient, TimeSpan? flushWait = null)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _flushWait = flushWait ?? DefaultFlushWait;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _telemetryClient.Flush();
                // Deliberately NOT the caller's cancellationToken: shutdown already requested it,
                // so honouring it here would skip the wait entirely and reopen the exact gap this
                // service exists to close.
                await Task.Delay(_flushWait, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn($"TelemetryFlushHostedService: flush on shutdown failed: {ex.Message}", GlobalConstants.WarewolfWarn);
            }
        }
    }
}
