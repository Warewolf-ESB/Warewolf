

namespace Dev2.Workspaces
{
    /// <summary>
    /// Defines the requirements for a workspace.
    /// </summary>
    public partial interface IWorkspace
    {

        /// <summary>
        /// Performs the <see cref="IWorkspaceItem.Action" /> on the specified workspace item.
        /// </summary>
        /// <param name="workspaceItem">The workspace item to be actioned.</param>
        /// <param name="roles">The roles.</param>
        /// <exception cref="System.ArgumentNullException">workspaceItem</exception>
        void Update(IWorkspaceItem workspaceItem, bool isLocalSave, string roles = null);

        /// <summary>
        /// Saves the specified object - does not load into.
        /// </summary>
        /// <param name="objectXml">The object's definition XML.</param>
        /// <param name="roles">The roles.</param>
        /// <exception cref="System.ArgumentNullException">objectXml</exception>
        string Save(string objectXml, string roles = null);
    }
}
