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

        public WindowsIdentity(IntPtr intPtr)
        {
            this.intPtr = intPtr;
        }

        public bool IsAnonymous { get; internal set; }
        public string Name { get; internal set; }
        public List<System.Security.Principal.IdentityReference> Groups { get; set; }

        public static WindowsIdentity GetCurrent()
        {
            throw new NotImplementedException();
        }

        public WindowsImpersonationContext Impersonate()
        {
            return null;
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
