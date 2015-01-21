using System;
using Dev2.Common.Interfaces.Explorer;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    /// <summary>
    /// Explorer update manager
    /// </summary>
    public interface IExplorerUpdateManager
    {
        /// <summary>
        /// Add a folder to a warewolf server
        /// </summary>
        /// <param name="path">relative path</param>
        void AddFolder(string path);
        /// <summary>
        /// delete a folder from a warewolf server
        /// </summary>
        /// <param name="path">the folder path</param>
        void DeleteFolder(string path);
        /// <summary>
        /// delete a resource from a warewolf server
        /// </summary>
        /// <param name="id">resource id</param>
        void DeleteResource(Guid id);
        /// <summary>
        /// Rename a resource
        /// </summary>
        /// <param name="id">the resource id</param>
        /// <param name="newName">the new name</param>
        void Rename(Guid id, string newName);
        /// <summary>
        /// Move a resource to another folder
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newPath"></param>
        void MoveItem(Guid id, string newPath);

    }
}