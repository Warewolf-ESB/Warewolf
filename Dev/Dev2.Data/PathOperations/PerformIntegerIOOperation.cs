using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using System.IO;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformIntegerIOOperation
    {
        public static bool FileExist(IActivityIOPath path, IFile fileWrapper) => fileWrapper.Exists(path.Path);
        public static SafeTokenHandle DoLogOn(IDev2LogonProvider dev2Logon, IActivityIOPath path) => dev2Logon.DoLogon(path);
        public static SafeTokenHandle RequiresAuth(IActivityIOPath destination, IDev2LogonProvider dev2LogonProvider) => string.IsNullOrEmpty(destination.Username) ? null : DoLogOn(dev2LogonProvider, destination);
        public abstract int ExecuteOperationWithAuth(Stream src, IActivityIOPath dst);
        public abstract int ExecuteOperation();
    }
}
