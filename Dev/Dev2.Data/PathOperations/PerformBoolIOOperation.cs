using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Operations;
using Dev2.PathOperations;
using System.IO;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformBoolIOOperation : ValidateAuthorization
    {
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
        public static DoDeleteOperation RequiresOverwrite(IDev2CRUDOperationTO arg, IActivityIOPath path, IDev2LogonProvider dev2LogonProvider)
         => arg.Overwrite ? new DoDeleteOperation(path, dev2LogonProvider) : null;

        public static bool FileExist(IActivityIOPath path, IFile fileWrapper) => fileWrapper.Exists(path.Path);
        public static bool DirectoryExist(IActivityIOPath path, IDirectory dirWrapper) => dirWrapper.Exists(path.Path);        
        
        public abstract bool ExecuteOperation();
        public abstract bool ExecuteOperationWithAuth();

    }
}
