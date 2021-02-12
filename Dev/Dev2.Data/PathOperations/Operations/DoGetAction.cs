/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using System;
using System.IO;
using Warewolf.Resource.Errors;
using Dev2.Common.Wrappers;
using Dev2.Common;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoGetAction : PerformStreamIOOperation
    {
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        protected readonly IFile _fileWrapper;
        readonly IWindowsImpersonationContext _impersonatedUser;

        public DoGetAction(IActivityIOPath path)
            :this(path, new LogonProvider(), new FileWrapper(), ValidateAuthorization.RequiresAuth)
        {
        }
        public DoGetAction(IActivityIOPath path, IDev2LogonProvider dev2LogonProvider, IFile fileWrapper, ImpersonationDelegate impersonationDelegate)
        :base(impersonationDelegate)
        {
            _logOnProvider = dev2LogonProvider;
            _fileWrapper = fileWrapper;
            _path = path;
            _impersonatedUser = _impersonationDelegate(_path, _logOnProvider);
        }
        public override Stream ExecuteOperation()
        {
            if (_impersonatedUser != null)
            {
                return ExecuteOperationWithAuth();
            }
            if (_fileWrapper.Exists(_path.Path))
            {
                return new MemoryStream(_fileWrapper.ReadAllBytes(_path.Path));
            }
            throw new Exception(string.Format(ErrorResource.FileNotFound, _path.Path));
        }
        public override Stream ExecuteOperationWithAuth()
        {
            using (_impersonatedUser)
            {
                try
                {
                    return new MemoryStream(_fileWrapper.ReadAllBytes(_path.Path));
                }
                catch (Exception exception)
                {
                    Dev2Logger.Error(exception.Message, GlobalConstants.WarewolfError);
                    throw new Exception(exception.Message, exception);
                }
            }
        }
    }
}