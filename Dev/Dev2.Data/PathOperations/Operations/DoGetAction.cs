using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using System;
using System.IO;
using Warewolf.Resource.Errors;
using Dev2.Common.Wrappers;
using Dev2.PathOperations;
using System.Security.Principal;
using Dev2.Common;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoGetAction : PerformStreamIOOperation
    {
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        protected readonly IFile _fileWrapper;
        readonly WindowsImpersonationContext ImpersonatedUser;

        public DoGetAction(IActivityIOPath path)
        {
            _logOnProvider = new LogonProvider();
            _fileWrapper = new FileWrapper();
            _path = path;
            ImpersonatedUser = ValidateAuthorization.RequiresAuth(_path, _logOnProvider);

        }
        public override Stream ExecuteOperation()
        {
            if (ImpersonatedUser != null)
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
            using (ImpersonatedUser)
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
                finally
                {
                    ImpersonatedUser.Undo();
                }
            }
        }
    }
}