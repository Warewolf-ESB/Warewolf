using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using System.IO;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformIntegerIOOperation
    {
        public static bool FileExist(IActivityIOPath path, IFile fileWrapper) => fileWrapper.Exists(path.Path);       
        public abstract int ExecuteOperationWithAuth(Stream src, IActivityIOPath dst);
        public abstract int ExecuteOperation();
    }
}
