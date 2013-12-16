using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace Tfs.Squish
{
   
    class Program
    {
        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
                                            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        private const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 
        private const int LOGON32_LOGON_INTERACTIVE = 2;


        // Not so nice to use this method, but darn MS changed track from 2010 to 2012 and I cannot find a working 2012 auth via API method.
        // You can always trust un-managed code to get around your issues ;)
        // AND yes it is fixed to auth against the Dev2 domain.
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void FetchAnnotationData(string server, string fileName, string userName = "IntegrationTester", string password = "I73573r0")
        {
            if(!string.IsNullOrEmpty(userName))
            {
                SafeTokenHandle safeTokenHandle;
                bool loginOk = LogonUser(userName, "Dev2", password, LOGON32_LOGON_INTERACTIVE,
                                         LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                if(loginOk)
                {
                    using(safeTokenHandle)
                    {

                        WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using(WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            // Do the operation here

                            TfsAnnotate tfsAnn = new TfsAnnotate(server);

                            tfsAnn.FetchAnnotateInfo(fileName, false);    

                            impersonatedUser.Undo(); // remove impersonation now
                        }
                    }
                }
            }
        }


        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                Console.WriteLine("Usage : tfs-squish [serverURI] [filePath]");
                foreach (var s in args)
                {
                    Console.WriteLine("Arg-> " + s);
                }

            }else if (args.Length == 2)
            {
                var serverURI = args[0];
                var fileName = args[1];
                FetchAnnotationData(serverURI, fileName);
            }
        }
    }
}
