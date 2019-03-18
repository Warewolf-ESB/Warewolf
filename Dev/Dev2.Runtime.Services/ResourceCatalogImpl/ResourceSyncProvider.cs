#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Runtime.Interfaces;

namespace Dev2.Runtime.ResourceCatalogImpl
{
    class ResourceSyncProvider: IResourceSyncProvider
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