/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;

namespace Warewolf.Driver.Persistence
{
    /// <summary>
    /// Optional host seam: lets the hosting process add its own keys to the job values
    /// persisted by <c>HangfireScheduler.ScheduleJob</c> — without changing
    /// <c>SuspendExecutionActivity</c> or the five legacy keys (<c>resourceID</c>,
    /// <c>environment</c>, <c>startActivityId</c>, <c>versionNumber</c>,
    /// <c>currentuserprincipal</c>).
    ///
    /// The Azure Execution Engine registers an implementation into
    /// <c>CustomContainer</c> that stamps engine-resume metadata (workflow name/path,
    /// execution id, suspend timestamp). The on-prem Server registers nothing and is
    /// unaffected. Implementations must only ADD keys; existing keys must not be
    /// modified or removed.
    /// </summary>
    public interface IJobValuesEnricher
    {
        /// <summary>Adds host-specific keys to <paramref name="values"/> before the job is created.</summary>
        void Enrich(Dictionary<string, StringBuilder> values);
    }
}
