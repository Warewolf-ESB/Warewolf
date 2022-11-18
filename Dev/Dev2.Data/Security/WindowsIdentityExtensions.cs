using System;
using System.Security.Principal;



namespace Dev2.Data.Security
{
    public static class WindowsIdentityExtensions
    {
        public static WindowsIdentity Impersonate(this WindowsIdentity identity)
        {
            try
            {
                identity.RunImpersonated(() => { });
                return identity;
            }
            catch
            {
                return null;
            }
        }


        public static WindowsIdentity RunImpersonated(this WindowsIdentity identity, Action action)
        {
            try
            {
                WindowsIdentity.RunImpersonated(identity.AccessToken, action);
                return identity;
            }
            catch
            {
                return null;
            }
        }

        public static T RunImpersonated<T>(this WindowsIdentity identity, Func<T> action)
        {
            try
            {
                return WindowsIdentity.RunImpersonated<T>(identity.AccessToken, action);

            }
            catch
            {
                return default(T);
            }
        }
    }
}
