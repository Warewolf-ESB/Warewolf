#pragma warning disable
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
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Security.Principal;


namespace Dev2.Common.Common
{
    public interface ISecurityIdentityFactory
    {
        ISecurityIdentity Current { get; }
    }
    public class SecurityIdentityFactory : ISecurityIdentityFactory
    {
        private static ISecurityIdentityFactory _instance;
        private static readonly object _lock = new object();

        public static ISecurityIdentityFactory Get()
        {
            void thrower()
            {
                throw new Exception("security identity factory not set");
            }

            if (_instance is null)
            {
                thrower();
            }
            return _instance;
        }

        public static void Set(ISecurityIdentityFactory securityIdentityFactory)
        {
            lock (_lock)
            {
                if (_instance is null)
                {
                    _instance = securityIdentityFactory;
                }
            }
        }

        public ISecurityIdentity Current => Get().Current;
    }

    public class SecurityIdentityFactoryForWindows : ISecurityIdentityFactory
    {
        public ISecurityIdentity Current => new SecurityIdentityForWindows();
    }

    public interface ISecurityIdentity
    {
        List<string> GetHosts();
    }
    internal class SecurityIdentityForWindows : ISecurityIdentity
    {
        private readonly WindowsIdentity _wi;
        public SecurityIdentityForWindows()
            :this(WindowsIdentity.GetCurrent())
        {
        }
        public SecurityIdentityForWindows(WindowsIdentity windowsIdentity)
        {
            _wi = windowsIdentity;
        }

        public List<string> GetHosts()
        {
            var serverUserName = _wi.Name;

            var domainOrWorkgroupName = GetWindowsDomainOrWorkgroupName(serverUserName);
            var queryStr = $"WinNT://{domainOrWorkgroupName}";

            return GetHosts(queryStr);
        }

        private static List<string> GetHosts(string queryStr)
        {
            var root = new DirectoryEntry(queryStr);

            var kids = root.Children;

            var result = (from DirectoryEntry node in kids where node.SchemaClassName == "Computer" select node.Name).ToList();
            return result;
        }

        public static string GetWindowsDomainOrWorkgroupName(string serverUserName)
        {
            var parts = serverUserName.Split('\\');


            var userHasWindowsDomain = parts.Length == 2;
            if (userHasWindowsDomain)
            {
                return parts[0];
            }
            else
            {
                var query = new SelectQuery("Win32_ComputerSystem");
                var searcher = new ManagementObjectSearcher(query);
                var itr = searcher.Get().GetEnumerator();
                if (itr.MoveNext())
                {
                    return itr.Current["Workgroup"] as string;
                }
            }

            return "";
        }
    }

    public interface IGetComputerNames
    {
        List<string> ComputerNames { get; }
        void GetComputerNamesList();
    }

    public class GetComputerNamesImpl : IGetComputerNames
    {
        List<string> _currentComputerNames;
        readonly ISecurityIdentityFactory _securityIdentityFactory;
        public GetComputerNamesImpl(ISecurityIdentityFactory securityIdentityFactory)
        {
            _securityIdentityFactory = securityIdentityFactory;
        }

        public List<string> ComputerNames
        {
            get
            {
                if (_currentComputerNames is null)
                {
                    GetComputerNamesList();
                }

                return _currentComputerNames;
            }
        }

        public void GetComputerNamesList() => _currentComputerNames = StandardComputerNameQuery();

        List<string> StandardComputerNameQuery()
        {
            var currentSecurityIdentity = _securityIdentityFactory.Current;

            if (currentSecurityIdentity != null)
            {
                var result = currentSecurityIdentity.GetHosts();

                if (result.Any())
                {
                    return result;
                }
            }

            return new List<string> { Environment.MachineName };
        }
    }

    public static class GetComputerNames
    {
        private static IGetComputerNames _instance;
        private static object _lock = new object();
        private static IGetComputerNames Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = new GetComputerNamesImpl(SecurityIdentityFactory.Get());
                        }
                    }
                }
                return _instance;
            }
        }
        public static List<string> ComputerNames
        {
            get => Instance.ComputerNames;
        }

        public static void GetComputerNamesList() => Instance.GetComputerNamesList();
    }
}