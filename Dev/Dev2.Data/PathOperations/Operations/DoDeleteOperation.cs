/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Interfaces;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoDeleteOperation : PerformBoolIOOperation
    {
        readonly IWindowsImpersonationContext _impersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;

        public DoDeleteOperation(IActivityIOPath path)
            : this(path, new LogonProvider(), ValidateAuthorization.RequiresAuth)
        {
        }
        public DoDeleteOperation(IActivityIOPath path, IDev2LogonProvider logOnProvider)
            :this(path, logOnProvider, null)
        {
        }
        public DoDeleteOperation(IActivityIOPath path, IDev2LogonProvider logOnProvider, ImpersonationDelegate impersonationDelegate)
            :base(impersonationDelegate)
        {
            _path = path;
            _logOnProvider = logOnProvider;
            _impersonatedUser = _impersonationDelegate?.Invoke(_path, _logOnProvider);
        }
        public override bool ExecuteOperation()
        {
            try
            {
                if (_impersonatedUser != null)
                {
                    return ExecuteOperationWithAuth();
                }
                return DeleteHelper.Delete(_path.Path);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error("Error getting file: " + _path.Path, exception, GlobalConstants.WarewolfError);
                return false;
            }
        }
        public override bool ExecuteOperationWithAuth()
        {
            using (_impersonatedUser)
            {
                try
                {
                    return DeleteHelper.Delete(_path.Path);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex.Message, GlobalConstants.Warewolf);
                    return false;
                }
                finally
                {
                    _impersonatedUser?.Undo();
                }
            }
        }
    }
}
