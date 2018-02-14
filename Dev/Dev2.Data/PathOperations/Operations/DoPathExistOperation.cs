using System;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoPathExistOperation : PerformBoolIOOperation
    {
        readonly WindowsImpersonationContext ImpersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        protected readonly IFile _fileWrapper;
        protected readonly IDirectory _dirWrapper;
        public DoPathExistOperation(IActivityIOPath path)
        {
            _logOnProvider = new LogonProvider();
            _fileWrapper = new FileWrapper();
            _dirWrapper = new DirectoryWrapper();
            _path = path;
            ImpersonatedUser = RequiresAuth(_path, _logOnProvider);
        }
        public override bool ExecuteOperation()
        {
            if (ImpersonatedUser != null)
            {
                return ExecuteOperationWithAuth();
            }
            return PathIs(_path, _fileWrapper, _dirWrapper) == enPathType.Directory ? _dirWrapper.Exists(_path.Path) : _fileWrapper.Exists(_path.Path);
        }

        public override bool ExecuteOperationWithAuth()
        {
            try
            {
                using (ImpersonatedUser)
                {
                    return PathIs(_path, _fileWrapper, _dirWrapper) == enPathType.Directory ? _dirWrapper.Exists(_path.Path) : _fileWrapper.Exists(_path.Path);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, GlobalConstants.Warewolf);
                return false;
            }
            finally
            {
                ImpersonatedUser?.Undo();
            }
        }
    }
}

