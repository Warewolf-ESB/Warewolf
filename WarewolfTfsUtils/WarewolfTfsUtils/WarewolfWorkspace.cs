using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace WarewolfTfsUtils
{
    public class WarewolfWorkspace
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
        public string FetchWorkspace(string server, string project, string workspaceName, string workingDirectory,
                                     string userName, string password)
        {

            SafeTokenHandle safeTokenHandle;


            if (!string.IsNullOrEmpty(userName))
            {
                bool loginOk = LogonUser(userName, "Dev2", password, LOGON32_LOGON_INTERACTIVE,
                                         LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);


                if (loginOk)
                {
                    using (safeTokenHandle)
                    {

                        WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            // Do the operation here

                            return InvokeOperation(server, project, workspaceName, workingDirectory);

                            impersonatedUser.Undo(); // remove impersonation now
                        }
                    }
                }
            }

            // non-impersonated 
            return InvokeOperation(server, project, workspaceName, workingDirectory);
        }

        private string InvokeOperation(string server, string project, string workspaceName, string workingDirectory)
        {
            StringBuilder sb = new StringBuilder("");

            Workspace workspace = null;

            try
            {

                TeamFoundationServer tfs = TeamFoundationServerFactory.GetServer(server);

                tfs.EnsureAuthenticated();

                VersionControlServer vcs = (VersionControlServer) tfs.GetService(typeof (VersionControlServer));

                // clean up ;)
                Workspace[] workspaces = vcs.QueryWorkspaces(workspaceName, vcs.AuthenticatedUser,
                                                             Workstation.Current.Name);
                if (workspaces.Length > 0)
                {
                    vcs.DeleteWorkspace(workspaceName, vcs.AuthenticatedUser);
                }

                workspace = vcs.CreateWorkspace(workspaceName, vcs.AuthenticatedUser, "Temporary Workspace");

                workspace.Map(project, workingDirectory);
                GetRequest request = new GetRequest(new ItemSpec(project, RecursionType.Full), VersionSpec.Latest);
                GetStatus status = workspace.Get(request, GetOptions.GetAll | GetOptions.Overwrite);
                // this line doesn't do anything - no failures or errors

                if (status.GetFailures().Length > 0)
                {
                    sb.Append("<TfsStatusMsg>");
                    foreach (var s in status.GetFailures())
                    {
                        sb.Append(s.Message + " ");
                    }
                    sb.Append("</TfsStatusMsg>");
                }
                else
                {
                    sb.Append("<TfsStatusMsg>Ok</TfsStatusMsg>");
                }

            }
            catch (Exception e)
            {
                sb.Append("<TfsStatusMsg>" + e.Message + "</TfsStatusMsg>");
            }
            finally
            {
                if (workspace != null)
                {
                    workspace.Delete();

                }
            }

            return sb.ToString();
        }
    }
}
