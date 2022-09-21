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

    public partial class WindowsIdentity
    {
        private IntPtr intPtr;
        private WindowsIdentity identity;

        public WindowsIdentity(IntPtr intPtr)
        {
            this.intPtr = intPtr;
        }

        public WindowsIdentity(WindowsIdentity identity)
        {
            this.identity = identity;
        }
        public WindowsIdentity()
        {

        }

        public bool IsAnonymous { get; internal set; }
        public string Name { get; internal set; }
        public List<System.Security.Principal.IdentityReference> Groups { get; set; }
        public object Identity { get; internal set; }

        public static WindowsIdentity GetCurrent()
        {
            return new WindowsIdentity() { Name = "Test" };
        }

        public virtual WindowsImpersonationContext Impersonate()
        {
            return null;
        }

        internal static IIdentity GetAnonymous()
        {
            throw new NotImplementedException();
        }
    }
    public class WindowsImpersonationContext : IDisposable
    {
        public void Dispose()
        {
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }
    }

}
