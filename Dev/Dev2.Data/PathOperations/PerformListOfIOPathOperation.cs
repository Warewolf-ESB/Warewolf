
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformListOfIOPathOperation : ValidateAuthorization
    {
        public abstract IList<IActivityIOPath> ExecuteOperation();
        public abstract IList<IActivityIOPath> ExecuteOperationWithAuth();
        public static bool DirectoryExist(IActivityIOPath path, IDirectory dirWrapper) => dirWrapper.Exists(path.Path);
        public static bool FileExist(IActivityIOPath path, IFile fileWrapper) => fileWrapper.Exists(path.Path);
        public static enPathType PathIs(IActivityIOPath path, IFile fileWrapper, IDirectory dirWrapper)
        {
            if (Dev2ActivityIOPathUtils.IsDirectory(path.Path))
            {
                return enPathType.Directory;
            }

            if ((FileExist(path, fileWrapper) || DirectoryExist(path, dirWrapper))
                && !Dev2ActivityIOPathUtils.IsStarWildCard(path.Path))
            {
                var fa = fileWrapper.GetAttributes(path.Path);
                if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return enPathType.Directory;
                }
            }
            return enPathType.File;
        }
        public static string AppendBackSlashes(IActivityIOPath path, IFile fileWrapper, IDirectory dirWrapper)
        {
            var newPath = path.Path;
            if (!path.Path.EndsWith("\\", System.StringComparison.Ordinal) && PathIs(path, fileWrapper, dirWrapper) == enPathType.Directory)
            {
                newPath = path.Path + "\\";
            }
            return newPath;
        }

        public static IList<IActivityIOPath> AddDirsToResults(IEnumerable<string> dirsToAdd, IActivityIOPath path)
        {
            IList<IActivityIOPath> results = new List<IActivityIOPath>();
            if (dirsToAdd != null)
            {
                foreach (string d in dirsToAdd)
                {
                    results.Add(ActivityIOFactory.CreatePathFromString(d, path.Username, path.Password, true, path.PrivateKeyFile));
                }
            }
            return results;
        }
       public static IEnumerable<string> GetDirectoriesForType(string path, string pattern, ReadTypes type, IDirectory dirWrapper)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                switch (type)
                {
                    case ReadTypes.Files:
                        return dirWrapper.EnumerateFiles(path);
                    case ReadTypes.Folders:
                        return dirWrapper.EnumerateDirectories(path);
                    default:
                        return dirWrapper.EnumerateFileSystemEntries(path);
                }
            }
            switch (type)
            {
                case ReadTypes.Files:
                    return dirWrapper.EnumerateFiles(path, pattern);
                case ReadTypes.Folders:
                    return dirWrapper.EnumerateDirectories(path, pattern);
                default:
                    return dirWrapper.EnumerateFileSystemEntries(path, pattern);
            }
        }
    }
}
