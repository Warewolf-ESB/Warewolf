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
using System.Security.Principal;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
#if !NETFRAMEWORK
using Dev2.Common;
using Dev2.Data.Security;
#endif

namespace Dev2.Data.PathOperations
{
    public interface IWindowsImpersonationContext : IDisposable
    {
#if !NETFRAMEWORK
        WindowsIdentity Identity { get; set; }
#endif
        void Undo();

    }

    class WindowsImpersonationContextImpl : IWindowsImpersonationContext
    {
#if !NETFRAMEWORK
        private WindowsIdentity _context;

        public WindowsImpersonationContextImpl(WindowsIdentity context)
#else
        private readonly WindowsImpersonationContext _context;
		
        public WindowsImpersonationContextImpl(WindowsImpersonationContext context)
#endif
        {
            _context = context;
        }

#if !NETFRAMEWORK
        public WindowsIdentity Identity { get => _context; set => _context = value; }
#endif

        public void Dispose()
        {
#if !NETFRAMEWORK
            //// _context.Dispose(); Should not dispose identity passed.
#else
            _context.Dispose();
#endif
        }
        public void Undo()
        {
#if !NETFRAMEWORK
            //// _context.Dispose(); Should not dispose identity passed.
#else
            _context.Undo();
#endif
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
