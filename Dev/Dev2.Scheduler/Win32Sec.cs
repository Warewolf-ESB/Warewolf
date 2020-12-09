#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Warewolf.Data;
using Warewolf.Resource.Errors;
using LSA_HANDLE = System.IntPtr;

#pragma warning disable IDE0040, IDE1006, S101
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
    [MarshalAs(UnmanagedType.LPWStr)] internal string Buffer;
}

[StructLayout(LayoutKind.Sequential)]
struct LSA_ENUMERATION_INFORMATION
{
    internal LSA_HANDLE PSid;
}
#pragma warning restore IDE0040, IDE1006, S101

static class Win32Sec
{
    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true), SuppressUnmanagedCodeSecurity]
    internal static extern uint LsaOpenPolicy(
        LSA_UNICODE_STRING[] SystemName,
        ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
        int AccessMask,
        out LSA_HANDLE PolicyHandle
    );

    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true), SuppressUnmanagedCodeSecurity]
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
    LSA_HANDLE _lsaHandle;

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
        _lsaHandle = IntPtr.Zero;
        LSA_UNICODE_STRING[] system = null;
        if (MachineName != null)
        {
            system = new LSA_UNICODE_STRING[1];
            system[0] = InitLsaString(MachineName);
        }

        var ret = Win32Sec.LsaOpenPolicy(system, ref lsaAttr, (int) Access.POLICY_ALL_ACCESS, out _lsaHandle);
        TestReturnValue(ret);
    }

    public bool IsWindowsAuthorised(string privilege, string userName)
    {
        var sanitizedUserName = userName.Trim();

        var isFullyQualifiedUser = sanitizedUserName.Contains(@"\");
        var unQualifiedUserName = GetUnqualifiedName(sanitizedUserName);
        var accounts = GetAccountsWithPrivilege(privilege);

        try
        {
            // when this throws every group is checked for a user that matches the userName stripping the host, if this happens we might as well just check Accounts for userName by stripping the host, it is the same thing.
            // Perhaps we 
            var isInRole = PrincipalIsInRole(accounts, unQualifiedUserName);
            if (isInRole)
            {
                return isInRole;
            }

            var qualifiedUser = isFullyQualifiedUser ? sanitizedUserName : Environment.MachineName + "\\" + unQualifiedUserName;
            var userIsAccount = accounts.Any(o => o.Equals(qualifiedUser, StringComparison.InvariantCultureIgnoreCase));
            if (userIsAccount)
            {
                return userIsAccount;
            }

            return IsUserAMemberOfAccount(unQualifiedUserName, accounts);
        }
        catch (Exception)
        {
            return IsUserAMemberOfAccount(unQualifiedUserName, accounts);
        }
    }

    private static bool PrincipalIsInRole(List<string> accounts, string unQualifiedUserName)
    {
        var principal = TryGetPrincipal(unQualifiedUserName);
        if (principal != null)
        {
            foreach (string account in accounts)
            {
                if (principal.IsInRole(account))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static WindowsPrincipal TryGetPrincipal(string username)
    {
        try
        {
            return new WindowsPrincipal(new WindowsIdentity(username));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private List<string> GetAccountsWithPrivilege(string privilege)
    {
        var privileges = new LSA_UNICODE_STRING[1];
        privileges[0] = InitLsaString(privilege);
        var gotAccounts = Win32Sec.LsaEnumerateAccountsWithUserRight(_lsaHandle, privileges, out LSA_HANDLE buffer, out ulong count) == 0;
        var accountNames = new List<string>();
        if (gotAccounts)
        {
            var LsaInfo = new LSA_ENUMERATION_INFORMATION[count];
            var myLsaus = new LSA_ENUMERATION_INFORMATION();
            for (ulong i = 0; i < count; i++)
            {
                var itemAddr = new IntPtr(buffer.ToInt64() + (long) (i * (ulong) Marshal.SizeOf(myLsaus)));
                LsaInfo[i] = (LSA_ENUMERATION_INFORMATION) Marshal.PtrToStructure(itemAddr, myLsaus.GetType());
                var sid = new SecurityIdentifier(LsaInfo[i].PSid);
                accountNames.Add(ResolveAccountName(sid));
            }
        }

        return accountNames;
    }

    static bool IsUserAMemberOfAccount(string userName, IList<string> AccountsToCheck) => GetGroupsUserBelongsTo(userName, AccountsToCheck).Any();

    static IList<string> GetGroupsUserBelongsTo(string userName, IList<string> AccountsToCheck)
    {
        var groups = new List<string>();
        using (var pcLocal = new PrincipalContext(ContextType.Machine))
        {
            foreach (var account in AccountsToCheck)
            {
                try
                {
                    var members = GetGroupMembers(pcLocal, account);
                    if (members.Any(member => member.SamAccountName.ToLower(CultureInfo.InvariantCulture) == userName.ToLower(CultureInfo.InvariantCulture)))
                    {
                        groups.Add(account);
                    }
                }
                catch (Exception err)
                {
                    Dev2Logger.Error(string.Format(ErrorResource.SchedulerErrorEnumeratingGroups, account), err, GlobalConstants.WarewolfError);
                }
            }
        }

        return groups;
    }

    private static Principal[] GetGroupMembers(PrincipalContext pcLocal, string account)
    {
        var group = GroupPrincipal.FindByIdentity(pcLocal, account);
        if (group != null)
        {
            return group.GetMembers().ToArray();
        }

        return new Principal[] { };
    }

    static string GetUnqualifiedName(string userName)
    {
        if (userName.Contains("\\"))
        {
            return userName.Split('\\').Last().Trim();
        }

        return userName;
    }

    static string ResolveAccountName(SecurityIdentifier SID)
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

    static void TestReturnValue(uint ReturnValue)
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

        throw new Win32Exception(Win32Sec.LsaNtStatusToWinError((int) ReturnValue));
    }

    public void Dispose()
    {
        if (_lsaHandle != IntPtr.Zero)
        {
            Win32Sec.LsaClose(_lsaHandle);
            _lsaHandle = IntPtr.Zero;
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

        var lus = new LSA_UNICODE_STRING {Buffer = Value, Length = (ushort) (Value.Length * sizeof(char))};
        lus.MaximumLength = (ushort) (lus.Length + sizeof(char));
        return lus;
    }

    public bool IsWarewolfAuthorised(string privilege, string userName, IWarewolfResource resource)
    {
        var unqualifiedUserName = GetUnqualifiedName(userName).Trim();

        IPrincipal identity;
        try
        {
            identity = new WindowsPrincipal(new WindowsIdentity(unqualifiedUserName));
        }
        catch (Exception e)
        {
            Dev2Logger.Warn("Failed to get windows security principal for " + unqualifiedUserName + " as a windows identity. " + e.Message, GlobalConstants.WarewolfWarn);
            var groups = GetGroupsUserBelongsTo(unqualifiedUserName, GetAccountsWithPrivilege(privilege));
            var tmp = new GenericIdentity(unqualifiedUserName);
            identity = new GenericPrincipal(tmp, groups.ToArray());
        }

        if (_authorizationService.IsAuthorized(identity, AuthorizationContext.Execute, resource))
        {
            return true;
        }

        Dev2Logger.Warn("User " + unqualifiedUserName + " was denied permission to create a scheduled task.", GlobalConstants.WarewolfWarn);

        return false;
    }

    public ClaimsPrincipal BuildUserClaimsPrincipal(string privilege, string unqualifiedUserName)
    {
        var groups = GetGroupsUserBelongsTo(unqualifiedUserName, GetAccountsWithPrivilege(privilege));
        var tmp = new GenericIdentity(unqualifiedUserName);
        ClaimsPrincipal identity = new GenericPrincipal(tmp, groups.ToArray());
        return identity;
    }

    public bool IsWarewolfAuthorised(string privilege, string userName, Guid resourceId)
    {
        var unqualifiedUserName = GetUnqualifiedName(userName).Trim();

        IPrincipal identity;
        try
        {
            identity = new WindowsPrincipal(new WindowsIdentity(unqualifiedUserName));
        }
        catch (Exception e)
        {
            Dev2Logger.Warn("Failed to get windows security principal for " + unqualifiedUserName + " as a windows identity. " + e.Message, GlobalConstants.WarewolfWarn);
            var groups = GetGroupsUserBelongsTo(unqualifiedUserName, GetAccountsWithPrivilege(privilege));
            var tmp = new GenericIdentity(unqualifiedUserName);
            identity = new GenericPrincipal(tmp, groups.ToArray());
        }

        if (_authorizationService.IsAuthorized(identity, AuthorizationContext.Execute, resourceId))
        {
            return true;
        }

        Dev2Logger.Warn("User " + unqualifiedUserName + " was denied permission to create a scheduled task.", GlobalConstants.WarewolfWarn);

        return false;
    }
}