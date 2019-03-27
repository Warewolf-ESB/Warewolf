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
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoCreateDirectory : PerformBoolIOOperation
    {
        readonly IWindowsImpersonationContext _impersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        protected readonly IFile _fileWrapper;
        protected readonly IDirectory _dirWrapper;
        protected readonly IDev2CRUDOperationTO _crudArguments;
        protected readonly SafeTokenHandle _safeToken;
        protected readonly DoDeleteOperation _handleOverwrite;

        public DoCreateDirectory(IActivityIOPath path, IDev2CRUDOperationTO args)
            :this(path, args, new LogonProvider(), new FileWrapper(), new DirectoryWrapper(), ValidateAuthorization.RequiresAuth)
        { }
        public DoCreateDirectory(IActivityIOPath path, IDev2CRUDOperationTO args, IDev2LogonProvider dev2LogonProvider, IFile fileWrapper, IDirectory directory, ImpersonationDelegate impersonationDelegate)
            :base(impersonationDelegate)
        {
            _logOnProvider = dev2LogonProvider;
            _fileWrapper = fileWrapper;
            _dirWrapper = directory;
            _path = path;
            _crudArguments = args;
            _impersonatedUser = _impersonationDelegate(_path, _logOnProvider);
            _handleOverwrite = RequiresOverwrite(_crudArguments, _path, _logOnProvider);

        }

        public override bool ExecuteOperation()
        {
            if (_impersonatedUser != null)
            {
                return ExecuteOperationWithAuth();
            }
            if (DirectoryExist(_path, _dirWrapper))
            {
                _handleOverwrite.ExecuteOperation();
            }
            _dirWrapper.CreateDirectory(_path.Path);
            return true;
        }

        public override bool ExecuteOperationWithAuth()
        {
            using (_impersonatedUser)
            {
                try
                {
                    if (_handleOverwrite == null)
                    {
                        _dirWrapper.CreateDirectory(_path.Path);
                        return true;
                    }
                    if (DirectoryExist(_path, _dirWrapper))
                    {
                        _handleOverwrite.ExecuteOperation();
                    }
                    _dirWrapper.CreateDirectory(_path.Path);
                    return true;

                }
                catch (Exception exception)
                {
                    Dev2Logger.Error(exception, GlobalConstants.WarewolfError);
                    throw;
                }
                finally
                {
                    _impersonatedUser.Undo();
                }
            }
        }
    }
}