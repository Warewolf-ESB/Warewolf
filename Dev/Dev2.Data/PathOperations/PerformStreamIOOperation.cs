
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using System.IO;
using System.Security.Principal;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformStreamIOOperation
    {        
        public static SafeTokenHandle DoLogOn(IDev2LogonProvider dev2Logon, IActivityIOPath path) => dev2Logon.DoLogon(path);
        public static WindowsImpersonationContext RequiresAuth(IActivityIOPath path, IDev2LogonProvider dev2LogonProvider)
        {
            var safeToken = string.IsNullOrEmpty(path.Username) ? null : DoLogOn(dev2LogonProvider, path);
            if (safeToken != null)
            {
                using (safeToken)
                {
                    var newID = new WindowsIdentity(safeToken.DangerousGetHandle());
                    return newID.Impersonate();
                }
            }
            return null;
        }
        public abstract Stream ExecuteOperationWithAuth();
        public abstract Stream ExecuteOperation();
    }
}
