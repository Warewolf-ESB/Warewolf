using System;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Common.Interfaces
{
    /// <summary>
    /// Explorer update manager
    /// </summary>
    public interface IExplorerUpdateManager
    {
        /// <summary>
        /// Add a folder to a warewolf server
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        void AddFolder(string path, string name, Guid id);
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
        /// Rename a folder
        /// </summary>
        /// <param name="path">the folder path</param>
        /// <param name="newName">the new name</param>
        void RenameFolder(string path, string newName);

        /// <summary>
        /// Move a resource to another folder
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        /// <param name="itemPath"></param>
        Task<IExplorerRepositoryResult> MoveItem(Guid sourcePath, string destinationPath,string itemPath);

    }
}