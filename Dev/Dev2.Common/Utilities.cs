/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Common
{
    public static class Utilities
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {            
            var second = e as IList<T> ?? e.ToList();
            return second.SelectMany(c => f(c).Flatten(f)).Concat(second);
        }

        public static void PerformActionInsideImpersonatedContext(IPrincipal userPrinciple, Action actionToBePerformed)
        {
            if (userPrinciple == null)
            {
                actionToBePerformed();
            }
            else
            {
                WindowsIdentity identity = userPrinciple.Identity as WindowsIdentity;
                WindowsImpersonationContext impersonationContext = null;
                if (identity != null)
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
                try
                {
                    actionToBePerformed();
                }
                catch (Exception)
                {
                    impersonationContext?.Undo();
                    identity = ServerUser.Identity as WindowsIdentity;
                    if (identity != null)
                    {
                        impersonationContext = identity.Impersonate();
                    }
                    try
                    {
                        actionToBePerformed();
                    }
                    catch (Exception)
                    {
                        //Ignore
                    }
                }
                finally
                {
                    impersonationContext?.Undo();
                }
            }
        }

        public static IPrincipal OrginalExecutingUser { get; set; }

        public static IPrincipal ServerUser
        {
            get;
            set;
        }
    }
}