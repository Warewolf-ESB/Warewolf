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
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace Dev2.Common
{
    public static class Utilities
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            if(e is null)
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
            if (userPrinciple == null || IsAlreadyImpersonated(userPrinciple))
            {
                actionToBePerformed?.Invoke();
            }
            else
            {
                var impersonationContext = Impersonate(userPrinciple);
                try
                {
                    actionToBePerformed?.Invoke();
                }
                catch (Exception e)
                {
                    impersonationContext?.Undo();
                    if (ServerUser.Identity is WindowsIdentity identity)
                    {
                        impersonationContext = identity.Impersonate();
                    }
                    actionToBePerformed?.Invoke();
                }
                finally
                {
                    impersonationContext?.Undo();
                }
            }
        }

        private static WindowsImpersonationContext Impersonate(IPrincipal userPrinciple)
        {
            WindowsImpersonationContext impersonationContext = null;
            if (userPrinciple.Identity is WindowsIdentity identity)
            {
                if (identity.IsAnonymous)
                {
                    identity = ServerUser.Identity as WindowsIdentity;
                }
                if (identity != null)
                {
                    impersonationContext = identity.Impersonate();
                }
            }

            return impersonationContext;
        }

        private static bool IsAlreadyImpersonated(IPrincipal userPrinciple)
        {
            if (userPrinciple.Identity is WindowsIdentity newIdentity && Thread.CurrentPrincipal.Identity is WindowsIdentity currentIdentity)
            {
                return newIdentity.User.Equals(currentIdentity.User);
            }
            return false;
        }

        public static IPrincipal OrginalExecutingUser { get; set; }

        public static IPrincipal ServerUser
        {
            get;
            set;
        }
    }
}