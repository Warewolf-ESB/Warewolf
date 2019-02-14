using System;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoCreateDirectory : PerformBoolIOOperation
    {
        readonly IWindowsImpersonationContext ImpersonatedUser;
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
            ImpersonatedUser = _impersonationDelegate(_path, _logOnProvider);
            _handleOverwrite = RequiresOverwrite(_crudArguments, _path, _logOnProvider);

        }

        public override bool ExecuteOperation()
        {
            if (ImpersonatedUser != null)
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
            using (ImpersonatedUser)
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
                    ImpersonatedUser.Undo();
                }
            }
        }
    }
}