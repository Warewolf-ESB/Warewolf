/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Thrown by <see cref="X6ToWorkflowConverter.X6JsonToModel"/> when the supplied X6 graph
    /// has no node that resolves to a flowchart start node (either the graph has no cells at all,
    /// or none of its cells are recognised as a start node), so there is no flowchart to write.
    /// </summary>
    public class EmptyWorkflowGraphException : Exception
    {
        public EmptyWorkflowGraphException()
            : base("The workflow graph has no reachable start node; a Flowchart could not be finalised.")
        {
        }

        public EmptyWorkflowGraphException(string message)
            : base(message)
        {
        }
    }
}
