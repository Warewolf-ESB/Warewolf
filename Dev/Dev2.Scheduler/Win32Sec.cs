
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.Services.Security;
using Dev2.Services.Security;
using LSA_HANDLE = System.IntPtr;


[StructLayout(LayoutKind.Sequential)]
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
internal struct LSA_OBJECT_ATTRIBUTES
{
    internal int Length;
    internal LSA_HANDLE RootDirectory;
    internal LSA_HANDLE ObjectName;
    internal int Attributes;
    internal LSA_HANDLE SecurityDescriptor;
    internal LSA_HANDLE SecurityQualityOfService;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct LSA_UNICODE_STRING
{
    internal ushort Length;
    internal ushort MaximumLength;
    [MarshalAs(UnmanagedType.LPWStr)]
    internal string Buffer;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LSA_ENUMERATION_INFORMATION
{
    internal LSA_HANDLE PSid;
}


/// <summary>
///     Provides direct Win32 calls to the security related functions
/// </summary>
internal sealed class Win32Sec
{
    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true), SuppressUnmanagedCodeSecurity]
    internal static extern uint LsaOpenPolicy(
        LSA_UNICODE_STRING[] SystemName,
        ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
        int AccessMask,
        out LSA_HANDLE PolicyHandle
        );

    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true), SuppressUnmanagedCodeSecurity]
    internal static extern uint LsaEnumerateAccountsWithUserRight(
        LSA_HANDLE PolicyHandle,
        LSA_UNICODE_STRING[] UserRights,
        out LSA_HANDLE EnumerationBuffer,
        out int CountReturned
        );

    [DllImport("advapi32")]
    internal static extern int LsaNtStatusToWinError(int NTSTATUS);

    [DllImport("advapi32")]
    internal static extern int LsaClose(LSA_HANDLE PolicyHandle);

    [DllImport("advapi32")]
    internal static extern int LsaFreeMemory(LSA_HANDLE Buffer);
}


/// <summary>
///     Provides a wrapper to the LSA classes
/// </summary>
public class SecurityWrapper : ISecurityWrapper
{
    private readonly IAuthorizationService _authorizationService;

    private enum Access
    {
        POLICY_READ = 0x20006,
        POLICY_ALL_ACCESS = 0x00F0FFF,
        POLICY_EXECUTE = 0X20801,

        POLICY_WRITE = 0X207F8
    }

    private const uint STATUS_ACCESS_DENIED = 0xc0000022;
    private const uint STATUS_INSUFFICIENT_RESOURCES = 0xc000009a;
    private const uint STATUS_NO_MEMORY = 0xc0000017;
    private const uint STATUS_NO_MORE_ENTRIES = 0xc000001A;
    private const uint ERROR_NO_MORE_ITEMS = 2147483674;
    private const uint ERROR_PRIVILEGE_DOES_NOT_EXIST = 3221225568;
    private LSA_HANDLE lsaHandle;


    /// <summary>
    ///     Creates a new LSA wrapper for the local machine
    /// </summary>
    [ExcludeFromCodeCoverage]
    public SecurityWrapper(IAuthorizationService authorizationService)
        : this(Environment.MachineName)
    {
        _authorizationService = authorizationService;
    }


    /// <summary>
    ///     Creates a new LSA wrapper for the specified MachineName
    /// </summary>
    /// <param name="MachineName">The name of the machine that should be connected to</param>
    [ExcludeFromCodeCoverage]
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
        uint ret = Win32Sec.LsaOpenPolicy(system, ref lsaAttr, (int)Access.POLICY_ALL_ACCESS, out lsaHandle);
        TestReturnValue(ret);
    }


    /// <summary>
    ///     Reads the user accounts which have the specific privilege
    /// </summary>
    /// <param name="privilege">The name of the privilege for which the accounts with this right should be enumerated</param>
    /// <param name="userName"></param>
    [ExcludeFromCodeCoverage]
    public bool IsWindowsAuthorised(string privilege, string userName)
    {
        bool windowsAuthorised = false;

        userName = CleanUser(userName);
        var privileges = new LSA_UNICODE_STRING[1];
        privileges[0] = InitLsaString(privilege);
        IntPtr buffer;
        int count;
        uint ret = Win32Sec.LsaEnumerateAccountsWithUserRight(lsaHandle, privileges, out buffer, out count);
        var Accounts = new List<String>();

        if (ret == 0)
        {
            var LsaInfo = new LSA_ENUMERATION_INFORMATION[count];
            for (int i = 0, elemOffs = (int)buffer; i < count; i++)
            {
                LsaInfo[i] =
                    (LSA_ENUMERATION_INFORMATION)
                    Marshal.PtrToStructure((IntPtr)elemOffs, typeof(LSA_ENUMERATION_INFORMATION));
                elemOffs += Marshal.SizeOf(typeof(LSA_ENUMERATION_INFORMATION));
                var SID = new SecurityIdentifier(LsaInfo[i].PSid);
                Accounts.Add(ResolveAccountName(SID));
            }

            try
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
            catch (Exception)
            {
                var localGroups = GetLocalUserGroupsForTaskSchedule(userName);

                var intersection = localGroups.Intersect(Accounts);

                return intersection.Any();

            }
        }

        return false;
    }


    private IEnumerable<string> FetchSchedulerGroups()
    {
        var privileges = new LSA_UNICODE_STRING[1];
        privileges[0] = InitLsaString("SeBatchLogonRight");
        IntPtr buffer;
        int count;
        uint ret = Win32Sec.LsaEnumerateAccountsWithUserRight(lsaHandle, privileges, out buffer, out count);
        var accounts = new List<String>();

        if (ret == 0)
        {
            var LsaInfo = new LSA_ENUMERATION_INFORMATION[count];
            for (int i = 0, elemOffs = (int)buffer; i < count; i++)
            {
                LsaInfo[i] =
                    (LSA_ENUMERATION_INFORMATION)
                    Marshal.PtrToStructure((IntPtr)elemOffs, typeof(LSA_ENUMERATION_INFORMATION));
                elemOffs += Marshal.SizeOf(typeof(LSA_ENUMERATION_INFORMATION));
                var SID = new SecurityIdentifier(LsaInfo[i].PSid);
                accounts.Add(ResolveAccountName(SID));
            }
        }

        return accounts;
    }


    private IEnumerable<string> GetLocalUserGroupsForTaskSchedule(string userName)
    {
        var groups = new List<string>();
        // Domain failed. Try local pc.
        using (var pcLocal = new PrincipalContext(ContextType.Machine))
        {
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var grp in FetchSchedulerGroups())
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if (CleanUser(grp).ToLower() == userName.ToLower())
                {
                    groups.Add(grp);
                }
                else
                {
                    try
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
                    catch (Exception err)
                    {
                        Dev2Logger.Log.Error(String.Format("Scheduler Error Enumerating Groups:{0}", grp), err);
                    }
                }

            }

        }
        return groups;
    }

    private static string CleanUser(string userName)
    {
        if (userName.Contains("\\"))
            userName = userName.Split(new[] { '\\' }).Last();
        return userName;
    }


    /// <summary>
    ///     Resolves the SID into it's account name. If the SID cannot be resolved the SDDL for the SID (for example "S-1-5-21-3708151440-578689555-182056876-1009") is returned.
    /// </summary>
    /// <param name="SID">The Security Identifier to resolve to an account name</param>
    /// <returns>An account name for example "NT AUTHORITY\LOCAL SERVICE" or SID in SDDL form</returns>
    [ExcludeFromCodeCoverage]
    private String ResolveAccountName(SecurityIdentifier SID)
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


    /// <summary>
    ///     Tests the return value from Win32 method calls
    /// </summary>
    /// <param name="ReturnValue">The return value from the a Win32 method call</param>
    [ExcludeFromCodeCoverage]
    private void TestReturnValue(uint ReturnValue)
    {
        if (ReturnValue == 0) return;
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
        if ((ReturnValue == STATUS_INSUFFICIENT_RESOURCES) || (ReturnValue == STATUS_NO_MEMORY))
        {
            throw new OutOfMemoryException();
        }
        throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int)ReturnValue));
    }


    /// <summary>
    ///     Disposes of this LSA wrapper
    /// </summary>
    public void Dispose()
    {
        if (lsaHandle != IntPtr.Zero)
        {
            Win32Sec.LsaClose(lsaHandle);
            lsaHandle = IntPtr.Zero;
        }
        GC.SuppressFinalize(this);
    }


    /// <summary>
    ///     Occurs on destruction of the LSA Wrapper
    /// </summary>
    ~SecurityWrapper()
    {
        Dispose();
    }


    /// <summary>
    ///     Converts the specified string to an LSA string value
    /// </summary>
    /// <param name="Value"></param>
    private static LSA_UNICODE_STRING InitLsaString(string Value)
    {
        if (Value.Length > 0x7ffe) throw new ArgumentException("String too long");
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
        catch (Exception)
        {
            var groups = GetLocalUserGroupsForTaskSchedule(userName);
            var tmp = new GenericIdentity(userName);
            identity = new GenericPrincipal(tmp, groups.ToArray());
        }

        if (_authorizationService.IsAuthorized(identity, AuthorizationContext.Execute, resourceGuid))
        {
            return true;
        }

        return false;
    }
}


