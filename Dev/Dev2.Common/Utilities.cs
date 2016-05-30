using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Dev2.Common
{
    public static class Utilities
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            var second = e as IList<T> ?? e.ToList();
            return second.SelectMany(c => f(c).Flatten(f)).Concat(second);
        }
        public static void PerformActionInsideImpersonatedContext(IPrincipal userPrinciple, Action actionToBePerformed)
        {
            if(userPrinciple == null)
            {
                actionToBePerformed();
            }
            else
            {
                WindowsIdentity identity = userPrinciple.Identity as WindowsIdentity;
                WindowsImpersonationContext impersonationContext = null;
                if(identity != null)
                {
                    if(identity.IsAnonymous)
                    {
                        identity = ServerUser.Identity as WindowsIdentity;
                    }
                    if(identity != null)
                    {
                        impersonationContext = identity.Impersonate();
                    }
                }
                try
                {
                    actionToBePerformed();
                }
                catch(Exception)
                {
                    if(impersonationContext != null)
                    {
                        impersonationContext.Undo();
                    }
                    identity = ServerUser.Identity as WindowsIdentity;
                    if(identity != null)
                    {
                        impersonationContext = identity.Impersonate();
                    }
                    try
                    {
                        actionToBePerformed();
                    }
                    catch(Exception)
                    {
                        //Ignore
                    }
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

        public static IPrincipal ServerUser
        {
            get;
            set;
        }
    }
}