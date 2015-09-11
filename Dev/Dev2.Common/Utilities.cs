using System;
using System.Security.Principal;

namespace Dev2.Common
{
    public static class Utilities
    {
        public static void PerformActionInsideImpersonatedContext(IPrincipal userPrinciple, Action actionToBePerformed)
        {
            WindowsIdentity identity = userPrinciple.Identity as WindowsIdentity;
            WindowsImpersonationContext impersonationContext = null;
            if(identity != null)
            {
                impersonationContext = identity.Impersonate();
            }
            try
            {
                actionToBePerformed();
            }
            finally
            {
                if(impersonationContext != null)
                {
                    impersonationContext.Undo();
                }
            }
        }
    }
}