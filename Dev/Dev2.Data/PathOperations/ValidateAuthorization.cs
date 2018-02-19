using System.Security.Principal;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;



namespace Dev2.Data.PathOperations
{
    public static class ValidateAuthorization
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
    }
}
