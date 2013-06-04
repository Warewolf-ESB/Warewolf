
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
