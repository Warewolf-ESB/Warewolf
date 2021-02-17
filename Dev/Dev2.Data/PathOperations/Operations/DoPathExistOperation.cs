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
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoPathExistOperation : PerformBoolIOOperation
    {
        readonly IWindowsImpersonationContext _impersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        protected readonly IFile _fileWrapper;
        protected readonly IDirectory _dirWrapper;

        public DoPathExistOperation(IActivityIOPath path)
            : this(path, new LogonProvider(), new FileWrapper(), new DirectoryWrapper(),
                ValidateAuthorization.RequiresAuth)
        {
        }

        public DoPathExistOperation(IActivityIOPath path, IDev2LogonProvider dev2LogonProvider, IFile fileWrapper,
            IDirectory directory, ImpersonationDelegate impersonationDelegate)
            : base(impersonationDelegate)
        {
            _logOnProvider = dev2LogonProvider;
            _fileWrapper = fileWrapper;
            _dirWrapper = directory;
            _path = path;
            _impersonatedUser = _impersonationDelegate(_path, _logOnProvider);
        }

        public override bool ExecuteOperation()
        {
            if (_impersonatedUser != null)
            {
                return ExecuteOperationWithAuth();
            }

            return PathIs(_path, _fileWrapper, _dirWrapper) == enPathType.Directory
                ? _dirWrapper.Exists(_path.Path)
                : _fileWrapper.Exists(_path.Path);
        }

        public override bool ExecuteOperationWithAuth()
        {
            using (_impersonatedUser)
            {
                try
                {
                    return PathIs(_path, _fileWrapper, _dirWrapper) == enPathType.Directory
                        ? _dirWrapper.Exists(_path.Path)
                        : _fileWrapper.Exists(_path.Path);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex.Message, GlobalConstants.Warewolf);
                    return false;
                }
            }
        }
    }
}