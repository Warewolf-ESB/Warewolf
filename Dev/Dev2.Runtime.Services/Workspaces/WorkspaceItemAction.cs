
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.Workspaces
{
    /// <summary>
    /// Defines the possible actions on an <see cref="IWorkspaceItem"/>.
    /// </summary>
    public enum WorkspaceItemAction
    {
        /// <summary>
        /// No action - the item the server version.
        /// </summary>
        None,

        /// <summary>
        /// The item has been edited or requested for editing.
        /// </summary>
        Edit,

        /// <summary>
        /// Discard any edits and revert to server version.
        /// </summary>
        Discard,

        /// <summary>
        /// Commit any edits and overwrite server version.
        /// </summary>
        Commit
    }
}
