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
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace Dev2.Common
{
    public static class Utilities
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            if (e is null)
            {
                return new List<T>();
            }
            var second = e as IList<T> ?? e.ToList();
            return second.SelectMany(c => f.Invoke(c).Flatten(f)).Concat(second);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        public static void PerformActionInsideImpersonatedContext(IPrincipal userPrinciple, Action actionToBePerformed)
        {
            if (userPrinciple == null)
            {
                actionToBePerformed?.Invoke();
            }
            else
            {
                var widentity = GetWindowsIdentity(userPrinciple);
                if (null != widentity)
                {
                    try
                    {
                        WindowsIdentity.RunImpersonated(widentity.AccessToken, actionToBePerformed);
                    }
                    catch (Exception ex)
                    {
                        if (ServerUser != null && ServerUser.Identity is WindowsIdentity identity)
                        {
                            WindowsIdentity.RunImpersonated(identity.AccessToken, actionToBePerformed);
                        }
                        else
                        {
                            actionToBePerformed?.Invoke();
                        }
                    }
                }
                else
                {

                    try
                    {
                        actionToBePerformed?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (ServerUser != null && ServerUser.Identity is WindowsIdentity identity)
                        {
                            WindowsIdentity.RunImpersonated(identity.AccessToken, actionToBePerformed);
                        }
                        else
                        {
                            actionToBePerformed?.Invoke();
                        }
                    }

                }

            }
        }


        private static WindowsIdentity GetWindowsIdentity(IPrincipal userPrinciple)
        {
            if (userPrinciple.Identity is WindowsIdentity identity)
            {
                if (identity.IsAnonymous && ServerUser != null)
                {
                    identity = ServerUser.Identity as WindowsIdentity;

                    // since Anonymous identity can not perform an impersonation
                    if (identity.IsAnonymous)
                        throw new System.InvalidOperationException("An anonymous identity cannot perform an impersonation.");
                }
                return identity;
            }
            return null;

        }

        public static IPrincipal OrginalExecutingUser { get; set; }

        public static IPrincipal ServerUser
        {
            get;
            set;
        }

        public static long ByteConvertor(long longValue, long convertTo)
        {
            return longValue / convertTo;
        }
    }
}