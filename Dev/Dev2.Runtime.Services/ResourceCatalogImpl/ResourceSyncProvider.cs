using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Runtime.Interfaces;

namespace Dev2.Runtime.ResourceCatalogImpl
{
    internal class ResourceSyncProvider: IResourceSyncProvider
    {
        #region Implementation of IResourceSyncProvider

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath) => SyncTo(sourceWorkspacePath, targetWorkspacePath, true, true, null);

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite) => SyncTo(sourceWorkspacePath, targetWorkspacePath, overwrite, true, null);

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite, bool delete) => SyncTo(sourceWorkspacePath, targetWorkspacePath, overwrite, delete, null);

        public void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite, bool delete, IList<string> filesToIgnore)
        {
            if (filesToIgnore == null)
            {
                filesToIgnore = new List<string>();
            }
            var source = new DirectoryInfo(sourceWorkspacePath);
            var destination = new DirectoryInfo(targetWorkspacePath);

            if (!source.Exists)
            {
                return;
            }

            if (!destination.Exists)
            {
                destination.Create();
            }

            //
            // Get the files from the source and desitnations folders, excluding the files which are to be ignored
            //
            var sourceFiles = source.GetFiles().Where(f => !filesToIgnore.Contains(f.Name)).ToList();
            var destinationFiles = destination.GetFiles().Where(f => !filesToIgnore.Contains(f.Name)).ToList();

            //
            // Calculate the files which are to be copied from source to destination, this respects the override parameter
            //

            var filesToCopyFromSource = new List<FileInfo>();

            if (overwrite)
            {
                filesToCopyFromSource.AddRange(sourceFiles);
            }
            else
            {
                filesToCopyFromSource.AddRange(sourceFiles
                    
                    .Where(sf => !destinationFiles.Any(df => string.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                
            }

            //
            // Calculate the files which are to be deleted from the destination, this respects the delete parameter
            //
            
            var filesToDeleteFromDestination = new List<FileInfo>();
            if (delete)
            {
                filesToDeleteFromDestination.AddRange(destinationFiles
                    
                    .Where(sf => !sourceFiles.Any(df => string.Compare(df.Name, sf.Name, StringComparison.OrdinalIgnoreCase) == 0)));
                
            }

            //
            // Copy files from source to desination
            //
            foreach (var file in filesToCopyFromSource)
            {
                file.CopyTo(Path.Combine(destination.FullName, file.Name), true);
            }
        }

        #endregion
    }
}