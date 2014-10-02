
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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Emission
{
    internal static class EmissionUtility
    {
        private static Dictionary<Assembly, bool> _cache = new Dictionary<Assembly, bool>();
        private static ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        
        public static bool IsInternal(this MethodBase method)
        {
            return method.IsAssembly || (method.IsFamilyAndAssembly
                                         && !method.IsFamilyOrAssembly);
        }

        public static bool IsInternalToDynamicProxy(this Assembly asm, string dynamicProxyName)
        {
            _cacheLock.EnterUpgradeableReadLock();

            try
            {
                if (_cache.ContainsKey(asm)) return _cache[asm];

                _cacheLock.EnterWriteLock();

                try
                {
                    if (_cache.ContainsKey(asm)) return _cache[asm];
                    InternalsVisibleToAttribute[] internalsVisibleTo = asm.GetAttributes<InternalsVisibleToAttribute>();
                    bool found = false;

                    for (int i = 0; i < internalsVisibleTo.Length; i++)
                        if (VisibleToDynamicProxy(internalsVisibleTo[i], dynamicProxyName))
                        {
                            found = true;
                            break;
                        }

                    _cache.Add(asm, found);
                    return found;
                }
                finally
                {
                    _cacheLock.ExitWriteLock();
                }
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }

        private static bool VisibleToDynamicProxy(InternalsVisibleToAttribute attribute, string dynamicProxyName)
        {
            return attribute.AssemblyName.Contains(dynamicProxyName);
        }

        public static bool IsAccessible(this MethodBase method, string dynamicAssemblyName)
        {
            if (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly) return true;
            if (method.IsFamilyAndAssembly) return true;
            if (method.DeclaringType.Assembly.IsInternalToDynamicProxy(dynamicAssemblyName) && method.IsAssembly) return true;

            return false;
        }
    }
}
