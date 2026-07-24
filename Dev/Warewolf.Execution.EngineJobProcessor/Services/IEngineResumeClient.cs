/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.EngineJobProcessor.Services;

/// <summary>
/// Dispatch seam for <see cref="Functions.JobPollFunction"/> — implemented by
/// <see cref="EngineResumeClient"/>, substituted in unit tests.
/// </summary>
public interface IEngineResumeClient
{
    /// <summary>
    /// Fire-and-forget dispatch of <paramref name="jobId"/> to the Execution Engine's
    /// resume route; awaits only the claim acknowledgment. Must never throw — failures
    /// map to <see cref="ResumeDispatchOutcome.Failed"/>.
    /// </summary>
    Task<ResumeDispatchOutcome> TryResumeAsync(string jobId, CancellationToken cancellationToken);
}
