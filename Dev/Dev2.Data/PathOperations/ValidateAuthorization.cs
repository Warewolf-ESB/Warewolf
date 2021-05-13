/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Claims;
using System.Security.Principal;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;

namespace Dev2.Data.PathOperations
{
    public interface IWindowsImpersonationContext : IDisposable
    {
        void Undo();
    }

    class WindowsImpersonationContextImpl : IWindowsImpersonationContext
    {
        private readonly WindowsImpersonationContext _context;

        public WindowsImpersonationContextImpl(WindowsImpersonationContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Undo()
        {
            _context.Undo();
        }
    }

    public static class ValidateAuthorization
    {      
        public static SafeTokenHandle DoLogOn(IDev2LogonProvider dev2Logon, IActivityIOPath path) => dev2Logon.DoLogon(path);
        public static IWindowsImpersonationContext RequiresAuth(IActivityIOPath path, IDev2LogonProvider dev2LogonProvider)
        {
            var safeToken = string.IsNullOrEmpty(path.Username) ? null : DoLogOn(dev2LogonProvider, path);
            if (safeToken != null)
            {
                using (safeToken)
                {
                    var newID = new WindowsIdentity(safeToken.DangerousGetHandle());
                    return new WindowsImpersonationContextImpl(newID.Impersonate());
                }
            }
            return null;
        }
    }
}
