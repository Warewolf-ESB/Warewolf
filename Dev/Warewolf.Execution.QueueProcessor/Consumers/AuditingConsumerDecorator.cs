/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Diagnostics;
using System.Text;
using Dev2.Common;
using Warewolf.Data;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Streams;

namespace Warewolf.Execution.QueueProcessor.Consumers
{
    /// <summary>
    /// Execution-history auditing around the forwarder — the container equivalent of
    /// <c>LoggingConsumerWrapper</c> (<c>QueueWorker/LoggingConsumerWrapper.cs:39-103</c>).
    ///
    /// <para>The on-prem wrapper published <c>ExecutionHistory</c> over a <b>WebSocket</b> to the
    /// Warewolf logging service (<c>NetworkLogger</c>), which is unreachable from a container.
    /// Per decision #6 the same signal is emitted through <c>Dev2Logger</c> instead, so it lands
    /// in whichever sinks the replica has configured (console → ACA log stream, and App Insights
    /// when <c>ENABLEAPPLICATIONINSIGHTS=true</c>) with the same env-var semantics as the
    /// engine.</para>
    ///
    /// <para>It never changes the outcome — a decorator, not a policy — so the failure contract
    /// stays exactly where <see cref="EngineForwarder"/> defines it. Note this differs from the
    /// on-prem wrapper, which <i>did</i> rewrite a Failed result to Success; here that mapping
    /// lives in the forwarder where it is visible alongside the dead-letter publish.</para>
    /// </summary>
    public sealed class AuditingConsumerDecorator : IConsumer
    {
        const string ExecutionId = "QueueProcessor-Audit";
        const int BodyLogLimit = 4096;

        readonly IConsumer _inner;
        readonly ResolvedQueueConfiguration _config;

        public AuditingConsumerDecorator(IConsumer inner, ResolvedQueueConfiguration config)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<ConsumerResult> Consume(byte[] body, object parameters)
        {
            var headers = parameters as Headers ?? new Headers();
            if (!headers.KeyExists("Warewolf-Execution-Id"))
            {
                headers["Warewolf-Execution-Id"] = new[] { Guid.NewGuid().ToString() };
            }

            var executionId = headers["Warewolf-Execution-Id", Array.Empty<string>()]?.FirstOrDefault()
                              ?? string.Empty;
            var customTransactionId =
                headers["Warewolf-Custom-Transaction-Id", Array.Empty<string>()]?.FirstOrDefault()
                ?? string.Empty;

            var startedUtc = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            Dev2Logger.Info(
                $"Queue execution starting. queue='{_config.QueueName}' " +
                $"workflow='{_config.WorkflowPath}' txn='{customTransactionId}' " +
                $"bytes={body.Length} body={Preview(body)}", executionId);

            try
            {
                var result = await _inner.Consume(body, headers).ConfigureAwait(false);
                stopwatch.Stop();

                if (result == ConsumerResult.Success)
                {
                    Dev2Logger.Info(
                        $"Queue execution succeeded. queue='{_config.QueueName}' " +
                        $"workflow='{_config.WorkflowPath}' txn='{customTransactionId}' " +
                        $"durationMs={stopwatch.ElapsedMilliseconds} startedUtc={startedUtc:O}",
                        executionId);
                }
                else
                {
                    Dev2Logger.Error(
                        $"Queue execution failed (message will be redelivered). " +
                        $"queue='{_config.QueueName}' workflow='{_config.WorkflowPath}' " +
                        $"txn='{customTransactionId}' durationMs={stopwatch.ElapsedMilliseconds}",
                        executionId);
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Dev2Logger.Error(
                    $"Queue execution threw. queue='{_config.QueueName}' " +
                    $"workflow='{_config.WorkflowPath}' txn='{customTransactionId}' " +
                    $"durationMs={stopwatch.ElapsedMilliseconds}", ex, executionId);
                throw;
            }
        }

        static string Preview(byte[] body)
        {
            if (body.Length == 0) return string.Empty;
            var text = Encoding.UTF8.GetString(body, 0, Math.Min(body.Length, BodyLogLimit));
            return body.Length > BodyLogLimit ? text + "...(truncated)" : text;
        }
    }
}
