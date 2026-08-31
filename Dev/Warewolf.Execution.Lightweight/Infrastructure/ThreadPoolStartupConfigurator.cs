using System;
using System.Threading;

namespace Warewolf.Execution.Lightweight.Infrastructure
{
    /// <summary>
    /// Raises the CLR ThreadPool's minimum thread counts at startup so a burst of concurrent
    /// Service Bus deliveries doesn't have to wait on the pool's default slow thread-injection
    /// rate before it can even schedule the lightweight continuations (lock renewal,
    /// <c>Task.Delay</c> timeout races, telemetry callbacks) that sit alongside the heavier
    /// blocking work <c>ServiceBusWorkflowTriggerFunction</c> runs via <c>Task.Run</c>.
    ///
    /// <para>
    /// WOLF-8512: the 2026-08-30/31 ShovelBridge load-test incidents left 12-16 correlation ids
    /// permanently stuck with a claim but no result, no dead-letter, and no released claim even
    /// 10 minutes past the 20-minute stale-claim mark — proving the trigger's own
    /// <c>ExecutionTimeout</c> safety net (a <c>Task.WhenAny</c> race against <c>Task.Delay</c>)
    /// never got a chance to run. The working theory is that a small number of CPU-heavy
    /// concurrent executions (cold-start Roslyn compilation, synchronous ADO.NET calls) on a
    /// Consumption-plan instance can starve the pool badly enough that even that safety net's
    /// own continuation never gets scheduled. Raising the floor here is a mitigation for that
    /// starvation, not a fix for whatever is CPU/memory-heavy inside a single execution.
    /// </para>
    /// </summary>
    public static class ThreadPoolStartupConfigurator
    {
        /// <summary>
        /// Headroom multiplier over <c>maxConcurrentExecutions</c> for worker threads: one
        /// thread per concurrently-running blocking execution is not enough on its own, since
        /// the same pool also has to schedule every other continuation running alongside them.
        /// A reasoned starting point, not a profiled number — revisit once a load test run has
        /// real telemetry (thanks to the flush fix above) to check thread-pool queue length
        /// against.
        /// </summary>
        const int WorkerThreadsPerExecution = 4;

        /// <summary>
        /// Smaller multiplier for I/O completion-port threads: nothing in this trigger path does
        /// heavy asynchronous I/O completion work (the blocking calls run via <c>Task.Run</c> on
        /// worker threads, not as overlapped I/O), so completion ports need far less headroom.
        /// </summary>
        const int CompletionPortThreadsPerExecution = 2;

        /// <summary>
        /// Computes the minimum worker/completion-port thread counts this process should run
        /// with, given how many workflow executions <c>ServiceBusTriggerOptions.MaxConcurrentExecutions</c>
        /// allows to run concurrently. Pure function — no side effects, safe to unit test directly.
        /// </summary>
        public static (int Worker, int CompletionPort) ComputeMinThreads(int maxConcurrentExecutions)
        {
            var executions = Math.Max(1, maxConcurrentExecutions);
            var worker = Math.Max(executions * WorkerThreadsPerExecution, Environment.ProcessorCount * 2);
            var completionPort = Math.Max(executions * CompletionPortThreadsPerExecution, Environment.ProcessorCount);
            return (worker, completionPort);
        }

        /// <summary>
        /// Raises the process's ThreadPool minimum thread counts to at least the computed floor
        /// for <paramref name="maxConcurrentExecutions"/> — never lowers them below whatever the
        /// runtime's own defaults already are.
        /// </summary>
        public static void Configure(int maxConcurrentExecutions)
        {
            ThreadPool.GetMinThreads(out var currentWorker, out var currentCompletionPort);
            var (floorWorker, floorCompletionPort) = ComputeMinThreads(maxConcurrentExecutions);

            ThreadPool.SetMinThreads(
                Math.Max(currentWorker, floorWorker),
                Math.Max(currentCompletionPort, floorCompletionPort));
        }
    }
}
