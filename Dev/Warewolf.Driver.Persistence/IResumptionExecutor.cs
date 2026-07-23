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
using Dev2.Interfaces;

namespace Warewolf.Driver.Persistence
{
    /// <summary>
    /// Host seam for executing a suspended workflow's continuation.
    ///
    /// The on-prem Server registers nothing: <c>HangfireScheduler</c> falls back to the
    /// in-process <c>WorkflowResume</c> management endpoint (ResourceCatalog +
    /// ServerAuthorizationService), byte-for-byte the existing behaviour.
    ///
    /// The Azure Execution Engine registers its <c>ResumptionExecutor</c> into
    /// <c>CustomContainer</c> at startup so BOTH manual-resumption paths of
    /// <c>ManualResumptionActivity</c> execute on the engine's lightweight pipeline —
    /// synchronously, preserving the activity's contract that <c>Response</c> reflects
    /// a completed continuation (its value feeds the next activity of the calling
    /// workflow).
    /// </summary>
    public interface IResumptionExecutor
    {
        /// <summary>
        /// No-override path (parity with <c>WorkflowResume.Execute</c>): runs the
        /// continuation described by the persisted job <paramref name="values"/>
        /// (already decrypted by the caller) to completion and returns a serialized
        /// <c>ExecuteMessage</c> — <c>HasError=true</c> carries the failure message.
        /// </summary>
        StringBuilder Execute(Dictionary<string, StringBuilder> values);

        /// <summary>
        /// Override path: executes the continuation synchronously against
        /// <paramref name="dsfDataObject"/>'s CURRENT (merged) environment, starting at
        /// <c>dsfDataObject.StartActivityId</c>, resolving the suspended workflow from
        /// the persisted job <paramref name="values"/>. Resets <c>StartActivityId</c>
        /// when done. Exceptions propagate to the caller
        /// (<c>ManualResumptionActivity</c>'s error handling).
        /// </summary>
        void ExecuteOverrideContinuation(IDSFDataObject dsfDataObject, Dictionary<string, StringBuilder> values);
    }
}
