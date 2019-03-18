#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;
using LSA_HANDLE = System.IntPtr;


[StructLayout(LayoutKind.Sequential)]
struct LSA_OBJECT_ATTRIBUTES
{
    internal int Length;
    internal LSA_HANDLE RootDirectory;
    internal LSA_HANDLE ObjectName;
    internal int Attributes;
    internal LSA_HANDLE SecurityDescriptor;
    internal LSA_HANDLE SecurityQualityOfService;
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
    internal LSA_HANDLE PSid;
}


static class Win32Sec
{
    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true), SuppressUnmanagedCodeSecurity]
    internal static extern uint LsaOpenPolicy(
        LSA_UNICODE_STRING[] SystemName,
        ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
        int AccessMask,
        out LSA_HANDLE PolicyHandle
        );

    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true,PreserveSig = true), SuppressUnmanagedCodeSecurity]
    internal static extern uint LsaEnumerateAccountsWithUserRight(
        LSA_HANDLE PolicyHandle,
        LSA_UNICODE_STRING[] UserRights,
        out IntPtr EnumerationBuffer,
        out ulong CountReturned
        );

    [DllImport("advapi32")]
    internal static extern int LsaNtStatusToWinError(int NTSTATUS);

    [DllImport("advapi32")]
    internal static extern int LsaClose(LSA_HANDLE PolicyHandle);
}

public class SecurityWrapper : ISecurityWrapper
{
    readonly IAuthorizationService _authorizationService;

    enum Access
    {
        POLICY_READ = 0x20006,
        POLICY_ALL_ACCESS = 0x00F0FFF,
        POLICY_EXECUTE = 0X20801,

        POLICY_WRITE = 0X207F8
    }

    const uint STATUS_ACCESS_DENIED = 0xc0000022;
    const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;
    const uint STATUS_NO_MEMORY = 0xc0000017;
    const uint ERROR_NO_MORE_ITEMS = 2147483674;
    const uint ERROR_PRIVILEGE_DOES_NOT_EXIST = 3221225568;
    LSA_HANDLE lsaHandle;

    public SecurityWrapper(IAuthorizationService authorizationService)
        : this(Environment.MachineName)
    {
        _authorizationService = authorizationService;
    }

    public SecurityWrapper(string MachineName)
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
        if (MachineName != null)
        {
            system = new LSA_UNICODE_STRING[1];
            system[0] = InitLsaString(MachineName);
        }
        var ret = Win32Sec.LsaOpenPolicy(system, ref lsaAttr, (int)Access.POLICY_ALL_ACCESS, out lsaHandle);
        TestReturnValue(ret);
    }

    public bool IsWindowsAuthorised(string privilege, string userName)
    {
        var windowsAuthorised = false;

        var username = CleanUser(userName);
        var privileges = new LSA_UNICODE_STRING[1];
        privileges[0] = InitLsaString(privilege);
        var ret = Win32Sec.LsaEnumerateAccountsWithUserRight(lsaHandle, privileges, out LSA_HANDLE buffer, out ulong count);
        var Accounts = new List<String>();

        if (ret == 0)
        {
            var LsaInfo = new LSA_ENUMERATION_INFORMATION[count];
            var myLsaus = new LSA_ENUMERATION_INFORMATION();
            for (ulong i = 0; i < count; i++)
            {
                var itemAddr = new IntPtr(buffer.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(myLsaus)));
                LsaInfo[i] =
                    (LSA_ENUMERATION_INFORMATION)Marshal.PtrToStructure(itemAddr, myLsaus.GetType());
                var SID = new SecurityIdentifier(LsaInfo[i].PSid);
                Accounts.Add(ResolveAccountName(SID));
            }

            try
            {
                return IsWindowsAuthorised(username, ref windowsAuthorised, Accounts);
            }
            catch (Exception)
            {
                var localGroups = GetLocalUserGroupsForTaskSchedule(username);
                var intersection = localGroups.Intersect(Accounts);
                return intersection.Any(s => !s.Equals(Environment.MachineName+"\\"+ username, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        return false;
    }

    static bool IsWindowsAuthorised(string userName, ref bool windowsAuthorised, List<string> Accounts)
    {
        var wp = new WindowsPrincipal(new WindowsIdentity(userName));

        foreach (string account in Accounts)
        {
            if (wp.IsInRole(account))
            {
                windowsAuthorised = true;
            }
        }

        return windowsAuthorised;
    }

    IEnumerable<string> FetchSchedulerGroups()
    {
        var privileges = new LSA_UNICODE_STRING[1];
        privileges[0] = InitLsaString("SeBatchLogonRight");
        var ret = Win32Sec.LsaEnumerateAccountsWithUserRight(lsaHandle, privileges, out LSA_HANDLE buffer, out ulong count);
        var accounts = new List<String>();

        if (ret == 0)
        {
            var LsaInfo = new LSA_ENUMERATION_INFORMATION[count];
            var myLsaus = new LSA_ENUMERATION_INFORMATION();
            for (ulong i = 0; i < count; i++)
            {
                var itemAddr = new IntPtr(buffer.ToInt64() + (long)(i * (ulong)Marshal.SizeOf(myLsaus)));

                LsaInfo[i] =
                    (LSA_ENUMERATION_INFORMATION)Marshal.PtrToStructure(itemAddr, myLsaus.GetType());
                var SID = new SecurityIdentifier(LsaInfo[i].PSid);
                accounts.Add(ResolveAccountName(SID));
            }
        }

        return accounts;
    }


    IEnumerable<string> GetLocalUserGroupsForTaskSchedule(string userName)
    {
        var groups = new List<string>();
        using (var pcLocal = new PrincipalContext(ContextType.Machine))
        {
            foreach (var grp in FetchSchedulerGroups())
            {
                if (CleanUser(grp).ToLower() == userName.ToLower())
                {
                    groups.Add(grp);
                }
                else
                {
                    try
                    {
                        GetLocalUserGroupsForTaskSchedule(userName, groups, pcLocal, grp);
                    }
                    catch (Exception err)
                    {
                        Dev2Logger.Error(string.Format(ErrorResource.SchedulerErrorEnumeratingGroups, grp), err, GlobalConstants.WarewolfError);
                    }
                }
            }
        }
        return groups;
    }

    static void GetLocalUserGroupsForTaskSchedule(string userName, List<string> groups, PrincipalContext pcLocal, string grp)
    {
        var group = GroupPrincipal.FindByIdentity(pcLocal, grp);
        if (group != null)
        {
            var members = group.GetMembers();
            if (members.Any(member => member.SamAccountName.ToLower() == userName.ToLower()))
            {
                groups.Add(grp);
            }
        }
    }

    static string CleanUser(string userName)
    {
        if (userName.Contains("\\"))
        {
            userName = userName.Split('\\').Last();
        }

        return userName;
    }

    String ResolveAccountName(SecurityIdentifier SID)
    {
        try
        {
            return SID.Translate(typeof(NTAccount)).Value;
        }
        catch (Exception)
        {
            return SID.ToString();
        }
    }

    void TestReturnValue(uint ReturnValue)
    {
        if (ReturnValue == 0)
        {
            return;
        }

        if (ReturnValue == ERROR_PRIVILEGE_DOES_NOT_EXIST)
        {
            return;
        }
        if (ReturnValue == ERROR_NO_MORE_ITEMS)
        {
            return;
        }
        if (ReturnValue == STATUS_ACCESS_DENIED)
        {
            throw new UnauthorizedAccessException();
        }
        if (ReturnValue == STATUS_INSUFFICIENT_RESOURCES || ReturnValue == STATUS_NO_MEMORY)
        {
            return;
        }
        throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int)ReturnValue));
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
    
    ~SecurityWrapper()
    {
        Dispose();
    }

    static LSA_UNICODE_STRING InitLsaString(string Value)
    {
        if (Value.Length > 0x7ffe)
        {
            throw new ArgumentException(ErrorResource.StringTooLong);
        }

        var lus = new LSA_UNICODE_STRING { Buffer = Value, Length = (ushort)(Value.Length * sizeof(char)) };
        lus.MaximumLength = (ushort)(lus.Length + sizeof(char));
        return lus;
    }

    public bool IsWarewolfAuthorised(string privilege, string userName, string resourceGuid)
    {
        userName = CleanUser(userName);

        IPrincipal identity;
        try
        {
            identity = new WindowsPrincipal(new WindowsIdentity(userName));
        }
        catch (Exception e)
        {
            Dev2Logger.Warn("Failed to get windows security principal for " + userName + " as a windows identity. " + e.Message, GlobalConstants.WarewolfWarn);
            var groups = GetLocalUserGroupsForTaskSchedule(userName);
            var tmp = new GenericIdentity(userName);
            identity = new GenericPrincipal(tmp, groups.ToArray());
        }

        if (_authorizationService.IsAuthorized(identity, AuthorizationContext.Execute, resourceGuid))
        {
            return true;
        }
        Dev2Logger.Warn("User " + userName + " was denied permission to create a scheduled task.", GlobalConstants.WarewolfWarn);

        return false;
    }
}


