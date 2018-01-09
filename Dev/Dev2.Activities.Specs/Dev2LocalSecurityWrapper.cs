﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using Dev2.Common;


namespace Dev2.Activities.Specs
{
    class Dev2LocalSecurityWrapper
    {
        // Import the LSA functions

        [DllImport("advapi32.dll", PreserveSig = true)]
        static extern UInt32 LsaOpenPolicy(
            ref LSA_UNICODE_STRING SystemName,
            ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
            Int32 DesiredAccess,
            out IntPtr PolicyHandle
            );

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        static extern uint LsaAddAccountRights(
                       IntPtr PolicyHandle,
                       IntPtr AccountSid,
                       LSA_UNICODE_STRING[] UserRights,
                       uint CountOfRights);

        [DllImport("advapi32")]
        public static extern void FreeSid(IntPtr pSid);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
        static extern bool LookupAccountName(
            string lpSystemName, string lpAccountName,
            IntPtr psid,
            ref int cbsid,
            StringBuilder domainName, ref int cbdomainLength, ref int use);

        [DllImport("advapi32.dll")]
        static extern bool IsValidSid(IntPtr pSid);

        [DllImport("advapi32.dll")]
        static extern long LsaClose(IntPtr ObjectHandle);

        [DllImport("kernel32.dll")]
        static extern int GetLastError();

        //[DllImport("advapi32.dll")]
        //private static extern long LsaNtStatusToWinError(long status);
        [DllImport("advapi32.dll", SetLastError = false)]
        static extern uint LsaNtStatusToWinError(uint status);

        // define the structures

        enum LSA_AccessPolicy : long
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

        [StructLayout(LayoutKind.Sequential)]
        struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public readonly LSA_UNICODE_STRING ObjectName;
            public UInt32 Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LSA_UNICODE_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            public IntPtr Buffer;
        }
        /// 
        //Adds a privilege to an account

        /// Name of an account - "domain\account" or only "account"
        /// Name ofthe privilege
        /// The windows error code returned by LsaAddAccountRights
        public long SetRight(String accountName, String privilegeName)
        {
            
            long winErrorCode = 0; //contains the last error

            //pointer an size for the SID
            var sid = IntPtr.Zero;
            var sidSize = 0;
            //StringBuilder and size for the domain name
            var domainName = new StringBuilder();
            var nameSize = 0;
            //account-type variable for lookup
            var accountType = 0;

            //get required buffer size
            LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);

            //allocate buffers
            domainName = new StringBuilder(nameSize);
            sid = Marshal.AllocHGlobal(sidSize);

            //lookup the SID for the account
            var result = LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize,
                                            ref accountType);

            //say what you're doing
            Dev2Logger.Info("LookupAccountName result = " + result, "Warewolf Info");
            Dev2Logger.Info("IsValidSid: " + IsValidSid(sid), "Warewolf Info");
            Dev2Logger.Info("LookupAccountName domainName: " + domainName, "Warewolf Info");

            if (!result)
            {
                winErrorCode = GetLastError();
                Dev2Logger.Info("LookupAccountName failed: " + winErrorCode, "Warewolf Info");
            }
            else
            {
                //initialize an empty unicode-string
                var systemName = new LSA_UNICODE_STRING();
                //combine all policies
                var access = (int)(
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
                //initialize a pointer for the policy handle
                
                var policyHandle = IntPtr.Zero;

                //these attributes are not used, but LsaOpenPolicy wants them to exists
                var ObjectAttributes = new LSA_OBJECT_ATTRIBUTES();
                ObjectAttributes.Length = 0;
                ObjectAttributes.RootDirectory = IntPtr.Zero;
                ObjectAttributes.Attributes = 0;
                ObjectAttributes.SecurityDescriptor = IntPtr.Zero;
                ObjectAttributes.SecurityQualityOfService = IntPtr.Zero;

                //get a policy handle
                var resultPolicy = LsaOpenPolicy(ref systemName, ref ObjectAttributes, access, out policyHandle);
                winErrorCode = LsaNtStatusToWinError(resultPolicy);

                if (winErrorCode != 0)
                {
                    Dev2Logger.Info("OpenPolicy failed: " + winErrorCode, "Warewolf Info");
                }
                else
                {
                    //Now that we have the SID an the policy,
                    //we can add rights to the account.

                    //initialize an unicode-string for the privilege name
                    var userRights = new LSA_UNICODE_STRING[1];
                    userRights[0] = new LSA_UNICODE_STRING();
                    userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                    userRights[0].Length = (UInt16)(privilegeName.Length * UnicodeEncoding.CharSize);
                    userRights[0].MaximumLength = (UInt16)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);

                    //add the right to the account
                    long res = LsaAddAccountRights(policyHandle, sid, userRights, 1);
                    var status = (uint)res;
                    winErrorCode = LsaNtStatusToWinError(status);
                    if (winErrorCode != 0)
                    {
                        Dev2Logger.Info("LsaAddAccountRights failed: " + winErrorCode, "Warewolf Info");
                    }

                    LsaClose(policyHandle);
                }
                FreeSid(sid);
            }

            return winErrorCode;
        }
    }
}
