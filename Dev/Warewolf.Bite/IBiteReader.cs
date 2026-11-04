/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;

namespace Warewolf.Bite
{
    /// <summary>
    /// Turns a <c>.bite</c> workflow resource into the executable <see cref="IDev2Activity"/> graph.
    /// Today's WF path (<c>ActivityXamlServices.Load</c> + <c>ActivityParser</c>) and the WF-free
    /// reader both sit behind this seam so they can be compared (parity harness, shadow mode).
    /// </summary>
    public interface IBiteReader
    {
        /// <summary>Returns the start node, or null when the workflow has no start node.</summary>
        IDev2Activity? Read(string bite);
    }
}
