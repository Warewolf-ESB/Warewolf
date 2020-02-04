//
// This code has been adapted from http://www.codeproject.com/KB/cs/lsadotnet.aspx
// The rights enumeration code came from http://www.tech-archive.net/Archive/DotNet/microsoft.public.dotnet.framework.interop/2004-11/0394.html
//
// Windows Security via .NET is covered on by Pluralsight:http://alt.pluralsight.com/wiki/default.aspx/Keith.GuideBook/HomePage.html
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

//
// Provides methods the local security authority which controls user rights. Managed via secpol.msc normally.
//
public class LocalSecurityAuthorityController
{
    private const int Access = (int)(
                                            LSA_AccessPolicy.POLICY_AUDIT_LOG_ADMIN |
                                            LSA_AccessPolicy.POLICY_CREATE_ACCOUNT |
                                            LSA_AccessPolicy.POLICY_CREATE_PRIVILEGE |
                                            LSA_AccessPolicy.POLICY_CREATE_SECRET |
                                            LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION |
                                            LSA_AccessPolicy.POLICY_LOOKUP_NAMES |
                                            LSA_AccessPolicy.POLICY_NOTIFICATION |
                                            LSA_AccessPolicy.POLICY_SERVER_ADMIN |
                                            LSA_AccessPolicy.POLICY_SET_AUDIT_REQUIREMENTS |
                                            LSA_AccessPolicy.POLICY_SET_DEFAULT_QUOTA_LIMITS |
                                            LSA_AccessPolicy.POLICY_TRUST_ADMIN |
                                            LSA_AccessPolicy.POLICY_VIEW_AUDIT_INFORMATION |
                                            LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION
                                        );

    [DllImport("advapi32.dll", PreserveSig = true)]
    private static extern UInt32 LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, Int32 DesiredAccess, out IntPtr PolicyHandle);

    [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
    private static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, LSA_UNICODE_STRING[] UserRights, int CountOfRights);

    [DllImport("advapi32")]
    public static extern void FreeSid(IntPtr pSid);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
    private static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr psid, ref int cbsid, StringBuilder domainName, ref int cbdomainLength, ref int use);

    [DllImport("advapi32.dll")]
    private static extern bool IsValidSid(IntPtr pSid);

    [DllImport("advapi32.dll")]
    private static extern int LsaClose(IntPtr ObjectHandle);

    [DllImport("kernel32.dll")]
    private static extern int GetLastError();

    [DllImport("advapi32.dll")]
    private static extern int LsaNtStatusToWinError(uint status);

    [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
    private static extern uint LsaEnumerateAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, out IntPtr UserRightsPtr, out int CountOfRights);

    [StructLayout(LayoutKind.Sequential)]
    private struct LSA_UNICODE_STRING
    {
        public UInt16 Length;
        public UInt16 MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LSA_OBJECT_ATTRIBUTES
    {
        public int Length;
        public IntPtr RootDirectory;
        public LSA_UNICODE_STRING ObjectName;
        public UInt32 Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

    [Flags]
    private enum LSA_AccessPolicy : long
    {
        POLICY_VIEW_LOCAL_INFORMATION = 0x00000001L,
        POLICY_VIEW_AUDIT_INFORMATION = 0x00000002L,
        POLICY_GET_PRIVATE_INFORMATION = 0x00000004L,
        POLICY_TRUST_ADMIN = 0x00000008L,
        POLICY_CREATE_ACCOUNT = 0x00000010L,
        POLICY_CREATE_SECRET = 0x00000020L,
        POLICY_CREATE_PRIVILEGE = 0x00000040L,
        POLICY_SET_DEFAULT_QUOTA_LIMITS = 0x00000080L,
        POLICY_SET_AUDIT_REQUIREMENTS = 0x00000100L,
        POLICY_AUDIT_LOG_ADMIN = 0x00000200L,
        POLICY_SERVER_ADMIN = 0x00000400L,
        POLICY_LOOKUP_NAMES = 0x00000800L,
        POLICY_NOTIFICATION = 0x00001000L
    }

    // Returns the Local Security Authority rights granted to the account
    public static IList<string> GetRights(string accountName)
    {
        IList<string> rights = new List<string>();
        var errorMessage = string.Empty;

        long winErrorCode = 0;
        IntPtr sid = IntPtr.Zero;
        int sidSize = 0;
        var domainName = new StringBuilder();
        int nameSize = 0;
        int accountType = 0;

        LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

        domainName = new StringBuilder(nameSize);
        sid = Marshal.AllocHGlobal(sidSize);

        if (!LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType))
        {
            winErrorCode = GetLastError();
            errorMessage = ("LookupAccountName failed: " + winErrorCode);
            throw new Win32Exception((int)winErrorCode, errorMessage);
        }
        else
        {
            var systemName = new LSA_UNICODE_STRING();

            IntPtr policyHandle = IntPtr.Zero;
            IntPtr userRightsPtr = IntPtr.Zero;
            int countOfRights = 0;

            var objectAttributes = CreateLSAObject();

            uint policyStatus = LsaOpenPolicy(ref systemName, ref objectAttributes, Access, out policyHandle);
            winErrorCode = LsaNtStatusToWinError(policyStatus);

            if (winErrorCode != 0)
            {
                errorMessage = string.Format("OpenPolicy failed: {0}.", winErrorCode);
                throw new Win32Exception((int)winErrorCode, errorMessage);
            }
            else
            {
                try
                {
                    uint result = LsaEnumerateAccountRights(policyHandle, sid, out userRightsPtr, out countOfRights);
                    winErrorCode = LsaNtStatusToWinError(result);
                    if (winErrorCode != 0)
                    {
                        if (winErrorCode == 2)
                        {
                            return new List<string>();
                        }
                        errorMessage = string.Format("LsaEnumerateAccountRights failed: {0}", winErrorCode);
                        throw new Win32Exception((int)winErrorCode, errorMessage);
                    }

                    Int32 ptr = userRightsPtr.ToInt32();
                    LSA_UNICODE_STRING userRight;

                    for (int i = 0; i < countOfRights; i++)
                    {
                        userRight = (LSA_UNICODE_STRING)Marshal.PtrToStructure(new IntPtr(ptr), typeof(LSA_UNICODE_STRING));
                        var userRightStr = Marshal.PtrToStringAuto(userRight.Buffer);
                        rights.Add(userRightStr);
                        ptr += Marshal.SizeOf(userRight);
                    }
                }
                finally
                {
                    LsaClose(policyHandle);
                }
            }
            FreeSid(sid);
        }
        return rights;
    }

    // Adds a privilege to an account
    public static void SetRight(string accountName, string privilegeName)
    {
        long winErrorCode = 0;
        var errorMessage = string.Empty;

        IntPtr sid = IntPtr.Zero;
        int sidSize = 0;
        var domainName = new StringBuilder();
        int nameSize = 0;
        int accountType = 0;

        LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

        domainName = new StringBuilder(nameSize);
        sid = Marshal.AllocHGlobal(sidSize);

        if (!LookupAccountName(string.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType))
        {
            winErrorCode = GetLastError();
            errorMessage = string.Format("LookupAccountName failed: {0}", winErrorCode);
            throw new Win32Exception((int)winErrorCode, errorMessage);
        }
        else
        {
            var systemName = new LSA_UNICODE_STRING();
            IntPtr policyHandle = IntPtr.Zero;
            var objectAttributes = CreateLSAObject();

            uint resultPolicy = LsaOpenPolicy(ref systemName, ref objectAttributes, Access, out policyHandle);
            winErrorCode = LsaNtStatusToWinError(resultPolicy);

            if (winErrorCode != 0)
            {
                errorMessage = string.Format("OpenPolicy failed: {0} ", winErrorCode);
                throw new Win32Exception((int)winErrorCode, errorMessage);
            }
            else
            {
                try
                {
                    var userRights = new LSA_UNICODE_STRING[1];
                    userRights[0] = new LSA_UNICODE_STRING();
                    userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                    userRights[0].Length = (UInt16)(privilegeName.Length * UnicodeEncoding.CharSize);
                    userRights[0].MaximumLength = (UInt16)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);

                    uint res = LsaAddAccountRights(policyHandle, sid, userRights, 1);
                    winErrorCode = LsaNtStatusToWinError(res);
                    if (winErrorCode != 0)
                    {
                        errorMessage = string.Format("LsaAddAccountRights failed: {0}", winErrorCode);
                        throw new Win32Exception((int)winErrorCode, errorMessage);
                    }
                }
                finally
                {
                    LsaClose(policyHandle);
                }
            }
            FreeSid(sid);
        }
    }

    private static LSA_OBJECT_ATTRIBUTES CreateLSAObject()
    {
        var newInstance = new LSA_OBJECT_ATTRIBUTES
        {
            Length = 0,
            RootDirectory = IntPtr.Zero,
            Attributes = 0,
            SecurityDescriptor = IntPtr.Zero,
            SecurityQualityOfService = IntPtr.Zero
        };

        return newInstance;
    }
}

// Local security rights managed by the Local Security Authority
public class LocalSecurityAuthorityRights
{
    // Log on as a service right
    public const string LogonAsService = "SeServiceLogonRight";
    // Log on as a batch job right
    public const string LogonAsBatchJob = "SeBatchLogonRight";
    // Interactive log on right
    public const string InteractiveLogon = "SeInteractiveLogonRight";
    // Network log on right
    public const string NetworkLogon = "SeNetworkLogonRight";
    // Generate security audit logs right
    public const string GenerateSecurityAudits = "SeAuditPrivilege";
}

/* added wrapper for PowerShell */

public class LSAWrapper
{
    public static IList<string> GetRights(string accountName)
    {
        return LocalSecurityAuthorityController.GetRights(accountName);
    }

    public static void SetRight(string accountName, string privilegeName)
    {
        LocalSecurityAuthorityController.SetRight(accountName, privilegeName);
    }
}