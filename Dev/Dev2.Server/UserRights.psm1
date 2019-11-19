<#  -- UserRights.psm1 --

The latest version of this script is available at https://gallery.technet.microsoft.com/Grant-Revoke-Query-user-26e259b0

VERSION   DATE          AUTHOR
1.0       2015-03-10    Tony Pombo
    - Initial Release

1.1       2015-03-11    Tony Pombo
    - Added enum Rights, and configured functions to use it
    - Fixed a couple typos in the help

1.2       2015-11-13    Tony Pombo
    - Fixed exception in LsaWrapper.EnumerateAccountsWithUserRight when SID cannot be resolved
    - Added Grant-TokenPrivilege
    - Added Revoke-TokenPrivilege

1.3       2016-10-29    Tony Pombo
    - Minor changes to support Nano server
    - SIDs can now be specified for all account parameters
    - Script is now digitally signed

1.3.1     2017-02-16    Tony Pombo
    - Fixed dash/hyphen typo in Get-AccountsWithUserRight and Get-UserRightsGrantedToAccount

2.0.03    2018-02-23    Tony Pombo
    - Added missing SeDelegateSessionUserImpersonatePrivilege privilege
    - Get-AccountsWithUserRight now returns SIDs in addition to Account names
        - Behavior Change: 'Account' will be blank if SID cannot be resolved to a name
    - Packaged as a module file

2.1.07    2018-04-25    Tony Pombo
    - Modified the output of Get-AccountsWithUserRight and Get-UserRightsGrantedToAccount to be more pipeline-able
        - Behavior Change: Output is now an array of objects instead of an object with array members
    - Added -WhatIf support to Grant-UserRight and Revoke-UserRight
    - Added -SidForUnresolvedName parameter to Get-AccountsWithUserRight

#> # Revision History

Set-StrictMode -Version 2.0
#Requires -Version 3.0

#### Internal Variables and Functions #############################################################

Add-Type -TypeDefinition @'
using System;
namespace PS_LSA
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using LSA_HANDLE = IntPtr;

    public enum Rights
    {
        SeTrustedCredManAccessPrivilege,             // Access Credential Manager as a trusted caller
        SeNetworkLogonRight,                         // Access this computer from the network
        SeTcbPrivilege,                              // Act as part of the operating system
        SeMachineAccountPrivilege,                   // Add workstations to domain
        SeIncreaseQuotaPrivilege,                    // Adjust memory quotas for a process
        SeInteractiveLogonRight,                     // Allow log on locally
        SeRemoteInteractiveLogonRight,               // Allow log on through Remote Desktop Services
        SeBackupPrivilege,                           // Back up files and directories
        SeChangeNotifyPrivilege,                     // Bypass traverse checking
        SeSystemtimePrivilege,                       // Change the system time
        SeTimeZonePrivilege,                         // Change the time zone
        SeCreatePagefilePrivilege,                   // Create a pagefile
        SeCreateTokenPrivilege,                      // Create a token object
        SeCreateGlobalPrivilege,                     // Create global objects
        SeCreatePermanentPrivilege,                  // Create permanent shared objects
        SeCreateSymbolicLinkPrivilege,               // Create symbolic links
        SeDebugPrivilege,                            // Debug programs
        SeDenyNetworkLogonRight,                     // Deny access this computer from the network
        SeDenyBatchLogonRight,                       // Deny log on as a batch job
        SeDenyServiceLogonRight,                     // Deny log on as a service
        SeDenyInteractiveLogonRight,                 // Deny log on locally
        SeDenyRemoteInteractiveLogonRight,           // Deny log on through Remote Desktop Services
        SeEnableDelegationPrivilege,                 // Enable computer and user accounts to be trusted for delegation
        SeRemoteShutdownPrivilege,                   // Force shutdown from a remote system
        SeAuditPrivilege,                            // Generate security audits
        SeImpersonatePrivilege,                      // Impersonate a client after authentication
        SeIncreaseWorkingSetPrivilege,               // Increase a process working set
        SeIncreaseBasePriorityPrivilege,             // Increase scheduling priority
        SeLoadDriverPrivilege,                       // Load and unload device drivers
        SeLockMemoryPrivilege,                       // Lock pages in memory
        SeBatchLogonRight,                           // Log on as a batch job
        SeServiceLogonRight,                         // Log on as a service
        SeSecurityPrivilege,                         // Manage auditing and security log
        SeRelabelPrivilege,                          // Modify an object label
        SeSystemEnvironmentPrivilege,                // Modify firmware environment values
        SeDelegateSessionUserImpersonatePrivilege,   // Obtain an impersonation token for another user in the same session
        SeManageVolumePrivilege,                     // Perform volume maintenance tasks
        SeProfileSingleProcessPrivilege,             // Profile single process
        SeSystemProfilePrivilege,                    // Profile system performance
        SeUnsolicitedInputPrivilege,                 // "Read unsolicited input from a terminal device"
        SeUndockPrivilege,                           // Remove computer from docking station
        SeAssignPrimaryTokenPrivilege,               // Replace a process level token
        SeRestorePrivilege,                          // Restore files and directories
        SeShutdownPrivilege,                         // Shut down the system
        SeSyncAgentPrivilege,                        // Synchronize directory service data
        SeTakeOwnershipPrivilege                     // Take ownership of files or other objects
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LSA_OBJECT_ATTRIBUTES
    {
        internal int Length;
        internal IntPtr RootDirectory;
        internal IntPtr ObjectName;
        internal int Attributes;
        internal IntPtr SecurityDescriptor;
        internal IntPtr SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct LSA_UNICODE_STRING
    {
        internal ushort Length;
        internal ushort MaximumLength;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LSA_ENUMERATION_INFORMATION
    {
        internal IntPtr PSid;
    }

    internal sealed class Win32Sec
    {
        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint LsaOpenPolicy(
            LSA_UNICODE_STRING[] SystemName,
            ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
            int AccessMask,
            out IntPtr PolicyHandle
        );

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint LsaAddAccountRights(
            LSA_HANDLE PolicyHandle,
            IntPtr pSID,
            LSA_UNICODE_STRING[] UserRights,
            int CountOfRights
        );

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint LsaRemoveAccountRights(
            LSA_HANDLE PolicyHandle,
            IntPtr pSID,
            bool AllRights,
            LSA_UNICODE_STRING[] UserRights,
            int CountOfRights
        );

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint LsaEnumerateAccountRights(
            LSA_HANDLE PolicyHandle,
            IntPtr pSID,
            out IntPtr /*LSA_UNICODE_STRING[]*/ UserRights,
            out ulong CountOfRights
        );

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint LsaEnumerateAccountsWithUserRight(
            LSA_HANDLE PolicyHandle,
            LSA_UNICODE_STRING[] UserRights,
            out IntPtr EnumerationBuffer,
            out ulong CountReturned
        );

        [DllImport("advapi32")]
        internal static extern int LsaNtStatusToWinError(int NTSTATUS);

        [DllImport("advapi32")]
        internal static extern int LsaClose(IntPtr PolicyHandle);

        [DllImport("advapi32")]
        internal static extern int LsaFreeMemory(IntPtr Buffer);
    }

    internal sealed class Sid : IDisposable
    {
        public IntPtr pSid = IntPtr.Zero;
        public SecurityIdentifier sid = null;

        public Sid(string account)
        {
            try { sid = new SecurityIdentifier(account); }
            catch { sid = (SecurityIdentifier)(new NTAccount(account)).Translate(typeof(SecurityIdentifier)); }
            Byte[] buffer = new Byte[sid.BinaryLength];
            sid.GetBinaryForm(buffer, 0);

            pSid = Marshal.AllocHGlobal(sid.BinaryLength);
            Marshal.Copy(buffer, 0, pSid, sid.BinaryLength);
        }

        public void Dispose()
        {
            if (pSid != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pSid);
                pSid = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
        ~Sid() { Dispose(); }
    }

    public sealed class LsaWrapper : IDisposable
    {
        enum Access : int
        {
            POLICY_READ = 0x20006,
            POLICY_ALL_ACCESS = 0x00F0FFF,
            POLICY_EXECUTE = 0X20801,
            POLICY_WRITE = 0X207F8
        }
        const uint STATUS_ACCESS_DENIED = 0xc0000022;
        const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;
        const uint STATUS_NO_MEMORY = 0xc0000017;
        const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xc0000034;
        const uint STATUS_NO_MORE_ENTRIES = 0x8000001a;

        IntPtr lsaHandle;

        public LsaWrapper() : this(null) { } // local system if systemName is null
        public LsaWrapper(string systemName)
        {
            LSA_OBJECT_ATTRIBUTES lsaAttr;
            lsaAttr.RootDirectory = IntPtr.Zero;
            lsaAttr.ObjectName = IntPtr.Zero;
            lsaAttr.Attributes = 0;
            lsaAttr.SecurityDescriptor = IntPtr.Zero;
            lsaAttr.SecurityQualityOfService = IntPtr.Zero;
            lsaAttr.Length = Marshal.SizeOf(typeof(LSA_OBJECT_ATTRIBUTES));
            lsaHandle = IntPtr.Zero;
            LSA_UNICODE_STRING[] system = null;
            if (systemName != null)
            {
                system = new LSA_UNICODE_STRING[1];
                system[0] = InitLsaString(systemName);
            }

            uint ret = Win32Sec.LsaOpenPolicy(system, ref lsaAttr, (int)Access.POLICY_ALL_ACCESS, out lsaHandle);
            if (ret == 0) return;
            if (ret == STATUS_ACCESS_DENIED) throw new UnauthorizedAccessException();
            if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY)) throw new OutOfMemoryException();
            throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int)ret));
        }

        public void AddPrivilege(string account, Rights privilege)
        {
            uint ret = 0;
            using (Sid sid = new Sid(account))
            {
                LSA_UNICODE_STRING[] privileges = new LSA_UNICODE_STRING[1];
                privileges[0] = InitLsaString(privilege.ToString());
                ret = Win32Sec.LsaAddAccountRights(lsaHandle, sid.pSid, privileges, 1);
            }
            if (ret == 0) return;
            if (ret == STATUS_ACCESS_DENIED) throw new UnauthorizedAccessException();
            if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY)) throw new OutOfMemoryException();
            throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int)ret));
        }

        public void RemovePrivilege(string account, Rights privilege)
        {
            uint ret = 0;
            using (Sid sid = new Sid(account))
            {
                LSA_UNICODE_STRING[] privileges = new LSA_UNICODE_STRING[1];
                privileges[0] = InitLsaString(privilege.ToString());
                ret = Win32Sec.LsaRemoveAccountRights(lsaHandle, sid.pSid, false, privileges, 1);
            }
            if (ret == 0) return;
            if (ret == STATUS_ACCESS_DENIED) throw new UnauthorizedAccessException();
            if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY)) throw new OutOfMemoryException();
            throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int)ret));
        }

        public Rights[] EnumerateAccountPrivileges(string account)
        {
            uint ret = 0;
            ulong count = 0;
            IntPtr privileges = IntPtr.Zero;
            Rights[] rights = null;

            using (Sid sid = new Sid(account))
            {
                ret = Win32Sec.LsaEnumerateAccountRights(lsaHandle, sid.pSid, out privileges, out count);
            }
            if (ret == 0)
            {
                rights = new Rights[count];
                for (int i = 0; i < (int)count; i++)
                {
                    LSA_UNICODE_STRING str = (LSA_UNICODE_STRING)Marshal.PtrToStructure(
                        IntPtr.Add(privileges, i * Marshal.SizeOf(typeof(LSA_UNICODE_STRING))),
                        typeof(LSA_UNICODE_STRING));
                    rights[i] = (Rights)Enum.Parse(typeof(Rights), str.Buffer);
                }
                Win32Sec.LsaFreeMemory(privileges);
                return rights;
            }
            if (ret == STATUS_OBJECT_NAME_NOT_FOUND) return null;  // No privileges assigned
            if (ret == STATUS_ACCESS_DENIED) throw new UnauthorizedAccessException();
            if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY)) throw new OutOfMemoryException();
            throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int)ret));
        }

        public string[] EnumerateAccountsWithUserRight(Rights privilege, bool resolveSid = true)
        {
            uint ret = 0;
            ulong count = 0;
            LSA_UNICODE_STRING[] rights = new LSA_UNICODE_STRING[1];
            rights[0] = InitLsaString(privilege.ToString());
            IntPtr buffer = IntPtr.Zero;
            string[] accounts = null;

            ret = Win32Sec.LsaEnumerateAccountsWithUserRight(lsaHandle, rights, out buffer, out count);
            if (ret == 0)
            {
                accounts = new string[count];
                for (int i = 0; i < (int)count; i++)
                {
                    LSA_ENUMERATION_INFORMATION LsaInfo = (LSA_ENUMERATION_INFORMATION)Marshal.PtrToStructure(
                        IntPtr.Add(buffer, i * Marshal.SizeOf(typeof(LSA_ENUMERATION_INFORMATION))),
                        typeof(LSA_ENUMERATION_INFORMATION));

                        if (resolveSid) {
                            try {
                                accounts[i] = (new SecurityIdentifier(LsaInfo.PSid)).Translate(typeof(NTAccount)).ToString();
                            } catch (System.Security.Principal.IdentityNotMappedException) {
                                accounts[i] = (new SecurityIdentifier(LsaInfo.PSid)).ToString();
                            }
                        } else { accounts[i] = (new SecurityIdentifier(LsaInfo.PSid)).ToString(); }
                }
                Win32Sec.LsaFreeMemory(buffer);
                return accounts;
            }
            if (ret == STATUS_NO_MORE_ENTRIES) return null;  // No accounts assigned
            if (ret == STATUS_ACCESS_DENIED) throw new UnauthorizedAccessException();
            if ((ret == STATUS_INSUFFICIENT_RESOURCES) || (ret == STATUS_NO_MEMORY)) throw new OutOfMemoryException();
            throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int)ret));
        }

        public void Dispose()
        {
            if (lsaHandle != IntPtr.Zero)
            {
                Win32Sec.LsaClose(lsaHandle);
                lsaHandle = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
        ~LsaWrapper() { Dispose(); }

        // helper functions:
        static LSA_UNICODE_STRING InitLsaString(string s)
        {
            // Unicode strings max. 32KB
            if (s.Length > 0x7ffe) throw new ArgumentException("String too long");
            LSA_UNICODE_STRING lus = new LSA_UNICODE_STRING();
            lus.Buffer = s;
            lus.Length = (ushort)(s.Length * sizeof(char));
            lus.MaximumLength = (ushort)(lus.Length + sizeof(char));
            return lus;
        }
    }

    public sealed class TokenManipulator
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        internal const int SE_PRIVILEGE_DISABLED = 0x00000000;
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

        internal sealed class Win32Token
        {
            [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
            internal static extern bool AdjustTokenPrivileges(
                IntPtr htok,
                bool disall,
                ref TokPriv1Luid newst,
                int len,
                IntPtr prev,
                IntPtr relen
            );

            [DllImport("kernel32.dll", ExactSpelling = true)]
            internal static extern IntPtr GetCurrentProcess();

            [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
            internal static extern bool OpenProcessToken(
                IntPtr h,
                int acc,
                ref IntPtr phtok
            );

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool LookupPrivilegeValue(
                string host,
                string name,
                ref long pluid
            );

            [DllImport("kernel32.dll", ExactSpelling = true)]
            internal static extern bool CloseHandle(
                IntPtr phtok
            );
        }

        public static void AddPrivilege(Rights privilege)
        {
            bool retVal;
            int lasterror;
            TokPriv1Luid tp;
            IntPtr hproc = Win32Token.GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            retVal = Win32Token.OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            retVal = Win32Token.LookupPrivilegeValue(null, privilege.ToString(), ref tp.Luid);
            retVal = Win32Token.AdjustTokenPrivileges(htok, false, ref tp, Marshal.SizeOf(tp), IntPtr.Zero, IntPtr.Zero);
            Win32Token.CloseHandle(htok);
            lasterror = Marshal.GetLastWin32Error();
            if (lasterror != 0) throw new Win32Exception();
        }

        public static void RemovePrivilege(Rights privilege)
        {
            bool retVal;
            int lasterror;
            TokPriv1Luid tp;
            IntPtr hproc = Win32Token.GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            retVal = Win32Token.OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_DISABLED;
            retVal = Win32Token.LookupPrivilegeValue(null, privilege.ToString(), ref tp.Luid);
            retVal = Win32Token.AdjustTokenPrivileges(htok, false, ref tp, Marshal.SizeOf(tp), IntPtr.Zero, IntPtr.Zero);
            Win32Token.CloseHandle(htok);
            lasterror = Marshal.GetLastWin32Error();
            if (lasterror != 0) throw new Win32Exception();
        }
    }
}
'@ # This type (PS_LSA) is used by Grant-UserRight, Revoke-UserRight, Get-UserRightsGrantedToAccount, Get-AccountsWithUserRight, Grant-TokenPriviledge, Revoke-TokenPrivilege

function Convert-SIDtoName([String[]] $SIDs, [bool] $OnErrorReturnSID) {
    foreach ($sid in $SIDs) {
        try {
            $objSID = New-Object System.Security.Principal.SecurityIdentifier($sid) 
            $objUser = $objSID.Translate([System.Security.Principal.NTAccount]) 
            $objUser.Value
        } catch { if ($OnErrorReturnSID) { $sid } else { "" } }
    }
}

#### Exported Functions ###########################################################################

function Grant-UserRight {
 <#
  .SYNOPSIS
    Assigns user rights to accounts
  .DESCRIPTION
    Assigns one or more user rights (privileges) to one or more accounts. If you specify privileges already granted to the account, they are ignored.
  .PARAMETER Account
    Logon name of the account. More than one account can be listed. If the account is not found on the computer, the default domain is searched. To specify a domain, you may use either "DOMAIN\username" or "username@domain.dns" formats. SIDs may be also be specified.
  .PARAMETER Right
    Name of the right to grant. More than one right may be listed.

    Possible values: 
      SeTrustedCredManAccessPrivilege              Access Credential Manager as a trusted caller
      SeNetworkLogonRight                          Access this computer from the network
      SeTcbPrivilege                               Act as part of the operating system
      SeMachineAccountPrivilege                    Add workstations to domain
      SeIncreaseQuotaPrivilege                     Adjust memory quotas for a process
      SeInteractiveLogonRight                      Allow log on locally
      SeRemoteInteractiveLogonRight                Allow log on through Remote Desktop Services
      SeBackupPrivilege                            Back up files and directories
      SeChangeNotifyPrivilege                      Bypass traverse checking
      SeSystemtimePrivilege                        Change the system time
      SeTimeZonePrivilege                          Change the time zone
      SeCreatePagefilePrivilege                    Create a pagefile
      SeCreateTokenPrivilege                       Create a token object
      SeCreateGlobalPrivilege                      Create global objects
      SeCreatePermanentPrivilege                   Create permanent shared objects
      SeCreateSymbolicLinkPrivilege                Create symbolic links
      SeDebugPrivilege                             Debug programs
      SeDenyNetworkLogonRight                      Deny access this computer from the network
      SeDenyBatchLogonRight                        Deny log on as a batch job
      SeDenyServiceLogonRight                      Deny log on as a service
      SeDenyInteractiveLogonRight                  Deny log on locally
      SeDenyRemoteInteractiveLogonRight            Deny log on through Remote Desktop Services
      SeEnableDelegationPrivilege                  Enable computer and user accounts to be trusted for delegation
      SeRemoteShutdownPrivilege                    Force shutdown from a remote system
      SeAuditPrivilege                             Generate security audits
      SeImpersonatePrivilege                       Impersonate a client after authentication
      SeIncreaseWorkingSetPrivilege                Increase a process working set
      SeIncreaseBasePriorityPrivilege              Increase scheduling priority
      SeLoadDriverPrivilege                        Load and unload device drivers
      SeLockMemoryPrivilege                        Lock pages in memory
      SeBatchLogonRight                            Log on as a batch job
      SeServiceLogonRight                          Log on as a service
      SeSecurityPrivilege                          Manage auditing and security log
      SeRelabelPrivilege                           Modify an object label
      SeSystemEnvironmentPrivilege                 Modify firmware environment values
      SeDelegateSessionUserImpersonatePrivilege    Obtain an impersonation token for another user in the same session
      SeManageVolumePrivilege                      Perform volume maintenance tasks
      SeProfileSingleProcessPrivilege              Profile single process
      SeSystemProfilePrivilege                     Profile system performance
      SeUnsolicitedInputPrivilege                  "Read unsolicited input from a terminal device"
      SeUndockPrivilege                            Remove computer from docking station
      SeAssignPrimaryTokenPrivilege                Replace a process level token
      SeRestorePrivilege                           Restore files and directories
      SeShutdownPrivilege                          Shut down the system
      SeSyncAgentPrivilege                         Synchronize directory service data
      SeTakeOwnershipPrivilege                     Take ownership of files or other objects
  .PARAMETER Computer
    Specifies the name of the computer on which to run this cmdlet. If the input for this parameter is omitted, then the cmdlet runs on the local computer.
  .EXAMPLE
    Grant-UserRight "bilbo.baggins" SeServiceLogonRight

    Grants bilbo.baggins the "Logon as a service" right on the local computer.
  .EXAMPLE
    Grant-UserRight -Account "Edward","Karen" -Right SeServiceLogonRight,SeCreateTokenPrivilege -Computer TESTPC

    Grants both Edward and Karen, "Logon as a service" and "Create a token object" rights on the TESTPC system.
  .EXAMPLE
    Grant-UserRight -Account "S-1-1-0" -Right SeNetworkLogonRight

    Grants "Everyone" the "Access this computer from the network" right on the local computer.
  .INPUTS
    String Account
    PS_LSA.Rights Right
    String Computer
  .OUTPUTS
    None
  .LINK
    http://msdn.microsoft.com/en-us/library/ms721786.aspx
    http://msdn.microsoft.com/en-us/library/bb530716.aspx
 #>
    [CmdletBinding(SupportsShouldProcess=$true)]
    param (
        [Parameter(Position=0, Mandatory=$true, ValueFromPipelineByPropertyName=$true, ValueFromPipeline=$true)]
        [Alias('User','Username','SID')][String[]] $Account,
        [Parameter(Position=1, Mandatory=$true, ValueFromPipelineByPropertyName=$true)]
        [Alias('Privilege')] [PS_LSA.Rights[]] $Right,
        [Parameter(ValueFromPipelineByPropertyName=$true, HelpMessage="Computer name")]
        [Alias('System','ComputerName','Host')][String] $Computer
    )
    process {
        $lsa = New-Object PS_LSA.LsaWrapper($Computer)
        foreach ($Acct in $Account) {
            foreach ($Priv in $Right) {
                if ($PSCmdlet.ShouldProcess($Acct, "Grant $Priv right")) { $lsa.AddPrivilege($Acct,$Priv) }
            }
        }
    }
} # Assigns user rights to accounts

function Revoke-UserRight {
 <#
  .SYNOPSIS
    Removes user rights from accounts
  .DESCRIPTION
    Removes one or more user rights (privileges) from one or more accounts. If you specify privileges not held by the account, they are ignored.
  .PARAMETER Account
    Logon name of the account. More than one account can be listed. If the account is not found on the computer, the default domain is searched. To specify a domain, you may use either "DOMAIN\username" or "username@domain.dns" formats. SIDs may be also be specified.
  .PARAMETER Right
    Name of the right to revoke. More than one right may be listed.

    Possible values: 
      SeTrustedCredManAccessPrivilege              Access Credential Manager as a trusted caller
      SeNetworkLogonRight                          Access this computer from the network
      SeTcbPrivilege                               Act as part of the operating system
      SeMachineAccountPrivilege                    Add workstations to domain
      SeIncreaseQuotaPrivilege                     Adjust memory quotas for a process
      SeInteractiveLogonRight                      Allow log on locally
      SeRemoteInteractiveLogonRight                Allow log on through Remote Desktop Services
      SeBackupPrivilege                            Back up files and directories
      SeChangeNotifyPrivilege                      Bypass traverse checking
      SeSystemtimePrivilege                        Change the system time
      SeTimeZonePrivilege                          Change the time zone
      SeCreatePagefilePrivilege                    Create a pagefile
      SeCreateTokenPrivilege                       Create a token object
      SeCreateGlobalPrivilege                      Create global objects
      SeCreatePermanentPrivilege                   Create permanent shared objects
      SeCreateSymbolicLinkPrivilege                Create symbolic links
      SeDebugPrivilege                             Debug programs
      SeDenyNetworkLogonRight                      Deny access this computer from the network
      SeDenyBatchLogonRight                        Deny log on as a batch job
      SeDenyServiceLogonRight                      Deny log on as a service
      SeDenyInteractiveLogonRight                  Deny log on locally
      SeDenyRemoteInteractiveLogonRight            Deny log on through Remote Desktop Services
      SeEnableDelegationPrivilege                  Enable computer and user accounts to be trusted for delegation
      SeRemoteShutdownPrivilege                    Force shutdown from a remote system
      SeAuditPrivilege                             Generate security audits
      SeImpersonatePrivilege                       Impersonate a client after authentication
      SeIncreaseWorkingSetPrivilege                Increase a process working set
      SeIncreaseBasePriorityPrivilege              Increase scheduling priority
      SeLoadDriverPrivilege                        Load and unload device drivers
      SeLockMemoryPrivilege                        Lock pages in memory
      SeBatchLogonRight                            Log on as a batch job
      SeServiceLogonRight                          Log on as a service
      SeSecurityPrivilege                          Manage auditing and security log
      SeRelabelPrivilege                           Modify an object label
      SeSystemEnvironmentPrivilege                 Modify firmware environment values
      SeDelegateSessionUserImpersonatePrivilege    Obtain an impersonation token for another user in the same session
      SeManageVolumePrivilege                      Perform volume maintenance tasks
      SeProfileSingleProcessPrivilege              Profile single process
      SeSystemProfilePrivilege                     Profile system performance
      SeUnsolicitedInputPrivilege                  "Read unsolicited input from a terminal device"
      SeUndockPrivilege                            Remove computer from docking station
      SeAssignPrimaryTokenPrivilege                Replace a process level token
      SeRestorePrivilege                           Restore files and directories
      SeShutdownPrivilege                          Shut down the system
      SeSyncAgentPrivilege                         Synchronize directory service data
      SeTakeOwnershipPrivilege                     Take ownership of files or other objects
  .PARAMETER Computer
    Specifies the name of the computer on which to run this cmdlet. If the input for this parameter is omitted, then the cmdlet runs on the local computer.
  .EXAMPLE
    Revoke-UserRight "bilbo.baggins" SeServiceLogonRight

    Removes the "Logon as a service" right from bilbo.baggins on the local computer.
  .EXAMPLE
    Revoke-UserRight "S-1-5-21-3108507890-3520248245-2556081279-1001" SeServiceLogonRight

    Removes the "Logon as a service" right from the specified SID on the local computer.
  .EXAMPLE
    Revoke-UserRight -Account "Edward","Karen" -Right SeServiceLogonRight,SeCreateTokenPrivilege -Computer TESTPC

    Removes the "Logon as a service" and "Create a token object" rights from both Edward and Karen on the TESTPC system.
  .EXAMPLE
    Revoke-UserRight -Account "S-1-1-0" -Right SeNetworkLogonRight

    Removes the "Access this computer from the network" right from "Everyone" on the local computer.
  .INPUTS
    String Account
    PS_LSA.Rights Right
    String Computer
  .OUTPUTS
    None
  .LINK
    http://msdn.microsoft.com/en-us/library/ms721809.aspx
    http://msdn.microsoft.com/en-us/library/bb530716.aspx
 #>
    [CmdletBinding(SupportsShouldProcess=$true)]
    param (
        [Parameter(Position=0, Mandatory=$true, ValueFromPipelineByPropertyName=$true, ValueFromPipeline=$true)]
        [Alias('User','Username','SID')][String[]] $Account,
        [Parameter(Position=1, Mandatory=$true, ValueFromPipelineByPropertyName=$true)]
        [Alias('Privilege')] [PS_LSA.Rights[]] $Right,
        [Parameter(ValueFromPipelineByPropertyName=$true, HelpMessage="Computer name")]
        [Alias('System','ComputerName','Host')][String] $Computer
    )
    process {
        $lsa = New-Object PS_LSA.LsaWrapper($Computer)
        foreach ($Acct in $Account) {
            foreach ($Priv in $Right) {
                if ($PSCmdlet.ShouldProcess($Acct, "Revoke $Priv right")) { $lsa.RemovePrivilege($Acct,$Priv) }
            }
        }
    }
} # Removes user rights from accounts

function Get-UserRightsGrantedToAccount {
 <#
  .SYNOPSIS
    Gets all user rights granted to an account
  .DESCRIPTION
    Retrieves a list of all the user rights (privileges) granted to one or more accounts. The rights retrieved are those granted directly to the user account, and does not include those rights obtained as part of membership to a group.
  .PARAMETER Account
    Logon name of the account. More than one account can be listed. If the account is not found on the computer, the default domain is searched. To specify a domain, you may use either "DOMAIN\username" or "username@domain.dns" formats. SIDs may be also be specified.
  .PARAMETER Computer
    Specifies the name of the computer on which to run this cmdlet. If the input for this parameter is omitted, then the cmdlet runs on the local computer.
  .EXAMPLE
    Get-UserRightsGrantedToAccount "bilbo.baggins"

    Returns a list of all user rights granted to bilbo.baggins on the local computer.
  .EXAMPLE
    Get-UserRightsGrantedToAccount -Account "Edward","Karen" -Computer TESTPC

    Returns a list of user rights granted to Edward, and a list of user rights granted to Karen, on the TESTPC system.
  .EXAMPLE
    Get-UserRightsGrantedToAccount -Account "S-1-1-0"

    Returns a list of user rights granted to "Everyone" on the local computer.
  .INPUTS
    String Account
    String Computer
  .OUTPUTS
    String Account
    PS_LSA.Rights Right
  .LINK
    http://msdn.microsoft.com/en-us/library/ms721790.aspx
    http://msdn.microsoft.com/en-us/library/bb530716.aspx
 #>
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true, ValueFromPipelineByPropertyName=$true, ValueFromPipeline=$true)]
        [Alias('User','Username','SID')][String[]] $Account,
        [Parameter(ValueFromPipelineByPropertyName=$true, HelpMessage="Computer name")]
        [Alias('System','ComputerName','Host')][String] $Computer
    )
    process {
        $lsa = New-Object PS_LSA.LsaWrapper($Computer)
        foreach ($Acct in $Account) {
            $rights = $lsa.EnumerateAccountPrivileges($Acct)
            foreach ($right in $rights) {
                $output = @{'Account'=$Acct; 'Right'=$right; }
                Write-Output (New-Object -Typename PSObject -Prop $output)
            }
        }
    }
} # Gets all user rights granted to an account

function Get-AccountsWithUserRight {
 <#
  .SYNOPSIS
    Gets all accounts that are assigned a specified privilege
  .DESCRIPTION
    Retrieves a list of all accounts that hold a specified right (privilege). The accounts returned are those that hold the specified privilege directly through the user account, not as part of membership to a group. A list of SIDs and account names is returned. For each SID that cannot be resolved to a name, the Account property is set to an empty string ("").
  .PARAMETER Right
    Name of the right to query. More than one right may be listed.

    Possible values: 
      SeTrustedCredManAccessPrivilege              Access Credential Manager as a trusted caller
      SeNetworkLogonRight                          Access this computer from the network
      SeTcbPrivilege                               Act as part of the operating system
      SeMachineAccountPrivilege                    Add workstations to domain
      SeIncreaseQuotaPrivilege                     Adjust memory quotas for a process
      SeInteractiveLogonRight                      Allow log on locally
      SeRemoteInteractiveLogonRight                Allow log on through Remote Desktop Services
      SeBackupPrivilege                            Back up files and directories
      SeChangeNotifyPrivilege                      Bypass traverse checking
      SeSystemtimePrivilege                        Change the system time
      SeTimeZonePrivilege                          Change the time zone
      SeCreatePagefilePrivilege                    Create a pagefile
      SeCreateTokenPrivilege                       Create a token object
      SeCreateGlobalPrivilege                      Create global objects
      SeCreatePermanentPrivilege                   Create permanent shared objects
      SeCreateSymbolicLinkPrivilege                Create symbolic links
      SeDebugPrivilege                             Debug programs
      SeDenyNetworkLogonRight                      Deny access this computer from the network
      SeDenyBatchLogonRight                        Deny log on as a batch job
      SeDenyServiceLogonRight                      Deny log on as a service
      SeDenyInteractiveLogonRight                  Deny log on locally
      SeDenyRemoteInteractiveLogonRight            Deny log on through Remote Desktop Services
      SeEnableDelegationPrivilege                  Enable computer and user accounts to be trusted for delegation
      SeRemoteShutdownPrivilege                    Force shutdown from a remote system
      SeAuditPrivilege                             Generate security audits
      SeImpersonatePrivilege                       Impersonate a client after authentication
      SeIncreaseWorkingSetPrivilege                Increase a process working set
      SeIncreaseBasePriorityPrivilege              Increase scheduling priority
      SeLoadDriverPrivilege                        Load and unload device drivers
      SeLockMemoryPrivilege                        Lock pages in memory
      SeBatchLogonRight                            Log on as a batch job
      SeServiceLogonRight                          Log on as a service
      SeSecurityPrivilege                          Manage auditing and security log
      SeRelabelPrivilege                           Modify an object label
      SeSystemEnvironmentPrivilege                 Modify firmware environment values
      SeDelegateSessionUserImpersonatePrivilege    Obtain an impersonation token for another user in the same session
      SeManageVolumePrivilege                      Perform volume maintenance tasks
      SeProfileSingleProcessPrivilege              Profile single process
      SeSystemProfilePrivilege                     Profile system performance
      SeUnsolicitedInputPrivilege                  "Read unsolicited input from a terminal device"
      SeUndockPrivilege                            Remove computer from docking station
      SeAssignPrimaryTokenPrivilege                Replace a process level token
      SeRestorePrivilege                           Restore files and directories
      SeShutdownPrivilege                          Shut down the system
      SeSyncAgentPrivilege                         Synchronize directory service data
      SeTakeOwnershipPrivilege                     Take ownership of files or other objects
  .PARAMETER Computer
    Specifies the name of the computer on which to run this cmdlet. If the input for this parameter is omitted, then the cmdlet runs on the local computer.
  .PARAMETER SidForUnresolvedName
    For each SID that cannot be resolved to a name, set the Account property to the SID instead of leaving it blank.
  .EXAMPLE
    Get-AccountsWithUserRight SeServiceLogonRight

    Returns a list of all accounts that hold the "Log on as a service" right.
  .EXAMPLE
    Get-AccountsWithUserRight -Right SeServiceLogonRight,SeDebugPrivilege -Computer TESTPC

    Returns a list of accounts that hold the "Log on as a service" right, and a list of accounts that hold the "Debug programs" right, on the TESTPC system.
  .INPUTS
    PS_LSA.Rights Right
    String Computer
    Switch SidForUnresolvedName
  .OUTPUTS
    String Account
    String SID
    String Right
  .LINK
    http://msdn.microsoft.com/en-us/library/ms721792.aspx
    http://msdn.microsoft.com/en-us/library/bb530716.aspx
 #>
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true, ValueFromPipelineByPropertyName=$true, ValueFromPipeline=$true)]
        [Alias('Privilege')] [PS_LSA.Rights[]] $Right,
        [Parameter(ValueFromPipelineByPropertyName=$true, HelpMessage="Computer name")]
        [Alias('System','ComputerName','Host')][String] $Computer,
        [switch] $SidForUnresolvedName
    )
    process {
        $lsa = New-Object PS_LSA.LsaWrapper($Computer)
        foreach ($Priv in $Right) {
            $sids = $lsa.EnumerateAccountsWithUserRight($Priv, $false)
            foreach ($sid in $sids) {
                $output = @{'Account'=(Convert-SIDtoName $sid $SidForUnresolvedName); 'SID'=$sid; 'Right'=$Priv; }
                Write-Output (New-Object -Typename PSObject -Prop $output)
            }
        }
    }
} # Gets all accounts that are assigned specified rights

function Grant-TokenPrivilege {
 <#
  .SYNOPSIS
    Enables privileges in the current process token.
  .DESCRIPTION
    Enables one or more privileges for the current process token. If a privilege cannot be enabled, an exception is thrown.
  .PARAMETER Privilege
    Name of the privilege to enable. More than one privilege may be listed.

    Possible values: 
      SeTrustedCredManAccessPrivilege              Access Credential Manager as a trusted caller
      SeNetworkLogonRight                          Access this computer from the network
      SeTcbPrivilege                               Act as part of the operating system
      SeMachineAccountPrivilege                    Add workstations to domain
      SeIncreaseQuotaPrivilege                     Adjust memory quotas for a process
      SeInteractiveLogonRight                      Allow log on locally
      SeRemoteInteractiveLogonRight                Allow log on through Remote Desktop Services
      SeBackupPrivilege                            Back up files and directories
      SeChangeNotifyPrivilege                      Bypass traverse checking
      SeSystemtimePrivilege                        Change the system time
      SeTimeZonePrivilege                          Change the time zone
      SeCreatePagefilePrivilege                    Create a pagefile
      SeCreateTokenPrivilege                       Create a token object
      SeCreateGlobalPrivilege                      Create global objects
      SeCreatePermanentPrivilege                   Create permanent shared objects
      SeCreateSymbolicLinkPrivilege                Create symbolic links
      SeDebugPrivilege                             Debug programs
      SeDenyNetworkLogonRight                      Deny access this computer from the network
      SeDenyBatchLogonRight                        Deny log on as a batch job
      SeDenyServiceLogonRight                      Deny log on as a service
      SeDenyInteractiveLogonRight                  Deny log on locally
      SeDenyRemoteInteractiveLogonRight            Deny log on through Remote Desktop Services
      SeEnableDelegationPrivilege                  Enable computer and user accounts to be trusted for delegation
      SeRemoteShutdownPrivilege                    Force shutdown from a remote system
      SeAuditPrivilege                             Generate security audits
      SeImpersonatePrivilege                       Impersonate a client after authentication
      SeIncreaseWorkingSetPrivilege                Increase a process working set
      SeIncreaseBasePriorityPrivilege              Increase scheduling priority
      SeLoadDriverPrivilege                        Load and unload device drivers
      SeLockMemoryPrivilege                        Lock pages in memory
      SeBatchLogonRight                            Log on as a batch job
      SeServiceLogonRight                          Log on as a service
      SeSecurityPrivilege                          Manage auditing and security log
      SeRelabelPrivilege                           Modify an object label
      SeSystemEnvironmentPrivilege                 Modify firmware environment values
      SeDelegateSessionUserImpersonatePrivilege    Obtain an impersonation token for another user in the same session
      SeManageVolumePrivilege                      Perform volume maintenance tasks
      SeProfileSingleProcessPrivilege              Profile single process
      SeSystemProfilePrivilege                     Profile system performance
      SeUnsolicitedInputPrivilege                  "Read unsolicited input from a terminal device"
      SeUndockPrivilege                            Remove computer from docking station
      SeAssignPrimaryTokenPrivilege                Replace a process level token
      SeRestorePrivilege                           Restore files and directories
      SeShutdownPrivilege                          Shut down the system
      SeSyncAgentPrivilege                         Synchronize directory service data
      SeTakeOwnershipPrivilege                     Take ownership of files or other objects
  .EXAMPLE
    Grant-TokenPrivilege SeIncreaseWorkingSetPrivilege

    Enables the "Increase a process working set" privilege for the current process.
  .INPUTS
    PS_LSA.Rights Right
  .OUTPUTS
    None
  .LINK
    http://msdn.microsoft.com/en-us/library/aa375202.aspx
    http://msdn.microsoft.com/en-us/library/bb530716.aspx
 #>
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true, ValueFromPipelineByPropertyName=$true, ValueFromPipeline=$true)]
        [Alias('Right')] [PS_LSA.Rights[]] $Privilege
    )
    process {
        foreach ($Priv in $Privilege) {
            try { [PS_LSA.TokenManipulator]::AddPrivilege($Priv) }
            catch [System.ComponentModel.Win32Exception] {
                throw New-Object System.ComponentModel.Win32Exception("$($_.Exception.Message) ($Priv)", $_.Exception)
            }
        }
    }
} # Enables privileges in the current process token

function Revoke-TokenPrivilege {
 <#
  .SYNOPSIS
    Disables privileges in the current process token.
  .DESCRIPTION
    Disables one or more privileges for the current process token. If a privilege cannot be disabled, an exception is thrown.
  .PARAMETER Privilege
    Name of the privilege to disable. More than one privilege may be listed.

    Possible values: 
      SeTrustedCredManAccessPrivilege              Access Credential Manager as a trusted caller
      SeNetworkLogonRight                          Access this computer from the network
      SeTcbPrivilege                               Act as part of the operating system
      SeMachineAccountPrivilege                    Add workstations to domain
      SeIncreaseQuotaPrivilege                     Adjust memory quotas for a process
      SeInteractiveLogonRight                      Allow log on locally
      SeRemoteInteractiveLogonRight                Allow log on through Remote Desktop Services
      SeBackupPrivilege                            Back up files and directories
      SeChangeNotifyPrivilege                      Bypass traverse checking
      SeSystemtimePrivilege                        Change the system time
      SeTimeZonePrivilege                          Change the time zone
      SeCreatePagefilePrivilege                    Create a pagefile
      SeCreateTokenPrivilege                       Create a token object
      SeCreateGlobalPrivilege                      Create global objects
      SeCreatePermanentPrivilege                   Create permanent shared objects
      SeCreateSymbolicLinkPrivilege                Create symbolic links
      SeDebugPrivilege                             Debug programs
      SeDenyNetworkLogonRight                      Deny access this computer from the network
      SeDenyBatchLogonRight                        Deny log on as a batch job
      SeDenyServiceLogonRight                      Deny log on as a service
      SeDenyInteractiveLogonRight                  Deny log on locally
      SeDenyRemoteInteractiveLogonRight            Deny log on through Remote Desktop Services
      SeEnableDelegationPrivilege                  Enable computer and user accounts to be trusted for delegation
      SeRemoteShutdownPrivilege                    Force shutdown from a remote system
      SeAuditPrivilege                             Generate security audits
      SeImpersonatePrivilege                       Impersonate a client after authentication
      SeIncreaseWorkingSetPrivilege                Increase a process working set
      SeIncreaseBasePriorityPrivilege              Increase scheduling priority
      SeLoadDriverPrivilege                        Load and unload device drivers
      SeLockMemoryPrivilege                        Lock pages in memory
      SeBatchLogonRight                            Log on as a batch job
      SeServiceLogonRight                          Log on as a service
      SeSecurityPrivilege                          Manage auditing and security log
      SeRelabelPrivilege                           Modify an object label
      SeSystemEnvironmentPrivilege                 Modify firmware environment values
      SeDelegateSessionUserImpersonatePrivilege    Obtain an impersonation token for another user in the same session
      SeManageVolumePrivilege                      Perform volume maintenance tasks
      SeProfileSingleProcessPrivilege              Profile single process
      SeSystemProfilePrivilege                     Profile system performance
      SeUnsolicitedInputPrivilege                  "Read unsolicited input from a terminal device"
      SeUndockPrivilege                            Remove computer from docking station
      SeAssignPrimaryTokenPrivilege                Replace a process level token
      SeRestorePrivilege                           Restore files and directories
      SeShutdownPrivilege                          Shut down the system
      SeSyncAgentPrivilege                         Synchronize directory service data
      SeTakeOwnershipPrivilege                     Take ownership of files or other objects
  .EXAMPLE
    Revoke-TokenPrivilege SeIncreaseWorkingSetPrivilege

    Disables the "Increase a process working set" privilege for the current process.
  .INPUTS
    PS_LSA.Rights Right
  .OUTPUTS
    None
  .LINK
    http://msdn.microsoft.com/en-us/library/aa375202.aspx
    http://msdn.microsoft.com/en-us/library/bb530716.aspx
 #>
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true, ValueFromPipelineByPropertyName=$true, ValueFromPipeline=$true)]
        [Alias('Right')] [PS_LSA.Rights[]] $Privilege
    )
    process {
        foreach ($Priv in $Privilege) {
            try { [PS_LSA.TokenManipulator]::RemovePrivilege($Priv) }
            catch [System.ComponentModel.Win32Exception] {
                throw New-Object System.ComponentModel.Win32Exception("$($_.Exception.Message) ($Priv)", $_.Exception)
            }
        }
    }
} # Disables privileges in the current process token

Export-ModuleMember -Function Grant-UserRight, Revoke-UserRight, Get-UserRightsGrantedToAccount, Get-AccountsWithUserRight
Export-ModuleMember -Function Grant-TokenPrivilege, Revoke-TokenPrivilege

# SIG # Begin signature block
# MIIcxAYJKoZIhvcNAQcCoIIctTCCHLECAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU7pCRH/qCNkaqakZUl26hDPUa
# BAegghfzMIIFMDCCBBigAwIBAgIQBAkYG1/Vu2Z1U0O1b5VQCDANBgkqhkiG9w0B
# AQsFADBlMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYD
# VQQLExB3d3cuZGlnaWNlcnQuY29tMSQwIgYDVQQDExtEaWdpQ2VydCBBc3N1cmVk
# IElEIFJvb3QgQ0EwHhcNMTMxMDIyMTIwMDAwWhcNMjgxMDIyMTIwMDAwWjByMQsw
# CQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cu
# ZGlnaWNlcnQuY29tMTEwLwYDVQQDEyhEaWdpQ2VydCBTSEEyIEFzc3VyZWQgSUQg
# Q29kZSBTaWduaW5nIENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA
# +NOzHH8OEa9ndwfTCzFJGc/Q+0WZsTrbRPV/5aid2zLXcep2nQUut4/6kkPApfmJ
# 1DcZ17aq8JyGpdglrA55KDp+6dFn08b7KSfH03sjlOSRI5aQd4L5oYQjZhJUM1B0
# sSgmuyRpwsJS8hRniolF1C2ho+mILCCVrhxKhwjfDPXiTWAYvqrEsq5wMWYzcT6s
# cKKrzn/pfMuSoeU7MRzP6vIK5Fe7SrXpdOYr/mzLfnQ5Ng2Q7+S1TqSp6moKq4Tz
# rGdOtcT3jNEgJSPrCGQ+UpbB8g8S9MWOD8Gi6CxR93O8vYWxYoNzQYIH5DiLanMg
# 0A9kczyen6Yzqf0Z3yWT0QIDAQABo4IBzTCCAckwEgYDVR0TAQH/BAgwBgEB/wIB
# ADAOBgNVHQ8BAf8EBAMCAYYwEwYDVR0lBAwwCgYIKwYBBQUHAwMweQYIKwYBBQUH
# AQEEbTBrMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC5kaWdpY2VydC5jb20wQwYI
# KwYBBQUHMAKGN2h0dHA6Ly9jYWNlcnRzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydEFz
# c3VyZWRJRFJvb3RDQS5jcnQwgYEGA1UdHwR6MHgwOqA4oDaGNGh0dHA6Ly9jcmw0
# LmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydEFzc3VyZWRJRFJvb3RDQS5jcmwwOqA4oDaG
# NGh0dHA6Ly9jcmwzLmRpZ2ljZXJ0LmNvbS9EaWdpQ2VydEFzc3VyZWRJRFJvb3RD
# QS5jcmwwTwYDVR0gBEgwRjA4BgpghkgBhv1sAAIEMCowKAYIKwYBBQUHAgEWHGh0
# dHBzOi8vd3d3LmRpZ2ljZXJ0LmNvbS9DUFMwCgYIYIZIAYb9bAMwHQYDVR0OBBYE
# FFrEuXsqCqOl6nEDwGD5LfZldQ5YMB8GA1UdIwQYMBaAFEXroq/0ksuCMS1Ri6en
# IZ3zbcgPMA0GCSqGSIb3DQEBCwUAA4IBAQA+7A1aJLPzItEVyCx8JSl2qB1dHC06
# GsTvMGHXfgtg/cM9D8Svi/3vKt8gVTew4fbRknUPUbRupY5a4l4kgU4QpO4/cY5j
# DhNLrddfRHnzNhQGivecRk5c/5CxGwcOkRX7uq+1UcKNJK4kxscnKqEpKBo6cSgC
# PC6Ro8AlEeKcFEehemhor5unXCBc2XGxDI+7qPjFEmifz0DLQESlE/DmZAwlCEIy
# sjaKJAL+L3J+HNdJRZboWR3p+nRka7LrZkPas7CM1ekN3fYBIM6ZMWM9CBoYs4Gb
# T8aTEAb8B4H6i9r5gkn3Ym6hU/oSlBiFLpKR6mhsRDKyZqHnGKSaZFHvMIIFfDCC
# BGSgAwIBAgIQAarMGW1/STo93o1TEWGeDjANBgkqhkiG9w0BAQsFADByMQswCQYD
# VQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGln
# aWNlcnQuY29tMTEwLwYDVQQDEyhEaWdpQ2VydCBTSEEyIEFzc3VyZWQgSUQgQ29k
# ZSBTaWduaW5nIENBMB4XDTE2MDgyMzAwMDAwMFoXDTE5MTEyMTEyMDAwMFowgbgx
# CzAJBgNVBAYTAlVTMQ0wCwYDVQQIEwRPaGlvMRQwEgYDVQQHEwtCZWF2ZXJjcmVl
# azEcMBoGA1UEChMTRWRpY3QgU3lzdGVtcywgSW5jLjEcMBoGA1UECxMTRWRpY3Qg
# U3lzdGVtcywgSW5jLjEcMBoGA1UEAxMTRWRpY3QgU3lzdGVtcywgSW5jLjEqMCgG
# CSqGSIb3DQEJARYbc2VydmVydGVhbUBlZGljdHN5c3RlbXMuY29tMIIBIjANBgkq
# hkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArIiBNxH+fwhHOImuhnPB8KkW7W2YOjs0
# jUPmBMCOz3tEGw+f3pxFY3excm6i2dpitj86tmtGkdg3eQFW83q0uRgSA8VYPyE5
# OiKoTwfJpt4RYbpcDXf7o7t/gwMEWh08A7I9bVyU4qtsUv5PF6SrdD4u7d16MxYm
# 4M4qjLv+u9sI7/urfzxQhbzGhGEuqMJGkNyYGX3QYMXq+nZThAA1u2NNkJNzzSh5
# fcsiPv8utB4r4pIgtL64eUIuYkx+j2n3BI/6yNxKCLb6Uu8/aSjS7I8MVFwJAFAr
# ueEflGCPi2Ab6CVwOrllEmxYVVqzPtXd+w376wxtc6cwZGcOqoWOzwIDAQABo4IB
# xTCCAcEwHwYDVR0jBBgwFoAUWsS5eyoKo6XqcQPAYPkt9mV1DlgwHQYDVR0OBBYE
# FArHIjPvp9Tepl+BpM6rruu3OF9rMA4GA1UdDwEB/wQEAwIHgDATBgNVHSUEDDAK
# BggrBgEFBQcDAzB3BgNVHR8EcDBuMDWgM6Axhi9odHRwOi8vY3JsMy5kaWdpY2Vy
# dC5jb20vc2hhMi1hc3N1cmVkLWNzLWcxLmNybDA1oDOgMYYvaHR0cDovL2NybDQu
# ZGlnaWNlcnQuY29tL3NoYTItYXNzdXJlZC1jcy1nMS5jcmwwTAYDVR0gBEUwQzA3
# BglghkgBhv1sAwEwKjAoBggrBgEFBQcCARYcaHR0cHM6Ly93d3cuZGlnaWNlcnQu
# Y29tL0NQUzAIBgZngQwBBAEwgYQGCCsGAQUFBwEBBHgwdjAkBggrBgEFBQcwAYYY
# aHR0cDovL29jc3AuZGlnaWNlcnQuY29tME4GCCsGAQUFBzAChkJodHRwOi8vY2Fj
# ZXJ0cy5kaWdpY2VydC5jb20vRGlnaUNlcnRTSEEyQXNzdXJlZElEQ29kZVNpZ25p
# bmdDQS5jcnQwDAYDVR0TAQH/BAIwADANBgkqhkiG9w0BAQsFAAOCAQEAdHHzVcLG
# dSA7XSBpDwHelcVX/UxMufr1KZ7QIRAANGJNm78wxr/qiaOTTMu0Yb4eumOIdcwn
# K3L3kxlebUPh4vTiYcdaO5GbuGESS18xZ6qn0qFOCG25Grm2IJKU+cc2bl31XWdp
# nCaDtCKa5XkRxFlk2VXuA52cdqmGK+Gc6H+J/1EBlNhBbdguvcZJ/U1+JBTeuCgM
# MXbk5bRUEUrXjXwOt+XUXiqP2ENUAlv/4/uATxxE5VJAVeHulXtr7UsUcINIBD9w
# z8BpzvLbBVNNCn7/WGvcvtif7ShIYQgZ28dvaMto3kNNhPvT9aMDkGowTdzjl5xn
# ddzZhcJ0RxRr7jCCBmowggVSoAMCAQICEAMBmgI6/1ixa9bV6uYX8GYwDQYJKoZI
# hvcNAQEFBQAwYjELMAkGA1UEBhMCVVMxFTATBgNVBAoTDERpZ2lDZXJ0IEluYzEZ
# MBcGA1UECxMQd3d3LmRpZ2ljZXJ0LmNvbTEhMB8GA1UEAxMYRGlnaUNlcnQgQXNz
# dXJlZCBJRCBDQS0xMB4XDTE0MTAyMjAwMDAwMFoXDTI0MTAyMjAwMDAwMFowRzEL
# MAkGA1UEBhMCVVMxETAPBgNVBAoTCERpZ2lDZXJ0MSUwIwYDVQQDExxEaWdpQ2Vy
# dCBUaW1lc3RhbXAgUmVzcG9uZGVyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIB
# CgKCAQEAo2Rd/Hyz4II14OD2xirmSXU7zG7gU6mfH2RZ5nxrf2uMnVX4kuOe1Vpj
# WwJJUNmDzm9m7t3LhelfpfnUh3SIRDsZyeX1kZ/GFDmsJOqoSyyRicxeKPRktlC3
# 9RKzc5YKZ6O+YZ+u8/0SeHUOplsU/UUjjoZEVX0YhgWMVYd5SEb3yg6Np95OX+Ko
# ti1ZAmGIYXIYaLm4fO7m5zQvMXeBMB+7NgGN7yfj95rwTDFkjePr+hmHqH7P7IwM
# Nlt6wXq4eMfJBi5GEMiN6ARg27xzdPpO2P6qQPGyznBGg+naQKFZOtkVCVeZVjCT
# 88lhzNAIzGvsYkKRrALA76TwiRGPdwIDAQABo4IDNTCCAzEwDgYDVR0PAQH/BAQD
# AgeAMAwGA1UdEwEB/wQCMAAwFgYDVR0lAQH/BAwwCgYIKwYBBQUHAwgwggG/BgNV
# HSAEggG2MIIBsjCCAaEGCWCGSAGG/WwHATCCAZIwKAYIKwYBBQUHAgEWHGh0dHBz
# Oi8vd3d3LmRpZ2ljZXJ0LmNvbS9DUFMwggFkBggrBgEFBQcCAjCCAVYeggFSAEEA
# bgB5ACAAdQBzAGUAIABvAGYAIAB0AGgAaQBzACAAQwBlAHIAdABpAGYAaQBjAGEA
# dABlACAAYwBvAG4AcwB0AGkAdAB1AHQAZQBzACAAYQBjAGMAZQBwAHQAYQBuAGMA
# ZQAgAG8AZgAgAHQAaABlACAARABpAGcAaQBDAGUAcgB0ACAAQwBQAC8AQwBQAFMA
# IABhAG4AZAAgAHQAaABlACAAUgBlAGwAeQBpAG4AZwAgAFAAYQByAHQAeQAgAEEA
# ZwByAGUAZQBtAGUAbgB0ACAAdwBoAGkAYwBoACAAbABpAG0AaQB0ACAAbABpAGEA
# YgBpAGwAaQB0AHkAIABhAG4AZAAgAGEAcgBlACAAaQBuAGMAbwByAHAAbwByAGEA
# dABlAGQAIABoAGUAcgBlAGkAbgAgAGIAeQAgAHIAZQBmAGUAcgBlAG4AYwBlAC4w
# CwYJYIZIAYb9bAMVMB8GA1UdIwQYMBaAFBUAEisTmLKZB+0e36K+Vw0rZwLNMB0G
# A1UdDgQWBBRhWk0ktkkynUoqeRqDS/QeicHKfTB9BgNVHR8EdjB0MDigNqA0hjJo
# dHRwOi8vY3JsMy5kaWdpY2VydC5jb20vRGlnaUNlcnRBc3N1cmVkSURDQS0xLmNy
# bDA4oDagNIYyaHR0cDovL2NybDQuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0QXNzdXJl
# ZElEQ0EtMS5jcmwwdwYIKwYBBQUHAQEEazBpMCQGCCsGAQUFBzABhhhodHRwOi8v
# b2NzcC5kaWdpY2VydC5jb20wQQYIKwYBBQUHMAKGNWh0dHA6Ly9jYWNlcnRzLmRp
# Z2ljZXJ0LmNvbS9EaWdpQ2VydEFzc3VyZWRJRENBLTEuY3J0MA0GCSqGSIb3DQEB
# BQUAA4IBAQCdJX4bM02yJoFcm4bOIyAPgIfliP//sdRqLDHtOhcZcRfNqRu8WhY5
# AJ3jbITkWkD73gYBjDf6m7GdJH7+IKRXrVu3mrBgJuppVyFdNC8fcbCDlBkFazWQ
# EKB7l8f2P+fiEUGmvWLZ8Cc9OB0obzpSCfDscGLTYkuw4HOmksDTjjHYL+NtFxMG
# 7uQDthSr849Dp3GdId0UyhVdkkHa+Q+B0Zl0DSbEDn8btfWg8cZ3BigV6diT5VUW
# 8LsKqxzbXEgnZsijiwoc5ZXarsQuWaBh3drzbaJh6YoLbewSGL33VVRAA5Ira8JR
# wgpIr7DUbuD0FAo6G+OPPcqvao173NhEMIIGzTCCBbWgAwIBAgIQBv35A5YDreoA
# Cus/J7u6GzANBgkqhkiG9w0BAQUFADBlMQswCQYDVQQGEwJVUzEVMBMGA1UEChMM
# RGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMSQwIgYDVQQD
# ExtEaWdpQ2VydCBBc3N1cmVkIElEIFJvb3QgQ0EwHhcNMDYxMTEwMDAwMDAwWhcN
# MjExMTEwMDAwMDAwWjBiMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQg
# SW5jMRkwFwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMSEwHwYDVQQDExhEaWdpQ2Vy
# dCBBc3N1cmVkIElEIENBLTEwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIB
# AQDogi2Z+crCQpWlgHNAcNKeVlRcqcTSQQaPyTP8TUWRXIGf7Syc+BZZ3561JBXC
# mLm0d0ncicQK2q/LXmvtrbBxMevPOkAMRk2T7It6NggDqww0/hhJgv7HxzFIgHwe
# og+SDlDJxofrNj/YMMP/pvf7os1vcyP+rFYFkPAyIRaJxnCI+QWXfaPHQ90C6Ds9
# 7bFBo+0/vtuVSMTuHrPyvAwrmdDGXRJCgeGDboJzPyZLFJCuWWYKxI2+0s4Grq2E
# b0iEm09AufFM8q+Y+/bOQF1c9qjxL6/siSLyaxhlscFzrdfx2M8eCnRcQrhofrfV
# dwonVnwPYqQ/MhRglf0HBKIJAgMBAAGjggN6MIIDdjAOBgNVHQ8BAf8EBAMCAYYw
# OwYDVR0lBDQwMgYIKwYBBQUHAwEGCCsGAQUFBwMCBggrBgEFBQcDAwYIKwYBBQUH
# AwQGCCsGAQUFBwMIMIIB0gYDVR0gBIIByTCCAcUwggG0BgpghkgBhv1sAAEEMIIB
# pDA6BggrBgEFBQcCARYuaHR0cDovL3d3dy5kaWdpY2VydC5jb20vc3NsLWNwcy1y
# ZXBvc2l0b3J5Lmh0bTCCAWQGCCsGAQUFBwICMIIBVh6CAVIAQQBuAHkAIAB1AHMA
# ZQAgAG8AZgAgAHQAaABpAHMAIABDAGUAcgB0AGkAZgBpAGMAYQB0AGUAIABjAG8A
# bgBzAHQAaQB0AHUAdABlAHMAIABhAGMAYwBlAHAAdABhAG4AYwBlACAAbwBmACAA
# dABoAGUAIABEAGkAZwBpAEMAZQByAHQAIABDAFAALwBDAFAAUwAgAGEAbgBkACAA
# dABoAGUAIABSAGUAbAB5AGkAbgBnACAAUABhAHIAdAB5ACAAQQBnAHIAZQBlAG0A
# ZQBuAHQAIAB3AGgAaQBjAGgAIABsAGkAbQBpAHQAIABsAGkAYQBiAGkAbABpAHQA
# eQAgAGEAbgBkACAAYQByAGUAIABpAG4AYwBvAHIAcABvAHIAYQB0AGUAZAAgAGgA
# ZQByAGUAaQBuACAAYgB5ACAAcgBlAGYAZQByAGUAbgBjAGUALjALBglghkgBhv1s
# AxUwEgYDVR0TAQH/BAgwBgEB/wIBADB5BggrBgEFBQcBAQRtMGswJAYIKwYBBQUH
# MAGGGGh0dHA6Ly9vY3NwLmRpZ2ljZXJ0LmNvbTBDBggrBgEFBQcwAoY3aHR0cDov
# L2NhY2VydHMuZGlnaWNlcnQuY29tL0RpZ2lDZXJ0QXNzdXJlZElEUm9vdENBLmNy
# dDCBgQYDVR0fBHoweDA6oDigNoY0aHR0cDovL2NybDMuZGlnaWNlcnQuY29tL0Rp
# Z2lDZXJ0QXNzdXJlZElEUm9vdENBLmNybDA6oDigNoY0aHR0cDovL2NybDQuZGln
# aWNlcnQuY29tL0RpZ2lDZXJ0QXNzdXJlZElEUm9vdENBLmNybDAdBgNVHQ4EFgQU
# FQASKxOYspkH7R7for5XDStnAs0wHwYDVR0jBBgwFoAUReuir/SSy4IxLVGLp6ch
# nfNtyA8wDQYJKoZIhvcNAQEFBQADggEBAEZQPsm3KCSnOB22WymvUs9S6TFHq1Zc
# e9UNC0Gz7+x1H3Q48rJcYaKclcNQ5IK5I9G6OoZyrTh4rHVdFxc0ckeFlFbR67s2
# hHfMJKXzBBlVqefj56tizfuLLZDCwNK1lL1eT7EF0g49GqkUW6aGMWKoqDPkmzmn
# xPXOHXh2lCVz5Cqrz5x2S+1fwksW5EtwTACJHvzFebxMElf+X+EevAJdqP77BzhP
# DcZdkbkPZ0XN1oPt55INjbFpjE/7WeAjD9KqrgB87pxCDs+R1ye3Fu4Pw718CqDu
# LAhVhSK46xgaTfwqIa1JMYNHlXdx3LEbS0scEJx3FMGdTy9alQgpECYxggQ7MIIE
# NwIBATCBhjByMQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkw
# FwYDVQQLExB3d3cuZGlnaWNlcnQuY29tMTEwLwYDVQQDEyhEaWdpQ2VydCBTSEEy
# IEFzc3VyZWQgSUQgQ29kZSBTaWduaW5nIENBAhABqswZbX9JOj3ejVMRYZ4OMAkG
# BSsOAwIaBQCgeDAYBgorBgEEAYI3AgEMMQowCKACgAChAoAAMBkGCSqGSIb3DQEJ
# AzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMCMG
# CSqGSIb3DQEJBDEWBBTcc+CdqVeOheMxaknEJXOP1jaYIzANBgkqhkiG9w0BAQEF
# AASCAQBANT+GaxO/3TkOB/FxPILgt4wM3SR+mPGIpLiol+bIQPU5plH2zEKxLzZn
# dT6KYmOjqgbQyhOCrzyb8Zlzzz6K1IdI+mVFtJExsYcmEfoIpcIyBBk2D3uEc2G/
# 9QRmned2crKdR4Nm8GbMDs4fV1KIP4qfmzkImtW4x8vATMYg1gZx1MLdY2C5M/xK
# 66ZhZlY3/mkvfhsrmZytBvwAwRs1dnuQnR/lJYloHc6jC7+DBGVkaxLqkfG6ubgo
# jloXUB8Z3iJ8l4QPl28X46rBJHBTto6pbuQezo47o8h+t7kS+EceFKd7CgUNcDSZ
# KgAyVwN3/afdqhj6u4tgtLyLFpjfoYICDzCCAgsGCSqGSIb3DQEJBjGCAfwwggH4
# AgEBMHYwYjELMAkGA1UEBhMCVVMxFTATBgNVBAoTDERpZ2lDZXJ0IEluYzEZMBcG
# A1UECxMQd3d3LmRpZ2ljZXJ0LmNvbTEhMB8GA1UEAxMYRGlnaUNlcnQgQXNzdXJl
# ZCBJRCBDQS0xAhADAZoCOv9YsWvW1ermF/BmMAkGBSsOAwIaBQCgXTAYBgkqhkiG
# 9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0xODA0MjUyMDMxNTBa
# MCMGCSqGSIb3DQEJBDEWBBSmVKd5kzaU17bihi0BkW6h5Mu7ATANBgkqhkiG9w0B
# AQEFAASCAQBiHDufDZIY5FIj8Z5hkd1JKpHuBcIpuVln1YoR3Srp8nKaDOuZm+sD
# I6x+MfNGY1Bsth7Qn22ZiqO4+biCJgDB78CwqRXpMXXhqO45OO04cna9YiHZI1Wq
# 3eE1s4bYL2/HklMcDUgLhcRFb/MjcF4SesBgZ3Jam1Wi+35SnkXu2RiHE9Wv17Fa
# qSYvDH6V/ztp9onSCr2N+qNwW93qoK0oO2LXpeVm7leNpZoJ46Qjw89lkc4T0nI+
# tfVAaP/6ZGfG85ZZGDLXajavRzUCFUCG08ySITY3b1+FPeRlWDVrn/99bbgHKcjY
# 06SDVYw9qCKvdwbOjFrQ99VaTAXhVw+s
# SIG # End signature block
